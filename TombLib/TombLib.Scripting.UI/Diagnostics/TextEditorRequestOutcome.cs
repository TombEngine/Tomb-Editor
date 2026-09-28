namespace TombLib.Scripting.UI.Diagnostics;

/// <summary>
/// Describes how an admitted error-detection run completed.
/// </summary>
/// <remarks>
/// The worker classifies its own runs: a run whose provider call threw is reported as
/// <see cref="Failed"/>, a run replaced by a newer request or an owner invalidation is
/// <see cref="Superseded"/>, and a run whose operation ownership ended before completion is
/// <see cref="Stale"/>.
/// </remarks>
internal enum TextEditorRequestOutcome
{
	/// <summary>
	/// The run completed and its result was published.
	/// </summary>
	Completed,

	/// <summary>
	/// The run's provider call failed; the error is published with the completion event.
	/// </summary>
	Failed,

	/// <summary>
	/// Cancellation was requested for the run, so its result is not published.
	/// </summary>
	Canceled,

	/// <summary>
	/// A newer request or an owner invalidation replaced the run before its result could be published.
	/// </summary>
	Superseded,

	/// <summary>
	/// The run outlived its operation ownership: the session generation advanced or the logical
	/// document identity changed before the result could be published.
	/// </summary>
	Stale
}
