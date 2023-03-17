namespace MikoEngine;

public class Texture
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
}