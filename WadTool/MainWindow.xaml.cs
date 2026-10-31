#nullable enable

using ContextMenu = System.Windows.Controls.ContextMenu;
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
using TombLib.Input;
using TombLib.LevelData;
using TombLib.Services;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using TombLib.WPF;
using TombLib.WPF.CustomControls;
using TombLib.WPF.Input;
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
        menu.Items.Add(NewContextMenuItem("Edit animations...", (s, e) => EditItemButton_Click(null, null)));
        menu.Items.Add(NewContextMenuItem("Edit skeleton...", (s, e) => EditSkeletonButton_Click(null, null)));
        menu.Items.Add(NewContextMenuItem("Edit meshes...", (s, e) => MeshEditorMenu_ClickHandler(s, e)));
        menu.Items.Add(NewContextMenuItem("Edit properties...", (s, e) => EditSelectedObjectProperties()));
        menu.Items.Add(new Separator());
        menu.Items.Add(NewContextMenuItem("Change slot...", (s, e) => ChangeSlotButton_Click(null, null)));
        menu.Items.Add(NewContextMenuItem("Delete object", (s, e) => DeleteObjectButton_Click(null, null)));
        return menu;
    }

    private ContextMenu BuildStaticsContextMenu()
    {
        var menu = new ContextMenu();
        menu.Items.Add(NewContextMenuItem("Edit object...", (s, e) => EditItemButton_Click(null, null)));
        menu.Items.Add(NewContextMenuItem("Edit mesh...", (s, e) => MeshEditorMenu_ClickHandler(s, e)));
        menu.Items.Add(NewContextMenuItem("Edit properties...", (s, e) => EditSelectedObjectProperties()));
        menu.Items.Add(new Separator());
        menu.Items.Add(NewContextMenuItem("Change slot...", (s, e) => ChangeSlotButton_Click(null, null)));
        menu.Items.Add(NewContextMenuItem("Delete object", (s, e) => DeleteObjectButton_Click(null, null)));
        return menu;
    }

    private static MenuItem NewContextMenuItem(string text, RoutedEventHandler onClick)
    {
        var item = new MenuItem { Header = text };
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

    #region Tree events

    private void treeDestWad_SelectedWadObjectIdsChanged(object? sender, EventArgs e)
    {
        var currentSelection = _treeDestWad.SelectedWadObjectIds.FirstOrDefault();
        if (currentSelection is not null)
            _tool.MainSelection = new MainSelection { WadArea = WadArea.Destination, Id = currentSelection };

        // Update context menu
        if (currentSelection is WadMoveableId)
            _treeDestWad.ContextMenu = _contextMenuMoveableItem;
        else if (currentSelection is WadStaticId)
            _treeDestWad.ContextMenu = _contextMenuStatics;
        else
            _treeDestWad.ContextMenu = null;

        _viewModel.HasDestinationSelection = _treeDestWad.SelectedWadObjectIds.Any();
    }

    private void treeSourceWad_SelectedWadObjectIdsChanged(object? sender, EventArgs e)
    {
        _viewModel.HasSourceSelection = _treeSourceWad.SelectedWadObjectIds.Any();
        var currentSelection = _treeSourceWad.SelectedWadObjectIds.FirstOrDefault();
        if (currentSelection is not null)
            _tool.MainSelection = new MainSelection { WadArea = WadArea.Source, Id = currentSelection };
    }

    private void treeDestWad_DoubleClick(object? sender, EventArgs e)
    {
        if (_treeDestWad.ItemSelected)
            EditItemButton_Click(null, null);
    }

    private void treeSourceWad_DoubleClick(object? sender, EventArgs e)
    {
        if (_treeSourceWad.SelectedWadObjectIds.Count() > 0)
            CopyObject(KeyboardState.ModifierKeys.HasFlag(EditorKey.Alt));
    }

    private void treeDestWad_KeyDown(object? sender, System.Windows.Input.KeyEventArgs e)
    {
        // Text inputs hosted by the tree control (notes, search, game version) must keep their own Delete behavior.
        if (e.OriginalSource is System.Windows.Controls.Primitives.TextBoxBase or System.Windows.Controls.ComboBox)
            return;

        switch (e.Key)
        {
            case System.Windows.Input.Key.Delete:
                WadActions.DeleteObjects(_tool, this.GetWin32Window(), WadArea.Destination, _treeDestWad.SelectedWadObjectIds.ToList());
                break;
        }
    }

    private void treeDestWad_ClickOnEmpty(object? sender, EventArgs e)
    {
        WadActions.LoadWadOpenFileDialog(_tool, this.GetWin32Window(), true);
    }

    private void treeSourceWad_ClickOnEmpty(object? sender, EventArgs e)
    {
        WadActions.LoadWadOpenFileDialog(_tool, this.GetWin32Window(), false);
    }

    private void treeDestWad_MetadataChanged(object? sender, EventArgs e)
    {
        _tool.ToggleUnsavedChanges(true);
    }

    #endregion

    #region Menu / toolbar / button handlers

    private void NewWad2Menu_Click(object? sender, RoutedEventArgs? e) => TryMakeNewWad();

    private void OpenSourceWadMenu_Click(object? sender, RoutedEventArgs? e)
    {
        WadActions.LoadWadOpenFileDialog(_tool, this.GetWin32Window(), false);
    }

    private void OpenDestinationWadMenu_Click(object? sender, RoutedEventArgs? e) => TryOpenDestWad();

    private void OpenReferenceLevelMenu_Click(object? sender, RoutedEventArgs? e)
    {
        WadActions.LoadReferenceLevel(_tool, this.GetWin32Window());
    }

    private void CloseReferenceLevelMenu_Click(object? sender, RoutedEventArgs? e)
    {
        WadActions.UnloadReferenceLevel(_tool);
    }

    private void RefLevelLabel_Click(object? sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_tool.ReferenceLevel is null)
            WadActions.LoadReferenceLevel(_tool, this.GetWin32Window());
    }

    private void SaveMenu_Click(object? sender, RoutedEventArgs? e)
    {
        WadActions.SaveWad(_tool, this.GetWin32Window(), _tool.DestinationWad, false);
    }

    private void SaveAsMenu_Click(object? sender, RoutedEventArgs? e)
    {
        WadActions.SaveWad(_tool, this.GetWin32Window(), _tool.DestinationWad, true);
    }

    private void ExitMenu_Click(object? sender, RoutedEventArgs? e) => Close();

    private void NewMoveableMenu_Click(object? sender, RoutedEventArgs? e)
    {
        var result = WadActions.CreateObject(_tool, this.GetWin32Window(), new WadMoveable(new WadMoveableId()));
        if (result is not null)
            _treeDestWad.Select(result);
    }

    private void NewStaticMenu_Click(object? sender, RoutedEventArgs? e)
    {
        var result = WadActions.CreateObject(_tool, this.GetWin32Window(), new WadStatic(new WadStaticId()));
        if (result is not null)
            _treeDestWad.Select(result);
    }

    private void NewSpriteSequenceMenu_Click(object? sender, RoutedEventArgs? e)
    {
        var result = WadActions.CreateObject(_tool, this.GetWin32Window(), new WadSpriteSequence(new WadSpriteSequenceId()));
        if (result is not null)
            _treeDestWad.Select(result);
    }

    private void ConvertToStaticLightingMenu_Click(object? sender, RoutedEventArgs? e)
    {
        WadActions.ConvertSelectedObjectLighting(_tool, this.GetWin32Window(), _treeDestWad.SelectedWadObjectIds.ToList(), WadMeshLightingType.VertexColors);
    }

    private void ConvertToDynamicLightingMenu_Click(object? sender, RoutedEventArgs? e)
    {
        WadActions.ConvertSelectedObjectLighting(_tool, this.GetWin32Window(), _treeDestWad.SelectedWadObjectIds.ToList(), WadMeshLightingType.Normals);
    }

    private void ConvertToTiledMenu_Click(object? sender, RoutedEventArgs? e)
    {
        WadActions.ConvertSelectedObjectUVMapping(_tool, this.GetWin32Window(), _treeDestWad.SelectedWadObjectIds.ToList(), false);
    }

    private void ConvertToUVMappedMenu_Click(object? sender, RoutedEventArgs? e)
    {
        WadActions.ConvertSelectedObjectUVMapping(_tool, this.GetWin32Window(), _treeDestWad.SelectedWadObjectIds.ToList(), true);
    }

    private void ConsolidateTexturesMenu_Click(object? sender, RoutedEventArgs? e)
    {
        WadActions.ConsolidateSelectedObjectTextures(_tool, this.GetWin32Window(), _treeDestWad.SelectedWadObjectIds.ToList());
    }

    private void AnimatedTexturesMenu_Click(object? sender, RoutedEventArgs? e)
    {
        if (_tool.DestinationWad is null)
            return;

        var context = new WadToolAnimatedTexturesContext(_tool, new List<WadTexture>());
        var textureMap = new WpfAnimatedTextureMapView(_tool);
        var viewModel = new TombLib.WPF.Features.AnimatedTextures.AnimatedTexturesWindowViewModel(context, textureMap);
        var window = new TombLib.WPF.Features.AnimatedTextures.AnimatedTexturesWindow { DataContext = viewModel, Owner = this };
        window.ShowDialog();
    }

    private void MeshEditorMenu_Click(object? sender, RoutedEventArgs? e) => MeshEditorMenu_ClickHandler(null, null);

    private void MeshEditorMenu_ClickHandler(object? sender, EventArgs? e)
    {
        if (_tool.DestinationWad is null)
            return;

        var meshViewModel = new Features.Dialogs.MeshEditor.MeshEditorWindowViewModel(
            _tool, DeviceManager.DefaultDeviceManager, _tool.DestinationWad, focusObjectId: _tool.MainSelection?.Id);
        var meshDialog = new Features.Dialogs.MeshEditor.MeshEditorWindow { DataContext = meshViewModel, Owner = this };
        meshDialog.ShowDialog();
    }

    private void ItemPropertiesMenu_Click(object? sender, RoutedEventArgs? e)
    {
        WadActions.EditLuaProperties(_tool, this.GetWin32Window(), null);
    }

    private void EditSelectedObjectProperties()
    {
        WadActions.EditLuaProperties(_tool, this.GetWin32Window(), _tool.DestinationWad?.TryGet(_tool.MainSelection?.Id).Id);
    }

    private void ConvertWadToTombEngineMenu_Click(object? sender, RoutedEventArgs? e)
    {
        Wad2 dest = WadActions.ConvertWad2ToTombEngine(_tool, this.GetWin32Window(), _tool.DestinationWad);
        if (WadActions.SaveWad(_tool, this.GetWin32Window(), dest, true))
        {
            WadActions.LoadWad(_tool, this.GetWin32Window(), true, dest.FileName);
            PopUpInfo.Show(_popup, null, _panel3D, "Destination wad was converted and saved to " + dest.FileName + ".", PopupType.Info);
        }
    }

    private void OptionsMenu_Click(object? sender, RoutedEventArgs? e)
    {
        var dialog = new Features.Dialogs.Options.OptionsWindow
        {
            DataContext = new Features.Dialogs.Options.OptionsWindowViewModel(_tool),
            Owner = this
        };
        dialog.ShowDialog();

        // FIXME: Later, when WT bloats up, move this to events

        _tool.UndoManager.Resize(_tool.Configuration.AnimationEditor_UndoDepth);

        _panel3D.AnimatePreview = _tool.Configuration.RenderingItem_Animate;
        _panel3D.Invalidate();
    }

    private void AboutMenu_Click(object? sender, RoutedEventArgs? e)
    {
        var about = new TombLib.WPF.Features.About.AboutWindow(
            new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/AboutScreen_800.png")))
        {
            Owner = this
        };
        about.ShowDialog();
    }

    private void DebugReloadCatalogMenu_Click(object? sender, RoutedEventArgs? e)
    {
        TrCatalog.LoadCatalog("Editor\\TRCatalog.xml");
    }

    private void DebugMeshEditorMenu_Click(object? sender, RoutedEventArgs? e)
    {
        var meshViewModel = new Features.Dialogs.MeshEditor.MeshEditorWindowViewModel(
            _tool, DeviceManager.DefaultDeviceManager, _tool.DestinationWad);
        var meshDialog = new Features.Dialogs.MeshEditor.MeshEditorWindow { DataContext = meshViewModel, Owner = this };
        meshDialog.ShowDialog();
    }

    private void EditItemButton_Click(object? sender, RoutedEventArgs? e)
    {
        WadActions.EditObject(_tool, this.GetWin32Window(), DeviceManager.DefaultDeviceManager);
    }

    private void EditSkeletonButton_Click(object? sender, RoutedEventArgs? e)
    {
        WadActions.EditSkeleton(_tool, this.GetWin32Window());
    }

    private void ChangeSlotButton_Click(object? sender, RoutedEventArgs? e)
    {
        var result = WadActions.ChangeSlot(_tool, this.GetWin32Window());
        if (result is not null)
            _treeDestWad.Select(result);
    }

    private void DeleteObjectButton_Click(object? sender, RoutedEventArgs? e)
    {
        if (_treeDestWad.SelectedWadObjectIds.Count() == 0)
            return;

        if (UserDialogs.Show(this.GetWin32Window(), "Do you really want to delete selected objects?", "Delete objects",
                                DialogButtons.YesNo, DialogIcon.Question) != DialogChoice.Yes)
            return;

        foreach (IWadObjectId id in _treeDestWad.SelectedWadObjectIds)
            _tool.DestinationWad.Remove(id);
        _tool.MainSelection = null;
        _tool.WadChanged(WadArea.Destination);
    }

    private void AddObjectButton_Click(object? sender, RoutedEventArgs? e) => CopyObject(false);

    private void AddObjectToDifferentSlotButton_Click(object? sender, RoutedEventArgs? e) => CopyObject(true);

    #endregion

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
