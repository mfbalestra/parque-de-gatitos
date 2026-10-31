using System;
using System.Linq;
using Gatitos;

internal static class BehaviorContracts
{
    public static void Run(Action<bool, string> check)
    {
        var groupRules = new CatBehavior(19);
        foreach (int energy in Enumerable.Range(0, 11))
        foreach (int quiet in Enumerable.Range(0, 20))
        foreach (string action in new[] { "sleep", "sit", "wander", "groom", "knead", "social", "environment" })
        {
            double a = groupRules.GroupFactor(action, quiet / 20.0, energy / 10.0);
            double b = groupRules.GroupFactor(action, (quiet + 1) / 20.0, energy / 10.0);
            check(a > 0 && b > 0, "Group balancing cannot forbid activities or impose a fixed choice.");
            check(action is "sleep" or "sit" ? b <= a + 1e-10 : b >= a - 1e-10,
                "More quiet peers must favor activity over an additional long rest.");
            if (quiet <= 10) check(a == 1, "An active group needs no correction.");
        }
        check(groupRules.GroupFactor("wander", 1, 1) > groupRules.GroupFactor("wander", 1, 0),
            "Fatigue must reduce the encouragement to move.");
        foreach (bool park in new[] { false, true })
        foreach (int count in new[] { 1, 4, 8 })
        foreach (int seed in new[] { 2, 7, 19 })
        {
            var group = Enumerable.Range(0, count).Select(i => new CatMotion(i, count, 1920, 0)).ToArray();
            double landingX = 237.0 / 256 * 1920, landingDepth = park ? 1032 - 7 - 118.0 / 144 * 1032 : 0;
            foreach (var cat in group) while (!cat.MoveTo(landingX, landingDepth, .5, 1920)) { }
            var behavior = new CatBehavior(seed);
            for (int i = 0; i < count; i++) behavior.DemoAt(group, i, "sleep");
            var durations = Enumerable.Range(0, count).Select(i => behavior.Inspect(i).remaining).ToArray();
            var arrived = new bool[count];
            for (int frame = 0; frame < 2000 && arrived.Any(x => !x); frame++)
            {
                behavior.Advance(.1, group, 1920, 1032, park);
                for (int i = 0; i < count; i++)
                {
                    if (!behavior.InspectRoute(i).restReady)
                        check(behavior.Inspect(i).remaining == durations[i], "A waiting or approaching rest keeps its full duration.");
                    else arrived[i] = true;
                    for (int j = i + 1; j < count; j++)
                    {
                        var a = behavior.InspectRest(i); var b = behavior.InspectRest(j);
                        if (a.reserved && b.reserved)
                            check(Math.Abs(a.x - b.x) >= 104 || Math.Abs(a.depth - b.depth) >= 56,
                                "Concurrent durable rests must reserve separate footprints.");
                        if (behavior.InspectRoute(i).restReady && behavior.InspectRoute(j).restReady)
                            check(Math.Abs(group[i].X - group[j].X) >= 104 || Math.Abs(group[i].Depth - group[j].Depth) >= 56,
                                "Resting cats must not remain overlapped.");
                    }
                }
            }
            check(arrived.All(x => x), "Every cat in the clustered fixture must reach its rest.");
        }
        var trio = Enumerable.Range(0, 3).Select(i => new CatMotion(i, 3, 1920, 0)).ToArray();
        double[] trioX = { 700, 1000, 850 };
        for (int i = 0; i < trio.Length; i++) while (!trio[i].MoveTo(trioX[i], 0, .5, 1920)) { }
        var trioBehavior = new CatBehavior(5); trioBehavior.Demo(trio, "cuddle");
        trioBehavior.DemoAt(trio, 2, "sleep");
        trioBehavior.Advance(.1, trio, 1920, 1032, false);
        var third = trioBehavior.InspectRest(2);
        check(third.reserved, "A third cat must find a rest outside the pair.");
        for (int i = 0; i < 2; i++)
        {
            var partner = trioBehavior.InspectRest(i);
            check(Math.Abs(third.x - partner.x) >= 104 || Math.Abs(third.depth - partner.depth) >= 56,
                "A pair's reserved footprints must remain free of a third resting cat.");
        }
        var fullMats = Enumerable.Range(0, 4).Select(i => new CatMotion(i, 4, 1920, 0)).ToArray();
        var positions = new[] { (237.0, 118.0), (27.0, 120.0), (96.0, 137.0), (211.0, 132.0) };
        for (int i = 0; i < fullMats.Length; i++)
            while (!fullMats[i].MoveTo(positions[i].Item1 / 256 * 1920, 1032 - 7 - positions[i].Item2 / 144 * 1032, .5, 1920)) { }
        var fullMatBehavior = new CatBehavior(3);
        fullMatBehavior.DemoAt(fullMats, 0, "knead");
        for (int i = 1; i < fullMats.Length; i++) fullMatBehavior.DemoAt(fullMats, i, "sleep");
        for (int frame = 0; frame < 500 && !fullMatBehavior.InspectRoute(0).restReady; frame++)
        {
            fullMatBehavior.Advance(.1, fullMats, 1920, 1032, true);
            check(fullMatBehavior.Inspect(0).action == "sit" && !fullMats[0].Pose.StartsWith("knead"),
                "With all mats occupied, the cat must find a sitting place instead of kneading off a mat.");
        }
        check(fullMatBehavior.InspectRoute(0).restReady &&
            (Math.Abs(fullMats[0].X - 237.0 / 256 * 1920) >= 20.0 / 256 * 1920 ||
             Math.Abs(fullMats[0].Depth - (1032 - 7 - 118.0 / 144 * 1032)) >= 13.0 / 144 * 1032),
            "With all mats occupied, the cat must leave the slide landing before sitting.");
        var cluster = Enumerable.Range(0, 4).Select(i => new CatMotion(i, 4, 1920, 0)).ToArray();
        foreach (var cat in cluster) while (!cat.MoveTo(1000, 80, .5, 1920)) { }
        var clustered = new CatBehavior(8); clustered.Demo(cluster, "sleep");
        double restDuration = clustered.Inspect(0).remaining;
        clustered.Advance(.1, cluster, 1920, 1032, true);
        check(cluster[0].Pose.StartsWith("walk") && clustered.Inspect(0).remaining == restDuration,
            "A cat in a landing cluster must walk to a free rest destination before its timer starts.");
        for (int i = 0; i < cluster.Length; i++)
        for (int j = i + 1; j < cluster.Length; j++)
        {
            var a = clustered.InspectRest(i); var b = clustered.InspectRest(j);
            if (a.reserved && b.reserved)
                check(Math.Abs(a.x - b.x) >= 104 || Math.Abs(a.depth - b.depth) >= 56,
                    "Simultaneous rest destinations must reserve separate footprints.");
        }
        for (int frame = 0; frame < 500 && !clustered.InspectRoute(0).restReady; frame++)
        {
            check(clustered.Inspect(0).remaining == restDuration, "Approach must not shorten rest.");
            clustered.Advance(.1, cluster, 1920, 1032, true);
        }
        check(clustered.InspectRoute(0).restReady && cluster.Skip(1).All(other =>
            Math.Abs(cluster[0].X - other.X) >= 104 || Math.Abs(cluster[0].Depth - other.Depth) >= 56),
            "A resting cat must reach a destination clear of the other cats.");
        bool sawSlide = false, sawExit = false, sawClear = false;
        for (int seed = 1; seed <= 8 && !sawClear; seed++)
        {
            var gymCat = new[] { new CatMotion(0, 1, 1920, 0) };
            var gymBehavior = new CatBehavior(seed);
            for (int frame = 0; frame < 30000 && !sawClear; frame++)
            {
                gymBehavior.Advance(.1, gymCat, 1920, 1032, true);
                var route = gymBehavior.InspectRoute(0);
                if (route.gym && route.step == 6 && gymCat[0].X > 1505)
                {
                    sawSlide = true;
                    check(gymCat[0].Height == 0 && gymCat[0].Pose == "sit", "The slide must support a seated cat without a hop.");
                }
                if (route.exit)
                {
                    sawExit = true;
                    check(gymBehavior.Inspect(0).action == "wander", "A cat must walk clear of the gym exit before resting.");
                }
                if (sawExit && !route.exit && !route.gym) sawClear = true;
            }
        }
        check(sawSlide && sawExit && sawClear, "A seeded cat must traverse the slide and walk clear of its exit.");
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
