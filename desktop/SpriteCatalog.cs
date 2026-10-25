using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Gatitos;

internal sealed class SpriteCatalog
{
    internal sealed record Sprite(Drawing Drawing, double Width, double Height, int PixelWidth, int PixelHeight, double AnchorX, double AnchorY, int Facing);
    private readonly Dictionary<(int, string), Sprite> sprites = new();
    private readonly Dictionary<string, BitmapSource> images = new();
    private readonly JsonDocument document;
    private static readonly double[] Sizes = { 1, .96, 1.04, .98, 1.05, .95, 1.02, 1 };
    private static readonly string[] Names = { "standing", "walkA", "walkB", "sit", "sleep", "groomA", "groomB", "crouch", "jumpRise", "jumpTuck", "jumpFall", "landing" };

    public SpriteCatalog()
    {
        string source = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "art-data.js"));
        // Parse the shared data as JSON; never execute the web project's JavaScript.
        int start = source.IndexOf('{'), end = source.LastIndexOf('}');
        document = JsonDocument.Parse(source.Substring(start, end - start + 1));
    }

    public void Load(int count)
    {
        for (int coat = 0; coat < count; coat++)
            foreach (string name in Names)
                if (!sprites.ContainsKey((coat, name))) sprites.Add((coat, name), Create(coat, name));
    }

    public Sprite Get(int coat, string name) => sprites[(coat, name)];

    private Sprite Create(int coat, string name)
    {
        var poses = document.RootElement.GetProperty("coats")[coat].GetProperty("poses");
        if (!poses.TryGetProperty(name, out var rect)) rect = poses.GetProperty(name == "landing" ? "crouch" : "standing");
        string sheet = rect.GetProperty("sheet").GetString()!;
        if (!images.TryGetValue(sheet, out var bitmap))
        {
            string relative = document.RootElement.GetProperty("sheets").GetProperty(sheet).GetString()!;
            string root = Path.GetFullPath(AppContext.BaseDirectory);
            string path = Path.GetFullPath(Path.Combine(root, relative));
            if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Ruta de ilustración inválida.");
            var loaded = new BitmapImage(); loaded.BeginInit(); loaded.CacheOption = BitmapCacheOption.OnLoad;
            loaded.UriSource = new Uri(path); loaded.EndInit(); loaded.Freeze(); bitmap = loaded; images.Add(sheet, bitmap);
        }
        int x = rect.GetProperty("x").GetInt32(), y = rect.GetProperty("y").GetInt32();
        int w = rect.GetProperty("w").GetInt32(), h = rect.GetProperty("h").GetInt32();
        var crop = new CroppedBitmap(bitmap, new Int32Rect(x, y, w, h)); crop.Freeze();
        var drawing = new DrawingGroup();
        if (rect.TryGetProperty("clip", out var polygon))
        {
            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                var points = polygon.EnumerateArray().Select(p => new Point(p[0].GetDouble(), p[1].GetDouble())).ToArray();
                context.BeginFigure(points[0], true, true); context.PolyLineTo(points.Skip(1).ToArray(), true, false);
            }
            geometry.Freeze(); drawing.ClipGeometry = geometry;
        }
        drawing.Children.Add(new ImageDrawing(crop, new Rect(0, 0, w, h))); drawing.Freeze();
        double maxW = name is "jumpRise" or "jumpFall" ? 24 : 23;
        double maxH = name switch { "sleep" => 11, "crouch" or "landing" => 13, "jumpTuck" => 14, "sit" or "groomA" or "groomB" => 18, _ => 17 };
        double scale = Math.Min(maxW / w, maxH / h) * Sizes[coat] * 4;
        int facing = rect.TryGetProperty("facing", out var f) ? f.GetInt32() : name is "sit" or "sleep" or "groomA" or "groomB" ? -1 : 1;
        return new(drawing, w * scale, h * scale, w, h,
            rect.TryGetProperty("anchorX", out var ax) ? ax.GetDouble() : .5,
            rect.TryGetProperty("anchorY", out var ay) ? ay.GetDouble() : 1, facing);
    }
}
