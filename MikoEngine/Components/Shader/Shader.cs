namespace MikoEngine;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public abstract class Shader<a2v> : IShader where a2v : unmanaged
{
    internal override int a2vLength => Marshal.SizeOf<a2v>() / sizeof(float);
    internal override int v2fLength => 0;

    #region Converter
    public abstract MKVector4 Vert(ref a2v input);
    public virtual MKVector4 Frag() => MKVector4.Zero;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static ref TTo SpanCast<TFrom, TTo>(Span<TFrom> source) =>
        ref Unsafe.As<TFrom, TTo>(ref MemoryMarshal.GetReference(source));

    internal override MKVector4 Vert(Span<float> input, Span<float> output) =>
        Vert(ref SpanCast<float, a2v>(input));

    internal override MKVector4 Frag(Span<float> input) =>
        Frag();
    #endregion
}

public abstract class Shader<a2v, v2f> : IShader where a2v : unmanaged where v2f : unmanaged
{
    internal override int a2vLength => Marshal.SizeOf<a2v>() / sizeof(float);
    internal override int v2fLength => Marshal.SizeOf<v2f>() / sizeof(float);

    #region Converter
    public abstract MKVector4 Vert(ref a2v input, ref v2f output);
    public abstract MKVector4 Frag(ref v2f input);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static ref TTo SpanCast<TFrom, TTo>(Span<TFrom> source) =>
        ref Unsafe.As<TFrom, TTo>(ref MemoryMarshal.GetReference(source));

    internal override MKVector4 Vert(Span<float> input, Span<float> output) =>
        Vert(ref SpanCast<float, a2v>(input), ref SpanCast<float, v2f>(output));

    internal override MKVector4 Frag(Span<float> input) =>
        Frag(ref SpanCast<float, v2f>(input));
    #endregion
}