using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.Hover;
using Nickelony.IDEKit.IntelliSense.Navigation;
using System;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Types;

namespace TombLib.Scripting.ClassicScript.Hover;

/// <summary>
/// Resolves hover information for ClassicScript words.
/// </summary>
public sealed class ClassicScriptHoverProvider : ITextHoverProvider
{
	private readonly IClassicScriptLineService _lineService;
	private readonly IClassicScriptCommandService _commandService;
	private readonly ClassicScriptMnemonicCatalogService _mnemonicCatalogService;

	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptHoverProvider"/> class.
	/// </summary>
	/// <param name="lineService">The line service used to identify hovered words.</param>
	/// <param name="commandService">The command service used to resolve command hover content.</param>
	/// <param name="mnemonicCatalogService">The mnemonic catalog service used to resolve mnemonic hover content.</param>
	public ClassicScriptHoverProvider(
		IClassicScriptLineService lineService,
		IClassicScriptCommandService commandService,
		ClassicScriptMnemonicCatalogService mnemonicCatalogService)
	{
		_lineService = lineService;
		_commandService = commandService;
		_mnemonicCatalogService = mnemonicCatalogService;
	}

	/// <summary>
	/// Gets the hover information for the given request.
	/// </summary>
	/// <param name="request">The hover request.</param>
	/// <returns>The hover information, or <c>null</c> when the hovered word is not a known symbol.</returns>
	public TextHoverInfo? GetHoverInfo(TextHoverRequest request)
	{
		var source = new StringTextSnapshot(request.DocumentText);

		return TryGetHoverWord(source, request.HoveredOffset, out string hoveredWord, out WordType type)
			? CreateHoverInfo(source, request.HoveredOffset, hoveredWord, type)
			: null;
	}

	private bool TryGetHoverWord(ITextSnapshot source, int hoveredOffset, out string hoveredWord, out WordType type)
	{
		string? word = _lineService.GetWordAtOffset(source, hoveredOffset);
		type = _lineService.GetWordTypeAtOffset(source, hoveredOffset);

		if (type == WordType.MnemonicConstant && !_mnemonicCatalogService.ContainsFlag(word))
			type = WordType.Unknown;

		if (type == WordType.Unknown && int.TryParse(word, out _))
			type = WordType.Decimal;

		if (word is not null && word.StartsWith('#'))
		{
			type = WordType.Directive;
			word = word.Split(' ')[0];
		}

		hoveredWord = word ?? string.Empty;
		return hoveredWord.Length > 0 && type != WordType.Unknown;
	}

	private TextHoverInfo? CreateHoverInfo(ITextSnapshot source, int hoveredOffset, string hoveredWord, WordType type)
	{
		if (type is WordType.MnemonicConstant or WordType.Hexadecimal or WordType.Decimal)
			return CreateConstantHoverInfo(source, hoveredOffset, hoveredWord, type);

		return new TextHoverInfo($"For more information about the \"{hoveredWord}\" {type}, Press F12.")
		{
			SymbolName = hoveredWord,
			DefinitionDiscriminator = GetDefinitionDiscriminator(type)
		};
	}

	private static TextDefinitionDiscriminator? GetDefinitionDiscriminator(WordType type)
		=> type == WordType.Header ? new ClassicScriptObjectDiscriminator(ObjectType.Section) : null;

	private TextHoverInfo? CreateConstantHoverInfo(ITextSnapshot source, int hoveredOffset, string hoveredWord, WordType type)
	{
		string? currentFlagPrefix = _commandService.GetFlagPrefixOfCurrentArgument(source, hoveredOffset);

		if (currentFlagPrefix is null)
		{
			return type == WordType.MnemonicConstant
				? new TextHoverInfo($"For more information about the \"{hoveredWord}\" Constant, Press F12.") { SymbolName = hoveredWord }
				: null;
		}

		if (!TryGetMnemonicInfo(hoveredWord, type, currentFlagPrefix, out string flagName, out string hexValue, out string decimalValue))
			return null;

		string content = $"{flagName}\n{hexValue}\n{decimalValue}\n\nFor more information about the \"{flagName}\" Constant, Press F12.";

		return new TextHoverInfo(content) { SymbolName = hoveredWord };
	}

	private bool TryGetMnemonicInfo(string hoveredWord, WordType type, string currentFlagPrefix, out string flagName, out string hexValue, out string decimalValue)
	{
		flagName = string.Empty;
		hexValue = string.Empty;
		decimalValue = string.Empty;

		bool found = type switch
		{
			WordType.MnemonicConstant => _mnemonicCatalogService.TryResolveFlagByValue(hoveredWord, false, null, out flagName)
				&& _mnemonicCatalogService.TryGetDescription(flagName, out _),
			WordType.Hexadecimal => _mnemonicCatalogService.TryResolveFlagByValue(hoveredWord, true, currentFlagPrefix, out flagName),
			WordType.Decimal => _mnemonicCatalogService.TryResolveFlagByValue(hoveredWord, false, currentFlagPrefix, out flagName),
			_ => false
		};

		if (!found)
			return false;

		if (!_mnemonicCatalogService.TryGetEntry(flagName, out ClassicScriptMnemonicEntry entry))
			return false;

		hexValue = entry.HexValue;
		decimalValue = entry.DecimalValue;
		return true;
	}
}
