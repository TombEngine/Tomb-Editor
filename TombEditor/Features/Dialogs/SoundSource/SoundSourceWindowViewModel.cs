#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TombLib.LevelData;
using TombLib.Wad;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.SoundSource;

public sealed record SoundEntry(WadSoundInfo Info, string DisplayName);

public sealed record SoundPlayModeItem(SoundSourcePlayMode Mode, string DisplayName);

public partial class SoundSourceWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly SoundSourceInstance _soundSource;
    private readonly Editor _editor;
    private readonly IMessageService _messageService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private string _searchText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PlayCommand))]
    private SoundEntry? _selectedSound;

    [ObservableProperty] private SoundPlayModeItem _selectedPlayMode;

    public ObservableCollection<SoundEntry> Sounds { get; } = new();
    public IReadOnlyList<SoundPlayModeItem> PlayModes { get; }

    public SoundSourceWindowViewModel(
        SoundSourceInstance soundSource,
        Editor? editor = null,
        IMessageService? messageService = null,
        ILocalizationService? localizationService = null)
    {
        _soundSource = soundSource;
        _editor = editor ?? Editor.Instance;
        _messageService = ServiceLocator.ResolveService(messageService);
        _localizationService = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        PlayModes = new List<SoundPlayModeItem>
        {
            new(SoundSourcePlayMode.Always, _localizationService["PlayModeAlways"]),
            new(SoundSourcePlayMode.OnlyInBaseRoom, _localizationService["PlayModeFlipmapsOff"]),
            new(SoundSourcePlayMode.OnlyInAlternateRoom, _localizationService["PlayModeFlipmapsOn"]),
            new(SoundSourcePlayMode.Automatic, _localizationService["PlayModeAuto"])
        };

        _selectedPlayMode = PlayModes.First(p => p.Mode == soundSource.PlayMode);

        foreach (WadSoundInfo info in _editor.Level.Settings.GlobalSoundMap.OrderBy(s => s.Id))
            Sounds.Add(new SoundEntry(info, $"{info.Id.ToString().PadLeft(4, '0')}: {info.Name}"));

        _selectedSound = Sounds.FirstOrDefault(s => s.Info.Id == soundSource.SoundId);
    }

    [RelayCommand]
    private void Search()
    {
        if (string.IsNullOrEmpty(SearchText))
            return;

        // Wrap-around case-insensitive search starting just after the current selection.
        int start = SelectedSound is null ? 0 : (Sounds.IndexOf(SelectedSound) + 1) % Sounds.Count;

        for (int offset = 0; offset < Sounds.Count; offset++)
        {
            int index = (start + offset) % Sounds.Count;

            if (Sounds[index].DisplayName.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase))
            {
                SelectedSound = Sounds[index];
                return;
            }
        }
    }

    private bool CanPlay() => SelectedSound is not null;

    [RelayCommand(CanExecute = nameof(CanPlay))]
    private void Play()
    {
        if (SelectedSound is null)
            return;

        try
        {
            WadSoundPlayer.PlaySoundInfo(_editor.Level, SelectedSound.Info);
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, "Unable to play sample");
            _messageService.ShowError(_localizationService.Format("PlayFailedMessage", ex.Message));
        }
    }

    [RelayCommand]
    private void Confirm()
    {
        _soundSource.SoundId = SelectedSound?.Info.Id ?? -1;
        _soundSource.PlayMode = SelectedPlayMode.Mode;
        WadSoundPlayer.StopSample();
        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel()
    {
        WadSoundPlayer.StopSample();
        DialogResult = false;
    }
}
