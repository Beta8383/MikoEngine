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
                data = new float[image.Width * image.Height * 3],
            };
            int index = 0;
            for (int i = image.Height - 1; i >= 0; i--)
                for (int j = 0; j < image.Width; j++)
                {
                    Rgb24 color = image[j, i];
                    texture.data[index++] = color.R / 255f;
                    texture.data[index++] = color.G / 255f;
                    texture.data[index++] = color.B / 255f;
                }
            return texture;
        }
        catch (Exception e)
        {
            return Create(MKVector3.Zero);
        }
    }

    public static Texture Create(MKVector3 color)
    {
        Texture texture = new()
        {
            width = 1,
            height = 1,
            bytesPerPixel = 3,
            data = new float[3],
        };
        texture.data[0] = Math.Clamp(color.X, 0f, 1f);
        texture.data[1] = Math.Clamp(color.Y, 0f, 1f);
        texture.data[2] = Math.Clamp(color.Z, 0f, 1f);
        return texture;
    }
}