using System.Runtime.CompilerServices;

namespace MikoEngine;

public static class MKMath
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Min<T>(params T[] source) where T : IComparable<T> => source.Min();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Max<T>(params T[] source) where T : IComparable<T> => source.Max();

    internal static bool ComputeBarycentric(in MKVector4 p1, in MKVector4 p2, in MKVector4 p3, float x, float y, out float i, out float j, out float k)
    {
        i = j = k = 0f;

        float a = p1.Y - p3.Y;
        float b = p1.X - p3.X;
        float c = (p2.Y - p3.Y) * b - (p2.X - p3.X) * a;
        j = ((y - p3.Y) * b - (x - p3.X) * a) / c;
        if (j < 0)
            return false;

        float a1 = p3.Y - p2.Y;
        float b1 = p3.X - p2.X;
        float c1 = (p1.Y - p2.Y) * b1 - (p1.X - p2.X) * a1;
        i = ((y - p2.Y) * b1 - (x - p2.X) * a1) / c1;
        if (i < 0)
            return false;

        k = 1 - i - j;
        return k > 0;
    }
}