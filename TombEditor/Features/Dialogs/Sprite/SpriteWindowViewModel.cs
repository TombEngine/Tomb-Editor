#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using TombLib.LevelData;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using TombLib.WPF;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Imaging;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.Sprite;

public partial class SpriteWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly Editor _editor;
    private readonly SpriteInstance _instance;
    private List<WadSprite> _previewSprites = [];

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private ImageSource? _previewImage;
    [ObservableProperty] private Brush _previewBackground;
    [ObservableProperty] private int _selectedSpriteIndex = -1;

    /// <summary>Gets the names of available sprite frames.</summary>
    public ObservableCollection<string> Sprites { get; } = [];

    public SpriteWindowViewModel(SpriteInstance instance, ILocalizationService? localizationService = null)
    {
        _instance = instance;
        _editor = Editor.Instance;
        _previewBackground = _editor.Configuration.UI_ColorScheme.Color3DBackground.ToWPFBrush();
        _ = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);
        _editor.EditorEventRaised += OnEditorEvent;
        Populate();
    }

    partial void OnSelectedSpriteIndexChanged(int value) => UpdatePreview();

    private void OnEditorEvent(IEditorEvent editorEvent)
    {
        if (editorEvent is Editor.LoadedWadsChangedEvent or Editor.LevelChangedEvent)
            Populate();
        else if (editorEvent is Editor.ConfigurationChangedEvent)
            PreviewBackground = _editor.Configuration.UI_ColorScheme.Color3DBackground.ToWPFBrush();
    }

    private void Populate()
    {
        Sprites.Clear();
        _previewSprites = _editor.Level.Settings.WadGetAllSprites();
        var sequences = _editor.Level.Settings.WadGetAllSpriteSequences();
        var version = _editor.Level.Settings.GameVersion;

        foreach (var sequence in sequences.Values)
        {
            string name = TrCatalog.GetSpriteSequenceName(version, sequence.Id.TypeId);

            for (int frame = 0; frame < sequence.Sprites.Count; frame++)
                Sprites.Add($"{name}, Frame {frame}");
        }

        SelectedSpriteIndex = _instance.SpriteID >= 0 && _instance.SpriteID < Sprites.Count ? _instance.SpriteID : (Sprites.Count > 0 ? 0 : -1);
        UpdatePreview();
    }

    private void UpdatePreview() => PreviewImage = SelectedSpriteIndex >= 0 && SelectedSpriteIndex < _previewSprites.Count
        ? ImageSourceFactory.FromImage(_previewSprites[SelectedSpriteIndex].Texture.Image)
        : null;

    /// <summary>Releases the editor subscription when the dialog closes.</summary>
    public void Detach() => _editor.EditorEventRaised -= OnEditorEvent;

    [RelayCommand]
    private void Confirm()
    {
        if (SelectedSpriteIndex >= 0)
            _instance.SetSequenceAndFrame(SelectedSpriteIndex);

        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel() => DialogResult = false;
}
