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
        public bool RestReserved, RestReady, GymExit;
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
    internal (bool gym, int step, bool exit, bool restReady) InspectRoute(int index) =>
        (states[index].Gym, states[index].GymStep, states[index].GymExit, states[index].RestReady);
    internal (bool reserved, double x, double depth) InspectRest(int index) =>
        (states[index].RestReserved, states[index].X, states[index].Depth);
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
            state.Partner = -1; state.Remaining = 0; state.Action = ""; state.Gym = false; state.Mat = -1; state.RestReserved = state.RestReady = state.GymExit = false;
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
    internal void DemoAt(CatMotion[] current, int index, string action)
    {
        Ensure(current);
        Start(index, action);
    }
    private void Start(int i, string action)
    {
        var s = states[i];
        bool settling = s.Action == "settle" && action == "sleep";
        if (action == "sleep" && !CanSleep(i)) action = s.Random.NextDouble() < .5 ? "sit" : "groom";
        bool retainRest = s.RestReserved && ((s.Action == "knead" && action == "settle") || settling);
        s.Previous = s.Action; s.Action = action; s.Age = 0;
        if (!retainRest) s.RestReserved = s.RestReady = false;
        if (action == "knead") s.Mat = -1;
        else if (action != "settle" && action != "sleep") s.Mat = -1;
        s.Remaining = Duration(action, s.Random);
    }
    private static bool IsRest(string action) => action is "sleep" or "sit" or "groom" or "settle" or "knead";
    internal bool CanSleep(params int[] participants) => states.Length == 1 ||
        states.Where((s, i) => !participants.Contains(i)).Any(s => s.Action is not ("sleep" or "cuddle"));
    private bool FindRest(int i, double width, double height, bool park, bool matOnly = false)
    {
        var s = states[i]; var cat = cats[i];
        double clearanceX = Data.GetProperty("clearanceX").GetDouble() * 4;
        double clearanceY = Data.GetProperty("clearanceY").GetDouble() * 4;
        bool Free(double x, double depth)
        {
            if (x < 70 || x > Math.Max(70, width - 70) || depth < 0 || depth > (park ? height * .2 : 0)) return false;
            if (park && Data.GetProperty("restAvoid").EnumerateArray().Any(p =>
                Math.Abs(x - p.GetProperty("x").GetDouble() / 256 * width) < p.GetProperty("rx").GetDouble() / 256 * width &&
                Math.Abs(depth - (height - 7 - p.GetProperty("y").GetDouble() / 144 * height)) < p.GetProperty("ry").GetDouble() / 144 * height)) return false;
            return !Enumerable.Range(0, cats.Length).Any(j => j != i &&
                ((Math.Abs(x - cats[j].X) < clearanceX && Math.Abs(depth - cats[j].Depth) < clearanceY) ||
                 (states[j].RestReserved && Math.Abs(x - states[j].X) < clearanceX && Math.Abs(depth - states[j].Depth) < clearanceY) ||
                 (states[j].Partner >= 0 && Math.Abs(x - states[j].X) < clearanceX && Math.Abs(depth - states[j].Depth) < clearanceY)));
        }
        var candidates = new System.Collections.Generic.List<(double x, double depth, int mat)>();
        if (matOnly)
            candidates.AddRange(Mats.Select((m, n) => (m.x / 256 * width, Math.Max(0, height - 7 - m.y / 144 * height), n)));
        else
        {
            candidates.Add((cat.X, cat.Depth, -1));
            for (int n = 0; n < 24; n++)
                candidates.Add((Math.Clamp(cat.X + (s.Random.NextDouble() * 2 - 1) * clearanceX * 3, 70, Math.Max(70, width - 70)),
                    park ? Math.Clamp(cat.Depth + (s.Random.NextDouble() * 2 - 1) * clearanceY * 3, 0, height * .2) : 0, -1));
            for (double x = 70; x <= Math.Max(70, width - 70); x += clearanceX / 2)
                for (double d = 0; d <= (park ? height * .2 : 0); d += clearanceY / 2)
                    candidates.Add((x, d, -1));
        }
        var found = candidates.Where(p => Free(p.x, p.depth))
            .OrderBy(p => Math.Pow(p.x - cat.X, 2) + Math.Pow(p.depth - cat.Depth, 2)).Take(1).ToArray();
        if (found.Length == 0) return false;
        s.X = found[0].x; s.Depth = found[0].depth; s.Mat = found[0].mat;
        s.RestReserved = true; s.RestReady = false;
        return true;
    }
    private void StartPair(int i, int j, string action)
    {
        if (action == "cuddle" && !CanSleep(i, j)) action = "rub";
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
            if (IsRest(s.Action) && !s.RestReady)
            {
                if (!s.RestReserved && !FindRest(i, width, height, park, s.Action == "knead" && park))
                {
                    if (s.Action != "knead" || !park) continue;
                    Start(i, "sit");
                    if (!FindRest(i, width, height, park)) continue;
                }
                if (!cat.MoveTo(s.X, s.Depth, dt, width, speed: park ? 90 : 32)) continue;
                s.RestReady = true;
            }
            s.Age += dt; s.Remaining -= dt;
            if (s.Gym) s.Remaining = Math.Max(s.Remaining, 1);
            switch (s.Action)
            {
                case "wander":
                    if (s.GymExit && !s.RestReserved && !FindRest(i, width, height, park)) { s.Remaining = 1; break; }
                    bool blocked = states.Where((other, j) => j != i && other.Partner >= 0)
                        .Any(other => Math.Abs(cat.X - other.X) < 115 && Math.Abs(cat.Depth - other.Depth) < 70 &&
                            Math.Abs(s.X - other.X) < Math.Abs(cat.X - other.X));
                    bool sliding = s.Gym && s.GymStep == Gym.Length - 1;
                    bool arrived = !blocked && cat.MoveTo(s.X, s.Depth, dt, width, s.Gym && s.GymStep > 0 && !sliding, park ? 90 : 32);
                    if (sliding) cat.SetPose("sit");
                    if (arrived)
                    {
                        if (s.Gym && ++s.GymStep < Gym.Length)
                        {
                            s.X = Gym[s.GymStep].x / 256 * width;
                            s.Depth = height - 7 - Gym[s.GymStep].y / 144 * height;
                        }
                        else if (s.Gym)
                        {
                            s.Gym = false; s.GymExit = true;
                            s.RestReserved = false;
                            if (!FindRest(i, width, height, park)) { s.Remaining = 1; }
                        }
                        else { s.GymExit = false; s.RestReserved = false; s.Remaining = 0; }
                    }
                    if (s.GymExit) s.Remaining = Math.Max(s.Remaining, 1);
                    s.Energy = Math.Max(0, s.Energy - dt * .004); break;
                case "sleep": cat.Rest(s.Age, 1); s.Energy = Math.Min(1, s.Energy + dt * .005); break;
                case "sit": cat.Rest(s.Age, 0); s.Energy = Math.Min(1, s.Energy + dt * .002); break;
                case "groom": cat.Rest(s.Age, 2); break;
                case "knead": cat.SetPose("knead" + ((int)(s.Age * 2) % 4)); break;
                case "settle": cat.SetPose("settle" + Math.Min(3, (int)(s.Age / (s.Age + s.Remaining) * 4))); break;
            }
        }
    }
    internal double GroupFactor(string action, double quietFraction, double energy)
    {
        var rule = Data.GetProperty("groupBalance");
        double threshold = rule.GetProperty("quietThreshold").GetDouble();
        double floor = rule.GetProperty("minimumEnergyInfluence").GetDouble();
        double pressure = Math.Clamp((quietFraction - threshold) / (1 - threshold), 0, 1) *
            (floor + (1 - floor) * Math.Clamp(energy, 0, 1));
        double multiplier = rule.GetProperty("multipliers").TryGetProperty(action, out var value) ? value.GetDouble() : 1;
        return 1 + (multiplier - 1) * pressure;
    }
    private void Choose(int i, double width, double height, bool park)
    {
        var s = states[i]; var weights = Data.GetProperty(park ? "weightsPark" : "weightsCompanion");
        var names = new[] { "sleep", "sit", "groom", "wander", "knead", "social", "environment" };
        double[] values = new double[names.Length];
        double quiet = states.Length > 1 ? states.Where((other, j) => j != i &&
            ((other.RestReady && (other.Action is "sleep" or "sit" or "settle")) ||
             (other.Action == "cuddle" && other.Contact && !other.Exiting))).Count() / (double)(states.Length - 1) : 0;
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
            values[n] *= GroupFactor(names[n], quiet, s.Energy);
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
