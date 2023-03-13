using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MikoEngine;

unsafe class AllocSpan<T> : IDisposable where T : unmanaged
{
    T* _reference;
    public readonly int Length;

    /// <summary>
    /// 创建非托管内存数组
    /// 多线程不安全😨，谨慎使用
    /// </summary>
    /// <param name="length">数组长度</param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="OutOfMemoryException"></exception>
    public AllocSpan(int length)
    {
        if (length <= 0)
            throw new ArgumentException();

        Length = length;
        _reference = (T*)Marshal.AllocHGlobal(Length * Marshal.SizeOf<T>());
    }

    ~AllocSpan() => Free();

    /// <summary>
    /// 释放非托管内存
    /// </summary>
    public void Dispose()
    {
        Free();
        GC.SuppressFinalize(this);
    }

    void Free()
    {
        Marshal.FreeHGlobal((nint)_reference);
        _reference = null;
        Console.WriteLine(" Free");
    }

    public ref T this[int i]
    {
        get => ref GetElement(i);
    }

    ref T GetElement(int i)
    {
#if DEBUG
        if (_reference is null)
            throw new Exception("");

        if (i < 0 || i >= Length)
            throw new IndexOutOfRangeException($"Index({i}) is out of AllocSpan.");
#endif
        return ref *(_reference + i);
    }

    public Span<T> Slice(int start, int length)
    {
#if DEBUG
        if (start < 0 || length <= 0)
            throw new ArgumentException();
        
        if (start + length > Length)
            throw new IndexOutOfRangeException();
#endif

        return new Span<T>(_reference + start, length);
    }

    public static implicit operator Span<T>(AllocSpan<T> d) => new(d._reference, d.Length);
    public static implicit operator ReadOnlySpan<T>(AllocSpan<T> d) => new(d._reference, d.Length);
}