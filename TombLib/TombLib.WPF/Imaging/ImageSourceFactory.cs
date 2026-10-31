#nullable enable

using System.Windows.Media;
using System.Windows.Media.Imaging;
using TombLib.Utils;

namespace TombLib.WPF.Imaging;

/// <summary>Creates immutable WPF image sources from editor image data.</summary>
public static class ImageSourceFactory
{
    /// <summary>Copies an image into a frozen bitmap, or returns null for an empty image.</summary>
    public static BitmapSource? FromImage(ImageC image)
    {
        if (image.Width == 0 || image.Height == 0)
            return null;

        var bitmap = BitmapSource.Create(image.Width, image.Height, 96, 96, PixelFormats.Bgra32, null, image.ToByteArray(), image.Width * 4);
        bitmap.Freeze();
        return bitmap;
    }
}
