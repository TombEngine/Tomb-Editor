using Nickelony.LanguageServer.Abstractions.Navigation;
using System;
using TombLib.Scripting.Navigation;
using TombLib.Scripting.Text;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Scripting.TRX.Navigation;

/// <summary>
/// Resolves definition locations for TRX level names.
/// </summary>
public sealed class TRXDefinitionProvider : ITextDefinitionProvider
{
	private readonly ITRXDocumentService _documentService;

	/// <summary>
	/// Initializes a new instance of the <see cref="TRXDefinitionProvider"/> class.
	/// </summary>
	/// <param name="documentService">The document service used to locate level definitions.</param>
	public TRXDefinitionProvider(ITRXDocumentService documentService)
	{
		ArgumentNullException.ThrowIfNull(documentService);
		_documentService = documentService;
	}

	/// <inheritdoc/>
	public TextDefinitionLocation? GetDefinition(TextDefinitionRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.SymbolName))
			return null;

		var source = new StringTextSnapshot(request.DocumentText);
		int? lineNumber = _documentService.FindDocumentLineOfLevel(source, request.SymbolName);

		return lineNumber is null ? null : new TextDefinitionLocation(lineNumber.Value);
	}
}
