using DarkUI.Docking;
using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombEditor.Controls.ContextMenus;
using TombEditor.Forms;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.ToolWindows
{
	public partial class TexturePanel : DarkToolWindow
	{
		private readonly Editor _editor;

		public TexturePanel()
		{
			InitializeComponent();
			CommandHandler.AssignCommandsToControls(Editor.Instance, this, toolTip);

			_editor = Editor.Instance;
			_editor.EditorEventRaised += EditorEventRaised;

			butDeleteTexture.Enabled =
			butBrowseTexture.Enabled =
			butAnimationRanges.Enabled =
			butBumpMaps.Enabled =
			butTextureSounds.Enabled = false;

			panelTextureMap.SelectedTextureChanged += delegate
			{ _editor.SelectedTexture = panelTextureMap.SelectedTexture; };

			// Populate selection tile size
			for (int i = 1; i <= 8; i++)
				cmbTileSize.Items.Add((float)Math.Pow(2, i));

			RepopulateBlendingModes();

			cmbBlending.SelectedIndex = 0;
			if (cmbTileSize.Items.Contains(_editor.Configuration.TextureMap_TileSelectionSize))
				cmbTileSize.SelectedItem = _editor.Configuration.TextureMap_TileSelectionSize;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				_editor.EditorEventRaised -= EditorEventRaised;
			if (disposing && components != null)
				components.Dispose();
			base.Dispose(disposing);
		}

		private void EditorEventRaised(IEditorEvent obj)
		{
			// Disable version-specific controls
			if (obj is Editor.InitEvent ||
				obj is Editor.GameVersionChangedEvent ||
				obj is Editor.LevelChangedEvent)
				UpdateUI();

			// Update texture map
			if (obj is Editor.SelectedTexturesChangedEvent)
			{
				var e = (Editor.SelectedTexturesChangedEvent)obj;

				LevelTexture toSelect = e.Current.Texture as LevelTexture;
				if (toSelect != null)
					comboCurrentTexture.SelectedItem = toSelect;
				panelTextureMap.SelectedTexture = e.Current;

				UpdateTextureControls(e.Current);
			}

			// Center texture on texture map
			if (obj is Editor.SelectTextureAndCenterViewEvent)
			{
				var newTexture = ((Editor.SelectTextureAndCenterViewEvent)obj).Texture;
				comboCurrentTexture.SelectedItem = newTexture.Texture as LevelTexture;
				panelTextureMap.ShowTexture(((Editor.SelectTextureAndCenterViewEvent)obj).Texture);

				if (newTexture.Texture is LevelTexture)
				{
					UpdateTextureControls(newTexture);
					panelTextureMap.ShowTexture(newTexture);
					MakeActive();
				}
			}

			// Reset texture map
			if (obj is Editor.LevelChangedEvent)
			{
				comboCurrentTexture.Items.Clear();
				comboCurrentTexture.Items.AddRange(_editor.Level.Settings.Textures.ToArray());
				comboCurrentTexture.SelectedItem = _editor.Level.Settings.Textures.FirstOrDefault();
			}

			if (obj is Editor.LoadedTexturesChangedEvent)
			{
				// Populate current texture combo box
				LevelTexture current = comboCurrentTexture.SelectedItem as LevelTexture;
				comboCurrentTexture.Items.Clear();
				comboCurrentTexture.Items.AddRange(_editor.Level.Settings.Textures.ToArray());
				if (_editor.Level.Settings.Textures.Contains(current))
					comboCurrentTexture.SelectedItem = current;
				else
					panelTextureMap.ResetVisibleTexture(null);
				if (((Editor.LoadedTexturesChangedEvent)obj).NewToSelect != null)
					comboCurrentTexture.SelectedItem = ((Editor.LoadedTexturesChangedEvent)obj).NewToSelect;
				panelTextureMap.Invalidate();
			}

			// Activate default control
			if (obj is Editor.DefaultControlActivationEvent)
			{
				if (DockPanel != null && ((Editor.DefaultControlActivationEvent)obj).ContainerName == GetType().Name)
				{
					MakeActive();
					comboCurrentTexture.Search();
				}
			}

			// Update tooltip texts
			if (obj is Editor.ConfigurationChangedEvent)
			{
				if (((Editor.ConfigurationChangedEvent)obj).UpdateKeyboardShortcuts)
					CommandHandler.AssignCommandsToControls(_editor, this, toolTip, true);

				if (cmbTileSize.Items.Contains(_editor.Configuration.TextureMap_TileSelectionSize))
					cmbTileSize.SelectedItem = _editor.Configuration.TextureMap_TileSelectionSize;
			}
		}

		private void UpdateUI()
		{
			butDeleteTexture.Enabled =
			butBrowseTexture.Enabled =
			butAnimationRanges.Enabled =
			butMaterialEditor.Enabled = comboCurrentTexture.SelectedItem != null;

            butTextureSounds.Enabled = comboCurrentTexture.SelectedItem != null && 
                _editor.Level.Settings.GameVersion.Native() >= TRVersion.Game.TR3;

            butBumpMaps.Enabled = comboCurrentTexture.SelectedItem != null &&
                (_editor.Level.Settings.GameVersion.Native() == TRVersion.Game.TR4 || _editor.Level.IsTombEngine);

			panelMaterials.Visible = _editor.Level.IsTombEngine;

			RepopulateBlendingModes();
		}

		private void RepopulateBlendingModes()
		{
			cmbBlending.Items.Clear();
			TextureExtensions.BlendModeUserNames(_editor.Level.Settings).ForEach(s => cmbBlending.Items.Add(s));

			// Restore current blending mode
			UpdateTextureControls(_editor.SelectedTexture);
		}

		private void comboCurrentTexture_SelectedValueChanged(object sender, EventArgs e)
		{
			if (panelTextureMap.VisibleTexture != comboCurrentTexture.SelectedItem)
			{
				var selectedTexture = comboCurrentTexture.SelectedItem as LevelTexture;
				panelTextureMap.ResetVisibleTexture(selectedTexture);
				_editor.SelectedLevelTextureChanged(selectedTexture);
			}
			UpdateUI();
		}

		private void comboCurrentTexture_DropDown(object sender, EventArgs e)
		{
			// Make the combo box as wide as possible
			Point screenPointLeft = comboCurrentTexture.PointToScreen(new Point(0, 0));
			Rectangle screenPointRight = Screen.GetBounds(comboCurrentTexture.PointToScreen(new Point(0, comboCurrentTexture.Width)));
			comboCurrentTexture.DropDownWidth = screenPointRight.Right - screenPointLeft.X - 15; // Margin
		}

		private void butTextureSounds_Click(object sender, EventArgs e)
		{
			LevelTexture texture = comboCurrentTexture.SelectedItem as LevelTexture;
			if (texture != null)
				using (var form = new FormFootStepSounds(_editor, texture))
					form.ShowDialog(this);
		}

		private void butBumpMaps_Click(object sender, EventArgs e)
		{
			LevelTexture texture = comboCurrentTexture.SelectedItem as LevelTexture;
			if (texture != null)
				using (var form = new FormBumpMaps(_editor, texture))
					form.ShowDialog(this);
		}

		private void cmbBlending_SelectedIndexChanged(object sender, EventArgs e)
		{
			var selectedTexture = _editor.SelectedTexture;
			selectedTexture.BlendMode = TextureExtensions.ToBlendMode(cmbBlending.SelectedIndex);
			_editor.SelectedTexture = selectedTexture;
		}

		private void UpdateTextureControls(TextureArea texture)
		{
			butDoubleSide.Checked = texture.DoubleSided;

			int newIndex = texture.BlendMode.ToUserIndex();
			if (newIndex < cmbBlending.Items.Count)
				cmbBlending.SelectedIndex = newIndex;
			else
				cmbBlending.SelectedIndex = -1;
		}

		private void butDeleteTexture_Click(object sender, EventArgs e)
		{
			LevelTexture texture = comboCurrentTexture.SelectedItem as LevelTexture;
			if (texture != null)
				EditorActions.RemoveTexture(this, texture);
		}

		private void butBrowseTexture_Click(object sender, EventArgs e)
		{
			LevelTexture texture = comboCurrentTexture.SelectedItem as LevelTexture;
			if (texture != null)
				EditorActions.ReloadResource(this, _editor.Level.Settings, texture);
		}

		private void cmbTileSize_SelectionChangeCommitted(object sender, EventArgs e) => EditorActions.SetSelectionTileSize((float)cmbTileSize.SelectedItem);

		public class PanelTextureMapMain : Controls.PanelTextureMap
		{
			private static readonly Pen _defaultTexturePen = new Pen(Color.FromArgb(230, 238, 150, 238), 2);

			protected override void OnPaintSelection(PaintEventArgs e)
			{
				base.OnPaintSelection(e);

				if (_editor == null || _editor.Level == null)
					return;

				if (_editor.Level.Settings.DefaultTexture == TextureArea.None)
					return;

				if (_editor.Level.Settings.DefaultTexture.Texture == null ||
					_editor.Level.Settings.DefaultTexture.Texture != VisibleTexture)
					return;

				PointF[] edges = new[]
				{
					ToVisualCoord(_editor.Level.Settings.DefaultTexture.TexCoord0),
					ToVisualCoord(_editor.Level.Settings.DefaultTexture.TexCoord1),
					ToVisualCoord(_editor.Level.Settings.DefaultTexture.TexCoord2),
					ToVisualCoord(_editor.Level.Settings.DefaultTexture.TexCoord3)
				};

				e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
				e.Graphics.DrawPolygon(_defaultTexturePen, edges);
			}

			protected override void OnMouseUp(MouseEventArgs e)
			{
				if (e.Button == MouseButtons.Right)
				{
					if (!_startPos.HasValue)
						return;

					var newPos = new Vector2(e.Location.X, e.Location.Y);
					if ((newPos - _startPos.Value).Length() > 4.0f)
						return;

					var menu = new TextureMapContextMenu(_editor, this, FromVisualCoord(e.Location));
					menu.Show(PointToScreen(e.Location));
				}

				base.OnMouseUp(e);
			}
		}

		private void butMaterialEditor_Click(object sender, EventArgs e)
		{
			var list = comboCurrentTexture.Items.Cast<Texture>();
			using (var form = new FormMaterialEditor(list, _editor.Configuration, comboCurrentTexture.SelectedItem as Texture))
			{
				if (form.ShowDialog() == DialogResult.OK && form.MaterialChanged && !string.IsNullOrEmpty(form.MaterialFileName))
					_editor.SendMessage("Material settings for selected texture were saved to " + form.MaterialFileName + ".", PopupType.Info);
			}
		}
	}
}
