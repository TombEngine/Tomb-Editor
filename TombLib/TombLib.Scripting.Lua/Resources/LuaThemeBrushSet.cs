using Nickelony.IDEKit.IntelliSense.Completion;
using System.Collections.Generic;
using System.Windows.Media;

namespace TombLib.Scripting.Lua.Resources;

/// <summary>
/// Bundles the frozen WPF brushes derived from a resolved Lua theme.
/// </summary>
internal sealed class LuaThemeBrushSet
{
	private readonly IReadOnlyDictionary<LuaThemeBrushRole, SolidColorBrush> _brushes;

	/// <summary>
	/// Initializes a new instance of the <see cref="LuaThemeBrushSet"/> class.
	/// </summary>
	/// <param name="themeName">The canonical name of the theme that produced this brush set.</param>
	/// <param name="brushes">The brushes keyed by their semantic role.</param>
	public LuaThemeBrushSet(string themeName, IReadOnlyDictionary<LuaThemeBrushRole, SolidColorBrush> brushes)
	{
		ThemeName = themeName;
		_brushes = brushes;
	}

	/// <summary>
	/// Gets the canonical name of the theme that produced this brush set.
	/// </summary>
	public string ThemeName { get; }

	/// <summary>
	/// Gets the editor background brush.
	/// </summary>
	public SolidColorBrush EditorBackground => _brushes[LuaThemeBrushRole.EditorBackground];

	/// <summary>
	/// Gets the editor foreground brush.
	/// </summary>
	public SolidColorBrush EditorForeground => _brushes[LuaThemeBrushRole.EditorForeground];

	/// <summary>
	/// Gets the muted text brush used for secondary completion details and similar metadata.
	/// </summary>
	public SolidColorBrush MutedTextBrush => _brushes[LuaThemeBrushRole.MutedText];

	/// <summary>
	/// Gets the fallback brush used for uncategorized symbols.
	/// </summary>
	public SolidColorBrush MiscBrush => _brushes[LuaThemeBrushRole.Misc];

	/// <summary>
	/// Gets the brush used for methods and functions.
	/// </summary>
	public SolidColorBrush MethodBrush => _brushes[LuaThemeBrushRole.Method];

	/// <summary>
	/// Gets the brush used for variables and parameters.
	/// </summary>
	public SolidColorBrush VariableBrush => _brushes[LuaThemeBrushRole.Variable];

	/// <summary>
	/// Gets the brush used for properties and fields.
	/// </summary>
	public SolidColorBrush PropertyBrush => _brushes[LuaThemeBrushRole.Property];

	/// <summary>
	/// Gets the brush used for types and namespaces.
	/// </summary>
	public SolidColorBrush TypeBrush => _brushes[LuaThemeBrushRole.Type];

	/// <summary>
	/// Gets the brush used for keywords.
	/// </summary>
	public SolidColorBrush KeywordBrush => _brushes[LuaThemeBrushRole.Keyword];

	/// <summary>
	/// Gets the brush used for language-defined constants.
	/// </summary>
	public SolidColorBrush LanguageConstantBrush => _brushes[LuaThemeBrushRole.LanguageConstant];

	/// <summary>
	/// Gets the brush used for user-defined constants and enum members.
	/// </summary>
	public SolidColorBrush ConstantBrush => _brushes[LuaThemeBrushRole.Constant];

	/// <summary>
	/// Gets the brush used for file and folder completion items.
	/// </summary>
	public SolidColorBrush FileBrush => _brushes[LuaThemeBrushRole.File];

	/// <summary>
	/// Gets the brush used for signature help parameter documentation.
	/// </summary>
	public SolidColorBrush SignatureParamDocForeground => _brushes[LuaThemeBrushRole.SignatureParamDoc];

	/// <summary>
	/// Gets the brush used to emphasize the active signature parameter.
	/// </summary>
	public SolidColorBrush SignatureActiveParamForeground => _brushes[LuaThemeBrushRole.SignatureActiveParam];

	/// <summary>
	/// Gets the base brush used for signature labels.
	/// </summary>
	public SolidColorBrush SignatureForeground => _brushes[LuaThemeBrushRole.SignatureForeground];

	/// <summary>
	/// Gets the brush used to render the specified completion item kind.
	/// </summary>
	/// <param name="kind">The completion item icon kind.</param>
	/// <returns>The brush associated with that kind.</returns>
	public Brush GetCompletionItemBrush(TextCompletionItemKind kind) => kind.Identifier switch
	{
		"Variable" => VariableBrush,
		"Field" => PropertyBrush,
		"Method" => MethodBrush,
		"Property" => PropertyBrush,
		"Class" => TypeBrush,
		"Keyword" => KeywordBrush,
		"Constant" => ConstantBrush,
		"Parameter" => VariableBrush,
		"Namespace" => TypeBrush,
		"Function" or "Constructor" or "Event" => MethodBrush,
		"Interface" or "Enum" or "Struct" or "TypeParameter" => TypeBrush,
		"Module" => TypeBrush,
		"Value" or "Reference" => VariableBrush,
		"EnumMember" => ConstantBrush,
		"Snippet" or "Operator" => KeywordBrush,
		"File" => FileBrush,
		"Folder" => FileBrush,
		_ => MiscBrush
	};
}

/// <summary>
/// Identifies the semantic role of a brush within a Lua theme brush set.
/// </summary>
internal enum LuaThemeBrushRole
{
	EditorBackground,
	EditorForeground,
	MutedText,
	Misc,
	Method,
	Variable,
	Property,
	Type,
	Keyword,
	LanguageConstant,
	Constant,
	File,
	SignatureParamDoc,
	SignatureActiveParam,
	SignatureForeground
}
