using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Gatitos;

internal sealed class ParkScene
{
    private BitmapSource? background;
    private readonly Geometry wing = Geometry.Parse("M0,0 C2,-11 17,-15 18,-5 C18,0 12,2 8,3 C16,6 12,14 5,10 C1,8 0,3 0,0Z");
    private readonly Brush[] colours = { Brushes.PaleVioletRed, Brushes.Plum, Brushes.Goldenrod, Brushes.LightSkyBlue };
    private readonly Pen outline = new(Brushes.SaddleBrown, .8);
    public ParkScene() { wing.Freeze(); outline.Freeze(); }
    public void Load()
    {
        if (background != null) return;
        var image = new BitmapImage(); image.BeginInit(); image.CacheOption = BitmapCacheOption.OnLoad;
        image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
        image.UriSource = new Uri(Path.Combine(AppContext.BaseDirectory, "assets", "parque-windows.png"));
        image.EndInit(); image.Freeze(); background = image;
    }
    public void Release() { background = null; }
    public void Draw(DrawingContext dc, double width, double height, double opacity, double time)
    {
        if (background == null || opacity <= 0) return;
        dc.PushOpacity(opacity);
        dc.DrawImage(background, new Rect(0, 0, width, height));
        for (int i = 0; i < 5; i++)
        {
            double x = width * (.16 + i * .15) + Math.Sin(time * .37 + i * 2) * 48;
            double y = height * (.73 + .055 * Math.Sin(time * .5 + i));
            double flutter = .25 + .75 * (1 + Math.Sin(time * 17 + i * 2)) / 2;
            dc.PushTransform(new TranslateTransform(x, y));
            for (int side = -1; side <= 1; side += 2)
            {
                dc.PushTransform(new ScaleTransform(side * flutter * .6, .6));
                dc.DrawGeometry(colours[i % colours.Length], outline, wing); dc.Pop();
            }
            dc.DrawEllipse(Brushes.SaddleBrown, null, new Point(0, 1), 1.1, 5); dc.Pop();
        }
        dc.Pop();
    }
}
