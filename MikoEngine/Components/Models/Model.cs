namespace MikoEngine;

public class Model :IDisposable
{
    internal IShader Shader = new LitShader();
    internal AllocSpan<float> Data;
    internal MKMatrix4x4 Transform;

    ~Model() => Data?.Dispose();

    public void Dispose()
    {
        Data?.Dispose();
        GC.SuppressFinalize(this);
    }
}