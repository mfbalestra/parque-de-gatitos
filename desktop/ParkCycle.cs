using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Gatitos;

internal sealed record ParkSettings(int CompanionMinutes = 30, int StayMinutes = 5, int Cats = 4, int Fps = 30)
{
    public static bool ValidCompanion(int value) => value is >= 1 and <= 240;
    public static bool ValidStay(int value) => value is >= 1 and <= 60;
    public static bool ValidCount(int value) => value is 1 or 4 or 8;
    public static bool ValidFps(int value) => value is 30 or 60;
    public bool Valid => ValidCompanion(CompanionMinutes) && ValidStay(StayMinutes) && ValidCount(Cats) && ValidFps(Fps);
    public static string PathName => Path.Combine(AppContext.BaseDirectory, "gatitos-settings.json");
    public static ParkSettings Load(string? path = null)
    {
        path ??= PathName;
        if (!File.Exists(path)) return new();
        var value = JsonSerializer.Deserialize<ParkSettings>(File.ReadAllText(path));
        return value?.Valid == true ? value : throw new InvalidDataException("Las preferencias guardadas no son válidas.");
    }
    public void Save(string? path = null)
    {
        if (!Valid) throw new ArgumentOutOfRangeException(nameof(ParkSettings));
        path ??= PathName;
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temporary, path, true);
        }
        finally
        {
            // Cleanup must not hide the original write failure or turn a successful commit into a failure.
            try { if (File.Exists(temporary)) File.Delete(temporary); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}

internal enum ParkPhase { Desktop, Appearing, Entering, Visiting, Leaving, Disappearing }

internal sealed class ParkCycle
{
    public ParkSettings Settings { get; private set; }
    public ParkPhase Phase { get; private set; }
    public double Opacity { get; private set; }
    public double Elapsed { get; private set; }
    public double SceneTime { get; private set; }
    public bool Expanded => Phase != ParkPhase.Desktop;
    public int Visits { get; private set; }
    private bool revisit;
    private double[] home = Array.Empty<double>();
    private readonly CatBehavior behavior = new();
    public void CancelInteractions() => behavior.Cancel();
    public void Demo(CatMotion[] cats, string action) => behavior.Demo(cats, action);

    public ParkCycle(ParkSettings settings)
    {
        if (!settings.Valid) throw new ArgumentOutOfRangeException(nameof(settings));
        Settings = settings;
    }
    public void Configure(ParkSettings settings)
    {
        if (!settings.Valid) throw new ArgumentOutOfRangeException(nameof(settings));
        Settings = settings;
    }
    private void SetPhase(ParkPhase phase) { Phase = phase; Elapsed = 0; }
    public void Visit(CatMotion[] cats, double width)
    {
        if (Phase is ParkPhase.Leaving or ParkPhase.Disappearing) { revisit = true; return; }
        if (Expanded) return;
        behavior.Cancel();
        home = cats.Select(c => c.X / width).ToArray();
        SetPhase(ParkPhase.Appearing); Visits++;
    }
    public void Return()
    {
        behavior.Cancel();
        revisit = false;
        if (Phase is ParkPhase.Appearing or ParkPhase.Entering or ParkPhase.Visiting) SetPhase(ParkPhase.Leaving);
    }
    public string Status => Phase switch
    {
        ParkPhase.Desktop => $"Próxima visita en {Remaining(Settings.CompanionMinutes * 60)}",
        ParkPhase.Appearing => "El parque está apareciendo",
        ParkPhase.Entering => "Los gatos van al parque",
        ParkPhase.Visiting => $"Vuelta al escritorio en {Remaining(Settings.StayMinutes * 60)}",
        ParkPhase.Leaving => "Los gatos vuelven al escritorio",
        _ => "El parque se está despidiendo"
    };
    private string Remaining(double duration)
    {
        var remaining = TimeSpan.FromSeconds(Math.Max(0, Math.Ceiling(duration - Elapsed)));
        return remaining.ToString(remaining.TotalHours >= 1 ? @"hh\:mm\:ss" : @"mm\:ss");
    }

    public void Advance(double dt, CatMotion[] cats, double width, double height)
    {
        if (!double.IsFinite(dt) || dt < 0 || dt > 2) throw new ArgumentOutOfRangeException(nameof(dt));
        Elapsed += dt; SceneTime += dt;
        if (Expanded && home.Length != cats.Length) throw new InvalidOperationException("No se puede cambiar la cantidad durante la visita.");
        switch (Phase)
        {
            case ParkPhase.Desktop:
                behavior.Advance(dt, cats, width, height, false);
                if (Elapsed >= Settings.CompanionMinutes * 60) Visit(cats, width);
                break;
            case ParkPhase.Appearing:
                Opacity = Math.Min(1, Opacity + dt / 2);
                foreach (var cat in cats) cat.FinishJump(dt, width);
                if (Opacity >= 1) SetPhase(ParkPhase.Entering);
                break;
            case ParkPhase.Entering:
                bool arrived = true;
                for (int i = 0; i < cats.Length; i++)
                    arrived &= cats[i].MoveTo(GroundX(i, width), GroundDepth(i, height), dt, width);
                if (arrived) SetPhase(ParkPhase.Visiting);
                break;
            case ParkPhase.Visiting:
                behavior.Advance(dt, cats, width, height, true);
                if (Elapsed >= Settings.StayMinutes * 60) Return();
                break;
            case ParkPhase.Leaving:
                bool returned = true;
                for (int i = 0; i < cats.Length; i++)
                {
                    double ground = height - 7 - 118.0 / 144 * height;
                    if (cats[i].Depth > ground + 1)
                    { cats[i].MoveTo(cats[i].X, ground, dt, width, true); returned = false; }
                    else returned &= cats[i].MoveTo(Math.Clamp(home[i] * width, 70, Math.Max(70, width - 70)), 0, dt, width);
                }
                if (returned) SetPhase(ParkPhase.Disappearing);
                break;
            case ParkPhase.Disappearing:
                Opacity = Math.Max(0, Opacity - dt / 2);
                if (Opacity <= 0)
                {
                    foreach (var cat in cats) cat.ResumeDesktop();
                    SetPhase(ParkPhase.Desktop);
                    if (revisit) { revisit = false; Visit(cats, width); }
                }
                break;
        }
    }

    private static double GroundX(int i, double width) => Math.Clamp(width * (.12 + i * .105), 70, Math.Max(70, width - 70));
    private static double GroundDepth(int i, double height) => height * (.055 + i % 3 * .055);
}
