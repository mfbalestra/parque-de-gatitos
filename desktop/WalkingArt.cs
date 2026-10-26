using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Gatitos;

internal static class WalkingArt
{
    private static readonly string[] Coats = { "naranja", "gris", "calico", "negro", "blanco", "tuxedo", "siames", "marron" };
    // These regions separate the five pieces in the generated rig sheets. Alpha
    // bounds only remove empty margins; the original illustrations are untouched.
    public static SpriteCatalog.Sprite[] Load(int coat)
    {
        var image = new BitmapImage(); image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad; image.DecodePixelWidth = 768;
        image.UriSource = new Uri(Path.Combine(AppContext.BaseDirectory, "assets", "walk-" + Coats[coat] + ".png"));
        image.EndInit(); image.Freeze();
        var bitmap = new FormatConvertedBitmap(image, PixelFormats.Bgra32, null, 0); bitmap.Freeze();
        int width = bitmap.PixelWidth, height = bitmap.PixelHeight, split = (int)(height * .56);
        var pixels = new byte[width * height * 4]; bitmap.CopyPixels(pixels, width * 4, 0);
        BitmapSource Piece(int left, int top, int right, int bottom)
        {
            int x0 = right, y0 = bottom, x1 = left, y1 = top;
            for (int y = top; y < bottom; y++)
            for (int x = left; x < right; x++)
                if (pixels[(y * width + x) * 4 + 3] >= 32)
                { x0 = Math.Min(x0, x); y0 = Math.Min(y0, y); x1 = Math.Max(x1, x); y1 = Math.Max(y1, y); }
            if (x1 <= x0 || y1 <= y0) throw new InvalidDataException("Falta una pieza de caminata: " + Coats[coat]);
            x0 = Math.Max(left, x0 - 1); y0 = Math.Max(top, y0 - 1);
            x1 = Math.Min(right - 1, x1 + 1); y1 = Math.Min(bottom - 1, y1 + 1);
            var crop = new CroppedBitmap(bitmap, new Int32Rect(x0, y0, x1 - x0 + 1, y1 - y0 + 1)); crop.Freeze(); return crop;
        }
        var body = Piece(0, 0, width, split);
        int[] columns = { 0, (int)(width * .29), width / 2, (int)(width * .73), width };
        var legs = new BitmapSource[4];
        for (int i = 0; i < 4; i++) legs[i] = Piece(columns[i], split, columns[i + 1], height);
        var result = new SpriteCatalog.Sprite[WalkGait.FrameCount];
        for (int frame = 0; frame < result.Length; frame++)
        {
            var drawing = new DrawingGroup();
            using (var dc = drawing.Open())
            {
                // Far limbs sit behind the body. Near thighs/shoulders cover the
                // belly outline, so their overlapping caps connect to the torso.
                double[] offsets = { .5, .75, 0, .25 };
                for (int leg = 0; leg < 4; leg++)
                {
                    if (leg == 2) dc.DrawImage(body, new Rect(10, 4, 92, 50));
                    bool front = leg % 2 == 1, far = leg < 2;
                    double rootX = (front ? 77 : 35) + (far ? 3 : 0), top = far ? 39 : 32;
                    var paw = WalkGait.Paw(frame / 8.0 + offsets[leg]);
                    double footX = rootX + paw.X, footY = 72 - paw.Lift;
                    double legWidth = front ? 16 : 23;
                    var source = legs[leg];
                    var transform = new MatrixTransform(new Matrix(legWidth / source.PixelWidth, 0,
                        (footX - rootX) / source.PixelHeight, (footY - top) / source.PixelHeight,
                        rootX - legWidth / 2, top));
                    dc.PushTransform(transform);
                    if (!far)
                    {
                        // Blend the upper cap into the coat instead of leaving a
                        // dark closed outline that reads as a detachable joint.
                        var blend = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(0, 1) };
                        blend.GradientStops.Add(new GradientStop(Colors.Transparent, 0));
                        blend.GradientStops.Add(new GradientStop(Colors.Transparent, .2));
                        blend.GradientStops.Add(new GradientStop(Colors.Black, .55));
                        blend.GradientStops.Add(new GradientStop(Colors.Black, 1)); blend.Freeze();
                        dc.PushOpacityMask(blend);
                    }
                    dc.DrawImage(source, new Rect(0, 0, source.PixelWidth, source.PixelHeight));
                    if (!far) dc.Pop();
                    dc.Pop();
                }
            }
            drawing.Freeze();
            double scale = WalkGait.Size(coat);
            result[frame] = new(drawing, 112 * scale, 80 * scale, 112, 80, .5, .9, 1);
        }
        return result;
    }
}
