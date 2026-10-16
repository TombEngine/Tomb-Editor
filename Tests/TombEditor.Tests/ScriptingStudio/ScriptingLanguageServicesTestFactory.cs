using Moq;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Diagnostics;
using TombLib.Scripting.ClassicScript.Hover;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Navigation;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Signatures;
using TombLib.Scripting.ClassicScript.Syntaxes;
using Nickelony.IDEKit.IntelliSense.Completion;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.GameFlowScript.Completion;
using TombLib.Scripting.GameFlowScript.Hover;
using TombLib.Scripting.GameFlowScript.Navigation;
using TombLib.Scripting.GameFlowScript.Services;
using Nickelony.IDEKit.IntelliSense.Hover;
using Nickelony.IDEKit.IntelliSense.Navigation;
using Nickelony.IDEKit.IntelliSense.Signatures;
using TombLib.Scripting.TRX;
using TombLib.Scripting.TRX.Completion;
using TombLib.Scripting.TRX.Hover;
using TombLib.Scripting.TRX.Navigation;
using TombLib.Scripting.TRX.Services;

namespace TombEditor.Tests.ScriptingStudio;

internal static class ScriptingLanguageServicesTestFactory
{
    public static ClassicScriptLanguageServices CreateClassicScript()
    {
        var lineService = new ClassicScriptLineService();
        var mnemonicCatalogService = new ClassicScriptMnemonicCatalogService();
        var syntaxCatalogService = new ClassicScriptSyntaxCatalogService();
        var commandService = new ClassicScriptCommandService(lineService, mnemonicCatalogService, syntaxCatalogService);
        var indexService = new ClassicScriptIndexService(commandService, lineService, mnemonicCatalogService);
        var errorDetector = new ErrorDetector(lineService, commandService, syntaxCatalogService);

        return new ClassicScriptLanguageServices(
            new ClassicScriptDefinitionProvider(commandService),
            new ClassicScriptHoverProvider(lineService, commandService, mnemonicCatalogService),
            new ClassicScriptSignatureHelpProvider(commandService),
            errorDetector,
            lineService,
            commandService,
            indexService);
    }

    public static ClassicScriptLanguageServices CreateClassicScriptStub()
    {
        var lineService = new Mock<IClassicScriptLineService>().Object;
        var commandService = new Mock<IClassicScriptCommandService>().Object;

        return new ClassicScriptLanguageServices(
            new Mock<ITextDefinitionProvider>().Object,
            new Mock<ITextHoverProvider>().Object,
            new Mock<ITextSignatureHelpProvider>().Object,
            new ErrorDetector(
                lineService,
                commandService,
                new ClassicScriptSyntaxCatalogService()),
            lineService,
            commandService,
            new Mock<IClassicScriptIndexService>().Object);
    }

    public static GameFlowLanguageServices CreateGameFlowScript()
    {
        var lineService = new GameFlowScriptLineService();
        var documentService = new GameFlowScriptDocumentService(lineService);

        return new GameFlowLanguageServices(
            new GameFlowDefinitionProvider(documentService),
            new GameFlowHoverProvider(),
            new GameFlowCompletionProvider(),
            lineService,
            documentService);
    }

    public static GameFlowLanguageServices CreateGameFlowScriptStub()
        => new(
            new Mock<ITextDefinitionProvider>().Object,
            new Mock<ITextHoverProvider>().Object,
            new GameFlowCompletionProvider(),
            new Mock<IGameFlowScriptLineService>().Object,
            new Mock<IGameFlowScriptDocumentService>().Object);

    public static TRXLanguageServices CreateTRX()
    {
        var lineService = new TRXLineService();
        var documentService = new TRXDocumentService(lineService);
        var schemaService = new TRXGameFlowSchemaService(TRXResourcePaths.GetGameFlowSchemaPath());

        return new TRXLanguageServices(
            schemaService,
            lineService,
            documentService,
            new TRXDefinitionProvider(documentService),
            new TRXGameFlowCompletionService(schemaService),
            new TRXGameFlowHoverService(schemaService));
    }

    public static TRXLanguageServices CreateTRXStub()
    {
        var lineService = new Mock<ITRXLineService>().Object;
        var documentService = new Mock<ITRXDocumentService>().Object;

        return new TRXLanguageServices(
            new Mock<ITRXGameFlowSchemaService>().Object,
            lineService,
            documentService,
            new Mock<ITextDefinitionProvider>().Object,
            new Mock<ITextCompletionProvider>().Object,
            new Mock<ITextHoverProvider>().Object);
    }
}