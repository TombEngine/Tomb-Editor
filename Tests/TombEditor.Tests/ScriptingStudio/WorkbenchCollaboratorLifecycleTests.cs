#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using Moq;
using MvvmDialogs;
using Nickelony.LanguageServer.Abstractions;
using Nickelony.LanguageServer.Lua;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TombIDE.ScriptingStudio.ClassicScript;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.FindAndReplace;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Messaging;
using TombIDE.ScriptingStudio.Navigation;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Workbench;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Messaging.Scripting;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.Lua;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;
using TombLib.LevelData;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class WorkbenchCollaboratorLifecycleTests
{
	[TestMethod]
	public void DialogCoordinator_DisposeClosesModelessViewOnce()
	{
		StaTestHelper.RunInSta(() =>
		{
			var coordinator = new WorkbenchDialogCoordinator(
				new Mock<IDialogService>().Object,
				new Mock<IWin32DialogOwnerProvider>().Object,
				CreateFindAndReplaceViewModel(new Mock<IEditorDocumentController>().Object));

			FieldInfo field = typeof(WorkbenchDialogCoordinator).GetField(
				"_findAndReplaceView",
				BindingFlags.Instance | BindingFlags.NonPublic)
				?? throw new AssertFailedException("The dialog view field was not found.");
			var view = (FindAndReplaceView)field.GetValue(coordinator)!;

			coordinator.Dispose();
			coordinator.Dispose();

			FieldInfo closingField = typeof(FindAndReplaceView).GetField(
				"_isActuallyClosing",
				BindingFlags.Instance | BindingFlags.NonPublic)
				?? throw new AssertFailedException("The dialog close state field was not found.");
			Assert.IsTrue((bool)closingField.GetValue(view)!);
		});
	}

	[TestMethod]
	public void ReferenceBrowserProvider_DisposeClosesReferenceInfoAndDetachesViewModel()
	{
		StaTestHelper.RunInSta(() =>
		{
			ResourceDictionary[] addedResources = EnsureDarkUiResources(out System.Windows.Application application);
			string scriptDirectoryPath = System.IO.Directory.CreateTempSubdirectory("TombEditor-ReferenceInfo-").FullName;
			try
			{
				System.IO.File.WriteAllText(System.IO.Path.Combine(scriptDirectoryPath, "Script.txt"), string.Empty);
				ScriptingWorkspaceProfile profile = ScriptingWorkspaceProfileTestFactory.CreateSelectorProfile(
					TRVersion.Game.TR4,
					false,
					scriptDirectoryPath);
				var localizationService = new Mock<ILocalizationService>();
				localizationService
					.Setup(service => service[It.IsAny<string>()])
					.Returns((string key) => key);
				localizationService
					.Setup(service => service.WithKeysFor(It.IsAny<System.ComponentModel.INotifyPropertyChanged>()))
					.Returns(localizationService.Object);
				var referenceInfoViewModel = new ReferenceInfoViewModel(
					() => false,
					_ => { },
					() => false,
					_ => { });
				var provider = new ReferenceBrowserPaneProvider(
					profile,
					new ReferenceBrowserViewModel(new Mock<IMessageService>().Object, localizationService.Object),
					referenceInfoViewModel);

				FieldInfo viewField = typeof(ReferenceBrowserPaneProvider).GetField(
					"_referenceInfoView",
					BindingFlags.Instance | BindingFlags.NonPublic)
					?? throw new AssertFailedException("The reference-info view field was not found.");
				var view = (ReferenceInfoView)viewField.GetValue(provider)!;
				view.Show("TEST", "Description");
				Assert.AreEqual(1, referenceInfoViewModel.Tabs.Count);

				provider.Dispose();
				provider.Dispose();

				Assert.IsFalse(view.IsVisible);
				FieldInfo requestHideField = typeof(ReferenceInfoViewModel).GetField(
					"RequestHide",
					BindingFlags.Instance | BindingFlags.NonPublic)
					?? throw new AssertFailedException("The reference-info event field was not found.");
				Assert.IsNull(requestHideField.GetValue(referenceInfoViewModel));
			}
			finally
			{
				foreach (ResourceDictionary resource in addedResources)
					application.Resources.MergedDictionaries.Remove(resource);

				if (System.IO.Directory.Exists(scriptDirectoryPath))
					System.IO.Directory.Delete(scriptDirectoryPath, recursive: true);
			}
		});
	}

	[TestMethod]
	public void LuaEventCoordinator_DisposeUnregistersMessengerHandlers()
	{
		StaTestHelper.RunInSta(() =>
		{
			var messenger = new WeakReferenceMessenger();
			var messageService = new Mock<IMessageService>();
			var coordinator = new LuaWorkbenchEventCoordinator(
				messenger,
				ScriptingWorkspaceProfileTestFactory.CreateLuaProfile(supportsLuaActivation: true),
				new Mock<IEditorDocumentController>().Object,
				CreatePaneCoordinator(),
				CreateLuaHostServices(),
				messageService.Object);

			coordinator.Dispose();
			coordinator.Dispose();
			messenger.Send(new LuaWorkspaceWatcherFailedMessage(new WorkspaceWatcherFailure("late watcher failure")));

			messageService.Verify(
				service => service.ShowInformation("late watcher failure", "Lua IntelliSense"),
				Times.Never);
		});
	}

	[TestMethod]
	public void CodeNavigationCoordinator_DisposeCancelsPendingReferenceSearch()
	{
		StaTestHelper.RunInSta(() =>
		{
			var editor = new LuaEditor(new Version(1, 0))
			{
				FilePath = @"C:\Scripts\active.lua"
			};
			var documentController = new Mock<IEditorDocumentController>();
			documentController.SetupGet(controller => controller.CurrentEditor).Returns(editor);
			ScriptingDocumentRegistration registration = new(
				EditorType.Text,
				DocumentMode.Lua,
				_ => true,
				_ => true,
				_ => editor,
				new(null, ScriptingDocumentConfigurationKind.Lua));
			documentController.SetupGet(controller => controller.CurrentDocumentContext).Returns(
				new ScriptingDocumentContext(1, editor, editor.FilePath, registration));
			var completion = new TaskCompletionSource<IReadOnlyList<TextReferenceLocation>>(
				TaskCreationOptions.RunContinuationsAsynchronously);
			var requestStarted = new TaskCompletionSource<CancellationToken>(
				TaskCreationOptions.RunContinuationsAsynchronously);
			var referencesProvider = new Mock<ILanguageServerReferencesProvider>();
			referencesProvider.SetupGet(provider => provider.SupportsReferences).Returns(true);
			referencesProvider
				.Setup(provider => provider.GetReferencesAsync(It.IsAny<TextReferenceRequest>(), It.IsAny<CancellationToken>()))
				.Callback<TextReferenceRequest, CancellationToken>((_, token) => requestStarted.SetResult(token))
				.Returns(completion.Task);
			ITextEditorHost textEditorHost = new Mock<ITextEditorHost>().Object;
			var coordinator = new WorkbenchCodeNavigationCoordinator(
				ScriptingWorkspaceProfileTestFactory.CreateLuaProfile(supportsLuaActivation: true),
				documentController.Object,
				CreatePaneCoordinator(),
				CreateLuaHostServices(referencesProvider.Object, textEditorHost),
				new WorkbenchDialogCoordinator(
					new Mock<IDialogService>().Object,
					new Mock<IWin32DialogOwnerProvider>().Object,
					CreateFindAndReplaceViewModel(documentController.Object)),
				new Mock<IMessageService>().Object,
				ScriptingLanguageServicesTestFactory.CreateClassicScript());

			Task search = coordinator.FindLuaReferencesAsync(editor);
			try
			{
				Task completedTask = Task.WhenAny(requestStarted.Task, search)
					.WaitAsync(TimeSpan.FromSeconds(5))
					.GetAwaiter()
					.GetResult();

				if (completedTask == search)
					search.GetAwaiter().GetResult();

				Assert.IsTrue(
					requestStarted.Task.IsCompletedSuccessfully,
					$"Reference search ended before invoking the provider. Search status: {search.Status}.");
				CancellationToken requestToken = requestStarted.Task.GetAwaiter().GetResult();

				coordinator.Dispose();
				Assert.IsTrue(requestToken.IsCancellationRequested);

				completion.SetResult([]);
				search.WaitAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
			}
			finally
			{
				coordinator.Dispose();
				completion.TrySetResult([]);
				editor.Dispose();
			}
		});
	}

	[TestMethod]
	public void TextDiagnosticsCoordinator_DisposeDetachesEditorSubscription()
	{
		StaTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(
				new Version(1, 0),
				ScriptingLanguageServicesTestFactory.CreateClassicScript())
			{
				FilePath = @"C:\Scripts\active.txt"
			};
			ScriptingDocumentRegistration registration = new(
				EditorType.Text,
				DocumentMode.ClassicScript,
				_ => true,
				_ => true,
				_ => editor,
				new(null, ScriptingDocumentConfigurationKind.ClassicScript));
			var coordinator = new TextEditorDiagnosticsCoordinator(
				new Mock<IEditorDocumentController>().Object,
				CreatePaneCoordinator());

			coordinator.Update(new ScriptingDocumentContext(1, editor, editor.FilePath, registration));
			Assert.AreEqual(1, GetDiagnosticsSubscriptionCount(editor));

			coordinator.Dispose();
			coordinator.Dispose();

			Assert.AreEqual(0, GetDiagnosticsSubscriptionCount(editor));
			editor.Dispose();
		});
	}

	private static FindAndReplaceViewModel CreateFindAndReplaceViewModel(IEditorDocumentController documentController)
		=> new(documentController, new WeakReferenceMessenger(), new FindReplaceService());

	private static ResourceDictionary[] EnsureDarkUiResources(out System.Windows.Application application)
	{
		application = System.Windows.Application.Current ?? new System.Windows.Application();
		var addedResources = new List<ResourceDictionary>();
		string[] resourceUris =
		[
			"/DarkUI.WPF;component/Generic.xaml",
			"/DarkUI.WPF;component/Dictionaries/DarkColors.xaml"
		];

		foreach (string resourceUri in resourceUris)
		{
			if (application.Resources.MergedDictionaries.Any(dictionary =>
				dictionary.Source?.OriginalString == resourceUri))
				continue;

			var resource = new ResourceDictionary
			{
				Source = new Uri(resourceUri, UriKind.RelativeOrAbsolute)
			};
			application.Resources.MergedDictionaries.Add(resource);
			addedResources.Add(resource);
		}

		return [.. addedResources];
	}

	private static WorkbenchPaneCoordinator CreatePaneCoordinator()
	{
		var dockHost = new Mock<IAvalonDockHost>();
		dockHost.SetupGet(host => host.View).Returns(Mock.Of<FrameworkElement>());
		dockHost.SetupGet(host => host.Dispatcher).Returns(Dispatcher.CurrentDispatcher);
		return new WorkbenchPaneCoordinator(
			new PaneCatalog([]),
			dockHost.Object,
			new Mock<IPaneHostService>().Object,
			null);
	}

	private static LuaHostServices CreateLuaHostServices(
		ILanguageServerReferencesProvider? referencesProvider = null,
		ITextEditorHost? textEditorHost = null)
	{
		textEditorHost ??= new Mock<ITextEditorHost>().Object;
		return new LuaHostServices(
			new Mock<ILuaEditorLifecycleService>().Object,
			new Mock<ILuaIntellisenseBridge>().Object,
			new LuaTrackedDocumentStateService(textEditorHost, new Mock<ILuaLanguageServerIntelliSenseProvider>().Object),
			new LuaReferenceSearchService(
				textEditorHost,
				referencesProvider ?? new Mock<ILanguageServerReferencesProvider>().Object,
				@"C:\Scripts"),
			new TextWorkspaceCommandService(
				new TombLib.Scripting.UI.Editing.TextWorkspaceEditApplier(textEditorHost),
				new Mock<ILanguageServerRenameProvider>().Object));
	}

	private static int GetDiagnosticsSubscriptionCount(TextEditorBase editor)
	{
		FieldInfo? field = typeof(TextEditorBase).GetField(
			"DiagnosticsChanged",
			BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.IsNotNull(field);
		return (field.GetValue(editor) as Delegate)?.GetInvocationList().Length ?? 0;
	}
}
