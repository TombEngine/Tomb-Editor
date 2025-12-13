using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool;

public class WadToolAnimatedTexturesContext : FormAnimatedTextures.IAnimatedTexturesContext
{
	private readonly WadToolClass _tool;
	private readonly List<WadTexture> _userTextures;

	public WadToolAnimatedTexturesContext(WadToolClass tool, List<WadTexture> userTextures)
	{
		_tool = tool;
		_userTextures = userTextures;
		_tool.EditorEventRaised += EditorEventRaised;
	}

	private void EditorEventRaised(IEditorEvent obj)
	{
		// @FIXME: Proper event handling for these events was removed, because there were
		// serious issues with DarkDataGridView on level reloading or unloading textures.
		if (obj is WadToolClass.DestinationWadChangedEvent)
			OnContextInvalidated?.Invoke();

		// Update animated texture set combo box
		if (obj is WadToolClass.AnimatedTexturesChangedEvent)
			OnAnimatedTexturesChanged?.Invoke();
	}

	public TextureArea SelectedTexture { get; set; }

	public List<Texture> AvailableTextures => _tool.DestinationWad.MeshTexturesUnique
		.Where(x => x is not null)
		.Union(_userTextures)
		.Cast<Texture>()
		.Distinct()
		.ToList();

	public Action OnAnimatedTexturesChanged { get; set; }

	public Action OnContextInvalidated { get; set; }

	public List<AnimatedTextureSet> AnimatedTextureSets => _tool.DestinationWad.AnimatedTextureSets;

	public TRVersion.Game Version { get => _tool.DestinationWad.GameVersion; }
}
