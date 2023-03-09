namespace MikoEngine;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public static class TextureCreator
{
    public static Texture Create(string path)
    {
        try
        {
            using Image<Rgb24> image = Image.Load<Rgb24>(path);
            Texture texture = new()
            {
                width = image.Width,
                height = image.Height,
                bytesPerPixel = 3,
                data = new byte[image.Width * image.Height * 3],
            };
            image.CopyPixelDataTo(texture.data);
            return texture;
        }
        catch (Exception e)
        {
            return default;
        }
    }

    public static Texture Create(MKVector3 color)
    {
        Texture texture = new()
        {
            width = 1,
            height = 1,
            bytesPerPixel = 3,
            data = new byte[3],
        };
        texture.data[0] = (byte)(Math.Clamp(color.X, 0f, 1f) * 255);
        texture.data[1] = (byte)(Math.Clamp(color.Y, 0f, 1f) * 255);
        texture.data[2] = (byte)(Math.Clamp(color.Z, 0f, 1f) * 255);
        return texture;
    }
}