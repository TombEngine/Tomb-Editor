#nullable enable

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.Features.DockableViews.PalettePanel;

public partial class PaletteView : UserControl
{
	private readonly Editor _editor;
	private readonly PaletteViewModel _viewModel;
	private List<ColorC>? _lastTexturePalette;

	public PaletteView()
	{
		InitializeComponent();

		_editor = Editor.Instance;
		_viewModel = new PaletteViewModel(_editor);
		DataContext = _viewModel;

		_editor.EditorEventRaised += EditorEventRaised;

		SizeChanged += (_, _) => UpdateToolbarDock();
		PaletteGridControl.LoadPalette(LevelSettings.LoadPalette());
		UpdatePalette(false);
	}

	/// <summary>Docks the toolbar to the left when the panel is wide and to the top otherwise.</summary>
	private void UpdateToolbarDock()
	{
		bool left = ActualWidth >= ActualHeight * 2.0;
		var spacing = DarkUI.WPF.Defaults.SmallThicknessWidth;

		DockPanel.SetDock(PaletteToolbar, left ? Dock.Left : Dock.Top);
		PaletteToolbar.Orientation = left ? Orientation.Vertical : Orientation.Horizontal;
		PaletteToolbar.Margin = left ? new Thickness(0, 0, spacing, 0) : new Thickness(0, 0, 0, spacing);
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
			PaletteGridControl.LoadPalette(levelChanged.Current.Settings.Palette);
		}

		if (obj is Editor.SelectedObjectChangedEvent)
			PaletteGridControl.PickColor();

		if (obj is Editor.ObjectChangedEvent objectChanged)
		{
			if (objectChanged.ChangeType == ObjectChangeType.Change &&
				objectChanged.Object == _editor.SelectedObject &&
				objectChanged.Object is IColorable)
				PaletteGridControl.PickColor();
		}

		if (obj is Editor.ResetPaletteEvent)
		{
			if (!_editor.Configuration.Palette_TextureSamplingMode)
			{
				PaletteGridControl.LoadPalette(LevelSettings.LoadPalette());
				_editor.Level.Settings.Palette = PaletteGridControl.Palette;
			}
		}

		if (obj is Editor.SelectedLevelTextureChangedSetEvent textureChanged)
		{
			var palette = textureChanged.Texture?.Image.Palette;
			_lastTexturePalette = palette is null ? null : [.. palette];
			UpdatePalette();
		}

		if (obj is Editor.ConfigurationChangedEvent)
			UpdatePalette();
	}

	private void UpdatePalette(bool reloadPalette = true)
	{
		PaletteGridControl.Editable = !_editor.Configuration.Palette_TextureSamplingMode;

		if (reloadPalette)
		{
			bool useTexture = _editor.Configuration.Palette_TextureSamplingMode;

			if (useTexture && _lastTexturePalette is not null && _lastTexturePalette.Count > 0)
				PaletteGridControl.LoadPalette(_lastTexturePalette);
			else
				PaletteGridControl.LoadPalette(_editor.Level.Settings.Palette);

			_editor.LastUsedPaletteColourChange(PaletteGridControl.SelectedColorC);
		}
	}
}
