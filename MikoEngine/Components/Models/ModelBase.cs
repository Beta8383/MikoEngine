namespace MikoEngine.Components;

public abstract class ModelBase
{
    public MKMatrix4x4 ModelTransform = MKMatrix4x4.Identity;

    public float[] VertexData
    {
        get;
        protected set;
    }

    public int[] Indices
    {
        get;
        protected set;
    }

    public ModelBase()
    {
        VertexData = Array.Empty<float>();
        Indices = Array.Empty<int>();
    }
}