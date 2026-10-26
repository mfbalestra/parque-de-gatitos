using System;

namespace Gatitos;

internal sealed class CatMotion
{
    public double X { get; private set; }
    public double Height { get; private set; }
    public int Direction { get; private set; }
    public string Pose { get; private set; } = "standing";
    public double Squash { get; private set; } = 1;
    public int Coat { get; }
    private double time;
    public double Depth { get; private set; }
    private double stride, hopTime, hopDuration, hopX, hopDepth, hopTargetX, hopTargetDepth;
    public double WalkDistance => stride;
    private void Walk(double distance)
    {
        stride = (stride + distance) % WalkGait.StrideLength(Coat);
        Pose = "walk" + WalkGait.Frame(stride, Coat);
    }
    private const double Speed = 32, Gravity = 620, Impulse = 245;
    public const double AirTime = 2 * Impulse / Gravity;

    public CatMotion(int index, int count, double width)
    {
        Coat = index; X = 70 + (width - 140) * (index + .5) / count;
        Direction = index % 2 == 0 ? 1 : -1;
        time = index * 2.3;
    }

    public static double JumpHeight(double seconds) => Math.Max(0, Impulse * seconds - Gravity * seconds * seconds / 2);

    public void ResumeDesktop() { time = 0; Depth = 0; Height = 0; Pose = "standing"; Squash = 1; }
    public void Rest(double elapsed, int kind)
    {
        Pose = kind == 0 ? "sit" : kind == 1 ? "sleep" : (int)(elapsed * 3) % 2 == 0 ? "groomA" : "groomB";
        Squash = 1;
    }
    public bool FinishJump(double dt, double width)
    {
        if (hopDuration > 0) { MoveTo(hopTargetX, hopTargetDepth, dt, width); return false; }
        if (Height > 0) { Advance(dt, width); return false; }
        Height = 0; Squash = 1; Pose = "standing"; return true;
    }
    public bool MoveTo(double x, double depth, double dt, double width, bool hop = false)
    {
        x = Math.Clamp(x, 70, Math.Max(70, width - 70));
        if (hopDuration > 0)
        {
            hopTime = Math.Min(hopDuration, hopTime + dt);
            double fraction = hopTime / hopDuration;
            X = hopX + (hopTargetX - hopX) * fraction;
            Depth = hopDepth + (hopTargetDepth - hopDepth) * fraction;
            Height = 4 * 55 * fraction * (1 - fraction);
            Pose = fraction < .3 ? "jumpRise" : fraction < .65 ? "jumpTuck" : "jumpFall";
            if (hopTime >= hopDuration) { hopDuration = 0; Height = 0; Pose = "landing"; }
            return false;
        }
        if (!FinishJump(dt, width)) return false;
        double dx = x - X, dy = depth - Depth, distance = Math.Sqrt(dx * dx + dy * dy);
        if (distance < .01) { X = x; Depth = depth; return true; }
        if (Math.Abs(dx) > .01) Direction = dx > 0 ? 1 : -1;
        if (hop && Math.Abs(dy) > 2)
        {
            hopTime = 0; hopDuration = Math.Clamp(distance / 190, .6, 1.1);
            hopX = X; hopDepth = Depth; hopTargetX = x; hopTargetDepth = depth; Pose = "crouch";
            return false;
        }
        double travel = Math.Min(distance, 90 * dt);
        if (travel >= distance) { X = x; Depth = depth; }
        else { X += dx / distance * travel; Depth += dy / distance * travel; }
        Walk(travel);
        return travel >= distance;
    }

    public void Advance(double dt, double width)
    {
        if (!double.IsFinite(dt) || dt < 0 || dt > 2) throw new ArgumentOutOfRangeException(nameof(dt));
        // Split at behavior boundaries, so a frame that straddles takeoff or
        // landing accounts for exactly the moving portion at either render rate.
        double remaining = dt;
        while (remaining > 1e-12)
        {
            double phaseAtStart = Math.Round(time % 20, 10);
            if (phaseAtStart >= 20) { time = Math.Round(time / 20) * 20; phaseAtStart = 0; }
            double boundary = phaseAtStart < 7 ? 7 : phaseAtStart < 7.22 ? 7.22 : phaseAtStart < 7.22 + AirTime ? 7.22 + AirTime : 20;
            double slice = Math.Min(remaining, boundary - phaseAtStart);
            double speed = phaseAtStart < 7 ? Speed : phaseAtStart >= 7.22 && phaseAtStart < 7.22 + AirTime ? Speed * 2 : 0;
            double travel = speed * slice;
            X += Direction * travel;
            double right = Math.Max(70, width - 70);
            if (right == 70) X = 70;
            else
            {
                while (X > right || X < 70)
                {
                    if (X > right) { X = right - (X - right); Direction = -1; }
                    if (X < 70) { X = 70 + (70 - X); Direction = 1; }
                }
                if (phaseAtStart < 7) Walk(travel);
            }
            time += slice; remaining -= slice;
        }
        double phase = Math.Round(time % 20, 10);
        if (phase >= 20) phase = 0;
        Height = 0; Squash = 1;
        if (phase < 7)
        {
            Walk(0);
        }
        else if (phase < 7.22) { Pose = "crouch"; Squash = 1 - .10 * Math.Sin((phase - 7) / .22 * Math.PI); }
        else if (phase < 7.22 + AirTime)
        {
            double flight = phase - 7.22;
            Height = JumpHeight(flight);
            Pose = flight < AirTime * .3 ? "jumpRise" : flight < AirTime * .65 ? "jumpTuck" : "jumpFall";
        }
        else if (phase < 7.22 + AirTime + .24)
        { Pose = "landing"; Squash = 1 - .13 * Math.Sin((phase - 7.22 - AirTime) / .24 * Math.PI); }
        else if (phase < 12) Pose = "sit";
        else if (phase < 15) Pose = (int)(time * 3) % 2 == 0 ? "groomA" : "groomB";
        else Pose = "sleep";
    }
}
