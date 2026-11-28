#nullable enable

using ContextMenu = System.Windows.Controls.ContextMenu;
using Image = System.Windows.Controls.Image;
using MenuItem = System.Windows.Controls.MenuItem;
using PopUpInfo = TombLib.WPF.PopUpInfo;
using Separator = System.Windows.Controls.Separator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using TombLib.Graphics;
using TombLib.Icons;
using TombLib.Input;
using TombLib.LevelData;
using TombLib.Services;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using TombLib.WPF;
using TombLib.WPF.CustomControls;
using WadTool.Controls;
using WadTool.Features.Main;

namespace WadTool;

/// <summary>
/// WPF shell with native object trees, SVG commands and a Direct3D preview surface.
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly WadToolClass _tool;

    private readonly PopUpInfo _popup = new PopUpInfo();

    private readonly PanelRenderingMainPreview _panel3D;
    private readonly WadTreeView _treeDestWad;
    private readonly WadTreeView _treeSourceWad;

    private readonly ContextMenu _contextMenuMoveableItem;
    private readonly ContextMenu _contextMenuStatics;


    public MainWindow(WadToolClass tool)
    {
        _tool = tool;

        InitializeComponent();
        _viewModel = new MainWindowViewModel(tool, CreateActions());
        DataContext = _viewModel;

        // Only show debug menu when a debugger is attached (mirrors FormMain).
        debugMenu.Visibility = Debugger.IsAttached ? Visibility.Visible : Visibility.Collapsed;

        // Initialize the rendering device before the native WPF viewport draws its first frame.
        _panel3D = new PanelRenderingMainPreview
        {
            Configuration = tool.Configuration,
        };
        _panel3D.InitializeRendering(DeviceManager.DefaultDeviceManager.Device, tool.Configuration.RenderingItem_Antialias);
        panel3DHost.Child = _panel3D;

        _treeDestWad = treeDestWad;
        _treeDestWad.ReadOnly = false;
        _treeDestWad.ClickOnEmpty += treeDestWad_ClickOnEmpty;
        _treeDestWad.SelectedWadObjectIdsChanged += treeDestWad_SelectedWadObjectIdsChanged;
        _treeDestWad.MetadataChanged += treeDestWad_MetadataChanged;
        _treeDestWad.DoubleClick += treeDestWad_DoubleClick;
        _treeDestWad.KeyDown += treeDestWad_KeyDown;

        _treeSourceWad = treeSourceWad;
        _treeSourceWad.ReadOnly = false;
        _treeSourceWad.ClickOnEmpty += treeSourceWad_ClickOnEmpty;
        _treeSourceWad.SelectedWadObjectIdsChanged += treeSourceWad_SelectedWadObjectIdsChanged;
        _treeSourceWad.DoubleClick += treeSourceWad_DoubleClick;

        _contextMenuMoveableItem = BuildMoveableContextMenu();
        _contextMenuStatics = BuildStaticsContextMenu();

        _tool.EditorEventRaised += Tool_EditorEventRaised;

        Tool_EditorEventRaised(new InitEvent());

        // Reuses the legacy Window_FormMain_* config slots so existing user configs keep applying.
        WindowConfiguration.ConfigureWindow(this, _tool.Configuration, key: "FormMain");

        // Try to load reference project on start-up, if specified in config
        if (!string.IsNullOrEmpty(tool.Configuration.Tool_ReferenceProject) &&
            File.Exists(tool.Configuration.Tool_ReferenceProject))
            WadActions.LoadReferenceLevel(tool, this.GetWin32Window(), tool.Configuration.Tool_ReferenceProject);

        // Load recent files
        RefreshRecentWadsList();

        PreviewKeyDown += OnPreviewKeyDown;

        // Like WinForms, the destination tree starts with the focus (its header shows the active colour).
        ContentRendered += (_, _) => Dispatcher.InvokeAsync(() => treeDestWad.FocusTree(), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
    }

    private Dictionary<string, Action> CreateActions() => new()
    {
        ["AboutMenu_Click"] = () => AboutMenu_Click(this, new RoutedEventArgs()),
        ["AddObjectButton_Click"] = () => AddObjectButton_Click(this, new RoutedEventArgs()),
        ["AddObjectToDifferentSlotButton_Click"] = () => AddObjectToDifferentSlotButton_Click(this, new RoutedEventArgs()),
        ["AnimatedTexturesMenu_Click"] = () => AnimatedTexturesMenu_Click(this, new RoutedEventArgs()),
        ["ChangeSlotButton_Click"] = () => ChangeSlotButton_Click(this, new RoutedEventArgs()),
        ["CloseReferenceLevelMenu_Click"] = () => CloseReferenceLevelMenu_Click(this, new RoutedEventArgs()),
        ["ConsolidateTexturesMenu_Click"] = () => ConsolidateTexturesMenu_Click(this, new RoutedEventArgs()),
        ["ConvertToDynamicLightingMenu_Click"] = () => ConvertToDynamicLightingMenu_Click(this, new RoutedEventArgs()),
        ["ConvertToStaticLightingMenu_Click"] = () => ConvertToStaticLightingMenu_Click(this, new RoutedEventArgs()),
        ["ConvertToTiledMenu_Click"] = () => ConvertToTiledMenu_Click(this, new RoutedEventArgs()),
        ["ConvertToUVMappedMenu_Click"] = () => ConvertToUVMappedMenu_Click(this, new RoutedEventArgs()),
        ["ConvertWadToTombEngineMenu_Click"] = () => ConvertWadToTombEngineMenu_Click(this, new RoutedEventArgs()),
        ["DebugMeshEditorMenu_Click"] = () => DebugMeshEditorMenu_Click(this, new RoutedEventArgs()),
        ["DebugReloadCatalogMenu_Click"] = () => DebugReloadCatalogMenu_Click(this, new RoutedEventArgs()),
        ["DeleteObjectButton_Click"] = () => DeleteObjectButton_Click(this, new RoutedEventArgs()),
        ["EditItemButton_Click"] = () => EditItemButton_Click(this, new RoutedEventArgs()),
        ["EditSkeletonButton_Click"] = () => EditSkeletonButton_Click(this, new RoutedEventArgs()),
        ["ExitMenu_Click"] = () => ExitMenu_Click(this, new RoutedEventArgs()),
        ["ItemPropertiesMenu_Click"] = () => ItemPropertiesMenu_Click(this, new RoutedEventArgs()),
        ["MeshEditorMenu_Click"] = () => MeshEditorMenu_Click(this, new RoutedEventArgs()),
        ["NewMoveableMenu_Click"] = () => NewMoveableMenu_Click(this, new RoutedEventArgs()),
        ["NewSpriteSequenceMenu_Click"] = () => NewSpriteSequenceMenu_Click(this, new RoutedEventArgs()),
        ["NewStaticMenu_Click"] = () => NewStaticMenu_Click(this, new RoutedEventArgs()),
        ["NewWad2Menu_Click"] = () => NewWad2Menu_Click(this, new RoutedEventArgs()),
        ["OpenDestinationWadMenu_Click"] = () => OpenDestinationWadMenu_Click(this, new RoutedEventArgs()),
        ["OpenReferenceLevelMenu_Click"] = () => OpenReferenceLevelMenu_Click(this, new RoutedEventArgs()),
        ["OpenSourceWadMenu_Click"] = () => OpenSourceWadMenu_Click(this, new RoutedEventArgs()),
        ["OptionsMenu_Click"] = () => OptionsMenu_Click(this, new RoutedEventArgs()),
        ["SaveAsMenu_Click"] = () => SaveAsMenu_Click(this, new RoutedEventArgs()),
        ["SaveMenu_Click"] = () => SaveMenu_Click(this, new RoutedEventArgs()),
    };

    private class InitEvent : IEditorEvent { };

    private ContextMenu BuildMoveableContextMenu()
    {
        var menu = new ContextMenu();
        menu.Items.Add(NewContextMenuItem("Edit animations...", "Misc/animations", (s, e) => EditItemButton_Click(null, null)));
        menu.Items.Add(NewContextMenuItem("Edit skeleton...", "Misc/skeleton", (s, e) => EditSkeletonButton_Click(null, null)));
        menu.Items.Add(NewContextMenuItem("Edit meshes...", "Misc/wireframe", (s, e) => MeshEditorMenu_ClickHandler(s, e)));
        menu.Items.Add(NewContextMenuItem("Edit properties...", "Misc/properties_16xLG", (s, e) => EditSelectedObjectProperties()));
        menu.Items.Add(new Separator());
        menu.Items.Add(NewContextMenuItem("Change slot...", "General/Find-and-replace", (s, e) => ChangeSlotButton_Click(null, null)));
        menu.Items.Add(NewContextMenuItem("Delete object", "General/trash", (s, e) => DeleteObjectButton_Click(null, null)));
        return menu;
    }

    private ContextMenu BuildStaticsContextMenu()
    {
        var menu = new ContextMenu();
        menu.Items.Add(NewContextMenuItem("Edit object...", "General/edit", (s, e) => EditItemButton_Click(null, null)));
        menu.Items.Add(NewContextMenuItem("Edit mesh...", "Misc/wireframe", (s, e) => MeshEditorMenu_ClickHandler(s, e)));
        menu.Items.Add(NewContextMenuItem("Edit properties...", "Misc/properties_16xLG", (s, e) => EditSelectedObjectProperties()));
        menu.Items.Add(new Separator());
        menu.Items.Add(NewContextMenuItem("Change slot...", "General/Find-and-replace", (s, e) => ChangeSlotButton_Click(null, null)));
        menu.Items.Add(NewContextMenuItem("Delete object", "General/trash", (s, e) => DeleteObjectButton_Click(null, null)));
        return menu;
    }

    private static MenuItem NewContextMenuItem(string text, string iconId, RoutedEventHandler onClick)
    {
        var item = new MenuItem { Header = text, Icon = new Image { Source = IconSources.Load(iconId) } };
        item.Click += onClick;
        return item;
    }

    private void Tool_EditorEventRaised(IEditorEvent obj)
    {
        if (obj is InitEvent)
        {
            // At startup initialise a new Wad2
            if (_tool.Configuration.Tool_MakeEmptyWadAtStartup)
            {
                _tool.DestinationWad = new Wad2 { GameVersion = TRVersion.Game.TR4 };
                _tool.RaiseEvent(new WadToolClass.DestinationWadChangedEvent());
            }

            _panel3D.AnimatePreview = _tool.Configuration.RenderingItem_Animate;
        }

        if (obj is WadToolClass.MessageEvent)
        {
            var msg = (WadToolClass.MessageEvent)obj;
            PopUpInfo.Show(_popup, null, _panel3D, msg.Message, msg.Type);
        }

        if (obj is WadToolClass.DestinationWadChangedEvent || obj is InitEvent)
        {
            _treeDestWad.Wad = _tool.DestinationWad;
            _treeDestWad.UpdateContent();

            _panel3D.Invalidate();

            bool isWadLoaded = _tool.DestinationWad is not null;
            if (isWadLoaded)
            {
                _viewModel.Statistics = "Moveables: " + _tool.DestinationWad.Moveables.Count + " | " +
                                       "Statics: " + _tool.DestinationWad.Statics.Count + " | " +
                                       "Sprite sequences: " + _tool.DestinationWad.SpriteSequences.Count + " | " +
                                       "Textures: " + _tool.DestinationWad.MeshTexturesUnique.Count + " | " +
                                       "Texture infos: " + _tool.DestinationWad.MeshTexInfosUnique.Count;
            }
            else
            {
                _viewModel.Statistics = "";
            }

        }

        if (obj is WadToolClass.SourceWadChangedEvent || obj is InitEvent)
        {
            var header = "Source";
            if (_tool?.SourceWad is not null)
                header += (String.IsNullOrEmpty(_tool.SourceWad.FileName) ? " (Imported)" : " (" + Path.GetFileName(_tool.SourceWad.FileName) + ")");
            _viewModel.SourceHeader = header;

            _treeSourceWad.Wad = _tool.SourceWad;
            _treeSourceWad.UpdateContent();

            _panel3D.Invalidate();
        }

        if (obj is WadToolClass.MainSelectionChangedEvent ||
            obj is WadToolClass.DestinationWadChangedEvent ||
            obj is WadToolClass.SourceWadChangedEvent ||
            obj is InitEvent)
        {
            var mainSelection = _tool.MainSelection;
            if (mainSelection is null)
            {
                _panel3D.CurrentObject = null;

                butEditAnimations.Visibility = Visibility.Collapsed;
                butEditSkeleton.Visibility = Visibility.Collapsed;
                butEditStaticModel.Visibility = Visibility.Collapsed;
                butEditSpriteSequence.Visibility = Visibility.Collapsed;
            }
            else
            {
                Wad2 wad = _tool.GetWad(mainSelection.Value.WadArea);

                // Display the object (or set it to Lara's skin instead if it's Lara)
                if (mainSelection.Value.Id is WadMoveableId)
                {
                    var skin = wad.TryGet(new WadMoveableId(TrCatalog.GetMoveableSkin(wad.GameVersion, ((WadMoveableId)mainSelection.Value.Id).TypeId)));
                    var msh = wad.TryGet(mainSelection.Value.Id);
                    if (skin is not null && skin != msh)
                        _panel3D.CurrentObject = (msh as WadMoveable)?.ReplaceDummyMeshes((WadMoveable)skin);
                    else
                        _panel3D.CurrentObject = msh;
                }
                else
                    _panel3D.CurrentObject = wad.TryGet(mainSelection.Value.Id);

                // Update the toolbar below the rendering area
                butEditAnimations.Visibility = (mainSelection.Value.Id is WadMoveableId) ? Visibility.Visible : Visibility.Collapsed;
                butEditSkeleton.Visibility = (mainSelection.Value.Id is WadMoveableId) ? Visibility.Visible : Visibility.Collapsed;
                butEditStaticModel.Visibility = (mainSelection.Value.Id is WadStaticId) ? Visibility.Visible : Visibility.Collapsed;
                butEditSpriteSequence.Visibility = (mainSelection.Value.Id is WadSpriteSequenceId) ? Visibility.Visible : Visibility.Collapsed;

                _panel3D.ResetCamera();
                _panel3D.Invalidate();
            }

            _panel3D.Invalidate();
        }

        if (obj is WadToolClass.ReferenceLevelChangedEvent)
        {
            if (_tool.ReferenceLevel is not null)
            {
                lblRefLevel.IsEnabled = true;
                lblRefLevel.Opacity = 1.0;
                lblRefLevel.Content = Path.GetFileNameWithoutExtension(_tool.ReferenceLevel.Settings.LevelFilePath);
            }
            else
            {
                lblRefLevel.IsEnabled = false;
                lblRefLevel.Opacity = 0.6;
                lblRefLevel.Content = "(project not loaded)";
            }
        }

        if (obj is WadToolClass.UnsavedChangesEvent)
            UpdateSaveUI(((WadToolClass.UnsavedChangesEvent)obj).UnsavedChanges);

        if (obj is WadToolClass.SourceWadChangedEvent ||
            obj is WadToolClass.DestinationWadChangedEvent)
            _panel3D.GarbageCollect();
    }

    private void UpdateSaveUI(bool hasUnsavedChanges)
    {
        var title = "WadTool";
        if (_tool?.DestinationWad is not null)
        {
            var newOrImported = String.IsNullOrEmpty(_tool.DestinationWad.FileName);

            title += " - ";
            title += newOrImported ? "Untitled" : _tool.DestinationWad.FileName;
            title += hasUnsavedChanges ? "*" : "";

            if (!hasUnsavedChanges)
                _treeDestWad.UpdateMetadata(); // Refresh timestamp
        }
        _viewModel.Title = title;
    }

    private DialogChoice CheckIfSaved()
    {
        if (_viewModel.CanSave && _tool.DestinationWad is { WadIsEmpty: false })
            return UserDialogs.Show(this.GetWin32Window(), "You have unsaved changes. Do you want to save changes?",
                                       "Confirm", DialogButtons.YesNoCancel, DialogIcon.Question);
        else
            return DialogChoice.OK;
    }

    private void TryMakeNewWad()
    {
        if (!CanDiscardChanges())
            return;

        WadActions.CreateNewWad(_tool, this.GetWin32Window());
        RefreshRecentWadsList();
    }

    private void TryOpenDestWad()
    {
        if (!CanDiscardChanges())
            return;

        WadActions.LoadWadOpenFileDialog(_tool, this.GetWin32Window(), true);
        RefreshRecentWadsList();
    }

    private bool CanDiscardChanges()
    {
        var choice = CheckIfSaved();
        if (choice == DialogChoice.Cancel)
            return false;

        // Like the original form, the outcome of a requested save does not block the follow-up action.
        if (choice == DialogChoice.Yes)
            WadActions.SaveWad(_tool, this.GetWin32Window(), _tool.DestinationWad, false);

        return true;
    }

    private void CopyObject(bool otherSlot)
    {
        var result = WadActions.CopyObject(_tool, this.GetWin32Window(), _treeSourceWad.SelectedWadObjectIds.ToList(), otherSlot);
        if (result is not null)
        {
            if (result.Count > 0)
                _treeDestWad.Select(result);
            else
                _tool.SendMessage("No objects were copied because they are already in different slots.", PopupType.Warning);
        }
        else
            _tool.SendMessage("No objects were copied.", PopupType.Info);
    }

    private void RefreshRecentWadsList()
    {
        openRecentMenu.Items.Clear();

        if (Properties.Settings.Default.RecentProjects is not null && Properties.Settings.Default.RecentProjects.Count > 0)
            foreach (var fileName in Properties.Settings.Default.RecentProjects)
            {
                if (fileName == _tool.DestinationWad?.FileName)   // Skip currently loaded wad
                    continue;

                if (!File.Exists(fileName)) // Skip nonexistent wads
                    continue;

                var item = new MenuItem { Header = fileName.Replace("_", "__") };
                item.Click += (s, e) =>
                {
                    WadActions.LoadWad(_tool, this.GetWin32Window(), true, fileName);
                    RefreshRecentWadsList();
                };
                openRecentMenu.Items.Add(item);
            }

        // Add "Clear recent files" option
        openRecentMenu.Items.Add(new Separator());
        var item2 = new MenuItem { Header = "Clear recent file list" };
        item2.Click += (s, e) => { Properties.Settings.Default.RecentProjects.Clear(); RefreshRecentWadsList(); Properties.Settings.Default.Save(); };
        openRecentMenu.Items.Add(item2);

        // Disable menu item, if list is empty
        openRecentMenu.IsEnabled = openRecentMenu.Items.Count > 2;
    }



    #region Hotkeys

    /// <summary>
    /// Handles shortcuts routed from WPF controls and the native Direct3D viewport.
    /// </summary>
    private bool HandleHotkey(EditorKey keyData)
    {
        if (!IsActive)
            return false;

        var name = keyData switch
        {
            EditorKey.Control | EditorKey.N => "NewWad2Menu_Click",
            EditorKey.Control | EditorKey.O => "OpenSourceWadMenu_Click",
            EditorKey.Control | EditorKey.Shift | EditorKey.O => "OpenDestinationWadMenu_Click",
            EditorKey.Control | EditorKey.Alt | EditorKey.O => "OpenReferenceLevelMenu_Click",
            EditorKey.Control | EditorKey.S => "SaveMenu_Click",
            EditorKey.Control | EditorKey.Shift | EditorKey.S => "SaveAsMenu_Click",
            _ => null
        };
        return name is not null && _viewModel.TryExecute(name);
    }

    private void OnPreviewKeyDown(object? sender, System.Windows.Input.KeyEventArgs e)
    {
        var key = e.Key == System.Windows.Input.Key.System ? e.SystemKey : e.Key;
        var keyData = (EditorKey)System.Windows.Input.KeyInterop.VirtualKeyFromKey(key);
        var modifiers = System.Windows.Input.Keyboard.Modifiers;
        if (modifiers.HasFlag(System.Windows.Input.ModifierKeys.Control))
            keyData |= EditorKey.Control;
        if (modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift))
            keyData |= EditorKey.Shift;
        if (modifiers.HasFlag(System.Windows.Input.ModifierKeys.Alt))
            keyData |= EditorKey.Alt;

        if (HandleHotkey(keyData))
            e.Handled = true;
    }

    #endregion

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (!CanDiscardChanges())
        {
            e.Cancel = true;
            return;
        }

        // Raises the Closing event; WindowConfiguration's hook persists the window placement.
        base.OnClosing(e);

        if (!e.Cancel)
            _tool.Configuration.SaveTry();
    }

    protected override void OnClosed(EventArgs e)
    {
        _tool.EditorEventRaised -= Tool_EditorEventRaised;
        _viewModel.Dispose();

        PreviewKeyDown -= OnPreviewKeyDown;

        _popup?.Dispose();
        _panel3D?.Dispose();

        base.OnClosed(e);

        // The WPF Application uses ShutdownMode.OnExplicitShutdown (WPFInitializer), so closing
        // the main window does not end Application.Run on its own.
        System.Windows.Application.Current?.Shutdown();
    }
}
