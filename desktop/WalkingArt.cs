using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Gatitos;

internal static class WalkingArt
{
    private static readonly string[] Coats = { "naranja", "gris", "calico", "negro", "blanco", "tuxedo", "siames", "marron" };

    public static SpriteCatalog.Sprite[] Load(int coat)
    {
        var image = new BitmapImage(); image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad; image.DecodePixelWidth = 896;
        image.UriSource = new Uri(Path.Combine(AppContext.BaseDirectory, "assets", "walk-" + Coats[coat] + ".png"));
        image.EndInit(); image.Freeze();
        var bitmap = new FormatConvertedBitmap(image, PixelFormats.Bgra32, null, 0); bitmap.Freeze();
        int width = bitmap.PixelWidth, height = bitmap.PixelHeight;
        var pixels = new byte[width * height * 4]; bitmap.CopyPixels(pixels, width * 4, 0);
        var poses = new BitmapSource[WalkGait.FrameCount];
        var cropX0 = new int[poses.Length];
        var anchors = new double[poses.Length];
        int maxHeight = 0;
        double maxLeftExtent = 0, maxRightExtent = 0;
        for (int frame = 0; frame < poses.Length; frame++)
        {
            int col = frame % 4, row = frame / 4;
            int left = col * width / 4, right = (col + 1) * width / 4;
            int top = row * height / 2, bottom = (row + 1) * height / 2;
            double anchor = (right - left) * (800.0 / 1536.0);
            int x0 = right, y0 = bottom, x1 = left, y1 = top;
            for (int y = top; y < bottom; y++)
            for (int x = left; x < right; x++)
                if (pixels[(y * width + x) * 4 + 3] >= 32)
                { x0 = Math.Min(x0, x); y0 = Math.Min(y0, y); x1 = Math.Max(x1, x); y1 = Math.Max(y1, y); }
            if (x1 <= x0 || y1 <= y0) throw new InvalidDataException("Falta una pose de caminata: " + Coats[coat]);
            x0 = Math.Max(left, x0 - 1); y0 = Math.Max(top, y0 - 1);
            x1 = Math.Min(right - 1, x1 + 1); y1 = Math.Min(bottom - 1, y1 + 1);
            var crop = new CroppedBitmap(bitmap, new Int32Rect(x0, y0, x1 - x0 + 1, y1 - y0 + 1));
            crop.Freeze(); poses[frame] = crop;
            cropX0[frame] = x0 - left;
            anchors[frame] = anchor;
            maxHeight = Math.Max(maxHeight, crop.PixelHeight);
            maxLeftExtent = Math.Max(maxLeftExtent, anchor - cropX0[frame]);
            maxRightExtent = Math.Max(maxRightExtent, cropX0[frame] + crop.PixelWidth - anchor);
        }
        // One scale per coat preserves the proportions drawn across the full cycle.
        double fit = Math.Min(68.0 / maxHeight, Math.Min(50.0 / maxLeftExtent, 50.0 / maxRightExtent));
        var result = new SpriteCatalog.Sprite[poses.Length];
        for (int frame = 0; frame < result.Length; frame++)
        {
            var pose = poses[frame];
            double w = pose.PixelWidth * fit, h = pose.PixelHeight * fit;
            var drawing = new ImageDrawing(pose, new Rect(56 + (cropX0[frame] - anchors[frame]) * fit, 72 - h, w, h));
            drawing.Freeze();
            double scale = WalkGait.Size(coat);
            result[frame] = new(drawing, 112 * scale, 80 * scale, 112, 80, .5, .9, 1);
        }
        return result;
    }
}
