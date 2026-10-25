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
        var art = new SpriteCatalog(); art.Load(8);
        for (int coat = 0; coat < 8; coat++) Check(art.Get(coat, "standing").Width > 0, "Arte faltante.");
        File.WriteAllText(output!, JsonSerializer.Serialize(new { passed = true, assertions, coats = 8 }));
    }
}
