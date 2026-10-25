using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Gatitos;

internal sealed class Measurement : IDisposable
{
    private readonly OverlayWindow window;
    private readonly string output;
    private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly Stopwatch clock = new();
    private readonly Process process = Process.GetCurrentProcess();
    private readonly List<object> phases = new();
    private readonly List<object> samples = new();
    private int stage;
    private double phaseStart, cpuStart;
    private long framesStart, stepsStart;
    private readonly List<long> resident = new(), committed = new();
    private int foregroundSamples;

    public Measurement(OverlayWindow window, string output) { this.window = window; this.output = output; timer.Tick += (_, _) => Tick(); }
    public void Start() { clock.Start(); timer.Start(); }

    private void Begin()
    {
        process.Refresh(); phaseStart = clock.Elapsed.TotalSeconds; cpuStart = process.TotalProcessorTime.TotalSeconds;
        framesStart = window.Frames; stepsStart = window.Steps;
        resident.Clear(); committed.Clear(); foregroundSamples = 0;
    }

    private void Sample()
    {
        process.Refresh(); resident.Add(process.WorkingSet64); committed.Add(process.PrivateMemorySize64);
        bool foreground = Native.GetForegroundWindow() == window.Handle;
        if (foreground) foregroundSamples++;
        samples.Add(new { timestampUtc = DateTime.UtcNow, elapsed = clock.Elapsed.TotalSeconds, state = window.Hidden ? "hidden" : "visible",
            cpuSeconds = process.TotalProcessorTime.TotalSeconds, residentBytes = process.WorkingSet64,
            privateBytes = process.PrivateMemorySize64, frames = window.Frames, steps = window.Steps, foreground });
    }

    private void End(string state)
    {
        double elapsed = clock.Elapsed.TotalSeconds - phaseStart;
        double cpu = process.TotalProcessorTime.TotalSeconds - cpuStart;
        phases.Add(new { state, elapsedSeconds = elapsed, cpuSeconds = cpu, cpuPercentOneCore = cpu / elapsed * 100,
            cpuPercentMachine = cpu / elapsed * 100 / Environment.ProcessorCount,
            residentMeanMiB = resident.Average() / 1048576, residentPeakMiB = resident.Max() / 1048576.0,
            privateMeanMiB = committed.Average() / 1048576, privatePeakMiB = committed.Max() / 1048576.0,
            renderedFrames = window.Frames - framesStart, simulationSteps = window.Steps - stepsStart,
            renderedFps = (window.Frames - framesStart) / elapsed, foregroundSamples });
    }

    private void Tick()
    {
        double now = clock.Elapsed.TotalSeconds;
        switch (stage)
        {
            case 0 when now >= 5: Begin(); stage = 1; break;
            case 1:
                Sample();
                if (now - phaseStart >= 20) { End("visible"); window.SetHidden(true); phaseStart = now; stage = 2; }
                break;
            case 2 when now - phaseStart >= 2: Begin(); stage = 3; break;
            case 3:
                Sample();
                if (now - phaseStart >= 10)
                {
                    End("hidden");
                    var dpi = VisualTreeHelper.GetDpi(window);
                    var result = new { measuredAtUtc = DateTime.UtcNow, processId = process.Id, cats = window.CatCount, targetFps = 30,
                        logicalProcessors = Environment.ProcessorCount, runtime = Environment.Version.ToString(),
                        os = Environment.OSVersion.VersionString, renderTier = RenderCapability.Tier >> 16,
                        widthDip = window.Width, heightDip = window.Height, dpiScale = dpi.DpiScaleX,
                        styles = $"0x{Native.GetWindowLongPtr(window.Handle, -20).ToInt64():X}",
                        overlayActivations = window.Activations, phases, samples };
                    File.WriteAllText(output, JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
                    stage = 4; timer.Stop(); window.Close();
                }
                break;
        }
    }

    public void Dispose() { timer.Stop(); process.Dispose(); }
}
