using System;

namespace Gatitos;

internal static class WalkGait
{
    public const int FrameCount = 8;
    private static readonly double[] Sizes = { 1, .96, 1.04, .98, 1.05, .95, 1.02, 1 };
    public static double Size(int coat) => Sizes[coat];
    public static double StrideLength(int coat) => 28.8 * Size(coat);
    public static int Frame(double distance, int coat) => (int)Math.Floor((distance / StrideLength(coat) % 1) * FrameCount + 1e-9) % FrameCount;
}
