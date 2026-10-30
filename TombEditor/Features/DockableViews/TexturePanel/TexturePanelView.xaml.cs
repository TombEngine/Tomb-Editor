#nullable enable

using System.Windows.Controls;

namespace TombEditor.Features.DockableViews.TexturePanel;

public partial class TexturePanelView : UserControl
{
	private readonly TexturePanelViewModel _viewModel;

	public TexturePanelView()
	{
		InitializeComponent();

		// Bridge the texture-map selection back to Editor state.
		MapView.SelectedTextureChanged += (_, _) =>
			Editor.Instance.SelectedTexture = MapView.SelectedTexture;

		_viewModel = new TexturePanelViewModel(Editor.Instance);
		_viewModel.RequestResetVisibleTexture += (_, tex) => MapView.ResetVisibleTexture(tex);
		_viewModel.RequestShowTexture += (_, area) => MapView.ShowTexture(area);
		_viewModel.RequestInvalidate += (_, _) => MapView.InvalidateBitmapCache();

		DataContext = _viewModel;
	}

	public void FocusSearch() => TexturesCombo.OpenAndFocusSearch();

	public void Cleanup() => _viewModel.Cleanup();
}
