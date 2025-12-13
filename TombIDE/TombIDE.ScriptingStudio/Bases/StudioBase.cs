using DarkUI.Docking;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Helpers;
using TombIDE.ScriptingStudio.Objects;
using TombIDE.ScriptingStudio.Properties;
using TombIDE.ScriptingStudio.Settings;
using TombIDE.ScriptingStudio.ToolStrips;
using TombIDE.ScriptingStudio.ToolWindows;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;
using TombLib.Forms;
using TombLib.Scripting.Bases;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.Forms;
using TombLib.Scripting.Interfaces;
using TombLib.Scripting.Objects;

namespace TombIDE.ScriptingStudio.Bases
{
	public abstract class StudioBase : Control
	{
		#region Properties

		public DocumentMode DocumentMode
		{
			get => MenuStrip.DocumentMode;
			set
			{
				MenuStrip.DocumentMode = value;
				ToolStrip.DocumentMode = value;
				StatusStrip.DocumentMode = value;
				EditorContextMenu.DocumentMode = value;

				ContentExplorer.DocumentMode = value;
			}
		}

		public string ScriptRootDirectoryPath
		{
			get => EditorTabControl.ScriptRootDirectoryPath;
			set
			{
				EditorTabControl.ScriptRootDirectoryPath = value;
				FileExplorer.RootDirectoryPath = value;
				FileExplorer.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
			}
		}

		public string EngineDirectoryPath { get; set; }

		/// <summary>
		/// The current tab's Editor control.
		/// </summary>
		public IEditorControl CurrentEditor => EditorTabControl.CurrentEditor;

		public DockPanelState DockPanelState { get; set; }

		public bool IsMainWindowFocued { get; set; }

		#endregion Properties

		#region Fields

		protected ConfigurationCollection Configs;

		protected FormFindReplace FindReplaceForm;

		public StudioMenuStrip MenuStrip;
		public StudioToolStrip ToolStrip;
		public StudioStatusStrip StatusStrip;
		public EditorContextMenu EditorContextMenu;

		protected DarkDockPanel DockPanel;

		/// <summary>
		/// Dockable document. (Parent of <c>EditorTabControl</c>)
		/// </summary>
		public DarkDocument EditorTabControlDocument;

		public EditorTabControl EditorTabControl;

		public ContentExplorer ContentExplorer;
		public FileExplorer FileExplorer;
		public CompilerLogs CompilerLogs;
		public SearchResults SearchResults;

		/* Very frequently accessed items */

		protected ToolStripItem UndoMenuItem;
		protected ToolStripItem UndoToolStripButton;
		protected ToolStripItem RedoMenuItem;
		protected ToolStripItem RedoToolStripButton;
		protected ToolStripItem SaveMenuItem;
		protected ToolStripItem SaveToolStripButton;
		protected ToolStripItem SaveAsMenuItem;
		protected ToolStripItem SaveAllMenuItem;
		protected ToolStripItem SaveAllToolStripButton;

		protected ToolStripMenuItem ToolStripViewItem;
		protected ToolStripMenuItem ContentExplorerViewItem;
		protected ToolStripMenuItem FileExplorerViewItem;
		protected ToolStripMenuItem ReferenceBrowserViewItem;
		protected ToolStripMenuItem CompilerLogsViewItem;
		protected ToolStripMenuItem SearchResultsViewItem;
		protected ToolStripMenuItem StatusStripViewItem;

		#endregion Fields

		#region Construction

		public StudioBase(string scriptRootDirectoryPath, string engineDirectoryPath)
		{
			Configs = new ConfigurationCollection();

			InitializeToolStrips();
			InitializeTabControl();
			InitializeContentExplorer();
			InitializeFileExplorer();
			InitializeFindReplaceForm();

			CompilerLogs = new CompilerLogs();
			SearchResults = new SearchResults(EditorTabControl);

			IDE.Instance.IDEEventRaised += OnIDEEventRaised;

			ScriptRootDirectoryPath = scriptRootDirectoryPath;
			EngineDirectoryPath = engineDirectoryPath;
		}

		private void InitializeToolStrips()
		{
			MenuStrip = new StudioMenuStrip() { Dock = DockStyle.Top, StudioMode = StudioMode };
			MenuStrip.ItemClicked += ToolStrip_ItemClicked;
			MenuStrip.StudioModeChanged += MenuStrip_StudioModeChanged;

			ToolStrip = new StudioToolStrip() { Dock = DockStyle.Top, StudioMode = StudioMode };
			ToolStrip.ItemClicked += ToolStrip_ItemClicked;
			ToolStrip.StudioModeChanged += ToolStrip_StudioModeChanged;

			StatusStrip = new StudioStatusStrip() { Dock = DockStyle.Bottom };

			EditorContextMenu = new EditorContextMenu();
			EditorContextMenu.ItemClicked += ToolStrip_ItemClicked;

			Controls.Add(StatusStrip);
			Controls.Add(ToolStrip);
			Controls.Add(MenuStrip);

			InitializeFrequentlyAccessedMenuStripItems();
			InitializeFrequentlyAccessedToolStripItems();
		}

		private void InitializeTabControl()
		{
			EditorTabControl = new EditorTabControl();
			EditorTabControl.SelectedIndexChanged += EditorTabControl_SelectedIndexChanged;
			EditorTabControl.FileOpened += EditorTabControl_FileOpened;
			EditorTabControl.TabClosing += EditorTabControl_TabClosing;

			EditorTabControlDocument = new DarkDocument();
			EditorTabControlDocument.SerializationKey = "EditorTabControlDocument";
			EditorTabControlDocument.Controls.Add(EditorTabControl);
		}

		private void InitializeContentExplorer()
		{
			ContentExplorer = new ContentExplorer();
			ContentExplorer.ObjectClicked += ContentExplorer_ObjectClicked;
		}

		private void InitializeFileExplorer()
		{
			FileExplorer = new FileExplorer();
			FileExplorer.FileOpened += FileExplorer_FileOpened;
			FileExplorer.FileChanged += FileExplorer_FileChanged;
			FileExplorer.FileRenamed += FileExplorer_FileRenamed;
			FileExplorer.FileDeleted += FileExplorer_FileDeleted;
		}

		private void InitializeFindReplaceForm()
		{
			FindReplaceForm = new FormFindReplace(EditorTabControl);
			FindReplaceForm.FindAllPerformed += FormFindReplace_FindAllPerformed;
		}

		private void InitializeDockPanel()
		{
			DockPanel = new DarkDockPanel
			{
				Dock = DockStyle.Fill,
				EqualizeGroupSizes = true,
				Padding = new Padding(2),
				PrioritizeLeft = false,
				PrioritizeRight = false
			};

			DockPanel.ContentAdded += DockPanel_ContentChanged;
			DockPanel.ContentRemoved += DockPanel_ContentChanged;

			Controls.Add(DockPanel);

			DockPanel.BringToFront();

			// We have to initialize a dummy layout before applying any other one
			// otherwise we're gonna experience control priority issues.
			DockPanel.RestoreDockPanelState(DefaultLayouts.DummyLayout, FindDockContentByKey);

			// We can remove the content after the initialization is done
			DockPanel.RemoveContent();

			// The fact that we have to do this for it to work correctly is complete bullsh*t

			// Apply the current layout
			DockPanel.RestoreDockPanelState(DockPanelState, FindDockContentByKey);

			ApplyMessageFilters();
		}

		private void InitializeFrequentlyAccessedMenuStripItems()
		{
			UndoMenuItem = MenuStrip.FindItem(UICommand.Undo);
			RedoMenuItem = MenuStrip.FindItem(UICommand.Redo);
			SaveMenuItem = MenuStrip.FindItem(UICommand.Save);
			SaveAsMenuItem = MenuStrip.FindItem(UICommand.SaveAs);
			SaveAllMenuItem = MenuStrip.FindItem(UICommand.SaveAll);

			ToolStripViewItem = MenuStrip.FindItem(UICommand.ToolStrip) as ToolStripMenuItem;
			ContentExplorerViewItem = MenuStrip.FindItem(UICommand.ContentExplorer) as ToolStripMenuItem;
			FileExplorerViewItem = MenuStrip.FindItem(UICommand.FileExplorer) as ToolStripMenuItem;
			ReferenceBrowserViewItem = MenuStrip.FindItem(UICommand.ReferenceBrowser) as ToolStripMenuItem;
			CompilerLogsViewItem = MenuStrip.FindItem(UICommand.CompilerLogs) as ToolStripMenuItem;
			SearchResultsViewItem = MenuStrip.FindItem(UICommand.SearchResults) as ToolStripMenuItem;
			StatusStripViewItem = MenuStrip.FindItem(UICommand.StatusStrip) as ToolStripMenuItem;
		}

		private void InitializeFrequentlyAccessedToolStripItems()
		{
			UndoToolStripButton = ToolStrip.FindItem(UICommand.Undo);
			RedoToolStripButton = ToolStrip.FindItem(UICommand.Redo);
			SaveToolStripButton = ToolStrip.FindItem(UICommand.Save);
			SaveAllToolStripButton = ToolStrip.FindItem(UICommand.SaveAll);
		}

		private void ApplyMessageFilters()
		{
			Application.AddMessageFilter(DockPanel.DockContentDragFilter);
			Application.AddMessageFilter(DockPanel.DockResizeFilter);
		}

		#endregion Construction

		#region Override / new region

		public new Control Parent
		{
			get => base.Parent;
			set
			{
				base.Parent = value;

				if (value is Form)
				{
					InitializeDockPanel();
					ApplyUserSettings();
				}
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			UpdateViewMenu();
		}

		#endregion Override / new region

		#region Virtual region

		protected virtual void OnIDEEventRaised(IIDEEvent obj)
		{
			if (obj is IDE.ProgramClosingEvent e)
				e.CanClose = EditorTabControl.AskSaveAll();
		}

		protected virtual void OnToolStripItemClicked(UICommand e)
		{
			HandleGlobalCommands(e);
			HandleDocumentCommands(e);
		}

		#endregion Virtual region

		#region Abstract region

		public abstract StudioMode StudioMode { get; }

		protected abstract void ApplyUserSettings(IEditorControl editor);
		protected abstract void ApplyUserSettings();
		protected abstract void Build();
		protected abstract void RestoreDefaultLayout();
		protected abstract void ShowDocumentation();

		#endregion Abstract region

		#region Events

		private void DockPanel_ContentChanged(object sender, DockContentEventArgs e)
			=> UpdateViewMenu();

		private void MenuStrip_StudioModeChanged(object sender, EventArgs e)
			=> InitializeFrequentlyAccessedMenuStripItems();

		private void ToolStrip_StudioModeChanged(object sender, EventArgs e)
			=> InitializeFrequentlyAccessedToolStripItems();

		private void ToolStrip_ItemClicked(object sender, EventArgs e)
			=> OnToolStripItemClicked(((sender as ToolStripItem).Tag as UIElementArgs).Command);

		private void EditorTabControl_SelectedIndexChanged(object sender, EventArgs e)
			=> UpdateUI();

		private void EditorTabControl_FileOpened(object sender, EventArgs e)
		{
			var editor = sender as IEditorControl;

			editor.ContentChangedWorkerRunCompleted += Editor_ContentChangedWorkerRunCompleted;

			if (editor is TextEditorBase textEditor)
			{
				textEditor.KeyDown += TextEditor_KeyDown;
				textEditor.TextChanged += TextEditor_TextChanged;
			}

			ApplyUserSettings(editor);

			UpdateUI();
		}

		private void EditorTabControl_TabClosing(object sender, TabControlCancelEventArgs e)
		{
			if (!e.Cancel && EditorTabControl.TabCount == 1)
				DocumentMode = DocumentMode.None;
		}

		private void Editor_ContentChangedWorkerRunCompleted(object sender, EventArgs e)
			=> UpdateUndoRedoSaveStates();

		private void TextEditor_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if ((System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
			&& (e.Key == System.Windows.Input.Key.F || e.Key == System.Windows.Input.Key.H))
				FindReplaceForm.Show(this, (CurrentEditor as TextEditorBase).SelectedText);
		}

		private void TextEditor_TextChanged(object sender, EventArgs e)
			=> UpdateUndoRedoSaveStates();

		private void ContentExplorer_ObjectClicked(object sender, ObjectClickedEventArgs e)
			=> CurrentEditor.GoToObject(e.ObjectName, e.IdentifyingObject);

		private void FileExplorer_FileOpened(object sender, FileOpenedEventArgs e)
			=> EditorTabControl.OpenFile(e.FilePath, e.EditorType);

		private void FileExplorer_FileChanged(object sender, FileSystemEventArgs e)
		{
			if (!IsMainWindowFocued)
				EditorTabControl.AddFileToReloadQueue(e.FullPath);
		}

		private void FileExplorer_FileRenamed(object sender, RenamedEventArgs e)
			=> EditorTabControl.RenameDocumentTabPage(e.OldFullPath, e.FullPath);

		private void FileExplorer_FileDeleted(object sender, FileSystemEventArgs e)
			=> EditorTabControl.CloseInvalidTabPages();

		private void FormFindReplace_FindAllPerformed(object sender, FindReplaceEventArgs e)
		{
			if (!DockPanel.ContainsContent(SearchResults))
			{
				SearchResults.DockArea = DarkDockArea.Bottom;
				DockPanel.AddContent(SearchResults);
			}

			SearchResults.DockGroup.SetVisibleContent(SearchResults);

			SearchResults.UpdateResults(e);
		}

		#endregion Events

		#region Event methods

		protected void UpdateUI()
		{
			if (CurrentEditor != null)
				DocumentMode = FileHelper.GetDocumentModeOfEditor(CurrentEditor);

			if (EditorTabControl.SelectedTab != null)
			{
				if (CurrentEditor is TextEditorBase)
				{
					ElementHost elementHost = EditorTabControl.SelectedTab.Controls.OfType<ElementHost>().FirstOrDefault();

					if (elementHost != null)
						elementHost.ContextMenuStrip = EditorContextMenu;
				}
				else if (CurrentEditor is Control control)
					control.ContextMenuStrip = EditorContextMenu;
			}

			ContentExplorer.EditorControl = CurrentEditor;
			StatusStrip.EditorControl = CurrentEditor;

			UpdateUndoRedoSaveStates();
		}

		protected void UpdateUndoRedoSaveStates()
		{
			// Undo buttons
			UndoMenuItem.Enabled = CurrentEditor != null && CurrentEditor.CanUndo;
			UndoMenuItem.Text = UndoMenuItem.Enabled ? Strings.Default.Undo : Strings.Default.CantUndo;

			UndoToolStripButton.Enabled = UndoMenuItem.Enabled;
			UndoToolStripButton.ToolTipText = UndoMenuItem.Text;

			// Redo buttons
			RedoMenuItem.Enabled = CurrentEditor != null && CurrentEditor.CanRedo;
			RedoMenuItem.Text = RedoMenuItem.Enabled ? Strings.Default.Redo : Strings.Default.CantRedo;

			RedoToolStripButton.Enabled = RedoMenuItem.Enabled;
			RedoToolStripButton.ToolTipText = RedoMenuItem.Text;

			// Save buttons
			SaveMenuItem.Enabled = CurrentEditor != null && CurrentEditor.IsContentChanged;
			SaveToolStripButton.Enabled = SaveMenuItem.Enabled;

			SaveAsMenuItem.Enabled = CurrentEditor != null;

			SaveAllMenuItem.Enabled = !EditorTabControl.IsEveryTabSaved();
			SaveAllToolStripButton.Enabled = SaveAllMenuItem.Enabled;
		}

		protected void UpdateViewMenu()
		{
			foreach (FieldInfo field in GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
				if (field.Name.EndsWith("ViewItem") && field.GetValue(this) is ToolStripMenuItem fieldValue)
					SetCheckedIfNotNull(fieldValue);
		}

		private void HandleGlobalCommands(UICommand command)
		{
			switch (command)
			{
				// File
				case UICommand.NewFile: FileExplorer.CreateNewFile(); break;
				case UICommand.Save: EditorTabControl.SaveFile(); break;
				case UICommand.SaveAs: EditorTabControl.SaveFileAs(); break;
				case UICommand.SaveAll: EditorTabControl.SaveAll(); break;
				case UICommand.Build: Build(); break;
				case UICommand.Exit: IDE.Instance.RequestProgramClose(); break;

				// Edit
				case UICommand.Undo: CurrentEditor?.Undo(); break;
				case UICommand.Redo: CurrentEditor?.Redo(); break;
				case UICommand.Cut: CurrentEditor?.Cut(); break;
				case UICommand.Copy: CurrentEditor?.Copy(); break;
				case UICommand.Paste: CurrentEditor?.Paste(); break;
				case UICommand.Find: FindReplaceForm.Show(this, CurrentEditor?.SelectedContent?.ToString()); break;
				case UICommand.SelectAll: CurrentEditor?.SelectAll(); break;

				// Options
				case UICommand.UseNewInclude:
				case UICommand.ShowLogsAfterBuild:
				case UICommand.ReindentOnSave:
					ToggleSetting(command); break;

				case UICommand.Settings: ShowSettingsForm(); break;

				// View
				case UICommand.RestoreDefaultLayout: RestoreDefaultLayout(); break;

				// Help
				case UICommand.ScriptingDocumentation: ShowDocumentation(); break;
				case UICommand.About: ShowAboutForm(); break;
			}

			if (command >= UICommand.ToolStrip && command <= UICommand.StatusStrip) // All "View" menu items
				ToggleItemVisibility(command);

			UpdateUndoRedoSaveStates();
		}

		protected virtual void HandleDocumentCommands(UICommand command)
		{
			if (CurrentEditor is TextEditorBase textEditor)
				switch (command)
				{
					case UICommand.TabsToSpaces: textEditor.ConvertTabsToSpaces(); break;
					case UICommand.SpacesToTabs: textEditor.ConvertSpacesToTabs(); break;
					case UICommand.Reindent: textEditor.TidyCode(); break;
					case UICommand.TrimWhiteSpace: textEditor.TidyCode(true); break;
					case UICommand.CommentOut: textEditor.CommentOutLines(); break;
					case UICommand.Uncomment: textEditor.UncommentLines(); break;
					case UICommand.ToggleBookmark: textEditor.ToggleBookmark(); break;
					case UICommand.PrevBookmark: textEditor.GoToPrevBookmark(); break;
					case UICommand.NextBookmark: textEditor.GoToNextBookmark(); break;
					case UICommand.ClearBookmarks: textEditor.ClearAllBookmarks(this); break;
				}

			if (CurrentEditor is StringEditor stringEditor)
				switch (command)
				{
					case UICommand.PrevSection:
						stringEditor.GoToPreviousSection();
						ContentExplorer.SelectNode(stringEditor.CurrentDataGrid.Name);
						break;

					case UICommand.NextSection:
						stringEditor.GoToNextSection();
						ContentExplorer.SelectNode(stringEditor.CurrentDataGrid.Name);
						break;

					case UICommand.ClearString: stringEditor.CurrentDataGrid?.ClearSelectedString(); break;
					case UICommand.RemoveLastString: stringEditor.CurrentDataGrid?.RemoveLastString(); break;
				}
		}

		protected void ToggleItemVisibility(UICommand command)
		{
			Control control = GetControlByKey<Control>(command.ToString());
			var menuItem = MenuStrip.FindItem(command) as ToolStripMenuItem;

			if (control is DarkToolWindow toolWindow)
			{
				ToggleToolWindow(toolWindow);
				menuItem.Checked = DockPanel.ContainsContent(toolWindow);
			}
			else
			{
				control.Visible = !control.Visible;
				menuItem.Checked = control.Visible;
			}
		}

		#endregion Event methods

		#region Other methods

		protected void ShowAboutForm()
		{
			using (var form = new FormAbout(Resources.AboutScreen_800))
				form.ShowDialog(this);
		}

		/// <summary>
		/// NOTE: Can only catch <c>public</c> fields. Returns <c>null</c> on failure.
		/// </summary>
		protected DarkDockContent FindDockContentByKey(string key)
			=> GetControlByKey<DarkDockContent>(key);

		/// <summary>
		/// NOTE: Can only catch <c>public</c> fields. Returns <c>null</c> on failure.
		/// </summary>
		protected T GetControlByKey<T>(string key) where T : Control
		{
			FieldInfo field = GetType().GetField(key);
			return field != null ? (field.GetValue(this) as T) : null;
		}

		protected void ToggleToolWindow(DarkToolWindow toolWindow)
		{
			if (toolWindow.DockPanel == null)
				DockPanel.AddContent(toolWindow);
			else
				DockPanel.RemoveContent(toolWindow);
		}

		protected void SetCheckedIfNotNull(ToolStripMenuItem item)
		{
			if (item != null)
			{
				UICommand command = (item.Tag as UIElementArgs).Command;

				if (command == UICommand.ToolStrip || command == UICommand.StatusStrip)
					item.Checked = GetControlByKey<Control>(command.ToString()).Visible;
				else
					item.Checked = DockPanel != null && DockPanel.ContainsContent(GetControlByKey<DarkToolWindow>(command.ToString()));
			}
		}

		protected void UpdateSettings()
		{
			UpdateSetting(UICommand.UseNewInclude);
			UpdateSetting(UICommand.ShowLogsAfterBuild);
			UpdateSetting(UICommand.ReindentOnSave);
		}

		protected void UpdateSetting(UICommand command)
		{
			var menuItem = MenuStrip.FindItem(command) as ToolStripMenuItem;

			if (menuItem != null)
				switch (command)
				{
					case UICommand.UseNewInclude: menuItem.Checked = IDE.Instance.IDEConfiguration.UseNewIncludeMethod; break;
					case UICommand.ShowLogsAfterBuild: menuItem.Checked = IDE.Instance.IDEConfiguration.ShowCompilerLogsAfterBuild; break;
					case UICommand.ReindentOnSave: menuItem.Checked = IDE.Instance.IDEConfiguration.ReindentOnSave; break;
				}
		}

		protected void ToggleSetting(UICommand command)
		{
			var menuItem = MenuStrip.FindItem(command) as ToolStripMenuItem;

			switch (command)
			{
				case UICommand.UseNewInclude: IDE.Instance.IDEConfiguration.UseNewIncludeMethod = menuItem.Checked; break;
				case UICommand.ShowLogsAfterBuild: IDE.Instance.IDEConfiguration.ShowCompilerLogsAfterBuild = menuItem.Checked; break;
				case UICommand.ReindentOnSave: IDE.Instance.IDEConfiguration.ReindentOnSave = menuItem.Checked; break;
			}

			Configs.SaveAllConfigs();
			IDE.Instance.IDEConfiguration.Save();
		}

		protected void ShowSettingsForm()
		{
			using (var form = new FormTextEditorSettings(StudioMode))
				if (form.ShowDialog() == DialogResult.OK)
				{
					Configs = new ConfigurationCollection();
					ApplyUserSettings();
				}
		}

		#endregion Other methods
	}
}
