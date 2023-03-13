namespace MikoEngine;

public class Texture : IDisposable
{
    internal AllocSpan<MKVector4> Data;
    internal int width, height, bytesPerPixel;
    ~Texture() => Data?.Dispose();

    public void Dispose()
    {
        Data?.Dispose();
        GC.SuppressFinalize(this);
    }
}