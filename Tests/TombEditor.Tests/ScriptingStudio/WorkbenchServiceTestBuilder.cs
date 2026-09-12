#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using Moq;
using MvvmDialogs;
using Nickelony.LanguageServer.Abstractions;
using Nickelony.LanguageServer.Lua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Diagnostics;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.FindAndReplace;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Navigation;
using TombIDE.ScriptingStudio.Shell;
using Nickelony.IDEKit.KeyBindings;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Workbench;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Docking;
using TombIDE.Shared.Messaging.Scripting;
using TombIDE.Shared.NewStructure;
using TombLib.Forms.ViewModels;
using TombLib.LevelData;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.Lua;
using TombLib.Scripting.TRX;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editing;
using TombLib.Scripting.UI.Editors;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Tests.ScriptingStudio;

internal sealed class WorkbenchServiceTestBuilder : IDisposable
{
	private readonly List<IEditorControl> _ownedEditors = [];
	private readonly Dictionary<IEditorControl, ScriptingDocumentRegistration> _registrations = [];
	private readonly Mock<IEditorDocumentController> _documentController = new();
	private IMessenger _messenger = new WeakReferenceMessenger();
	private readonly TaskCompletionSource<IReadOnlyList<TextReferenceLocation>> _referenceCompletion =
		new(TaskCreationOptions.RunContinuationsAsynchronously);
	private ScriptingDocumentContext _documentContext = ScriptingDocumentContext.Empty;
	private ScriptingWorkspaceProfile? _profile;
	private PaneCatalog? _paneCatalog;
	private Mock<IAvalonDockHost>? _dockHost;
	private bool _ownsPaneCatalog;
	private WorkbenchService? _workbench;
	private Mock<ITextReferencesProvider> _referencesProvider = new();
	private Mock<ITextEditProvider> _editProvider = new();
	private Mock<IMessageService> _messageService = new();
	private Mock<IDialogService> _dialogService = new();
	private Mock<IGameProject>? _project;
	private ILuaEditorLifecycleService? _luaEditorLifecycleService;
	private bool _ownsLuaEditorLifecycleService = true;
	private ILuaIntellisenseBridge? _luaIntellisenseBridge;
	private bool _ownsLuaIntellisenseBridge = true;
	private LuaTrackedDocumentStateService? _luaTrackedDocumentStateService;
	private LuaReferenceSearchService? _luaReferenceSearchService;
	private TextWorkspaceCommandService? _luaWorkspaceCommandService;
	private CancellationToken? _referenceCancellationToken;
	private bool _disposed;

	public WorkbenchServiceTestBuilder()
	{
		_profile = ScriptingWorkspaceProfileTestFactory.CreateLuaProfile(
			viewCommands: [UICommand.LuaDiagnostics, UICommand.LuaReferencesResults],
			supportsLuaActivation: true);
		_luaEditorLifecycleService = new Mock<ILuaEditorLifecycleService>().Object;
		_luaIntellisenseBridge = new Mock<ILuaIntellisenseBridge>().Object;
		ConfigureController();

		_referencesProvider
			.SetupGet(provider => provider.SupportsReferences)
			.Returns(true);
		_referencesProvider
			.Setup(provider => provider.GetReferencesAsync(It.IsAny<TextReferenceRequest>(), It.IsAny<CancellationToken>()))
			.Callback<TextReferenceRequest, CancellationToken>((_, cancellationToken) => _referenceCancellationToken = cancellationToken)
			.Returns(_referenceCompletion.Task);
	}

	public Mock<IEditorDocumentController> DocumentController => _documentController;

	public IMessenger Messenger => _messenger;

	public Mock<ITextReferencesProvider> ReferencesProvider => _referencesProvider;

	public Mock<ITextEditProvider> EditProvider => _editProvider;

	public Mock<IMessageService> MessageService => _messageService;

	public Mock<IDialogService> DialogService => _dialogService;

	public TaskCompletionSource<IReadOnlyList<TextReferenceLocation>> ReferenceCompletion => _referenceCompletion;

	public CancellationToken? ReferenceCancellationToken => _referenceCancellationToken;

	public WorkbenchService Workbench
		=> _workbench ?? throw new InvalidOperationException("The workbench has not been built.");

	public PaneCatalog PaneCatalog
		=> _paneCatalog ?? throw new InvalidOperationException("The pane catalog has not been built.");

	public WorkbenchServiceTestBuilder WithProfile(ScriptingWorkspaceProfile profile)
	{
		ArgumentNullException.ThrowIfNull(profile);
		_profile = profile;
		return this;
	}

	public WorkbenchServiceTestBuilder WithPaneCatalog(PaneCatalog paneCatalog)
	{
		ArgumentNullException.ThrowIfNull(paneCatalog);
		_paneCatalog = paneCatalog;
		_ownsPaneCatalog = false;
		return this;
	}

	public WorkbenchServiceTestBuilder WithDockHost(Mock<IAvalonDockHost> dockHost)
	{
		ArgumentNullException.ThrowIfNull(dockHost);
		_dockHost = dockHost;
		return this;
	}

	public WorkbenchServiceTestBuilder WithMessageService(Mock<IMessageService> messageService)
	{
		ArgumentNullException.ThrowIfNull(messageService);
		_messageService = messageService;
		return this;
	}

	public WorkbenchServiceTestBuilder WithDialogService(Mock<IDialogService> dialogService)
	{
		ArgumentNullException.ThrowIfNull(dialogService);
		_dialogService = dialogService;
		return this;
	}

	public WorkbenchServiceTestBuilder WithMessenger(IMessenger messenger)
	{
		ArgumentNullException.ThrowIfNull(messenger);
		_messenger = messenger;
		return this;
	}

	public WorkbenchServiceTestBuilder WithProject(Mock<IGameProject> project)
	{
		ArgumentNullException.ThrowIfNull(project);
		_project = project;
		return this;
	}

	public WorkbenchServiceTestBuilder WithReferenceProvider(Mock<ITextReferencesProvider> referencesProvider)
	{
		ArgumentNullException.ThrowIfNull(referencesProvider);
		_referencesProvider = referencesProvider;
		return this;
	}

	public WorkbenchServiceTestBuilder WithEditProvider(Mock<ITextEditProvider> editProvider)
	{
		ArgumentNullException.ThrowIfNull(editProvider);
		_editProvider = editProvider;
		return this;
	}

	public WorkbenchServiceTestBuilder WithLuaCapabilities(
		ILuaEditorLifecycleService? luaEditorLifecycleService,
		ILuaIntellisenseBridge? luaIntellisenseBridge,
		LuaTrackedDocumentStateService? luaTrackedDocumentStateService,
		LuaReferenceSearchService? luaReferenceSearchService,
		TextWorkspaceCommandService? luaWorkspaceCommandService)
	{
		_luaEditorLifecycleService = luaEditorLifecycleService;
		_ownsLuaEditorLifecycleService = false;
		_luaIntellisenseBridge = luaIntellisenseBridge;
		_ownsLuaIntellisenseBridge = false;
		_luaTrackedDocumentStateService = luaTrackedDocumentStateService;
		_luaReferenceSearchService = luaReferenceSearchService;
		_luaWorkspaceCommandService = luaWorkspaceCommandService;
		return this;
	}

	public void AddEditor(IEditorControl editor, ScriptingDocumentRegistration registration)
	{
		ArgumentNullException.ThrowIfNull(editor);
		ArgumentNullException.ThrowIfNull(registration);

		_ownedEditors.Add(editor);
		_registrations.Add(editor, registration);
	}

	public void Activate(IEditorControl editor, ScriptingDocumentRegistration registration)
	{
		ArgumentNullException.ThrowIfNull(editor);
		ArgumentNullException.ThrowIfNull(registration);

		_documentContext = new ScriptingDocumentContext(
			_documentContext.Generation + 1,
			editor,
			editor.FilePath,
			registration);
		_documentController.Raise(
			controller => controller.CurrentEditorChanged += null,
			new ScriptingDocumentContextChangedEventArgs(_documentContext));
	}

	public void SetNoDocument()
	{
		_documentContext = new ScriptingDocumentContext(_documentContext.Generation + 1, null, null, null);
		_documentController.Raise(
			controller => controller.CurrentEditorChanged += null,
			new ScriptingDocumentContextChangedEventArgs(_documentContext));
	}

	public void RaiseEditorClosed(IEditorControl editor)
	{
		ArgumentNullException.ThrowIfNull(editor);
		_documentController.Raise(
			controller => controller.EditorClosed += null,
			new EditorControlEventArgs(editor));
	}

	public void CloseEditor(IEditorControl editor)
	{
		ArgumentNullException.ThrowIfNull(editor);
		_ownedEditors.Remove(editor);
		RaiseEditorClosed(editor);
	}

	public void RaiseFileOpened(IEditorControl editor)
	{
		ArgumentNullException.ThrowIfNull(editor);
		_documentController.Raise(controller => controller.FileOpened += null, editor, EventArgs.Empty);
	}

	public WeakReference DisposeWorkbench()
	{
		WorkbenchService workbench = _workbench
			?? throw new InvalidOperationException("The workbench has not been built.");
		_workbench = null;
		workbench.Dispose();
		_documentController.Invocations.Clear();
		return new WeakReference(workbench);
	}

	public ScriptingDocumentRegistration CreateRegistration(
		DocumentMode documentMode,
		IEditorControl editor,
		ScriptingDocumentConfigurationKind configurationKind)
		=> new(
			EditorType.Text,
			documentMode,
			_ => true,
			_ => true,
			_ => editor,
			new(null, configurationKind));

	public WorkbenchService Build()
	{
		if (_workbench is not null)
			throw new InvalidOperationException("The workbench has already been built.");

		ScriptingWorkspaceProfile profile = _profile
			?? throw new InvalidOperationException("A workspace profile is required.");
		if (_paneCatalog is null)
		{
			_paneCatalog = new PaneCatalog([
				new DocumentDiagnosticsPaneProvider(profile, _documentController.Object),
				new LuaReferencesPaneProvider(profile, _documentController.Object)]);
			_ownsPaneCatalog = true;
		}

		ITextEditorHost textEditorHost = CreateTextEditorHost();
		_luaTrackedDocumentStateService ??= CreateLuaTrackedDocumentStateService(textEditorHost);
		_luaReferenceSearchService ??= new LuaReferenceSearchService(
			textEditorHost,
			_referencesProvider.Object,
			@"C:\Scripts");
		_luaWorkspaceCommandService ??= new TextWorkspaceCommandService(
			new TextWorkspaceEditApplier(textEditorHost),
			_editProvider.Object);

		var projectContext = new Mock<IScriptingProjectContext>();
		Mock<IGameProject> project = _project ?? new Mock<IGameProject>();
		if (_project is null)
		{
			project.Setup(value => value.GetEngineRootDirectoryPath()).Returns(@"C:\Engine");
			project.Setup(value => value.GetEngineExecutableFilePath()).Returns(@"C:\Engine\Game.exe");
			project.Setup(value => value.GetCurrentEngineVersion()).Returns(new Version(4, 8));
		}
		projectContext.Setup(value => value.Project).Returns(project.Object);

		Mock<IAvalonDockHost> dockHost = _dockHost ?? new Mock<IAvalonDockHost>();
		dockHost.Setup(value => value.View).Returns(Mock.Of<FrameworkElement>());
		dockHost.Setup(value => value.Dispatcher).Returns(Dispatcher.CurrentDispatcher);
		dockHost.Setup(value => value.SaveLayout()).Returns(string.Empty);

		ClassicScriptLanguageServices classicScriptLanguageServices = ScriptingLanguageServicesTestFactory.CreateClassicScript();
		GameFlowLanguageServices gameFlowLanguageServices = ScriptingLanguageServicesTestFactory.CreateGameFlowScript();
		TRXLanguageServices trxLanguageServices = ScriptingLanguageServicesTestFactory.CreateTRX();
		var findAndReplaceViewModel = new FindAndReplaceViewModel(
			_documentController.Object,
			_messenger,
			new FindReplaceService());

		LuaHostServices? luaHostServices = profile.SupportsLua ? CreateLuaHostServices() : null;

		_workbench = WorkbenchServiceTestFactory.Create(
			profile,
			projectContext.Object,
			_messenger,
			_messageService.Object,
			CreateKeyBindingService(),
			ScriptingStudioChromeTestFixture.CreateMenuServiceMock().Object,
			ScriptingStudioChromeTestFixture.CreateToolBarServiceMock().Object,
			ScriptingStudioChromeTestFixture.CreateStatusBarServiceMock().Object,
			new Mock<IPaneHostService>().Object,
			new Mock<IWin32DialogOwnerProvider>().Object,
			_documentController.Object,
			dockHost.Object,
			_paneCatalog,
			findAndReplaceViewModel,
			luaHostServices,
			_dialogService.Object,
			() => false,
			() => false,
			classicScriptLanguageServices,
			gameFlowLanguageServices,
			trxLanguageServices);

		return _workbench;
	}

	private LuaHostServices CreateLuaHostServices()
		=> new(
			_luaEditorLifecycleService ?? throw new InvalidOperationException("Lua editor lifecycle service is not configured."),
			_luaIntellisenseBridge ?? throw new InvalidOperationException("Lua IntelliSense bridge is not configured."),
			_luaTrackedDocumentStateService ?? throw new InvalidOperationException("Lua tracked document state service is not configured."),
			_luaReferenceSearchService ?? throw new InvalidOperationException("Lua reference search service is not configured."),
			_luaWorkspaceCommandService ?? throw new InvalidOperationException("Lua workspace command service is not configured."));

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;
		_workbench?.Dispose();

		if (_ownsPaneCatalog)
			_paneCatalog?.Dispose();

		if (_ownsLuaEditorLifecycleService)
			_luaEditorLifecycleService?.Dispose();

		if (_ownsLuaIntellisenseBridge)
			_luaIntellisenseBridge?.Dispose();

		foreach (IEditorControl editor in _ownedEditors)
			editor.Dispose();
	}

	private void ConfigureController()
	{
		_documentController.SetupGet(controller => controller.CurrentEditor).Returns(() => _documentContext.Editor);
		_documentController.SetupGet(controller => controller.CurrentDocumentContext).Returns(() => _documentContext);
		_documentController.SetupGet(controller => controller.ScriptRootDirectoryPath).Returns(@"C:\Scripts");
		_documentController.Setup(controller => controller.GetOpenEditors()).Returns(() => _ownedEditors);
		_documentController
			.Setup(controller => controller.GetDocumentRegistration(It.IsAny<IEditorControl?>()))
			.Returns((IEditorControl? editor) => editor is not null && _registrations.TryGetValue(editor, out ScriptingDocumentRegistration? registration)
				? registration
				: null);
		_documentController
			.Setup(controller => controller.FindEditorsOfFile(It.IsAny<string>()))
			.Returns((string filePath) => _ownedEditors.Where(editor =>
				string.Equals(editor.FilePath, filePath, StringComparison.OrdinalIgnoreCase)));
		_documentController.Setup(controller => controller.IsEveryDocumentSaved()).Returns(true);
	}

	private ITextEditorHost CreateTextEditorHost()
	{
		var textEditorHost = new Mock<ITextEditorHost>();
		textEditorHost
			.Setup(host => host.GetOpenEditors(It.IsAny<string>()))
			.Returns((string filePath) => _ownedEditors
				.Where(editor => string.Equals(editor.FilePath, filePath, StringComparison.OrdinalIgnoreCase))
				.ToArray());
		return textEditorHost.Object;
	}

	private LuaTrackedDocumentStateService CreateLuaTrackedDocumentStateService(ITextEditorHost textEditorHost)
	{
		var intellisenseProvider = new Mock<ILuaIntelliSenseProvider>();
		intellisenseProvider.Setup(provider => provider.GetDiagnostics(It.IsAny<string>())).Returns([]);
		intellisenseProvider.Setup(provider => provider.GetSemanticTokens(It.IsAny<string>())).Returns([]);
		return new LuaTrackedDocumentStateService(textEditorHost, intellisenseProvider.Object);
	}

	private static IKeyBindingService<UICommand> CreateKeyBindingService()
		=> new KeyBindingService<UICommand>(
			new CommandCatalog<UICommand>([]),
			new KeyBindingOverrideCollection(),
			_ => true);

}
