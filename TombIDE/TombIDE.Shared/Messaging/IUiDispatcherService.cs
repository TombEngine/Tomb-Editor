#nullable enable

using System;

namespace TombIDE.Shared.Messaging;

/// <summary>
/// Provides explicit UI-thread marshaling for shared message publication.
/// </summary>
public interface IUiDispatcherService
{
	/// <summary>
	/// Gets whether the current call already runs on the dispatcher context.
	/// </summary>
	bool CheckAccess();

	/// <summary>
	/// Runs the supplied action on the dispatcher context.
	/// </summary>
	/// <param name="action">The work to execute.</param>
	void Invoke(Action action);
}
