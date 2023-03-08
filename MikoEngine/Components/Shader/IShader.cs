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
    #endregion
}