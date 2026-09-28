using Nickelony.IDEKit.IntelliSense.Completion;

namespace TombLib.Scripting.ClassicScript.Completion;

/// <summary>
/// Defines the ClassicScript-specific completion triggers that describe what the editor was doing
/// when it requested completion.
/// </summary>
public static class ClassicScriptCompletionTriggers
{
	/// <summary>
	/// Completion was requested for a new or empty line.
	/// </summary>
	public static readonly TextCompletionTrigger EmptyLine = TextCompletionTrigger.CreateCustom("EmptyLine");

	/// <summary>
	/// Completion was requested in a syntax-aware argument context.
	/// </summary>
	public static readonly TextCompletionTrigger Contextual = TextCompletionTrigger.CreateCustom("Contextual");

	/// <summary>
	/// Completion was requested while extending or replacing a word.
	/// </summary>
	public static readonly TextCompletionTrigger Word = TextCompletionTrigger.CreateCustom("Word");
}
