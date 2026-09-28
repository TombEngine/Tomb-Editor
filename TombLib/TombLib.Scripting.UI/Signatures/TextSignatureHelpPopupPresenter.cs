using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Presentation;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.UI.Signatures;

/// <summary>
/// Presents signature help content in a caret-anchored popup for editors that use popup-based signature help.
/// </summary>
public sealed class TextSignatureHelpPopupPresenter
{
	private readonly TextEditorBase _editor;
	private readonly Action? _prepareForShow;
	private readonly Popup _popup = new();
	private readonly Border _popupBorder = new();
	private readonly ContentPresenter _popupPresenter = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="TextSignatureHelpPopupPresenter"/> class.
	/// </summary>
	/// <param name="editor">The editor the popup is anchored to.</param>
	/// <param name="prepareForShow">An optional callback invoked before the popup is shown.</param>
	public TextSignatureHelpPopupPresenter(TextEditorBase editor, Action? prepareForShow = null)
	{
		ArgumentNullException.ThrowIfNull(editor);
		_editor = editor;
		_prepareForShow = prepareForShow;

		(_popup, _popupBorder, _popupPresenter) = PopupShell.Create();

		_popupBorder.BorderBrush = TextEditorColorPalette.ToolTipBorder;
		_popupBorder.Background = TextEditorColorPalette.ToolTipBackground;
	}

	/// <summary>
	/// Gets whether the popup is currently open.
	/// </summary>
	public bool IsOpen => _popup.IsOpen;

	/// <summary>
	/// Closes the popup and clears its content.
	/// </summary>
	public void Close()
	{
		if (_popup.IsOpen)
			_popup.IsOpen = false;

		_popupPresenter.Content = null;
	}

	/// <summary>
	/// Releases the resources used by the presenter by closing the popup and clearing its content.
	/// </summary>
	public void Dispose()
		=> Close();

	/// <summary>
	/// Shows the popup with content built for the available width.
	/// </summary>
	/// <param name="createContent">The callback that builds the popup content for a given width.</param>
	/// <param name="maxPopupWidth">The maximum popup width.</param>
	public void Show(Func<double, FrameworkElement> createContent, double maxPopupWidth = ToolTipDefaults.PopupMaxWidth)
	{
		ArgumentNullException.ThrowIfNull(createContent);

		double availablePopupWidth = Math.Max(0.0, Math.Min(maxPopupWidth, _editor.ActualWidth - 16.0));

		double popupHorizontalPadding = _popupBorder.Padding.Left
			+ _popupBorder.Padding.Right
			+ _popupBorder.BorderThickness.Left
			+ _popupBorder.BorderThickness.Right;

		double popupVerticalPadding = _popupBorder.Padding.Top
			+ _popupBorder.Padding.Bottom
			+ _popupBorder.BorderThickness.Top
			+ _popupBorder.BorderThickness.Bottom;

		double contentMaxWidth = Math.Max(0.0, availablePopupWidth - popupHorizontalPadding);
		FrameworkElement content = createContent(contentMaxWidth);
		content.Measure(new Size(contentMaxWidth, double.PositiveInfinity));

		Size popupSize = new(
			Math.Min(availablePopupWidth, content.DesiredSize.Width + popupHorizontalPadding),
			content.DesiredSize.Height + popupVerticalPadding);

		_popupPresenter.Content = content;
		_popupPresenter.InvalidateMeasure();
		_popupBorder.InvalidateMeasure();

		PrepareAndPositionPopup(popupSize);

		if (!_popup.IsOpen)
			_popup.IsOpen = true;
	}

	private void PrepareAndPositionPopup(Size popupSize)
	{
		_prepareForShow?.Invoke();
		_popup.PlacementTarget = _editor;
		_editor.TextArea.TextView.EnsureVisualLines();

		Rect caretRectangle = _editor.TextArea.Caret.CalculateCaretRectangle();
		Vector scrollOffset = _editor.TextArea.TextView.ScrollOffset;

		Point caretViewportPoint = new(
			caretRectangle.X - scrollOffset.X,
			caretRectangle.Y - scrollOffset.Y);

		Point editorPoint = _editor.TextArea.TextView.TranslatePoint(caretViewportPoint, _editor);
		double lineHeight = Math.Max(_editor.TextArea.TextView.DefaultLineHeight, caretRectangle.Height);
		double lineSlack = Math.Max(0.0, lineHeight - caretRectangle.Height);
		double horizontalOffset = Math.Max(0.0, editorPoint.X + 2.0);
		double maxHorizontalOffset = Math.Max(0.0, _editor.ActualWidth - popupSize.Width - 8.0);
		horizontalOffset = Math.Min(horizontalOffset, maxHorizontalOffset);

		double verticalOffset = editorPoint.Y - popupSize.Height - lineSlack - 8.0;

		if (verticalOffset < 0.0)
			verticalOffset = Math.Min(Math.Max(0.0, _editor.ActualHeight - popupSize.Height), editorPoint.Y + lineHeight + 4.0);

		_popup.HorizontalOffset = horizontalOffset;
		_popup.VerticalOffset = verticalOffset;
	}
}
