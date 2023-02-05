using System.Runtime.CompilerServices;

namespace MikoEngine;

public struct MKMatrix4x4 : IEquatable<MKMatrix4x4>
{
    public static MKMatrix4x4 Identity
    {
        get => new(1f, 0f, 0f, 0f,
            0f, 1f, 0f, 0f,
            0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f);
    }

    public float M11, M12, M13, M14;
    public float M21, M22, M23, M24;
    public float M31, M32, M33, M34;
    public float M41, M42, M43, M44;

    public MKMatrix4x4() { }
    public MKMatrix4x4(float m11, float m12, float m13, float m14,
                       float m21, float m22, float m23, float m24,
                       float m31, float m32, float m33, float m34,
                       float m41, float m42, float m43, float m44)
    {

        M11 = m11; M12 = m12; M13 = m13; M14 = m14;
        M21 = m21; M22 = m22; M23 = m23; M24 = m24;
        M31 = m31; M32 = m32; M33 = m33; M34 = m34;
        M41 = m41; M42 = m42; M43 = m43; M44 = m44;
    }

    public float this[int i, int j]
    {
        get => GetElement(ref this, i, j);
        set => SetElement(ref this, i, j, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float GetElement(ref MKMatrix4x4 matrix, int i, int j)
    {
        if (i is < 1 or > 4 || j is < 1 or > 4)
            throw new Exception();
        return Unsafe.Add(ref Unsafe.As<MKMatrix4x4, float>(ref matrix), (i - 1) * 4 + j - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetElement(ref MKMatrix4x4 matrix, int i, int j, float value)
    {
        if (i is < 1 or > 4 || j is < 1 or > 4)
            throw new Exception();
        Unsafe.Add(ref Unsafe.As<MKMatrix4x4, float>(ref matrix), (i - 1) * 4 + j - 1) = value;
    }

    public MKVector3 Translation
    {
        get => new(this[1, 4], this[2, 4], this[3, 4]);
        set
        {
            M14 = value.X;
            M24 = value.Y;
            M34 = value.Z;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKMatrix4x4 CreateTranslation(float x, float y, float z) =>
        CreateTranslation(new MKVector3(x, y, z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MKMatrix4x4 CreateTranslation(MKVector3 vec)
    {
        var result = MKMatrix4x4.Identity;
        result.Translation = vec;
        return result;
    }

    public static MKMatrix4x4 operator +(MKMatrix4x4 m1, MKMatrix4x4 m2) =>
        new MKMatrix4x4(
            m1[1, 1] + m2[1, 1], m1[1, 2] + m2[1, 2], m1[1, 3] + m2[1, 3], m1[1, 4] + m2[1, 4],
            m1[2, 1] + m2[2, 1], m1[2, 2] + m2[2, 2], m1[2, 3] + m2[2, 3], m1[2, 4] + m2[2, 4],
            m1[3, 1] + m2[3, 1], m1[3, 2] + m2[3, 2], m1[3, 3] + m2[3, 3], m1[3, 4] + m2[3, 4],
            m1[4, 1] + m2[4, 1], m1[4, 2] + m2[4, 2], m1[4, 3] + m2[4, 3], m1[4, 4] + m2[4, 4]
        );

    public static MKMatrix4x4 operator -(MKMatrix4x4 m1, MKMatrix4x4 m2) =>
        new MKMatrix4x4(
            m1[1, 1] - m2[1, 1], m1[1, 2] - m2[1, 2], m1[1, 3] - m2[1, 3], m1[1, 4] - m2[1, 4],
            m1[2, 1] - m2[2, 1], m1[2, 2] - m2[2, 2], m1[2, 3] - m2[2, 3], m1[2, 4] - m2[2, 4],
            m1[3, 1] - m2[3, 1], m1[3, 2] - m2[3, 2], m1[3, 3] - m2[3, 3], m1[3, 4] - m2[3, 4],
            m1[4, 1] - m2[4, 1], m1[4, 2] - m2[4, 2], m1[4, 3] - m2[4, 3], m1[4, 4] - m2[4, 4]
        );

    public static MKMatrix4x4 operator *(MKMatrix4x4 m, float scale) =>
        new MKMatrix4x4(
            m[1, 1] * scale, m[1, 2] * scale, m[1, 3] * scale, m[1, 4] * scale,
            m[2, 1] * scale, m[2, 2] * scale, m[2, 3] * scale, m[2, 4] * scale,
            m[3, 1] * scale, m[3, 2] * scale, m[3, 3] * scale, m[3, 4] * scale,
            m[4, 1] * scale, m[4, 2] * scale, m[4, 3] * scale, m[4, 4] * scale
        );

    public static MKMatrix4x4 operator *(float scale, MKMatrix4x4 m) => m * scale;

    public static MKMatrix4x4 operator *(MKMatrix4x4 m1, MKMatrix4x4 m2)
    {
        MKMatrix4x4 result = new();

        result[1, 1] = m2[1, 1] * m1[1, 1] + m2[2, 1] * m1[1, 2] + m2[3, 1] * m1[1, 3] + m2[4, 1] * m1[1, 4];
        result[2, 1] = m2[1, 1] * m1[2, 1] + m2[2, 1] * m1[2, 2] + m2[3, 1] * m1[2, 3] + m2[4, 1] * m1[2, 4];
        result[3, 1] = m2[1, 1] * m1[3, 1] + m2[2, 1] * m1[3, 2] + m2[3, 1] * m1[3, 3] + m2[4, 1] * m1[3, 4];
        result[4, 1] = m2[1, 1] * m1[4, 1] + m2[2, 1] * m1[4, 2] + m2[3, 1] * m1[4, 3] + m2[4, 1] * m1[4, 4];

        result[1, 2] = m2[1, 2] * m1[1, 1] + m2[2, 2] * m1[1, 2] + m2[3, 2] * m1[1, 3] + m2[4, 2] * m1[1, 4];
        result[2, 2] = m2[1, 2] * m1[2, 1] + m2[2, 2] * m1[2, 2] + m2[3, 2] * m1[2, 3] + m2[4, 2] * m1[2, 4];
        result[3, 2] = m2[1, 2] * m1[3, 1] + m2[2, 2] * m1[3, 2] + m2[3, 2] * m1[3, 3] + m2[4, 2] * m1[3, 4];
        result[4, 2] = m2[1, 2] * m1[4, 1] + m2[2, 2] * m1[4, 2] + m2[3, 2] * m1[4, 3] + m2[4, 2] * m1[4, 4];

        result[1, 3] = m2[1, 3] * m1[1, 1] + m2[2, 3] * m1[1, 2] + m2[3, 3] * m1[1, 3] + m2[4, 3] * m1[1, 4];
        result[2, 3] = m2[1, 3] * m1[2, 1] + m2[2, 3] * m1[2, 2] + m2[3, 3] * m1[2, 3] + m2[4, 3] * m1[2, 4];
        result[3, 3] = m2[1, 3] * m1[3, 1] + m2[2, 3] * m1[3, 2] + m2[3, 3] * m1[3, 3] + m2[4, 3] * m1[3, 4];
        result[4, 3] = m2[1, 3] * m1[4, 1] + m2[2, 3] * m1[4, 2] + m2[3, 3] * m1[4, 3] + m2[4, 3] * m1[4, 4];

        result[1, 4] = m2[1, 4] * m1[1, 1] + m2[2, 4] * m1[1, 2] + m2[3, 4] * m1[1, 3] + m2[4, 4] * m1[1, 4];
        result[2, 4] = m2[1, 4] * m1[2, 1] + m2[2, 4] * m1[2, 2] + m2[3, 4] * m1[2, 3] + m2[4, 4] * m1[2, 4];
        result[3, 4] = m2[1, 4] * m1[3, 1] + m2[2, 4] * m1[3, 2] + m2[3, 4] * m1[3, 3] + m2[4, 4] * m1[3, 4];
        result[4, 4] = m2[1, 4] * m1[4, 1] + m2[2, 4] * m1[4, 2] + m2[3, 4] * m1[4, 3] + m2[4, 4] * m1[4, 4];
        return result;
    }

    public static MKVector4 operator *(MKMatrix4x4 m, MKVector4 vec) =>
        new MKVector4(
            vec.X * m[1, 1] + vec.Y * m[1, 2] + vec.Z * m[1, 3] + vec.W * m[1, 4],
            vec.X * m[2, 1] + vec.Y * m[2, 2] + vec.Z * m[2, 3] + vec.W * m[2, 4],
            vec.X * m[3, 1] + vec.Y * m[3, 2] + vec.Z * m[3, 3] + vec.W * m[3, 4],
            vec.X * m[4, 1] + vec.Y * m[4, 2] + vec.Z * m[4, 3] + vec.W * m[4, 4]
        );

    public bool Equals(MKMatrix4x4 other) =>
        this[1, 1] == other[1, 1] && this[1, 2] == other[1, 2] && this[1, 3] == other[1, 3] && this[1, 4] == other[1, 4] &&
        this[2, 1] == other[2, 1] && this[2, 2] == other[2, 2] && this[2, 3] == other[2, 3] && this[2, 4] == other[2, 4] &&
        this[3, 1] == other[3, 1] && this[3, 2] == other[3, 2] && this[3, 3] == other[3, 3] && this[3, 4] == other[3, 4] &&
        this[4, 1] == other[4, 1] && this[4, 2] == other[4, 2] && this[4, 3] == other[4, 3] && this[4, 4] == other[4, 4];
}