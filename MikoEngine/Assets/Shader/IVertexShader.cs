namespace MikoEngine.Assets;

public interface IVertexShader<VertexData, FragmentData> where VertexData : struct where FragmentData : struct
{
    static abstract void main(in VertexData inPara, ref FragmentData outPara, MKMatrix4x4 projectionTransform, MKMatrix4x4 cameraTransform, MKMatrix4x4 modelTransform, out MKVector4 Position);
}