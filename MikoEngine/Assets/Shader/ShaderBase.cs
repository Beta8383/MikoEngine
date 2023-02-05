using MikoEngine.Components;

namespace MikoEngine.Assets;

public abstract class ShaderBase
{
    internal int VertexDataSize
    {
        get;
        private protected set;
    }

    internal int FragmentDataSize
    {
        get;
        private protected set;
    }

    internal abstract void Vertex(ReadOnlySpan<float> inPara, Span<float> outPara, MKMatrix4x4 projectionTransform, MKMatrix4x4 cameraTransform, MKMatrix4x4 modelTransform, out MKVector4 Position);
    internal abstract void Fragment(ReadOnlySpan<float> inPara, Light light, Camera camera, out MKVector4 color);
}