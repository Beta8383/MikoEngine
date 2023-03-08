#define TimeTest
namespace MikoEngine;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MikoEngine.Assets;
using MikoEngine.Components;
using static MikoEngine.MKMath;

public unsafe partial class MKEngine
{
    const int BytesPerPixel = 3;

    private readonly int width, height;

    private byte[] pixelsBuffer;
    private float[] zBuffer;
    private int[] coveredIndexBuffer;
    private (float i, float j, float k)[] barycentricBuffer;

    private MKVector4 background;

    List<Model> models = new();
    Light light;
    Camera camera;

    MKMatrix4x4 viewportTransform = MKMatrix4x4.Identity,
                cameraTransform = MKMatrix4x4.Identity,
                projectionTransform = MKMatrix4x4.Identity;

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
        barycentricBuffer = new (float i, float j, float k)[height * width];
        pixelsBuffer = new byte[width * height * BytesPerPixel];
    }

    private void SetPixel(MKVector4 color, int bufferIndex)
    {
        int index = bufferIndex * BytesPerPixel;
        pixelsBuffer[index] = (byte)Limit(color.X, 0f, 255f);
        pixelsBuffer[index + 1] = (byte)Limit(color.Y, 0f, 255f);
        pixelsBuffer[index + 2] = (byte)Limit(color.Z, 0f, 255f);
    }

    private void Rasterize(MKVector4* vectors, int count)
    {
#if index
        for (int triangleIndex = 0; triangleIndex < vectorsIndices.Length; triangleIndex += 3)
        {
            
            ref readonly var vec1 = ref vectors[vectorsIndices[triangleIndex]];
            ref readonly var vec2 = ref vectors[vectorsIndices[triangleIndex + 1]];
            ref readonly var vec3 = ref vectors[vectorsIndices[triangleIndex + 2]];
#else
        for (int index = 0; index < count; index += 3)
        {
            ref readonly var vec1 = ref vectors[index];
            ref readonly var vec2 = ref vectors[index + 1];
            ref readonly var vec3 = ref vectors[index + 2];
#endif

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
#if index
                            coveredIndexBuffer[y * width + x] = triangleIndex;
#else
                            coveredIndexBuffer[y * width + x] = index / 3;
#endif
                            barycentricBuffer[y * width + x].i = i;
                            barycentricBuffer[y * width + x].j = j;
                            barycentricBuffer[y * width + x].k = k;
                            //灰度值
                            //SetPixel(new MKVector4(255) * (depth + 1) * 0.5f, x, y);
                        }
                    }
                }
        }
    }

    private void Shading(float* _v2fs, IShader shader)
    {
        float* v2fs = _v2fs;

        const int pixelsInGroup = 50000;
        int pixels = width * height;
        int groupCount = (int)Math.Ceiling(pixels / (float)pixelsInGroup);

        Parallel.For(0, groupCount, groupIndex =>
        {
            int startIndex = groupIndex * pixelsInGroup;
            int endIndex = Min((groupIndex + 1) * pixelsInGroup, pixels);
            float* v2f = stackalloc float[shader.v2fLength];
            for (int bufferIndex = startIndex; bufferIndex < endIndex; bufferIndex++)
            {
                int vectorIndex = coveredIndexBuffer[bufferIndex];
                if (vectorIndex != -1)
                {
                    int v2fsIndex1 = vectorIndex * 3 * shader.v2fLength;
                    int v2fsIndex2 = v2fsIndex1 + shader.v2fLength;
                    int v2fsIndex3 = v2fsIndex2 + shader.v2fLength;

                    for (int index = 0; index < shader.v2fLength; index++)
                        v2f[index] = v2fs[v2fsIndex1 + index] * barycentricBuffer[bufferIndex].i +
                                     v2fs[v2fsIndex2 + index] * barycentricBuffer[bufferIndex].j +
                                     v2fs[v2fsIndex3 + index] * barycentricBuffer[bufferIndex].k;
                    SetPixel(shader.Frag(new(v2f, shader.v2fLength)), bufferIndex);
                }
            }
        });

        /*float* v2f = stackalloc float[shader.v2fLength];
        for (int bufferIndex = 0; bufferIndex < pixels; bufferIndex++)
        {
            int vectorIndex = coveredIndexBuffer[bufferIndex];
            if (vectorIndex != -1)
            {
                int v2fsIndex1 = vectorIndex * 3 * shader.v2fLength;
                int v2fsIndex2 = v2fsIndex1 + shader.v2fLength;
                int v2fsIndex3 = v2fsIndex2 + shader.v2fLength;

                for (int index = 0; index < shader.v2fLength; index++)
                    v2f[index] = v2fs[v2fsIndex1 + index] * barycentricBuffer[bufferIndex].i +
                                 v2fs[v2fsIndex2 + index] * barycentricBuffer[bufferIndex].j +
                                 v2fs[v2fsIndex3 + index] * barycentricBuffer[bufferIndex].k;
                SetPixel(shader.Frag(new(v2f, shader.v2fLength)), bufferIndex);
            }
        }*/
    }

    private void RenderModel(Model model)
    {
        IShader shader = new TestShader();
        fixed (float* modeldata = model.Data)
        {
            shader.uni = new()
            {
                modelTransform = model.Transform,
                projectionTransform = projectionTransform,
                cameraTransform = cameraTransform,
                light = light,
                camera = camera
            };

            float* a2vs = modeldata;

            int a2vLength = shader.a2vLength;
            int verticescount = model.Data.Length / a2vLength;

            int v2fLenght = shader.v2fLength;
            float* v2fsPtr = (float*)Marshal.AllocHGlobal(shader.v2fLength * verticescount * sizeof(float));

            MKVector4* vectorsPtr = (MKVector4*)Marshal.AllocHGlobal(verticescount * sizeof(MKVector4));

#if DEBUG
            Span<float> v2fsSpan = new(v2fsPtr, shader.v2fLength * verticescount);
            Span<float> vectorsSpan = new(vectorsPtr, verticescount);
#endif

            TimeTest.Run(() =>
            {
                for (int index = 0; index < verticescount; index++)
                //Parallel.For(0, verticescount, index =>
                {
                    Span<float> a2v = new(a2vs + index * a2vLength, a2vLength);
                    Span<float> v2f = new(v2fsPtr + index * v2fLenght, v2fLenght);
                    ref MKVector4 vector = ref *(vectorsPtr + index);
                    vector = shader.Vert(a2v, v2f);
                    vector = viewportTransform * vector;
                    vector.X /= vector.W;
                    vector.Y /= vector.W;
                    vector.Z /= vector.W;
                }
            }, "VertexShader");

            Array.Fill<int>(coveredIndexBuffer, -1);
            TimeTest.Run(() =>
                Rasterize(vectorsPtr, verticescount)
            , "Rasterize");

            TimeTest.Run(() =>
            {
                Shading(v2fsPtr, shader);
            }, "Shading");

            Marshal.FreeHGlobal((nint)v2fsPtr);
            Marshal.FreeHGlobal((nint)vectorsPtr);
        }
    }

    public ReadOnlySpan<byte> GetFrame()
    {
        Reset();

        foreach (Model model in models)
            RenderModel(model);

        return new ReadOnlySpan<byte>(pixelsBuffer);
    }

    public void Clear(MKVector4 color) =>
        background = color;

    private void Reset()
    {
        for (int bufferIndex = 0; bufferIndex < width * height; bufferIndex++)
            SetPixel(background, bufferIndex);

        Array.Fill<float>(zBuffer, float.NegativeInfinity);
    }

    private static MKMatrix4x4 CreateCameraTransform(MKVector3 position, MKVector3 direction, MKVector3 up)
    {
        //cameraTransform = Matrix4x4.CreateLookAt(camera.Position, camera.Direction, camera.Up);
        MKVector3 xaxis, yaxis, zaxis;

        zaxis = -(direction - position);
        zaxis = zaxis.Normalize();

        xaxis = up ^ zaxis;
        xaxis = xaxis.Normalize();

        yaxis = zaxis ^ xaxis;
        yaxis = yaxis.Normalize();

        var translate = MKMatrix4x4.CreateTranslation(-position.X, -position.Y, -position.Z);

        var rotate = new MKMatrix4x4(
            xaxis.X, xaxis.Y, xaxis.Z, 0f,
            yaxis.X, yaxis.Y, yaxis.Z, 0f,
            zaxis.X, zaxis.Y, zaxis.Z, 0f,
            0f, 0f, 0f, 1f);

        return rotate * translate;
    }

    private static MKMatrix4x4 CreateOrthographicTransform(float width, float height, float zNearPlane, float zFarPlane) =>
        new MKMatrix4x4(
            2f / width, 0f, 0f, 0f,
            0f, 2f / height, 0f, 0f,
            0f, 0f, 2f / (zFarPlane - zNearPlane), (zFarPlane + zNearPlane) / (zFarPlane - zNearPlane),
            0f, 0f, 0f, 1f
        );

    private static MKMatrix4x4 CreatePerspectiveTransform(float width, float height, float zNearPlane, float zFarPlane)
    {
        MKMatrix4x4 scale = new(
            -zNearPlane, 0f, 0f, 0f,
            0f, -zNearPlane, 0f, 0f,
            0f, 0f, -zNearPlane - zFarPlane, -zNearPlane * zFarPlane,
            0f, 0f, 1f, 0f
        );

        return CreateOrthographicTransform(width, height, zNearPlane, zFarPlane) * scale;
    }

    public MKEngine SetCamera(Camera camera)
    {
        this.camera = camera;
        if (camera.zNearPlane <= 0 || camera.zFarPlane <= 0 || camera.zNearPlane >= camera.zFarPlane)
            throw new ArgumentException();

        cameraTransform = CreateCameraTransform(camera.Position, camera.Direction, camera.Up);

        if (camera.Mode == ProjectionMode.Orthographic)
            projectionTransform = CreateOrthographicTransform(camera.Width, camera.Height, camera.zNearPlane, camera.zFarPlane);
        else projectionTransform = CreatePerspectiveTransform(camera.Width, camera.Height, camera.zNearPlane, camera.zFarPlane);

        return this;
    }

    public MKEngine SetLight(Light light)
    {
        this.light = light;
        return this;
    }

    public MKEngine AddModel(Model model)
    {
        models.Add(model);
        return this;
    }
}