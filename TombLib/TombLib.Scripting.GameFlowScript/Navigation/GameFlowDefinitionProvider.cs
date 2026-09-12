using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.Navigation;
using System;
using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.GameFlowScript.Types;

namespace TombLib.Scripting.GameFlowScript.Navigation;

/// <summary>
/// Resolves definition locations for GameFlow objects.
/// </summary>
public sealed class GameFlowDefinitionProvider : ITextDefinitionProvider
{
	private readonly IGameFlowScriptDocumentService _documentService;

	/// <summary>
	/// Initializes a new instance of the <see cref="GameFlowDefinitionProvider"/> class.
	/// </summary>
	/// <param name="documentService">The document service used to locate objects.</param>
	public GameFlowDefinitionProvider(IGameFlowScriptDocumentService documentService)
	{
		ArgumentNullException.ThrowIfNull(documentService);
		_documentService = documentService;
	}

	/// <summary>
	/// Gets the definition location for the given request.
	/// </summary>
	/// <param name="request">The definition request.</param>
	/// <returns>The definition location, or <c>null</c> when the object cannot be located.</returns>
	public TextDefinitionLocation? GetDefinition(TextDefinitionRequest request)
	{
		if (request.Identifier is not GameFlowObjectDiscriminator discriminator || string.IsNullOrWhiteSpace(request.SymbolName))
			return null;

		var source = new StringTextSnapshot(request.DocumentText);
		int? lineNumber = _documentService.FindDocumentLineOfObject(source, request.SymbolName, discriminator.ObjectType);

		return lineNumber is null ? null : new TextDefinitionLocation(lineNumber.Value);
	}
}
