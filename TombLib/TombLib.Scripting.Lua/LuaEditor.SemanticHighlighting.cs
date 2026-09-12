using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.AvalonEdit.IntelliSense.Highlighting;
using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.SemanticTokens;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TombLib.Scripting.Lua.Highlighting;

namespace TombLib.Scripting.Lua;

public sealed partial class LuaEditor
{
	private SemanticTokensColorizer? _semanticTokensColorizer;
	private LuaSemanticTokenStyleResolver? _semanticTokenStyleResolver;

	/// <summary>
	/// Replaces the current semantic token set used to colorize the document.
	/// </summary>
	/// <param name="tokens">The semantic tokens to apply to the editor.</param>
	public void SetSemanticTokens(IReadOnlyList<LuaSemanticToken> tokens)
	{
		EnsureSemanticTokensColorizerAttached();
		_semanticTokensColorizer.SetTokens(ConvertToOffsetTokens(tokens));
	}

	/// <summary>
	/// Removes all semantic token formatting from the current document.
	/// </summary>
	public void ClearSemanticTokens()
		=> _semanticTokensColorizer?.ClearTokens();

	[MemberNotNull(nameof(_semanticTokensColorizer))]
	private void EnsureSemanticTokensColorizerAttached()
	{
		var brushSet = GetThemeBrushSet();

		if (_semanticTokensColorizer is null || _semanticTokenStyleResolver is null)
		{
			_semanticTokenStyleResolver = new LuaSemanticTokenStyleResolver(brushSet);
			_semanticTokensColorizer = new(TextArea.TextView, _semanticTokenStyleResolver);
		}
		else
		{
			_semanticTokenStyleResolver.UpdateTheme(brushSet);
			_semanticTokensColorizer.Rebuild();
		}

		if (!TextArea.TextView.LineTransformers.Contains(_semanticTokensColorizer))
			TextArea.TextView.LineTransformers.Add(_semanticTokensColorizer);
	}

	private IReadOnlyList<TextSemanticToken> ConvertToOffsetTokens(IReadOnlyList<LuaSemanticToken> tokens)
	{
		var converted = new List<TextSemanticToken>(tokens.Count);

		for (int i = 0; i < tokens.Count; i++)
		{
			LuaSemanticToken token = tokens[i];
			int offset = GetOffsetForToken(token);

			if (offset < 0)
				continue;

			converted.Add(new TextSemanticToken(new TextRange(offset, token.Length), token.Type, token.Modifiers));
		}

		return converted;
	}

	private int GetOffsetForToken(LuaSemanticToken token)
	{
		int lineNumber = token.Line + 1;

		if (lineNumber < 1 || lineNumber > Document.LineCount)
			return -1;

		DocumentLine line = Document.GetLineByNumber(lineNumber);
		int character = Math.Max(0, token.Character);
		return line.Offset + Math.Min(character, line.Length);
	}
}
