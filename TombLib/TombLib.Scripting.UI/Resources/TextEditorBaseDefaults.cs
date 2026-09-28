namespace TombLib.Scripting.UI.Resources;

/// <summary>
/// Defines default values used by the base text editor configuration.
/// </summary>
public static class TextEditorBaseDefaults
{
	/// <summary>
	/// The default font size of the editor.
	/// </summary>
	public const double FontSize = 16d;

	/// <summary>
	/// The default font family of the editor.
	/// </summary>
	public const string FontFamily = "Consolas";

	/// <summary>
	/// The default undo stack size of the editor.
	/// </summary>
	public const int UndoStackSize = 1024;

	/// <summary>
	/// The default value for the IntelliSense feature gate.
	/// </summary>
	public const bool IntelliSenseEnabled = true;

	/// <summary>
	/// The default value for the completion feature gate.
	/// </summary>
	public const bool CompletionEnabled = true;

	/// <summary>
	/// The default value for the live error underlining feature gate.
	/// </summary>
	public const bool LiveErrorUnderlining = true;

	/// <summary>
	/// The default value for the signature help popups feature gate.
	/// </summary>
	public const bool SignatureHelpPopupsEnabled = true;

	/// <summary>
	/// The default value for auto-closing parentheses.
	/// </summary>
	public const bool AutoCloseParentheses = true;

	/// <summary>
	/// The default value for auto-closing braces.
	/// </summary>
	public const bool AutoCloseBraces = true;

	/// <summary>
	/// The default value for auto-closing brackets.
	/// </summary>
	public const bool AutoCloseBrackets = true;

	/// <summary>
	/// The default value for auto-closing double quotes.
	/// </summary>
	public const bool AutoCloseDoubleQuotes = true;

	/// <summary>
	/// The default value for auto-closing single quotes.
	/// </summary>
	public const bool AutoCloseSingleQuotes = true;

	/// <summary>
	/// The default value for word wrapping.
	/// </summary>
	public const bool WordWrapping = false;

	/// <summary>
	/// The default value for highlighting the current line.
	/// </summary>
	public const bool HighlightCurrentLine = true;

	/// <summary>
	/// The default value for showing line numbers.
	/// </summary>
	public const bool ShowLineNumbers = true;

	/// <summary>
	/// The default value for showing spaces visually.
	/// </summary>
	public const bool ShowVisualSpaces = false;

	/// <summary>
	/// The default value for showing tabs visually.
	/// </summary>
	public const bool ShowVisualTabs = false;
}
