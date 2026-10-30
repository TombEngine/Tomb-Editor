#nullable enable

using System.Windows.Controls;
using TombEditor.Controls;
using TombLib.Rendering;
using TombLib.Wad;

namespace TombEditor.Features.DockableViews.ItemBrowser;

public partial class ItemBrowserView : UserControl
{
    private readonly ItemBrowserViewModel _viewModel;
    private readonly EditorItemPreview _panelItem;

    public ItemBrowserView()
    {
        InitializeComponent();

        _panelItem = new EditorItemPreview(Editor.Instance);
        ItemPreviewHost.Child = _panelItem;

        _viewModel = new ItemBrowserViewModel(Editor.Instance);
        _viewModel.SelectedItemChanged += OnSelectedItemChanged;

        DataContext = _viewModel;
    }

    public void InitializeRendering(RenderingDevice device)
    {
        _panelItem.InitializeRendering(device, Editor.Instance.Configuration.RenderingItem_Antialias);
        _panelItem.AnimatePreview = Editor.Instance.Configuration.RenderingItem_Animate;
    }

    public void FocusSearch() => ItemsCombo.OpenAndFocusSearch();

    public void Cleanup()
    {
        _viewModel.Cleanup();
        _panelItem.Dispose();
    }

    private void OnSelectedItemChanged(object? sender, IWadObject? wadObject)
    {
        if (wadObject is null)
            return;

        // For WadMoveable we delegate to WadObjectRenderHelper so Lara skin/joints are stitched
        // together correctly (replicates FindLaraSkin in the previous panel).
        if (wadObject is WadMoveable moveable && Editor.Instance.Level?.Settings is { } settings)
            _panelItem.CurrentObject = WadObjectRenderHelper.GetRenderObject(moveable, settings);
        else
            _panelItem.CurrentObject = wadObject;

        _panelItem.ResetCamera();
    }
}
