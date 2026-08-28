using System;
using System.Threading.Tasks;
using System.Windows.Input;
using TombLib.Scripting.UI.Rendering;

namespace TombLib.Scripting.UI.Bases;

public abstract partial class TextEditorBase
{
	/// <summary>
	/// Occurs when the status of the editor changes.
	/// </summary>
	public event EventHandler? StatusChanged;

	/// <summary>
	/// Raises the <see cref="StatusChanged"/> event.
	/// </summary>
	/// <param name="e">The event arguments.</param>
	protected virtual void OnStatusChanged(EventArgs e)
		=> StatusChanged?.Invoke(this, e);

	internal void RaiseStatusChanged()
		=> OnStatusChanged(EventArgs.Empty);

	/// <summary>
	/// Occurs when the zoom level of the editor changes.
	/// </summary>
	public event EventHandler? ZoomChanged;

	/// <summary>
	/// Raises the <see cref="ZoomChanged"/> event.
	/// </summary>
	/// <param name="e">The event arguments.</param>
	protected virtual void OnZoomChanged(EventArgs e)
	{
		ZoomChanged?.Invoke(this, e);
		OnStatusChanged(EventArgs.Empty);
	}

	internal void RaiseZoomChanged()
		=> OnZoomChanged(EventArgs.Empty);

	/// <summary>
	/// Occurs after the delayed text-changed interval elapses.
	/// </summary>
	public event EventHandler? TextChangedDelayed;

	/// <summary>
	/// Raises the <see cref="TextChangedDelayed"/> event.
	/// </summary>
	/// <param name="e">The event arguments.</param>
	protected virtual void OnTextChangedDelayed(EventArgs e)
		=> TextChangedDelayed?.Invoke(this, e);

	/// <summary>
	/// Occurs when a content-change worker run has completed.
	/// </summary>
	public event EventHandler? ContentChangedWorkerRunCompleted;

	/// <summary>
	/// Raises the <see cref="ContentChangedWorkerRunCompleted"/> event.
	/// </summary>
	/// <param name="e">The event arguments.</param>
	protected virtual void OnContentChangedWorkerRunCompleted(EventArgs e)
		=> ContentChangedWorkerRunCompleted?.Invoke(this, e);

	private void ContentPersistenceCoordinator_ContentChangedWorkerRunCompleted(object? sender, EventArgs e)
		=> OnContentChangedWorkerRunCompleted(EventArgs.Empty);

	private void TextArea_TextEntering(object? sender, TextCompositionEventArgs e)
	{
		CloseDefinitionToolTip(true); // Prevents the ToolTip from covering the screen while typing

		HandleAutoClosing(e);
		OnLanguageTextEntering(e);
	}

	private void TextEditor_TextEntered(object? sender, TextCompositionEventArgs e)
		=> OnLanguageTextEntered(e);

	private void TextEditor_TextChanged(object? sender, EventArgs e)
	{
		LastModified = DateTime.Now;
		IsContentChanged = _contentPersistenceCoordinator.HandleContentChanged();

		OnLanguageTextChanged(e);
	}

	private void TextEditor_KeyDown(object? sender, KeyEventArgs e)
		=> RunLanguageEventHook(() => OnLanguageKeyDown(e));

	private void TextEditor_PreviewMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
		=> RunLanguageEventHook(() => OnLanguagePreviewMouseLeftButtonDown(e));

	private void ContentPersistenceCoordinator_TextChangedDelayed(object? sender, EventArgs e)
		=> OnTextChangedDelayed(EventArgs.Empty);

	private void TextEditor_MouseHover(object? sender, MouseEventArgs e)
		=> RunLanguageEventHook(() => OnLanguageMouseHover(e));

	/// <summary>
	/// Runs an asynchronous language event hook, keeping this adapter the single <c>async void</c>
	/// boundary. Cancellation is expected when a request is superseded; unexpected exceptions are logged.
	/// </summary>
	private static async void RunLanguageEventHook(Func<Task> hook)
	{
		try
		{
			await hook().ConfigureAwait(true);
		}
		catch (OperationCanceledException)
		{ }
		catch (Exception ex)
		{
			s_logger.Error(ex, "Language event hook failed.");
		}
	}

	/// <summary>
	/// Handles mouse hover for error tooltips. Override to customize hover behavior.
	/// </summary>
	protected virtual Task HandleMouseHover(MouseEventArgs e)
	{
		HandleErrorToolTips(e);
		return Task.CompletedTask;
	}

	private void TextEditor_MouseHoverStopped(object? sender, MouseEventArgs e)
		=> ScheduleDefinitionToolTipClose();

	private void TextEditor_PreviewMouseWheel(object? sender, MouseWheelEventArgs e)
	{
		if (Keyboard.Modifiers == ModifierKeys.Control
			&& _statusCoordinator.TryHandleZoom(e.Delta, MinZoom, MaxZoom, ZoomStepSize, DefaultFontSize, fontSize => FontSize = fontSize))
		{
			e.Handled = true;
		}
	}

	private void TextEditor_MouseRightButtonDown(object? sender, MouseButtonEventArgs e)
	{
		_viewService.TryMoveCaretToMousePosition();

		ContextMenu ??= TextEditorContextMenuFactory.BuildDefault();
		ContextMenu.IsOpen = true;

		e.Handled = true;
	}

	/// <summary>
	/// Closes the definition tooltip, optionally forcing immediate close.
	/// </summary>
	/// <param name="force">Whether to close the tooltip immediately regardless of its current state.</param>
	protected void CloseDefinitionToolTip(bool force = false)
		=> _toolTipPresenter.Close(force);

	private void ScheduleDefinitionToolTipClose()
		=> _toolTipPresenter.ScheduleClose();
}
