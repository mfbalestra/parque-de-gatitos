using System;
using System.IO;
using System.Text.Json;

namespace Gatitos;

internal static class Checks
{
    public static void Run(string? output)
    {
        int assertions = 0;
        void Check(bool condition, string message) { assertions++; if (!condition) throw new InvalidOperationException(message); }
        foreach (int count in new[] { 1, 4, 8 })
        foreach (int width in new[] { 320, 800, 1920, 3840 })
        foreach (double dt in new[] { .01, 1.0 / 30, .1 })
        for (int i = 0; i < count; i++)
        {
            var cat = new CatMotion(i, count, width);
            for (int frame = 0; frame < 2400; frame++)
            {
                cat.Advance(dt, width);
                Check(cat.X >= 70 && cat.X <= width - 70, "Un gato salió de los límites.");
                Check(cat.Height >= 0 && cat.Height <= 49, "Altura de salto inválida.");
                Check(cat.Direction is 1 or -1, "Dirección inválida.");
            }
        }
        Check(CatMotion.JumpHeight(0) == 0 && CatMotion.JumpHeight(CatMotion.AirTime) < 1e-8, "El salto no vuelve al suelo.");
        for (int i = 0; i <= 100; i++)
        {
            double t = CatMotion.AirTime * i / 100;
            Check(Math.Abs(CatMotion.JumpHeight(t) - CatMotion.JumpHeight(CatMotion.AirTime - t)) < 1e-8, "Arco asimétrico.");
        }
        for (int n = -100; n <= 100; n++) Check(Options.ValidCount(n) == (n == 1 || n == 4 || n == 8), "Cantidad inválida aceptada.");
        var defaults = Options.Parse(Array.Empty<string>());
        Check(defaults.Cats == null && defaults.Fps == null, "El inicio debe usar las preferencias guardadas.");
        foreach (int fps in new[] { 30, 60 }) Check(Options.Parse(new[] { "--fps", fps.ToString() }).Fps == fps, "Override de FPS perdido.");
        foreach (string fps in new[] { "0", "45", "120", "x" })
        {
            bool rejected = false;
            try { Options.Parse(new[] { "--fps", fps }); } catch (ArgumentException) { rejected = true; }
            Check(rejected, "FPS inválidos aceptados.");
        }
        for (int companion = -2; companion <= 242; companion++)
        for (int stay = -2; stay <= 62; stay++)
            Check(new ParkSettings(companion, stay).Valid == (companion >= 1 && companion <= 240 && stay >= 1 && stay <= 60), "Rango de tiempos incorrecto.");
        foreach (int count in new[] { 1, 4, 8 })
        foreach (int width in new[] { 800, 1920, 3840 })
        foreach (double dt in new[] { 1.0 / 30, .1 })
        {
            var cats = new CatMotion[count];
            for (int i = 0; i < count; i++) cats[i] = new(i, count, width);
            var cycle = new ParkCycle(new(1, 1));
            double simulated = 0; bool visiting = false, returning = false, completed = false;
            for (int frame = 0; frame < 10000; frame++)
            {
                cycle.Advance(dt, cats, width, 1032); simulated += dt;
                if (simulated < 59.9) Check(cycle.Phase == ParkPhase.Desktop, "Visita antes del intervalo.");
                visiting |= cycle.Phase == ParkPhase.Visiting;
                returning |= cycle.Phase == ParkPhase.Leaving;
                Check(cycle.Opacity >= 0 && cycle.Opacity <= 1, "Opacidad fuera de rango.");
                foreach (var cat in cats)
                {
                    Check(double.IsFinite(cat.X) && double.IsFinite(cat.Depth) && cat.Depth >= 0, $"Ruta inválida: {cycle.Phase}, gato {cat.Coat}, x={cat.X:R}, profundidad={cat.Depth:R}.");
                    Check(cat.X >= 70 && cat.X <= width - 70 && cat.Height >= 0 && cat.Height <= 56, "Gato fuera de escena.");
                    if (cycle.Phase == ParkPhase.Desktop) Check(cat.Depth == 0, "Gato suspendido al volver.");
                }
                if (returning && cycle.Phase == ParkPhase.Desktop) { completed = true; break; }
            }
            Check(visiting && completed && cycle.Visits == 1, "El ciclo automático no se completó.");
            foreach (int interruptFrame in new[] { 0, 15, 90, 900 })
            {
                cycle.Visit(cats, width);
                for (int frame = 0; frame < interruptFrame; frame++) cycle.Advance(.033, cats, width, 1032);
                var cat0 = cats[0]; double x = cat0.X, depth = cat0.Depth;
                cycle.Return();
                Check(cat0.X == x && cat0.Depth == depth, "Volver teletransportó un gato.");
                for (int frame = 0; frame < 4000 && cycle.Expanded; frame++) cycle.Advance(.033, cats, width, 1032);
                Check(!cycle.Expanded && cat0.Depth == 0, "No se pudo cancelar la visita.");
            }
            cycle.Visit(cats, width); cycle.Return(); cycle.Visit(cats, width);
            for (int frame = 0; frame < 4000 && cycle.Phase != ParkPhase.Visiting; frame++) cycle.Advance(.033, cats, width, 1032);
            Check(cycle.Phase == ParkPhase.Visiting, "La visita solicitada durante la vuelta se perdió.");
        }
        var art = new SpriteCatalog(); art.Load(8);
        for (int coat = 0; coat < 8; coat++)
        {
            Check(art.Get(coat, "standing").Width > 0, "Arte faltante.");
            for (int frame = 0; frame < 8; frame++)
            {
                var sprite = art.Get(coat, "walk" + frame);
                Check(sprite.Drawing.IsFrozen && sprite.Width > 0 && sprite.AnchorY == .9, "Cuadro de caminata faltante o sin apoyo registrado.");
            }
        }
        File.WriteAllText(output!, JsonSerializer.Serialize(new { passed = true, assertions, coats = 8 }));
    }
}
