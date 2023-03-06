namespace MikoEngine.Components;

using MikoEngine.Assets;

public abstract class Model
{
    public IShader Shader = new TestShader();
    public float[] Data = Array.Empty<float>();
    public int VertexSize, VertexCount;
}