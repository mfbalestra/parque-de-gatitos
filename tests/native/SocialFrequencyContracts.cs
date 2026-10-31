using System;
using System.Collections.Generic;
using System.Linq;
using Gatitos;

internal static class SocialFrequencyContracts
{
    public static void Run(Action<bool, string> check)
    {
        var counts = new Dictionary<string, int> { ["rub"] = 0, ["cuddle"] = 0, ["spar"] = 0 };
        for (int seed = 0; seed < 300; seed++)
        {
            var cats = Enumerable.Range(0, 4).Select(i => new CatMotion(i, 4, 1920)).ToArray();
            var behavior = new CatBehavior(seed);
            for (int frame = 0; frame < 10000; frame++)
            {
                behavior.Advance(.1, cats, 1920, 1032, false);
                var pair = Enumerable.Range(0, cats.Length)
                    .Select(behavior.Inspect).FirstOrDefault(state => state.partner >= 0);
                if (pair.partner < 0 || pair.action == null) continue;
                counts[pair.action]++;
                break;
            }
        }
        check(counts["rub"] > counts["cuddle"] * 2 && counts["rub"] > counts["spar"] * 2,
            "Affectionate rubbing must be more frequent than paired sleep and play.");
        check(counts["cuddle"] > 0 && counts["spar"] > 0,
            "Paired sleep and play must remain available.");
    }
}
