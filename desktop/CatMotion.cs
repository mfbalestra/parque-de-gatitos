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
    private const double Speed = 32, Gravity = 620, Impulse = 245;
    public const double AirTime = 2 * Impulse / Gravity;

    public CatMotion(int index, int count, double width)
    {
        Coat = index; X = 70 + (width - 140) * (index + .5) / count;
        Direction = index % 2 == 0 ? 1 : -1;
        time = index * 2.3;
    }

    public static double JumpHeight(double seconds) => Math.Max(0, Impulse * seconds - Gravity * seconds * seconds / 2);

    public void Advance(double dt, double width)
    {
        time += dt;
        double phase = time % 20;
        Height = 0; Squash = 1;
        double speed = 0;
        if (phase < 7)
        {
            Pose = ((int)(time * 5) % 2 == 0) ? "walkA" : "walkB"; speed = Speed;
        }
        else if (phase < 7.22) { Pose = "crouch"; Squash = 1 - .10 * Math.Sin((phase - 7) / .22 * Math.PI); }
        else if (phase < 7.22 + AirTime)
        {
            double flight = phase - 7.22;
            Height = JumpHeight(flight); speed = Speed * 2;
            Pose = flight < AirTime * .3 ? "jumpRise" : flight < AirTime * .65 ? "jumpTuck" : "jumpFall";
        }
        else if (phase < 7.22 + AirTime + .24)
        { Pose = "landing"; Squash = 1 - .13 * Math.Sin((phase - 7.22 - AirTime) / .24 * Math.PI); }
        else if (phase < 12) Pose = "sit";
        else if (phase < 15) Pose = (int)(time * 3) % 2 == 0 ? "groomA" : "groomB";
        else Pose = "sleep";
        X += Direction * speed * dt;
        double right = Math.Max(70, width - 70);
        if (X >= right) { X = right; Direction = -1; }
        if (X <= 70) { X = 70; Direction = 1; }
    }
}
