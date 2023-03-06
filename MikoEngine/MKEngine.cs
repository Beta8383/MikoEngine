namespace MikoEngine;

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

#if index
    private void Rasterize(ReadOnlySpan<MKVector4> vectors, ReadOnlySpan<int> vectorsIndices)
    {
        for (int triangleIndex = 0; triangleIndex < vectorsIndices.Length; triangleIndex += 3)
        {
            
            ref readonly var vec1 = ref vectors[vectorsIndices[triangleIndex]];
            ref readonly var vec2 = ref vectors[vectorsIndices[triangleIndex + 1]];
            ref readonly var vec3 = ref vectors[vectorsIndices[triangleIndex + 2]];
#else
        private void Rasterize(ReadOnlySpan<MKVector4> vectors)
    {
        for (int index = 0; index < vectors.Length; index += 3)
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
                        //depth = p[0].Z * i + p[1].Z * j + p[2].Z * k;
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

    private void Shading(Span<MKVector4> vectors, Span<int> vectorsIndices, Span<float> fragParams, IShader shader)
    {
        Span<float> fragParam = stackalloc float[shader.FragmentDataSize];
        int triangleIndex;

        for (int bufferIndex = 0; bufferIndex < width * height; bufferIndex++)
        {
            triangleIndex = coveredIndexBuffer[bufferIndex];
            if (triangleIndex != -1)
            {
                int fragParamIndex1 = vectorsIndices[triangleIndex] * shader.FragmentDataSize;
                int fragParamIndex2 = vectorsIndices[triangleIndex + 1] * shader.FragmentDataSize;
                int fragParamIndex3 = vectorsIndices[triangleIndex + 2] * shader.FragmentDataSize;

                for (int paraIndex = 0; paraIndex < shader.FragmentDataSize; paraIndex++)
                    fragParam[paraIndex] = fragParams[fragParamIndex1 + paraIndex] * barycentricBuffer[bufferIndex].i +
                                        fragParams[fragParamIndex2 + paraIndex] * barycentricBuffer[bufferIndex].j +
                                        fragParams[fragParamIndex3 + paraIndex] * barycentricBuffer[bufferIndex].k;

                shader.Fragment(fragParam, light, camera, out MKVector4 color);
                SetPixel(color, bufferIndex);
            }
        }
    }

    private void RenderModel(Model model)
    {
        IShader shader = new TestShader();

        fixed (float* modeldata = model.Data)
        {
            float* a2vs = modeldata;

            int a2vLength = shader.a2vSize / sizeof(float);
            int verticescount = model.VertexCount;

            int v2fLenght = shader.v2fSize / sizeof(float);
            nint v2fsAllocPtr = Marshal.AllocHGlobal(shader.v2fSize * verticescount * sizeof(float));
            float* v2fsPtr = (float*)v2fsAllocPtr;

            nint vectorsAllocPtr = Marshal.AllocHGlobal(verticescount * sizeof(MKVector4));
            MKVector4* vectorsPtr = (MKVector4*)vectorsAllocPtr;
            Span<MKVector4> vectors = new(vectorsPtr, verticescount);

            Parallel.For(0, verticescount, index =>
            {
                Span<float> a2v = new(a2vs + index * a2vLength, a2vLength);
                Span<float> v2f = new(v2fsPtr + index * v2fLenght, v2fLenght);
                ref MKVector4 vector = ref *(vectorsPtr + index);
                vector = shader.Vert(a2v, v2f);
                vector = viewportTransform * vector;
                vector.X /= vector.W;
                vector.Y /= vector.W;
                vector.Z /= vector.W;
            });


            Array.Fill<int>(coveredIndexBuffer, -1);
            Rasterize(vectors);
            //DrawLine(vertices[model.Indices[i * 3]], vertices[model.Indices[i * 3 + 1]], model.Color);
            //DrawLine(vertices[model.Indices[i * 3]], vertices[model.Indices[i * 3 + 2]], model.Color);
            //DrawLine(vertices[model.Indices[i * 3 + 2]], vertices[model.Indices[i * 3 + 1]], model.Color);

            Shading(vectors, Indices, fragInPara, shader);

            Marshal.FreeHGlobal(v2fsAllocPtr);
            Marshal.FreeHGlobal(vectorsAllocPtr);
        }
    }

    public ReadOnlySpan<byte> GetFrame()
    {
        Reset();

        foreach (ModelBase model in models)
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

    public MKEngine AddModel(ModelBase model)
    {
        models.Add(model);
        return this;
    }
}