using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace TombLib.Scripting.UI.Presentation;

internal sealed class EditorToolTipPresenter : IDisposable
{
	private static readonly TimeSpan CloseDelay = TimeSpan.FromMilliseconds(900.0);

	private readonly FrameworkElement _owner;
	private readonly DispatcherTimer _closeTimer = new();
	private bool _contentHovered;
	private bool _isDisposed;

	public EditorToolTipPresenter(FrameworkElement owner)
	{
		_owner = owner;

		(Popup, Border, ContentPresenter) = PopupShell.Create();

		Border.MouseEnter += Border_MouseEnter;
		Border.MouseLeave += Border_MouseLeave;

		_closeTimer.Interval = CloseDelay;
		_closeTimer.Tick += CloseTimer_Tick;
	}

	public Popup Popup { get; }

	public Border Border { get; }

	public ContentPresenter ContentPresenter { get; }

	public void Show(object content, Brush borderBrush, Brush background)
	{
		_closeTimer.Stop();
		_contentHovered = false;
		Popup.PlacementTarget = _owner;

		Point mousePosition = Mouse.GetPosition(_owner);
		Popup.HorizontalOffset = mousePosition.X + 14.0;
		Popup.VerticalOffset = mousePosition.Y + 20.0;

		Border.BorderBrush = borderBrush;
		Border.Background = background;
		ContentPresenter.Content = content;
		Popup.IsOpen = true;
	}

	public void Close(bool force = false)
	{
		_closeTimer.Stop();

		if (!force && (_contentHovered || Border.IsMouseOver))
			return;

		if (Popup.IsOpen)
			Popup.IsOpen = false;

		ContentPresenter.Content = null;
		_contentHovered = false;
	}

	public void ScheduleClose()
	{
		if (!Popup.IsOpen)
			return;

		_closeTimer.Stop();
		_closeTimer.Start();
	}

	/// <summary>
	/// Stops the close timer, detaches popup handlers and force-closes the popup.
	/// </summary>
	public void Dispose()
	{
		if (_isDisposed)
			return;

		_isDisposed = true;
		_closeTimer.Stop();
		_closeTimer.Tick -= CloseTimer_Tick;
		Border.MouseEnter -= Border_MouseEnter;
		Border.MouseLeave -= Border_MouseLeave;
		Close(force: true);
	}

	private void CloseTimer_Tick(object? sender, EventArgs e)
	{
		_closeTimer.Stop();

		if (!_contentHovered && !Border.IsMouseOver)
			Close(true);
	}

	private void Border_MouseEnter(object sender, MouseEventArgs e)
	{
		_contentHovered = true;
		_closeTimer.Stop();
	}

	private void Border_MouseLeave(object sender, MouseEventArgs e)
	{
		_contentHovered = false;
		ScheduleClose();
	}
}
