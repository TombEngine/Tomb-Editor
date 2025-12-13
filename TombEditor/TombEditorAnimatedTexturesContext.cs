using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor;

public class TombEditorAnimatedTexturesContext : FormAnimatedTextures.IAnimatedTexturesContext
{
	private readonly Editor _editor;

	public TombEditorAnimatedTexturesContext(Editor editor)
	{
		_editor = editor;
		_editor.EditorEventRaised += EditorEventRaised;
	}

	private void EditorEventRaised(IEditorEvent obj)
	{
		// @FIXME: Proper event handling for these events was removed, because there were
		// serious issues with DarkDataGridView on level reloading or unloading textures.
		if (obj is Editor.LevelChangedEvent or Editor.LoadedTexturesChangedEvent)
			OnContextInvalidated?.Invoke();

		// Update animated texture set combo box
		if (obj is Editor.AnimatedTexturesChangedEvent)
			OnAnimatedTexturesChanged?.Invoke();
	}

	public TextureArea SelectedTexture
	{
		get => _editor.SelectedTexture;
		set => _editor.SelectedTexture = value;
	}

	public List<Texture> AvailableTextures => _editor.Level.Settings.Textures
		.Where(x => x is not null)
		.Cast<Texture>()
		.ToList();

	public Action OnAnimatedTexturesChanged { get; set; }

	public Action OnContextInvalidated { get; set; }

	public List<AnimatedTextureSet> AnimatedTextureSets => _editor.Level.Settings.AnimatedTextureSets;

	public TRVersion.Game Version => _editor.Level.Settings.GameVersion;
}
