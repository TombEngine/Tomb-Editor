namespace TombIDE.Shared;

/// <summary>
/// Represents a form that can report progress updates.
/// </summary>
public interface IProgressReportingForm
{
	/// <summary>
	/// Increments the current progress value by the specified amount.
	/// </summary>
	/// <param name="value">The value to increment the progress by.</param>
	void IncrementProgress(int value);

	/// <summary>
	/// Sets the total progress value that represents 100% completion. For example, if you have 10 files to process,
	/// you would call <c>SetTotalProgress(10)</c> at the start, and then call <c>IncrementProgress(1)</c> after processing each file.
	/// </summary>
	/// <param name="total">The total progress value.</param>
	void SetTotalProgress(int total);
}
