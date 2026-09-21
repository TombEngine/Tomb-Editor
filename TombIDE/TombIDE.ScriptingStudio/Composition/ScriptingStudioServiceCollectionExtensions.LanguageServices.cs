#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Host;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.Shared.Messaging.Scripting;
using TombIDE.Shared.SharedClasses;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Diagnostics;
using TombLib.Scripting.ClassicScript.Hover;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Navigation;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Signatures;
using TombLib.Scripting.ClassicScript.Syntaxes;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.GameFlowScript.Completion;
using TombLib.Scripting.GameFlowScript.Hover;
using TombLib.Scripting.GameFlowScript.Navigation;
using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.TRX;
using TombLib.Scripting.TRX.Completion;
using TombLib.Scripting.TRX.Hover;
using TombLib.Scripting.TRX.Navigation;
using TombLib.Scripting.TRX.Services;
using TombLib.Scripting.UI.Editing;
using TombLib.Scripting.UI.Editors;
using Nickelony.IDEKit.Workspace.Documents;

namespace TombIDE.ScriptingStudio.Composition;

/// <summary>
/// Registers the language-service providers for the supported script languages.
/// </summary>
public static partial class ScriptingStudioServiceCollectionExtensions
{
	private static void AddClassicScriptServices(IServiceCollection services)
	{
		services.AddSingleton<ClassicScriptMnemonicCatalogService>();
		services.AddSingleton<ClassicScriptSyntaxCatalogService>();
		services.AddSingleton<IClassicScriptLineService, ClassicScriptLineService>();
		services.AddSingleton<IClassicScriptCommandService, ClassicScriptCommandService>();
		services.AddSingleton<IClassicScriptIndexService, ClassicScriptIndexService>();
		services.AddSingleton<ClassicScriptLanguageServices>(sp =>
		{
			var lineService = sp.GetRequiredService<IClassicScriptLineService>();
			var commandService = sp.GetRequiredService<IClassicScriptCommandService>();
			var indexService = sp.GetRequiredService<IClassicScriptIndexService>();
			var mnemonicCatalogService = sp.GetRequiredService<ClassicScriptMnemonicCatalogService>();
			var syntaxCatalogService = sp.GetRequiredService<ClassicScriptSyntaxCatalogService>();
			var errorDetector = new ErrorDetector(lineService, commandService, syntaxCatalogService);

			return new ClassicScriptLanguageServices(
				new ClassicScriptDefinitionProvider(commandService),
				new ClassicScriptHoverProvider(lineService, commandService, mnemonicCatalogService),
				new ClassicScriptSignatureHelpProvider(commandService),
				errorDetector,
				lineService,
				commandService,
				indexService);
		});
	}

	private static void AddGameFlowServices(IServiceCollection services)
	{
		services.AddSingleton<IGameFlowScriptLineService, GameFlowScriptLineService>();
		services.AddSingleton<IGameFlowScriptDocumentService, GameFlowScriptDocumentService>();
		services.AddSingleton<GameFlowLanguageServices>(sp =>
		{
			var lineService = sp.GetRequiredService<IGameFlowScriptLineService>();
			var documentService = sp.GetRequiredService<IGameFlowScriptDocumentService>();

			return new GameFlowLanguageServices(
				new GameFlowDefinitionProvider(documentService),
				new GameFlowHoverProvider(),
				new GameFlowCompletionProvider(),
				lineService,
				documentService);
		});
	}

	private static void AddTrxServices(IServiceCollection services)
	{
		services.AddSingleton<ITRXGameFlowSchemaService>(_ =>
			new TRXGameFlowSchemaService(TRXResourcePaths.GetGameFlowSchemaPath()));
		services.AddSingleton<ITRXLineService, TRXLineService>();
		services.AddSingleton<ITRXDocumentService, TRXDocumentService>();
		services.AddSingleton<TRXLanguageServices>(sp =>
		{
			var schemaService = sp.GetRequiredService<ITRXGameFlowSchemaService>();
			var lineService = sp.GetRequiredService<ITRXLineService>();
			var documentService = sp.GetRequiredService<ITRXDocumentService>();

			return new TRXLanguageServices(
				schemaService,
				lineService,
				documentService,
				new TRXDefinitionProvider(documentService),
				new TRXGameFlowCompletionService(schemaService),
				new TRXGameFlowHoverService(schemaService));
		});
	}

	private static void AddLuaServices(IServiceCollection services)
	{
		// Lua workspace ownership:
		// - this scoped provider owns the language-server lifetime;
		// - LuaDocumentLifecycleCoordinator owns host event attachment and open/update/rename
		//   synchronization;
		// - LuaEditor owns its editor-local request state and releases its provider document
		//   reference during disposal;
		// - the child DI scope owns provider disposal; LuaIntellisenseEventBridge owns event
		//   subscriptions and detachment only.
		// Keep workspace automation, references, and workspace edits as host contributions.
		// They must not dispose the provider or duplicate editor document cleanup.
		services.AddScoped<ILuaLanguageServerIntelliSenseProvider>(sp =>
		{
			var projectContext = sp.GetRequiredService<IScriptingProjectContext>();

			TENApiService.InjectTENApi(
				projectContext.Project, projectContext.Project.GetCurrentEngineVersion());

			string? executablePath = LuaLanguageServerLocator.ResolveExecutablePath();

			ILogger<LuaLanguageServerIntelliSenseProvider> logger =
				sp.GetRequiredService<ILogger<LuaLanguageServerIntelliSenseProvider>>();

			return new LuaLanguageServerIntelliSenseProvider(
				[projectContext.ScriptRootDirectoryPath],
				executablePath,
				new LuaLanguageServerOptions
				{
					// Tomb scripts intentionally redefine table fields in places, so the duplicate-set-field diagnostic is suppressed.
					DisabledDiagnostics = [@"duplicate-set-field"]
				},
				logger);
		});

		// Lua tracked document state (manages per-document diagnostics and semantic tokens).
		services.AddScoped<LuaTrackedDocumentStateService>(sp =>
		{
			var textEditorHost = sp.GetRequiredService<ITextEditorHost>();
			var intellisenseProvider = sp.GetRequiredService<ILuaLanguageServerIntelliSenseProvider>();
			return new LuaTrackedDocumentStateService(textEditorHost, intellisenseProvider);
		});
		services.AddScoped<LuaReferenceSearchService>(sp =>
		{
			var projectContext = sp.GetRequiredService<IScriptingProjectContext>();
			var textEditorHost = sp.GetRequiredService<ITextEditorHost>();
			var intellisenseProvider = sp.GetRequiredService<ILuaLanguageServerIntelliSenseProvider>();
			var documentManager = sp.GetRequiredService<IWorkspaceDocumentManager>();
			return new LuaReferenceSearchService(
				textEditorHost,
				intellisenseProvider,
				projectContext.ScriptRootDirectoryPath,
				documentManager);
		});
		services.AddScoped<TextWorkspaceEditApplier>();
		services.AddScoped<TextWorkspaceCommandService>(sp =>
		{
			var editApplier = sp.GetRequiredService<TextWorkspaceEditApplier>();
			var intellisenseProvider = sp.GetRequiredService<ILuaLanguageServerIntelliSenseProvider>();
			return new TextWorkspaceCommandService(editApplier, intellisenseProvider);
		});

		// Lua IntelliSense event manager (wires provider events to the UI dispatcher).
		services.AddScoped<ILuaIntellisenseBridge>(sp =>
		{
			var dockHost = sp.GetRequiredService<IAvalonDockHost>();
			var messenger = sp.GetRequiredService<IMessenger>();
			var intellisenseProvider = sp.GetRequiredService<ILuaLanguageServerIntelliSenseProvider>();
			return new LuaIntellisenseEventBridge(dockHost, messenger, intellisenseProvider);
		});

		// Lua editor lifecycle coordinator (attaches/detaches Lua editor events).
		services.AddScoped<ILuaEditorLifecycleService>(sp =>
		{
			var documentController = sp.GetRequiredService<IEditorDocumentController>();
			var messenger = sp.GetRequiredService<IMessenger>();
			var intellisenseProvider = sp.GetRequiredService<ILuaLanguageServerIntelliSenseProvider>();
			var trackedDocumentStateService = sp.GetRequiredService<LuaTrackedDocumentStateService>();
			return new LuaDocumentLifecycleCoordinator(
				documentController,
				messenger,
				intellisenseProvider,
				trackedDocumentStateService);
		});
	}
}
