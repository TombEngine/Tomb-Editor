using ICSharpCode.AvalonEdit.Editing;
using System;

namespace TombLib.Scripting.UI.Editors;

/// <summary>
/// Coordinates caret and selection notifications and maintains zoom state for an AvalonEdit <see cref="TextArea"/>.
/// </summary>
/// <remarks>
/// The coordinator is not thread-safe: use it from the text area's owner thread, where its callbacks are raised.
/// A selection change also moves the caret, so the status callback can run twice for one input event
/// (once from the caret notification and once from the selection notification); the host callbacks should
/// be idempotent.
/// </remarks>
internal sealed class TextEditorViewStateCoordinator : IDisposable
{
	private readonly Action _raiseStatusChanged;
	private readonly Action _raiseZoomChanged;

	private readonly TextArea _textArea;

	private bool _attached;
	private bool _disposed;

	private int _zoomPercent = 100;

	/// <summary>
	/// Associates a text area with callbacks for status and zoom changes.
	/// </summary>
	/// <param name="textArea">The text area to observe after <see cref="Attach"/> is called.</param>
	/// <param name="raiseStatusChanged">The callback invoked when the observed caret position or selection changes.</param>
	/// <param name="raiseZoomChanged">The callback invoked after a zoom change is applied.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="textArea"/>, <paramref name="raiseStatusChanged"/>, or <paramref name="raiseZoomChanged"/> is <see langword="null"/>.
	/// </exception>
	public TextEditorViewStateCoordinator(TextArea textArea, Action raiseStatusChanged, Action raiseZoomChanged)
	{
		ArgumentNullException.ThrowIfNull(textArea);
		ArgumentNullException.ThrowIfNull(raiseStatusChanged);
		ArgumentNullException.ThrowIfNull(raiseZoomChanged);

		_textArea = textArea;
		_raiseStatusChanged = raiseStatusChanged;
		_raiseZoomChanged = raiseZoomChanged;
	}

	/// <summary>
	/// Gets or sets the stored zoom percentage.
	/// </summary>
	/// <remarks>
	/// Direct assignment does not apply a font size or invoke the zoom callback, and it does not clamp the
	/// value against the host's bounds. A later <see cref="TryApplyZoomStep"/> call clamps the stored value
	/// into its supplied bounds when it computes a step, and an applied step replaces the stored value
	/// with the clamped and stepped result.
	/// </remarks>
	/// <exception cref="ArgumentOutOfRangeException">The assigned value is less than or equal to zero.</exception>
	public int ZoomPercent
	{
		get => _zoomPercent;
		set
		{
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
			_zoomPercent = value;
		}
	}

	/// <summary>
	/// Subscribes to the text area's caret and selection change events.
	/// </summary>
	/// <remarks>
	/// Repeated calls before disposal have no effect. There is no detach operation: the host disposes
	/// the coordinator when it unloads or replaces the editor and creates a new one.
	/// Attaching does not raise the status callback, so the host publishes the current caret and
	/// selection state itself when the text area is attached.
	/// </remarks>
	/// <exception cref="ObjectDisposedException">The coordinator has been disposed.</exception>
	public void Attach()
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		if (_attached)
			return;

		_attached = true;

		_textArea.Caret.PositionChanged += TextArea_PositionChanged;
		_textArea.SelectionChanged += TextArea_SelectionChanged;
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;

		if (!_attached)
			return;

		_attached = false;

		_textArea.Caret.PositionChanged -= TextArea_PositionChanged;
		_textArea.SelectionChanged -= TextArea_SelectionChanged;
	}

	/// <summary>
	/// Tries to adjust the stored zoom by one step toward the relevant supplied bound and applies the corresponding font size.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The stored zoom is clamped into the inclusive
	/// [<see cref="ZoomOptions.MinZoom"/>, <see cref="ZoomOptions.MaxZoom"/>] range before the step is
	/// computed, so a value that direct assignment to <see cref="ZoomPercent"/> placed outside the range
	/// does not prevent a step back inside it. When a step is applied, the clamped and stepped value
	/// replaces the stored value; a rejected step (already at the relevant bound, or a
	/// <paramref name="delta"/> of <c>0</c>) leaves the stored value unchanged.
	/// </para>
	/// <para>
	/// Positive deltas move toward <see cref="ZoomOptions.MaxZoom"/> and negative deltas toward
	/// <see cref="ZoomOptions.MinZoom"/>, stopping at the bound.
	/// </para>
	/// <para>
	/// The font-size callback runs before the stored zoom changes, so a callback that throws leaves the stored
	/// zoom unchanged. The zoom-changed callback runs after the stored zoom changes, so it observes the new value.
	/// </para>
	/// </remarks>
	/// <param name="delta">
	/// The direction of the step: positive to zoom in, negative to zoom out,
	/// or <c>0</c> to make no change. Only the sign is used.
	/// </param>
	/// <param name="options">The zoom bounds, step size, and font size at <c>100</c>% zoom.</param>
	/// <param name="applyFontSize">The callback that receives the scaled font size.</param>
	/// <returns>
	/// <see langword="true"/> when the zoom changes and the font-size and zoom-change callbacks are invoked;
	/// otherwise, <see langword="false"/>.
	/// </returns>
	/// <exception cref="ObjectDisposedException">The coordinator has been disposed.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="applyFontSize"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <see cref="ZoomOptions.MinZoom"/> is not positive or is greater than <see cref="ZoomOptions.MaxZoom"/>,
	/// <see cref="ZoomOptions.StepSize"/> is not positive, or
	/// <see cref="ZoomOptions.ReferenceFontSize"/> is not a finite positive number.
	/// </exception>
	public bool TryApplyZoomStep(int delta, ZoomOptions options, Action<double> applyFontSize)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ArgumentNullException.ThrowIfNull(applyFontSize);

		ValidateZoomOptions(options);

		int currentZoom = Math.Clamp(ZoomPercent, options.MinZoom, options.MaxZoom);
		int nextZoom;

		if (delta > 0)
		{
			if (currentZoom >= options.MaxZoom)
				return false;

			// The headroom is computed before the step is added, so a large step size cannot overflow.
			nextZoom = options.StepSize > options.MaxZoom - currentZoom ? options.MaxZoom : currentZoom + options.StepSize;
		}
		else if (delta < 0)
		{
			if (currentZoom <= options.MinZoom)
				return false;

			nextZoom = options.StepSize > currentZoom - options.MinZoom ? options.MinZoom : currentZoom - options.StepSize;
		}
		else
		{
			return false;
		}

		// The font size is applied first so a throwing callback leaves the stored zoom unchanged.
		// The zoom is divided before multiplying so a large reference font size cannot overflow the
		// intermediate product to infinity.
		applyFontSize(options.ReferenceFontSize * (nextZoom / 100.0));

		ZoomPercent = nextZoom;

		_raiseZoomChanged();
		return true;
	}

	private static void ValidateZoomOptions(ZoomOptions options)
	{
		if (options.MinZoom <= 0)
			throw new ArgumentOutOfRangeException(nameof(options), options.MinZoom, "The minimum zoom must be positive.");

		if (options.MaxZoom < options.MinZoom)
			throw new ArgumentOutOfRangeException(nameof(options), options.MaxZoom, "The maximum zoom must not be less than the minimum zoom.");

		if (options.StepSize <= 0)
			throw new ArgumentOutOfRangeException(nameof(options), options.StepSize, "The zoom step size must be positive.");

		if (!double.IsFinite(options.ReferenceFontSize) || options.ReferenceFontSize <= 0)
			throw new ArgumentOutOfRangeException(nameof(options), options.ReferenceFontSize, "The reference font size must be a finite positive number.");
	}

	private void TextArea_PositionChanged(object? sender, EventArgs e)
		=> _raiseStatusChanged();

	private void TextArea_SelectionChanged(object? sender, EventArgs e)
		=> _raiseStatusChanged();
}
