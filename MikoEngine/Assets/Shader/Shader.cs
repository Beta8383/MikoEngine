namespace MikoEngine.Assets;

using System.Runtime.InteropServices;
using MikoEngine.Components;

public unsafe sealed class Shader<VertexData, FragmentData> : ShaderBase where VertexData : struct where FragmentData : struct
{
    public Shader()
    {
        FragmentDataSize = Marshal.SizeOf(typeof(FragmentData)) / sizeof(float);
        VertexDataSize = Marshal.SizeOf(typeof(VertexData)) / sizeof(float);
    }

    private delegate*<in VertexData, ref FragmentData, MKMatrix4x4, MKMatrix4x4, MKMatrix4x4, out MKVector4, void> vertexShader;

    private delegate*<in FragmentData, Light, Camera, out MKVector4, void> fragmentShader;

    public void SetShader<_vertex, _fragment>() where _vertex : IVertexShader<VertexData, FragmentData> where _fragment : IFragmentShader<FragmentData>
    {
        vertexShader = &_vertex.main;
        fragmentShader = &_fragment.main;
    }

    internal override void Vertex(ReadOnlySpan<float> inPara, Span<float> outPara, MKMatrix4x4 projectionTransform, MKMatrix4x4 cameraTransform, MKMatrix4x4 modelTransform, out MKVector4 Position)
    {
        if (vertexShader is null)
            throw new Exception();
        ReadOnlySpan<VertexData> vecInPara = MemoryMarshal.Cast<float, VertexData>(inPara);
        Span<FragmentData> vecOutPara = MemoryMarshal.Cast<float, FragmentData>(outPara);
        vertexShader(in vecInPara[0], ref vecOutPara[0], projectionTransform, cameraTransform, modelTransform, out Position);
    }

    internal override void Fragment(ReadOnlySpan<float> inPara, Light light, Camera camera, out MKVector4 color)
    {
        if (fragmentShader is null)
            throw new Exception();
        ReadOnlySpan<FragmentData> fragInPara = MemoryMarshal.Cast<float, FragmentData>(inPara);
        fragmentShader(in fragInPara[0], light, camera, out color);
    }
}