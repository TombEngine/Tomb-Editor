using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.AvalonEdit.LanguageFeatures.Highlighting;
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
	public void SetSemanticTokens(IReadOnlyList<SemanticToken> tokens)
	{
		EnsureSemanticTokensColorizerAttached();
		_semanticTokensColorizer.SetTokens(SemanticTokenConversion.ToTextSemanticTokens(tokens, TextLineMap.Build(Document.Text)));
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
}
