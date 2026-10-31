using System;
using System.Linq;
using Gatitos;

internal static class AwakeContracts
{
    private static CatMotion[] Cats(int count) => Enumerable.Range(0, count).Select(i => new CatMotion(i, count, 1920, 0)).ToArray();
    public static void Run(Action<bool, string> check)
    {
        for (int count = 1; count <= 8; count++)
        for (int asleep = 0; asleep < count; asleep++)
        {
            var cats = Cats(count); var behavior = new CatBehavior(19);
            behavior.DemoAt(cats, count - 1, "sit");
            for (int i = 0; i < asleep; i++) behavior.DemoAt(cats, i, "sleep");
            for (int joining = 1; joining <= Math.Min(2, count - asleep); joining++)
                check(behavior.CanSleep(Enumerable.Range(asleep, joining).ToArray()) == (count == 1 || asleep + joining < count),
                    "Pending solo and paired sleeps must leave another cat awake, except a solitary cat.");
        }
        var group = Cats(4); var settling = new CatBehavior(19);
        for (int i = 0; i < 3; i++) settling.DemoAt(group, i, "sleep");
        settling.DemoAt(group, 3, "settle");
        check(settling.Inspect(3).action is "sit" or "groom", "The last cat cannot begin settling.");
        for (int frame = 0; frame < 500 && Enumerable.Range(0, 3).Any(i => settling.Inspect(i).action == "settle"); frame++)
            settling.Advance(.05, group, 1920, 1032, false);
        for (int i = 0; i < 3; i++) check(settling.Inspect(i).action == "sleep" && settling.Inspect(i).remaining > 30,
            "Admitted settling must lead to a full siesta.");
        var pairs = new CatBehavior(2); group = Cats(4);
        pairs.Demo(group, "cuddle");
        check(!pairs.CanSleep(2, 3), "An approaching pair already reserves two sleeping cats.");
        pairs.DemoAt(group, 2, "sleep"); pairs.DemoAt(group, 3, "sleep");
        check(pairs.Inspect(3).action is "sit" or "groom", "Direct sleep cannot consume the last awake cat.");
        var blockedPair = new CatBehavior(2); group = Cats(4);
        blockedPair.DemoAt(group, 2, "sleep"); blockedPair.DemoAt(group, 3, "sleep");
        blockedPair.Demo(group, "cuddle");
        check(blockedPair.Inspect(0).action == "rub" && blockedPair.Inspect(1).action == "rub",
            "The last two awake cats may interact without sleeping together.");
        var kneading = new CatBehavior(19); group = Cats(4);
        for (int i = 0; i < 3; i++) kneading.DemoAt(group, i, "sleep");
        kneading.DemoAt(group, 3, "knead");
        for (int frame = 0; frame < 800 && kneading.Inspect(3).action == "knead"; frame++)
            kneading.Advance(.05, group, 1920, 1032, false);
        check(kneading.Inspect(3).action is not ("settle" or "sleep"),
            "Kneading does not force sleep and the last cat remains awake.");
        foreach (int count in new[] { 2, 4, 8 })
        foreach (int seed in new[] { 2, 19, 71 })
        foreach (bool park in new[] { false, true })
        {
            var cats = Cats(count); var behavior = new CatBehavior(seed);
            for (int frame = 0; frame < 12000; frame++)
            {
                behavior.Advance(.1, cats, 1920, 1032, park);
                check(Enumerable.Range(0, count).Any(i => behavior.Inspect(i).action is not ("sleep" or "settle" or "cuddle")),
                    "Autonomy must never commit the whole group to sleep.");
            }
        }
    }
}
