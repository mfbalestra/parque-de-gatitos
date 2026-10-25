using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Gatitos;

internal sealed record ParkSettings(int CompanionMinutes = 30, int StayMinutes = 5)
{
    public bool Valid => CompanionMinutes is >= 1 and <= 240 && StayMinutes is >= 1 and <= 60;
    public static string PathName => Path.Combine(AppContext.BaseDirectory, "gatitos-settings.json");
    public static ParkSettings Load()
    {
        if (!File.Exists(PathName)) return new();
        var value = JsonSerializer.Deserialize<ParkSettings>(File.ReadAllText(PathName));
        return value?.Valid == true ? value : throw new InvalidDataException("Los tiempos guardados no son válidos.");
    }
    public void Save()
    {
        if (!Valid) throw new ArgumentOutOfRangeException(nameof(CompanionMinutes));
        string temporary = PathName + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporary, PathName, true);
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
    private int[] route = Array.Empty<int>();
    private double[] rest = Array.Empty<double>();
    private static readonly (double x, double y)[] Gym = { (136, 118), (146, 110), (157, 102), (173, 94), (188, 78), (200, 78), (237, 118) };

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
        home = cats.Select(c => c.X / width).ToArray();
        route = new int[cats.Length]; rest = new double[cats.Length];
        SetPhase(ParkPhase.Appearing); Visits++;
    }
    public void Return()
    {
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
                foreach (var cat in cats) cat.Advance(dt, width);
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
                for (int i = 0; i < cats.Length; i++) AnimateVisitor(cats[i], i, dt, width, height);
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
    private void AnimateVisitor(CatMotion cat, int i, double dt, double width, double height)
    {
        if (rest[i] > 0)
        {
            rest[i] -= dt; cat.Rest(SceneTime, i % 3); return;
        }
        int step = route[i] % 9;
        bool gym = i % 3 == 0 && step >= 1 && step <= 7;
        double x = gym ? Gym[step - 1].x / 256 * width : GroundX((i + step) % 8, width);
        double depth = gym ? height - 7 - Gym[step - 1].y / 144 * height : GroundDepth(i, height);
        if (cat.MoveTo(x, depth, dt, width, gym && step > 1 && step < 7))
        {
            route[i]++; rest[i] = 3 + i % 3 * 2;
        }
    }
}
