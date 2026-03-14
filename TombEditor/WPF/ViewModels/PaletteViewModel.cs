using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Windows.Input;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.WPF.ViewModels;

public partial class PaletteViewModel : ObservableObject
{
    private readonly Editor _editor;

    [ObservableProperty] private bool _isTextureSamplingMode;
    [ObservableProperty] private bool _canResetPalette;
    [ObservableProperty] private bool _canEditPalette;
    [ObservableProperty] private bool _canEditColor;

    public ICommand ResetPaletteCommand { get; }
    public ICommand SampleFromTexturesCommand { get; }
    public ICommand EditObjectColorCommand { get; }

    private List<ColorC> _lastTexturePalette;

    public PaletteViewModel(Editor editor)
    {
        _editor = editor;
        _editor.EditorEventRaised += EditorEventRaised;

        var args = new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor);
        ResetPaletteCommand = CommandHandler.GetCommand("ResetPalette", args);
        SampleFromTexturesCommand = CommandHandler.GetCommand("SamplePaletteFromTextures", args);
        EditObjectColorCommand = CommandHandler.GetCommand("EditObjectColor", args);

        UpdateControlState();
    }

    public void Cleanup()
    {
        _editor.EditorEventRaised -= EditorEventRaised;
    }

    private void EditorEventRaised(IEditorEvent obj)
    {
        if (obj is Editor.SelectedObjectChangedEvent selectedObjChanged)
            CanEditColor = selectedObjChanged.Current.CanBeColored();

        if (obj is Editor.SelectedLevelTextureChangedSetEvent textureChanged)
        {
            _lastTexturePalette = new List<ColorC>(textureChanged.Texture.Image.Palette);
            UpdateControlState();
        }

        if (obj is Editor.ConfigurationChangedEvent)
            UpdateControlState();
    }

    public List<ColorC> GetLastTexturePalette() => _lastTexturePalette;

    private void UpdateControlState()
    {
        bool useTexture = _editor.Configuration.Palette_TextureSamplingMode;

        IsTextureSamplingMode = useTexture;
        CanResetPalette = !useTexture;
        CanEditPalette = !useTexture;
    }
}
