namespace MikoEngine;

using System.Runtime.CompilerServices;

public abstract class IShader
{
    internal abstract MKVector4 Vert(Span<float> input, Span<float> ouput);
    internal abstract MKVector4 Frag(Span<float> input);
    internal virtual int a2vLength { get; }
    internal virtual int v2fLength { get; }

    protected internal MKMatrix4x4 modelTransform { get; internal set; }
    protected internal MKMatrix4x4 projectionTransform { get; internal set; }
    protected internal MKMatrix4x4 cameraTransform { get; internal set; }

    #region Lights
    protected internal Light light0 { get; internal set; }
    protected internal Light light1 { get; internal set; }
    protected internal Light light2 { get; internal set; }
    protected internal Light light3 { get; internal set; }
    #endregion

    protected internal Camera camera;

    #region Inline Method
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected MKVector4 WorldToScreenPos(MKVector3 p) =>
        projectionTransform * cameraTransform * modelTransform * new MKVector4(p, 1f);

    protected static MKVector3 TexelColor(ref Texture texture, MKVector2 uv)
    {
        float u = Math.Clamp(uv.X, 0f, 1f);
        float v = Math.Clamp(uv.Y, 0f, 1f);
        int x = (int)((texture.width - 1) * u);
        int y = (int)((texture.height - 1) * (1 - v));
        int index = (y * texture.width + x) * texture.bytesPerPixel;
        return new(texture.data[index], texture.data[index + 1], texture.data[index + 2]);
    }

    protected static MKVector3 TexelColor(ref Texture texture, MKVector2 uv, int i)
    {
        float x = Math.Clamp(uv.X, 0f, 1f) * (texture.width - 1);
        float y = (1 - Math.Clamp(uv.Y, 0f, 1f)) * (texture.height - 1);
        int x0 = (int)Math.Floor(x);
        int y0 = (int)Math.Ceiling(y);
        int x1 = (int)Math.Ceiling(x);
        int y1 = (int)Math.Floor(y);

        int pos0_0 = (y0 * texture.width + x0) * texture.bytesPerPixel;
        int pos1_0 = (y0 * texture.width + x1) * texture.bytesPerPixel;
        int pos0_1 = (y1 * texture.width + x0) * texture.bytesPerPixel;
        int pos1_1 = (y1 * texture.width + x1) * texture.bytesPerPixel;

        MKVector3 color0_0 = new(texture.data[pos0_0], texture.data[pos0_0 + 1], texture.data[pos0_0 + 2]);
        MKVector3 color1_0 = new(texture.data[pos1_0], texture.data[pos1_0 + 1], texture.data[pos1_0 + 2]);
        MKVector3 color0_1 = new(texture.data[pos0_1], texture.data[pos0_1 + 1], texture.data[pos0_1 + 2]);
        MKVector3 color1_1 = new(texture.data[pos1_1], texture.data[pos1_1 + 1], texture.data[pos1_1 + 2]);

        MKVector3 left = color0_0 + (color0_1 - color0_0) * (y0 - y);
        MKVector3 right = color1_0 + (color1_1 - color1_0) * (y0 - y);
        return left + (right - left) * (x - x0);
    }
    #endregion
}