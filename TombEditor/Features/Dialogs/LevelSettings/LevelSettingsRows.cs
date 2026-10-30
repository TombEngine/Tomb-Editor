#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelSettingsData = TombLib.LevelData.LevelSettings;
using TombLib.GeometryIO;
using TombLib.LevelData;
using TombLib.WPF;
using TombLib.WPF.Services;

namespace TombEditor.Features.Dialogs.LevelSettings
{
    /// <summary>
    /// Localized strings for the row view-models in this file. The rows deliberately resolve their
    /// keys from the parent dialog's "LevelSettings" section (via absolute <see cref="Localizer"/>
    /// keys) instead of per-class sections, so all Level Settings strings live in one place.
    /// </summary>
    internal static class LevelSettingsStrings
    {
        internal static string Get(string key) => Localizer.Instance["TombEditor.LevelSettings." + key];
    }

    /// <summary>Row view-model for the Textures grid (wraps a <see cref="LevelTexture"/>).</summary>
    public partial class TextureRow : ReloadableRow
    {
        private readonly LevelSettingsData _settings;
        public LevelTexture Texture { get; private set; }

        /// <summary>Gets whether the texture is loaded and can be shown in the preview window.</summary>
        public bool CanPreview => Texture.LoadException == null;

        public TextureRow(LevelSettingsData settings, LevelTexture texture)
        {
            _settings = settings;
            Texture = texture;
        }

        public string Path
        {
            get => Texture.Path;
            set
            {
                if (Texture.Path == value)
                    return;
                Texture = (LevelTexture)Texture.Clone();
                Texture.SetPath(_settings, value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(Message));
                OnPropertyChanged(nameof(Size));
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public bool IsValid => Texture.LoadException == null;

        public bool ReplaceMagentaWithTransparency
        {
            get => Texture.ReplaceMagentaWithTransparency;
            set
            {
                if (Texture.ReplaceMagentaWithTransparency == value)
                    return;
                Texture = (LevelTexture)Texture.Clone();
                Texture.SetReplaceMagentaWithTransparency(_settings, value);
                OnPropertyChanged();
            }
        }

        public bool Convert512PixelsToDoubleRows
        {
            get => Texture.Convert512PixelsToDoubleRows;
            set
            {
                if (Texture.Convert512PixelsToDoubleRows == value)
                    return;
                Texture = (LevelTexture)Texture.Clone();
                Texture.SetConvert512PixelsToDoubleRows(_settings, value);
                OnPropertyChanged();
            }
        }

        public string Message => Texture.LoadException == null ? LevelSettingsStrings.Get("StatusLoaded") : Texture.LoadException.Message + " (" + Texture.LoadException.GetType().Name + ")";
        public string Size => Texture.LoadException != null || Texture.Image == TombLib.Utils.Texture.UnloadedPlaceholder ? "-" : Texture.Image.Width + " x " + Texture.Image.Height;

        [RelayCommand]
        private void Browse() => ReloadFromNewFile(_settings, Texture);
    }

    /// <summary>Row view-model for the Objects grid (wraps a <see cref="ReferencedWad"/>).</summary>
    public partial class WadRow : ReloadableRow
    {
        private readonly LevelSettingsData _settings;
        public ReferencedWad Wad { get; private set; }

        public WadRow(LevelSettingsData settings, ReferencedWad wad)
        {
            _settings = settings;
            Wad = wad;
        }

        public string Path
        {
            get => Wad.Path;
            set
            {
                if (Wad.Path == value)
                    return;
                Wad = new ReferencedWad(_settings, value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(Message));
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public bool IsValid => Wad.LoadException == null
            && !(Wad.Wad != null && (Wad.Wad.GameVersion.Native() != _settings.GameVersion.Native() || Wad.Wad.HasUnknownData));

        /// <summary>Gets whether the wad is loaded and can be shown in the preview window.</summary>
        public bool CanPreview => Wad.LoadException == null && Wad.Wad != null;

        public string Message
        {
            get
            {
                if (Wad.Wad != null && Wad.Wad.GameVersion.Native() != _settings.GameVersion.Native())
                    return LevelSettingsStrings.Get("StatusVersionMismatch");
                if (Wad.Wad != null && Wad.Wad.HasUnknownData)
                    return LevelSettingsStrings.Get("StatusUnknownData");
                if (Wad.LoadException == null)
                    return LevelSettingsStrings.Get("StatusLoaded");
                return Wad.LoadException.Message + " (" + Wad.LoadException.GetType().Name + ")";
            }
        }

        [RelayCommand]
        private void Browse() => ReloadFromNewFile(_settings, Wad);
    }

    /// <summary>Row view-model for the Imported geometry grid (wraps an <see cref="ImportedGeometry"/>).</summary>
    public partial class ImportedGeometryRow : ObservableObject
    {
        private readonly LevelSettingsData _settings;
        public ImportedGeometry Object { get; }

        public ImportedGeometryRow(LevelSettingsData settings, ImportedGeometry obj)
        {
            _settings = settings;
            Object = obj;
        }

        private void Update(System.Func<ImportedGeometryInfo, ImportedGeometryInfo> change)
        {
            _settings.ImportedGeometryUpdate(Object, change(Object.Info));
            OnPropertyChanged(string.Empty); // refresh all displayed columns
        }

        public string Name => Object.Info.Name;
        public bool IsValid => Object.LoadException == null;
        public string ErrorMessage => Object.LoadException == null ? LevelSettingsStrings.Get("StatusLoaded") : Object.LoadException.Message + " (" + Object.LoadException.GetType().Name + ")";

        public string Path { get => Object.Info.Path; set { if (Object.Info.Path == value) return; Update(i => { i.Path = value; return i; }); } }
        public float Scale { get => Object.Info.Scale; set { if (Object.Info.Scale == value) return; Update(i => { i.Scale = value; return i; }); } }
        public bool SwapXY { get => Object.Info.SwapXY; set { if (Object.Info.SwapXY == value) return; Update(i => { i.SwapXY = value; return i; }); } }
        public bool SwapXZ { get => Object.Info.SwapXZ; set { if (Object.Info.SwapXZ == value) return; Update(i => { i.SwapXZ = value; return i; }); } }
        public bool SwapYZ { get => Object.Info.SwapYZ; set { if (Object.Info.SwapYZ == value) return; Update(i => { i.SwapYZ = value; return i; }); } }
        public bool FlipX { get => Object.Info.FlipX; set { if (Object.Info.FlipX == value) return; Update(i => { i.FlipX = value; return i; }); } }
        public bool FlipY { get => Object.Info.FlipY; set { if (Object.Info.FlipY == value) return; Update(i => { i.FlipY = value; return i; }); } }
        public bool FlipZ { get => Object.Info.FlipZ; set { if (Object.Info.FlipZ == value) return; Update(i => { i.FlipZ = value; return i; }); } }
        public bool MappedUV { get => Object.Info.MappedUV; set { if (Object.Info.MappedUV == value) return; Update(i => { i.MappedUV = value; return i; }); } }
        public bool FlipUV_V { get => Object.Info.FlipUV_V; set { if (Object.Info.FlipUV_V == value) return; Update(i => { i.FlipUV_V = value; return i; }); } }
        public bool InvertFaces { get => Object.Info.InvertFaces; set { if (Object.Info.InvertFaces == value) return; Update(i => { i.InvertFaces = value; return i; }); } }

        [RelayCommand]
        private void Browse()
        {
            string? result = LevelFileDialog.BrowseFile(WPFUtils.GetWindowOwner(), _settings, Object.Info.Path, LevelSettingsStrings.Get("Browse3DFileTitle"), BaseGeometryImporter.FileExtensions, VariableType.LevelDirectory, false);
            if (result != null)
                Path = result;
        }
    }

    /// <summary>Row view-model for the selected-sounds grid in the Sound catalogs tab.</summary>
    public partial class SoundInfoRow : ObservableObject
    {
        private readonly System.Collections.Generic.List<int> _selectedSounds;
        private readonly System.Action _onSelectionChanged;

        public SoundInfoRow(System.Collections.Generic.List<int> selectedSounds, System.Action onSelectionChanged,
            int id, string name, string catalog, string samples, string area, int originalId, bool isMissing)
        {
            _selectedSounds = selectedSounds;
            _onSelectionChanged = onSelectionChanged;
            Id = id;
            Name = name;
            Catalog = catalog;
            Samples = samples;
            Area = area;
            OriginalId = originalId;
            IsMissing = isMissing;
        }

        public int Id { get; }
        public string Name { get; }
        public string Catalog { get; }
        public string Samples { get; }
        public string Area { get; }
        public int OriginalId { get; }
        public bool IsMissing { get; }

        public bool Selected
        {
            get => _selectedSounds.Contains(Id);
            set
            {
                bool current = _selectedSounds.Contains(Id);
                if (current == value)
                    return;
                if (value)
                    _selectedSounds.Add(Id);
                else
                    _selectedSounds.Remove(Id);
                OnPropertyChanged();
                _onSelectionChanged();
            }
        }
    }

    /// <summary>Row view-model for the Samples grid (wraps a sound folder <see cref="WadSoundPath"/>).</summary>
    public partial class SampleRow : ObservableObject
    {
        private readonly LevelSettingsData _settings;
        public WadSoundPath SoundPath { get; private set; }

        public SampleRow(LevelSettingsData settings, WadSoundPath soundPath)
        {
            _settings = settings;
            SoundPath = soundPath;
        }

        public string Path
        {
            get => SoundPath.Path;
            set
            {
                if (SoundPath.Path == value)
                    return;
                SoundPath = new WadSoundPath(value);
                OnPropertyChanged();
            }
        }

        [RelayCommand]
        private void Browse()
        {
            string? result = LevelFileDialog.BrowseFolder(WPFUtils.GetWindowOwner(), _settings, SoundPath.Path, LevelSettingsStrings.Get("BrowseSoundFolderTitle"), VariableType.LevelDirectory);
            if (result != null)
                Path = result;
        }
    }
}
