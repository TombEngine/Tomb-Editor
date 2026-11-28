#nullable enable

using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using PopUpInfo = TombLib.WPF.PopUpInfo;
using Separator = System.Windows.Controls.Separator;
using System;
using System.Linq;
using TombLib.Input;
using TombLib.Wad;
using TombLib.WPF;
using TombLib.WPF.Input;

namespace WadTool;

/// <summary>Object tree event handlers of the main window.</summary>
public partial class MainWindow
{
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

}
