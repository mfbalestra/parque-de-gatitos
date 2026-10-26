using System;
using System.Collections.Generic;
using Gatitos;

internal static class WalkContracts
{
    public static void Run(Action<bool, string> check)
    {
        for (int coat = 0; coat < 8; coat++)
        {
            var frames = new HashSet<int>();
            double length = WalkGait.StrideLength(coat);
            for (int sample = 0; sample < 800; sample++)
            {
                double distance = length * sample / 800;
                frames.Add(WalkGait.Frame(distance, coat));
                check(WalkGait.Frame(distance, coat) == WalkGait.Frame(distance + 20 * length, coat), "Walk frames must repeat once per physical stride.");
                double phase = sample / 800.0;
                var paw = WalkGait.Paw(phase);
                check(paw.Lift >= 0 && paw.Lift <= 8, "Paws must stay at or above their contact plane.");
                if (paw.Planted)
                    check(Math.Abs(distance + paw.X * WalkGait.Size(coat) - WalkGait.Reach * WalkGait.Size(coat)) < 1e-8, "A supporting paw must stay fixed while the body passes over it.");
            }
            check(frames.Count == 8, "Every coat must use all eight walk frames.");
            var at30 = new CatMotion(coat, 8, 1920);
            var at60 = new CatMotion(coat, 8, 1920);
            for (int i = 0; i < 180; i++) at30.Advance(1.0 / 30, 1920);
            for (int i = 0; i < 360; i++) at60.Advance(1.0 / 60, 1920);
            check(Math.Abs(at30.X - at60.X) < 1e-8 && at30.Pose == at60.Pose, $"Frame rate must not alter desktop speed or walk phase: coat {coat}, x30={at30.X:R}, x60={at60.X:R}, poses={at30.Pose}/{at60.Pose}.");
            var route30 = new CatMotion(coat, 8, 1920);
            var route60 = new CatMotion(coat, 8, 1920);
            for (int i = 0; i < 30; i++) route30.MoveTo(1100, 70, 1.0 / 30, 1920);
            for (int i = 0; i < 60; i++) route60.MoveTo(1100, 70, 1.0 / 60, 1920);
            check(Math.Abs(route30.X - route60.X) < 1e-8 && Math.Abs(route30.Depth - route60.Depth) < 1e-8 && route30.Pose == route60.Pose, "Frame rate must not alter route speed or walk phase.");
            double before = route30.WalkDistance;
            route30.Rest(1, 0);
            check(route30.WalkDistance == before, "A resting cat must not advance its walk phase.");
        }
    }
}
