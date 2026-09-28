#nullable enable

using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using Nickelony.IDEKit.Workspace.Documents;

namespace TombIDE.ScriptingStudio.TextEditing;

/// <summary>
/// Determines whether a leased editor view remains open when its session ends.
/// </summary>
public enum EditorSessionMode
{
	/// <summary>The leased view stays open after the session ends.</summary>
	Persistent,

	/// <summary>A view that a transient session opened is closed when the session ends.</summary>
	Transient
}

/// <summary>
/// Configures how an editor view is opened through an <see cref="IEditorViewHost"/>.
/// </summary>
/// <remarks>A persistent session never closes the view merely because the session is disposed.</remarks>
/// <param name="Mode">The mode that governs whether the leased view stays open when the session ends.</param>
public readonly record struct EditorSessionOptions(
	EditorSessionMode Mode = EditorSessionMode.Persistent);

/// <summary>
/// Represents a leased editor view that must be released before the caller finishes.
/// </summary>
/// <remarks>
/// Disposing a session is idempotent. Disposal restores the previously active editor view; a
/// transient session closes the view only when this session opened it.
/// </remarks>
public interface IEditorSession : IDisposable
{
	/// <summary>Gets the workspace document key of the leased view.</summary>
	WorkspaceDocumentKey DocumentKey { get; }

	/// <summary>Gets the workspace document identifier of the leased view.</summary>
	string DocumentId { get; }

	/// <summary>Gets the mode that governs how the view is released.</summary>
	EditorSessionMode Mode { get; }

	/// <summary>Gets a value indicating whether the session has not yet been disposed.</summary>
	bool IsActive { get; }
}

/// <summary>
/// Describes how an editor view attach operation completed.
/// </summary>
public enum EditorSessionOpenStatus
{
	/// <summary>A new editor view was opened and leased.</summary>
	Opened,

	/// <summary>An existing editor view was leased without opening a new one.</summary>
	AlreadyOpen,

	/// <summary>No editor view could be produced.</summary>
	Unavailable
}

/// <summary>
/// Contains the outcome of attaching an editor view through an <see cref="IEditorViewHost"/>.
/// </summary>
/// <remarks>
/// Use the static factories, which produce the three well-formed outcomes so combinations that
/// contradict a status cannot be constructed: <see cref="Opened"/>, <see cref="Reused"/>, and
/// <see cref="Unavailable"/>.
/// </remarks>
public sealed class EditorSessionOpenResult
{
	private EditorSessionOpenResult(EditorSessionOpenStatus status, IEditorSession? session)
	{
		Status = status;
		Session = session;
	}

	/// <summary>Gets the outcome in which this caller opened a new editor view and leased it.</summary>
	/// <param name="session">The session for the opened view.</param>
	/// <returns>An opened result carrying the session.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="session"/> is <see langword="null"/>.</exception>
	public static EditorSessionOpenResult Opened(IEditorSession session)
		=> new(EditorSessionOpenStatus.Opened, session ?? throw new ArgumentNullException(nameof(session)));

	/// <summary>Gets the outcome in which an existing editor view was leased without opening a new one.</summary>
	/// <param name="session">The session for the existing view.</param>
	/// <returns>An already-open result carrying the session.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="session"/> is <see langword="null"/>.</exception>
	public static EditorSessionOpenResult Reused(IEditorSession session)
		=> new(EditorSessionOpenStatus.AlreadyOpen, session ?? throw new ArgumentNullException(nameof(session)));

	/// <summary>Gets the outcome in which no editor view could be produced.</summary>
	public static EditorSessionOpenResult Unavailable { get; } =
		new(EditorSessionOpenStatus.Unavailable, null);

	/// <summary>Gets the attach outcome.</summary>
	public EditorSessionOpenStatus Status { get; }

	/// <summary>Gets the leased session when a view was produced; otherwise, <see langword="null"/>.</summary>
	public IEditorSession? Session { get; }
}

/// <summary>
/// Opens and leases an editor view for a workspace document snapshot.
/// </summary>
/// <remarks>
/// Implementations may reuse an existing view. The returned session ensures that transient disposal
/// closes only a view that this session opened, not a view owned by another caller.
/// </remarks>
public interface IEditorViewHost
{
	/// <summary>
	/// Opens or reuses an editor view for the snapshot and returns a session for it.
	/// </summary>
	/// <param name="snapshot">The workspace document snapshot to open a view for.</param>
	/// <param name="options">The session options whose <see cref="EditorSessionOptions.Mode"/> governs the release.</param>
	/// <returns>The attach outcome with a session when a view was produced.</returns>
	EditorSessionOpenResult Open(
		WorkspaceDocumentSnapshot snapshot,
		EditorSessionOptions options);
}

/// <summary>
/// Releases a leased editor view and restores the previously active editor view.
/// </summary>
/// <remarks>
/// The host owns the view; this session owns only the release contract. Create one with
/// <see cref="EditorSession.Open"/> when this session's caller opened the view, or
/// <see cref="EditorSession.Reuse"/> when another caller already opened it. See
/// <see cref="Dispose"/> for the callback ordering, the transient-close rule, and the idempotence
/// guarantee.
/// </remarks>
public sealed class EditorSession : IEditorSession
{
	private readonly WorkspaceDocumentKey _documentKey;
	private readonly string _documentId;
	private readonly bool _openedView;
	private Action? _closeView;
	private Action? _activateEditor;
	private int _disposed;

	/// <summary>
	/// Creates a session for a view that this session opened.
	/// </summary>
	/// <param name="snapshot">The workspace document snapshot the session describes.</param>
	/// <param name="options">The session options whose <see cref="EditorSessionOptions.Mode"/> governs the release.</param>
	/// <param name="closeView">Closes the leased view.</param>
	/// <param name="activateEditor">Restores the previously active editor view.</param>
	/// <returns>A session that owns the opened view.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="snapshot"/>, <paramref name="closeView"/>, or <paramref name="activateEditor"/> is <see langword="null"/>.</exception>
	public static EditorSession Open(
		WorkspaceDocumentSnapshot snapshot,
		EditorSessionOptions options,
		Action closeView,
		Action activateEditor)
		=> new(snapshot, options, openedView: true, closeView, activateEditor);

	/// <summary>
	/// Creates a session for a view that another caller already opened.
	/// </summary>
	/// <remarks>
	/// A reused view is never closed by this session, even in
	/// <see cref="EditorSessionMode.Transient"/> mode, so no close callback is required.
	/// </remarks>
	/// <param name="snapshot">The workspace document snapshot the session describes.</param>
	/// <param name="options">The session options whose <see cref="EditorSessionOptions.Mode"/> governs the release.</param>
	/// <param name="activateEditor">Restores the previously active editor view.</param>
	/// <returns>A session that leases the existing view.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="snapshot"/> or <paramref name="activateEditor"/> is <see langword="null"/>.</exception>
	public static EditorSession Reuse(
		WorkspaceDocumentSnapshot snapshot,
		EditorSessionOptions options,
		Action activateEditor)
		=> new(snapshot, options, openedView: false, closeView: null, activateEditor);

	private EditorSession(
		WorkspaceDocumentSnapshot snapshot,
		EditorSessionOptions options,
		bool openedView,
		Action? closeView,
		Action activateEditor)
	{
		ArgumentNullException.ThrowIfNull(snapshot);
		if (openedView)
			ArgumentNullException.ThrowIfNull(closeView);
		ArgumentNullException.ThrowIfNull(activateEditor);

		// Only the identity and the release callbacks are retained; keeping the snapshot would make the
		// document content reachable for the session's whole lifetime for two fields.
		_documentKey = snapshot.DocumentKey;
		_documentId = snapshot.DocumentId;
		_openedView = openedView;
		_closeView = closeView;
		_activateEditor = activateEditor;

		Mode = options.Mode;
	}

	/// <inheritdoc />
	public WorkspaceDocumentKey DocumentKey => _documentKey;

	/// <inheritdoc />
	public string DocumentId => _documentId;

	/// <inheritdoc />
	public EditorSessionMode Mode { get; }

	/// <inheritdoc />
	public bool IsActive => Volatile.Read(ref _disposed) == 0;

	/// <summary>
	/// Releases the session's view responsibilities and restores the previously active editor view.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Disposal is idempotent; only the first call invokes the host callbacks. The callbacks run on the
	/// calling thread, so dispose on a thread where host view operations are legal.
	/// </para>
	/// <para>
	/// A transient session closes the view only when this session opened it. The editor activation
	/// always runs, even when the close callback throws; when both callbacks throw, the close failure
	/// is rethrown so the root cause is not replaced by a follow-up activation failure. The callback
	/// references are dropped after disposal.
	/// </para>
	/// </remarks>
	public void Dispose()
	{
		if (Interlocked.Exchange(ref _disposed, 1) == 1)
			return;

		ExceptionDispatchInfo? closeFailure = null;
		try
		{
			if (_openedView && Mode == EditorSessionMode.Transient)
				_closeView?.Invoke();
		}
		catch (Exception exception)
		{
			closeFailure = ExceptionDispatchInfo.Capture(exception);
		}
		finally
		{
			_closeView = null;
		}

		ExceptionDispatchInfo? activationFailure = null;
		try
		{
			_activateEditor?.Invoke();
		}
		catch (Exception exception)
		{
			activationFailure = ExceptionDispatchInfo.Capture(exception);
		}
		finally
		{
			_activateEditor = null;
		}

		(closeFailure ?? activationFailure)?.Throw();
	}
}
