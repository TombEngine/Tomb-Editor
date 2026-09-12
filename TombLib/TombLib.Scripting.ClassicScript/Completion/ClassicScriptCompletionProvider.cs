using Nickelony.IDEKit.Core.Identifiers;
using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.Completion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TombLib.Scripting.ClassicScript.Commands;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.UI.Completion;
using TombLib.Scripting.UI.Extensions;

namespace TombLib.Scripting.ClassicScript.Completion;

/// <summary>
/// Builds completion items for ClassicScript documents.
/// </summary>
public sealed class ClassicScriptCompletionProvider : ITextCompletionProvider
{
	private static readonly Regex FlagSyntaxRegex = new(@"\(.*_\.*\)");

	private readonly IClassicScriptCommandService _commandService;
	private readonly ClassicScriptMnemonicCatalogService _mnemonicCatalogService;
	private readonly ClassicScriptCommandCatalogService _commandCatalogService = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptCompletionProvider"/> class.
	/// </summary>
	/// <param name="commandService">The command service used to resolve command context.</param>
	/// <param name="mnemonicCatalogService">The mnemonic catalog service used to source mnemonic items.</param>
	public ClassicScriptCompletionProvider(
		IClassicScriptCommandService commandService,
		ClassicScriptMnemonicCatalogService mnemonicCatalogService)
	{
		_commandService = commandService;
		_mnemonicCatalogService = mnemonicCatalogService;
	}

	/// <summary>
	/// Gets the completion items for the given context.
	/// </summary>
	/// <param name="context">The completion context.</param>
	/// <returns>The completion items, or an empty list when no completion applies.</returns>
	public IReadOnlyList<TextCompletionItem> GetCompletionItems(TextCompletionContext context)
	{
		ArgumentNullException.ThrowIfNull(context);

		var source = new StringTextSnapshot(context.DocumentText);

		return context.Trigger switch
		{
			TextCompletionTrigger.EmptyLine => GetNewLineCompletionItems(source, context.CaretOffset),
			TextCompletionTrigger.Contextual => GetContextualCompletionItems(source, context.CaretOffset, context.ArgumentIndex),
			TextCompletionTrigger.Word => GetWordCompletionItems(context.DocumentText, context.CaretOffset),
			_ => []
		};
	}

	private IReadOnlyList<TextCompletionItem> GetNewLineCompletionItems(ITextSnapshot source, int caretOffset)
	{
		string? currentSection = _commandService.GetCurrentSectionName(source, caretOffset);

		if (currentSection is not null && currentSection.IgnoreCaseEqualsAny("Strings", "PSXStrings", "PCStrings", "ExtraNG"))
			return [];

		var items = new List<TextCompletionItem>();

		AddItems(items, _commandCatalogService.OldCommands, "=", ClassicScriptCompletionKinds.OldCommand);
		AddItems(items, _commandCatalogService.NewCommands.Where(name => !name.StartsWith('#')), "=", ClassicScriptCompletionKinds.NewCommand);
		AddItems(items, _commandCatalogService.Sections.Select(section => $"[{section}]"), string.Empty, TextCompletionItemKind.Section);

		items.Add(CreateItem("#INCLUDE ", "#INCLUDE ", TextCompletionItemKind.Directive));
		items.Add(CreateItem("#DEFINE ", "#DEFINE ", TextCompletionItemKind.Directive));
		items.Add(CreateItem("#FIRST_ID ", "#FIRST_ID ", TextCompletionItemKind.Directive));

		return items;
	}

	private IReadOnlyList<TextCompletionItem> GetContextualCompletionItems(ITextSnapshot source, int caretOffset, int argumentIndex)
	{
		string? syntax = _commandService.GetCommandSyntax(source, caretOffset);

		if (string.IsNullOrEmpty(syntax))
			return [];

		if (!FlagSyntaxRegex.IsMatch(syntax) && !syntax.Contains("ENABLED", StringComparison.OrdinalIgnoreCase) && !syntax.Contains("DISABLED", StringComparison.OrdinalIgnoreCase))
			return [];

		string[] arguments = syntax.Split(',');

		if (argumentIndex == -1)
			argumentIndex = _commandService.GetArgumentIndexAtOffset(source, caretOffset);

		if (arguments.Length <= argumentIndex || argumentIndex == -1)
			return [];

		string currentArgument = arguments[argumentIndex];
		var items = new List<TextCompletionItem>();

		if (FlagSyntaxRegex.IsMatch(currentArgument))
		{
			string mnemonicPrefix = currentArgument.Split('(')[1].Split(')')[0].Trim('.').Trim();

			foreach (string mnemonicConstant in _mnemonicCatalogService.GetAllFlags())
			{
				if (mnemonicConstant.StartsWith(mnemonicPrefix, StringComparison.OrdinalIgnoreCase))
					items.Add(CreateItem(mnemonicConstant, mnemonicConstant, TextCompletionItemKind.Constant));
			}
		}
		else if (currentArgument.Contains("ENABLED", StringComparison.OrdinalIgnoreCase) || currentArgument.Contains("DISABLED", StringComparison.OrdinalIgnoreCase))
		{
			items.Add(CreateItem("ENABLED", "ENABLED", TextCompletionItemKind.Constant));
			items.Add(CreateItem("DISABLED", "DISABLED", TextCompletionItemKind.Constant));
		}

		return items;
	}

	private IReadOnlyList<TextCompletionItem> GetWordCompletionItems(string documentText, int caretOffset)
	{
		if (caretOffset <= 0 || caretOffset > documentText.Length)
			return [];

		string word = IdentifierHelper.GetPrefix(documentText, caretOffset);

		if (string.IsNullOrEmpty(word)
			|| !_mnemonicCatalogService.GetAllFlags().Any(constant => constant.StartsWith(word, StringComparison.OrdinalIgnoreCase)))
			return [];

		var items = new List<TextCompletionItem>();

		foreach (string mnemonicConstant in _mnemonicCatalogService.GetAllFlags())
		{
			if (mnemonicConstant.StartsWith(word, StringComparison.OrdinalIgnoreCase))
				items.Add(CreateItem(mnemonicConstant, mnemonicConstant, TextCompletionItemKind.Constant));
		}

		return items;
	}

	private static void AddItems(List<TextCompletionItem> items, IEnumerable<string> values, string suffix, TextCompletionItemKind kind)
	{
		foreach (string value in values)
			items.Add(CreateItem(value, value + suffix, kind));
	}

	private static TextCompletionItem CreateItem(string label, string insertText, TextCompletionItemKind kind)
		=> new(label, insertText, kind: kind, detail: GetDetail(kind));

	private static string GetDetail(TextCompletionItemKind kind) => kind.Identifier switch
	{
		"OldCommand" => "Old Command",
		"NewCommand" => "New Command",
		"Constant" => "Constant",
		"Section" => "Section",
		"Directive" => "Directive",
		_ => kind.ToString()
	};
}
