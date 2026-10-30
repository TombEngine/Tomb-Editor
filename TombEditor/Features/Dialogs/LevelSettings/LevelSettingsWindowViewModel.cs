#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelSettingsData = TombLib.LevelData.LevelSettings;
using static TombLib.LevelData.TRVersion;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using TombLib.GeometryIO;
using TombLib.LevelData;
using TombLib.Services;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using TombLib.WPF;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;
using TombLib.WPF.ViewModels;
using WpfColor = System.Windows.Media.Color;

namespace TombEditor.Features.Dialogs.LevelSettings
{
    /// <summary>
    /// WPF port of <c>FormLevelSettings</c> (phase 1: shell + Game + Paths tabs). Edits a clone of
    /// the level's <see cref="LevelSettingsData"/> and applies it via <c>Editor.UpdateLevelSettings</c>
    /// on Apply/OK, mirroring the previous form. The remaining tabs (Textures, Objects, sounds,
    /// imported geometry, static meshes, sky/font, misc) are ported in later phases.
    /// </summary>
    public partial class LevelSettingsWindowViewModel : ObservableObject, IModalDialogViewModel
    {
        private readonly Editor _editor;
        private readonly LevelSettingsData _settings;
        private readonly IDialogService _dialogService;
        private readonly IColorPickerService _colorPickerService;
        private readonly ILocalizationService _localizationService;

        [ObservableProperty] private bool? _dialogResult;

        public IReadOnlyList<Game> GameVersions { get; } = AllVersions.ToList();
        public IReadOnlyList<PathVariableRow> PathVariables { get; }

        public LevelSettingsWindowViewModel(
            Editor editor,
            IDialogService? dialogService = null,
            IColorPickerService? colorPickerService = null,
            ILocalizationService? localizationService = null)
        {
            _editor = editor;
            _dialogService = ServiceLocator.ResolveService(dialogService);
            _colorPickerService = ServiceLocator.ResolveService(colorPickerService);
            _localizationService = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);
            _settings = editor.Level.Settings.Clone();

            TrxTextureDepths = new[]
            {
                _localizationService["TrxDepthDefault"],
                _localizationService["TrxDepth8Bit"],
                _localizationService["TrxDepth16Bit"],
                _localizationService["TrxDepth32Bit"]
            };
            LightQualities = new[]
            {
                _localizationService["LightQualityLow"],
                _localizationService["LightQualityMedium"],
                _localizationService["LightQualityHigh"]
            };

            PathVariables = Enum.GetValues<VariableType>()
                .Select(type => new PathVariableRow(_settings, LevelSettingsData.VariableCreate(type)))
                .ToList();

            Font = CreateCustomResource("Font.pc", "BrowseFontTitle", () => _settings.FontTextureFilePath, path => _settings.FontTextureFilePath = path, _settings.LoadFontTexture);
            Sky = CreateCustomResource("pcsky.raw", "BrowseSkyTitle", () => _settings.SkyTextureFilePath, path => _settings.SkyTextureFilePath = path, _settings.LoadSkyTexture);
            Tr5Sprites = CreateCustomResource("Extra.Tr5.pc", "BrowseTr5SpritesTitle", () => _settings.Tr5ExtraSpritesFilePath, path => _settings.Tr5ExtraSpritesFilePath = path, _settings.LoadTr5ExtraSprites);

            BuildStaticMeshMerges();

            foreach (var texture in _settings.Textures)
                Textures.Add(new TextureRow(_settings, texture));
            foreach (var wad in _settings.Wads)
                WadRows.Add(new WadRow(_settings, wad));
            foreach (var catalog in _settings.SoundCatalogs)
                SoundCatalogs.Add(CreateSoundCatalogRow(catalog));
            foreach (var soundPath in _settings.WadSoundPaths)
                SampleRows.Add(new SampleRow(_settings, soundPath.Clone()));
            foreach (var geometry in _settings.ImportedGeometries)
                ImportedGeometries.Add(new ImportedGeometryRow(_settings, geometry));

            WadRows.CollectionChanged += (_, _) =>
            {
                SyncResources();
                BuildStaticMeshMerges();
            };
            SoundCatalogs.CollectionChanged += (_, _) => OnSoundSourcesChanged();
            SampleRows.CollectionChanged += (_, _) => OnSoundSourcesChanged();

            PopulateSoundInfoList();
        }

        private CustomResourceViewModel CreateCustomResource(string defaultFile, string browseTitleKey, Func<string?> getPath, Action<string?> setPath, Func<string?, ImageC> loadImage)
            => new(_settings, defaultFile, _localizationService.Format("UseDefaultFile", defaultFile), _localizationService[browseTitleKey], getPath, setPath, loadImage);

        private SoundCatalogRow CreateSoundCatalogRow(ReferencedSoundCatalog catalog)
            => new(_settings, catalog, OnSoundSourcesChanged, IncludeAllSounds);

        private void OnSoundSourcesChanged()
        {
            SyncResources();
            SoundFilter = string.Empty;
            PopulateSoundInfoList();
        }

        // Editable resource grids (Textures / Objects / Sound catalogs / Samples).

        public ObservableCollection<TextureRow> Textures { get; } = new();
        public ObservableCollection<WadRow> WadRows { get; } = new();
        public ObservableCollection<SoundCatalogRow> SoundCatalogs { get; } = new();
        public ObservableCollection<SampleRow> SampleRows { get; } = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteTextureCommand))]
        [NotifyCanExecuteChangedFor(nameof(MoveTextureUpCommand))]
        [NotifyCanExecuteChangedFor(nameof(MoveTextureDownCommand))]
        private TextureRow? _selectedTexture;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteWadCommand))]
        [NotifyCanExecuteChangedFor(nameof(MoveWadUpCommand))]
        [NotifyCanExecuteChangedFor(nameof(MoveWadDownCommand))]
        private WadRow? _selectedWad;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteSoundCatalogCommand))]
        [NotifyCanExecuteChangedFor(nameof(MoveSoundCatalogUpCommand))]
        [NotifyCanExecuteChangedFor(nameof(MoveSoundCatalogDownCommand))]
        private SoundCatalogRow? _selectedSoundCatalog;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteSampleCommand))]
        [NotifyCanExecuteChangedFor(nameof(MoveSampleUpCommand))]
        [NotifyCanExecuteChangedFor(nameof(MoveSampleDownCommand))]
        private SampleRow? _selectedSample;

        public bool HasSelectedTexture => SelectedTexture != null;
        public bool HasSelectedWad => SelectedWad != null;
        public bool HasSelectedSoundCatalog => SelectedSoundCatalog != null;
        public bool HasSelectedSample => SelectedSample != null;

        [RelayCommand]
        private void AddTextures()
        {
            foreach (var path in LevelFileDialog.BrowseFiles(Owner, _settings, _settings.LevelFilePath, _localizationService["BrowseTexturesTitle"], ImageC.FileExtensions, VariableType.LevelDirectory))
                Textures.Add(new TextureRow(_settings, new LevelTexture(_settings, path)));
        }

        [RelayCommand(CanExecute = nameof(HasSelectedTexture))] private void DeleteTexture() => Remove(Textures, SelectedTexture);
        [RelayCommand(CanExecute = nameof(HasSelectedTexture))] private void MoveTextureUp() => Move(Textures, SelectedTexture, -1);
        [RelayCommand(CanExecute = nameof(HasSelectedTexture))] private void MoveTextureDown() => Move(Textures, SelectedTexture, 1);

        [RelayCommand]
        private async Task AddWadsAsync()
        {
            var paths = LevelFileDialog.BrowseFiles(Owner, _settings, _settings.LevelFilePath, _localizationService["BrowseWadsTitle"], Wad2.FileExtensions, VariableType.LevelDirectory).ToList();

            if (paths.Count == 0)
                return;

            // Loading happens off the UI thread so the dialog handler can still show prompts on it.
            var dialogHandler = new GraphicalDialogHandler(Owner);
            var loaded = await Task.Run(() => ReferencedWadLoader.Load(_settings, paths, dialogHandler));

            foreach (var catalog in loaded.SoundCatalogs.Where(catalog => SoundCatalogs.All(row => row.Path != catalog.Path)))
            {
                SoundCatalogs.Add(CreateSoundCatalogRow(catalog));
                IncludeAllSounds(catalog);
            }

            foreach (var wad in loaded.Wads)
                WadRows.Add(new WadRow(_settings, wad));

            // Autoswitch game version if necessary.
            if (EditorActions.AutoswitchGameVersion(_settings, loaded.Wads.Select(wad => wad.Wad)))
                NotifyGameVersionChanged();
        }

        /// <summary>Copies the wad and catalog lists from the grid rows into the edited settings.</summary>
        private void SyncResources()
        {
            _settings.Textures.Clear();
            _settings.Textures.AddRange(Textures.Select(row => row.Texture));

            _settings.Wads.Clear();
            _settings.Wads.AddRange(WadRows.Select(row => row.Wad));

            _settings.SoundCatalogs.Clear();
            _settings.SoundCatalogs.AddRange(SoundCatalogs.Select(row => row.Catalog));

            _settings.WadSoundPaths.Clear();
            _settings.WadSoundPaths.AddRange(SampleRows.Select(row => row.SoundPath.Clone()));
        }

        private void IncludeAllSounds(ReferencedSoundCatalog catalog)
        {
            if (catalog.Sounds is null)
                return;

            foreach (var sound in catalog.Sounds.SoundInfos.Where(sound => !_settings.SelectedSounds.Contains(sound.Id)))
                _settings.SelectedSounds.Add(sound.Id);

            SoundFilter = string.Empty;
            PopulateSoundInfoList();
        }

        [RelayCommand(CanExecute = nameof(HasSelectedWad))] private void DeleteWad() => Remove(WadRows, SelectedWad);
        [RelayCommand(CanExecute = nameof(HasSelectedWad))] private void MoveWadUp() => Move(WadRows, SelectedWad, -1);
        [RelayCommand(CanExecute = nameof(HasSelectedWad))] private void MoveWadDown() => Move(WadRows, SelectedWad, 1);

        [RelayCommand]
        private void AddSoundCatalogs()
        {
            foreach (var path in LevelFileDialog.BrowseFiles(Owner, _settings, _settings.LevelFilePath, _localizationService["BrowseSoundCatalogsTitle"], WadSounds.FileExtensions, VariableType.LevelDirectory)
                .Where(p => SoundCatalogs.All(c => c.Path != p)))
                SoundCatalogs.Add(CreateSoundCatalogRow(new ReferencedSoundCatalog(_settings, path)));
        }

        [RelayCommand(CanExecute = nameof(HasSelectedSoundCatalog))] private void DeleteSoundCatalog() => Remove(SoundCatalogs, SelectedSoundCatalog);
        [RelayCommand(CanExecute = nameof(HasSelectedSoundCatalog))] private void MoveSoundCatalogUp() => Move(SoundCatalogs, SelectedSoundCatalog, -1);
        [RelayCommand(CanExecute = nameof(HasSelectedSoundCatalog))] private void MoveSoundCatalogDown() => Move(SoundCatalogs, SelectedSoundCatalog, 1);

        [RelayCommand]
        private void AddSample()
        {
            string? result = LevelFileDialog.BrowseFolder(Owner, _settings, _settings.LevelFilePath, _localizationService["BrowseNewSoundFolderTitle"], VariableType.LevelDirectory);
            if (result != null)
                SampleRows.Add(new SampleRow(_settings, new WadSoundPath(result)));
        }

        [RelayCommand(CanExecute = nameof(HasSelectedSample))] private void DeleteSample() => Remove(SampleRows, SelectedSample);
        [RelayCommand(CanExecute = nameof(HasSelectedSample))] private void MoveSampleUp() => Move(SampleRows, SelectedSample, -1);
        [RelayCommand(CanExecute = nameof(HasSelectedSample))] private void MoveSampleDown() => Move(SampleRows, SelectedSample, 1);

        private static void Remove<T>(ObservableCollection<T> list, T? item) where T : class
        {
            if (item != null)
                list.Remove(item);
        }

        private static void Move<T>(ObservableCollection<T> list, T? item, int direction) where T : class
        {
            if (item == null)
                return;
            int index = list.IndexOf(item);
            int target = index + direction;
            if (index < 0 || target < 0 || target >= list.Count)
                return;
            list.Move(index, target);
        }

        // Game tab.

        public Game GameVersion
        {
            get => _settings.GameVersion;
            set
            {
                if (_settings.GameVersion == value)
                    return;
                _settings.GameVersion = value;
                _settings.ConvertLevelExtension();
                NotifyGameVersionChanged();
            }
        }

        private void NotifyGameVersionChanged()
        {
            OnPropertyChanged(nameof(GameVersion));
            OnPropertyChanged(nameof(GameLevelFilePath));
            OnPropertyChanged(nameof(IsTRNG));
            OnPropertyChanged(nameof(IsTombEngine));
            OnPropertyChanged(nameof(IsNotTombEngine));
            OnPropertyChanged(nameof(IsTR4Native));
            OnPropertyChanged(nameof(IsTR5));
            OnPropertyChanged(nameof(Supports16BitDithering));
            OnPropertyChanged(nameof(CanDither16BitTextures));
            OnPropertyChanged(nameof(SupportsFontAndSky));
            OnPropertyChanged(nameof(IsTRX));
            OnPropertyChanged(nameof(UsesMainSfx));
            OnPropertyChanged(nameof(SupportsSamplePaths));
            OnPropertyChanged(nameof(CatalogsPrompt));
            OnPropertyChanged(nameof(SamplesPrompt));
            RaiseAllValidity();
            PopulateSoundInfoList();
        }

        public bool UsesMainSfx => _settings.GameVersion.UsesMainSfx();
        public bool SupportsSamplePaths => !UsesMainSfx;
        public string CatalogsPrompt => _localizationService["CatalogsPromptBase"] + "\n" + _localizationService[UsesMainSfx ? "CatalogsPromptMainSfx" : "CatalogsPromptNew"];
        public string SamplesPrompt => _localizationService[UsesMainSfx ? "SamplesPromptMainSfx" : "SamplesPromptNew"];

        public string GameExecutableFilePath { get => _settings.GameExecutableFilePath; set { SetSetting(_settings.GameExecutableFilePath, value, v => _settings.GameExecutableFilePath = v); RaiseAllValidity(); } }
        public string GameLevelFilePath { get => _settings.GameLevelFilePath; set { SetSetting(_settings.GameLevelFilePath, value, v => _settings.GameLevelFilePath = v); RaiseAllValidity(); } }
        public string ScriptDirectory { get => _settings.ScriptDirectory; set { SetSetting(_settings.ScriptDirectory, value, v => _settings.ScriptDirectory = v); RaiseAllValidity(); } }
        public string TenLuaScriptFile { get => _settings.TenLuaScriptFile; set { SetSetting(_settings.TenLuaScriptFile, value, v => _settings.TenLuaScriptFile = v); RaiseAllValidity(); } }

        public bool GameEnableQuickStartFeature { get => _settings.GameEnableQuickStartFeature; set => SetSetting(_settings.GameEnableQuickStartFeature, value, v => _settings.GameEnableQuickStartFeature = v); }
        public bool GameEnableExtraReverbPresets { get => _settings.GameEnableExtraReverbPresets; set => SetSetting(_settings.GameEnableExtraReverbPresets, value, v => _settings.GameEnableExtraReverbPresets = v); }
        public bool GameEnableExtraBlendingModes { get => _settings.GameEnableExtraBlendingModes ?? false; set => SetSetting(_settings.GameEnableExtraBlendingModes ?? false, value, v => _settings.GameEnableExtraBlendingModes = v); }

        // Paths tab.

        public string LevelFilePath { get => _settings.LevelFilePath; set { SetSetting(_settings.LevelFilePath, value, v => _settings.LevelFilePath = v); RaiseAllValidity(); RefreshResourcePreviews(); } }
        public string GameDirectory { get => _settings.GameDirectory; set { SetSetting(_settings.GameDirectory, value, v => _settings.GameDirectory = v); RaiseAllValidity(); RefreshResourcePreviews(); } }

        private void RefreshResourcePreviews()
        {
            Font.Refresh();
            Sky.Refresh();
            Tr5Sprites.Refresh();
        }

        // Path validity (true = path exists; bound to red highlight when false).

        public bool LevelFilePathValid => Directory.Exists(PathC.GetDirectoryNameTry(_settings.LevelFilePath));
        public bool GameDirectoryValid => Directory.Exists(_settings.MakeAbsolute(_settings.GameDirectory));
        public bool GameLevelFilePathValid
        {
            get
            {
                string path = _settings.MakeAbsolute(_settings.GameLevelFilePath);
                return Directory.Exists(PathC.GetDirectoryNameTry(path)) && !string.IsNullOrEmpty(Path.GetExtension(path));
            }
        }
        public bool GameExecutableValid => File.Exists(_settings.MakeAbsolute(_settings.GameExecutableFilePath));
        public bool ScriptDirectoryValid => Directory.Exists(_settings.MakeAbsolute(_settings.ScriptDirectory));
        public bool LuaValid => File.Exists(_settings.MakeAbsolute(_settings.TenLuaScriptFile));

        private void RaiseAllValidity()
        {
            foreach (var variable in PathVariables)
                variable.Refresh();

            OnPropertyChanged(nameof(LevelFilePathValid));
            OnPropertyChanged(nameof(GameDirectoryValid));
            OnPropertyChanged(nameof(GameLevelFilePathValid));
            OnPropertyChanged(nameof(GameExecutableValid));
            OnPropertyChanged(nameof(ScriptDirectoryValid));
            OnPropertyChanged(nameof(LuaValid));
        }

        // Version-dependent visibility.

        public bool IsTRNG => _settings.GameVersion == Game.TRNG;
        public bool IsTombEngine => _settings.GameVersion == Game.TombEngine;
        public bool IsNotTombEngine => _settings.GameVersion != Game.TombEngine;
        public bool IsTR4Native => _settings.GameVersion.Native() == Game.TR4;
        public bool IsTR5 => _settings.GameVersion == Game.TR5;
        public bool Supports16BitDithering => _settings.GameVersion.Supports16BitDithering();

        /// <summary>Dithering is available for the games that support 16-bit textures and for the TRX games.</summary>
        public bool CanDither16BitTextures => Supports16BitDithering || IsTRX;
        public bool SupportsFontAndSky => _settings.GameVersion.SupportsFontAndSkySettings();
        public bool IsTRX => _settings.GameVersion.IsTRX();

        // TRX tab.

        public IReadOnlyList<string> TrxTextureDepths { get; }

        public string SelectedTrxDepth
        {
            get => _settings.TrxTextureBitDepth switch { TrxTextureBitDepth.Bit8 => TrxTextureDepths[1], TrxTextureBitDepth.Bit16 => TrxTextureDepths[2], TrxTextureBitDepth.Bit32 => TrxTextureDepths[3], _ => TrxTextureDepths[0] };
            set { _settings.TrxTextureBitDepth = value == TrxTextureDepths[1] ? TrxTextureBitDepth.Bit8 : value == TrxTextureDepths[2] ? TrxTextureBitDepth.Bit16 : value == TrxTextureDepths[3] ? TrxTextureBitDepth.Bit32 : TrxTextureBitDepth.Default; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets whether TRX levels support multiple alternate room groups.
        /// </summary>
        public bool TrxUseAlternateGroups
        {
            get => _settings.TrxUseAlternateGroups;
            set { _settings.TrxUseAlternateGroups = value; OnPropertyChanged(); }
        }

        public bool TrxConvertFlybysToCinematicFrames
        {
            get => _settings.TrxConvertFlybysToCinematicFrames;
            set { _settings.TrxConvertFlybysToCinematicFrames = value; OnPropertyChanged(); }
        }

        // Sky & font tab.

        public IReadOnlyList<Tr5WeatherType> Tr5WeatherTypes { get; } = Enum.GetValues(typeof(Tr5WeatherType)).Cast<Tr5WeatherType>().ToList();
        public IReadOnlyList<Tr5LaraType> Tr5LaraTypes { get; } = Enum.GetValues(typeof(Tr5LaraType)).Cast<Tr5LaraType>().ToList();

        public Tr5WeatherType Tr5WeatherType { get => _settings.Tr5WeatherType; set => SetSetting(_settings.Tr5WeatherType, value, v => _settings.Tr5WeatherType = v); }
        public Tr5LaraType Tr5LaraType { get => _settings.Tr5LaraType; set => SetSetting(_settings.Tr5LaraType, value, v => _settings.Tr5LaraType = v); }

        public CustomResourceViewModel Font { get; }
        public CustomResourceViewModel Sky { get; }
        public CustomResourceViewModel Tr5Sprites { get; }

        // Misc tab.

        public IReadOnlyList<string> SampleRates { get; } = new[] { "11025", "22050", "44100", "48000" };
        public IReadOnlyList<string> LightQualities { get; }

        public double TexturePadding { get => _settings.TexturePadding; set => SetSetting<double>(_settings.TexturePadding, value, v => _settings.TexturePadding = (int)v); }
        public bool Dither16BitTextures { get => _settings.Dither16BitTextures; set => SetSetting(_settings.Dither16BitTextures, value, v => _settings.Dither16BitTextures = v); }
        public bool CompressTextures { get => _settings.CompressTextures; set => SetSetting(_settings.CompressTextures, value, v => _settings.CompressTextures = v); }
        public bool AgressiveTexturePacking { get => _settings.AgressiveTexturePacking; set => SetSetting(_settings.AgressiveTexturePacking, value, v => _settings.AgressiveTexturePacking = v); }
        public bool AgressiveFloordataPacking { get => _settings.AgressiveFloordataPacking; set => SetSetting(_settings.AgressiveFloordataPacking, value, v => _settings.AgressiveFloordataPacking = v); }
        public bool Room32BitLighting { get => _settings.Room32BitLighting; set => SetSetting(_settings.Room32BitLighting, value, v => _settings.Room32BitLighting = v); }
        public bool RemapAnimatedTextures { get => _settings.RemapAnimatedTextures; set => SetSetting(_settings.RemapAnimatedTextures, value, v => _settings.RemapAnimatedTextures = v); }
        public bool RearrangeVerticalRooms { get => _settings.RearrangeVerticalRooms; set => SetSetting(_settings.RearrangeVerticalRooms, value, v => _settings.RearrangeVerticalRooms = v); }
        public bool RemoveUnusedObjects { get => _settings.RemoveUnusedObjects; set => SetSetting(_settings.RemoveUnusedObjects, value, v => _settings.RemoveUnusedObjects = v); }
        public bool AutoAssignSoundsIfNoSelection { get => _settings.AutoAssignSoundsIfNoSelection; set => SetSetting(_settings.AutoAssignSoundsIfNoSelection, value, v => _settings.AutoAssignSoundsIfNoSelection = v); }

        public bool EnableCustomSampleRate
        {
            get => _settings.EnableCustomSampleRate;
            set { if (SetSetting(_settings.EnableCustomSampleRate, value, v => _settings.EnableCustomSampleRate = v)) OnPropertyChanged(nameof(CanEditSampleRate)); }
        }
        public bool CanEditSampleRate => _settings.EnableCustomSampleRate;

        public int DefaultLightQualityIndex
        {
            get => _settings.DefaultLightQuality == LightQuality.Default ? 0 : (int)_settings.DefaultLightQuality - 1;
            set { _settings.DefaultLightQuality = (LightQuality)(value + 1); OnPropertyChanged(); }
        }

        public string SelectedSampleRate
        {
            get => _settings.CustomSampleRate.ToString();
            set { if (int.TryParse(value, out int rate)) { _settings.CustomSampleRate = rate; OnPropertyChanged(); } }
        }

        public WpfColor AmbientLightColor => (_settings.DefaultAmbientLight * 0.5f).ToWPFColor();

        [RelayCommand]
        private void PickAmbientLight()
        {
            Vector3? pickedColor = _colorPickerService.PickColor(_settings.DefaultAmbientLight * 0.5f);
            if (pickedColor == null)
                return;

            _settings.DefaultAmbientLight = pickedColor.Value * 2.0f;
            OnPropertyChanged(nameof(AmbientLightColor));
        }

        // Browse commands.

        [RelayCommand]
        private void BrowseLevelFile()
            => SetIfBrowsed(LevelFileDialog.BrowseFile(Owner, _settings, _settings.LevelFilePath, _localizationService["BrowseLevelFileTitle"], LevelSettingsData.FileFormatsLevel, null, true), v => LevelFilePath = v);

        [RelayCommand]
        private void BrowseGameDirectory()
            => SetIfBrowsed(LevelFileDialog.BrowseFolder(Owner, _settings, _settings.GameDirectory, _localizationService["BrowseGameDirectoryTitle"], VariableType.LevelDirectory), v => GameDirectory = v);

        [RelayCommand]
        private void BrowseGameLevelFile()
            => SetIfBrowsed(LevelFileDialog.BrowseFile(Owner, _settings, _settings.GameLevelFilePath, _localizationService["BrowseGameLevelFileTitle"], LevelSettingsData.FileFormatsLevelCompiled, VariableType.GameDirectory, true), v => GameLevelFilePath = v);

        [RelayCommand]
        private void BrowseGameExecutable()
            => SetIfBrowsed(LevelFileDialog.BrowseFile(Owner, _settings, _settings.GameExecutableFilePath, _localizationService["BrowseGameExecutableTitle"], new[] { new FileFormat("Windows executables", "exe") }, VariableType.GameDirectory, false), v => GameExecutableFilePath = v);

        [RelayCommand]
        private void BrowseScript()
            => SetIfBrowsed(LevelFileDialog.BrowseFolder(Owner, _settings, _settings.ScriptDirectory, _localizationService["BrowseScriptTitle"], VariableType.LevelDirectory), v => ScriptDirectory = v);

        [RelayCommand]
        private void BrowseLua()
            => SetIfBrowsed(LevelFileDialog.BrowseFile(Owner, _settings, _settings.TenLuaScriptFile, _localizationService["BrowseLuaTitle"], new[] { new FileFormat("Lua script", "lua") }, VariableType.LevelDirectory, false), v => TenLuaScriptFile = v);

        // Static meshes tab.

        public ObservableCollection<StaticMeshMergeRow> StaticMeshMerges { get; } = new();

        [RelayCommand]
        private void SelectAllStatics() => ToggleStatics(true, bypassShatterable: false);

        [RelayCommand]
        private void SelectAllButShatterStatics() => ToggleStatics(true, bypassShatterable: true);

        [RelayCommand]
        private void DeselectAllStatics() => ToggleStatics(false, bypassShatterable: false);

        private void ToggleStatics(bool value, bool bypassShatterable)
        {
            foreach (var row in StaticMeshMerges)
            {
                if (value && bypassShatterable && TrCatalog.IsStaticShatterable(_settings.GameVersion, row.MeshId))
                {
                    row.Merge = false;
                    continue;
                }

                row.Merge = value;
                if (!value)
                    row.InterpretShadesAsEffect = false;
            }
        }

        private void BuildStaticMeshMerges()
        {
            StaticMeshMerges.Clear();
            foreach (var staticMesh in _settings.WadGetAllStatics())
            {
                uint typeId = staticMesh.Value.Id.TypeId;
                var existing = _settings.AutoStaticMeshMerges.FirstOrDefault(e => e.meshId.Equals(typeId));
                StaticMeshMerges.Add(new StaticMeshMergeRow(existing ?? new AutoStaticMeshMergeEntry(typeId, false, false, false, false, _settings)));
            }
        }

        // Imported geometry tab.

        public ObservableCollection<ImportedGeometryRow> ImportedGeometries { get; } = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteImportedGeometryCommand))]
        private ImportedGeometryRow? _selectedImportedGeometry;

        public bool HasSelectedImportedGeometry => SelectedImportedGeometry != null;

        [RelayCommand]
        private void AddImportedGeometry()
        {
            var paths = LevelFileDialog.BrowseFiles(Owner, _settings, _settings.LevelFilePath, _localizationService["BrowseImportedGeometryTitle"], BaseGeometryImporter.FileExtensions, VariableType.LevelDirectory).ToList();
            var importInfos = new List<KeyValuePair<ImportedGeometry, ImportedGeometryInfo>>();
            var config = _editor.Configuration;

            foreach (string path in paths)
            {
                var ioViewModel = new GeometryIOSettingsWindowViewModel(IOSettingsPresets.GeometryImportSettingsPresets);
                ioViewModel.SelectPreset(config?.GeometryIO_LastUsedGeometryImportPresetName);

                if (_dialogService.ShowDialog(this, ioViewModel) != true)
                    continue;

                if (config != null)
                    config.GeometryIO_LastUsedGeometryImportPresetName = ioViewModel.SelectedPreset?.Name;

                var info = new ImportedGeometryInfo(_settings.MakeRelative(path, VariableType.LevelDirectory), ioViewModel.GetCurrentSettings());
                importInfos.Add(new KeyValuePair<ImportedGeometry, ImportedGeometryInfo>(new ImportedGeometry(), info));
            }

            if (importInfos.Count == 0)
                return;

            _settings.ImportedGeometryUpdate(importInfos);
            _settings.ImportedGeometries.AddRange(importInfos.Select(e => e.Key));
            foreach (var entry in importInfos)
                ImportedGeometries.Add(new ImportedGeometryRow(_settings, entry.Key));
        }

        [RelayCommand(CanExecute = nameof(HasSelectedImportedGeometry))]
        private void DeleteImportedGeometry()
        {
            if (SelectedImportedGeometry == null)
                return;
            _settings.ImportedGeometries.Remove(SelectedImportedGeometry.Object);
            ImportedGeometries.Remove(SelectedImportedGeometry);
        }

        // Sound selection (Sound catalogs tab).

        public ObservableCollection<SoundInfoRow> SoundInfos { get; } = new();

        private string _soundFilter = string.Empty;
        public string SoundFilter { get => _soundFilter; set { if (SetProperty(ref _soundFilter, value)) PopulateSoundInfoList(); } }

        [RelayCommand] private void AssignFromWads() { EditorActions.AssignWadSounds(_settings); PopulateSoundInfoList(); }
        [RelayCommand] private void AssignFromSoundSources() { EditorActions.AssignSoundSourcesSounds(_settings); PopulateSoundInfoList(); }
        [RelayCommand] private void AssignHardcodedSounds() { EditorActions.AssignHardcodedSounds(_settings); PopulateSoundInfoList(); }
        [RelayCommand] private void AutodetectSounds() { EditorActions.AutodetectAndAssignSounds(_settings, Owner); PopulateSoundInfoList(); }

        [RelayCommand]
        private void AssignFromSelectedCatalog()
        {
            if (SelectedSoundCatalog != null)
                EditorActions.AssignCatalogSounds(_settings, SelectedSoundCatalog.Catalog);
            PopulateSoundInfoList();
        }

        [RelayCommand]
        private void SelectAllSounds()
        {
            foreach (var catalog in _settings.SoundCatalogs)
                if (catalog.Sounds != null)
                    foreach (var sound in catalog.Sounds.SoundInfos)
                        if (!_settings.SelectedSounds.Contains(sound.Id))
                            _settings.SelectedSounds.Add(sound.Id);
            PopulateSoundInfoList();
        }

        [RelayCommand]
        private void DeselectAllSounds()
        {
            _settings.SelectedSounds.Clear();
            PopulateSoundInfoList();
        }

        [RelayCommand]
        private void RemoveMissingSounds()
        {
            _settings.SelectedSounds = _settings.SelectedSounds.Except(_settings.SelectedAndMissingSounds).ToList();
            PopulateSoundInfoList();
        }

        private void PopulateSoundInfoList()
        {
            SoundInfos.Clear();
            var rows = new List<SoundInfoRow>();
            var paths = _settings.GetRecursiveListOfSoundPaths();

            foreach (var info in _settings.GlobalSoundMap)
            {
                if (!string.IsNullOrEmpty(SoundFilter) && info.Name.IndexOf(SoundFilter, StringComparison.InvariantCultureIgnoreCase) < 0)
                    continue;

                string area = GetNgDescription(info.Id, out int originalId);
                int sampleCount = info.SampleCount(_settings, paths);
                string samples = sampleCount == 0 ? _localizationService["SamplesMissing"] : sampleCount == -1 ? _localizationService["SamplesNone"] : sampleCount.ToString();
                rows.Add(new SoundInfoRow(_settings.SelectedSounds, null, info.Id, info.Name, info.SoundCatalog, samples, area, originalId, sampleCount == 0));
            }

            foreach (var missing in _settings.SelectedAndMissingSounds)
            {
                string name = TrCatalog.GetOriginalSoundName(_settings.GameVersion, (uint)missing);
                if (!string.IsNullOrEmpty(SoundFilter) && name.IndexOf(SoundFilter, StringComparison.InvariantCultureIgnoreCase) < 0)
                    continue;

                string area = GetNgDescription(missing, out int originalId);
                rows.Add(new SoundInfoRow(_settings.SelectedSounds, null, missing, name, _localizationService["CatalogNotPresent"], string.Empty, area, originalId, true));
            }

            foreach (var row in rows.OrderBy(row => row.Id))
                SoundInfos.Add(row);
        }

        private string GetNgDescription(int id, out int originalId)
        {
            originalId = id;
            if (_settings.GameVersion != Game.TRNG)
                return string.Empty;

            if (id < 370) { originalId = id; return "TR4"; }
            if (id < 500) { originalId = id - 370; return "NGReserved"; }
            if (id < 525) { originalId = id - 500; return "CustEnv"; }
            if (id < 602) { originalId = id - 525; return "CustAnims"; }
            if (id < 858) { originalId = id - 602; return "TR1"; }
            if (id < 1228) { originalId = id - 858; return "TR2"; }
            if (id < 1598) { originalId = id - 1228; return "TR3"; }
            originalId = id - 1598;
            return "TR5";
        }

        // Dialog commands.

        [RelayCommand]
        private void Apply()
        {
            CommitCollections();
            _editor.UpdateLevelSettings(_settings.Clone());
        }

        [RelayCommand]
        private void Ok()
        {
            CommitCollections();
            _editor.UpdateLevelSettings(_settings.Clone());
            DialogResult = true;
        }

        [RelayCommand]
        private void Cancel() => DialogResult = false;

        // Helpers.

        private static IWindowOwner Owner => WPFUtils.GetWindowOwner();

        private void CommitCollections()
        {
            SyncResources();

            _settings.AutoStaticMeshMerges.Clear();
            foreach (var row in StaticMeshMerges)
                if (row.Merge)
                    _settings.AutoStaticMeshMerges.Add(row.Entry.Clone()); // Clone(), otherwise merge entries won't update
        }

        private bool SetSetting<T>(T current, T value, System.Action<T> setter, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(current, value))
                return false;
            setter(value);
            OnPropertyChanged(propertyName);
            return true;
        }

        private static void SetIfBrowsed(string? result, System.Action<string> apply)
        {
            if (result != null)
                apply(result);
        }

        public partial class StaticMeshMergeRow : ObservableObject
        {
            public AutoStaticMeshMergeEntry Entry { get; }

            public StaticMeshMergeRow(AutoStaticMeshMergeEntry entry) => Entry = entry;

            public string StaticMesh => Entry.StaticMesh;
            public uint MeshId => Entry.meshId;

            public bool Merge { get => Entry.Merge; set { if (Entry.Merge == value) return; Entry.Merge = value; OnPropertyChanged(); } }
            public bool InterpretShadesAsEffect { get => Entry.InterpretShadesAsEffect; set { if (Entry.InterpretShadesAsEffect == value) return; Entry.InterpretShadesAsEffect = value; OnPropertyChanged(); } }
            public bool TintAsAmbient { get => Entry.TintAsAmbient; set { if (Entry.TintAsAmbient == value) return; Entry.TintAsAmbient = value; OnPropertyChanged(); } }
            public bool ClearShades { get => Entry.ClearShades; set { if (Entry.ClearShades == value) return; Entry.ClearShades = value; OnPropertyChanged(); } }
        }
    }
}
