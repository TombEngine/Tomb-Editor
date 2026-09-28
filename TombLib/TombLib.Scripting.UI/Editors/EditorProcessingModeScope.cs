using System;

namespace TombLib.Scripting.UI.Editors;

/// <summary>
/// Owns the nest-safe <see cref="EditorProcessingMode"/> state of a single editor lifecycle.
/// A control exposes the current mode through <see cref="CurrentMode"/> but does not own an
/// unbalanced public flag; callers change the mode only through a scoped <see cref="Begin"/> lease.
/// </summary>
internal sealed class EditorProcessingModeScope
{
	private EditorProcessingMode _currentMode;

	/// <summary>
	/// Gets the current processing mode.
	/// </summary>
	public EditorProcessingMode CurrentMode => _currentMode;

	/// <summary>
	/// Begins a nested scope that applies the given mode and restores the previous mode when disposed.
	/// </summary>
	/// <param name="mode">The processing mode to apply for the scope.</param>
	/// <returns>A scope that restores the previous mode when disposed.</returns>
	public IDisposable Begin(EditorProcessingMode mode)
	{
		EditorProcessingMode previousMode = _currentMode;
		_currentMode = mode;
		return new Scope(this, previousMode);
	}

	private sealed class Scope : IDisposable
	{
		private readonly EditorProcessingModeScope _owner;
		private readonly EditorProcessingMode _previousMode;
		private bool _disposed;

		public Scope(EditorProcessingModeScope owner, EditorProcessingMode previousMode)
		{
			_owner = owner;
			_previousMode = previousMode;
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;
			_owner._currentMode = _previousMode;
		}
	}
}
