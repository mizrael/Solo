namespace Solo.Tests;

internal static class AllocationMeter
{
    private const int WarmupFrames = 10;
    private const int MeasuredFrames = 100;

    /// <summary>Bytes allocated on this thread across <see cref="MeasuredFrames"/> warm frames.</summary>
    public static long MeasureFrames(Action frame)
    {
        for (int i = 0; i < WarmupFrames; i++)
            frame();

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < MeasuredFrames; i++)
            frame();
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }
}
