using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TombLib.Scripting.UI.Navigation;

/// <summary>
/// Handles editor-side definition navigation triggers such as F12 and Ctrl+Click according to the configured
/// <see cref="TextDefinitionNavigationGestures"/>. Disposal signals cancellation to in-flight navigation and
/// prevents further navigation; a disposed controller must not be reused.
/// </summary>
/// <remarks>
/// <para>
/// By default the controller handles F12 pressed without any modifier keys and a single Ctrl+LeftClick. Hosts
/// select the gestures they want through the constructor. An event that is already handled is ignored.
/// </para>
/// <para>
/// The F12 gesture reads the modifiers from the routed key event's keyboard device. The Ctrl+Click gesture uses
/// the configured modifier source, which defaults to the global <see cref="Keyboard.Modifiers"/> because WPF
/// mouse events carry no event-time keyboard state; hosts can supply their own source for tests or synthetic
/// input.
/// </para>
/// <para>
/// The controller must be created and used on the thread that owns the owner element - for editor
/// hosts, the editor thread - because it resolves pointer positions against the element and marks routed events
/// while handling them. The navigation callback itself may be asynchronous; disposal signals its cancellation
/// token.
/// </para>
/// </remarks>
public sealed class TextDefinitionTriggerController : IDisposable
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private readonly FrameworkElement _owner;
	private readonly Func<Point, int> _getOffsetFromPoint;
	private readonly Func<int, CancellationToken, Task<bool>> _tryNavigateAsync;
	private readonly Func<ModifierKeys> _getModifiers;
	private readonly TextDefinitionNavigationGestures _gestures;
	private readonly CancellationTokenSource _disposalCancellation = new();
	private bool _isDisposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="TextDefinitionTriggerController"/> class.
	/// </summary>
	/// <param name="owner">The element pointer positions are resolved against.</param>
	/// <param name="getOffsetFromPoint">Resolves the zero-based document offset for a point in the owner.</param>
	/// <param name="tryNavigateAsync">Tries to resolve and navigate to a definition asynchronously.</param>
	/// <param name="gestures">The input gestures to handle; defaults to <see cref="TextDefinitionNavigationGestures.Default"/>.</param>
	/// <param name="getModifiers">
	/// Returns the currently pressed modifier keys for the pointer gestures, or <see langword="null"/> to read the
	/// global <see cref="Keyboard.Modifiers"/>.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="owner"/>, <paramref name="getOffsetFromPoint"/>, or <paramref name="tryNavigateAsync"/> is
	/// <see langword="null"/>.
	/// </exception>
	public TextDefinitionTriggerController(
		FrameworkElement owner,
		Func<Point, int> getOffsetFromPoint,
		Func<int, CancellationToken, Task<bool>> tryNavigateAsync,
		TextDefinitionNavigationGestures gestures = TextDefinitionNavigationGestures.Default,
		Func<ModifierKeys>? getModifiers = null)
	{
		ArgumentNullException.ThrowIfNull(owner);
		ArgumentNullException.ThrowIfNull(getOffsetFromPoint);
		ArgumentNullException.ThrowIfNull(tryNavigateAsync);

		_owner = owner;
		_getOffsetFromPoint = getOffsetFromPoint;
		_tryNavigateAsync = tryNavigateAsync;
		_getModifiers = getModifiers ?? (static () => Keyboard.Modifiers);
		_gestures = gestures;
	}

	/// <summary>
	/// Handles an F12 key press without modifiers for definition navigation, when the F12 gesture is enabled.
	/// </summary>
	/// <param name="e">The key event to inspect.</param>
	/// <param name="caretOffset">The zero-based document caret offset.</param>
	/// <param name="cancellationToken">An optional caller cancellation token.</param>
	/// <returns><see langword="true"/> when the input was handled as a navigation; otherwise, <see langword="false"/>.</returns>
	/// <remarks>
	/// F12 combined with any modifier key is left to the host; the modifiers are read from the event's keyboard
	/// device. The event is marked handled before navigation is awaited so WPF routing does not pass the key on to
	/// further handlers; when navigation does not complete (failure or disposal), the handled flag is rolled back.
	/// Because routing has already finished by then, the rollback only corrects the flag for later readers.
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="e"/> is <see langword="null"/>.</exception>
	public async Task<bool> TryHandleKeyDownAsync(KeyEventArgs e, int caretOffset, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(e);

		if (_isDisposed
			|| (_gestures & TextDefinitionNavigationGestures.F12) == 0
			|| e.Handled
			|| e.Key != Key.F12
			|| e.KeyboardDevice.Modifiers != ModifierKeys.None)
		{
			return false;
		}

		using CancellationTokenSource linkedCancellation =
			CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _disposalCancellation.Token);

		e.Handled = true;

		if (!await NavigateWithLoggingAsync(caretOffset, linkedCancellation.Token).ConfigureAwait(true) || _isDisposed)
		{
			e.Handled = false;
			return false;
		}

		return true;
	}

	/// <summary>
	/// Handles a single Ctrl+LeftClick definition navigation attempt, when the Ctrl+Click gesture is enabled.
	/// Double-clicks and other buttons are left to the host.
	/// </summary>
	/// <param name="e">The mouse event to inspect.</param>
	/// <param name="cancellationToken">An optional caller cancellation token.</param>
	/// <returns><see langword="true"/> when the input was handled as a navigation; otherwise, <see langword="false"/>.</returns>
	/// <remarks>
	/// The Ctrl state is read from the configured modifier source, which defaults to the global
	/// <see cref="Keyboard.Modifiers"/> because WPF mouse events carry no event-time keyboard state. The event is
	/// marked handled before navigation is awaited so WPF routing does not perform the default single-click
	/// behavior while the navigation runs; when navigation does not complete (failure or disposal), the handled
	/// flag is rolled back. Because routing has already finished by then, the rollback only corrects the flag for
	/// later readers.
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="e"/> is <see langword="null"/>.</exception>
	public async Task<bool> TryHandlePointerNavigationAsync(MouseButtonEventArgs e, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(e);

		if (_isDisposed
			|| (_gestures & TextDefinitionNavigationGestures.ControlClick) == 0
			|| e.Handled
			|| e.ClickCount > 1
			|| (_getModifiers() & ModifierKeys.Control) == 0
			|| e.ChangedButton != MouseButton.Left)
		{
			return false;
		}

		int hoveredOffset = _getOffsetFromPoint(e.GetPosition(_owner));

		if (hoveredOffset == -1)
			return false;

		using CancellationTokenSource linkedCancellation =
			CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _disposalCancellation.Token);

		e.Handled = true;

		if (!await NavigateWithLoggingAsync(hoveredOffset, linkedCancellation.Token).ConfigureAwait(true) || _isDisposed)
		{
			e.Handled = false;
			return false;
		}

		return true;
	}

	/// <summary>
	/// Signals cancellation to any in-flight navigation and prevents further navigation. Disposal is
	/// idempotent; a disposed controller must not be reused.
	/// </summary>
	public void Dispose()
	{
		if (_isDisposed)
			return;

		_isDisposed = true;
		_disposalCancellation.Cancel();
		_disposalCancellation.Dispose();
	}

	// Definition navigation is user-triggered, so provider failures are converted to a logged
	// unsuccessful result at the event boundary.
	private async Task<bool> NavigateWithLoggingAsync(int offset, CancellationToken cancellationToken)
	{
		try
		{
			return await _tryNavigateAsync(offset, cancellationToken).ConfigureAwait(true);
		}
		catch (OperationCanceledException)
		{
			return false;
		}
		catch (Exception exception)
		{
			Log.Error(exception, "Definition navigation failed.");
			return false;
		}
	}
}
