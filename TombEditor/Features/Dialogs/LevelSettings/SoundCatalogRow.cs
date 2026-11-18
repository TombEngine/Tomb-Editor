#nullable enable

using CommunityToolkit.Mvvm.Input;
using NLog;
using System;
using System.Diagnostics;
using System.IO;
using TombLib.LevelData;
using LevelSettingsData = TombLib.LevelData.LevelSettings;

namespace TombEditor.Features.Dialogs.LevelSettings;

/// <summary>Row view-model for the Sound catalogs grid (wraps a <see cref="ReferencedSoundCatalog"/>).</summary>
public partial class SoundCatalogRow : ReloadableRow
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly LevelSettingsData _settings;
    private readonly Action? _changed;
    private readonly Action<ReferencedSoundCatalog>? _includeAll;

    public ReferencedSoundCatalog Catalog { get; private set; }

    public SoundCatalogRow(LevelSettingsData settings, ReferencedSoundCatalog catalog, Action? changed = null, Action<ReferencedSoundCatalog>? includeAll = null)
    {
        _settings = settings;
        _changed = changed;
        _includeAll = includeAll;
        Catalog = catalog;
    }

    public string Path
    {
        get => Catalog.Path;
        set
        {
            if (Catalog.Path == value)
                return;

            Catalog = new ReferencedSoundCatalog(_settings, value);
            OnPropertyChanged(string.Empty);
            _changed?.Invoke();
        }
    }

    public bool IsValid => Catalog.LoadException is null;
    public string Message => Catalog.LoadException is null ? LevelSettingsStrings.Get("StatusLoaded") : Catalog.LoadException.Message + " (" + Catalog.LoadException.GetType().Name + ")";
    public int SoundsCount => Catalog.LoadException is null ? Catalog.Sounds.SoundInfos.Count : 0;

    /// <summary>Gets whether the catalog is an XML file that can be edited in SoundTool.</summary>
    public bool CanEdit => Catalog.Path?.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase) ?? false;

    [RelayCommand]
    private void Browse() => ReloadFromNewFile(_settings, Catalog, _changed);

    [RelayCommand]
    private void Reload()
    {
        Catalog.Reload(_settings);
        OnPropertyChanged(string.Empty);
        _changed?.Invoke();
    }

    [RelayCommand]
    private void IncludeAll() => _includeAll?.Invoke(Catalog);

    [RelayCommand]
    private void Edit()
    {
        string path = _settings.MakeAbsolute(Catalog.Path);

        if (!File.Exists(DefaultPaths.SoundToolExecutable) || !File.Exists(path))
            return;

        try
        {
            string arguments = "\"" + path + "\"";

            if (!string.IsNullOrEmpty(_settings.LevelFilePath))
                arguments += " -r \"" + _settings.MakeAbsolute(_settings.LevelFilePath) + "\"";

            Process.Start(DefaultPaths.SoundToolExecutable, arguments);
        }
        catch (Exception exception)
        {
            Logger.Warn(exception, "Unable to start SoundTool.");
        }
    }
}
