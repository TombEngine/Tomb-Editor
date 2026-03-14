using System.Collections.Generic;
using System.Windows.Controls;
using TombLib.LevelData;
using TombLib.Utils;
using TombEditor.WPF.ViewModels;

namespace TombEditor.WPF.ToolWindows;

public partial class Palette : UserControl
{
	private readonly Editor _editor;
	private readonly PaletteViewModel _viewModel;
	private List<ColorC> _lastTexturePalette;

	public Palette()
	{
		InitializeComponent();

		_editor = Editor.Instance;
		_viewModel = new PaletteViewModel(_editor);
		DataContext = _viewModel;

		_editor.EditorEventRaised += EditorEventRaised;

		PaletteGrid.LoadPalette(LevelSettings.LoadPalette());
		UpdatePalette(false);
	}

	public void Cleanup()
	{
		_editor.EditorEventRaised -= EditorEventRaised;
		_viewModel.Cleanup();
	}

	private void EditorEventRaised(IEditorEvent obj)
	{
		if (obj is Editor.LevelChangedEvent levelChanged)
		{
			_lastTexturePalette = null;
			PaletteGrid.LoadPalette(levelChanged.Current.Settings.Palette);
		}

		if (obj is Editor.SelectedObjectChangedEvent)
			PaletteGrid.PickColor();

		if (obj is Editor.ObjectChangedEvent objectChanged)
		{
			if (objectChanged.ChangeType == ObjectChangeType.Change &&
				objectChanged.Object == _editor.SelectedObject &&
				objectChanged.Object is IColorable)
				PaletteGrid.PickColor();
		}

		if (obj is Editor.ResetPaletteEvent)
		{
			if (!_editor.Configuration.Palette_TextureSamplingMode)
			{
				PaletteGrid.LoadPalette(LevelSettings.LoadPalette());
				_editor.Level.Settings.Palette = PaletteGrid.Palette;
			}
		}

		if (obj is Editor.SelectedLevelTextureChangedSetEvent textureChanged)
		{
			_lastTexturePalette = new List<ColorC>(textureChanged.Texture.Image.Palette);
			UpdatePalette();
		}

		if (obj is Editor.ConfigurationChangedEvent)
			UpdatePalette();
	}

	private void UpdatePalette(bool reloadPalette = true)
	{
		PaletteGrid.Editable = !_editor.Configuration.Palette_TextureSamplingMode;

		if (reloadPalette)
		{
			bool useTexture = _editor.Configuration.Palette_TextureSamplingMode;

			if (useTexture && _lastTexturePalette != null && _lastTexturePalette.Count > 0)
				PaletteGrid.LoadPalette(_lastTexturePalette);
			else
				PaletteGrid.LoadPalette(_editor.Level.Settings.Palette);

			_editor.LastUsedPaletteColourChange(PaletteGrid.SelectedColorC);
		}
	}
}
