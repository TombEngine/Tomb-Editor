using TombLib.Scripting.ClassicScript.Completion;
using TombLib.Scripting.ClassicScript.Diagnostics;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Services;
using Nickelony.IDEKit.IntelliSense.Hover;
using Nickelony.IDEKit.IntelliSense.Navigation;
using Nickelony.IDEKit.IntelliSense.Signatures;

namespace TombLib.Scripting.ClassicScript;

/// <summary>
/// Aggregates the services used by the ClassicScript editor.
/// </summary>
public sealed class ClassicScriptLanguageServices
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptLanguageServices"/> class.
	/// </summary>
	/// <param name="definitionProvider">The definition provider.</param>
	/// <param name="hoverProvider">The hover provider.</param>
	/// <param name="signatureHelpProvider">The signature help provider.</param>
	/// <param name="errorDetector">The error detector.</param>
	/// <param name="lineService">The ClassicScript line service.</param>
	/// <param name="commandService">The ClassicScript command service.</param>
	/// <param name="indexService">The ClassicScript index service.</param>
	public ClassicScriptLanguageServices(
		ITextDefinitionProvider definitionProvider,
		ITextHoverProvider hoverProvider,
		ITextSignatureHelpProvider signatureHelpProvider,
		ErrorDetector errorDetector,
		IClassicScriptLineService lineService,
		IClassicScriptCommandService commandService,
		IClassicScriptIndexService indexService)
	{
		DefinitionProvider = definitionProvider;
		HoverProvider = hoverProvider;
		SignatureHelpProvider = signatureHelpProvider;
		ErrorDetector = errorDetector;
		LineService = lineService;
		CommandService = commandService;
		IndexService = indexService;
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
	/// Gets the signature help provider.
	/// </summary>
	public ITextSignatureHelpProvider SignatureHelpProvider { get; }

	/// <summary>
	/// Gets the error detector.
	/// </summary>
	public ErrorDetector ErrorDetector { get; }

	/// <summary>
	/// Gets the ClassicScript line service.
	/// </summary>
	public IClassicScriptLineService LineService { get; }

	/// <summary>
	/// Gets the ClassicScript command service.
	/// </summary>
	public IClassicScriptCommandService CommandService { get; }

	/// <summary>
	/// Gets the ClassicScript index service.
	/// </summary>
	public IClassicScriptIndexService IndexService { get; }

	/// <summary>
	/// Creates a completion session coordinator for a single editor instance.
	/// The coordinator tracks per-request completion ordering (it is shared across editors
	/// only at the service level, never as a single instance), so each editor owns its own
	/// coordinator created through this composition root.
	/// </summary>
	/// <returns>A completion session coordinator bound to this service set.</returns>
	public ClassicScriptCompletionSessionCoordinator CreateCompletionCoordinator()
		=> new(LineService, CommandService, new ClassicScriptMnemonicCatalogService());
}
