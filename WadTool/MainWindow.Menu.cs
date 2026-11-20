#nullable enable

using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using PopUpInfo = TombLib.WPF.PopUpInfo;
using Separator = System.Windows.Controls.Separator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TombLib.Graphics;
using TombLib.Services;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using TombLib.WPF;
using WadTool.Controls;

namespace WadTool;

/// <summary>Menu, toolbar and button handlers of the main window.</summary>
public partial class MainWindow
{
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
        WindowConfiguration.ConfigureWindow(window, _tool.Configuration, "FormAnimatedTextures");
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

}
