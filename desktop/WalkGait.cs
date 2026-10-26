using System;

namespace Gatitos;

// Eight sampled poses of a four-beat walk. During stance the paw moves backward
// by exactly the distance the body advances; the lifted return uses a smooth arc.
internal static class WalkGait
{
    public const int FrameCount = 8;
    public const double Reach = 9, Support = .625;
    private static readonly double[] Sizes = { 1, .96, 1.04, .98, 1.05, .95, 1.02, 1 };
    public static double Size(int coat) => Sizes[coat];
    public static double StrideLength(int coat) => 2 * Reach / Support * Size(coat);
    public static int Frame(double distance, int coat) => (int)Math.Floor((distance / StrideLength(coat) % 1) * FrameCount + 1e-9) % FrameCount;
    public static (double X, double Lift, bool Planted) Paw(double phase)
    {
        phase -= Math.Floor(phase);
        if (phase < Support) return (Reach - 2 * Reach * phase / Support, 0, true);
        double swing = (phase - Support) / (1 - Support);
        double smooth = swing * swing * (3 - 2 * swing);
        return (-Reach + 2 * Reach * smooth, 8 * Math.Sin(Math.PI * swing), false);
    }
}
