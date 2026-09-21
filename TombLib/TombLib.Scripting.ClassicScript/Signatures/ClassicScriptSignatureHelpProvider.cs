using System;
using System.Collections.Generic;
using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.Signatures;
using TombLib.Scripting.ClassicScript.Services;

namespace TombLib.Scripting.ClassicScript.Signatures;

/// <summary>
/// Resolves signature help information for ClassicScript commands.
/// </summary>
public sealed class ClassicScriptSignatureHelpProvider : ITextSignatureHelpProvider
{
	private readonly IClassicScriptCommandService _commandService;

	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptSignatureHelpProvider"/> class.
	/// </summary>
	/// <param name="commandService">The command service used to resolve command syntax.</param>
	public ClassicScriptSignatureHelpProvider(IClassicScriptCommandService commandService)
		=> _commandService = commandService;

	/// <summary>
	/// Gets the signature help for the given request.
	/// </summary>
	/// <param name="request">The signature help request.</param>
	/// <returns>The signature help information, or <c>null</c> when the caret is not inside a known command.</returns>
	public TextSignatureHelp? GetSignatureHelp(TextSignatureHelpRequest request)
	{
		var source = new StringTextSnapshot(request.DocumentText);
		string? syntax = _commandService.GetCommandSyntax(source, request.CaretOffset);

		if (string.IsNullOrWhiteSpace(syntax))
			return null;

		int activeParameterIndex = _commandService.GetArgumentIndexAtOffset(source, request.CaretOffset);

		// The list-based model resolves the effective active parameter against the signature's
		// parameter list, so the syntax text's comma-separated argument fragments become the
		// parameters; the fragments are literal substrings of the label.
		string[] argumentTexts = syntax.Split(',');
		var parameters = new List<TextSignatureParameterInfo>(argumentTexts.Length);

		for (int i = 0; i < argumentTexts.Length; i++)
		{
			string argument = argumentTexts[i].Trim();

			if (i == 0 && argument.Contains('='))
				argument = argument[(argument.IndexOf('=') + 1)..].Trim();

			parameters.Add(new TextSignatureParameterInfo(argument));
		}

		// A caret past the last argument selects the last parameter here; the shared payload applies
		// the LSP default (first parameter) to an out-of-range index, so the provider clamps its own
		// caret-derived index to keep the local highlight behavior.
		activeParameterIndex = Math.Clamp(activeParameterIndex, 0, parameters.Count - 1);

		return new TextSignatureHelp(
			[new TextSignatureInformation(syntax, parameters: parameters)],
			activeParameter: TextSignatureActiveParameter.At(activeParameterIndex));
	}
}
