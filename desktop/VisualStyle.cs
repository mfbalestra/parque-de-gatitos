using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Media;

namespace Gatitos;

internal static class VisualStyle
{
    private static readonly JsonDocument palette = Load();
    public static readonly SolidColorBrush Outline = Brush("outline");
    public static readonly SolidColorBrush Paper = Brush("paper");
    public static readonly SolidColorBrush Ink = Brush("ink");
    public static readonly SolidColorBrush Accent = Brush("accent");
    public static readonly SolidColorBrush ControlEdge = Brush("controlEdge");
    public static readonly SolidColorBrush Cream = Brush("cream");
    public static readonly SolidColorBrush[] Butterflies = palette.RootElement.GetProperty("butterflies")
        .EnumerateArray().Select(value => FrozenBrush(value.GetString()!)).ToArray();
    public static readonly RadialGradientBrush Shadow = MakeShadow();

    private static JsonDocument Load()
    {
        string source = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "visual-style.js"));
        return JsonDocument.Parse(source.Substring(source.IndexOf('{'), source.LastIndexOf('}') - source.IndexOf('{') + 1));
    }
    private static SolidColorBrush Brush(string name) => FrozenBrush(palette.RootElement.GetProperty(name).GetString()!);
    private static SolidColorBrush FrozenBrush(string value)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
        brush.Freeze(); return brush;
    }
    private static RadialGradientBrush MakeShadow()
    {
        var color = (Color)ColorConverter.ConvertFromString(palette.RootElement.GetProperty("shadow").GetString()!);
        color.A = 45;
        var brush = new RadialGradientBrush(color, Colors.Transparent); brush.Freeze(); return brush;
    }
}
