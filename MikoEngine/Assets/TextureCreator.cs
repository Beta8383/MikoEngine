namespace MikoEngine;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public static class TextureCreator
{
    public static Texture Create(string path)
    {
        try
        {
            using Image<Rgba32> image = Image.Load<Rgba32>(path);
            Texture texture = new()
            {
                width = image.Width,
                height = image.Height,
                bytesPerPixel = 3,
                Data = new(image.Width * image.Height),
            };
            int index = 0;
            for (int i = image.Height - 1; i >= 0; i--)
                for (int j = 0; j < image.Width; j++)
                {
                    Rgba32 color = image[j, i];
                    texture.Data[index].X = color.R / 255f;
                    texture.Data[index].Y = color.G / 255f;
                    texture.Data[index].Z = color.B / 255f;
                    texture.Data[index].W = color.A / 255f;
                    index++;
                }
            return texture;
        }
        catch (Exception e)
        {
            return Create(MKVector4.Zero);
        }
    }

    public static Texture Create(MKVector4 color)
    {
        Texture texture = new()
        {
            width = 1,
            height = 1,
            bytesPerPixel = 3,
            Data = new(1),
        };
        texture.Data[0] = color;
        return texture;
    }
}