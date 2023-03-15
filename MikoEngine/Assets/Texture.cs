namespace MikoEngine;

public class Texture : IDisposable
{
    internal AllocSpan<MKVector4> Data;
    internal readonly int Width, Height, BytesPerPixel;

    internal Texture(int width, int height, int bytesPerPixel)
    {
        Width = width;
        Height = height;
        BytesPerPixel = bytesPerPixel;
        Data = new(Width * Height);
    }

    ~Texture() => Data?.Dispose();

    public void Dispose()
    {
        Data?.Dispose();
        GC.SuppressFinalize(this);
    }
}