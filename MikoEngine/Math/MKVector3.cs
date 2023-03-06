using System.Runtime.CompilerServices;

namespace MikoEngine;

public struct MKVector3 : IEquatable<MKVector3>
{
    #region Specific Value
    public static MKVector3 Zero { get => new(0f, 0f, 0f); }
    public static MKVector3 UnitX { get => new MKVector3(1f, 0f, 0f); }
    public static MKVector3 UnitY { get => new MKVector3(0f, 1f, 0f); }
    public static MKVector3 UnitZ { get => new MKVector3(0f, 0f, 1f); }
    public static MKVector3 Identity { get => new MKVector3(1f, 1f, 1f); }
    #endregion

    public float X, Y, Z;

    public MKVector3() : this(0f, 0f, 0f) { }

    public MKVector3(float value) : this(value, value, value) { }

    public MKVector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public MKVector3(ReadOnlySpan<float> data)
    {
        if (data.Length < 3)
            throw new Exception();
        X = data[0];
        Y = data[1];
        Z = data[2];
    }

    public float this[int i]
    {
        get => GetElement(ref this, i);
        set => SetElement(ref this, i, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float GetElement(ref MKVector3 vector, int i)
    {
        if (i is < 0 or > 2)
            throw new Exception();
        return Unsafe.Add(ref Unsafe.As<MKVector3, float>(ref vector), i);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetElement(ref MKVector3 vector, int i, float value)
    {
        if (i is < 0 or > 2)
            throw new Exception();
        Unsafe.Add(ref Unsafe.As<MKVector3, float>(ref vector), i) = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector3 operator +(MKVector3 vec) => vec;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector3 operator -(MKVector3 vec) =>
        new MKVector3(-vec.X, -vec.Y, -vec.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector3 operator +(MKVector3 vec1, MKVector3 vec2) =>
        new MKVector3(vec1.X + vec2.X, vec1.Y + vec2.Y, vec1.Z + vec2.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector3 operator -(MKVector3 vec1, MKVector3 vec2) =>
        new MKVector3(vec1.X - vec2.X, vec1.Y - vec2.Y, vec1.Z - vec2.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float operator *(MKVector3 vec1, MKVector3 vec2) =>
        vec1.X * vec2.X + vec1.Y * vec2.Y + vec1.Z * vec2.Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector3 operator *(MKVector3 vec, float scale) =>
        new MKVector3(vec.X * scale, vec.Y * scale, vec.Z * scale);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector3 operator *(float scale, MKVector3 vec) => vec * scale;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector3 operator ^(MKVector3 vec1, MKVector3 vec2) =>
        new MKVector3(
            vec1.Y * vec2.Z - vec1.Z * vec2.Y,
            vec1.Z * vec2.X - vec1.X * vec2.Z,
            vec1.X * vec2.Y - vec1.Y * vec2.X);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector3 Lerp(MKVector3 vec1, MKVector3 vec2) =>
        new MKVector3(vec1.X * vec2.X, vec1.Y * vec2.Y, vec1.Z * vec2.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector3 Lerp(MKVector3 vec1, MKVector3 vec2, float value) =>
        vec1 * value + vec2 * (1 - value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Distance() =>
        MathF.Sqrt(X * X + Y * Y + Z * Z);

    public MKVector3 Normalize()
    {
        float distance = Distance();
        if (distance == .0f)
            return MKVector3.Zero;

        return this * (1f / distance);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(MKVector3 other) =>
        X == other.X && Y == other.Y && Z == other.Z;
}