namespace TombLib.Scripting.ClassicScript.Syntaxes;

/// <summary>
/// Describes a single ClassicScript syntax definition.
/// </summary>
/// <param name="Key">The lookup key of the syntax definition.</param>
/// <param name="SyntaxText">The display text of the syntax definition.</param>
/// <param name="ApplicableSection">The section the syntax applies to.</param>
/// <param name="ArgumentCount">The number of arguments the syntax takes.</param>
/// <param name="HasArrayArguments">Whether the syntax takes array arguments.</param>
public sealed record class ClassicScriptSyntaxDefinition(
	string Key,
	string SyntaxText,
	string ApplicableSection,
	int ArgumentCount,
	bool HasArrayArguments);
