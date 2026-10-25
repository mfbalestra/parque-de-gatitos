using System;
using System.IO;
using System.Threading;
using System.Windows;

namespace Gatitos;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            var options = Options.Parse(args);
            if (options.SelfTest) { Checks.Run(options.Output); return 0; }
            var app = new Application { ShutdownMode = ShutdownMode.OnMainWindowClose };
            if (options.Probe) return app.Run(new InputProbe(options.Output));
            using var instance = new Mutex(true, @"Local\ParqueGatitosWpfPrototype", out bool created);
            if (!created) throw new InvalidOperationException("Ya hay una prueba de gatitos abierta. Cerrala desde su icono en la bandeja.");
            return app.Run(new OverlayWindow(options));
        }
        catch (Exception error)
        {
            if (Array.IndexOf(args, "--self-test") >= 0)
            {
                int outputIndex = Array.IndexOf(args, "--output");
                if (outputIndex >= 0 && outputIndex + 1 < args.Length)
                    File.WriteAllText(args[outputIndex + 1], System.Text.Json.JsonSerializer.Serialize(new { passed = false, error = error.ToString() }));
                return 1;
            }
            MessageBox.Show(error.Message, "Gatitos — no se pudo iniciar", MessageBoxButton.OK, MessageBoxImage.Error);
            return 1;
        }
    }
}

internal sealed record Options(int Cats, bool Paused, bool Probe, bool SelfTest, bool Panel, string? Output, bool ParkNow)
{
    public static bool ValidCount(int count) => count is 1 or 4 or 8;

    public static Options Parse(string[] args)
    {
        int cats = 4; bool paused = false, probe = false, selfTest = false, panel = false, parkNow = false; string? output = null;
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--cats" when i + 1 < args.Length && int.TryParse(args[++i], out int count) && ValidCount(count): cats = count; break;
                case "--paused": paused = true; break;
                case "--panel": panel = true; break;
                case "--park-now": parkNow = true; break;
                case "--probe": probe = true; break;
                case "--self-test": selfTest = true; break;
                case "--output" when i + 1 < args.Length: output = Path.GetFullPath(args[++i]); break;
                default: throw new ArgumentException("Opciones: --cats 1|4|8, --paused, --panel, --park-now, --output archivo.json, --probe, --self-test.");
            }
        }
        if ((probe || selfTest) && output == null) throw new ArgumentException("La prueba necesita --output archivo.json.");
        if (probe && selfTest || paused && output != null && !probe) throw new ArgumentException("Combinación de opciones incompatible.");
        return new(cats, paused, probe, selfTest, panel, output, parkNow);
    }
}
