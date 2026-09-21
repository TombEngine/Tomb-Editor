using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.ToolStrips;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public class StudioContributionSurfaceTests
{
	[TestMethod]
	public void CreateCommandItem_PreservesPhase1TypedSurfaceContract()
	{
		StudioToolStripItem item = StudioCommandSurfaceItemFactory.CreateCommandItem(
			"Build",
			UICommand.Build,
			icon: "Play_16",
			shortcutDisplayText: "F9",
			checkOnClick: true,
			position: 4);

		Assert.AreEqual("Build", item.LangKey);
		Assert.AreEqual(UICommand.Build, item.Command);
		Assert.AreEqual("Play_16", item.Icon);
		Assert.AreEqual("F9", item.ShortcutDisplayText);
		Assert.IsTrue(item.CheckOnClick);
		Assert.AreEqual(4, item.Position);
		Assert.AreEqual(0, item.DropDownItems.Count);
	}

	[TestMethod]
	public void RebuildWorkspaceItems_MenuStrip_UsesWorkspaceContributions()
	{
		RunInSta(() =>
		{
			UICommand? invokedCommand = null;
			var menuStrip = new StudioMenuStrip
			{
				WorkspaceContributionItems =
				new[]
				{
					new StudioToolStripItem
					{
						LangKey = "TestRoot",
						Position = 0,
						DropDownItems =
						new List<StudioToolStripItem>
						{
							new StudioToolStripItem
							{
								LangKey = "TestCommand",
								Command = UICommand.About
							}
						}
					}
				}
			};

			menuStrip.ItemClicked += (_, args) => invokedCommand = args.Command;

			menuStrip.RebuildWorkspaceItems();

			Menu menu = GetMenu(menuStrip);
			Assert.AreEqual(1, menu.Items.Count);

			var rootItem = menu.Items[0] as MenuItem ?? throw new AssertFailedException();
			Assert.AreEqual(1, rootItem.Items.Count);

			var commandItem = rootItem.Items[0] as MenuItem ?? throw new AssertFailedException();
			commandItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

			Assert.AreEqual(UICommand.About, invokedCommand);
		});
	}

	[TestMethod]
	public void RebuildWorkspaceItems_ToolStrip_UsesWorkspaceContributions()
	{
		RunInSta(() =>
		{
			UICommand? invokedCommand = null;
			var toolStrip = new StudioToolStrip
			{
				WorkspaceContributionItems =
				new[]
				{
					new StudioToolStripItem
					{
						LangKey = "TestCommand",
						Command = UICommand.About
					}
				}
			};

			toolStrip.ItemClicked += (_, args) => invokedCommand = args.Command;

			toolStrip.RebuildWorkspaceItems();

			ToolBar toolBar = GetToolBar(toolStrip);
			Assert.AreEqual(1, toolBar.Items.Count);

			var button = toolBar.Items[0] as System.Windows.Controls.Primitives.ButtonBase ?? throw new AssertFailedException();
			button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

			Assert.AreEqual(UICommand.About, invokedCommand);
		});
	}

	[TestMethod]
	public void TypedDocumentCommandSurfaceProvider_ReturnsExpectedRepresentativeContributions()
	{
		IStudioDocumentCommandSurfaceProvider luaProvider = TypedDocumentCommandSurfaceProvider.CreateLua();
		IStudioDocumentCommandSurfaceProvider classicScriptProvider = TypedDocumentCommandSurfaceProvider.CreateClassicScript();
		IStudioDocumentCommandSurfaceProvider stringsProvider = TypedDocumentCommandSurfaceProvider.CreateStrings();

		StudioToolStripItem luaMenuRoot = AssertSingleRoot(luaProvider.GetMenuStripItems(null!));
		Assert.AreEqual("Document", luaMenuRoot.LangKey);
		Assert.AreEqual(2, luaMenuRoot.Position);
		CollectionAssert.AreEqual(
			new[]
			{
				"Convert",
				string.Empty,
				"Reindent",
				"TrimWhitespace",
				string.Empty,
				"GoToDefinition",
				"FindReferences",
				"RenameSymbol",
				"NavigateBack",
				"NavigateForward",
				string.Empty,
				"ToggleComment",
				"CommentOut",
				"Uncomment",
				string.Empty,
				"ToggleBookmark",
				"PrevBookmark",
				"NextBookmark",
				"ClearBookmarks"
			},
			luaMenuRoot.DropDownItems.Select(static item => item.LangKey).ToArray());

		CollectionAssert.AreEqual(
			new[]
			{
				string.Empty,
				"CommentOut",
				"Uncomment",
				string.Empty,
				"ToggleBookmark",
				"PrevBookmark",
				"NextBookmark",
				"ClearBookmarks"
			},
			classicScriptProvider.GetToolStripItems(null!).Select(static item => item.LangKey).ToArray());

		CollectionAssert.AreEqual(
			new[]
			{
				"Cut",
				"Copy",
				"Paste"
			},
			stringsProvider.GetContextMenuItems(null!).Select(static item => item.LangKey).ToArray());

		Assert.IsNull(ScriptingDocumentContributions.None.CommandSurfaceProvider);
	}

	[TestMethod]
	public void TypedDocumentCommandSurfaceProvider_ContextMenu_ReturnsEmptyForNoneMode()
	{
		// EditorContextMenu is removed (replaced by native WPF context menus).
		// Context menu items are still provided by the typed provider.
		IStudioDocumentCommandSurfaceProvider? typedProvider = ScriptingDocumentContributions.None.CommandSurfaceProvider;

		Assert.IsNull(typedProvider);
	}

	[TestMethod]
	public void DocumentContributions_SelectCommandSurfaceByRegistrationIdentity()
	{
		IStudioDocumentCommandSurfaceProvider luaProvider = TypedDocumentCommandSurfaceProvider.CreateLua();
		IStudioDocumentCommandSurfaceProvider trxProvider = TypedDocumentCommandSurfaceProvider.CreateTrx();

		Assert.AreNotSame(luaProvider, trxProvider);
		Assert.AreEqual("GoToDefinition", luaProvider.GetMenuStripItems(null!)[0].DropDownItems[5].LangKey);
		Assert.AreEqual("TrimWhitespace", trxProvider.GetMenuStripItems(null!)[0].DropDownItems[2].LangKey);
	}

	[TestMethod]
	public void DocumentCommandSurfaceProvider_ClonesItemsForEachRequest()
	{
		IStudioDocumentCommandSurfaceProvider provider = TypedDocumentCommandSurfaceProvider.CreateLua();

		StudioToolStripItem firstRoot = provider.GetMenuStripItems(null!)[0];
		StudioToolStripItem secondRoot = provider.GetMenuStripItems(null!)[0];

		Assert.AreNotSame(firstRoot, secondRoot);
		Assert.AreNotSame(firstRoot.DropDownItems[0], secondRoot.DropDownItems[0]);
	}

	private static void RunInSta(Action action)
	{
		ExceptionDispatchInfo? capturedException = null;

		var thread = new Thread(() =>
		{
			try
			{
				action();
			}
			catch (Exception exception)
			{
				capturedException = ExceptionDispatchInfo.Capture(exception);
			}
		});

		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		thread.Join();

		capturedException?.Throw();
	}

	private static Menu GetMenu(StudioMenuStrip menuStrip)
	{
		object view = typeof(StudioMenuStrip)
			.GetProperty("View", BindingFlags.Instance | BindingFlags.NonPublic)?
			.GetValue(menuStrip) ?? throw new AssertFailedException();

		return ((System.Windows.Controls.UserControl)view).Content as Menu ?? throw new AssertFailedException();
	}

	private static ToolBar GetToolBar(StudioToolStrip toolStrip)
	{
		object view = typeof(StudioToolStrip)
			.GetProperty("View", BindingFlags.Instance | BindingFlags.NonPublic)?
			.GetValue(toolStrip) ?? throw new AssertFailedException();

		var tray = ((System.Windows.Controls.UserControl)view).Content as ToolBarTray ?? throw new AssertFailedException();
		return tray.ToolBars[0];
	}

	private static void AssertItemsEqual(IReadOnlyList<StudioToolStripItem> expected, IReadOnlyList<StudioToolStripItem> actual)
	{
		Assert.AreEqual(expected.Count, actual.Count);

		for (int i = 0; i < expected.Count; i++)
			AssertItemEqual(expected[i], actual[i]);
	}

	private static void AssertItemEqual(StudioToolStripItem expected, StudioToolStripItem actual)
	{
		Assert.AreEqual(expected.GetType(), actual.GetType());
		Assert.AreEqual(expected.LangKey, actual.LangKey);
		Assert.AreEqual(expected.Command, actual.Command);
		Assert.AreEqual(expected.Icon, actual.Icon);
		Assert.AreEqual(expected.CheckOnClick, actual.CheckOnClick);
		Assert.AreEqual(expected.ShortcutDisplayText, actual.ShortcutDisplayText);
		Assert.AreEqual(expected.Position, actual.Position);

		AssertItemsEqual(expected.DropDownItems, actual.DropDownItems);
	}

	private static StudioToolStripItem AssertSingleRoot(IReadOnlyList<StudioToolStripItem> items)
	{
		Assert.AreEqual(1, items.Count);
		return items[0];
	}

	private sealed class TestEditorControl : IEditorControl
	{
		private int _zoom = 100;

		public EditorType EditorType => EditorType.Text;

		public string FilePath { get; set; } = string.Empty;

		private EditorProcessingMode _processingMode;

		public EditorProcessingMode ProcessingMode => _processingMode;

		public IDisposable BeginProcessingScope(EditorProcessingMode mode)
		{
			EditorProcessingMode previousMode = _processingMode;
			_processingMode = mode;
			return new ProcessingScope(this, previousMode);
		}

		private sealed class ProcessingScope : IDisposable
		{
			private readonly TestEditorControl _owner;
			private readonly EditorProcessingMode _previousMode;
			private bool _disposed;

			public ProcessingScope(TestEditorControl owner, EditorProcessingMode previousMode)
			{
				_owner = owner;
				_previousMode = previousMode;
			}

			public void Dispose()
			{
				if (_disposed)
					return;

				_disposed = true;
				_owner._processingMode = _previousMode;
			}
		}

		public bool CreateBackupFiles { get; set; }

		public string Content { get; set; } = string.Empty;

		public bool IsContentChanged { get; set; }

		public DateTime LastModified { get; set; }

		public bool CanUndo => false;

		public bool CanRedo => false;

		public int CurrentRow => 1;

		public int CurrentColumn => 1;

		public string? SelectedContent => string.Empty;

		public int SelectionLength => 0;

		public int Zoom
		{
			get => _zoom;
			set
			{
				if (value == _zoom)
					return;

				_zoom = value;
				ZoomChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public int MinZoom { get; set; } = 10;

		public int MaxZoom { get; set; } = 400;

		public int ZoomStepSize { get; set; } = 10;

		public string DefaultFileExtension => ".txt";

		public Version EngineVersion { get; set; } = new(1, 0);

		public event EventHandler? ContentChangedWorkerRunCompleted;

		public event EventHandler? StatusChanged;

		public event EventHandler? ZoomChanged;

		public void ApplyPersistedContent(string content)
			=> Content = content;

		public void Copy()
		{ }

		public void Cut()
		{ }

		public void Dispose()
		{ }

		public void Load(string fileName)
			=> Load(fileName, default);

		public void Load(string fileName, DocumentLoadOptions options)
			=> FilePath = fileName;

		public void Paste()
		{ }

		public void Redo()
		{ }

		public void Save()
		{ }

		public void Save(string fileName)
			=> FilePath = fileName;

		public void SelectAll()
		{ }

		public void RunContentChangedWorker()
			=> ContentChangedWorkerRunCompleted?.Invoke(this, EventArgs.Empty);

		public void Undo()
		{ }

		public void UpdateSettings(ConfigurationBase configuration)
			=> StatusChanged?.Invoke(this, EventArgs.Empty);
	}
}