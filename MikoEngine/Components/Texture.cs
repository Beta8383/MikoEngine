namespace MikoEngine;

using static MikoEngine.MKMath;

public unsafe class Texture
{
    internal byte[] data;
    int width, height, bytesPerPixel, totalPixels;

    public Texture(int width, int height, int bytesPerPixel)
    {
        this.width = width;
        this.height = height;
        this.bytesPerPixel = bytesPerPixel;
        totalPixels = width * height * bytesPerPixel;
        data = new byte[totalPixels];
        //data = (byte*)Marshal.AllocHGlobal(totalPixels * sizeof(byte));
    }

    public Span<byte> GetPixelsData()
    {
        if (data == null)
            throw new Exception();
        return new Span<byte>(data);
        //return new Span<byte>(data, totalPixels);
    }

    public MKVector3 GetColor(MKVector2 uv)
    {
        float u = Limit(uv.X, 0f, 1f);
        float v = Limit(uv.Y, 0f, 1f);
        int x = (int)(width * u);
        int y = (int)((height - 1) * (1 - v));
        int index = (y * width + x) * bytesPerPixel;
        return new(data[index] / 255f, data[index + 1] / 255f, data[index + 2] / 255f);
    }

    ~Texture()
    {
        //if (data != null)
        //    Marshal.FreeHGlobal((nint)data);
    }
}