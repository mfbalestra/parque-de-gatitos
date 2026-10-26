using System;
using System.IO;
using Gatitos;

internal static class SettingsContracts
{
    public static void Run(Action<bool, string> check)
    {
        string directory = Path.Combine(Path.GetTempPath(), "gatitos-preferences-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "settings.json");
        try
        {
            check(ParkSettings.Load(path) == new ParkSettings(30, 5, 4, 30), "Missing settings must use 4 cats and 30 FPS.");
            File.WriteAllText(path, "{\"CompanionMinutes\":73,\"StayMinutes\":12}");
            check(ParkSettings.Load(path) == new ParkSettings(73, 12, 4, 30), "Old settings must retain both durations and supply new defaults.");
            var saved = new ParkSettings(81, 14, 8, 60);
            saved.Save(path);
            check(ParkSettings.Load(path) == saved, "A save must persist all preferences together.");
            foreach (var invalid in new[] { saved with { Cats = 2 }, saved with { Fps = 45 }, saved with { StayMinutes = 0 }, saved with { CompanionMinutes = 241 } })
            {
                bool rejected = false;
                try { invalid.Save(path); } catch (ArgumentOutOfRangeException) { rejected = true; }
                check(rejected && ParkSettings.Load(path) == saved, "An invalid save must preserve the previously persisted record.");
            }
            bool failed = false;
            using (new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                try { new ParkSettings(10, 2, 1, 30).Save(path); }
                catch (IOException) { failed = true; }
                catch (UnauthorizedAccessException) { failed = true; }
            }
            check(failed && ParkSettings.Load(path) == saved, "A blocked replacement must retain the complete previous settings.");
            check(Directory.GetFiles(directory).Length == 1, "A failed save must clean its temporary file.");
        }
        finally
        {
            foreach (string file in Directory.GetFiles(directory)) File.Delete(file);
            Directory.Delete(directory);
        }

        foreach (int target in new[] { 30, 60 })
        foreach (double refresh in new[] { 24.0, 59.94, 60.0, 75.0, 120.0, 144.0 })
        {
            var pacer = new FramePacer(target); int rendered = 0;
            const int seconds = 300;
            for (int frame = 0; frame < seconds * refresh; frame++)
            {
                double timestamp = Math.Round(frame / refresh, 7);
                if (pacer.ShouldRender(timestamp)) rendered++;
                check(!pacer.ShouldRender(timestamp), "A repeated compositor timestamp must not produce a second drawing.");
            }
            check(Math.Abs(rendered - seconds * Math.Min(refresh, target)) <= 1.1,
                $"Rendering must not drift at {target} FPS on a {refresh} Hz display: {rendered} drawings.");
        }
        var delayed = new FramePacer(60);
        check(delayed.ShouldRender(0) && delayed.ShouldRender(10) && !delayed.ShouldRender(10.001), "A delayed frame must skip missed drawings without a catch-up burst.");
        delayed.Configure(30);
        check(delayed.ShouldRender(10.01) && !delayed.ShouldRender(10.02) && delayed.ShouldRender(10.044), "Changing the target must apply immediately at the new cadence.");

        var cats = new[] { new CatMotion(0, 1, 1920) };
        var cycle = new ParkCycle(new ParkSettings(60, 5, 1, 30));
        for (int i = 0; i < 600; i++) cycle.Advance(.1, cats, 1920, 1032);
        double elapsed = cycle.Elapsed, sceneTime = cycle.SceneTime, x = cats[0].X;
        var original = cats[0];
        cycle.Configure(new ParkSettings(81, 14, 1, 60));
        check(cycle.Elapsed == elapsed && cycle.SceneTime == sceneTime && cats[0].X == x && ReferenceEquals(cats[0], original),
            "Changing preferences must not restart the cycle or move an existing cat.");
    }
}
