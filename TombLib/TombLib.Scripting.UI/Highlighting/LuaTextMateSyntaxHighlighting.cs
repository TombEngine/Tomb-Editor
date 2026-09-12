using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using Nickelony.IDEKit.AvalonEdit.Extras.TextMate.Highlighting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using TextMateSharp.Grammars;
using TextMateSharp.Model;
using TextMateSharp.Registry;

namespace TombLib.Scripting.UI.Highlighting;

/// <summary>
/// Installs TextMate-based syntax highlighting on a Lua <see cref="TextEditor"/>.
/// </summary>
public static class LuaTextMateSyntaxHighlighting
{
	private static readonly Lazy<IGrammar?> GrammarState = new(LoadGrammarState);
	private static readonly TextMateTokenTheme DefaultTheme = LuaBuiltInTextMateThemeDefaults.CreateDefaultTextMateTheme();

	/// <summary>
	/// Tries to install the default TextMate highlighting on the given editor.
	/// </summary>
	/// <param name="editor">The editor to install highlighting on.</param>
	/// <param name="installation">The installation created when the install succeeds.</param>
	/// <returns><c>true</c> when the installation succeeded; otherwise, <c>false</c>.</returns>
	public static bool TryInstall(TextEditor editor, [NotNullWhen(true)] out LuaTextMateInstallation? installation)
		=> TryInstall(editor, DefaultTheme, out installation);

	/// <summary>
	/// Tries to install the given TextMate theme highlighting on the given editor.
	/// </summary>
	/// <param name="editor">The editor to install highlighting on.</param>
	/// <param name="theme">The token theme to apply.</param>
	/// <param name="installation">The installation created when the install succeeds.</param>
	/// <returns><c>true</c> when the installation succeeded; otherwise, <c>false</c>.</returns>
	public static bool TryInstall(TextEditor editor, TextMateTokenTheme theme, [NotNullWhen(true)] out LuaTextMateInstallation? installation)
	{
		installation = null;

		if (editor?.Document is null)
			return false;

		IGrammar? grammar = GrammarState.Value;

		if (grammar is null)
			return false;

		var documentLines = new TextMateDocumentLineList(editor.Document);
		var model = new TMModel(documentLines);
		model.SetGrammar(grammar);
		var styleResolver = new TextMateThemeStyleResolver(theme ?? DefaultTheme);
		var transformer = new TextMateColorizingTransformer(editor.TextArea.TextView, model, styleResolver);

		editor.TextArea.TextView.LineTransformers.Add(transformer);
		installation = new LuaTextMateInstallation(editor, documentLines, model, transformer);
		return true;
	}

	/// <summary>
	/// Loads the fallback highlighting definition used when the grammar is unavailable.
	/// </summary>
	/// <returns>The fallback highlighting definition, or <c>null</c> when it cannot be loaded.</returns>
	public static IHighlightingDefinition? LoadFallbackHighlighting()
		=> LuaFallbackHighlightingLoader.Load();

	private static IGrammar? LoadGrammarState()
	{
		string grammarFilePath = Path.Combine(AppContext.BaseDirectory, "Configs", "TextEditors", "Grammars", "Lua", "lua.tmLanguage.json");

		if (!File.Exists(grammarFilePath))
			return null;

		var registry = new Registry(new RegistryOptions(ThemeName.DarkPlus));
		return registry.LoadGrammarFromPathSync(grammarFilePath, 0, new Dictionary<string, int>());
	}
}

/// <summary>
/// Owns the resources of an installed TextMate highlighting session.
/// </summary>
public sealed class LuaTextMateInstallation : IDisposable
{
	private readonly TextEditor _editor;
	private readonly TextMateDocumentLineList _documentLines;
	private readonly TextMateColorizingTransformer _transformer;
	private bool _isDisposed;

	internal LuaTextMateInstallation(TextEditor editor, TextMateDocumentLineList documentLines, TMModel model,
		TextMateColorizingTransformer transformer)
	{
		_editor = editor;
		_documentLines = documentLines;
		_transformer = transformer;
		Model = model;
	}

	/// <summary>
	/// Gets the TextMate model backing the highlighting.
	/// </summary>
	public TMModel Model { get; }

	/// <summary>
	/// Disposes the highlighting transformer, model and document lines, and removes the transformer from the editor.
	/// </summary>
	public void Dispose()
	{
		if (_isDisposed)
			return;

		_isDisposed = true;

		_transformer.Dispose();

		if (_editor.TextArea.TextView.LineTransformers.Contains(_transformer))
			_editor.TextArea.TextView.LineTransformers.Remove(_transformer);

		Model.Dispose();
		_documentLines.Dispose();
	}
}
