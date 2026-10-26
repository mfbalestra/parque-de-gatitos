using System;

namespace Gatitos;

// Compositor timestamps avoid wall-clock jitter. Deadlines remain anchored across frames:
// a late callback drops missed drawings, never accumulates catch-up renders or simulation time.
internal sealed class FramePacer
{
    private double next = double.NaN;
    private double lastTimestamp = double.NaN;
    public int Target { get; private set; }

    public FramePacer(int target) => Configure(target);
    public void Configure(int target)
    {
        if (!ParkSettings.ValidFps(target)) throw new ArgumentOutOfRangeException(nameof(target));
        Target = target; Reset();
    }
    public void Reset() { next = double.NaN; lastTimestamp = double.NaN; }
    public bool ShouldRender(double timestamp)
    {
        if (!double.IsFinite(timestamp)) throw new ArgumentOutOfRangeException(nameof(timestamp));
        if (timestamp == lastTimestamp) return false;
        if (double.IsNaN(next) || timestamp < lastTimestamp) next = timestamp;
        lastTimestamp = timestamp;
        // RenderingTime uses TimeSpan ticks; tolerate its sub-microsecond quantization only.
        const double tolerance = .0000002;
        if (timestamp + tolerance < next) return false;
        double interval = 1.0 / Target;
        next += (Math.Floor(Math.Max(0, timestamp + tolerance - next) / interval) + 1) * interval;
        return true;
    }
}
