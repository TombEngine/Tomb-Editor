using DarkUI.Controls;
using DarkUI.Extensions;
using DarkUI.Forms;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Controls;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.Forms
{
	public partial class FormAnimatedTextures : DarkForm
	{
		public interface IAnimatedTexturesContext
		{
			TextureArea SelectedTexture { get; set; }
			List<Texture> AvailableTextures { get; }
			List<AnimatedTextureSet> AnimatedTextureSets { get; }
			Action OnAnimatedTexturesChanged { get; set; }
			Action OnContextInvalidated { get; set; }
			TRVersion.Game Version { get; }
		}

		private enum ProceduralAnimationType
		{
			HorizontalStretch,
			VerticalStretch,
			Scale,
			HorizontalSkew1,
			HorizontalSkew2,
			VerticalSkew1,
			VerticalSkew2,
			Spin,
			HorizontalPan,
			VerticalPan,
			Shake
		}

		private enum AnimGenerationType
		{
			New,
			Clone,
			Merge,
			Replace,
			AddFrames
		}

		private class TransparentBindingList<T> : BindingList<T>
		{
			public TransparentBindingList(IList<T> list) : base(list) { }
			public new IList<T> Items => base.Items;
		}
		private struct CachedImageInfo
		{
			public ImageC _image;
			public Vector2 _sourceTexCoord0;
			public Vector2 _sourceTexCoord1;
			public Vector2 _sourceTexCoord2;
			public Vector2 _sourceTexCoord3;
			public Size _destinationSize;
		}
		private struct NgAnimatedTextureSettingPair
		{
			public readonly float Key;
			public readonly string Value;

			public NgAnimatedTextureSettingPair(float key, string value)
			{
				Key = key;
				Value = value;
			}

			public override string ToString()
			{
				return Value;
			}
		}

		private struct TombEngineFpsSetting
		{
			public readonly float Key;
			public readonly string Value;

			public TombEngineFpsSetting(float key, string value)
			{
				Key = key;
				Value = value;
			}

			public override string ToString()
			{
				return Value;
			}
		}

		public AnimatedTextureSet SelectedSet { get; set; }

		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly PopUpInfo popup = new PopUpInfo();

		private readonly Cache<CachedImageInfo, Bitmap> _imageCache;
		private readonly static ImageC transparentBackground = ImageC.FromSystemDrawingImage(TombLib.Properties.Resources.misc_TransparentBackground);

		private readonly Timer _previewTimer = new Timer();
		private AnimatedTextureFrame _previewCurrentFrame;
		private int _previewCurrentRepeatTimes;
		private const int _maxLegacyFrames = 16;
		private const string animNameCombineString = " (with ";
		private float _lastX;
		private float _lastY;

		private readonly TRVersion.Game _version;
		private readonly TextureMapBase _textureMap;
		private readonly IAnimatedTexturesContext _context;

		private List<AnimatedTextureSet> _backupSets = new List<AnimatedTextureSet>();
		private List<AnimatedTextureSet> _animatedTextureSets = new List<AnimatedTextureSet>();

		public FormAnimatedTextures(TextureMapBase _textureMap,
			IAnimatedTexturesContext context, ConfigurationBase configuration)
		{
			InitializeComponent();

			_version = context.Version;
			_animatedTextureSets = context.AnimatedTextureSets;
			_context = context;

			// The texture panel is initialized in Tomb Editor or WadTool
			this._textureMap = _textureMap;
			_textureMap.Dock = DockStyle.Fill;
			_textureMap.DoubleClick += _textureMap_DoubleClick;
			panelTextureMapContainer.Controls.Add(_textureMap);

			context.OnAnimatedTexturesChanged = OnAnimatedTexturesChanged;
			context.OnContextInvalidated = () => Close();

			_previewTimer.Tick += _previewTimer_Tick;
			previewImage.Paint += _onPicturePreviewPaint;

			// Setup image cache
			_imageCache = new Cache<CachedImageInfo, Bitmap>(512, subsection =>
			{
				return GetPerspectivePreview(subsection._image, subsection._sourceTexCoord0, subsection._sourceTexCoord1, subsection._sourceTexCoord2,
					subsection._sourceTexCoord3, subsection._destinationSize).ToBitmap();
			});

			// Set window property handlers
			ConfigurationBase.ConfigureWindow(this, configuration);

			// Setup controls
			SetupControls();
			UpdateEnable();

			// Setup data grid view
			texturesDataGridViewControls.DataGridView = texturesDataGridView;
			texturesDataGridViewControls.CreateNewRow = GetSelectedAnimatedTextureFrame;
			texturesDataGridViewColumnTexture.DataSource = new BindingList<Texture>(_context.AvailableTextures);

			// Init state
			if (comboAnimatedTextureSets.Items.Count > 0)
				comboAnimatedTextureSets.SelectedIndex = 0;

			// Setup texture map
			if (_context.SelectedTexture.TextureIsInvisible)
				_textureMap.ResetVisibleTexture(_context.AvailableTextures.Any() ? _context.AvailableTextures.First() : null);
			else
				_textureMap.ShowTexture(_context.SelectedTexture);

			// Backup existing animated texture sets
			foreach (var set in _animatedTextureSets)
				_backupSets.Add(set.Clone());

			// Hack to prevent artifacts with progress bar redraw
			Resize += (s, e) => { Refresh(); };
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_previewTimer?.Dispose();
				_imageCache?.Dispose();
			}
			base.Dispose(disposing);
		}

		private void OnAnimatedTexturesChanged()
		{
			while (comboAnimatedTextureSets.Items.Count > _animatedTextureSets.Count)
				comboAnimatedTextureSets.Items.RemoveAt(comboAnimatedTextureSets.Items.Count - 1);

			for (int i = 0; i < comboAnimatedTextureSets.Items.Count; ++i)
				if (!ReferenceEquals(comboAnimatedTextureSets.Items[i], _animatedTextureSets[i]))
					comboAnimatedTextureSets.Items[i] = _animatedTextureSets[i];

			while (comboAnimatedTextureSets.Items.Count < _animatedTextureSets.Count)
				comboAnimatedTextureSets.Items.Add(_animatedTextureSets[comboAnimatedTextureSets.Items.Count]);

			if (comboAnimatedTextureSets.SelectedItem == null)
				if (comboAnimatedTextureSets.Items.Count > 0)
					comboAnimatedTextureSets.SelectedIndex = comboAnimatedTextureSets.Items.Count - 1;
				else
					comboAnimatedTextureSets.Text = "";
			comboAnimatedTextureSets.Invalidate();

			UpdateCurrentAnimationDisplay();
			_textureMap.Invalidate();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.N)
			{
				AddFrame();
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		private AnimatedTextureSet GetOrAddCurrentSet()
		{
			var currentSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
			if (currentSet == null)
				currentSet = NewSet();
			return currentSet;
		}

		private AnimatedTextureSet NewSet()
		{
			var newSet = new AnimatedTextureSet() { Name = "Animation #" + _animatedTextureSets.Count };
			_animatedTextureSets.Add(newSet);
			_context.OnAnimatedTexturesChanged.Invoke();
			comboAnimatedTextureSets.SelectedItem = newSet;
			return newSet;
		}

		private void AddFrame()
		{
			var selectedSet = GetOrAddCurrentSet();
			var frame = GetSelectedAnimatedTextureFrame();
			if (frame != null)
			{
				selectedSet.Frames.Add(frame);
				_context.OnAnimatedTexturesChanged.Invoke();
			}

			texturesDataGridView.ClearSelection();
			texturesDataGridView.CurrentCell = texturesDataGridView.Rows[texturesDataGridView.Rows.Count - 1].Cells[0];
		}

		private void SetupControls()
		{
			// Clear previous elements
			comboCurrentTexture.Items.Clear();
			comboAnimatedTextureSets.Items.Clear();
			comboEffect.Items.Clear();
			comboUvRotate.Items.Clear();
			comboFps.Items.Clear();

			// Populate texture list
			comboCurrentTexture.Items.AddRange(_context.AvailableTextures.ToArray());
			if (_context.SelectedTexture.Texture != null)
				comboCurrentTexture.SelectedItem = _context.SelectedTexture.Texture;
			else
				comboCurrentTexture.SelectedItem = _context.AvailableTextures.FirstOrDefault();

			// Populate anim sets list
			while (comboAnimatedTextureSets.Items.Count < _animatedTextureSets.Count)
				comboAnimatedTextureSets.Items.Add(_animatedTextureSets[comboAnimatedTextureSets.Items.Count]);

			// Add common animation types
			comboEffect.Items.Add(AnimatedTextureAnimationType.Frames);

			if (_version.Native() is TRVersion.Game.TR4 or TRVersion.Game.TR5)
			{
				comboEffect.Items.Add(AnimatedTextureAnimationType.UVRotate);
			}

			if (_version == TRVersion.Game.TombEngine)
			{
				comboEffect.Items.Add(AnimatedTextureAnimationType.Video);
				comboEffect.Items.Add(AnimatedTextureAnimationType.UVRotate);
			}

			// NG settings
			if (_version == TRVersion.Game.TRNG)
			{
				// For now, add only P-Frames mode, as Half-Rotate and River-Rotate modes are faulty.
				comboEffect.Items.Add(AnimatedTextureAnimationType.PFrames);

				// Fill uv rotate combobox
				for (int i = -64; i < 0; i++)
					comboUvRotate.Items.Add(new NgAnimatedTextureSettingPair(i, "UvRotate = " + i));
				comboUvRotate.Items.Add(new NgAnimatedTextureSettingPair(0, "Default (from script)"));
				for (int i = 1; i <= 64; i++)
					comboUvRotate.Items.Add(new NgAnimatedTextureSettingPair(i, "UvRotate = " + i));

				// Fill with NG predefined FPS values required for river rotate etc.
				for (int i = 1; i <= 32; i++)
					comboFps.Items.Add(new NgAnimatedTextureSettingPair(i, i + " FPS"));
			}
			else if (_version == TRVersion.Game.TombEngine)
			{
				comboUvRotate.Enabled = false;

				comboFps.Enabled = true;

				for (int i = 1; i <= 120; i++)
					comboFps.Items.Add(new TombEngineFpsSetting(i, i + " FPS"));
			}
			else
			{
				comboUvRotate.Enabled = false;
				comboFps.Enabled = false;
			}

			// Legacy engine settings
			comboEffect.Enabled = _version.Native() >= TRVersion.Game.TR4;

			comboProcPresets.SelectedIndex = 0;
			numFrames.Value = _maxLegacyFrames;
		}

		private void UpdateCurrentAnimationDisplay()
		{
			var selectedSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
			if (selectedSet == null)
				selectedSet = new AnimatedTextureSet();

			// Setup frames
			var dataSource = (TransparentBindingList<AnimatedTextureFrame>)texturesDataGridView.DataSource;
			if (dataSource?.Items != selectedSet.Frames)
			{
				var newDataSource = new TransparentBindingList<AnimatedTextureFrame>(selectedSet.Frames);
				texturesDataGridView.DataSource = newDataSource;
				newDataSource.ListChanged += NewDataSource_ListChanged; // This needs to happen *after* it's set as a datasource
																		// otherwise the view is updated before the datagridview is in a valid state and an exception can occure in certain cicumstances while cell formatting a deleted row.
			}
			else
			{
				try
				{
					dataSource.ListChanged -= NewDataSource_ListChanged;
					dataSource.ResetBindings(); // TODO Preserve selection?
				}
				finally
				{
					dataSource.ListChanged += NewDataSource_ListChanged;
				}
			}
			UpdateEnable();

			// Setup preview
			if (selectedSet.Frames.Count == 0)
			{
				_previewTimer.Enabled = false;
				previewImage.Image = null;
				previewProgressBar.Maximum = 0;
				previewProgressBar.Value = 0;
				previewProgressBar.TextMode = DarkProgressBarMode.NoText;
			}
			else
			{
				_previewTimer.Enabled = true;
				previewProgressBar.TextMode = DarkProgressBarMode.XOfN;
			}
			if (_version == TRVersion.Game.TombEngine && selectedSet.AnimationType == AnimatedTextureAnimationType.UVRotate)
				_previewTimer.Interval = (int)MathC.Clamp(Math.Round(1000.0f / 30.0f), 1, int.MaxValue);
			else
				_previewTimer.Interval = (int)MathC.Clamp(Math.Round(1000.0f / selectedSet.Fps), 1, int.MaxValue); // Without NG, the default value of 15 should be present at this point.
			_lastX = 0;
			_lastY = 0;

			// Update warning about too many frames
			int frameCount = 0;
			AnimatedTextureSet currentSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
			if (currentSet != null)
				foreach (AnimatedTextureFrame frame in currentSet.Frames)
					frameCount += frame.Repeat;

			if (tooManyFramesWarning.Visible = _version == TRVersion.Game.TRNG && frameCount > _maxLegacyFrames)
				toolTip.SetToolTip(tooManyFramesWarning, "This animation uses " + frameCount + " frames which is more than " + _maxLegacyFrames + "!\nThis will cause crash in TRNG!");

			if (comboEffect.Items.Contains(selectedSet.AnimationType))
				comboEffect.SelectedItem = selectedSet.AnimationType;
			else
				comboEffect.SelectedItem = null;

			if (_version == TRVersion.Game.TRNG)
			{
				OnEffectChanged();
				switch (selectedSet.AnimationType)
				{
					case AnimatedTextureAnimationType.Frames:
						numericUpDownFPS.Value = (decimal)selectedSet.Fps;
						break;
					case AnimatedTextureAnimationType.UVRotate:
					case AnimatedTextureAnimationType.HalfRotate:
					case AnimatedTextureAnimationType.RiverRotate:
						NgSelectComboboxValue(selectedSet.Fps, comboFps);
						NgSelectComboboxValue(selectedSet.UvRotate, comboUvRotate);
						break;
				}
			}
			else if (_version == TRVersion.Game.TombEngine)
			{
				OnEffectChanged();
				switch (selectedSet.AnimationType)
				{
					case AnimatedTextureAnimationType.Frames:
						numericUpDownFPS.Value = (decimal)selectedSet.Fps;
						break;
					case AnimatedTextureAnimationType.UVRotate:
						numericUpDownFPS.Value = (decimal)selectedSet.TenUvRotateSpeed;
						numericUpDownUvRotateDirection.Value = (decimal)selectedSet.TenUvRotateDirection;
						break;
				}
			}
		}

		private void NgSelectComboboxValue(float value, DarkComboBox cb)
		{
			if (cb.Items.Count == 0)
				return;

			var bestItem = cb.Items.Cast<NgAnimatedTextureSettingPair>().First();
			foreach (NgAnimatedTextureSettingPair item in cb.Items)
				if (Math.Abs(item.Key - value) < Math.Abs(bestItem.Key - value)) // Chose the entry closest to the float we are looking for.
					bestItem = item;
			cb.SelectedItem = bestItem;
		}

		private void NewDataSource_ListChanged(object sender, ListChangedEventArgs e)
		{
			_context.OnAnimatedTexturesChanged.Invoke();
		}

		private void _previewTimer_Tick(object sender, EventArgs e)
		{
			var selectedSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;

			// Get frame and advance frame
			int frameCount = selectedSet?.Frames?.Count ?? 0;
			if (frameCount == 0)
			{
				previewImage.Image = null;
				return;
			}

			int frameIndex = 0;

			// Only advance to next frame for frame type sequences, otherwise show selected or first frame
			if (selectedSet.AnimationType == AnimatedTextureAnimationType.Frames)
			{
				if (++_previewCurrentRepeatTimes < (_previewCurrentFrame?.Repeat ?? 0))
					return;

				for (int i = 0; i < frameCount; ++i)
					if (selectedSet.Frames[i] == _previewCurrentFrame)
					{
						frameIndex = (i + 1) % frameCount; // Advance to next image
						break;
					}

				_previewCurrentRepeatTimes = 0;
			}
			else if (texturesDataGridView.CurrentRow != null)
				frameIndex = texturesDataGridView.CurrentRow.Index;

			_previewCurrentFrame = selectedSet.Frames[frameIndex];

			// Update view
			previewProgressBar.Minimum = 0;
			previewProgressBar.Maximum = frameCount - 1;
			previewProgressBar.SetProgressNoAnimation(frameIndex);

			previewImage.Image = _imageCache[new CachedImageInfo
			{
				_image = _previewCurrentFrame.Texture.Image,
				_sourceTexCoord0 = _previewCurrentFrame.TexCoord0,
				_sourceTexCoord1 = _previewCurrentFrame.TexCoord1,
				_sourceTexCoord2 = _previewCurrentFrame.TexCoord2,
				_sourceTexCoord3 = _previewCurrentFrame.TexCoord3,
				_destinationSize = previewImage.ClientSize
			}];
		}

		private float _scrollProgress = 0f;

		private void _onPicturePreviewPaint(object sender, PaintEventArgs args)
		{
			var selectedSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
			if (selectedSet == null)
				return;

			var image = (sender as PictureBox).Image;
			if (image == null)
				return;

			var g = args.Graphics;

			if (selectedSet.IsUvRotate && comboEffect.SelectedItem != null)
			{
				if (_version == TRVersion.Game.TRNG)
				{
					g.DrawImage(image, new Point(0, (int)_lastY * 2 - 128));
					g.DrawImage(image, new Point(0, (int)_lastY * 2));

					_lastY += selectedSet.UvRotate;
					if (_lastY >= 64 && selectedSet.UvRotate > 0)
						_lastY = 0;
					if (_lastY <= 0 && selectedSet.UvRotate < 0)
						_lastY = 64;
				}
				else
				{
					const float Tile = 64f;  
					const float Canvas = 128f;
					const float UvRotateStepFactor = Canvas / 30f; 

					float angleDeg = selectedSet.TenUvRotateDirection;

					float theta = (angleDeg + 90f) * (MathF.PI / 180f);
					float dirX = MathF.Cos(theta);
					float dirY = MathF.Sin(theta);

					float step = UvRotateStepFactor * selectedSet.TenUvRotateSpeed;

					_lastX = (_lastX + step * dirX) % Tile;
					_lastY = (_lastY + step * dirY) % Tile;
					if (_lastX < 0) _lastX += Tile;
					if (_lastY < 0) _lastY += Tile;

					float dx = _lastX * 2f;
					float dy = _lastY * 2f;

					g.DrawImage(image, new PointF(dx - Canvas, dy - Canvas));
					g.DrawImage(image, new PointF(dx, dy - Canvas));
					g.DrawImage(image, new PointF(dx - Canvas, dy));
					g.DrawImage(image, new PointF(dx, dy));
				}
			}
			else
			{
				g.DrawImage(image, new Point(0, 0));
			}
		}

		private void butAnimatedTextureSetNew_Click(object sender, EventArgs e)
		{
			NewSet();
		}

		private void butAnimatedTextureSetDelete_Click(object sender, EventArgs e)
		{
			var selectedSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
			if (selectedSet == null)
				return;

			if (selectedSet.Frames.Count > 0)
				if (DarkMessageBox.Show(this, "Are you sure you want to delete the animation set '" + selectedSet +
					"'?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
					return;

			_animatedTextureSets.Remove(selectedSet);
			_context.OnAnimatedTexturesChanged.Invoke();
		}

		private void UpdateEnable()
		{
			bool enable = comboAnimatedTextureSets.SelectedItem is AnimatedTextureSet;
			settingsPanel.Enabled = enable;
			texturesDataGridViewControls.Enabled = enable;
			butAnimatedTextureSetDelete.Enabled = enable;
			butEditSetName.Enabled = enable;
			butCloneProcAnim.Enabled = enable;
			butMergeProcAnim.Enabled = enable;
			butReplaceProcAnim.Enabled = enable;
			butAddProcAnim.Enabled = enable;

			if (enable)
			{
				AnimatedTextureSet selectedSet = (AnimatedTextureSet)comboAnimatedTextureSets.SelectedItem;
				texturesDataGridView.Enabled = selectedSet.Frames.Count > 0;
			}
			else
				texturesDataGridView.Enabled = enable;
		}

		private void comboAnimatedTextureSets_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboAnimatedTextureSets.SelectedIndex >= 0)
				SelectedSet = _animatedTextureSets[comboAnimatedTextureSets.SelectedIndex];
			else
				SelectedSet = null;
			UpdateEnable();
			UpdateCurrentAnimationDisplay();
			_textureMap.Invalidate();
		}

		private void butUpdate_Click(object sender, EventArgs e)
		{
			var newFrame = GetSelectedAnimatedTextureFrame();
			if (newFrame == null)
				return;

			foreach (DataGridViewRow selectedRow in texturesDataGridView.SelectedRows)
			{
				var frame = selectedRow.DataBoundItem as AnimatedTextureFrame;
				frame.Texture = newFrame.Texture;
				frame.TexCoord0 = newFrame.TexCoord0;
				frame.TexCoord1 = newFrame.TexCoord1;
				frame.TexCoord2 = newFrame.TexCoord2;
				frame.TexCoord3 = newFrame.TexCoord3;
			}
			_context.OnAnimatedTexturesChanged.Invoke();
		}

		private AnimatedTextureFrame GetSelectedAnimatedTextureFrame()
		{
			TextureArea textureArea = _textureMap.SelectedTexture;
			if (!(textureArea.Texture is LevelTexture) && !(textureArea.Texture is WadTexture))
			{
				popup.ShowError(_textureMap, "No valid texture region selected", "Invalid selection");
				return null;
			}

			return new AnimatedTextureFrame
			{
				Texture = textureArea.Texture,
				TexCoord0 = textureArea.TexCoord0,
				TexCoord1 = textureArea.TexCoord1,
				TexCoord2 = textureArea.TexCoord2,
				TexCoord3 = textureArea.TexCoord3
			};
		}

		private bool GenerateProceduralAnimation(ProceduralAnimationType type, int resultingFrameCount = _maxLegacyFrames, float effectStrength = 1.0f, bool smooth = true, bool loop = true, AnimGenerationType genType = AnimGenerationType.New)
		{
			TextureArea textureArea = _textureMap.SelectedTexture;
			if (!(textureArea.Texture is LevelTexture) && !(textureArea.Texture is WadTexture))
			{
				popup.ShowError(_textureMap, "No valid texture region selected", "Invalid selection");
				return false;
			}

			// Limit effect strength to reasonable value and additionally reverse it for scale/stretch types,
			// because for scale/stretch types, visible effect opposes mathematical function.
			effectStrength = (float)MathC.Clamp((type <= ProceduralAnimationType.Scale) ? -effectStrength : effectStrength, -1.0, 1.0);

			int startIndex = 0;
			AnimatedTextureSet targetSet = null;

			// Initialize any type which involves existing set
			if (genType != AnimGenerationType.New)
			{
				if (comboAnimatedTextureSets.SelectedItem == null || !(comboAnimatedTextureSets.SelectedItem is AnimatedTextureSet))
				{
					popup.ShowError(_textureMap, "No valid animation selected!");
					return false;
				}

				targetSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;

				if (genType == AnimGenerationType.Clone)
					targetSet = targetSet.Clone(); // Make a copy and use it for clone type
				else if (genType == AnimGenerationType.AddFrames && targetSet.Frames.Count > 0)
					startIndex = texturesDataGridView.CurrentRow.Index + 1; // Insert frames at selection point
				else if (genType == AnimGenerationType.Replace)
					targetSet.Frames.Clear();

				if (string.IsNullOrEmpty(targetSet.Name))
					targetSet.Name = "Animation #" + (_animatedTextureSets.Count + 1);
			}
			else
				targetSet = new AnimatedTextureSet() { Name = "Procedural animation #" + (_animatedTextureSets.Count + 1) };

			if (genType == AnimGenerationType.New ||
				genType == AnimGenerationType.Replace ||
				genType == AnimGenerationType.AddFrames)
			{
				// Generate dummy reference frames and insert them into set

				for (int i = 0; i < resultingFrameCount; i++)
				{
					var dummyFrame = new AnimatedTextureFrame
					{
						Texture = textureArea.Texture,
						TexCoord0 = textureArea.TexCoord0,
						TexCoord1 = textureArea.TexCoord1,
						TexCoord2 = textureArea.TexCoord2,
						TexCoord3 = textureArea.TexCoord3
					};

					if (genType != AnimGenerationType.AddFrames)
						targetSet.Frames.Add(dummyFrame);
					else
						targetSet.Frames.Insert(startIndex, dummyFrame);
				}
			}

			// Actualize frame count, cause it may change in regard of generation type
			if (genType != AnimGenerationType.AddFrames)
				resultingFrameCount = targetSet.Frames.Count;

			if (resultingFrameCount <= 0)
			{
				popup.ShowError(_textureMap, "Selected animation has no frames!");
				return false;
			}

			// Used to bypass smooth and loop, which doesn't make sense with single-frame anims
			bool realAnim = resultingFrameCount > 1;

			// Crop existing postfixes to prevent "Copy of Copy of Copy..." problem
			if (genType != AnimGenerationType.New)
			{
				int foundPostfixPos = targetSet.Name.IndexOf(animNameCombineString);
				if (foundPostfixPos != -1)
					targetSet.Name = targetSet.Name.Substring(0, foundPostfixPos);
				targetSet.Name += animNameCombineString + comboProcPresets.SelectedItem.ToString() + " effect)";
			}

			var rnd = new Random(); // Needed for shake procedure

			// For shake procedure, smooth makes no sense without loop and vice versa.
			if (type == ProceduralAnimationType.Shake && smooth)
				loop = true;

			// Process frames

			for (int cnt = 0, i = startIndex; cnt < resultingFrameCount; cnt++, i++)
			{
				var referenceFrame = targetSet.Frames[i].Clone();
				float midFrame = loop && realAnim ? resultingFrameCount / 2.0f : resultingFrameCount;
				float bias = Math.Abs(cnt - midFrame) / midFrame;
				float weight = (smooth && realAnim ? (float)MathC.SmoothStep(0.0, 1.0, bias) : bias) * effectStrength;

				switch (type)
				{
					case ProceduralAnimationType.HorizontalSkew1:
					case ProceduralAnimationType.HorizontalSkew2:
					case ProceduralAnimationType.VerticalSkew1:
					case ProceduralAnimationType.VerticalSkew2:
						{
							bool otherDirection = (int)type % 2 == 0;

							// Invert skew angle in regard of strength sign and direction
							if (effectStrength > 0.0f)
								weight = effectStrength - weight;
							else
								weight = Math.Abs(weight);
							if (!otherDirection)
								weight = 1.0f - weight;

							int[] index = new int[2];
							Vector2[] coord1 = new Vector2[2];
							Vector2[] coord2 = new Vector2[2];

							if (otherDirection)
							{
								index[0] = 0;
								index[1] = 2;
							}
							else
							{
								index[0] = 1;
								index[1] = 3;
							}

							if (type == ProceduralAnimationType.HorizontalSkew2)
							{
								coord2[0] = referenceFrame.TexCoord3;
								coord2[1] = referenceFrame.TexCoord1;
							}
							else
							{
								coord2[0] = referenceFrame.TexCoord1;
								coord2[1] = referenceFrame.TexCoord3;
							}

							if (type == ProceduralAnimationType.HorizontalSkew1)
							{
								coord1[0] = referenceFrame.TexCoord2;
								coord1[1] = referenceFrame.TexCoord0;
							}
							else
							{
								coord1[0] = referenceFrame.TexCoord0;
								coord1[1] = referenceFrame.TexCoord2;
							}

							for (int c = 0; c < 2; c++)
							{
								var result = Vector2.Lerp(coord1[c], coord2[c], weight);
								switch (index[c])
								{
									case 0: targetSet.Frames[i].TexCoord0 = result; break;
									case 1: targetSet.Frames[i].TexCoord1 = result; break;
									case 2: targetSet.Frames[i].TexCoord2 = result; break;
									case 3: targetSet.Frames[i].TexCoord3 = result; break;
								}
							}
						}
						break;

					case ProceduralAnimationType.HorizontalStretch:
					case ProceduralAnimationType.VerticalStretch:
					case ProceduralAnimationType.Scale:
						{
							// Reverse effect strength for negative numbers for proper UV positioning
							if (effectStrength < 0)
								weight += Math.Abs(effectStrength);

							// 0 = horizontal pass, 1 = vertical pass. Scale type uses both.
							bool[] passes = new bool[2]
							{
								type == ProceduralAnimationType.HorizontalStretch || type == ProceduralAnimationType.Scale,
								type == ProceduralAnimationType.VerticalStretch   || type == ProceduralAnimationType.Scale
							};

							for (int p = 0; p < 2; p++)
								if (passes[p])
								{
									var center1 = Vector2.Lerp(referenceFrame.TexCoord0, (p == 0 ? referenceFrame.TexCoord3 : referenceFrame.TexCoord1), 0.5f);
									var center2 = Vector2.Lerp(referenceFrame.TexCoord2, (p == 0 ? referenceFrame.TexCoord1 : referenceFrame.TexCoord3), 0.5f);

									targetSet.Frames[i].TexCoord0 = Vector2.Lerp(referenceFrame.TexCoord0, center1, weight);
									targetSet.Frames[i].TexCoord1 = Vector2.Lerp(referenceFrame.TexCoord1, (p == 0 ? center2 : center1), weight);
									targetSet.Frames[i].TexCoord2 = Vector2.Lerp(referenceFrame.TexCoord2, center2, weight);
									targetSet.Frames[i].TexCoord3 = Vector2.Lerp(referenceFrame.TexCoord3, (p == 0 ? center1 : center2), weight);

									if (p == 0)
										referenceFrame = targetSet.Frames[i].Clone(); // Reassign reference after first pass
								}
						}
						break;

					case ProceduralAnimationType.Spin:
						{
							// Invert weight and shift angle to start from the beginning
							weight = effectStrength - weight;
							double currAngle = ((2 * Math.PI) * weight) - Math.PI * 0.25;

							var cross1 = Vector3.Cross(new Vector3(referenceFrame.TexCoord0.X, referenceFrame.TexCoord0.Y, 1),
													   new Vector3(referenceFrame.TexCoord2.X, referenceFrame.TexCoord2.Y, 1));
							var cross2 = Vector3.Cross(new Vector3(referenceFrame.TexCoord1.X, referenceFrame.TexCoord1.Y, 1),
													   new Vector3(referenceFrame.TexCoord3.X, referenceFrame.TexCoord3.Y, 1));

							var intersection = Vector3.Cross(cross1, cross2);
							var center = new Vector2(intersection.X / intersection.Z, intersection.Y / intersection.Z);

							float r0 = Vector2.Distance(center, referenceFrame.TexCoord0);
							float r1 = Vector2.Distance(center, referenceFrame.TexCoord1);
							float r2 = Vector2.Distance(center, referenceFrame.TexCoord2);
							float r3 = Vector2.Distance(center, referenceFrame.TexCoord3);

							if (float.IsNaN(r1) || float.IsNaN(r2) || float.IsNaN(r3) || float.IsNaN(r0))
								continue;

							targetSet.Frames[i].TexCoord0 = new Vector2(center.X + r0 * (float)Math.Cos(currAngle + Math.PI), center.Y + r0 * (float)Math.Sin(currAngle + Math.PI));
							targetSet.Frames[i].TexCoord1 = new Vector2(center.X + r1 * (float)Math.Cos(currAngle + Math.PI * 1.5f), center.Y + r1 * (float)Math.Sin(currAngle + Math.PI * 1.5f));
							targetSet.Frames[i].TexCoord2 = new Vector2(center.X + r2 * (float)Math.Cos(currAngle), center.Y + r2 * (float)Math.Sin(currAngle));
							targetSet.Frames[i].TexCoord3 = new Vector2(center.X + r3 * (float)Math.Cos(currAngle + Math.PI / 2), center.Y + r3 * (float)Math.Sin(currAngle + Math.PI / 2));
						}
						break;

					case ProceduralAnimationType.HorizontalPan:
					case ProceduralAnimationType.VerticalPan:
						{
							bool horizontal = type == ProceduralAnimationType.HorizontalPan;
							var multiplier = -Math.Sign(effectStrength) * (Math.Abs(weight) - Math.Abs(effectStrength)); // Start from origin

							var dist1 = Vector2.Distance(referenceFrame.TexCoord0, horizontal ? referenceFrame.TexCoord3 : referenceFrame.TexCoord1) * multiplier;
							var dist2 = Vector2.Distance(referenceFrame.TexCoord1, horizontal ? referenceFrame.TexCoord2 : referenceFrame.TexCoord0) * multiplier;

							targetSet.Frames[i].TexCoord0 += horizontal ? new Vector2(dist1, 0) : new Vector2(0, dist1);
							targetSet.Frames[i].TexCoord1 += horizontal ? new Vector2(dist2, 0) : new Vector2(0, dist1);
							targetSet.Frames[i].TexCoord2 += horizontal ? new Vector2(dist1, 0) : new Vector2(0, dist2);
							targetSet.Frames[i].TexCoord3 += horizontal ? new Vector2(dist2, 0) : new Vector2(0, dist2);
						}
						break;

					case ProceduralAnimationType.Shake:
						{
							// Shake strength is constant in case effect is non-symmetric (looped).
							// Negative shake doesn't make sense, so we apply Abs() onto effect strength.
							int rndStrength = (int)((loop ? Math.Abs(effectStrength - weight) : effectStrength) * 16.0f);

							float xRnd = rnd.Next(-rndStrength, rndStrength);
							float yRnd = rnd.Next(-rndStrength, rndStrength);
							Vector2 rndAdd = new Vector2(xRnd, yRnd);

							targetSet.Frames[i].TexCoord0 += rndAdd;
							targetSet.Frames[i].TexCoord1 += rndAdd;
							targetSet.Frames[i].TexCoord2 += rndAdd;
							targetSet.Frames[i].TexCoord3 += rndAdd;
						}
						break;
				}
			}

			// Add new set to level for types which create new animation, otherwise just send change event.
			if (genType < AnimGenerationType.Merge)
			{
				if (_animatedTextureSets.Contains(targetSet))
				{
					popup.ShowInfo(_textureMap, "Animation with same properties already exists.");
					comboAnimatedTextureSets.SelectedItem = targetSet;
					return false;
				}
				else
				{
					_animatedTextureSets.Add(targetSet);
					_context.OnAnimatedTexturesChanged.Invoke();
					comboAnimatedTextureSets.SelectedItem = targetSet;
				}
			}
			else
				_context.OnAnimatedTexturesChanged.Invoke();

			// Point to last generated frame
			int rowIndex = startIndex + resultingFrameCount - 1;
			texturesDataGridView.CurrentCell = texturesDataGridView.Rows[rowIndex].Cells[0];
			texturesDataGridView.FirstDisplayedScrollingRowIndex = rowIndex;

			return true;
		}

		private void butOk_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void texturesDataGridView_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
		{
			if (e.DesiredType == typeof(LevelTexture) || e.DesiredType == typeof(WadTexture))
			{
				e.Value = _context.AvailableTextures.FirstOrDefault(texture => texture.ToString() == (string)e.Value);
				e.ParsingApplied = true;
			}
		}

		private void texturesDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			// Get frame for the currently formatted row
			if (e.RowIndex >= texturesDataGridView.Rows.Count || e.ColumnIndex >= texturesDataGridView.Columns.Count)
				return;
			AnimatedTextureFrame frame;
			try
			{
				frame = (AnimatedTextureFrame)texturesDataGridView.Rows[e.RowIndex].DataBoundItem;
			}
			catch (Exception exc)
			{
				logger.Info(exc, "Cell formatting failed for row " + e.RowIndex + ".");
				return;
			}

			// Do cell formatting
			if (e.DesiredType == typeof(Image) || e.DesiredType.IsSubclassOf(typeof(Image)))
			{
				// Image column
				CachedImageInfo info;
				info._destinationSize = new Size(texturesDataGridView.Columns[e.ColumnIndex].Width - 1, texturesDataGridView.Rows[e.RowIndex].Height - 1);
				info._sourceTexCoord0 = frame.TexCoord0;
				info._sourceTexCoord1 = frame.TexCoord1;
				info._sourceTexCoord2 = frame.TexCoord2;
				info._sourceTexCoord3 = frame.TexCoord3;
				info._image = frame.Texture.Image;

				e.Value = _imageCache[info];
				e.FormattingApplied = true;
			}
			else if (texturesDataGridView.Columns[e.ColumnIndex].Name == texturesDataGridViewColumnArea.Name)
			{
				var area = frame.Area;
				e.Value = "(" + area.X0 + ", " + area.Y1 + ")-> (" + area.X1 + ", " + area.Y1 + ")";
				e.FormattingApplied = true;
			}
		}

		private static ImageC GetPerspectivePreview(ImageC input, Vector2 texCoord01, Vector2 texCoord00, Vector2 texCoord10, Vector2 texCoord11, Size size)
		{
			ImageC output = ImageC.CreateNew(size.Width, size.Height);

			// Project the chosen texture onto the rectangular preview ...
			float xTexCoordFactor = 1.0f / size.Width;
			float yTexCoordFactor = 1.0f / size.Width;
			Vector2 max = input.Size - new Vector2(1.0f);

			for (int y = 0; y < size.Height; ++y)
				for (int x = 0; x < size.Width; ++x)
				{
					float outputTexCoordX = (x + 0.5f) * xTexCoordFactor;
					float outputTexCoordY = (y + 0.5f) * yTexCoordFactor;
					Vector2 inputTexCoord = texCoord00 * ((1.0f - outputTexCoordX) * (1.0f - outputTexCoordY)) +
											texCoord01 * ((1.0f - outputTexCoordX) * outputTexCoordY) + (
							texCoord10 * (outputTexCoordX * (1.0f - outputTexCoordY)) +
							texCoord11 * (outputTexCoordX * outputTexCoordY)
						);

					// Bilinear filter from the input
					inputTexCoord -= new Vector2(0.5f); // Offset of texture coordinate from texel midpoint
					inputTexCoord = Vector2.Min(Vector2.Max(inputTexCoord, new Vector2()), max); // Clamp into available texture space

					int firstX = (int)inputTexCoord.X;
					int firstY = (int)inputTexCoord.Y;
					int secondX = Math.Min(firstX + 1, input.Width - 1);
					int secondY = Math.Min(firstY + 1, input.Height - 1);
					float secondFactorX = inputTexCoord.X - firstX;
					float secondFactorY = inputTexCoord.Y - firstY;

					Vector4 foregroundPixel =
						(Vector4)input.GetPixel(secondX, secondY) * (secondFactorX * secondFactorY) +
						(Vector4)input.GetPixel(secondX, firstY) * (secondFactorX * (1.0f - secondFactorY)) + (
						(Vector4)input.GetPixel(firstX, secondY) * ((1.0f - secondFactorX) * secondFactorY) +
						(Vector4)input.GetPixel(firstX, firstY) * ((1.0f - secondFactorX) * (1.0f - secondFactorY)));
					ColorC backgroundPixel = transparentBackground.GetPixel(x % transparentBackground.Size.X, y % transparentBackground.Size.Y);
					output.SetPixel(x, y, (ColorC)ColorC.Mix(backgroundPixel, foregroundPixel));
				}
			return output;
		}

		private void texturesDataGridView_SelectionChanged(object sender, EventArgs e)
		{
			butUpdate.Enabled = texturesDataGridView.SelectedRows.Count > 0 && texturesDataGridView.Rows.Count > 0;
		}

		private void texturesDataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (texturesDataGridView.SelectedRows.Count > 0)
			{
				var frame = (AnimatedTextureFrame)texturesDataGridView.SelectedRows[0].DataBoundItem;
				var textureToShow = new TextureArea
				{
					Texture = frame.Texture,
					TexCoord0 = frame.TexCoord0,
					TexCoord1 = frame.TexCoord1,
					TexCoord2 = frame.TexCoord2,
					TexCoord3 = frame.TexCoord3
				};

				if (_textureMap.SelectedTexture != textureToShow)
				{
					_textureMap.ShowTexture(textureToShow);
					_context.SelectedTexture = textureToShow;
				}
			}
		}

		private void _textureMap_DoubleClick(object sender, EventArgs e)
		{
			AddFrame();
		}

		private void comboEffect_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_version is TRVersion.Game.TRNG or TRVersion.Game.TombEngine)
				OnEffectChanged();
		}

		private void OnEffectChanged()
		{
			// This case can happen if there is a mismatch between game version
			// and selected animation type.

			if (comboEffect.SelectedItem == null)
			{
				comboFps.Visible = false;
				numericUpDownFPS.Visible = true;
				numericUpDownFPS.Enabled = false;
				comboUvRotate.Enabled = false;
				numericUpDownUvRotateDirection.Visible = false;
				numericUpDownUvRotateDirection.Enabled = false;
				return;
			}

			var effect = (AnimatedTextureAnimationType)comboEffect.SelectedItem;

			lblFps.Text = "FPS";
			lblUvRotate.Text = "UvRotate";

			switch (effect)
			{
				case AnimatedTextureAnimationType.Frames:
					comboFps.Visible = false;
					numericUpDownFPS.Visible = true;
					numericUpDownFPS.Enabled = true;
					comboUvRotate.Enabled = false;
					comboUvRotate.Visible = true;
					numericUpDownUvRotateDirection.Visible = false;
					numericUpDownUvRotateDirection.Enabled = false;
					break;

				case AnimatedTextureAnimationType.PFrames:
					comboFps.Visible = false;
					numericUpDownFPS.Visible = true;
					numericUpDownFPS.Enabled = false;
					comboUvRotate.Enabled = false;
					comboUvRotate.Visible = true;
					numericUpDownUvRotateDirection.Visible = false;
					numericUpDownUvRotateDirection.Enabled = false;
					break;

				case AnimatedTextureAnimationType.UVRotate:
				case AnimatedTextureAnimationType.HalfRotate:
				case AnimatedTextureAnimationType.RiverRotate:
					if (_version == TRVersion.Game.TRNG)
					{
						comboFps.Visible = true;
						numericUpDownFPS.Visible = false;
						comboUvRotate.Enabled = true;

						comboFps.SelectedIndex = 0;
						comboUvRotate.SelectedIndex = 64;
					}
					else
					{
						comboFps.Visible = false;
						numericUpDownFPS.Visible = true;
						numericUpDownFPS.Enabled = true;
						lblFps.Text = "Cycles/s";

						comboUvRotate.Visible = false;
						numericUpDownUvRotateDirection.Visible = true;
						numericUpDownUvRotateDirection.Enabled = true;
						lblUvRotate.Text = "UvRotate (degrees)";
					}

					break;

				case AnimatedTextureAnimationType.Video:
					comboFps.Visible = false;
					numericUpDownFPS.Visible = true;
					numericUpDownFPS.Enabled = false;
					comboUvRotate.Enabled = false;
					comboUvRotate.Visible = true;
					numericUpDownUvRotateDirection.Visible = false;
					numericUpDownUvRotateDirection.Enabled = false;
					break;

				default:
					throw new NotSupportedException("Unsupported texture animation type encountered.");
			}
		}

		private void comboEffect_SelectionChangeCommitted(object sender, EventArgs e)
		{
			var selectedSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
			if (selectedSet == null)
				return;
			selectedSet.AnimationType = (AnimatedTextureAnimationType)comboEffect.SelectedItem;
			_context.OnAnimatedTexturesChanged.Invoke();
		}

		private void comboFps_SelectionChangeCommitted(object sender, EventArgs e)
		{
			var selectedSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
			if (selectedSet == null)
				return;
			if (_version == TRVersion.Game.TRNG)
				selectedSet.Fps = ((NgAnimatedTextureSettingPair)comboFps.SelectedItem).Key;
			else
				selectedSet.Fps = comboFps.SelectedIndex + 1;
			_context.OnAnimatedTexturesChanged.Invoke();
		}

		private void numericUpDownFPS_ValueChanged(object sender, EventArgs e)
		{
			var selectedSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
			if (selectedSet == null)
				return;
			if (selectedSet.AnimationType == AnimatedTextureAnimationType.UVRotate)
				selectedSet.TenUvRotateSpeed = (float)numericUpDownFPS.Value;
			else
				selectedSet.Fps = (float)numericUpDownFPS.Value;
			_context.OnAnimatedTexturesChanged.Invoke();
		}

		private void comboUvRotate_SelectionChangeCommitted(object sender, EventArgs e)
		{
			var selectedSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
			if (selectedSet == null)
				return;
			if (_version == TRVersion.Game.TRNG)
				selectedSet.UvRotate = (sbyte)((NgAnimatedTextureSettingPair)comboUvRotate.SelectedItem).Key;
			else
				selectedSet.UvRotate = (int)comboUvRotate.SelectedItem;
			_context.OnAnimatedTexturesChanged.Invoke();
		}

		private void comboCurrentTexture_SelectedValueChanged(object sender, EventArgs e)
		{
			if (_textureMap.VisibleTexture != comboCurrentTexture.SelectedItem)
				_textureMap.ResetVisibleTexture(comboCurrentTexture.SelectedItem as Texture);
		}

		private void butEditSetName_Click(object sender, EventArgs e)
		{
			var currentSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;

			if (currentSet == null)
				return;

			using (var form = new FormInputBox("Edit set name", "Insert the name of this animation set:", currentSet.Name))
			{
				if (form.ShowDialog(this) == DialogResult.Cancel)
					return;

				currentSet.Name = form.Result;
				_context.OnAnimatedTexturesChanged.Invoke();
			}
		}

		private void butGenerateProcAnim_Click(object sender, EventArgs e)
		{
			GenerateProceduralAnimation((ProceduralAnimationType)comboProcPresets.SelectedIndex, (int)numFrames.Value, (float)numStrength.Value / 100.0f, cbSmooth.Checked, cbLoop.Checked, AnimGenerationType.New);
		}

		private void butCloneProcAnim_Click(object sender, EventArgs e)
		{
			GenerateProceduralAnimation((ProceduralAnimationType)comboProcPresets.SelectedIndex, (int)numFrames.Value, (float)numStrength.Value / 100.0f, cbSmooth.Checked, cbLoop.Checked, AnimGenerationType.Clone);
		}

		private void butMergeProcAnim_Click(object sender, EventArgs e)
		{
			GenerateProceduralAnimation((ProceduralAnimationType)comboProcPresets.SelectedIndex, (int)numFrames.Value, (float)numStrength.Value / 100.0f, cbSmooth.Checked, cbLoop.Checked, AnimGenerationType.Merge);
		}

		private void butReplaceProcAnim_Click(object sender, EventArgs e)
		{
			GenerateProceduralAnimation((ProceduralAnimationType)comboProcPresets.SelectedIndex, (int)numFrames.Value, (float)numStrength.Value / 100.0f, cbSmooth.Checked, cbLoop.Checked, AnimGenerationType.Replace);
		}

		private void butAddProcAnim_Click(object sender, EventArgs e)
		{
			GenerateProceduralAnimation((ProceduralAnimationType)comboProcPresets.SelectedIndex, (int)numFrames.Value, (float)numStrength.Value / 100.0f, cbSmooth.Checked, cbLoop.Checked, AnimGenerationType.AddFrames);
		}

		private void butCancel_Click(object sender, EventArgs e)
		{
			_animatedTextureSets.Clear();
			foreach (var set in _backupSets)
				_animatedTextureSets.Add(set);
			_context.OnAnimatedTexturesChanged.Invoke();
			Close();
		}

		private void texturesDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			try
			{
				var cell = texturesDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
				var name = texturesDataGridView.Columns[e.ColumnIndex].Name;

				if (name == texturesDataGridViewColumnRepeat.Name)
				{
					Int16 parsedValue = 0;
					if (e.FormattedValue == null || !Int16.TryParse(e.FormattedValue.ToString(), out parsedValue))
					{
						if (!Int16.TryParse(cell.Value.ToString(), out parsedValue))
							parsedValue = 0;
					}

					if (parsedValue > Int16.MaxValue)
						cell.Value = Int16.MaxValue;
					else if (parsedValue < 1)
						cell.Value = (Int16)1;
					else
						cell.Value = parsedValue;
				}
			}
			catch (Exception) { }
		}

		private void numericUpDownUvRotateDirection_ValueChanged(object sender, EventArgs e)
		{
			var selectedSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
			if (selectedSet == null)
				return;
			selectedSet.TenUvRotateDirection = (float)numericUpDownUvRotateDirection.Value;
			_context.OnAnimatedTexturesChanged.Invoke();
		}
	}
}
