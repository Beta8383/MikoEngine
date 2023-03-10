using System.Runtime.InteropServices;

namespace MikoEngine;

unsafe class AllocSpan<T> where T : unmanaged
{
    T* _reference;
    int _length;
    public int Length => _length;

    public AllocSpan(int length)
    {
        if (length <= 0)
            throw new ArgumentException();
        _length = length;
        Allocate();
    }

    ~AllocSpan() => Free();

    public ref T this[int i]
    {
        get => ref GetElement(i);
    }

    ref T GetElement(int i)
    {
        if (i < 0 || i >= _length)
            throw new IndexOutOfRangeException();
        return ref *(_reference + i);
    }

    bool Allocate()
    {
        try
        {
            _reference = (T*)Marshal.AllocHGlobal(_length * Marshal.SizeOf<T>());
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Free() =>
        Marshal.FreeHGlobal((nint)_reference);

    public Span<T> Slice(int start, int length)
    {
        if (start + length > _length)
            throw new IndexOutOfRangeException();
        if (start < 0 || length <= 0)
            throw new ArgumentException();

        return new Span<T>(_reference + start, length);
    }

    public static implicit operator Span<T>(AllocSpan<T> d) => new(d._reference, d._length);
}