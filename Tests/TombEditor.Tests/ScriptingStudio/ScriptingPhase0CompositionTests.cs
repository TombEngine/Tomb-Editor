#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Moq;
using MvvmDialogs;
using Nickelony.LanguageServer.Lua;
using TombIDE.ScriptingStudio.Composition;
using TombIDE.ScriptingStudio.Navigation;
using TombIDE.ScriptingStudio.Settings;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Workbench;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Docking;
using TombIDE.Shared.Messaging.Scripting;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;
using TombLib.WPF.Services.Abstract;
using WpfApplication = System.Windows.Application;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class ScriptingPhase0CompositionTests
{
	[TestMethod]
	public void Profiles_ResolveProductionCompositionWithExpectedLuaReferencesPane()
	{
		StaTestHelper.RunInSta(() =>
		{
			ResourceDictionary[] addedResources = EnsureDarkUiResources(out WpfApplication application);
			SynchronizationContext? previousSynchronizationContext = SynchronizationContext.Current;
			SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(application.Dispatcher));
			(TRVersion.Game GameVersion, bool SupportsLua)[] profiles =
			[
				(TRVersion.Game.TR4, false),
				(TRVersion.Game.TR1, false),
				(TRVersion.Game.TombEngine, true),
				(TRVersion.Game.TR4, true),
				(TRVersion.Game.TR1, true)
			];

			try
			{
				foreach ((TRVersion.Game gameVersion, bool supportsLua) in profiles)
				{
					string scriptDirectoryPath = Directory.CreateTempSubdirectory("TombEditor-Phase0-Composition-").FullName;
					try
					{
						string scriptFileName = gameVersion == TRVersion.Game.TombEngine
							? "Gameflow.lua"
							: gameVersion == TRVersion.Game.TR1
							? "gameflow.json5"
							: "Script.txt";
						File.WriteAllText(Path.Combine(scriptDirectoryPath, scriptFileName), string.Empty);

						var project = new Mock<IGameProject>();
						project.SetupGet(value => value.GameVersion).Returns(gameVersion);
						project.Setup(value => value.GetCurrentEngineVersion()).Returns(new Version(4, 8));
						project.Setup(value => value.GetEngineRootDirectoryPath()).Returns(@"C:\Engine");
						project.Setup(value => value.GetEngineExecutableFilePath()).Returns(@"C:\Engine\Game.exe");
						project.Setup(value => value.GetScriptRootDirectory()).Returns(scriptDirectoryPath);

						var projectContext = new Mock<IScriptingProjectContext>();
						projectContext.SetupGet(value => value.Project).Returns(project.Object);
						projectContext.SetupGet(value => value.ScriptRootDirectoryPath).Returns(scriptDirectoryPath);

						var settingsStore = new Mock<IScriptingStudioShellSettingsStore>();
						settingsStore
							.Setup(value => value.IsLuaEnabled(It.IsAny<ScriptingWorkspaceKind>()))
							.Returns(supportsLua);
						settingsStore
							.Setup(value => value.Load(It.IsAny<ScriptingWorkspaceProfile>()))
							.Returns(new ScriptingStudioShellWorkspaceSettings());
						settingsStore
							.Setup(value => value.Load(It.IsAny<ScriptingWorkspaceKind>(), It.IsAny<DockPanelState>()))
							.Returns(new ScriptingStudioShellWorkspaceSettings());
						var localizationService = new Mock<ILocalizationService>();
						localizationService
							.Setup(value => value[It.IsAny<string>()])
							.Returns((string key) => key);
						localizationService
							.Setup(value => value.WithKeysFor(It.IsAny<System.ComponentModel.INotifyPropertyChanged>()))
							.Returns(localizationService.Object);

						var services = new ServiceCollection();
						services.AddScriptingStudioHostComposition();
						var messenger = new Mock<IMessenger>();
						services.AddSingleton<IMessenger>(_ => messenger.Object);
						services.AddScoped<IScriptingProjectContext>(_ => projectContext.Object);
						services.AddScoped<IScriptingStudioShellSettingsStore>(_ => settingsStore.Object);
						var dockHost = new Mock<IAvalonDockHost>();
						dockHost.SetupGet(value => value.View).Returns(Mock.Of<FrameworkElement>());
						dockHost.SetupGet(value => value.Dispatcher).Returns(application.Dispatcher);
						dockHost.Setup(value => value.SaveLayout()).Returns("layout");
						services.AddScoped<IAvalonDockHost>(_ => dockHost.Object);
						var chromeDisposalCounts = new Dictionary<string, int>();
						Mock<IMenuService> menuService = CreateDisposalTrackingMock<IMenuService>(chromeDisposalCounts, "menu");
						Mock<IToolBarService> toolBarService = CreateDisposalTrackingMock<IToolBarService>(chromeDisposalCounts, "toolbar");
						Mock<IStatusBarService> statusBarService = CreateDisposalTrackingMock<IStatusBarService>(chromeDisposalCounts, "status");
						Mock<IPaneHostService> paneHostService = CreateDisposalTrackingMock<IPaneHostService>(chromeDisposalCounts, "paneHost");
						var trackedPane = new TrackingPane();
						services.AddScoped<IMenuService>(_ => menuService.Object);
						services.AddScoped<IToolBarService>(_ => toolBarService.Object);
						services.AddScoped<IStatusBarService>(_ => statusBarService.Object);
						services.AddScoped<IPaneHostService>(_ => paneHostService.Object);
						services.AddScoped<IStudioPaneContributionProvider>(_ => new TrackingPaneProvider(trackedPane));
						services.AddSingleton<IMessageService>(Mock.Of<IMessageService>());
						services.AddSingleton<IDialogService>(Mock.Of<IDialogService>());
						services.AddSingleton<ILocalizationService>(localizationService.Object);
						Mock<ILuaIntelliSenseProvider>? provider = null;
						Mock<ILuaEditorLifecycleService>? lifecycleService = null;
						Mock<ILuaIntellisenseBridge>? intellisenseBridge = null;
						var disposalOrder = new List<string>();
						if (supportsLua)
						{
							var intellisenseProvider = new Mock<ILuaIntelliSenseProvider>();
							provider = intellisenseProvider;
							intellisenseProvider.Setup(value => value.Dispose()).Callback(() => disposalOrder.Add("provider"));
							intellisenseProvider.Setup(provider => provider.GetDiagnostics(It.IsAny<string>())).Returns([]);
							intellisenseProvider.Setup(provider => provider.GetSemanticTokens(It.IsAny<string>())).Returns([]);
							services.AddScoped<ILuaIntelliSenseProvider>(_ => intellisenseProvider.Object);
							lifecycleService = new Mock<ILuaEditorLifecycleService>();
							intellisenseBridge = new Mock<ILuaIntellisenseBridge>();
							lifecycleService.Setup(value => value.Dispose()).Callback(() => disposalOrder.Add("lifecycle"));
							intellisenseBridge.Setup(value => value.Dispose()).Callback(() => disposalOrder.Add("bridge"));
							services.AddScoped<ILuaEditorLifecycleService>(_ => lifecycleService.Object);
							services.AddScoped<ILuaIntellisenseBridge>(_ => intellisenseBridge.Object);
						}
						else
						{
							services.AddScoped<ILuaIntelliSenseProvider>(_ =>
								throw new InvalidOperationException("Lua provider must not be resolved for a Lua-disabled profile."));
							services.AddScoped<ILuaEditorLifecycleService>(_ =>
								throw new InvalidOperationException("Lua lifecycle must not be resolved for a Lua-disabled profile."));
							services.AddScoped<ILuaIntellisenseBridge>(_ =>
								throw new InvalidOperationException("Lua bridge must not be resolved for a Lua-disabled profile."));
						}

						using ServiceProvider serviceProvider = services.BuildServiceProvider();
						AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
						PaneCatalog? paneCatalog = null;
						try
						{
							paneCatalog = scope.ServiceProvider.GetRequiredService<PaneCatalog>();
							var composition = scope.ServiceProvider.GetRequiredService<WorkbenchComposition>();
							var workbench = scope.ServiceProvider.GetRequiredService<IWorkbenchService>();
							var shell = scope.ServiceProvider.GetRequiredService<RootShellViewModel>();
							Assert.IsNotNull(workbench);
							Assert.IsNotNull(shell);

							var referencesPane = paneCatalog.GetPane<TextReferencesResultsToolWindow>(UICommand.LuaReferencesResults);
							Assert.AreEqual(supportsLua, referencesPane is not null);
							Assert.AreEqual(supportsLua, composition.LuaHostServices is not null);

							if (supportsLua)
								Assert.AreEqual("LuaReferencesResults", referencesPane!.SerializationKey);

							Assert.AreEqual(
								supportsLua ? 1 : 0,
								paneCatalog.Panes.Count(pane => pane.SerializationKey == "LuaReferencesResults"));
						}
						finally
						{
							scope.DisposeAsync().AsTask().GetAwaiter().GetResult();
							scope.DisposeAsync().AsTask().GetAwaiter().GetResult();
						}

						if (supportsLua)
						{
							Assert.IsNotNull(provider);
							Assert.IsNotNull(lifecycleService);
							Assert.IsNotNull(intellisenseBridge);
							provider.Verify(value => value.Dispose(), Times.Once);
							lifecycleService.Verify(value => value.Dispose(), Times.Once);
							intellisenseBridge.Verify(value => value.Dispose(), Times.Once);
							CollectionAssert.AreEqual(new[] { "provider", "bridge", "lifecycle" }, disposalOrder);
						}

						Assert.IsNotNull(paneCatalog);
						Assert.AreEqual(0, paneCatalog.Panes.Count);
						dockHost.Verify(value => value.SaveLayout(), Times.Once);
						dockHost.Verify(value => value.DetachDocumentController(), Times.Once);
						Assert.AreEqual(1, chromeDisposalCounts["menu"]);
						Assert.AreEqual(1, chromeDisposalCounts["toolbar"]);
						Assert.AreEqual(1, chromeDisposalCounts["status"]);
						Assert.AreEqual(1, chromeDisposalCounts["paneHost"]);
						Assert.AreEqual(1, trackedPane.DisposeCount);
						messenger.Verify(value => value.UnregisterAll(It.IsAny<WorkbenchService>()), Times.Once);
						messenger.Verify(value => value.UnregisterAll(It.IsAny<ScriptingMessageService>()), Times.Once);
						messenger.Verify(
							value => value.UnregisterAll(It.IsAny<LuaWorkbenchEventCoordinator>()),
							supportsLua ? Times.Once() : Times.Never());
					}
					finally
					{
						if (Directory.Exists(scriptDirectoryPath))
							Directory.Delete(scriptDirectoryPath, recursive: true);
					}
				}
			}
			finally
			{
				SynchronizationContext.SetSynchronizationContext(previousSynchronizationContext);

				foreach (ResourceDictionary resource in addedResources)
					application.Resources.MergedDictionaries.Remove(resource);
			}
		});
	}

	private static ResourceDictionary[] EnsureDarkUiResources(out WpfApplication application)
	{
		application = WpfApplication.Current ?? new WpfApplication();
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

	private static Mock<T> CreateDisposalTrackingMock<T>(Dictionary<string, int> counts, string name)
		where T : class, IDisposable
	{
		var mock = new Mock<T>();
		mock.Setup(value => value.Dispose()).Callback(() => counts[name] = counts.GetValueOrDefault(name) + 1);
		return mock;
	}

	private sealed class TrackingPaneProvider(TrackingPane pane) : IStudioPaneContributionProvider
	{
		public IReadOnlyList<StudioPaneContribution> GetPaneContributions()
			=> [new StudioPaneContribution(UICommand.None, "Phase5TrackingPane", () => pane)];
	}

	private sealed class TrackingPane : StudioDockPane
	{
		public TrackingPane()
			: base("Phase 5 tracking", "Phase5TrackingPane", StudioDockPaneLocation.Right, new System.Windows.Size(100, 100))
		{ }

		public int DisposeCount { get; private set; }

		public override UIElement Content => new System.Windows.Controls.Border();

		public override void Dispose()
			=> DisposeCount++;
	}
}
