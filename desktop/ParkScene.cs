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
    private readonly Pen outline = new(VisualStyle.Outline, .8);
    private readonly Geometry antennae = Geometry.Parse("M-0.6,-3 Q-2,-7 -4,-7 M0.6,-3 Q2,-7 4,-7");
    public ParkScene() { wing.Freeze(); antennae.Freeze(); outline.Freeze(); }
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
                dc.DrawGeometry(VisualStyle.Butterflies[i % VisualStyle.Butterflies.Length], outline, wing);
                dc.DrawEllipse(VisualStyle.Cream, null, new Point(11, -5), 2.2, 1.7);
                dc.DrawEllipse(VisualStyle.Cream, null, new Point(7, 7), 1.4, 1); dc.Pop();
            }
            dc.DrawGeometry(null, outline, antennae);
            dc.DrawEllipse(VisualStyle.Outline, null, new Point(0, 1), 1.1, 5);
            dc.DrawEllipse(VisualStyle.Outline, null, new Point(0, -3.5), 1.6, 1.6); dc.Pop();
        }
        dc.Pop();
    }
}
