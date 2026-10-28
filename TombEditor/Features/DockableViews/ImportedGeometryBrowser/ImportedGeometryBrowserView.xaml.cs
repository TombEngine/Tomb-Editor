#nullable enable

using System.Windows.Controls;
using TombEditor.Controls;
using TombLib.Rendering;

namespace TombEditor.Features.DockableViews.ImportedGeometryBrowser;

public partial class ImportedGeometryBrowserView : UserControl
{
    private readonly ImportedGeometryBrowserViewModel _viewModel;
    private readonly EditorItemPreview _panelItem;

    public ImportedGeometryBrowserView()
    {
        InitializeComponent();

        _panelItem = new EditorItemPreview(Editor.Instance);
        ItemPreviewHost.Child = _panelItem;

        _viewModel = new ImportedGeometryBrowserViewModel(Editor.Instance);
        _viewModel.SelectedGeometryChanged += (_, geo) =>
        {
            // Mirror SelectedGeometry into the native preview's CurrentObject; the preview
            // listens to Editor.ChosenItemsChangedEvent on its own to reset the camera.
            _panelItem.CurrentObject = geo;
        };

        DataContext = _viewModel;
    }

    public void InitializeRendering(RenderingDevice device)
    {
        _panelItem.InitializeRendering(device, Editor.Instance.Configuration.RenderingItem_Antialias);
        _panelItem.AnimatePreview = Editor.Instance.Configuration.RenderingItem_Animate;
    }

    public void Cleanup()
    {
        _viewModel.Cleanup();
        _panelItem.Dispose();
    }
}
