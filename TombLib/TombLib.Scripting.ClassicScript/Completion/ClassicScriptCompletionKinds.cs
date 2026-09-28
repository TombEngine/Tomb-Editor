using Nickelony.IDEKit.IntelliSense.Completion;

namespace TombLib.Scripting.ClassicScript.Completion;

/// <summary>
/// Defines the ClassicScript-specific completion kinds layered on top of the shared
/// <see cref="TextCompletionItemKind"/> well-known set.
/// </summary>
internal static class ClassicScriptCompletionKinds
{
	/// <summary>
	/// Identifies a legacy (TR4/TRNG) command completion item.
	/// </summary>
	public static readonly TextCompletionItemKind OldCommand = TextCompletionItemKind.CreateCustom("OldCommand");

	/// <summary>
	/// Identifies a new-style (TRNG) command completion item.
	/// </summary>
	public static readonly TextCompletionItemKind NewCommand = TextCompletionItemKind.CreateCustom("NewCommand");
}
