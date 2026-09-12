using System;
using TombLib.Scripting.GameFlowScript.Completion;
using TombLib.Scripting.GameFlowScript.Services;
using Nickelony.IDEKit.IntelliSense.Completion;
using Nickelony.IDEKit.IntelliSense.Hover;
using Nickelony.IDEKit.IntelliSense.Navigation;

namespace TombLib.Scripting.GameFlowScript;

/// <summary>
/// Aggregates the services used by the GameFlow editor.
/// </summary>
public sealed class GameFlowLanguageServices
{
	private readonly GameFlowCompletionProvider _completionProvider;

	/// <summary>
	/// Initializes a new instance of the <see cref="GameFlowLanguageServices"/> class.
	/// </summary>
	/// <param name="definitionProvider">The definition provider.</param>
	/// <param name="hoverProvider">The hover provider.</param>
	/// <param name="completionProvider">The completion provider.</param>
	/// <param name="lineService">The GameFlow line service.</param>
	/// <param name="documentService">The GameFlow document service.</param>
	public GameFlowLanguageServices(
		ITextDefinitionProvider definitionProvider,
		ITextHoverProvider hoverProvider,
		GameFlowCompletionProvider completionProvider,
		IGameFlowScriptLineService lineService,
		IGameFlowScriptDocumentService documentService)
	{
		ArgumentNullException.ThrowIfNull(definitionProvider);
		ArgumentNullException.ThrowIfNull(hoverProvider);
		ArgumentNullException.ThrowIfNull(completionProvider);
		ArgumentNullException.ThrowIfNull(lineService);
		ArgumentNullException.ThrowIfNull(documentService);

		DefinitionProvider = definitionProvider;
		HoverProvider = hoverProvider;
		_completionProvider = completionProvider;
		LineService = lineService;
		DocumentService = documentService;
	}

	/// <summary>
	/// Gets the definition provider.
	/// </summary>
	public ITextDefinitionProvider DefinitionProvider { get; }

	/// <summary>
	/// Gets the hover provider.
	/// </summary>
	public ITextHoverProvider HoverProvider { get; }

	/// <summary>
	/// Gets the completion provider.
	/// </summary>
	public ITextCompletionProvider CompletionProvider => _completionProvider;

	/// <summary>
	/// Gets the GameFlow line service.
	/// </summary>
	public IGameFlowScriptLineService LineService { get; }

	/// <summary>
	/// Gets the GameFlow document service.
	/// </summary>
	public IGameFlowScriptDocumentService DocumentService { get; }

	/// <summary>
	/// Creates a completion session coordinator for a single editor instance.
	/// Each editor owns its own coordinator created through this composition root.
	/// </summary>
	/// <returns>A completion session coordinator bound to this service set.</returns>
	public GameFlowCompletionSessionCoordinator CreateCompletionCoordinator() => new(_completionProvider, LineService);
}
