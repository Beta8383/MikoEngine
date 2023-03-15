namespace MikoEngine;

static class AllocSpanExtension
{
    internal static Span<T> AsSpan<T>(this AllocSpan<T> source) where T : unmanaged
        => source;
}