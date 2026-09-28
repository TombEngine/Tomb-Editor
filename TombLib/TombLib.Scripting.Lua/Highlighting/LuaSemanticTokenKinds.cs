namespace TombLib.Scripting.Lua.Highlighting;

/// <summary>
/// Semantic token type and modifier names shared with the language server protocol.
/// </summary>
internal static class LuaSemanticTokenKinds
{
	// Token types
	public const string Namespace = "namespace";
	public const string Type = "type";
	public const string Class = "class";
	public const string Enum = "enum";
	public const string Interface = "interface";
	public const string Struct = "struct";
	public const string TypeParameter = "typeParameter";
	public const string Function = "function";
	public const string Method = "method";
	public const string Parameter = "parameter";
	public const string Property = "property";
	public const string Event = "event";
	public const string EnumMember = "enumMember";
	public const string Decorator = "decorator";
	public const string Macro = "macro";
	public const string Variable = "variable";

	// Token modifiers
	public const string DefaultLibrary = "defaultLibrary";
	public const string Declaration = "declaration";
	public const string Deprecated = "deprecated";
	public const string Global = "global";
}
