using System;
using System.Linq;
using Gatitos;

internal static class InteractionContracts
{
    public static void Run(Action<bool, string> check)
    {
        for (int coat = 0; coat < 8; coat++)
        {
            var cat = new[] { new CatMotion(0, 1, 1920, coat) };
            var behavior = new CatBehavior(37);
            bool knead = false, settle = false;
            for (int frame = 0; frame < 30000 && !(knead && settle); frame++)
            {
                behavior.Advance(.1, cat, 1920, 1032, false);
                knead |= cat[0].Pose.StartsWith("knead");
                settle |= cat[0].Pose.StartsWith("settle");
            }
            check(knead && settle, $"Coat {coat} must autonomously knead and settle.");
        }
        foreach (string action in new[] { "rub", "cuddle", "spar" })
        {
            var cats = Enumerable.Range(0, 4).Select(i => new CatMotion(i, 4, 1920)).ToArray();
            var behavior = new CatBehavior(7);
            behavior.Demo(cats, action);
            var preparation = new System.Collections.Generic.HashSet<string>();
            bool contact = false;
            for (int frame = 0; frame < 1000 && !contact; frame++)
            {
                behavior.Advance(.1, cats, 1920, 1032, false);
                if (action == "cuddle" && cats[0].Pose.StartsWith("settle"))
                {
                    preparation.Add(cats[0].Pose);
                    check(cats[0].Pose == cats[1].Pose, "Both partners prepare for sleep together.");
                }
                contact = action == "cuddle"
                    ? cats[0].Pose == "sleep" && cats[1].Pose == "sleep"
                    : cats[0].Pose.StartsWith(action) && cats[1].Pose.StartsWith(action);
            }
            check(contact && cats[0].Coat != cats[1].Coat, $"Mixed coats must perform {action} together.");
            if (action == "cuddle")
            {
                for (int frame = 0; frame < 4; frame++) check(preparation.Contains("settle" + frame), "Shared sleep requires the full preparation sequence.");
                check(behavior.Inspect(0).remaining >= 60 - .1, "Preparation does not consume the shared siesta.");
            }
        }
        var mixed = Enumerable.Range(0, 4).Select(i => new CatMotion(i, 4, 1920)).ToArray();
        var awake = new CatBehavior(19);
        for (int i = 0; i < 3; i++) awake.DemoAt(mixed, i, "sleep");
        awake.DemoAt(mixed, 3, "settle");
        check(awake.Inspect(3).action is "sit" or "groom", "The last mixed-coat cat cannot begin settling.");
        bool choseOther = false;
        for (int seed = 0; seed < 20 && !choseOther; seed++)
        {
            var cat = new[] { new CatMotion(0, 1, 1920, 7) };
            var behavior = new CatBehavior(seed);
            behavior.Demo(cat, "knead");
            for (int frame = 0; frame < 2000 && behavior.Inspect(0).action == "knead"; frame++)
                behavior.Advance(.1, cat, 1920, 1032, false);
            choseOther = behavior.Inspect(0).action is not ("knead" or "settle" or "sleep");
        }
        check(choseOther, "A cat may choose another activity after kneading.");
        bool pair = false;
        for (int seed = 0; seed < 30 && !pair; seed++)
        {
            var cats = new[] { new CatMotion(0, 2, 1920, 3), new CatMotion(1, 2, 1920, 7) };
            var behavior = new CatBehavior(seed);
            for (int frame = 0; frame < 12000 && !pair; frame++)
            {
                behavior.Advance(.1, cats, 1920, 1032, false);
                pair = behavior.Inspect(0).partner == 1;
            }
        }
        check(pair, "Two non-orange coats must autonomously form a pair.");
    }
}
