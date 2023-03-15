#define TimeTest
namespace MikoEngine;

using static MikoEngine.MKMath;

public partial class MKEngine : IDisposable
{
    readonly int width, height;
    readonly int pixels;

    private AllocSpan<MKVector3> pixelsBuffer;
    private AllocSpan<float> zBuffer;
    private AllocSpan<int> coveredIndexBuffer;
    private AllocSpan<MKVector3> barycentricBuffer;
    private MKVector4 background;
    internal List<Texture> textures;

    List<Model> models = new();
    List<Light> lights = new();
    Camera camera;

    MKMatrix4x4 viewportTransform, cameraTransform, projectionTransform;

    public MKEngine(int height, int width)
    {
        this.width = width;
        this.height = height;
        pixels = width * height;

        //(0,0)在视口中心，y轴上小下大，需要倒转
        viewportTransform = new(
            width / 2.0f, 0, 0, width / 2.0f,
            0, -height / 2.0f, 0, height / 2.0f,
            0, 0, 1, 0,
            0, 0, 0, 1
        );

        zBuffer = new(pixels);
        coveredIndexBuffer = new(pixels);
        barycentricBuffer = new(pixels);
        pixelsBuffer = new(pixels);
    }

    ~MKEngine() => Free();

    public void Dispose()
    {
        Free();
        GC.SuppressFinalize(this);
    }

    void Free()
    {
        pixelsBuffer.Dispose();
        zBuffer.Dispose();
        coveredIndexBuffer.Dispose();
        barycentricBuffer.Dispose();
        foreach (var model in models)
            model.Dispose();
    }

    private void SetPixel(MKVector4 color, int index)
    {
        pixelsBuffer[index].X = Math.Clamp(color.X, 0f, 1f);
        pixelsBuffer[index].Y = Math.Clamp(color.Y, 0f, 1f);
        pixelsBuffer[index].Z = Math.Clamp(color.Z, 0f, 1f);
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
                        int bufferIndex = y * width + x;
                        if (depth < 0 && depth > zBuffer[bufferIndex])
                        {
                            zBuffer[bufferIndex] = depth;
                            coveredIndexBuffer[bufferIndex] = index;
                            barycentricBuffer[bufferIndex].X = i;
                            barycentricBuffer[bufferIndex].Y = j;
                            barycentricBuffer[bufferIndex].Z = k;
                            //灰度值
                            //SetPixel(new MKVector4(255) * (depth + 1) * 0.5f, x, y);
                        }
                    }
                }
        });
    }

    private void Shading(AllocSpan<float> v2fs, IShader shader)
    {
        Parallel.For(0, pixels, pixelIndex =>
        {
            int vectorIndex = coveredIndexBuffer[pixelIndex];
            if (vectorIndex == -1) return;

            Span<float> v2f = stackalloc float[shader.v2fLength];
            int v2fsIndex1 = vectorIndex * 3 * shader.v2fLength;
            int v2fsIndex2 = v2fsIndex1 + shader.v2fLength;
            int v2fsIndex3 = v2fsIndex2 + shader.v2fLength;

            for (int index = 0; index < shader.v2fLength; index++)
                v2f[index] = v2fs[v2fsIndex1 + index] * barycentricBuffer[pixelIndex].X +
                             v2fs[v2fsIndex2 + index] * barycentricBuffer[pixelIndex].Y +
                             v2fs[v2fsIndex3 + index] * barycentricBuffer[pixelIndex].Z;
            SetPixel(shader.Frag(v2f), pixelIndex);
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
        shader.light1 = lights[1];
        shader.camera = camera;

        int a2vLength = shader.a2vLength;
        int verticescount = model.Data.Length / a2vLength;

        int v2fLength = shader.v2fLength;
        using AllocSpan<float> v2fs = new(shader.v2fLength * verticescount);

        using AllocSpan<MKVector4> vectors = new(verticescount);

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

        for (int i = 0; i < coveredIndexBuffer.Length;i++)
            coveredIndexBuffer[i] = -1;
        TimeTest.Run(() =>
            Rasterize(vectors, verticescount)
        , "Rasterize");

        TimeTest.Run(() =>
        {
            Shading(v2fs, shader);
        }, "Shading");
    }

    public ReadOnlySpan<MKVector3> GetFrame()
    {
        Reset();

        foreach (Model model in models)
            RenderModel(model);

        return pixelsBuffer;
    }

    public void Clear(MKVector4 color) =>
        background = color;

    private void Reset()
    {
        for (int bufferIndex = 0; bufferIndex < pixels; bufferIndex++)
            SetPixel(background, bufferIndex);

        for (int i = 0; i < pixels; i++)
            zBuffer[i] = float.NegativeInfinity;
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