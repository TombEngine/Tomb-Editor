#nullable enable

using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TombIDE.ScriptingStudio.CommandSurface;

namespace TombIDE.ScriptingStudio.ToolStrips;

internal static class WpfImageSourceFactory
{
	public static ImageSource? Create(string resourceKey)
	{
		if (string.IsNullOrWhiteSpace(resourceKey))
			return null;

		if (StudioCommandSurfaceResources.FindImageInResources(resourceKey) is not Image image)
			return null;

		using var bitmap = new Bitmap(image);
		IntPtr handle = bitmap.GetHbitmap();

		try
		{
			BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(
				handle,
				IntPtr.Zero,
				Int32Rect.Empty,
				BitmapSizeOptions.FromEmptyOptions());

			source.Freeze();
			return source;
		}
		finally
		{
			NativeMethods.DeleteObject(handle);
		}
	}
}
