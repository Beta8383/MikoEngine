namespace MikoEngine;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public static class MKEngineExtension
{
    public static Texture AddTexture(this MKEngine engine, string path)
    {
        try
        {
            using Image<Rgba32> image = Image.Load<Rgba32>(path);
            Texture texture = new(image.Width, image.Height, 3);
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
            engine.textures.Add(texture);
            return texture;
        }
        catch (Exception e)
        {
            return engine.AddTexture(new MKVector4(0f, 0f, 0f, 1f));
        }
    }

    public static Texture AddTexture(this MKEngine engine, MKVector4 color)
    {
        Texture texture = new(1, 1, 3);
        texture.Data[0] = color;
        engine.textures.Add(texture);
        return texture;
    }
}