#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using TombLib.LevelData;

namespace TombEditor.Features.Dialogs.TexturePreview;

/// <summary>
/// View-model for <see cref="TexturePreviewWindow"/>: holds the texture displayed
/// by the read-only preview map view.
/// </summary>
public partial class TexturePreviewWindowViewModel : ObservableObject
{
	[ObservableProperty] private LevelTexture? _visibleTexture;

	public TexturePreviewWindowViewModel(LevelTexture? texture = null)
	{
		_visibleTexture = texture;
	}
}
