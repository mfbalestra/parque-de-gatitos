using System;
using System.Linq;
using Gatitos;

internal static class BehaviorContracts
{
    public static void Run(Action<bool, string> check)
    {
        foreach (int width in new[] { 320, 800, 1920 })
        {
            var edgeCats = new[] { new CatMotion(0, 2, width, 0), new CatMotion(1, 2, width, 0) };
            foreach (var cat in edgeCats) for (int step = 0; step < 40; step++) cat.MoveTo(70, 0, .5, width);
            var edgeBehavior = new CatBehavior(12); edgeBehavior.Demo(edgeCats, "rub");
            bool contact = false;
            for (int step = 0; step < 200 && !contact; step++)
            {
                edgeBehavior.Advance(.05, edgeCats, width, 1032, false);
                contact = edgeCats[0].Pose.StartsWith("rub");
            }
            check(contact && edgeCats[0].X >= 70 && edgeCats[1].X <= width - 70 &&
                edgeCats[1].X - edgeCats[0].X >= 74.9, "Edge encounters must keep their full contact spacing inside the desktop.");
        }
        var signatures = new System.Collections.Generic.HashSet<string>();
        foreach (int seed in Enumerable.Range(1, 12))
        {
            var solo = new[] { new CatMotion(0, 1, 1920, 0) };
            var behavior = new CatBehavior(seed);
            var sequence = new System.Collections.Generic.List<string>();
            for (int step = 0; step < 6000; step++)
            {
                behavior.Advance(.1, solo, 1920, 1032, false);
                string action = behavior.Inspect(0).action;
                if (sequence.Count == 0 || sequence[^1] != action) sequence.Add(action);
            }
            signatures.Add(string.Join(",", sequence));
        }
        check(signatures.Count > 8, "Independent cats must not follow the same action train across seeds.");
        foreach (int count in new[] { 1, 4, 8 })
        foreach (int seed in Enumerable.Range(1, 12))
        {
            var cats = Enumerable.Range(0, count).Select(i => new CatMotion(i, count, 1920, 0)).ToArray();
            var behavior = new CatBehavior(seed);
            for (int frame = 0; frame < 6000; frame++)
            {
                behavior.Advance(.1, cats, 1920, 1032, count == 8);
                int intense = 0;
                for (int i = 0; i < count; i++)
                {
                    var state = behavior.Inspect(i);
                    if (state.partner >= 0)
                    {
                        check(state.partner != i && behavior.Inspect(state.partner).partner == i, "Pair reservations must be mutual.");
                        if (state.action == "spar") intense++;
                    }
                }
                check(intense / 2 <= (count == 8 ? 2 : 1), "Intense pair cap exceeded.");
            }
            behavior.Cancel();
            for (int i = 0; i < count; i++) check(behavior.Inspect(i).partner == -1, "Cancellation must release both cats.");
        }
        foreach (string action in new[] { "sleep", "sit", "groom" })
        {
            var cats = new[] { new CatMotion(0, 1, 1920) };
            var behavior = new CatBehavior(7);
            behavior.Demo(cats, action);
            double minimum = action == "sleep" ? 60 : action == "sit" ? 20 : 15;
            for (double elapsed = 0; elapsed < minimum - .2; elapsed += .1)
            {
                behavior.Advance(.1, cats, 1920, 1032, false);
                check(behavior.Inspect(0).action == action, "Rest ended before its minimum duration.");
            }
        }
        var pair = new[] { new CatMotion(0, 2, 1920, 0), new CatMotion(1, 2, 1920, 0) };
        var demo = new CatBehavior(4); demo.Demo(pair, "spar");
        bool sawContact = false;
        for (int frame = 0; frame < 2000 && demo.Inspect(0).partner >= 0; frame++)
        {
            demo.Advance(.1, pair, 1920, 1032, false);
            if (pair[0].Pose.StartsWith("spar"))
            {
                sawContact = true;
                check(pair[0].Direction == 1 && pair[1].Direction == -1, "Partners must face each other at contact.");
            }
        }
        check(sawContact, "The spar fixture must reach contact.");
        check(demo.Inspect(0).partner < 0 && demo.Inspect(1).partner < 0, "Completed pair must release both.");
        check(demo.Inspect(0).cooldown >= 29.8 && demo.Inspect(1).cooldown >= 29.8, "Both play partners must recover.");
        for (int frame = 0; frame < 295; frame++)
        {
            demo.Advance(.1, pair, 1920, 1032, false);
            check(demo.Inspect(0).action == "sit" && demo.Inspect(1).action == "sit", "Both cats must rest for the play cooldown.");
        }
        var cats30 = new[] { new CatMotion(0, 1, 1920) };
        var cats60 = new[] { new CatMotion(0, 1, 1920) };
        var at30 = new CatBehavior(15); var at60 = new CatBehavior(15);
        at30.Demo(cats30, "sleep"); at60.Demo(cats60, "sleep");
        for (int i = 0; i < 600; i++) at30.Advance(1.0 / 30, cats30, 1920, 1032, false);
        for (int i = 0; i < 1200; i++) at60.Advance(1.0 / 60, cats60, 1920, 1032, false);
        check(Math.Abs(at30.Inspect(0).remaining - at60.Inspect(0).remaining) < 1e-8, "Rest duration must not depend on frame rate.");
        demo.Demo(pair, "rub");
        demo.Advance(.1, pair, 1920, 1032, false);
        double firstX = pair[0].X, secondX = pair[1].X;
        demo.Cancel();
        check(pair[0].X == firstX && pair[1].X == secondX && demo.Inspect(0).partner < 0 && demo.Inspect(1).partner < 0,
            "Cancellation must release the pair in place.");
        var settling = new CatBehavior(9);
        var sleeper = new[] { new CatMotion(0, 1, 1920, 0) };
        settling.Demo(sleeper, "settle");
        int lastFrame = -1;
        for (int i = 0; i < 200 && settling.Inspect(0).action == "settle"; i++)
        {
            settling.Advance(.05, sleeper, 1920, 1032, false);
            if (sleeper[0].Pose.StartsWith("settle"))
            {
                int frame = sleeper[0].Pose[6] - '0';
                check(frame >= lastFrame, "Settling frames must progress without looping.");
                lastFrame = frame;
            }
        }
        check(lastFrame == 3 && settling.Inspect(0).action == "sleep", "Settling must lead to sleep.");
        var matCat = new[] { new CatMotion(0, 1, 1920, 0) };
        var matBehavior = new CatBehavior(3); matBehavior.Demo(matCat, "knead");
        double startX = matCat[0].X, duration = matBehavior.Inspect(0).remaining;
        matBehavior.Advance(.1, matCat, 1920, 1032, true);
        check(matCat[0].X != startX && !matCat[0].Pose.StartsWith("knead") &&
            matBehavior.Inspect(0).remaining == duration, "The cat must walk before the kneading timer starts.");
        for (int i = 0; i < 1000 && !matCat[0].Pose.StartsWith("knead"); i++)
            matBehavior.Advance(.1, matCat, 1920, 1032, true);
        check(matCat[0].Pose.StartsWith("knead") && Math.Abs(matCat[0].X - 96.0 / 256 * 1920) < .01 &&
            Math.Abs(matCat[0].Depth - (1032 - 7 - 137.0 / 144 * 1032)) < .01,
            "Kneading must start on the nearest available mat.");
    }
}
