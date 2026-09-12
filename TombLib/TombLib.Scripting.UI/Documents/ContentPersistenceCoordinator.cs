using System;
using System.ComponentModel;
using System.Windows.Threading;

namespace TombLib.Scripting.UI.Documents;

/// <summary>
/// Coordinates content-change detection and persistence for an editor.
/// </summary>
public sealed class ContentPersistenceCoordinator : IDisposable
{
	private readonly Func<string> _contentProvider;
	private readonly Func<bool> _suppressedProvider;
	private readonly ContentChangedWorker _contentChangedWorker;
	private readonly DispatcherTimer? _textChangedDelayedTimer;
	private TimeSpan _delayedInterval = TimeSpan.FromMilliseconds(300.0);
	private bool _isDisposed;
	private bool _notificationsSuppressed;

	/// <summary>
	/// Initializes a new instance of the <see cref="ContentPersistenceCoordinator"/> class.
	/// </summary>
	/// <param name="contentProvider">The callback that returns the current editor content.</param>
	/// <param name="suppressedProvider">The callback that reports whether editor processing is currently suppressed.</param>
	/// <param name="useDelayedScheduling">Whether content-change runs are scheduled through a delayed dispatcher timer.</param>
	public ContentPersistenceCoordinator(
		Func<string> contentProvider,
		Func<bool> suppressedProvider,
		bool useDelayedScheduling = false)
	{
		ArgumentNullException.ThrowIfNull(contentProvider);
		_contentProvider = contentProvider;
		ArgumentNullException.ThrowIfNull(suppressedProvider);
		_suppressedProvider = suppressedProvider;
		_contentChangedWorker = new ContentChangedWorker();

		if (useDelayedScheduling)
		{
			_textChangedDelayedTimer = new DispatcherTimer();
			_textChangedDelayedTimer.Tick += TextChangedDelayedTimer_Tick;
			_textChangedDelayedTimer.Interval = _delayedInterval;
		}

		_contentChangedWorker.RunWorkerCompleted += ContentChangedWorker_RunWorkerCompleted;
	}

	/// <summary>
	/// Raised when a content-changed worker run completes.
	/// </summary>
	public event EventHandler? ContentChangedWorkerRunCompleted;

	/// <summary>
	/// Raised when a content change is scheduled through the delayed timer.
	/// </summary>
	public event EventHandler? TextChangedDelayed;

	/// <summary>
	/// Gets whether delayed persistence and notifications are temporarily suppressed.
	/// </summary>
	public bool IsResetting => _notificationsSuppressed;

	/// <summary>
	/// Begins a nest-safe scope that cancels pending persistence work and suppresses delayed notifications.
	/// </summary>
	/// <returns>A scope that restores the previous suppression state when disposed.</returns>
	public IDisposable BeginResetScope()
	{
		bool previousNotificationsSuppressed = _notificationsSuppressed;
		_notificationsSuppressed = true;
		_textChangedDelayedTimer?.Stop();
		_contentChangedWorker.ResetPendingWork();

		return new ResetScope(this, previousNotificationsSuppressed);
	}

	/// <summary>
	/// Gets or sets the path of the file the content is persisted to.
	/// </summary>
	public string FilePath
	{
		get => _contentChangedWorker.FilePath;
		set
		{
			if (_isDisposed)
				return;

			_contentChangedWorker.FilePath = value;
		}
	}

	/// <summary>
	/// Gets or sets whether backup files are created when the content changes.
	/// </summary>
	public bool CreateBackupFiles
	{
		get => _isDisposed ? false : _contentChangedWorker.CreateBackupFiles;
		set
		{
			if (_isDisposed)
				return;

			_contentChangedWorker.CreateBackupFiles = value;
		}
	}

	/// <summary>
	/// Gets or sets the interval of the delayed content-change scheduling.
	/// </summary>
	public TimeSpan DelayedInterval
	{
		get => _textChangedDelayedTimer?.Interval ?? _delayedInterval;
		set
		{
			if (_isDisposed)
				return;

			_delayedInterval = value;

			if (_textChangedDelayedTimer is not null)
				_textChangedDelayedTimer.Interval = value;
		}
	}

	/// <summary>
	/// Reports whether the content changed and schedules the delayed persistence run when enabled.
	/// </summary>
	/// <returns><c>true</c> when the content differs from the persisted content; otherwise, <c>false</c>.</returns>
	public bool HandleContentChanged()
	{
		if (_isDisposed)
			return false;

		bool isChanged = _contentChangedWorker.HasChanges(_contentProvider());

		if (_notificationsSuppressed)
			return isChanged;

		if (_textChangedDelayedTimer is not null)
		{
			_textChangedDelayedTimer.Stop();
			_textChangedDelayedTimer.Start();
		}

		return isChanged;
	}

	/// <summary>
	/// Reports whether the content changed and runs the persistence worker unless processing is suppressed.
	/// </summary>
	/// <returns><c>true</c> when the content differs from the persisted content; otherwise, <c>false</c>.</returns>
	public bool RunContentChangedCheck()
	{
		if (_isDisposed)
			return false;

		return RunContentChangedCheck(_contentProvider());
	}

	/// <summary>
	/// Reports whether the supplied content changed and runs the persistence worker unless processing is suppressed.
	/// </summary>
	/// <param name="content">The current editor content.</param>
	/// <returns><c>true</c> when the content differs from the persisted content; otherwise, <c>false</c>.</returns>
	public bool RunContentChangedCheck(string content)
	{
		if (_isDisposed)
			return false;

		bool isChanged = _contentChangedWorker.HasChanges(content);

		if (!_notificationsSuppressed && !_suppressedProvider())
			_contentChangedWorker.Run(content);

		return isChanged;
	}

	/// <summary>
	/// Stops delayed scheduling and records the given content as the persisted baseline.
	/// </summary>
	/// <param name="content">The persisted content.</param>
	public void SetPersistedContent(string content)
	{
		if (_isDisposed)
			return;

		_textChangedDelayedTimer?.Stop();
		_contentChangedWorker.SetPersistedContent(content);
	}

	/// <summary>
	/// Reports whether the given content differs from the persisted baseline.
	/// </summary>
	/// <param name="content">The content to compare.</param>
	/// <returns><c>true</c> when the content changed; otherwise, <c>false</c>.</returns>
	public bool HasChanges(string content)
		=> _isDisposed ? false : _contentChangedWorker.HasChanges(content);

	/// <summary>
	/// Stops the delayed timer and disposes the underlying persistence worker.
	/// </summary>
	public void Dispose()
	{
		if (_isDisposed)
			return;

		_isDisposed = true;

		if (_textChangedDelayedTimer is not null)
		{
			_textChangedDelayedTimer.Stop();
			_textChangedDelayedTimer.Tick -= TextChangedDelayedTimer_Tick;
		}

		_contentChangedWorker.RunWorkerCompleted -= ContentChangedWorker_RunWorkerCompleted;
		_contentChangedWorker.Dispose();
	}

	private void ContentChangedWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
		=> ContentChangedWorkerRunCompleted?.Invoke(this, EventArgs.Empty);

	private void TextChangedDelayedTimer_Tick(object? sender, EventArgs e)
	{
		if (_isDisposed)
			return;

		_textChangedDelayedTimer?.Stop();

		if (_notificationsSuppressed)
			return;

		RunContentChangedCheck();
		TextChangedDelayed?.Invoke(this, EventArgs.Empty);

		if (_textChangedDelayedTimer is not null)
			_textChangedDelayedTimer.Stop();
	}

	private sealed class ResetScope : IDisposable
	{
		private readonly ContentPersistenceCoordinator _coordinator;
		private readonly bool _previousNotificationsSuppressed;
		private bool _disposed;

		public ResetScope(ContentPersistenceCoordinator coordinator, bool previousNotificationsSuppressed)
		{
			_coordinator = coordinator;
			_previousNotificationsSuppressed = previousNotificationsSuppressed;
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;
			_coordinator._contentChangedWorker.ResetPendingWork();
			_coordinator._notificationsSuppressed = _previousNotificationsSuppressed;
		}
	}
}
