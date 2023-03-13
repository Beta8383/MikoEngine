namespace MikoEngine;

using System.Runtime.CompilerServices;
using static MikoEngine.MKMath;

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

    protected static MKVector4 TexelColor(Texture texture, MKVector2 uv)
    {
        int x = (int)((texture.width - 1) * Math.Clamp(uv.X, 0f, 1f));
        int y = (int)((texture.height - 1) * Math.Clamp(uv.Y, 0f, 1f));
        return texture.Data[y * texture.width + x];
    }

    protected static MKVector4 BilinearTexelColor(Texture texture, MKVector2 uv)
    {
        float x = (texture.width - 1) * Math.Clamp(uv.X, 0f, 1f);
        float y = (texture.height - 1) * Math.Clamp(uv.Y, 0f, 1f);

        int x0 = (int)Math.Floor(x);
        int y0 = (int)Math.Floor(y);
        int x1 = Min(x0 + 1, texture.width - 1);
        int y1 = Min(y0 + 1, texture.height - 1);

        MKVector4 color0_0 = texture.Data[y0 * texture.width + x0];
        MKVector4 color1_0 = texture.Data[y0 * texture.width + x1];
        MKVector4 color0_1 = texture.Data[y1 * texture.width + x0];
        MKVector4 color1_1 = texture.Data[y1 * texture.width + x1];

        MKVector4 left = MKVector4.Lerp(color0_0, color0_1, y1 - y);
        MKVector4 right = MKVector4.Lerp(color1_0, color1_1, y1 - y);
        return MKVector4.Lerp(left, right, x1 - x);
    }
    #endregion
}