using System;
using Nickelony.IDEKit.IntelliSense.Completion;
using Nickelony.IDEKit.IntelliSense.Hover;
using Nickelony.IDEKit.IntelliSense.Navigation;
using TombLib.Scripting.TRX.Completion;
using TombLib.Scripting.TRX.Diagnostics;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Scripting.TRX;

/// <summary>
/// Aggregates the services used by the TRX editor.
/// </summary>
public sealed class TRXLanguageServices
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TRXLanguageServices"/> class.
	/// </summary>
	/// <param name="schemaService">The GameFlow schema service.</param>
	/// <param name="lineService">The TRX line service.</param>
	/// <param name="documentService">The TRX document service.</param>
	/// <param name="definitionProvider">The definition provider.</param>
	/// <param name="completionProvider">The completion provider.</param>
	/// <param name="hoverProvider">The hover provider.</param>
	public TRXLanguageServices(
		ITRXGameFlowSchemaService schemaService,
		ITRXLineService lineService,
		ITRXDocumentService documentService,
		ITextDefinitionProvider definitionProvider,
		ITextCompletionProvider completionProvider,
		ITextHoverProvider hoverProvider)
	{
		ArgumentNullException.ThrowIfNull(schemaService);
		ArgumentNullException.ThrowIfNull(lineService);
		ArgumentNullException.ThrowIfNull(documentService);
		ArgumentNullException.ThrowIfNull(definitionProvider);
		ArgumentNullException.ThrowIfNull(completionProvider);
		ArgumentNullException.ThrowIfNull(hoverProvider);

		SchemaService = schemaService;
		LineService = lineService;
		DocumentService = documentService;
		DefinitionProvider = definitionProvider;
		CompletionProvider = completionProvider;
		HoverProvider = hoverProvider;
		ErrorDetector = new ErrorDetector(lineService);
	}

	/// <summary>
	/// Gets the GameFlow schema service.
	/// </summary>
	public ITRXGameFlowSchemaService SchemaService { get; }

	/// <summary>
	/// Gets the TRX line service.
	/// </summary>
	public ITRXLineService LineService { get; }

	/// <summary>
	/// Gets the TRX document service.
	/// </summary>
	public ITRXDocumentService DocumentService { get; }

	/// <summary>
	/// Gets the definition provider.
	/// </summary>
	public ITextDefinitionProvider DefinitionProvider { get; }

	/// <summary>
	/// Gets the completion provider.
	/// </summary>
	public ITextCompletionProvider CompletionProvider { get; }

	/// <summary>
	/// Gets the hover provider.
	/// </summary>
	public ITextHoverProvider HoverProvider { get; }

	/// <summary>
	/// Gets the error detector used to diagnose TRX documents.
	/// </summary>
	public ErrorDetector ErrorDetector { get; }

	/// <summary>
	/// Creates a completion session coordinator for a single editor instance.
	/// Each editor owns its own coordinator created through this composition root; the
	/// coordinator's analysis and filtering helpers are constructed here as well.
	/// </summary>
	/// <returns>A completion session coordinator bound to this service set.</returns>
	public TRXCompletionSessionCoordinator CreateCompletionCoordinator()
		=> new(CompletionProvider, new TextAnalysisService(), new CompletionManager(LineService));
}
