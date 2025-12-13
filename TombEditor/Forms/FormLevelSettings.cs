using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using DarkUI.Config;
using DarkUI.Forms;
using NLog;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Utils;
using DarkUI.Controls;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using static TombEditor.Editor;
using static TombLib.LevelData.TRVersion;

namespace TombEditor.Forms
{
    public partial class FormLevelSettings : DarkForm
    {
        private const string _catalogsPromptBase = "Sound catalogs (eg *.xml, sounds.txt, *.sfx/*.sam) from which sound infos will be loaded.\n";
        private const string _catalogsPromptMSFX = "Warning: only single catalog may be used with TR2 and TR4 engines! Multiple catalogs may cause unexpected results.";
        private const string _catalogsPromptNew = "If any sound info ID is duplicated in any of catalog, first one will be used.";

        private const string _pathsPromptMSFX = "Sound sample paths are not available for this game version." + "\n" +
            "If you want to recompile MAIN.SFX file, use SoundTool.";
        private const string _pathsPromptNew = "Locations from which sound samples will be loaded." + "\n" +
            "Each required sample will be searched in folders in top to bottom order.If not found, sound is not played.";

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private class PictureTooltip : ToolTip
        {
            private readonly DarkForm parentForm;

            public PictureTooltip(DarkForm parent)
            {
                parentForm = parent;
                OwnerDraw = true;
                Popup += OnPopup;
                Draw += OnDraw;
                InitialDelay = 10;
                AutoPopDelay = short.MaxValue; // Unfortunately the maximum possible value
                ShowAlways = true;
            }

            private void OnPopup(object sender, PopupEventArgs e) // use this event to set the size of the tool tip
            {
                e.ToolTipSize = new Size(256, 256);
            }

            private void OnDraw(object sender, DrawToolTipEventArgs e) // use this to customzie the tool tip
            {
                PictureBox parent = e.AssociatedControl as PictureBox;
                if (parent == null)
                    return;

                // Draw background
                if (parent.BackgroundImage != null)
                    using (Brush backgroundBrush = new TextureBrush(parent.BackgroundImage, WrapMode.Tile))
                        e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
                else
                    using (Brush backgroundBrush = new SolidBrush(Colors.GreyBackground))
                        e.Graphics.FillRectangle(backgroundBrush, e.Bounds);

                // Draw image (if available)
                if (parent.Image != null)
                    e.Graphics.DrawImage(parent.Image, e.Bounds);

                e.Graphics.InterpolationMode = InterpolationMode.Bicubic;

                // Draw error message (if necessary)
                Exception exc = parent.Tag as Exception;
                if (exc != null)
                {
                    Rectangle textArea = e.Bounds;
                    textArea.Inflate(-5, -5);
                    e.Graphics.DrawString(" The image could not be loaded.\n\n Message: " + exc.Message,
                        parentForm.Font, Brushes.DarkGray, textArea,
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                }

                // Draw border
                e.Graphics.DrawRectangle(Pens.Black, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
            }
        }

        private class ReferencedSoundsCatalogWrapper : IReloadableResource
        {
            private readonly FormLevelSettings _parent;
            public ReferencedSoundCatalog Sounds;

            public ReferencedSoundsCatalogWrapper(FormLevelSettings parent, ReferencedSoundCatalog sounds)
            {
                _parent = parent;
                Sounds = sounds;
            }

            public string Path
            {
                get { return Sounds.Path; }
                set
                {
                    Sounds = new ReferencedSoundCatalog(_parent._levelSettings, value);
                    _parent.soundsCatalogsDataGridView.InvalidateRow(_parent._soundsCatalogsDataGridViewDataSource.IndexOf(this));
                }
            }

            public string Message
            {
                get
                {
                    if (Sounds.LoadException == null)
                        return "Successfully loaded";
                    return Sounds.LoadException.Message + " (" + Sounds.LoadException.GetType().Name + ")";
                }
            }

            public int SoundsCount
            {
                get
                {
                    if (Sounds.LoadException == null)
                        return Sounds.Sounds.SoundInfos.Count;
                    return 0;
                }
            }

            public ReloadableResourceType ResourceType => Sounds.ResourceType;
            public Exception LoadException { get { return Sounds.LoadException; } set { Sounds.LoadException = value; } }
            public IEnumerable<FileFormat> FileExtensions => Sounds.FileExtensions;
            public List<IReloadableResource> GetResourceList(LevelSettings settings) => Sounds.GetResourceList(settings);
            public string GetPath() => Sounds.GetPath();
            public void SetPath(LevelSettings settings, string path) => Sounds.SetPath(settings, path);
        }

        private class ReferencedWadWrapper : IReloadableResource
        {
            private readonly FormLevelSettings _parent;
            public ReferencedWad Wad;

            public ReferencedWadWrapper(FormLevelSettings parent, ReferencedWad wad)
            {
                _parent = parent;
                Wad = wad;
            }

            public string Path
            {
                get { return Wad.Path; }
                set
                {
                    Wad = new ReferencedWad(_parent._levelSettings, value);
                    _parent.objectFileDataGridView.InvalidateRow(_parent._objectFileDataGridViewDataSource.IndexOf(this));
                }
            }

            public string Message
            {
                get
                {
                    if (Wad.Wad != null && Wad.Wad.GameVersion.Native() != _parent._levelSettings.GameVersion.Native())
                        return "Game version mismatch";
                    if (Wad.Wad != null && Wad.Wad.HasUnknownData)
                        return "Wad has unknown data";
                    if (Wad.LoadException == null)
                        return "Successfully loaded";
                    return Wad.LoadException.Message + " (" + Wad.LoadException.GetType().Name + ")";
                }
            }

            public ReloadableResourceType ResourceType => Wad.ResourceType;
            public Exception LoadException { get { return Wad.LoadException; } set { Wad.LoadException = value; } }
            public IEnumerable<FileFormat> FileExtensions => Wad.FileExtensions;
            public List<IReloadableResource> GetResourceList(LevelSettings settings) => Wad.GetResourceList(settings);
            public string GetPath() => Wad.GetPath();
            public void SetPath(LevelSettings settings, string path) => Wad.SetPath(settings, path);
        }

        private class ReferencedTextureWrapper : IReloadableResource
        {
            private readonly FormLevelSettings _parent;
            public LevelTexture Texture;

            public ReferencedTextureWrapper(FormLevelSettings parent, LevelTexture texture)
            {
                _parent = parent;
                Texture = texture;
            }

            public string Path
            {
                get { return Texture.Path; }
                set
                {
                    Texture = (LevelTexture)Texture.Clone(); // We don't actually clone the image data array, so it's fine.
                    Texture.SetPath(_parent._levelSettings, value);
                    _parent._texturePreviewCache.RemoveAll(obj => obj.LevelTexture == Texture);
                    _parent.textureFileDataGridView.InvalidateRow(_parent._textureFileDataGridViewDataSource.IndexOf(this));
                }
            }

            public string Message
            {
                get
                {
                    if (Texture.LoadException == null)
                        return "Successfully loaded";
                    return Texture.LoadException.Message + " (" + Texture.LoadException.GetType().Name + ")";
                }
            }

            public string Size
            {
                get
                {
                    if (Texture.LoadException != null || Texture.Image == TombLib.Utils.Texture.UnloadedPlaceholder)
                        return "-";
                    return Texture.Image.Width + " × " + Texture.Image.Height;
                }
            }

            public bool ReplaceMagentaWithTransparency
            {
                get { return Texture.ReplaceMagentaWithTransparency; }
                set
                {
                    Texture = (LevelTexture)Texture.Clone(); // We don't actually clone the image data array, so it's fine.
                    Texture.SetReplaceMagentaWithTransparency(_parent._levelSettings, value);
                    _parent._texturePreviewCache.RemoveAll(obj => obj.LevelTexture == Texture);
                    _parent.textureFileDataGridView.InvalidateRow(_parent._textureFileDataGridViewDataSource.IndexOf(this));
                }
            }

            public bool Convert512PixelsToDoubleRows
            {
                get { return Texture.Convert512PixelsToDoubleRows; }
                set
                {
                    Texture = (LevelTexture)Texture.Clone(); // We don't actually clone the image data array, so it's fine.
                    Texture.SetConvert512PixelsToDoubleRows(_parent._levelSettings, value);
                    _parent._texturePreviewCache.RemoveAll(obj => obj.LevelTexture == Texture);
                    _parent.textureFileDataGridView.InvalidateRow(_parent._textureFileDataGridViewDataSource.IndexOf(this));
                }
            }

            public ReloadableResourceType ResourceType => Texture.ResourceType;
            public Exception LoadException { get { return Texture.LoadException; } set { Texture.LoadException = value; } }
            public IEnumerable<FileFormat> FileExtensions => Texture.FileExtensions;
            public List<IReloadableResource> GetResourceList(LevelSettings settings) => Texture.GetResourceList(settings);
            public string GetPath() => Texture.GetPath();
            public void SetPath(LevelSettings settings, string path) => Texture.SetPath(settings, path);
        }
        private struct TextureCachePreviewKey : IEquatable<TextureCachePreviewKey>
        {
            public Size RequiredSize { get; }
            public LevelTexture LevelTexture { get { LevelTexture result = null; _levelTexture?.TryGetTarget(out result); return result; } }
            private int _hashCode;
            private WeakReference<LevelTexture> _levelTexture; // Use a weak reference to avoid keeping big images alive.
            public TextureCachePreviewKey(Size size, LevelTexture levelTexture)
            {
                RequiredSize = size;
                _levelTexture = new WeakReference<LevelTexture>(levelTexture);
                _hashCode = RequiredSize.GetHashCode() + 866557253 * (levelTexture?.GetHashCode() ?? 0);
            }
            public bool Equals(TextureCachePreviewKey other) => RequiredSize == other.RequiredSize && (LevelTexture == null ? other.LevelTexture == null : LevelTexture.Equals(other.LevelTexture));
            public override bool Equals(object other) => other is TextureCachePreviewKey && Equals((TextureCachePreviewKey)other);
            public override int GetHashCode() => _hashCode;
        }

        private readonly int _scriptPathPanelSize;
        private readonly Color _correctColor;
        private readonly Color _wrongColor;
        private readonly Color _columnMessageCorrectColor;
        private readonly Color _columnMessageWrongColor;
        private readonly Editor _editor;
        private readonly LevelSettings _levelSettings;

        private string fontTextureFilePathPicPreviewCurrentPath;
        private string skyTextureFilePathPicPreviewCurrentPath;
        private string tr5ExtraSpritesFilePathPicPreviewCurrentPath;
        private readonly PictureTooltip _pictureTooltip;
        private readonly BindingList<ReferencedWadWrapper> _objectFileDataGridViewDataSource = new BindingList<ReferencedWadWrapper>();
        private readonly BindingList<ReferencedSoundsCatalogWrapper> _soundsCatalogsDataGridViewDataSource = new BindingList<ReferencedSoundsCatalogWrapper>();
        private readonly BindingList<ReferencedTextureWrapper> _textureFileDataGridViewDataSource = new BindingList<ReferencedTextureWrapper>();
        private readonly BindingList<WadSoundPath> _soundDataGridViewDataSource = new BindingList<WadSoundPath>();
        private readonly BindingList<AutoStaticMeshMergeEntry> _staticMeshMergeGridViewDataSource = new BindingList<AutoStaticMeshMergeEntry>();
        private readonly Cache<TextureCachePreviewKey, Bitmap> _texturePreviewCache;
        private FormPreviewWad _previewWad = null;
        private FormPreviewTexture _previewTexture = null;

        public FormLevelSettings(Editor editor)
        {
            _editor = editor;
            _levelSettings = editor.Level.Settings.Clone();

            InitializeComponent();

            this.SetActualSize();

            // Set window property handlers
            Configuration.ConfigureWindow(this, _editor.Configuration);

            // Remember colors and sizes
            _scriptPathPanelSize = panelScripts.Height;
            _correctColor = gameLevelFilePathTxt.BackColor;
            _wrongColor = _correctColor.MixWith(Color.DarkRed, 0.55);
            _columnMessageCorrectColor = objectFileDataGridView.BackColor.MixWith(Color.LimeGreen, 0.55);
            _columnMessageWrongColor = objectFileDataGridView.BackColor.MixWith(Color.DarkRed, 0.55);

            // Initialize texture file data grid view
            foreach (var texture in _levelSettings.Textures)
                _textureFileDataGridViewDataSource.Add(new ReferencedTextureWrapper(this, texture)); // We don't need to clone because we don't modify the wad, we create new wads
            _textureFileDataGridViewDataSource.ListChanged += delegate
            {
                _levelSettings.Textures.Clear();
                _levelSettings.Textures.AddRange(_textureFileDataGridViewDataSource.Select(o => o.Texture));
            };
            textureFileDataGridView.DataSource = _textureFileDataGridViewDataSource;
            textureFileDataGridViewControls.DataGridView = textureFileDataGridView;
            textureFileDataGridViewControls.CreateNewRow = textureFileDataGridViewCreateNewRow;
            textureFileDataGridViewControls.AllowUserDelete = true;
            textureFileDataGridViewControls.AllowUserMove = true;
            textureFileDataGridViewControls.AllowUserNew = true;
            textureFileDataGridViewControls.Enabled = true;
            _texturePreviewCache = new Cache<TextureCachePreviewKey, Bitmap>(1024, CreateTexturePreview);

            // Initialize object file data grid view
            foreach (var wad in _levelSettings.Wads)
                _objectFileDataGridViewDataSource.Add(new ReferencedWadWrapper(this, wad)); // We don't need to clone because we don't modify the wad, we create new wads
            _objectFileDataGridViewDataSource.ListChanged += delegate
            {
                _levelSettings.Wads.Clear();
                _levelSettings.Wads.AddRange(_objectFileDataGridViewDataSource.Select(o => o.Wad));
                RefreshAutoStaticMeshMergesList();
            };
            objectFileDataGridView.DataSource = _objectFileDataGridViewDataSource;
            objectFileDataGridViewControls.DataGridView = objectFileDataGridView;
            objectFileDataGridViewControls.CreateNewRow = objectFileDataGridViewCreateNewRow;
            objectFileDataGridViewControls.AllowUserDelete = true;
            objectFileDataGridViewControls.AllowUserMove = true;
            objectFileDataGridViewControls.AllowUserNew = true;
            objectFileDataGridViewControls.Enabled = true;

            // Initialize souns catalogs file data grid view
            foreach (var soundRef in _levelSettings.SoundCatalogs)
                _soundsCatalogsDataGridViewDataSource.Add(new ReferencedSoundsCatalogWrapper(this, soundRef)); // We don't need to clone because we don't modify the wad, we create new wads
            _soundsCatalogsDataGridViewDataSource.ListChanged += delegate
            {
                _levelSettings.SoundCatalogs.Clear();
                _levelSettings.SoundCatalogs.AddRange(_soundsCatalogsDataGridViewDataSource.Select(o => o.Sounds));
                PopulateSoundInfoListAndResetFilter();
            };
            soundsCatalogsDataGridView.DataSource = _soundsCatalogsDataGridViewDataSource;
            soundsCatalogsDataGridViewControls.DataGridView = soundsCatalogsDataGridView;
            soundsCatalogsDataGridViewControls.CreateNewRow = soundsCatalogDataGridViewCreateNewRow;
            soundsCatalogsDataGridViewControls.AllowUserDelete = true;
            soundsCatalogsDataGridViewControls.AllowUserMove = true;
            soundsCatalogsDataGridViewControls.AllowUserNew = true;
            soundsCatalogsDataGridViewControls.Enabled = true;

            // Initialize sound path data grid view
            foreach (var soundPath in _levelSettings.WadSoundPaths)
                _soundDataGridViewDataSource.Add(soundPath.Clone());
            _soundDataGridViewDataSource.ListChanged += delegate
                {
                    _levelSettings.WadSoundPaths.Clear();
                    foreach (var soundPath in _soundDataGridViewDataSource)
                        _levelSettings.WadSoundPaths.Add(soundPath.Clone());
                    PopulateSoundInfoList();
                };
            soundDataGridView.DataSource = _soundDataGridViewDataSource;
            soundDataGridViewControls.DataGridView = soundDataGridView;
            soundDataGridViewControls.CreateNewRow = soundDataGridViewCreateNewRow;
            soundDataGridViewControls.AllowUserDelete = true;
            soundDataGridViewControls.AllowUserMove = true;
            soundDataGridViewControls.AllowUserNew = true;
            soundDataGridViewControls.Enabled = true;

            // Initialize picture previews
            skyTextureFilePathPicPreview.BackColor = _wrongColor;
            fontTextureFilePathPicPreview.BackColor = _wrongColor;
            tr5SpritesTextureFilePathPicPreview.BackColor = _wrongColor;

            _pictureTooltip = new PictureTooltip(this);
            _pictureTooltip.SetToolTip(fontTextureFilePathPicPreview, "Font Preview");
            _pictureTooltip.SetToolTip(skyTextureFilePathPicPreview, "Sky Preview");
            _pictureTooltip.SetToolTip(tr5SpritesTextureFilePathPicPreview, "TR5 extra sprites Preview");
            components.Add(_pictureTooltip);

            // Initialize imported geometry manager
            importedGeometryManager.LevelSettings = _levelSettings;

            // Populate variable list
            foreach (VariableType variableType in Enum.GetValues(typeof(VariableType)))
                pathVariablesDataGridView.Rows.Add(LevelSettings.VariableCreate(variableType), "");

            // Populate versions
            comboGameVersion.Items.AddRange(AllVersions.Cast<object>().ToArray());

            // Populate TR5 lists
            comboTr5Weather.Items.AddRange(Enum.GetValues(typeof(Tr5WeatherType)).Cast<object>().ToArray());
            comboLaraType.Items.AddRange(Enum.GetValues(typeof(Tr5LaraType)).Cast<object>().ToArray());

            // Initialize options list
            tabbedContainer.LinkedControl = optionsList;

            // Initialize Static Mesh merge list
            RefreshAutoStaticMeshMergesList();

            // Initialize controls
            UpdateDialog();

            _editor.EditorEventRaised += EditorEventRaised;
        }

        private void EditorEventRaised(IEditorEvent evt)
        {
            if (evt is LoadedSoundsCatalogsChangedEvent)
            {
                soundsCatalogsDataGridView.Invalidate(true);
                PopulateSoundInfoList();
            }
            else if (evt is LoadedTexturesChangedEvent)
                textureFileDataGridView.Invalidate(true);
            else if (evt is LoadedWadsChangedEvent)
                objectFileDataGridView.Invalidate(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _previewWad?.Dispose();
                _previewTexture?.Dispose();
                _texturePreviewCache?.Dispose();
                components?.Dispose();
                _editor.EditorEventRaised -= EditorEventRaised;
            }
            base.Dispose(disposing);
        }

        private void UpdateLevelSettings()
        {
            LevelSettings settings = _levelSettings.Clone();

            settings.Wads.Clear();
            foreach (var reference in _objectFileDataGridViewDataSource)
                settings.Wads.Add(reference.Wad);

            settings.SoundCatalogs.Clear();
            foreach (var reference in _soundsCatalogsDataGridViewDataSource)
                settings.SoundCatalogs.Add(reference.Sounds);

            settings.Textures.Clear();
            foreach (var reference in _textureFileDataGridViewDataSource)
                settings.Textures.Add(reference.Texture);

            settings.AutoStaticMeshMerges.Clear();
            foreach (var entry in _staticMeshMergeGridViewDataSource)
            {
                if (entry.Merge)
                    settings.AutoStaticMeshMerges.Add(entry.Clone()); // HACK: Clone(), otherwise merge entries won't update!!!
            }

            _editor.UpdateLevelSettings(settings);
        }

        private void RefreshAutoStaticMeshMergesList()
        {
            _staticMeshMergeGridViewDataSource.Clear();

            // Initialize Static Mesh merge list
            foreach (var staticMesh in _levelSettings.WadGetAllStatics())
            {
                bool added = false;
                foreach (var entry in _levelSettings.AutoStaticMeshMerges)
                {
                    if (entry.meshId.Equals(staticMesh.Value.Id.TypeId))
                    {
                        _staticMeshMergeGridViewDataSource.Add(entry);
                        added = true;
                    }
                }
                if (!added)
                    _staticMeshMergeGridViewDataSource.Add(new AutoStaticMeshMergeEntry(staticMesh.Value.Id.TypeId, false, false, false, false, _levelSettings));
            }
            staticMeshMergeDataGridView.DataSource = _staticMeshMergeGridViewDataSource;
        }

        private void UpdateDialog()
        {
            levelFilePathTxt.Text = _levelSettings.LevelFilePath;
            gameDirectoryTxt.Text = _levelSettings.GameDirectory;
            gameLevelFilePathTxt.Text = _levelSettings.GameLevelFilePath;
            gameExecutableFilePathTxt.Text = _levelSettings.GameExecutableFilePath;
            GameEnableQuickStartFeatureCheckBox.Checked = _levelSettings.GameEnableQuickStartFeature;
            cbEnableExtraBlendingModes.Checked = _levelSettings.GameEnableExtraBlendingModes ?? false;
            GameEnableExtraReverbPresetsCheckBox.Checked = _levelSettings.GameEnableExtraReverbPresets;
            comboGameVersion.Text = _levelSettings.GameVersion.ToString(); // Must also accept none enum values.
            tbScriptPath.Text = _levelSettings.ScriptDirectory;
            comboTr5Weather.Text = _levelSettings.Tr5WeatherType.ToString(); // Must also accept none enum values.
            comboLaraType.Text = _levelSettings.Tr5LaraType.ToString(); // Must also accept none enum values.
            tbLuaPath.Text = _levelSettings.TenLuaScriptFile;

            fontTextureFilePathOptAuto.Checked = string.IsNullOrEmpty(_levelSettings.FontTextureFilePath);
            fontTextureFilePathOptCustom.Checked = !string.IsNullOrEmpty(_levelSettings.FontTextureFilePath);
            fontTextureFilePathTxt.Enabled = !string.IsNullOrEmpty(_levelSettings.FontTextureFilePath);
            fontTextureFilePathBut.Enabled = !string.IsNullOrEmpty(_levelSettings.FontTextureFilePath);
            if (string.IsNullOrEmpty(_levelSettings.FontTextureFilePath))
                fontTextureFilePathTxt.Text = GetLevelResourcePath("Font.pc");
            else
                fontTextureFilePathTxt.Text = _levelSettings.FontTextureFilePath;

            skyTextureFilePathOptAuto.Checked = string.IsNullOrEmpty(_levelSettings.SkyTextureFilePath);
            skyTextureFilePathOptCustom.Checked = !string.IsNullOrEmpty(_levelSettings.SkyTextureFilePath);
            skyTextureFilePathTxt.Enabled = !string.IsNullOrEmpty(_levelSettings.SkyTextureFilePath);
            skyTextureFilePathBut.Enabled = !string.IsNullOrEmpty(_levelSettings.SkyTextureFilePath);
            if (string.IsNullOrEmpty(_levelSettings.SkyTextureFilePath))
                skyTextureFilePathTxt.Text = GetLevelResourcePath("pcsky.raw");
            else
                skyTextureFilePathTxt.Text = _levelSettings.SkyTextureFilePath;

            tr5SpritesFilePathOptAuto.Checked = string.IsNullOrEmpty(_levelSettings.Tr5ExtraSpritesFilePath);
            tr5SpritesFilePathOptCustom.Checked = !string.IsNullOrEmpty(_levelSettings.Tr5ExtraSpritesFilePath);
            tr5SpritesTextureFilePathTxt.Enabled = !string.IsNullOrEmpty(_levelSettings.Tr5ExtraSpritesFilePath);
            tr5SpritesTextureFilePathBut.Enabled = !string.IsNullOrEmpty(_levelSettings.Tr5ExtraSpritesFilePath);
            if (string.IsNullOrEmpty(_levelSettings.Tr5ExtraSpritesFilePath))
                tr5SpritesTextureFilePathTxt.Text = GetLevelResourcePath("Extra.Tr5.pc");
            else
                tr5SpritesTextureFilePathTxt.Text = _levelSettings.Tr5ExtraSpritesFilePath;

            // Check correctness of the paths
            string levelFilePath = _levelSettings.LevelFilePath;
            string gameDirectory = _levelSettings.MakeAbsolute(_levelSettings.GameDirectory);
            string gameLevelFilePath = _levelSettings.MakeAbsolute(_levelSettings.GameLevelFilePath);
            string gameExecutableFilePath = _levelSettings.MakeAbsolute(_levelSettings.GameExecutableFilePath);
          
            levelFilePathTxt.BackColor = Directory.Exists(PathC.GetDirectoryNameTry(levelFilePath)) ? _correctColor : _wrongColor;
            gameDirectoryTxt.BackColor = Directory.Exists(gameDirectory) ? _correctColor : _wrongColor;
            gameLevelFilePathTxt.BackColor = Directory.Exists(PathC.GetDirectoryNameTry(gameLevelFilePath)) && !string.IsNullOrEmpty(Path.GetExtension(gameLevelFilePath)) ? _correctColor : _wrongColor;
            gameExecutableFilePathTxt.BackColor = File.Exists(gameExecutableFilePath) ? _correctColor : _wrongColor;
           
            pathToolTip.SetToolTip(levelFilePathTxt, levelFilePath);
            pathToolTip.SetToolTip(gameDirectoryTxt, gameDirectory);
            pathToolTip.SetToolTip(gameLevelFilePathTxt, gameLevelFilePath);
            pathToolTip.SetToolTip(gameExecutableFilePathTxt, gameExecutableFilePath);

            // Load previews
            string fontPath = _levelSettings.MakeAbsolute(_levelSettings.FontTextureFilePath) ?? "<default>";
            fontTextureFilePathPicPreviewCurrentPath = pathToolTip.GetToolTip(fontTextureFilePathTxt);

            if (fontTextureFilePathPicPreviewCurrentPath != fontPath)
            {
                pathToolTip.SetToolTip(fontTextureFilePathTxt, fontPath);
                fontTextureFilePathPicPreviewCurrentPath = fontPath;

                try
                {
                    fontTextureFilePathPicPreview.Image?.Dispose();
                    fontTextureFilePathPicPreview.Image = _levelSettings.LoadFontTexture(fontPath == "<default>" ? null : fontPath).ToBitmap();
                    fontTextureFilePathPicPreview.BackgroundImage = TombLib.Properties.Resources.misc_TransparentBackground;
                    fontTextureFilePathPicPreview.Tag = null;
                    fontTextureFilePathTxt.BackColor = _correctColor;
                }
                catch (Exception exc)
                {
                    logger.Info(exc, "Unable to load font texture preview.");
                    fontTextureFilePathPicPreview.Image = null;
                    fontTextureFilePathPicPreview.BackgroundImage = null;
                    fontTextureFilePathPicPreview.Tag = exc;
                    fontTextureFilePathTxt.BackColor = _wrongColor;
                }
            }

            string skyPath = _levelSettings.MakeAbsolute(_levelSettings.SkyTextureFilePath) ?? "<default>";
            skyTextureFilePathPicPreviewCurrentPath = pathToolTip.GetToolTip(skyTextureFilePathTxt);

            if (skyTextureFilePathPicPreviewCurrentPath != skyPath)
            {
                pathToolTip.SetToolTip(skyTextureFilePathTxt, skyPath);
                skyTextureFilePathPicPreviewCurrentPath = skyPath;

                try
                {
                    skyTextureFilePathPicPreview.Image?.Dispose();
                    skyTextureFilePathPicPreview.Image = _levelSettings.LoadSkyTexture(skyPath == "<default>" ? null : skyPath).ToBitmap();
                    skyTextureFilePathPicPreview.BackgroundImage = TombLib.Properties.Resources.misc_TransparentBackground;
                    skyTextureFilePathPicPreview.Tag = null;
                    skyTextureFilePathTxt.BackColor = _correctColor;
                }
                catch (Exception exc)
                {
                    logger.Info(exc, "Unable to load sky texture preview.");
                    skyTextureFilePathPicPreview.Image = null;
                    skyTextureFilePathPicPreview.BackgroundImage = null;
                    skyTextureFilePathPicPreview.Tag = exc;
                    skyTextureFilePathTxt.BackColor = _wrongColor;
                }
            }

            string tr5SpritesPath = _levelSettings.MakeAbsolute(_levelSettings.Tr5ExtraSpritesFilePath) ?? "<default>";
            tr5ExtraSpritesFilePathPicPreviewCurrentPath = pathToolTip.GetToolTip(tr5SpritesTextureFilePathTxt);

            if (tr5ExtraSpritesFilePathPicPreviewCurrentPath != tr5SpritesPath)
            {
                pathToolTip.SetToolTip(tr5SpritesTextureFilePathTxt, tr5SpritesPath);
                tr5ExtraSpritesFilePathPicPreviewCurrentPath = tr5SpritesPath;

                try
                {
                    tr5SpritesTextureFilePathPicPreview.Image?.Dispose();
                    tr5SpritesTextureFilePathPicPreview.Image = _levelSettings.LoadTr5ExtraSprites(tr5SpritesPath == "<default>" ? null : tr5SpritesPath).ToBitmap();
                    tr5SpritesTextureFilePathPicPreview.BackgroundImage = TombLib.Properties.Resources.misc_TransparentBackground;
                    tr5SpritesTextureFilePathPicPreview.Tag = null;
                    tr5SpritesTextureFilePathTxt.BackColor = _correctColor;
                }
                catch (Exception exc)
                {
                    logger.Info(exc, "Unable to load TR5 extra sprites preview.");
                    tr5SpritesTextureFilePathPicPreview.Image = null;
                    tr5SpritesTextureFilePathPicPreview.BackgroundImage = null;
                    tr5SpritesTextureFilePathPicPreview.Tag = exc;
                    tr5SpritesTextureFilePathTxt.BackColor = _wrongColor;
                }
            }

            // Update path variables
            foreach (DataGridViewRow row in pathVariablesDataGridView.Rows)
            {
                string value = _levelSettings.ParseVariables(row.Cells[0].Value.ToString());
                if (row.Cells[1].Value.ToString() != value)
                    row.Cells[1].Value = value;
            }

            // Update light options
            panelRoomAmbientLight.BackColor = (_levelSettings.DefaultAmbientLight * new Vector3(0.5f)).ToWinFormsColor();
            cbOverrideAllLightQuality.Checked = _levelSettings.OverrideIndividualLightQualitySettings;
            cmbDefaultLightQuality.SelectedIndex = _levelSettings.DefaultLightQuality == LightQuality.Default ? 0 : ((int)_levelSettings.DefaultLightQuality) - 1;

            // Update sound autodetection option
            cbAutodetectIfNoneSelected.Checked = _levelSettings.AutoAssignSoundsIfNoSelection;

            // Update compiler options
            numPadding.Value = _levelSettings.TexturePadding;
            cbDither16BitTextures.Checked = _levelSettings.Dither16BitTextures;
            cbCompressTextures.Checked = _levelSettings.CompressTextures;
            cbAgressiveTexturePacking.Checked = _levelSettings.AgressiveTexturePacking;
            cbAgressiveFloordataPacking.Checked = _levelSettings.AgressiveFloordataPacking;
            cbUse32BitLighting.Checked = _levelSettings.Room32BitLighting;
            cbRemapAnimTextures.Checked = _levelSettings.RemapAnimatedTextures;
            cbRearrangeRooms.Checked = _levelSettings.RearrangeVerticalRooms;
			cbRemoveObjects.Checked = _levelSettings.RemoveUnusedObjects;
            cbKeepSampleRate.Checked = _levelSettings.EnableCustomSampleRate;
            cmbSampleRate.SelectedIndex = cmbSampleRate.Items.IndexOf(_levelSettings.CustomSampleRate.ToString());
            cmbSampleRate.Enabled = cbKeepSampleRate.Checked;

            // Lock settings dependent on preview mode
            cbRemapAnimTextures.Enabled = !_levelSettings.FastMode;
            cbDither16BitTextures.Enabled = !_levelSettings.FastMode;

            // Hide version-specific controls
            // TRNG only
            bool currentVersionToCheck = (_levelSettings.GameVersion == Game.TRNG);
            cbUse32BitLighting.Enabled = currentVersionToCheck;
            panelScripts.Height = currentVersionToCheck ? _scriptPathPanelSize : 0;
            if (currentVersionToCheck)
            {
                string scriptPath = _levelSettings.MakeAbsolute(_levelSettings.ScriptDirectory);
                tbScriptPath.BackColor = Directory.Exists(scriptPath) ? _correctColor : _wrongColor;
            }
            if (selectedSoundsDataGridView.Columns.Count >= 7)
            {
                selectedSoundsDataGridView.Columns[5].Visible = currentVersionToCheck;
                selectedSoundsDataGridView.Columns[6].Visible = currentVersionToCheck;
            }

            // TR4 platform
            currentVersionToCheck = (_levelSettings.GameVersion.Native() == Game.TR4);
            GameEnableQuickStartFeatureCheckBox.Visible = currentVersionToCheck;
            GameEnableExtraReverbPresetsCheckBox.Visible = currentVersionToCheck;

            // TR5 platform
            currentVersionToCheck = (_levelSettings.GameVersion == Game.TR5);
            panelTr5LaraType.Visible = currentVersionToCheck;
            panelTr5Weather.Visible = currentVersionToCheck;
            panelTr5Sprites.Visible = currentVersionToCheck;

            // TombEngine
            currentVersionToCheck = (_levelSettings.GameVersion == Game.TombEngine);
            cbRearrangeRooms.Enabled = !currentVersionToCheck;
            cbAgressiveTexturePacking.Enabled = !currentVersionToCheck;
            cbAgressiveFloordataPacking.Enabled = !currentVersionToCheck;
            cbCompressTextures.Enabled = currentVersionToCheck;
            cbRemapAnimTextures.Checked = !currentVersionToCheck;
            cbRemapAnimTextures.Enabled = !currentVersionToCheck;

			panelLuaPath.Height = currentVersionToCheck ? _scriptPathPanelSize : 0;
            if (currentVersionToCheck)
            {
                tbLuaPath.BackColor = File.Exists(_levelSettings.MakeAbsolute(_levelSettings.TenLuaScriptFile)) ? _correctColor : _wrongColor;
            }

            // TR4 and TombEngine platforms
            currentVersionToCheck = (_levelSettings.GameVersion.Native() == Game.TR4);
            cbEnableExtraBlendingModes.Visible = currentVersionToCheck;

            // TR2-5 platforms
            currentVersionToCheck = (_levelSettings.GameVersion.Supports16BitDithering());
            cbDither16BitTextures.Enabled = currentVersionToCheck;

            // TR4 and above
            currentVersionToCheck = (_levelSettings.GameVersion.SupportsFontAndSkySettings());
            panelFont.Enabled = currentVersionToCheck;
            panelSky.Enabled = currentVersionToCheck;

            // MAIN.SFX options
            currentVersionToCheck = (_levelSettings.GameVersion.UsesMainSfx());
            lblCatalogsPrompt.Text = _catalogsPromptBase + (currentVersionToCheck ? _catalogsPromptMSFX : _catalogsPromptNew);
            soundDataGridView.Visible = !currentVersionToCheck;
            soundDataGridViewControls.Visible = !currentVersionToCheck;
            lblPathsPrompt.Text = currentVersionToCheck ? _pathsPromptMSFX : _pathsPromptNew;
            lblPathsPrompt.Dock = currentVersionToCheck ? DockStyle.Fill : DockStyle.Top;
            lblPathsPrompt.TextAlign = currentVersionToCheck ? ContentAlignment.MiddleCenter : ContentAlignment.TopLeft;
            lblPathsPrompt.AutoSize = !currentVersionToCheck;
            lblPathsPrompt.ForeColor = currentVersionToCheck ? Colors.DisabledText : Colors.LightText;
        }

        private void FitPreview(Control form, Rectangle screenArea)
        {
            const int WindowBorderMargin = 5;
            const int RightMargin = 5;
            Point pos = screenArea.Location + new Size(0, screenArea.Height / 2);
            pos -= new Size(form.Width + RightMargin, form.Height / 2);
            Rectangle parentWindowBounds = Bounds;
            pos.Y = Math.Max(pos.Y, parentWindowBounds.Top + WindowBorderMargin);
            pos.Y = Math.Min(pos.Y, parentWindowBounds.Bottom - form.Height - WindowBorderMargin);
            form.Location = pos;
        }

        private string GetLevelResourcePath(string file)
        {
            return LevelSettings.VariableCreate(VariableType.LevelDirectory) + LevelSettings.Dir + file;
        }

        // Level path
        private void levelFilePathTxt_TextChanged(object sender, EventArgs e)
        {
            if (_levelSettings.LevelFilePath == levelFilePathTxt.Text)
                return;
            _levelSettings.LevelFilePath = levelFilePathTxt.Text;
            UpdateDialog();
        }

        private void levelFilePathBut_Click(object sender, EventArgs e)
        {
            string result = LevelFileDialog.BrowseFile(this, _levelSettings, _levelSettings.LevelFilePath,
                "Select the level name", LevelSettings.FileFormatsLevel, null, true);
            if (result != null)
            {
                _levelSettings.LevelFilePath = result;
                UpdateDialog();
            }
        }

        // Script path
        private void tbScriptPath_TextChanged(object sender, EventArgs e)
        {
            if (_levelSettings.ScriptDirectory == tbScriptPath.Text)
                return;
            _levelSettings.ScriptDirectory = tbScriptPath.Text;
            UpdateDialog();
        }

        private void scriptPathBut_Click(object sender, EventArgs e)
        {
            string result = LevelFileDialog.BrowseFolder(this, _levelSettings, _levelSettings.ScriptDirectory,
                "Select the script TXT files folder", VariableType.LevelDirectory);
            if (result != null)
            {
                _levelSettings.ScriptDirectory = result;
                UpdateDialog();
            }
        }

        // Font Texture
        private void fontTextureFilePathOptAuto_CheckedChanged(object sender, EventArgs e)
        {
            if (fontTextureFilePathOptAuto.Checked == string.IsNullOrEmpty(_levelSettings.FontTextureFilePath))
                return;
            _levelSettings.FontTextureFilePath = fontTextureFilePathOptAuto.Checked ? null : GetLevelResourcePath("Font.pc");
            UpdateDialog();
        }

        private void fontTextureFilePathTxt_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_levelSettings.FontTextureFilePath) || _levelSettings.FontTextureFilePath == fontTextureFilePathTxt.Text)
                return;
            if (string.IsNullOrEmpty(fontTextureFilePathTxt.Text)) // Don't set if it would be empty otherwise.
                return;
            _levelSettings.FontTextureFilePath = fontTextureFilePathTxt.Text;
            UpdateDialog();
        }

        private void fontTextureFilePathBut_Click(object sender, EventArgs e)
        {
            string result = LevelFileDialog.BrowseFile(this, _levelSettings, _levelSettings.FontTextureFilePath,
                "Select a font texture", LevelSettings.FileFormatsLoadRawExtraTexture, VariableType.LevelDirectory, false);
            if (result != null)
            {
                _levelSettings.FontTextureFilePath = result;
                UpdateDialog();
            }
        }

        // Sky texture
        private void skyTextureFilePathOptAuto_CheckedChanged(object sender, EventArgs e)
        {
            if (skyTextureFilePathOptAuto.Checked == string.IsNullOrEmpty(_levelSettings.SkyTextureFilePath))
                return;
            _levelSettings.SkyTextureFilePath = skyTextureFilePathOptAuto.Checked ? null : GetLevelResourcePath("pcsky.raw");
            UpdateDialog();
        }

        private void skyTextureFilePathTxt_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_levelSettings.SkyTextureFilePath) || _levelSettings.SkyTextureFilePath == skyTextureFilePathTxt.Text)
                return;
            if (string.IsNullOrEmpty(skyTextureFilePathTxt.Text)) // Don't set if it would be empty otherwise.
                return;
            _levelSettings.SkyTextureFilePath = skyTextureFilePathTxt.Text;
            UpdateDialog();
        }

        private void skyTextureFilePathBut_Click(object sender, EventArgs e)
        {
            string result = LevelFileDialog.BrowseFile(this, _levelSettings, _levelSettings.SkyTextureFilePath,
                "Select a sky texture", LevelSettings.FileFormatsLoadRawExtraTexture, VariableType.LevelDirectory, false);
            if (result != null)
            {
                _levelSettings.SkyTextureFilePath = result;
                UpdateDialog();
            }
        }

        // Game directory
        private void gameDirectoryTxt_TextChanged(object sender, EventArgs e)
        {
            if (_levelSettings.GameDirectory == gameDirectoryTxt.Text)
                return;
            _levelSettings.GameDirectory = gameDirectoryTxt.Text;
            UpdateDialog();
        }

        private void GameDirectoryBut_Click(object sender, EventArgs e)
        {
            string result = LevelFileDialog.BrowseFolder(this, _levelSettings, _levelSettings.GameDirectory,
                "Select the game folder (should contain should contain game .exe file...)", VariableType.LevelDirectory);
            if (result != null)
            {
                _levelSettings.GameDirectory = result;
                UpdateDialog();
            }
        }

        // Sound list
        private WadSoundPath soundDataGridViewCreateNewRow()
        {
            string result = LevelFileDialog.BrowseFolder(this, _levelSettings, _levelSettings.LevelFilePath,
                "Select a new sound folder (should contain *.wav audio files)", VariableType.LevelDirectory);
            if (result != null)
                return new WadSoundPath(result);
            return null;
        }

        private void soundDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _soundDataGridViewDataSource.Count)
                return;

            if (soundDataGridView.Columns[e.ColumnIndex].Name == soundDataGridViewColumnPath.Name)
            {
                WadSoundPath path = _soundDataGridViewDataSource[e.RowIndex];
                string parsedPath = _levelSettings.ParseVariables(path.Path);
                string absolutePath = _levelSettings.MakeAbsolute(path.Path);
                if (Path.IsPathRooted(parsedPath) && !Directory.Exists(absolutePath))
                {
                    e.CellStyle.BackColor = _wrongColor;
                    e.CellStyle.SelectionBackColor = e.CellStyle.SelectionBackColor.MixWith(_wrongColor, 0.4);
                }
                soundDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = absolutePath;
            }
        }

        private void soundDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _soundDataGridViewDataSource.Count)
                return;

            if (soundDataGridView.Columns[e.ColumnIndex].Name == soundDataGridViewColumnSearch.Name)
            {
                string result = LevelFileDialog.BrowseFolder(this, _levelSettings, _soundDataGridViewDataSource[e.RowIndex].Path,
                    "Select the sound folder (should contain *.wav audio files)", VariableType.LevelDirectory);
                if (result != null)
                    _soundDataGridViewDataSource[e.RowIndex] = new WadSoundPath(result);
            }
        }

        // Merged static meshes list
        private void ToggleSelectionForStatics(bool toggleValue, bool bypassStatics = true)
        {
            foreach (DataGridViewRow row in staticMeshMergeDataGridView.Rows)
            {
                if (toggleValue && bypassStatics &&
                    TrCatalog.IsStaticShatterable(_levelSettings.GameVersion, ((AutoStaticMeshMergeEntry)row.DataBoundItem).meshId))
                {
                    row.Cells[1].Value = false;
                    continue;
                }

                row.Cells[1].Value = toggleValue;
                if (toggleValue == false) row.Cells[2].Value = false;
            }
        }

        // Texture list
        private Bitmap CreateTexturePreview(TextureCachePreviewKey data)
        {
            // Create image
            Bitmap result = new Bitmap(data.RequiredSize.Width, data.RequiredSize.Height);
            try
            {
                using (Graphics g = Graphics.FromImage(result))
                {
                    using (TextureBrush brush = new TextureBrush(TombLib.Properties.Resources.misc_TransparentBackground))
                        g.FillRectangle(brush, new Rectangle(new Point(), result.Size));
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    if (data.LevelTexture != null)
                        data.LevelTexture.Image.GetTempSystemDrawingBitmap(temp => g.DrawImage(temp,
                            new Rectangle(new Point(), result.Size),
                            new Rectangle(new Point(), temp.Size), GraphicsUnit.Pixel));
                }
                return result;
            }
            catch (Exception)
            {
                result.Dispose();
                throw;
            }
        }

        private IEnumerable<ReferencedTextureWrapper> textureFileDataGridViewCreateNewRow()
        {
            List<string> paths = LevelFileDialog.BrowseFiles(this, _levelSettings, _levelSettings.LevelFilePath,
                "Select new texture files", ImageC.FileExtensions, VariableType.LevelDirectory).ToList();

            // Load textures concurrently
            ReferencedTextureWrapper[] results = new ReferencedTextureWrapper[paths.Count];
            Parallel.For(0, paths.Count, i => results[i] = new ReferencedTextureWrapper(this, new LevelTexture(_levelSettings, paths[i])));
            return results;
        }

        private void textureFileDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _textureFileDataGridViewDataSource.Count)
                return;
            LevelTexture texture = _textureFileDataGridViewDataSource[e.RowIndex].Texture;

            if (textureFileDataGridView.Columns[e.ColumnIndex].Name == textureFileDataGridViewMessageColumn.Name)
            {
                if (texture.LoadException == null)
                {
                    e.CellStyle.BackColor = _columnMessageCorrectColor;
                    e.CellStyle.SelectionBackColor = e.CellStyle.SelectionBackColor.MixWith(_columnMessageCorrectColor, 0.4);
                }
                else
                {
                    e.CellStyle.BackColor = _columnMessageWrongColor;
                    e.CellStyle.SelectionBackColor = e.CellStyle.SelectionBackColor.MixWith(_columnMessageWrongColor, 0.4);
                }
            }
            else if (textureFileDataGridView.Columns[e.ColumnIndex].Name == textureFileDataGridViewPathColumn.Name)
            {
                string absolutePath = _levelSettings.MakeAbsolute(texture.Path);
                textureFileDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = absolutePath;
            }
            else if (textureFileDataGridView.Columns[e.ColumnIndex].Name == textureFileDataGridViewPreviewColumn.Name)
            {
                var cell = (DataGridViewImageCell)(textureFileDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex]);
                Size availableSpace = new Size(textureFileDataGridView.Columns[e.ColumnIndex].Width - 1, textureFileDataGridView.Rows[e.RowIndex].Height - 1);

                // Figure out required size of preview image
                float neededAspectRatio = (float)availableSpace.Width / availableSpace.Height;
                float givenAspectRatio = (float)texture.Image.Width / texture.Image.Height;
                float aspectRatioAdjust = neededAspectRatio / givenAspectRatio;
                Vector2 factor = Vector2.Min(new Vector2(1.0f / aspectRatioAdjust, aspectRatioAdjust), new Vector2(1.0f));
                Size previewImageSize = new Size((int)Math.Ceiling(availableSpace.Width * factor.X), (int)Math.Ceiling(availableSpace.Height * factor.Y));

                // Request and asign image
                e.Value = _texturePreviewCache[new TextureCachePreviewKey(previewImageSize, texture)];
            }
            else if (textureFileDataGridView.Columns[e.ColumnIndex].Name == textureFileDataGridViewShowColumn.Name)
            {
                var cell = textureFileDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                ((DarkDataGridViewButtonCell)cell).Enabled = texture.LoadException == null;
            }
        }

        private void textureFileDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _textureFileDataGridViewDataSource.Count)
                return;
            LevelTexture texture = _textureFileDataGridViewDataSource[e.RowIndex].Texture;

            if (textureFileDataGridView.Columns[e.ColumnIndex].Name == textureFileDataGridViewSearchColumn.Name)
            {
                EditorActions.ReloadResource(this, _levelSettings, _textureFileDataGridViewDataSource[e.RowIndex]);
            }
            else if (textureFileDataGridView.Columns[e.ColumnIndex].Name == textureFileDataGridViewShowColumn.Name)
            {
                if (texture.LoadException != null)
                    return;

                // Open preview
                _previewTexture?.Dispose();
                _previewTexture = new FormPreviewTexture(texture);
                var screenArea = textureFileDataGridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                FitPreview(_previewTexture, new Rectangle(textureFileDataGridView.PointToScreen(screenArea.Location), screenArea.Size));
                _previewTexture.Show(this);
            }
        }

        // Object list
        private IEnumerable<ReferencedWadWrapper> objectFileDataGridViewCreateNewRow()
        {
            List<string> paths = LevelFileDialog.BrowseFiles(this, _levelSettings, _levelSettings.LevelFilePath,
                "Select new object files", Wad2.FileExtensions, VariableType.LevelDirectory).ToList();

            // Load objects concurrently
            ReferencedWadWrapper[] results = new ReferencedWadWrapper[paths.Count];
            var synchronizedDialog = new GraphicalDialogHandler(this);
            using (var loadingTask = Task.Run(() =>
                Parallel.For(0, paths.Count, i => results[i] = new ReferencedWadWrapper(this, new ReferencedWad(_levelSettings, paths[i], synchronizedDialog)))))
                while (!loadingTask.IsCompleted)
                {
                    Thread.Sleep(1);
                    Application.DoEvents(); // Keep dialog handler responsive, otherwise wad loading can deadlock waiting on GUI thread, while GUI thread is waiting for Parallel.For.
                }

            synchronizedDialog = new GraphicalDialogHandler(this);
            ReferencedSoundCatalog[] soundsResults = new ReferencedSoundCatalog[paths.Count];
            using (var loadingTask = Task.Run(() =>
                Parallel.For(0, paths.Count, i =>
                {
                    string currentPath = paths[i].ToLower();
                    string extension = Path.GetExtension(currentPath);
                    if (extension == ".wad")
                    {
                        string sfxPath = Path.GetDirectoryName(currentPath) + "\\" + Path.GetFileNameWithoutExtension(currentPath) + ".sfx";
                        if (File.Exists(sfxPath))
                        {
                            soundsResults[i] = new ReferencedSoundCatalog(_editor.Level.Settings, sfxPath, synchronizedDialog);
                        }
                    }
                    else if (extension == ".wad2")
                    {
                        string xmlPath = Path.GetDirectoryName(currentPath) + "\\" + Path.GetFileNameWithoutExtension(currentPath) + ".xml";
                        if (File.Exists(xmlPath))
                        {
                            soundsResults[i] = new ReferencedSoundCatalog(_editor.Level.Settings, xmlPath, synchronizedDialog);
                        }
                    }
                })))
                while (!loadingTask.IsCompleted)
                {
                    Thread.Sleep(1);
                    Application.DoEvents(); // Keep dialog handler responsive, otherwise wad loading can deadlock waiting on GUI thread, while GUI thread is waiting for Parallel.For.
                }

            foreach (var soundRef in soundsResults)
                if (soundRef != null)
                {
                    _soundsCatalogsDataGridViewDataSource.Add(new ReferencedSoundsCatalogWrapper(this, soundRef));
                    AssignAllSounds(soundRef.Sounds);
                }

            // Autoswitch game version if necessary
            if (results.Count() > 0 &&
                EditorActions.AutoswitchGameVersion(_levelSettings, results.Select(w => w.Wad.Wad)))
                UpdateDialog();

            return results;
        }

        private IEnumerable<ReferencedSoundsCatalogWrapper> soundsCatalogDataGridViewCreateNewRow()
        {
            List<string> paths = LevelFileDialog.BrowseFiles(this, _levelSettings, _levelSettings.LevelFilePath,
                "Select new sound catalogs", WadSounds.FileExtensions, VariableType.LevelDirectory)
                // Filter out already loaded catalogs
                .Where(path => !_levelSettings.SoundCatalogs.Any(item => item.Path == path)).ToList();

            // Load catalogs concurrently
            ReferencedSoundsCatalogWrapper[] results = new ReferencedSoundsCatalogWrapper[paths.Count];
            var synchronizedDialog = new GraphicalDialogHandler(this);
            using (var loadingTask = Task.Run(() =>
                Parallel.For(0, paths.Count, i => results[i] = new ReferencedSoundsCatalogWrapper(this, new ReferencedSoundCatalog(_levelSettings, paths[i], synchronizedDialog)))))
                while (!loadingTask.IsCompleted)
                {
                    Thread.Sleep(1);
                    Application.DoEvents(); // Keep dialog handler responsive, otherwise wad loading can deadlock waiting on GUI thread, while GUI thread is waiting for Parallel.For.
                }

            return results;
        }

        private void objectFileDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _objectFileDataGridViewDataSource.Count)
                return;
            ReferencedWad wad = _objectFileDataGridViewDataSource[e.RowIndex].Wad;

            if (objectFileDataGridView.Columns[e.ColumnIndex].Name == objectFileDataGridViewMessageColumn.Name)
            {
                if (wad.LoadException == null &&
                    wad.Wad.HasUnknownData == false &&
                    wad.Wad.GameVersion.Native() == _levelSettings.GameVersion.Native())
                {
                    e.CellStyle.BackColor = _columnMessageCorrectColor;
                    e.CellStyle.SelectionBackColor = e.CellStyle.SelectionBackColor.MixWith(_columnMessageCorrectColor, 0.4);
                }
                else
                {
                    e.CellStyle.BackColor = _columnMessageWrongColor;
                    e.CellStyle.SelectionBackColor = e.CellStyle.SelectionBackColor.MixWith(_columnMessageWrongColor, 0.4);
                }
            }
            else if (objectFileDataGridView.Columns[e.ColumnIndex].Name == objectFileDataGridViewPathColumn.Name)
            {
                string absolutePath = _levelSettings.MakeAbsolute(wad.Path);
                objectFileDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = absolutePath;
            }
            else if (objectFileDataGridView.Columns[e.ColumnIndex].Name == objectFileDataGridViewShowColumn.Name)
            {
                var cell = objectFileDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                ((DarkDataGridViewButtonCell)cell).Enabled = wad.LoadException == null;
            }
        }

        private void objectFileDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _objectFileDataGridViewDataSource.Count)
                return;
            ReferencedWad wad = _objectFileDataGridViewDataSource[e.RowIndex].Wad;

            if (objectFileDataGridView.Columns[e.ColumnIndex].Name == objectFileDataGridViewSearchColumn.Name)
            {
                EditorActions.ReloadResource(this, _levelSettings, _objectFileDataGridViewDataSource[e.RowIndex]);
            }
            else if (objectFileDataGridView.Columns[e.ColumnIndex].Name == objectFileDataGridViewShowColumn.Name)
            {
                if (wad.LoadException != null)
                    return;

                // Open preview
                _previewWad?.Dispose();
                _previewWad = new FormPreviewWad(wad.Wad, TombLib.Graphics.DeviceManager.DefaultDeviceManager.Device, _editor);
                var screenArea = objectFileDataGridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                FitPreview(_previewWad, new Rectangle(objectFileDataGridView.PointToScreen(screenArea.Location), screenArea.Size));
                _previewWad.Show(this);
            }
        }

        // Game version
        private void comboGameVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            var gameVersion = (TRVersion.Game)Enum.Parse(typeof(TRVersion.Game), comboGameVersion.Text);
            if (_levelSettings.GameVersion == gameVersion)
                return;
            _levelSettings.GameVersion = gameVersion; // Must also check none enum values
            _levelSettings.ConvertLevelExtension();
            UpdateDialog();
        }


        // Game level directory
        private void gameLevelFilePathTxt_TextChanged(object sender, EventArgs e)
        {
            if (_levelSettings.GameLevelFilePath == gameLevelFilePathTxt.Text)
                return;
            _levelSettings.GameLevelFilePath = gameLevelFilePathTxt.Text;
            UpdateDialog();
        }

        private void gameLevelFilePathBut_Click(object sender, EventArgs e)
        {
            string result = LevelFileDialog.BrowseFile(this, _levelSettings, _levelSettings.GameLevelFilePath,
                "Select place for compiled level", LevelSettings.FileFormatsLevelCompiled, VariableType.GameDirectory, true);
            if (result != null)
            {
                _levelSettings.GameLevelFilePath = result;
                UpdateDialog();
            }
        }

        // Game executable
        private void gameExecutableFilePathTxt_TextChanged(object sender, EventArgs e)
        {
            if (_levelSettings.GameExecutableFilePath == gameExecutableFilePathTxt.Text)
                return;
            _levelSettings.GameExecutableFilePath = gameExecutableFilePathTxt.Text;
            UpdateDialog();
        }

        private void gameExecutableFilePathBut_Click(object sender, EventArgs e)
        {
            string result = LevelFileDialog.BrowseFile(this, _levelSettings, _levelSettings.GameExecutableFilePath,
                "Select an executable", new[] { new FileFormat("Windows executables", "exe") }, VariableType.GameDirectory, false);
            if (result != null)
            {
                _levelSettings.GameExecutableFilePath = result;
                UpdateDialog();
            }
        }

        private void GameEnableQuickStartFeatureCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _levelSettings.GameEnableQuickStartFeature = GameEnableQuickStartFeatureCheckBox.Checked;
            UpdateDialog();
        }

        private void cbEnableExtraBlendingModes_CheckedChanged(object sender, EventArgs e)
        {
            _levelSettings.GameEnableExtraBlendingModes = cbEnableExtraBlendingModes.Checked;
            UpdateDialog();
        }

        private void GameEnableExtraReverbPresetsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _levelSettings.GameEnableExtraReverbPresets = GameEnableExtraReverbPresetsCheckBox.Checked;
            UpdateDialog();
        }

        // Path variable list
        private void pathVariablesDataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                pathVariablesDataGridView.ClearSelection();
                pathVariablesDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected = true;
            }
        }

        private void pathVariablesDataGridViewContextMenuCopy_Click(object sender, EventArgs e)
        {
            string text = pathVariablesDataGridView.SelectedCells.Count == 0 ? "" :
                pathVariablesDataGridView.SelectedCells[0].Value.ToString();
            Clipboard.SetText(text);
        }

        // Dialog buttons
        private void butApply_Click(object sender, EventArgs e) => UpdateLevelSettings();

        private void butOk_Click(object sender, EventArgs e)
        {
            UpdateLevelSettings();
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void panelRoomAmbientLight_Click(object sender, EventArgs e)
        {
            colorDialog.Color = (_levelSettings.DefaultAmbientLight * 0.5f).ToWinFormsColor();
            if (colorDialog.ShowDialog(this) != DialogResult.OK)
                return;

            _levelSettings.DefaultAmbientLight = colorDialog.Color.ToFloat3Color() * 2.0f;
            UpdateDialog();
        }

        private void tr5SpritesFilePathOptAuto_CheckedChanged(object sender, EventArgs e)
        {
            if (tr5SpritesFilePathOptAuto.Checked == string.IsNullOrEmpty(_levelSettings.Tr5ExtraSpritesFilePath))
                return;
            _levelSettings.Tr5ExtraSpritesFilePath = tr5SpritesFilePathOptAuto.Checked ? null : GetLevelResourcePath("Extra.Tr5.pc");
            UpdateDialog();
        }

        private void tr5SpritesTextureFilePathTxt_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_levelSettings.Tr5ExtraSpritesFilePath) || _levelSettings.Tr5ExtraSpritesFilePath == tr5SpritesTextureFilePathTxt.Text)
                return;
            if (string.IsNullOrEmpty(tr5SpritesTextureFilePathTxt.Text)) // Don't set if it would be empty otherwise.
                return;
            _levelSettings.Tr5ExtraSpritesFilePath = tr5SpritesTextureFilePathTxt.Text;
            UpdateDialog();
        }

        private void tr5SpritesTextureFilePathBut_Click(object sender, EventArgs e)
        {
            string result = LevelFileDialog.BrowseFile(this, _levelSettings, _levelSettings.Tr5ExtraSpritesFilePath,
                "Select a font texture", LevelSettings.FileFormatsLoadRawExtraTexture, VariableType.LevelDirectory, false);
            if (result != null)
            {
                _levelSettings.Tr5ExtraSpritesFilePath = result;
                UpdateDialog();
            }
        }

        private void comboLaraType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var laraType = (Tr5LaraType)Enum.Parse(typeof(Tr5LaraType), comboLaraType.Text);
            if (_levelSettings.Tr5LaraType == laraType)
                return;
            _levelSettings.Tr5LaraType = laraType; // Must also check none enum values
            UpdateDialog();
        }

        private void comboTr5Weather_SelectedIndexChanged(object sender, EventArgs e)
        {
            var weather = (Tr5WeatherType)Enum.Parse(typeof(Tr5WeatherType), comboTr5Weather.Text);
            if (_levelSettings.Tr5WeatherType == weather)
                return;
            _levelSettings.Tr5WeatherType = weather; // Must also check none enum values
            UpdateDialog();
        }

        private void numPadding_ValueChanged(object sender, EventArgs e)
        {
            _levelSettings.TexturePadding = (int)numPadding.Value;
            UpdateDialog();
        }
        private void cbDither16BitTextures_CheckedChanged(object sender, EventArgs e)
        {
            _levelSettings.Dither16BitTextures = cbDither16BitTextures.Checked;
            UpdateDialog();
        }

        private void cbUse32BitLighting_CheckedChanged(object sender, EventArgs e)
        {
            _levelSettings.Room32BitLighting = cbUse32BitLighting.Checked;
            UpdateDialog();
        }

        private void cbAgressiveTexturePacking_CheckedChanged(object sender, EventArgs e)
        {
            _levelSettings.AgressiveTexturePacking = cbAgressiveTexturePacking.Checked;
            UpdateDialog();
        }

        private void cbAgressiveFloordataPacking_CheckedChanged(object sender, EventArgs e)
        {
            _levelSettings.AgressiveFloordataPacking = cbAgressiveFloordataPacking.Checked;
            UpdateDialog();
        }

        private void cbRemapAnimTextures_CheckedChanged(object sender, EventArgs e)
        {
            _levelSettings.RemapAnimatedTextures = cbRemapAnimTextures.Checked;
            UpdateDialog();
        }

        private void cbRearrangeRooms_CheckedChanged(object sender, EventArgs e)
        {
            _levelSettings.RearrangeVerticalRooms = cbRearrangeRooms.Checked;
            UpdateDialog();
        }

        private void cbAutodetectIfNoneSelected_CheckedChanged(object sender, EventArgs e)
        {
            _levelSettings.AutoAssignSoundsIfNoSelection = cbAutodetectIfNoneSelected.Checked;
            UpdateDialog();
        }

        private void cbOverrideAllLightQuality_CheckedChanged(object sender, EventArgs e)
        {
            _levelSettings.OverrideIndividualLightQualitySettings = cbOverrideAllLightQuality.Checked;
            UpdateDialog();
        }

        private void cmbDefaultLightQuality_SelectedIndexChanged(object sender, EventArgs e)
        {
            _levelSettings.DefaultLightQuality = (LightQuality)(cmbDefaultLightQuality.SelectedIndex + 1);
            UpdateDialog();
        }

        // Re-populates list of sounds, taking filtering into consideration.
        private void PopulateSoundInfoList()
        {
            var filter = tbFilterSounds.Text;
            selectedSoundsDataGridView.Rows.Clear();

            var paths = _levelSettings.GetRecursiveListOfSoundPaths();

            foreach (var soundInfo in _levelSettings.GlobalSoundMap)
            {
                if (!string.IsNullOrEmpty(filter) && soundInfo.Name.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) < 0)
                    continue;

                int originalId;
                string areaDesc = GetNGDescriptionAndOriginalID(soundInfo.Id, out originalId);
                var sampleCount = soundInfo.SampleCount(_levelSettings, paths);
                string samplePrompt = sampleCount == 0 ? "[ missing ]" : (sampleCount == -1 ? "[ none ]" : sampleCount.ToString());

                selectedSoundsDataGridView.Rows.Add(_levelSettings.SelectedSounds.Contains(soundInfo.Id), soundInfo.Id,
                    soundInfo.Name, soundInfo.SoundCatalog, samplePrompt, areaDesc, originalId);
                selectedSoundsDataGridView_HighlightRow(selectedSoundsDataGridView.Rows[selectedSoundsDataGridView.Rows.Count - 1], sampleCount == 0);
            }

            var missingList = _levelSettings.SelectedAndMissingSounds;
            butRemoveMissing.Enabled = (missingList.Count > 0);

            for (int i = 0; i < missingList.Count; i++)
            {
                var originalName = TrCatalog.GetOriginalSoundName(_editor.Level.Settings.GameVersion, (uint)missingList[i]);

                if (!string.IsNullOrEmpty(filter) && originalName.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) < 0)
                    continue;

                int originalId;
                string areaDesc = GetNGDescriptionAndOriginalID(missingList[i], out originalId);

                selectedSoundsDataGridView.Rows.Add(true, missingList[i], originalName, "[ Not present in any of loaded catalogs ]", "", areaDesc, originalId);
                selectedSoundsDataGridView_HighlightRow(selectedSoundsDataGridView.Rows[selectedSoundsDataGridView.Rows.Count - 1], true);
            }

            selectedSoundsDataGridView.Sort(selectedSoundsDataGridView.Columns[1], ListSortDirection.Ascending);
            UpdateSoundStatistics();
        }

        private string GetNGDescriptionAndOriginalID(int id, out int originalId)
        {
            string result = "";
            originalId = id;

            if (_levelSettings.GameVersion == Game.TRNG)
            {
                if (id < 370)
                {
                    result = "TR4";
                    originalId = id;
                }
                else if (id >= 370 && id < 500)
                {
                    result = "NGReserved";
                    originalId = id - 370;
                }
                else if (id >= 500 && id < 525)
                {
                    result = "CustEnv";
                    originalId = id - 500;
                }
                else if (id >= 525 && id < 602)
                {
                    result = "CustAnims";
                    originalId = id - 525;
                }
                else if (id >= 602 && id < 858)
                {
                    result = "TR1";
                    originalId = id - 602;
                }
                else if (id >= 858 && id < 1228)
                {
                    result = "TR2";
                    originalId = id - 858;
                }
                else if (id >= 1228 && id < 1598)
                {
                    result = "TR3";
                    originalId = id - 1228;
                }
                else if (id >= 1598)
                {
                    result = "TR5";
                    originalId = id - 1598;
                }
            }

            return result;
        }
            

        // Same as above, plus visually reset filter textbox.
        private void PopulateSoundInfoListAndResetFilter()
        {
            tbFilterSounds.Text = string.Empty;
            PopulateSoundInfoList();
        }

        // Select all sounds from desired sound info list.
        private void AssignAllSounds(WadSounds sounds, bool updateUI = true)
        {
            if (sounds == null || sounds.SoundInfos.Count == 0)
                return;

            foreach (var sound in sounds.SoundInfos)
                if (!_levelSettings.SelectedSounds.Contains(sound.Id))
                    _levelSettings.SelectedSounds.Add(sound.Id);

            if (updateUI)
                PopulateSoundInfoListAndResetFilter();
        }

        // Selects or deselects all sounds in UI.
        private void ToggleSelectionForAllSounds(bool toggleValue)
        {
            if (toggleValue == true)
                foreach (var catalog in _levelSettings.SoundCatalogs)
                    AssignAllSounds(catalog.Sounds, false);
            else
                _levelSettings.SelectedSounds.Clear();

            PopulateSoundInfoListAndResetFilter();
        }

        // Updates statistics in the bottom of the page.
        private void UpdateSoundStatistics()
        {
            var missingSoundsCount = _levelSettings.SelectedAndMissingSounds.Count;
            labelSoundsCatalogsStatistics.Text = "Total sounds: " + _levelSettings.GlobalSoundMap.Count + 
                                                 " | Selected sounds: " + _levelSettings.SelectedSounds.Count +
                                                 (missingSoundsCount == 0 ? "" : " | Missing sounds: " + missingSoundsCount);
        }

        private void selectedSoundsDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (selectedSoundsDataGridView.Columns[e.ColumnIndex].Name == colSoundsEnabled.Name)
            {
                var row = selectedSoundsDataGridView.Rows[e.RowIndex];

                selectedSoundsDataGridView_HighlightRow(row);
                int currentIndex = (int)row.Cells[1].Value;
                bool currentSelected = (bool)row.Cells[0].Value;

                if (_levelSettings.SelectedSounds.Contains(currentIndex) && !currentSelected)
                    _levelSettings.SelectedSounds.Remove(currentIndex);
                else if (!_levelSettings.SelectedSounds.Contains(currentIndex) && currentSelected)
                    _levelSettings.SelectedSounds.Add(currentIndex);

                UpdateSoundStatistics();
            }
        }

        private void selectedSoundsDataGridView_HighlightRow(DataGridViewRow row, bool missing = false)
        {
            if (row.DefaultCellStyle.BackColor == _columnMessageWrongColor)
                return; // Don't overwrite errors

            bool selected = (bool)row.Cells[0].Value;
            if (missing)
                row.DefaultCellStyle.BackColor = _columnMessageWrongColor;
            else if (selected)
                row.DefaultCellStyle.BackColor = Color.DarkGreen;
            else
                row.DefaultCellStyle.BackColor = selectedSoundsDataGridView.BackColor;
        }

        private void soundsCatalogsDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _soundsCatalogsDataGridViewDataSource.Count)
                return;
            ReferencedSoundCatalog sounds = _soundsCatalogsDataGridViewDataSource[e.RowIndex].Sounds;

            if (soundsCatalogsDataGridView.Columns[e.ColumnIndex].Name == SoundsCatalogMessageColumn.Name)
            {
                if (sounds.LoadException == null)
                {
                    e.CellStyle.BackColor = _columnMessageCorrectColor;
                    e.CellStyle.SelectionBackColor = e.CellStyle.SelectionBackColor.MixWith(_columnMessageCorrectColor, 0.4);
                }
                else
                {
                    e.CellStyle.BackColor = _columnMessageWrongColor;
                    e.CellStyle.SelectionBackColor = e.CellStyle.SelectionBackColor.MixWith(_columnMessageWrongColor, 0.4);
                }
            }
            else if (soundsCatalogsDataGridView.Columns[e.ColumnIndex].Name == SoundsCatalogPathColumn.Name)
            {
                string absolutePath = _levelSettings.MakeAbsolute(sounds.Path);
                soundsCatalogsDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = absolutePath;
            }
        }

        private void soundsCatalogsDataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= soundsCatalogsDataGridView.Rows.Count)
                return;

            if (soundsCatalogsDataGridView.Columns[e.ColumnIndex].Name == SoundsCatalogSearchColumn.Name)
                soundsCatalogsDataGridView.PaintCell(e, Properties.Resources.general_Open_16);
            else if (soundsCatalogsDataGridView.Columns[e.ColumnIndex].Name == SoundsCatalogReloadButton.Name)
                soundsCatalogsDataGridView.PaintCell(e, Properties.Resources.actions_refresh_16);
            else if (soundsCatalogsDataGridView.Columns[e.ColumnIndex].Name == SoundsCatalogEditColumn.Name)
            {
                var cell = soundsCatalogsDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex] as DarkDataGridViewButtonCell;

                if (_soundsCatalogsDataGridViewDataSource[e.RowIndex].Path?.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase) ?? false)
                {
                    cell.Hidden = false;
                    soundsCatalogsDataGridView.PaintCell(e, Properties.Resources.general_edit_16);
                }
                else
                    cell.Hidden = true;
            }
        }

        private void soundsCatalogsDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _soundsCatalogsDataGridViewDataSource.Count)
                return;
            ReferencedSoundCatalog soundsCatalog = _soundsCatalogsDataGridViewDataSource[e.RowIndex].Sounds;

            if (soundsCatalogsDataGridView.Columns[e.ColumnIndex].Name == SoundsCatalogSearchColumn.Name)
            {
                if (EditorActions.ReloadResource(this, _levelSettings, _soundsCatalogsDataGridViewDataSource[e.RowIndex].Sounds))
                    PopulateSoundInfoListAndResetFilter();
            }
            else if (soundsCatalogsDataGridView.Columns[e.ColumnIndex].Name == SoundsCatalogsAssignColumn.Name)
                AssignAllSounds(soundsCatalog.Sounds);
            else if (soundsCatalogsDataGridView.Columns[e.ColumnIndex].Name == SoundsCatalogReloadButton.Name)
            {
                _soundsCatalogsDataGridViewDataSource[e.RowIndex].Sounds.Reload(_levelSettings);
                soundsCatalogsDataGridView.InvalidateRow(e.RowIndex);
                PopulateSoundInfoListAndResetFilter();
            }
            else if (soundsCatalogsDataGridView.Columns[e.ColumnIndex].Name == SoundsCatalogEditColumn.Name)
            {
                var path = _levelSettings.MakeAbsolute(_soundsCatalogsDataGridViewDataSource[e.RowIndex].Path);
                if (File.Exists(DefaultPaths.SoundToolExecutable) && File.Exists(path) && Path.GetExtension(path).Equals(".xml", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(_levelSettings.LevelFilePath))
                        System.Diagnostics.Process.Start(DefaultPaths.SoundToolExecutable, "\"" + path + "\" -r \"" + _levelSettings.MakeAbsolute(_levelSettings.LevelFilePath));
                    else
                        System.Diagnostics.Process.Start(DefaultPaths.SoundToolExecutable, "\"" + path + "\"");
                }
            }
        }

        private void soundsCatalogsDataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e) => PopulateSoundInfoListAndResetFilter();
        private void soundsCatalogsDataGridView_Sorted(object sender, EventArgs e) => PopulateSoundInfoListAndResetFilter();

        private void butFilterSounds_Click(object sender, EventArgs e) => PopulateSoundInfoList();
        private void butSelectAllSounds_Click(object sender, EventArgs e) => ToggleSelectionForAllSounds(true);
        private void butDeselectAllSounds_Click(object sender, EventArgs e) => ToggleSelectionForAllSounds(false);

        private void ButAutodetectSoundsAndAssign_Click(object sender, EventArgs e)
        {
            EditorActions.AutodetectAndAssignSounds(_levelSettings, this);
            PopulateSoundInfoList();
        }

        private void ButAssignHardcodedSounds_Click(object sender, EventArgs e)
        {
            EditorActions.AssignHardcodedSounds(_levelSettings);
            PopulateSoundInfoList();
        }

        private void ButAssignSoundsFromSelectedCatalogs_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in soundsCatalogsDataGridView.SelectedRows)
                EditorActions.AssignCatalogSounds(_levelSettings, _soundsCatalogsDataGridViewDataSource[row.Index].Sounds);
            PopulateSoundInfoList();
        }

        private void ButAssignFromSoundSources_Click(object sender, EventArgs e)
        {
            EditorActions.AssignSoundSourcesSounds(_levelSettings);
            PopulateSoundInfoList();
        }

        private void tbFilterSounds_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                PopulateSoundInfoList();
        }

        private void butRemoveMissing_Click(object sender, EventArgs e)
        {
            _levelSettings.SelectedSounds = _levelSettings.SelectedSounds.Except(_levelSettings.SelectedAndMissingSounds).ToList();
            PopulateSoundInfoList();
        }

        private void butAssignFromWads_Click(object sender, EventArgs e)
        {
            EditorActions.AssignWadSounds(_levelSettings);
            PopulateSoundInfoList();
        }

        private void cbRemoveObjects_CheckedChanged(object sender, EventArgs e)
        {
            _levelSettings.RemoveUnusedObjects = cbRemoveObjects.Checked;
            UpdateDialog();
        }

        private void butDeselectAllStatics_Click(object sender, EventArgs e) => ToggleSelectionForStatics(false);
        private void butSelectAllButShatterStatics_Click(object sender, EventArgs e) => ToggleSelectionForStatics(true);
        private void butSelectAllStatics_Click(object sender, EventArgs e) => ToggleSelectionForStatics(true, false);

        private void butBrowseLuaPath_Click(object sender, EventArgs e)
        {
            string result = LevelFileDialog.BrowseFile(this, _levelSettings, _levelSettings.TenLuaScriptFile,
               "Select the Lua script file for this level", new[] { new FileFormat("Lua script file", "lua") }, 
               VariableType.LevelDirectory, false);

            if (result != null)
            {
                _levelSettings.TenLuaScriptFile = result;
                UpdateDialog();
            }
        }

        private void tbLuaPath_TextChanged(object sender, EventArgs e)
        {
            if (_levelSettings.TenLuaScriptFile == tbLuaPath.Text)
                return;
            _levelSettings.TenLuaScriptFile = tbLuaPath.Text;
            UpdateDialog();
        }

        private void cbKeepSampleRate_CheckedChanged(object sender, EventArgs e)
        {
            _levelSettings.EnableCustomSampleRate = cbKeepSampleRate.Checked;
            UpdateDialog();
        }

        private void cmbSampleRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            int result;
            if (cmbSampleRate.SelectedIndex != -1 &&
                int.TryParse(cmbSampleRate.SelectedItem.ToString(), out result))
                _levelSettings.CustomSampleRate = result;
            UpdateDialog();
        }

        private void cbCompressTextures_CheckedChanged(object sender, EventArgs e)
        {
            _levelSettings.CompressTextures = cbCompressTextures.Checked;
            UpdateDialog();
        }
    }
}
