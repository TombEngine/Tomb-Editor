#nullable enable

using Nickelony.IDEKit.IntelliSense.DocumentSymbols;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.ContentNodes;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.GameFlowScript.ContentNodes;
using TombLib.Scripting.TRX;
using TombLib.Scripting.TRX.ContentNodes;

namespace TombIDE.ScriptingStudio.DocumentOutline;

internal sealed class DocumentOutlineNodesProviderFactory
{
	private readonly ClassicScriptLanguageServices? _languageServices;
	private readonly GameFlowLanguageServices? _gameFlowLanguageServices;
	private readonly TRXLanguageServices? _trxLanguageServices;

	public DocumentOutlineNodesProviderFactory(
		ClassicScriptLanguageServices? languageServices,
		GameFlowLanguageServices? gameFlowLanguageServices,
		TRXLanguageServices? trxLanguageServices)
	{
		_languageServices = languageServices;
		_gameFlowLanguageServices = gameFlowLanguageServices;
		_trxLanguageServices = trxLanguageServices;
	}

	public ITextDocumentSymbolProvider? CreateClassicScript()
		=> _languageServices is null ? null : new ClassicScriptNodesProvider(_languageServices.LineService);

	public ITextDocumentSymbolProvider? CreateStrings()
		=> _languageServices is null ? null : new StringFileNodesProvider(_languageServices.LineService);

	public ITextDocumentSymbolProvider? CreateGameFlowScript()
		=> _gameFlowLanguageServices is null ? null : new GameFlowNodesProvider(_gameFlowLanguageServices.LineService);

	public ITextDocumentSymbolProvider? CreateTrx()
		=> _trxLanguageServices is null ? null : new TRXNodesProvider(_trxLanguageServices.LineService);
}
