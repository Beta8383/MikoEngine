#define TimeTest
namespace MikoEngine;

using System.Runtime.InteropServices;
using static MikoEngine.MKMath;

public unsafe partial class MKEngine
{
    const int BytesPerPixel = 3;

    private readonly int width, height;

    private float[] pixelsBuffer;
    private float[] zBuffer;
    private int[] coveredIndexBuffer;
    private MKVector3[] barycentricBuffer;

    private MKVector4 background;

    List<Model> models = new();
    List<Light> lights = new();
    Camera camera;

    MKMatrix4x4 viewportTransform, cameraTransform, projectionTransform;

    public MKEngine(int height, int width)
    {
        this.width = width;
        this.height = height;

        //(0,0)在视口中心，y轴上小下大，需要倒转
        viewportTransform = new(
            width / 2.0f, 0, 0, width / 2.0f,
            0, -height / 2.0f, 0, height / 2.0f,
            0, 0, 1, 0,
            0, 0, 0, 1
        );

        zBuffer = new float[height * width];
        coveredIndexBuffer = new int[height * width];
        barycentricBuffer = new MKVector3[height * width];
        pixelsBuffer = new float[width * height * BytesPerPixel];
    }

    private void SetPixel(MKVector4 color, int bufferIndex)
    {
        int index = bufferIndex * BytesPerPixel;
        pixelsBuffer[index] = Math.Clamp(color.X, 0f, 1f);
        pixelsBuffer[index + 1] = Math.Clamp(color.Y, 0f, 1f);
        pixelsBuffer[index + 2] = Math.Clamp(color.Z, 0f, 1f);
    }

    private void Rasterize(AllocSpan<MKVector4> vectors, int count)
    {
        Parallel.For(0, count / 3, index =>
        {
            ref readonly var vec1 = ref vectors[index * 3];
            ref readonly var vec2 = ref vectors[index * 3+ 1];
            ref readonly var vec3 = ref vectors[index * 3+ 2];

            int minx = (int)Min(vec1.X, vec2.X, vec3.X);
            int miny = (int)Min(vec1.Y, vec2.Y, vec3.Y);
            int maxx = (int)Max(vec1.X, vec2.X, vec3.X) + 1;
            int maxy = (int)Max(vec1.Y, vec2.Y, vec3.Y) + 1;

            minx = Max(minx, 0);
            miny = Max(miny, 0);
            maxx = Min(maxx, width - 1);
            maxy = Min(maxy, height - 1);

            float depth;

            for (int x = minx; x <= maxx; x++)
                for (int y = miny; y <= maxy; y++)
                {
                    if (ComputeBarycentric(in vec1, in vec2, in vec3, (float)(x + 0.5), (float)(y + 0.5), out float i, out float j, out float k))
                    {
                        i /= vec1.W;
                        j /= vec2.W;
                        k /= vec3.W;
                        depth = 1f / (i + j + k);
                        i *= depth;
                        j *= depth;
                        k *= depth;
                        if (depth < 0 && depth > zBuffer[y * width + x])
                        {
                            zBuffer[y * width + x] = depth;

                            coveredIndexBuffer[y * width + x] = index;
                            barycentricBuffer[y * width + x].X = i;
                            barycentricBuffer[y * width + x].Y = j;
                            barycentricBuffer[y * width + x].Z = k;
                            //灰度值
                            //SetPixel(new MKVector4(255) * (depth + 1) * 0.5f, x, y);
                        }
                    }
                }
        });
    }

    private void Shading(AllocSpan<float> v2fs, IShader shader)
    {
        int pixels = width * height;

        Parallel.For(0, pixels, bufferIndex =>
        {
            float* v2f = stackalloc float[shader.v2fLength];
            int vectorIndex = coveredIndexBuffer[bufferIndex];
            if (vectorIndex != -1)
            {
                int v2fsIndex1 = vectorIndex * 3 * shader.v2fLength;
                int v2fsIndex2 = v2fsIndex1 + shader.v2fLength;
                int v2fsIndex3 = v2fsIndex2 + shader.v2fLength;

                for (int index = 0; index < shader.v2fLength; index++)
                    v2f[index] = v2fs[v2fsIndex1 + index] * barycentricBuffer[bufferIndex].X +
                                 v2fs[v2fsIndex2 + index] * barycentricBuffer[bufferIndex].Y +
                                 v2fs[v2fsIndex3 + index] * barycentricBuffer[bufferIndex].Z;
                SetPixel(shader.Frag(new(v2f, shader.v2fLength)), bufferIndex);
            }
        });
    }

    private void RenderModel(Model model)
    {
        if (model.Data is null)
            return;

        IShader shader = model.Shader;
        shader.modelTransform = model.Transform;
        shader.projectionTransform = projectionTransform;
        shader.cameraTransform = cameraTransform;
        shader.light0 = lights[0];
        //shader.light1 = lights[1];
        shader.camera = camera;

        int a2vLength = shader.a2vLength;
        int verticescount = model.Data.Length / a2vLength;

        int v2fLength = shader.v2fLength;
        AllocSpan<float> v2fs = new(shader.v2fLength * verticescount);

        AllocSpan<MKVector4> vectors = new(verticescount);

        TimeTest.Run(() =>
        {
            for (int index = 0; index < verticescount; index++)
            //Parallel.For(0, verticescount, index =>
            {
                Span<float> a2v = model.Data.Slice(index * a2vLength, a2vLength);
                Span<float> v2f = v2fs.Slice(index * v2fLength, v2fLength);
                ref MKVector4 vector = ref vectors[index];
                vector = shader.Vert(a2v, v2f);
                vector = viewportTransform * vector;
                vector.X /= vector.W;
                vector.Y /= vector.W;
                vector.Z /= vector.W;
            }
        }, "VertexShader");

        Array.Fill<int>(coveredIndexBuffer, -1);
        TimeTest.Run(() =>
            Rasterize(vectors, verticescount)
        , "Rasterize");

        TimeTest.Run(() =>
        {
            Shading(v2fs, shader);
        }, "Shading");

        v2fs.Free();
        vectors.Free();
    }

    public ReadOnlySpan<float> GetFrame()
    {
        Reset();

        foreach (Model model in models)
            RenderModel(model);

        return new ReadOnlySpan<float>(pixelsBuffer);
    }

    public void Clear(MKVector4 color) =>
        background = color;

    private void Reset()
    {
        for (int bufferIndex = 0; bufferIndex < width * height; bufferIndex++)
            SetPixel(background, bufferIndex);

        Array.Fill<float>(zBuffer, float.NegativeInfinity);
    }

    public MKEngine SetCamera(Camera camera)
    {
        this.camera = camera;
        if (camera.zNearPlane <= 0 || camera.zFarPlane <= 0 || camera.zNearPlane >= camera.zFarPlane)
            throw new ArgumentException();

        cameraTransform = MKMatrix4x4.CreateCameraTransform(camera.Position, camera.Direction, camera.Up);

        if (camera.Mode == ProjectionMode.Orthographic)
            projectionTransform = MKMatrix4x4.CreateOrthographicTransform(camera.Width, camera.Height, camera.zNearPlane, camera.zFarPlane);
        else projectionTransform = MKMatrix4x4.CreatePerspectiveTransform(camera.Width, camera.Height, camera.zNearPlane, camera.zFarPlane);

        return this;
    }

    public MKEngine AddLight(Light light)
    {
        lights.Add(light);
        return this;
    }

    public MKEngine AddModel(Model model)
    {
        models.Add(model);
        return this;
    }
}