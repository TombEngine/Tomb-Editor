#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.FootStepSounds;

public partial class FootStepSoundsWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly Editor _editor;

    [ObservableProperty] private bool? _dialogResult;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAssign))]
    private LevelTexture? _selectedTexture;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAssign))]
    private int _selectedSoundIndex;

    public ObservableCollection<LevelTexture> Textures { get; } = new();
    public IReadOnlyList<string> SoundTypes { get; }

    public bool CanAssign => SelectedSoundIndex >= 0 && SelectedTexture is not null;

    public event EventHandler<LevelTexture?>? RequestResetVisibleTexture;
    public event EventHandler? RequestInvalidate;

    /// <summary>Set by the view: the current selection of the dialog's own texture map
    /// (the legacy form used textureMap.SelectedTexture, not the editor-wide selection).</summary>
    public Func<TextureArea>? MapSelectionProvider { get; set; }

    public FootStepSoundsWindowViewModel(
        LevelTexture? texture,
        Editor? editor = null,
        ILocalizationService? localizationService = null)
    {
        _editor = editor ?? Editor.Instance;
        _ = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        SoundTypes = TextureFootStep.GetNames(_editor.Level.Settings).ToList();

        foreach (var tex in _editor.Level.Settings.Textures)
            Textures.Add(tex);

        _selectedTexture = texture ?? Textures.FirstOrDefault();
        _selectedSoundIndex = 0;
    }

    partial void OnSelectedTextureChanged(LevelTexture? value)
    {
        RequestResetVisibleTexture?.Invoke(this, value);
    }

    [RelayCommand]
    private void AssignSound()
    {
        if (SelectedTexture is not { } texture || SelectedSoundIndex < 0)
            return;

        TextureArea selected = MapSelectionProvider?.Invoke() ?? TextureArea.None;
        if (selected.TextureIsInvisible)
            return;

        var sound = (TextureFootStep.Type)SelectedSoundIndex;

        Vector2 p0 = selected.TexCoord0 / LevelTexture.FootStepSoundGranularity;
        Vector2 p1 = selected.TexCoord1 / LevelTexture.FootStepSoundGranularity;
        Vector2 p2 = selected.TexCoord2 / LevelTexture.FootStepSoundGranularity;
        Vector2 p3 = selected.TexCoord3 / LevelTexture.FootStepSoundGranularity;

        int xMin = (int)Math.Min(Math.Min(Math.Min(p0.X, p1.X), p2.X), p3.X);
        int xMax = (int)Math.Max(Math.Max(Math.Max(p0.X, p1.X), p2.X), p3.X);
        int yMin = (int)Math.Min(Math.Min(Math.Min(p0.Y, p1.Y), p2.Y), p3.Y);
        int yMax = (int)Math.Max(Math.Max(Math.Max(p0.Y, p1.Y), p2.Y), p3.Y);

        for (int y = yMin; y < yMax; y++)
            for (int x = xMin; x < xMax; x++)
                texture.SetFootStepSound(x, y, sound);

        RequestInvalidate?.Invoke(this, EventArgs.Empty);
        _editor.TextureSoundsChange();
    }

    [RelayCommand]
    private void Confirm()
    {
        DialogResult = true;
    }
}
