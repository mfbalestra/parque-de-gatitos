using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Gatitos;

internal sealed class CatBehavior
{
    private sealed class State
    {
        public readonly Random Random;
        public readonly double Sociability;
        public string Action = "", Previous = "";
        public double Remaining, Age, Energy = .6, Cooldown, X, Depth;
        public int Partner = -1;
        public bool Gym;
        public bool Contact;
        public bool Exiting;
        public int GymStep;
        public int Mat = -1;
        public bool KneadReady;
        public State(int seed) { Random = new Random(seed); Sociability = Random.NextDouble(); }
    }
    private readonly JsonDocument profile;
    private State[] states = Array.Empty<State>();
    private CatMotion[] cats = Array.Empty<CatMotion>();
    private readonly int? seed;
    private static readonly (double x, double y)[] Gym = {
        (136, 118), (146, 110), (157, 102), (173, 94), (188, 78), (200, 78), (237, 118)
    };
    private static readonly (double x, double y)[] Mats = { (27, 120), (96, 137), (211, 132) };
    internal (string action, double remaining, double cooldown, int partner) Inspect(int index) =>
        (states[index].Action, states[index].Remaining, states[index].Cooldown, states[index].Partner);
    public CatBehavior(int? seed = null)
    {
        this.seed = seed;
        string source = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "behavior-profile.js"));
        int start = source.IndexOf('{'), end = source.LastIndexOf('}');
        profile = JsonDocument.Parse(source.Substring(start, end - start + 1));
    }
    private JsonElement Data => profile.RootElement;
    private double Duration(string action, Random random)
    {
        var range = Data.GetProperty("durations").GetProperty(action);
        return range[0].GetDouble() + random.NextDouble() * (range[1].GetDouble() - range[0].GetDouble());
    }
    private void Ensure(CatMotion[] current)
    {
        if (ReferenceEquals(cats, current)) return;
        Cancel();
        cats = current;
        states = current.Select((_, i) => new State(seed.HasValue ? unchecked(seed.Value + i * 7919) : Random.Shared.Next())).ToArray();
    }
    public void Cancel()
    {
        foreach (var state in states)
        {
            if (state.Partner >= 0 && state.Action == "spar") state.Cooldown = Data.GetProperty("playCooldown").GetDouble();
            if (state.Action != "") state.Previous = state.Action;
            state.Partner = -1; state.Remaining = 0; state.Action = ""; state.Gym = false; state.Mat = -1;
        }
    }
    public void Demo(CatMotion[] current, string action)
    {
        Ensure(current);
        if (current.Length == 0) return;
        if (action is "rub" or "cuddle" or "spar")
        {
            if (current.Length < 2) return;
            StartPair(0, 1, action);
        }
        else Start(0, action);
    }
    private void Start(int i, string action)
    {
        var s = states[i];
        s.Previous = s.Action; s.Action = action; s.Age = 0;
        if (action == "knead") { s.Mat = -1; s.KneadReady = false; }
        else if (action != "settle" && action != "sleep") s.Mat = -1;
        s.Remaining = Duration(action, s.Random);
    }
    private void StartPair(int i, int j, string action)
    {
        Start(i, action); Start(j, action);
        states[i].Partner = j; states[j].Partner = i;
        states[i].Contact = states[j].Contact = false;
        states[i].Exiting = states[j].Exiting = false;
        states[j].Remaining = states[i].Remaining;
        double center = (cats[i].X + cats[j].X) / 2;
        double offset = Data.GetProperty("pairSeparation").GetProperty(action).GetDouble() * 2 * (cats[i].X <= cats[j].X ? -1 : 1);
        states[i].X = center + offset; states[j].X = center - offset;
        states[i].Depth = states[j].Depth = (cats[i].Depth + cats[j].Depth) / 2;
    }
    private void Release(int i)
    {
        var s = states[i]; int partner = s.Partner;
        bool intense = s.Action == "spar";
        double cooldown = Data.GetProperty(intense ? "playCooldown" : "socialCooldown").GetDouble();
        s.Cooldown = cooldown;
        s.Partner = -1; Start(i, "sit");
        if (intense) s.Remaining = Math.Max(s.Remaining, cooldown);
        if (partner >= 0)
        {
            var other = states[partner];
            other.Cooldown = cooldown;
            other.Partner = -1; Start(partner, "sit");
            if (intense) other.Remaining = Math.Max(other.Remaining, cooldown);
        }
    }
    public void Advance(double dt, CatMotion[] current, double width, double height, bool park)
    {
        Ensure(current);
        for (int i = 0; i < cats.Length; i++)
        {
            var s = states[i]; var cat = cats[i];
            s.Cooldown = Math.Max(0, s.Cooldown - dt);
            if (s.Partner < 0 && cat.Height > 0 && !s.Gym) { cat.FinishJump(dt, width); continue; }
            if (s.Partner >= 0)
            {
                int j = s.Partner;
                if (j < i) continue;
                var other = states[j]; var mate = cats[j];
                if (!s.Contact && !s.Exiting && s.Age == 0)
                {
                    double half = Data.GetProperty("pairSeparation").GetProperty(s.Action).GetDouble() * 2;
                    if (width < 140 + half * 2) { Release(i); continue; }
                    double center = Math.Clamp((s.X + other.X) / 2, 70 + half, width - 70 - half);
                    double sign = s.X <= other.X ? -1 : 1;
                    s.X = center + sign * half; other.X = center - sign * half;
                }
                double speed = park ? 90 : 32;
                bool a = cat.MoveTo(s.X, s.Depth, dt, width, speed: speed);
                bool b = mate.MoveTo(other.X, other.Depth, dt, width, speed: speed);
                if (s.Exiting)
                {
                    s.Age += dt;
                    if ((a && b) || s.Age >= 2) Release(i);
                    continue;
                }
                if (a && b)
                {
                    cat.Face(cat.X <= mate.X ? 1 : -1);
                    mate.Face(cat.X <= mate.X ? -1 : 1);
                    if (!s.Contact) { s.Contact = other.Contact = true; s.Age = 0; }
                    s.Age += dt; s.Remaining -= dt; other.Age = s.Age; other.Remaining = s.Remaining;
                    if (s.Action == "spar")
                    {
                        s.Energy = Math.Max(0, s.Energy - dt * .015);
                        other.Energy = Math.Max(0, other.Energy - dt * .015);
                    }
                    string pose = s.Action == "cuddle" ? "sleep" : s.Action + ((int)(s.Age * 2) % 4);
                    cat.SetPose(pose);
                    mate.SetPose(s.Action == "spar" ? "spar" + (((int)(s.Age * 2) + 2) % 4) : pose);
                    if (s.Remaining <= 0)
                    {
                        s.Exiting = other.Exiting = true; s.Age = 0;
                        double sign = cat.X <= mate.X ? -1 : 1;
                        s.X = cat.X + sign * 32; other.X = mate.X - sign * 32;
                        s.Depth = cat.Depth; other.Depth = mate.Depth;
                    }
                }
                else if ((s.Age += dt) >= 20) Release(i);
                continue;
            }
            if (s.Remaining <= 0)
            {
                if (s.Action == "knead") Start(i, "settle");
                else if (s.Action == "settle") Start(i, "sleep");
                else Choose(i, width, height, park);
            }
            if (s.Action == "knead" && !s.KneadReady)
            {
                if (!park) s.KneadReady = true;
                else
                {
                    if (s.Mat < 0)
                    {
                        s.Mat = Enumerable.Range(0, Mats.Length)
                            .Where(m => Mats[m].x / 256 * width >= 70 && Mats[m].x / 256 * width <= Math.Max(70, width - 70) &&
                                height - 7 - Mats[m].y / 144 * height >= 0 &&
                                !states.Where((other, j) => j != i).Any(other => other.Mat == m) &&
                                !cats.Where((other, j) => j != i).Any(other =>
                                    Math.Abs(other.X - Mats[m].x / 256 * width) < 80 &&
                                    Math.Abs(other.Depth - (height - 7 - Mats[m].y / 144 * height)) < 60))
                            .OrderBy(m => Math.Pow(cat.X - Mats[m].x / 256 * width, 2) +
                                Math.Pow(cat.Depth - (height - 7 - Mats[m].y / 144 * height), 2))
                            .DefaultIfEmpty(-1).First();
                    }
                    if (s.Mat < 0) { s.Remaining = 0; continue; }
                    double matX = Mats[s.Mat].x / 256 * width;
                    double matDepth = Math.Max(0, height - 7 - Mats[s.Mat].y / 144 * height);
                    if (!cat.MoveTo(matX, matDepth, dt, width, speed: park ? 90 : 32)) continue;
                    s.KneadReady = true;
                }
            }
            s.Age += dt; s.Remaining -= dt;
            if (s.Gym) s.Remaining = Math.Max(s.Remaining, 1);
            switch (s.Action)
            {
                case "wander":
                    bool blocked = states.Where((other, j) => j != i && other.Partner >= 0)
                        .Any(other => Math.Abs(cat.X - other.X) < 115 && Math.Abs(cat.Depth - other.Depth) < 70 &&
                            Math.Abs(s.X - other.X) < Math.Abs(cat.X - other.X));
                    if (!blocked && cat.MoveTo(s.X, s.Depth, dt, width, s.Gym && s.GymStep > 0, park ? 90 : 32))
                    {
                        if (s.Gym && ++s.GymStep < Gym.Length)
                        {
                            s.X = Gym[s.GymStep].x / 256 * width;
                            s.Depth = height - 7 - Gym[s.GymStep].y / 144 * height;
                        }
                        else { s.Gym = false; s.Remaining = 0; }
                    }
                    s.Energy = Math.Max(0, s.Energy - dt * .004); break;
                case "sleep": cat.Rest(s.Age, 1); s.Energy = Math.Min(1, s.Energy + dt * .005); break;
                case "sit": cat.Rest(s.Age, 0); s.Energy = Math.Min(1, s.Energy + dt * .002); break;
                case "groom": cat.Rest(s.Age, 2); break;
                case "knead": cat.SetPose("knead" + ((int)(s.Age * 2) % 4)); break;
                case "settle": cat.SetPose("settle" + Math.Min(3, (int)(s.Age / (s.Age + s.Remaining) * 4))); break;
            }
        }
    }
    private void Choose(int i, double width, double height, bool park)
    {
        var s = states[i]; var weights = Data.GetProperty(park ? "weightsPark" : "weightsCompanion");
        var names = new[] { "sleep", "sit", "groom", "wander", "knead", "social", "environment" };
        double[] values = new double[names.Length];
        int pairCount = states.Count(x => x.Partner >= 0 && x.Action == "spar") / 2;
        int limit = Data.GetProperty(park ? (cats.Length >= 8 ? "pairLimitParkEight" : "pairLimitParkFour") : "pairLimitCompanion").GetInt32();
        int[] partners = Enumerable.Range(0, cats.Length).Where(j => j != i && cats[j].Coat == 0 && states[j].Partner < 0 && !states[j].Gym && (states[j].Remaining <= 0 || states[j].Action == "wander") && states[j].Cooldown <= 0 &&
            !Enumerable.Range(0, cats.Length).Any(k => k != i && k != j &&
                cats[k].X > Math.Min(cats[i].X, cats[j].X) - 110 && cats[k].X < Math.Max(cats[i].X, cats[j].X) + 110 &&
                Math.Abs(cats[k].Depth - (cats[i].Depth + cats[j].Depth) / 2) < 70)).ToArray();
        for (int n = 0; n < names.Length; n++)
        {
            if (!weights.TryGetProperty(names[n], out var weight)) continue;
            values[n] = weight.GetDouble();
            if (names[n] == s.Action || names[n] == s.Previous) values[n] *= .25;
            if (names[n] == "sleep") values[n] *= 1.5 - s.Energy;
            if (names[n] == "wander") values[n] *= .5 + s.Energy;
            if (names[n] == "social" || names[n] == "environment") values[n] *= .5 + s.Energy;
            if (names[n] == "social") values[n] *= .5 + s.Sociability;
            if (names[n] == "knead" && cats[i].Coat != 0) values[n] = 0;
            if (names[n] == "environment" && s.Cooldown > 0) values[n] = 0;
            if (names[n] == "social" && (cats[i].Coat != 0 || partners.Length == 0 || s.Cooldown > 0 || pairCount >= limit)) values[n] = 0;
        }
        double choice = s.Random.NextDouble() * values.Sum();
        int selected = 0;
        while (selected < values.Length - 1 && (choice -= values[selected]) >= 0) selected++;
        string action = names[selected];
        if (action == "social")
        {
            string[] actions = { "rub", "cuddle", "spar" };
            StartPair(i, partners[s.Random.Next(partners.Length)], actions[s.Random.Next(actions.Length)]); return;
        }
        bool gym = action == "environment";
        if (gym) action = "wander";
        Start(i, action);
        if (action == "wander")
        {
            s.Gym = gym;
            s.GymStep = 0;
            s.X = gym ? width * Gym[0].x / 256 : Math.Clamp(cats[i].X + (s.Random.NextDouble() * 2 - 1) * 180, 70, Math.Max(70, width - 70));
            s.Depth = gym ? height - 7 - Gym[0].y / 144 * height : park ? Math.Clamp(cats[i].Depth + (s.Random.NextDouble() * 2 - 1) * 40, 0, height * .2) : 0;
            if (gym) s.Remaining = 60;
        }
    }
}
