namespace MikoEngine.Assets;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public abstract class Shader<a2v, v2f> : IShader where a2v : unmanaged where v2f : unmanaged
{
    public Uniform uni
    {
        get;
        internal set;
    }

    public int a2vSize
    {
        get => Marshal.SizeOf<a2v>();
    }

    public int v2fSize
    {
        get => Marshal.SizeOf<v2f>();
    }

    public MKVector4 WorldToScreenPos(MKVector3 p) =>
        uni.projectionTransform * uni.cameraTransform * uni.modelTransform * new MKVector4(p, 1f);

    public abstract MKVector4 Vert(ref a2v input, ref v2f output);
    public abstract MKVector4 Frag(ref v2f input);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static ref TTo SpanCast<TFrom, TTo>(Span<TFrom> source) =>
        ref Unsafe.As<TFrom, TTo>(ref MemoryMarshal.GetReference(source));

    public MKVector4 Vert(Span<float> input, Span<float> output) =>
        Vert(ref SpanCast<float, a2v>(input), ref SpanCast<float, v2f>(output));

    public MKVector4 Frag(Span<float> input) =>
        Frag(ref SpanCast<float, v2f>(input));
}