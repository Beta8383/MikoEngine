using System.Runtime.CompilerServices;

namespace MikoEngine;

public struct MKVector2 : IEquatable<MKVector2>
{
    #region Specific Value
    public static MKVector2 Zero { get => new(0f, 0f); }
    public static MKVector2 UnitX { get => new(1f, 0f); }
    public static MKVector2 UnitY { get => new(0f, 1f); }
    #endregion

    public float X, Y;

    public MKVector2(float value) : this(value, value) { }

    public MKVector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector2 operator +(MKVector2 vec) => vec;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector2 operator -(MKVector2 vec) =>
        new MKVector2(-vec.X, -vec.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector2 operator +(MKVector2 vec1, MKVector2 vec2) =>
        new MKVector2(vec1.X + vec2.X, vec1.Y + vec2.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector2 operator -(MKVector2 vec1, MKVector2 vec2) =>
        new MKVector2(vec1.X - vec2.X, vec1.Y - vec2.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float operator *(MKVector2 vec1, MKVector2 vec2) =>
        vec1.X * vec2.X + vec1.Y * vec2.Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector2 operator %(MKVector2 vec1, MKVector2 vec2) =>
        new MKVector2(vec1.X * vec2.X, vec1.Y * vec2.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector2 operator *(MKVector2 vec, float scale) =>
        new MKVector2(vec.X * scale, vec.Y * scale);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKVector2 operator *(float scale, MKVector2 vec) => vec * scale;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Distance() =>
        MathF.Sqrt(X * X + Y * Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(MKVector2 other) =>
        X == other.X && Y == other.Y;
}