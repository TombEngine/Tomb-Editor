#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using TombLib.Wad;

namespace SoundTool.ViewModels;

/// <summary>Editable sound definition with change tracking for its samples and parameters.</summary>
public partial class SoundInfoViewModel : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private int _volume;
    [ObservableProperty] private int _pitchFactor;
    [ObservableProperty] private int _rangeInSectors;
    [ObservableProperty] private int _chance;
    [ObservableProperty] private bool _randomizeVolume;
    [ObservableProperty] private bool _randomizePitch;
    [ObservableProperty] private bool _disablePanning;
    [ObservableProperty] private bool _global;
    [ObservableProperty] private bool _indexed;
    [ObservableProperty] private WadSoundLoopBehaviour _loopBehaviour;

    public int Id { get; }
    public ObservableCollection<SampleViewModel> Samples { get; } = [];
    public event EventHandler? Changed;

    public SoundInfoViewModel(WadSoundInfo sound)
    {
        Id = sound.Id;
        _name = sound.Name;
        _volume = sound.Volume;
        _pitchFactor = sound.PitchFactor;
        _rangeInSectors = sound.RangeInSectors;
        _chance = sound.Chance;
        _randomizeVolume = sound.RandomizeVolume;
        _randomizePitch = sound.RandomizePitch;
        _disablePanning = sound.DisablePanning;
        _global = sound.Global;
        _indexed = sound.Indexed;
        _loopBehaviour = sound.LoopBehaviour;

        foreach (var sample in sound.Samples)
            Samples.Add(new SampleViewModel(sample.FileName));

        Samples.CollectionChanged += SamplesChanged;
        foreach (var sample in Samples)
            sample.PropertyChanged += SampleChanged;
    }

    public WadSoundInfo ToSound() => new(Id)
    {
        Name = Name,
        Volume = Volume,
        PitchFactor = PitchFactor,
        RangeInSectors = RangeInSectors,
        Chance = Chance,
        RandomizeVolume = RandomizeVolume,
        RandomizePitch = RandomizePitch,
        DisablePanning = DisablePanning,
        Global = Global,
        Indexed = Indexed,
        LoopBehaviour = LoopBehaviour,
        Samples = [.. Samples.Select(sample => new WadSample(sample.FileName))]
    };

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void SamplesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (SampleViewModel sample in e.OldItems)
                sample.PropertyChanged -= SampleChanged;

        if (e.NewItems is not null)
            foreach (SampleViewModel sample in e.NewItems)
                sample.PropertyChanged += SampleChanged;

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void SampleChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SampleViewModel.FileName))
            Changed?.Invoke(this, EventArgs.Empty);
    }
}
