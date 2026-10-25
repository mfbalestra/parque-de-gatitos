using System;
using Gatitos;

int assertions = 0;
void Check(bool value, string reason) { assertions++; if (!value) throw new Exception(reason); }
foreach (double dt in new[] { .01, 1.0 / 30, 1.0 / 60 })
foreach (int width in new[] { 800, 1920, 3840 })
{
    var cat = new CatMotion(0, 1, width);
    for (int i = 0; i < 150; i++) cat.Advance(.05, width);
    Check(cat.Height > 0, "The fixture must trigger a visit during an airborne jump.");
    double height = cat.Height, x = cat.X;
    var cycle = new ParkCycle(new());
    var cats = new[] { cat };
    cycle.Visit(cats, width);
    Check(cat.X == x && cat.Height == height && ReferenceEquals(cats[0], cat), "Starting a visit must preserve the airborne cat.");
    cycle.Advance(dt, cats, width, 1032);
    Check(cat.Height > 0 && Math.Abs(cat.Height - height) < 20, "The original jump must continue, not snap to the ground.");
    for (int frame = 0; frame < 200; frame++) cycle.Advance(dt, cats, width, 1032);
    Check(cat.Coat == 0 && ReferenceEquals(cats[0], cat), "Travel must retain cat identity.");
    cycle.Return();
    for (int frame = 0; frame < 10000 && cycle.Expanded; frame++) cycle.Advance(dt, cats, width, 1032);
    Check(!cycle.Expanded && cat.Depth == 0 && cat.Height == 0, "Return must finish exactly on the desktop floor.");
}
foreach (int minutes in new[] { 60, 120, 240 })
    Check(new ParkCycle(new(minutes, 5)).Status.Contains($"{minutes / 60:00}:00:00"), "Long intervals must display hours.");
Console.WriteLine($"PASS {assertions} independent park contracts: airborne entry, identity, exact return and hour display.");
