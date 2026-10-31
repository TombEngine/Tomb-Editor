#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using SoundTool.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Xml.Serialization;
using TombLib.LevelData;
using TombLib.LevelData.IO;
using TombLib.WPF.Services.Abstract;
using TombLib.Wad;

namespace SoundTool.ViewModels;

/// <summary>Coordinates catalog editing, sample preview and persistence independently of the view.</summary>
public partial class MainViewModel : ObservableObject
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private const string CatalogFilter = "Sound catalogs|*.xml;*.txt|XML sound catalogs|*.xml|Legacy sound catalogs|*.txt";
    private readonly Configuration _configuration;
    private readonly ISoundToolDialogs _dialogs;
    private readonly IMessageService _messages;
    private string? _currentArchive;
    private Level? _referenceLevel;
    private bool _saved = true;
    private bool _bulkUpdating;

    [ObservableProperty] private SoundInfoViewModel? _selectedSound;
    [ObservableProperty] private SampleViewModel? _selectedSample;
    [ObservableProperty] private string _searchText = string.Empty;

    public ObservableCollection<SoundInfoViewModel> Sounds { get; } = [];
    public ObservableCollection<string> RecentFiles { get; } = [];
    public IReadOnlyList<SoundInfoViewModel> SelectedSounds { get; set; } = [];
    public IReadOnlyList<SampleViewModel> SelectedSamples { get; set; } = [];
    public IReadOnlyList<LoopBehaviourOption> LoopBehaviours => LoopBehaviourOption.All;
    public bool HasSelection => SelectedSound is not null;
    public bool HasReferenceLevel => _referenceLevel is not null;
    public bool CanSave => _currentArchive is null || !_saved;
    public bool HasRecentFiles => RecentFiles.Count > 0;
    public string Title => "SoundTool - " + (_currentArchive ?? "Untitled") + (_saved ? string.Empty : "*");
    public string Status => "Sound Infos: " + Sounds.Count + (_referenceLevel is null ? string.Empty : " | Reference project: " + _configuration.SoundTool_ReferenceProject);

    public MainViewModel(Configuration configuration, ISoundToolDialogs dialogs, IMessageService messages)
    {
        _configuration = configuration;
        _dialogs = dialogs;
        _messages = messages;
        RefreshRecentFiles();
    }

    public void Initialize(string? archive, string? referenceLevel)
    {
        var reference = referenceLevel ?? _configuration.SoundTool_ReferenceProject;
        if (!string.IsNullOrWhiteSpace(reference))
            LoadReference(reference);

        if (archive is not null)
            Open(archive);
    }

    public bool CanClose() => ConfirmDiscard();

    [RelayCommand]
    private void New()
    {
        if (!ConfirmDiscard())
            return;

        ClearSounds();
        _currentArchive = null;
        _saved = true;
        RefreshState();
    }

    [RelayCommand]
    public void Open(string? filename = null)
    {
        if (!ConfirmDiscard())
            return;

        filename ??= _dialogs.OpenFile("Select archive to open", CatalogFilter, _configuration.SoundTool_LastCatalogPath);
        if (filename is null)
            return;

        Run(() => {
            var archive = WadSounds.ReadFromFile(filename);
            if (archive is null)
                return;

            ClearSounds();
            foreach (var sound in archive.SoundInfos.OrderBy(sound => sound.Id))
                Add(new SoundInfoViewModel(sound));

            _configuration.SoundTool_LastCatalogPath = filename;
            _saved = Path.GetExtension(filename).Equals(".xml", StringComparison.OrdinalIgnoreCase);
            _currentArchive = _saved ? filename : null;
            SelectedSound = Sounds.FirstOrDefault();
            if (_saved)
                AddRecent(filename);

            RefreshState();
        });
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save() => SaveArchive(_currentArchive);

    [RelayCommand]
    private void SaveAs() => SaveArchive(null);

    private bool SaveArchive(string? filename)
    {
        filename ??= _dialogs.SaveFile("Save sound catalog to XML", "XML sound catalog|*.xml", _currentArchive);
        if (filename is null)
            return false;

        return Run(() => {
            WadSounds.SaveToXml(filename, CreateArchive());
            _currentArchive = filename;
            _saved = true;
            AddRecent(filename);
            RefreshState();
        });
    }

    [RelayCommand]
    private void AddSound()
    {
        int nextId = 0;
        while (Sounds.Any(sound => sound.Id == nextId))
            nextId++;

        var value = _dialogs.Prompt("Add new sound info", "Insert the ID of the new sound:", nextId.ToString());
        if (value is null)
            return;

        if (!int.TryParse(value, out int id) || id < 0)
        {
            Warn("Insert a valid non-negative sound ID.");
            return;
        }

        var existing = Sounds.FirstOrDefault(sound => sound.Id == id);
        if (existing is not null)
        {
            SelectedSound = existing;
            Warn("This sound ID is already assigned to " + existing.Name + ".");
            return;
        }

        var sound = new SoundInfoViewModel(new WadSoundInfo(id));
        Add(sound);
        SelectedSound = sound;
        MarkChanged();
    }

    [RelayCommand]
    private void DeleteSounds()
    {
        SoundInfoViewModel[] selected = SelectedSounds.Count > 0 ? [.. SelectedSounds] : SelectedSound is null ? [] : [SelectedSound];
        if (selected.Length == 0 || !_messages.ShowConfirmation("Delete the selected sound infos?", "Delete sounds", isRisky: true))
            return;

        foreach (var sound in selected)
        {
            sound.Changed -= SoundChanged;
            Sounds.Remove(sound);
        }

        SelectedSound = Sounds.FirstOrDefault();
        MarkChanged();
    }
    [RelayCommand]
    private void Search()
    {
        if (Sounds.Count == 0)
        {
            _messages.ShowInformation("No sounds present. Nothing to search.", "SoundTool");
            return;
        }

        int start = SelectedSound is null ? -1 : Sounds.IndexOf(SelectedSound);
        for (int offset = 1; offset <= Sounds.Count; offset++)
        {
            var sound = Sounds[(start + offset) % Sounds.Count];
            if (sound.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || sound.Id.ToString().Contains(SearchText))
            {
                SelectedSound = sound;
                return;
            }
        }
    }

    [RelayCommand]
    private void LoadReference(string? filename = null)
    {
        filename ??= _dialogs.OpenFile("Open Tomb Editor reference project", "Tomb Editor project|*.prj2");
        if (filename is null || !File.Exists(filename))
            return;

        Run(() => {
            _referenceLevel = Prj2Loader.LoadFromPrj2(filename, null, CancellationToken.None,
                new Prj2Loader.Settings { IgnoreTextures = true, IgnoreWads = true });
            _configuration.SoundTool_ReferenceProject = filename;
            RefreshState();
        });
    }

    [RelayCommand]
    private void UnloadReference()
    {
        _referenceLevel = null;
        _configuration.SoundTool_ReferenceProject = string.Empty;
        RefreshState();
    }

    [RelayCommand]
    private void Play()
    {
        if (_referenceLevel is not null && SelectedSound is not null)
            Run(() => WadSoundPlayer.PlaySoundInfo(_referenceLevel, SelectedSound.ToSound()));
    }

    [RelayCommand]
    private void Stop() => WadSoundPlayer.StopSample();

    [RelayCommand]
    private void AddSample()
    {
        if (SelectedSound is null)
            return;

        var name = _dialogs.Prompt("New sample", "Insert the sample filename without the .wav extension:");
        if (!string.IsNullOrWhiteSpace(name))
        {
            SelectedSound.Samples.Add(new SampleViewModel(name.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) ? name : name + ".wav"));
            ResolveSamples();
        }
    }

    [RelayCommand]
    private void BrowseSamples()
    {
        if (SelectedSound is null)
            return;

        foreach (var file in _dialogs.OpenSamples(GetFirstReferenceSamplePath()))
        {
            var name = Path.GetFileName(file);
            if (!SelectedSound.Samples.Any(sample => sample.FileName.Equals(name, StringComparison.OrdinalIgnoreCase)))
                SelectedSound.Samples.Add(new SampleViewModel(name));
        }

        ResolveSamples();
        if (HasReferenceLevel && SelectedSound.Samples.Any(sample => sample.ResolvedPath == "[not found]"))
            Warn("Some samples are outside the reference project's sample paths. Copy them to a sample path or update the reference project.");
    }

    private string? GetFirstReferenceSamplePath()
    {
        var settings = _referenceLevel?.Settings;
        var firstPath = settings?.WadSoundPaths.FirstOrDefault();

        return settings is null || firstPath is null ? null : settings.MakeAbsolute(firstPath.Path);
    }

    [RelayCommand]
    private void DeleteSample()
    {
        if (SelectedSound is null)
            return;

        SampleViewModel[] selected = SelectedSamples.Count > 0 ? [.. SelectedSamples] : SelectedSample is null ? [] : [SelectedSample];
        foreach (var sample in selected)
            SelectedSound.Samples.Remove(sample);
    }
    [RelayCommand]
    private void Copy()
    {
        if (SelectedSound is null)
            return;

        Run(() => {
            using var stream = new MemoryStream();
            new XmlSerializer(typeof(WadSoundInfo)).Serialize(stream, SelectedSound.ToSound());
            Clipboard.SetData("SoundInfo", stream.ToArray());
        });
    }

    [RelayCommand]
    private void Paste()
    {
        if (SelectedSound is null)
            return;

        Run(() => {
            if (Clipboard.GetData("SoundInfo") is not byte[] data)
                return;

            using var stream = new MemoryStream(data, false);
            if (new XmlSerializer(typeof(WadSoundInfo)).Deserialize(stream) is WadSoundInfo sound)
            {
                sound.Id = SelectedSound.Id;
                ReplaceSelected(sound);
            }
        });
    }

    [RelayCommand]
    private void Reset()
    {
        if (SelectedSound is null)
            return;

        var sound = new WadSoundInfo(SelectedSound.Id)
        {
            Name = SelectedSound.Name,
            Samples = [.. SelectedSound.Samples.Select(sample => new WadSample(sample.FileName))]
        };
        ReplaceSelected(sound);
    }

    [RelayCommand]
    private void IndexAll() => SetIndexed(true);

    [RelayCommand]
    private void UnindexAll() => SetIndexed(false);

    [RelayCommand]
    private void ClearRecent()
    {
        Properties.Settings.Default.RecentProjects?.Clear();
        Properties.Settings.Default.Save();
        RefreshRecentFiles();
    }

    [RelayCommand]
    private void Options() => _dialogs.ShowOptions(_configuration);

    [RelayCommand]
    private void About() => _dialogs.ShowAbout();

    [RelayCommand]
    private void Exit() => _dialogs.Close();

    partial void OnSelectedSoundChanged(SoundInfoViewModel? value)
    {
        SelectedSample = null;
        OnPropertyChanged(nameof(HasSelection));
        ResolveSamples();
    }

    private bool ConfirmDiscard()
    {
        if (_saved)
            return true;

        var result = _messages.ShowConfirmation("Save changes to the current catalog?", "Unsaved changes", 1, 0, -1, 1);
        return result == 0 || result == 1 && SaveArchive(_currentArchive);
    }

    private WadSounds CreateArchive()
    {
        var archive = new WadSounds();
        archive.SoundInfos.AddRange(Sounds.OrderBy(sound => sound.Id).Select(sound => sound.ToSound()));
        return archive;
    }

    private void Add(SoundInfoViewModel sound)
    {
        sound.Changed += SoundChanged;
        Sounds.Add(sound);
    }

    private void ClearSounds()
    {
        foreach (var sound in Sounds)
            sound.Changed -= SoundChanged;

        Sounds.Clear();
        SelectedSound = null;
    }

    private void ReplaceSelected(WadSoundInfo sound)
    {
        if (SelectedSound is null)
            return;

        int index = Sounds.IndexOf(SelectedSound);
        SelectedSound.Changed -= SoundChanged;
        var replacement = new SoundInfoViewModel(sound);
        replacement.Changed += SoundChanged;
        Sounds[index] = replacement;
        SelectedSound = replacement;
        MarkChanged();
    }

    private void SetIndexed(bool indexed)
    {
        if (Sounds.All(sound => sound.Indexed == indexed))
            return;

        _bulkUpdating = true;
        try
        {
            foreach (var sound in Sounds)
                sound.Indexed = indexed;
        }
        finally
        {
            _bulkUpdating = false;
        }

        MarkChanged();
    }

    private void SoundChanged(object? sender, EventArgs e)
    {
        if (_bulkUpdating)
            return;

        MarkChanged();
        if (ReferenceEquals(sender, SelectedSound))
            ResolveSamples();
    }

    private void MarkChanged()
    {
        _saved = false;
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Status));
        RefreshSaveState();
    }

    private void RefreshSaveState()
    {
        OnPropertyChanged(nameof(CanSave));
        SaveCommand.NotifyCanExecuteChanged();
    }

    private void RefreshState()
    {
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(HasReferenceLevel));
        RefreshSaveState();
        ResolveSamples();
        RefreshRecentFiles();
    }

    private void ResolveSamples()
    {
        if (SelectedSound is null)
            return;

        var paths = _referenceLevel?.Settings.GetRecursiveListOfSoundPaths();
        foreach (var sample in SelectedSound.Samples)
            sample.ResolvedPath = paths is null ? string.Empty : WadSounds.TryGetSamplePath(paths, sample.FileName) ?? "[not found]";
    }

    private void AddRecent(string filename)
    {
        Properties.Settings.Default.RecentProjects ??= [];
        var recent = Properties.Settings.Default.RecentProjects;
        recent.RemoveAll(path => path.Equals(filename, StringComparison.OrdinalIgnoreCase));
        recent.Insert(0, filename);
        if (recent.Count > 10)
            recent.RemoveRange(10, recent.Count - 10);

        Properties.Settings.Default.Save();
        RefreshRecentFiles();
    }

    private void RefreshRecentFiles()
    {
        RecentFiles.Clear();
        OnPropertyChanged(nameof(HasRecentFiles));
        if (Properties.Settings.Default.RecentProjects is not { } recent)
            return;

        foreach (var file in recent.Where(file => file != _currentArchive && File.Exists(file)))
            RecentFiles.Add(file);

        OnPropertyChanged(nameof(HasRecentFiles));
    }

    private bool Run(Action action)
    {
        try
        {
            action();
            return true;
        }
        catch (Exception exception)
        {
            Logger.Error(exception, "SoundTool operation failed.");
            _messages.ShowError(exception.Message, "SoundTool");
            return false;
        }
    }

    private void Warn(string message)
    {
        Logger.Warn(message);
        _messages.ShowInformation(message, "SoundTool");
    }
}
