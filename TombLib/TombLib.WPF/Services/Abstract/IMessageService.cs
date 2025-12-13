namespace TombLib.WPF.Services.Abstract;

/// <summary>
/// Provides methods for displaying various types of messages to the user.
/// </summary>
public interface IMessageService
{
	/// <summary>
	/// Shows an informational message to the user.
	/// </summary>
	/// <param name="message">The message text to display.</param>
	/// <param name="caption">The caption/title of the message box. Defaults to "Information".</param>
	void ShowInformation(string message, string caption = "Information");

	/// <summary>
	/// Shows an error message to the user.
	/// </summary>
	/// <param name="message">The error message text to display.</param>
	/// <param name="caption">The caption/title of the message box. Defaults to "Error".</param>
	void ShowError(string message, string caption = "Error");

	/// <summary>
	/// Shows a confirmation message with "Yes" and "No" options.
	/// </summary>
	/// <param name="message">The confirmation message text to display.</param>
	/// <param name="caption">The caption/title of the confirmation dialog.</param>
	/// <param name="defaultValue">The default value to highlight/select. If <see langword="null" />, no default is set.</param>
	/// <param name="isRisky">Indicates whether the action is risky, which may affect the visual presentation of the dialog.</param>
	/// <returns><see langword="true" /> if the user confirms the action; otherwise, <see langword="false" />.</returns>
	bool ShowConfirmation(string message, string caption = "Are you sure?", bool? defaultValue = null, bool isRisky = false);

	/// <summary>
	/// Shows a confirmation message with "Yes", "No" and optionally "Cancel" options.
	/// </summary>
	/// <typeparam name="T">The type of value to return for each button option.</typeparam>
	/// <param name="message">The confirmation message text to display.</param>
	/// <param name="caption">The caption/title of the confirmation dialog.</param>
	/// <param name="yesValue">The value to return when the user selects "Yes".</param>
	/// <param name="noValue">The value to return when the user selects "No".</param>
	/// <param name="cancelValue">The value to return when the user selects "Cancel". If <see langword="null" />, no Cancel option is shown.</param>
	/// <param name="defaultValue">The default value to highlight/select. If <see langword="null" />, no default is set.</param>
	/// <param name="isRisky">Indicates whether the action is risky, which may affect the visual presentation of the dialog.</param>
	/// <returns>The value corresponding to the user's choice (yesValue, noValue, or cancelValue).</returns>
	T ShowConfirmation<T>(
		string message,
		string caption,
		T yesValue,
		T noValue,
		T? cancelValue = null,
		T? defaultValue = null,
		bool isRisky = false
	) where T : struct;
}
