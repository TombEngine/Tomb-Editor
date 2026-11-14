#nullable enable

using System.Windows.Media;
using TombEditor.Controls;

namespace TombEditor.Features.Dialogs.TexturePreview;

/// <summary>
/// Texture map view used by <see cref="TexturePreviewWindow"/>. Disables the
/// selection overlay because the preview pop-up is read-only.
/// </summary>
public sealed class TexturePreviewMapView : WpfTextureMapView
{
	protected override void OnPaintSelection(DrawingContext drawingContext)
	{
		// No-op: preview is informational only.
	}
}
