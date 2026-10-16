using Nickelony.IDEKit.AvalonEdit.LanguageFeatures.Highlighting;
using Nickelony.IDEKit.AvalonEdit.Rendering;
using Nickelony.IDEKit.IntelliSense.SemanticTokens;
using System;
using System.Windows;
using System.Windows.Media;
using TombLib.Scripting.Lua.Resources;

namespace TombLib.Scripting.Lua.Highlighting;

/// <summary>
/// Resolves Lua semantic tokens to visual styles from the active Lua theme brush set.
/// </summary>
internal sealed class LuaSemanticTokenStyleResolver : ISemanticTokenStyleResolver
{
	private static readonly TextDecorationCollection DeprecatedDecorations = CreateTextDecorations(TextDecorations.Strikethrough);

	private LuaThemeBrushSet _brushSet;

	/// <summary>
	/// Initializes a new instance of the <see cref="LuaSemanticTokenStyleResolver"/> class.
	/// </summary>
	/// <param name="brushSet">The active Lua theme brush set.</param>
	public LuaSemanticTokenStyleResolver(LuaThemeBrushSet brushSet)
	{
		ArgumentNullException.ThrowIfNull(brushSet);
		_brushSet = brushSet;
	}

	/// <summary>
	/// Replaces the active theme brush set used to resolve token styles.
	/// </summary>
	/// <param name="brushSet">The new active theme brush set.</param>
	public void UpdateTheme(LuaThemeBrushSet brushSet)
	{
		ArgumentNullException.ThrowIfNull(brushSet);
		_brushSet = brushSet;
	}

	/// <inheritdoc/>
	public TextRunStyle Resolve(TextSemanticToken token)
	{
		ArgumentNullException.ThrowIfNull(token);

		Brush? foreground = token.Type switch
		{
			LuaSemanticTokenKinds.Namespace => _brushSet.TypeBrush,
			LuaSemanticTokenKinds.Type => _brushSet.TypeBrush,
			LuaSemanticTokenKinds.Class => _brushSet.TypeBrush,
			LuaSemanticTokenKinds.Enum => _brushSet.TypeBrush,
			LuaSemanticTokenKinds.Interface => _brushSet.TypeBrush,
			LuaSemanticTokenKinds.Struct => _brushSet.TypeBrush,
			LuaSemanticTokenKinds.TypeParameter => _brushSet.TypeBrush,
			LuaSemanticTokenKinds.Function => token.HasModifier(LuaSemanticTokenKinds.DefaultLibrary) ? _brushSet.TypeBrush : _brushSet.MethodBrush,
			LuaSemanticTokenKinds.Method => token.HasModifier(LuaSemanticTokenKinds.DefaultLibrary) ? _brushSet.TypeBrush : _brushSet.MethodBrush,
			LuaSemanticTokenKinds.Parameter => _brushSet.VariableBrush,
			LuaSemanticTokenKinds.Property => _brushSet.PropertyBrush,
			LuaSemanticTokenKinds.Event => _brushSet.VariableBrush,
			LuaSemanticTokenKinds.EnumMember => _brushSet.ConstantBrush,
			LuaSemanticTokenKinds.Decorator => _brushSet.KeywordBrush,
			LuaSemanticTokenKinds.Macro => _brushSet.KeywordBrush,
			LuaSemanticTokenKinds.Variable => ResolveVariableBrush(token),
			_ => null
		};

		return new TextRunStyle(
			foreground,
			token.HasModifier(LuaSemanticTokenKinds.Declaration)
				&& (token.Type == LuaSemanticTokenKinds.Function || token.Type == LuaSemanticTokenKinds.Method),
			IsItalic: false,
			token.HasModifier(LuaSemanticTokenKinds.Deprecated) ? DeprecatedDecorations : null);
	}

	private SolidColorBrush? ResolveVariableBrush(TextSemanticToken token)
	{
		if (token.HasModifier(LuaSemanticTokenKinds.DefaultLibrary))
			return _brushSet.TypeBrush;

		if (token.HasModifier(LuaSemanticTokenKinds.Global))
			return _brushSet.PropertyBrush;

		return _brushSet.VariableBrush;
	}

	private static TextDecorationCollection CreateTextDecorations(TextDecorationCollection source)
	{
		var clone = source.Clone();
		clone.Freeze();
		return clone;
	}
}
