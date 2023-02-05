namespace MikoEngine.Assets;

class DefaultVertexShader : IVertexShader<Vertex, Fragmentdata>
{
    public static void main(in Vertex vertex, ref Fragmentdata data, MKMatrix4x4 projectionTransform, MKMatrix4x4 cameraTransform, MKMatrix4x4 modelTransform, out MKVector4 Position)
    {
        data.YLerp = vertex.Position.Y + 0.5f;
        MKVector4 FragPosition = modelTransform * (new MKVector4(vertex.Position, 1f));
        Position = projectionTransform * cameraTransform * FragPosition;
        data.FragPosition = new(FragPosition.X, FragPosition.Y, FragPosition.Z);
        data.Normal = vertex.Normal;
    }
}