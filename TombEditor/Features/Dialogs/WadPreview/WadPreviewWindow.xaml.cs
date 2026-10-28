#nullable enable

using System;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using TombEditor.Controls;
using TombLib.Rendering;
using TombLib.WPF.CustomControls;
using TombLib.Wad;

namespace TombEditor.Features.Dialogs.WadPreview;

/// <summary>
/// Borderless pop-up that previews a <see cref="Wad2"/>: a WAD tree on the left
/// for selection, a 3D item preview on the right. Mirrors the WinForms
/// <c>FormPreviewWad</c>, with a native WPF rendering surface.
/// </summary>
public partial class WadPreviewWindow : Window
{
    private readonly WadTreeView _wadTree;
    private readonly EditorItemPreview _panelItem;

    public WadPreviewWindow(RenderingDevice device, Editor editor)
    {
        InitializeComponent();

        _panelItem = new EditorItemPreview(editor, allowDragging: false)
        {
            AnimatePreview = editor.Configuration.RenderingItem_Animate
        };
        _panelItem.InitializeRendering(device, editor.Configuration.RenderingItem_Antialias);

        _wadTree = wadTree;
        _wadTree.SelectedWadObjectIdsChanged += OnTreeSelectionChanged;

        panelItemHost.Child = _panelItem;

        DataContextChanged += OnDataContextChanged;

        // Close when focus leaves the window AND the cursor is outside its bounds -
        // preserves the "hover preview" feel of the WinForms version.
        Deactivated += OnDeactivated;
        Closed += (_, _) => _panelItem.Dispose();
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is WadPreviewWindowViewModel vm)
        {
            _wadTree.Wad = vm.Wad;
            _wadTree.SelectFirst();
        }
    }

    private void OnTreeSelectionChanged(object? sender, EventArgs e)
    {
        if (DataContext is not WadPreviewWindowViewModel vm)
            return;

        vm.SelectObject(_wadTree.SelectedWadObjectIds.FirstOrDefault());

        _panelItem.CurrentObject = vm.ObjectToRender;
        _panelItem.ResetCamera();
    }

    private void OnDeactivated(object? sender, EventArgs e)
    {
        var cursor = System.Windows.Forms.Cursor.Position;
        var position = PointFromScreen(new Point(cursor.X, cursor.Y));
        if (!new Rect(RenderSize).Contains(position))
            Close();
    }

    /// <summary>Convenience helper for WinForms callers - sets the WPF owner from an HWND.</summary>
    public void SetOwnerFromHwnd(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero)
            return;

        new WindowInteropHelper(this).Owner = hwnd;
    }
}
