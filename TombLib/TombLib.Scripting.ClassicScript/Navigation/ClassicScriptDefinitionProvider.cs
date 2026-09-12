using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.Navigation;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Types;

namespace TombLib.Scripting.ClassicScript.Navigation;

/// <summary>
/// Resolves definition locations for ClassicScript objects.
/// </summary>
public sealed class ClassicScriptDefinitionProvider : ITextDefinitionProvider
{
	private readonly IClassicScriptCommandService _commandService;

	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptDefinitionProvider"/> class.
	/// </summary>
	/// <param name="commandService">The command service used to locate objects.</param>
	public ClassicScriptDefinitionProvider(IClassicScriptCommandService commandService)
		=> _commandService = commandService;

	/// <summary>
	/// Gets the definition location for the given request.
	/// </summary>
	/// <param name="request">The definition request.</param>
	/// <returns>The definition location, or <c>null</c> when the object cannot be located.</returns>
	public TextDefinitionLocation? GetDefinition(TextDefinitionRequest request)
	{
		if (request.Identifier is not ClassicScriptObjectDiscriminator discriminator || string.IsNullOrWhiteSpace(request.SymbolName))
			return null;

		var source = new StringTextSnapshot(request.DocumentText);
		int? lineNumber = _commandService.FindDocumentLineOfObject(source, request.SymbolName, discriminator.ObjectType);

		return lineNumber is null ? null : new TextDefinitionLocation(lineNumber.Value);
	}
}
