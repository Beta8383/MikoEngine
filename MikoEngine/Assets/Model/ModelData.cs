namespace MikoEngine;

public class ModelData<VertexData> where VertexData : unmanaged
{
    MKMatrix4x4 ModelTransform
    {
        get;
        set;
    }
}