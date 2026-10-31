using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Gatitos;

internal static class OrangeArt
{
    public static Dictionary<string, SpriteCatalog.Sprite> Load()
    {
        string source = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "orange-art-data.js"));
        using var document = JsonDocument.Parse(source.Substring(source.IndexOf('{'), source.LastIndexOf('}') - source.IndexOf('{') + 1));
        var data = document.RootElement;
        var image = new BitmapImage(); image.BeginInit(); image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(Path.Combine(AppContext.BaseDirectory, "assets", "acciones-naranja.png"));
        image.EndInit(); image.Freeze();
        var result = new Dictionary<string, SpriteCatalog.Sprite>();
        double scale = data.GetProperty("scale").GetDouble() * 4;
        foreach (var entry in data.GetProperty("frames").EnumerateObject())
        {
            var r = entry.Value;
            int x = r.GetProperty("x").GetInt32(), y = r.GetProperty("y").GetInt32();
            int w = r.GetProperty("w").GetInt32(), h = r.GetProperty("h").GetInt32();
            var crop = new CroppedBitmap(image, new Int32Rect(x, y, w, h)); crop.Freeze();
            var drawing = new ImageDrawing(crop, new Rect(0, 0, w, h)); drawing.Freeze();
            result.Add(entry.Name, new(drawing, w * scale, h * scale, w, h,
                r.GetProperty("anchorX").GetDouble(), r.GetProperty("anchorY").GetDouble(), 1));
        }
        return result;
    }
}
