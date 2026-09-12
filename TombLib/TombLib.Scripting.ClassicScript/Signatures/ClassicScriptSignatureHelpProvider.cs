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
	public TextSignatureHelpInfo? GetSignatureHelp(TextSignatureHelpRequest request)
	{
		var source = new StringTextSnapshot(request.DocumentText);
		string? syntax = _commandService.GetCommandSyntax(source, request.CaretOffset);

		if (string.IsNullOrWhiteSpace(syntax))
			return null;

		int activeParameterIndex = _commandService.GetArgumentIndexAtOffset(source, request.CaretOffset);
		return new TextSignatureHelpInfo(syntax, activeParameterIndex);
	}
}
