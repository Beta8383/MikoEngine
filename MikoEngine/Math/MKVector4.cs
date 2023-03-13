using System.Runtime.CompilerServices;

namespace MikoEngine;

public struct MKVector4 : IEquatable<MKVector4>
{
    #region Specific Value
    public static MKVector4 Zero { get => new(0f, 0f, 0f, 0f); }
    public static MKVector4 UnitX { get => new(1f, 0f, 0f, 0f); }
    public static MKVector4 UnitY { get => new(0f, 1f, 0f, 0f); }
    public static MKVector4 UnitZ { get => new(0f, 0f, 1f, 0f); }
    public static MKVector4 UnitW { get => new(0f, 0f, 0f, 1f); }
    public static MKVector4 Identity { get => new(1f, 1f, 1f, 1f); }
    #endregion

    public float X, Y, Z, W;

    public MKVector4() : this(0f, 0f, 0f, 0f) { }

    public MKVector4(float value) : this(value, value, value, value) { }

    public MKVector4(MKVector3 vec, float w) : this(vec.X, vec.Y, vec.Z, w) { }

    public MKVector4(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public MKVector4(ReadOnlySpan<float> data)
    {
        if (data.Length < 4)
            throw new Exception();
        X = data[0];
        Y = data[1];
        Z = data[2];
        W = data[3];
    }

    public float this[int i]
    {
        get => GetElement(ref this, i);
        set => SetElement(ref this, i, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float GetElement(ref MKVector4 vector, int i)
    {
        if (i is < 0 or > 3)
            throw new Exception();
        return Unsafe.Add(ref Unsafe.As<MKVector4, float>(ref vector), i);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetElement(ref MKVector4 vector, int i, float value)
    {
        if (i is < 0 or > 3)
            throw new Exception();
        Unsafe.Add(ref Unsafe.As<MKVector4, float>(ref vector), i) = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector4 operator +(MKVector4 vec) => vec;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector4 operator -(MKVector4 vec) =>
        new MKVector4(-vec.X, -vec.Y, -vec.Z, -vec.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector4 operator +(MKVector4 vec1, MKVector4 vec2) =>
        new MKVector4(vec1.X + vec2.X, vec1.Y + vec2.Y, vec1.Z + vec2.Z, vec1.W + vec2.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector4 operator -(MKVector4 vec1, MKVector4 vec2) =>
        new MKVector4(vec1.X - vec2.X, vec1.Y - vec2.Y, vec1.Z - vec2.Z, vec1.W - vec2.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float operator *(MKVector4 vec1, MKVector4 vec2) =>
        vec1.X * vec2.X + vec1.Y * vec2.Y + vec1.Z * vec2.Z + vec1.W * vec2.W;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector4 operator %(MKVector4 vec1, MKVector4 vec2) =>
        new MKVector4(vec1.X * vec2.X, vec1.Y * vec2.Y, vec1.Z * vec2.Z, vec1.W * vec2.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector4 operator *(MKVector4 vec, float scale) =>
        new MKVector4(vec.X * scale, vec.Y * scale, vec.Z * scale, vec.W * scale);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector4 operator *(float scale, MKVector4 vec) => vec * scale;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector4 Lerp(MKVector4 vec1, MKVector4 vec2) =>
        new MKVector4(vec1.X * vec2.X, vec1.Y * vec2.Y, vec1.Z * vec2.Z, vec1.W * vec2.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector4 Lerp(MKVector4 vec1, MKVector4 vec2, float value) =>
        vec1 * value + vec2 * (1 - value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Distance() =>
        MathF.Sqrt(X * X + Y * Y + Z * Z + W * W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MKVector4 Normalize()
    {
        float distance = Distance();
        if (distance == .0f) return MKVector4.Zero;
        return this * (1f / distance);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(MKVector4 other) =>
        X == other.X && Y == other.Y && Z == other.Z && W == other.W;
}