using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TombLib.Scripting.UI.Presentation;

/// <summary>
/// Builds the shared WPF shell (popup, bordered content presenter) used by editor
/// tooltips and signature help popups.
/// </summary>
internal static class PopupShell
{
	/// <summary>
	/// Creates a caret-anchorable popup whose child is a bordered content presenter.
	/// </summary>
	public static (Popup Popup, Border Border, ContentPresenter ContentPresenter) Create()
	{
		var popup = new Popup
		{
			AllowsTransparency = true,
			PopupAnimation = PopupAnimation.None,
			StaysOpen = true,
			Placement = PlacementMode.RelativePoint
		};

		var contentPresenter = new ContentPresenter();
		var border = new Border
		{
			SnapsToDevicePixels = true,
			CornerRadius = new CornerRadius(3.0),
			BorderThickness = new Thickness(1.0),
			Padding = new Thickness(8.0, 6.0, 8.0, 6.0)
		};

		border.Child = contentPresenter;
		popup.Child = border;

		return (popup, border, contentPresenter);
	}
}
