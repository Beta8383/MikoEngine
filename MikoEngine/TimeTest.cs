using System.Diagnostics;

namespace MikoEngine;

static class TimeTest
{
    internal static void Run(Action task,string name)
    {
        Stopwatch watch = new();
        watch.Start();
        task();
        watch.Stop();
        Console.WriteLine($"{name} Used Time: {watch.ElapsedMilliseconds}");
    }
}