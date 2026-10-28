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

namespace TombEditor.Features.Dialogs.BumpMaps;

public partial class BumpMapsWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly Editor _editor;
    private readonly IDialogService _dialogService;
    private readonly IMessageService _messageService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty] private bool? _dialogResult;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCustomMapped))]
    [NotifyPropertyChangedFor(nameof(CustomPathText))]
    [NotifyPropertyChangedFor(nameof(IsAssignEnabled))]
    private LevelTexture? _selectedTexture;

    [ObservableProperty] private BumpMappingLevel _selectedLevel = BumpMappingLevel.None;

    public ObservableCollection<LevelTexture> Textures { get; } = new();
    public IReadOnlyList<BumpMappingLevel> Levels { get; }

    public bool IsCustomMapped => SelectedTexture is { } t && !string.IsNullOrEmpty(t.BumpPath);

    public bool IsAssignEnabled => SelectedTexture is not null && !IsCustomMapped;

    public string CustomPathText => SelectedTexture is { } t && !string.IsNullOrEmpty(t.BumpPath)
        ? _editor.Level.Settings.MakeAbsolute(t.BumpPath) ?? string.Empty
        : _localizationService["NoCustomFile"];

    public event EventHandler<LevelTexture?>? RequestResetVisibleTexture;
    public event EventHandler? RequestInvalidate;

    /// <summary>Set by the view: the current selection of the dialog's own texture map
    /// (the legacy form used textureMap.SelectedTexture, not the editor-wide selection).</summary>
    public Func<TextureArea>? MapSelectionProvider { get; set; }

    public BumpMapsWindowViewModel(
        LevelTexture? texture,
        Editor? editor = null,
        IDialogService? dialogService = null,
        IMessageService? messageService = null,
        ILocalizationService? localizationService = null)
    {
        _editor = editor ?? Editor.Instance;
        _dialogService = ServiceLocator.ResolveService(dialogService);
        _messageService = ServiceLocator.ResolveService(messageService);
        _localizationService = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        Levels = Enum.GetValues<BumpMappingLevel>().ToList();

        foreach (var tex in _editor.Level.Settings.Textures)
            Textures.Add(tex);

        _selectedTexture = texture ?? Textures.FirstOrDefault();
    }

    partial void OnSelectedTextureChanged(LevelTexture? value)
    {
        RequestResetVisibleTexture?.Invoke(this, value);
    }

    [RelayCommand]
    private void ToggleCustomFile()
    {
        if (SelectedTexture is not { } texture)
            return;

        if (IsCustomMapped)
        {
            texture.BumpPath = null;
        }
        else
        {
            var settings = new OpenFileDialogSettings
            {
                Multiselect = false,
                Title = _localizationService["BrowseCustomTitle"],
                Filter = ImageC.FileExtensions.GetFilter(),
                CheckFileExists = true
            };

            if (!string.IsNullOrWhiteSpace(texture.Path))
                settings.InitialDirectory = _editor.Level.Settings.MakeAbsolute(texture.Path) ?? texture.Path;

            if (_dialogService.ShowOpenFileDialog(this, settings) != true)
            {
                texture.BumpPath = null;
            }
            else
            {
                var tempImage = ImageC.FromFile(settings.FileName);

                if (tempImage.Size != texture.Image.Size)
                {
                    _messageService.ShowError(_localizationService["WrongImageSize"]);
                    texture.BumpPath = null;
                }
                else
                {
                    texture.BumpPath = _editor.Level?.Settings?.MakeRelative(settings.FileName, VariableType.LevelDirectory);
                }
            }
        }

        OnPropertyChanged(nameof(IsCustomMapped));
        OnPropertyChanged(nameof(CustomPathText));
        OnPropertyChanged(nameof(IsAssignEnabled));
    }

    [RelayCommand]
    private void AssignBumpMap()
    {
        if (SelectedTexture is not { } texture || IsCustomMapped)
            return;

        TextureArea selected = MapSelectionProvider?.Invoke() ?? TextureArea.None;
        if (selected.TextureIsInvisible)
            return;

        Vector2 p0 = selected.TexCoord0 / LevelTexture.BumpMappingGranularity;
        Vector2 p1 = selected.TexCoord1 / LevelTexture.BumpMappingGranularity;
        Vector2 p2 = selected.TexCoord2 / LevelTexture.BumpMappingGranularity;
        Vector2 p3 = selected.TexCoord3 / LevelTexture.BumpMappingGranularity;

        int xMin = (int)Math.Min(Math.Min(Math.Min(p0.X, p1.X), p2.X), p3.X);
        int xMax = (int)Math.Max(Math.Max(Math.Max(p0.X, p1.X), p2.X), p3.X);
        int yMin = (int)Math.Min(Math.Min(Math.Min(p0.Y, p1.Y), p2.Y), p3.Y);
        int yMax = (int)Math.Max(Math.Max(Math.Max(p0.Y, p1.Y), p2.Y), p3.Y);

        for (int y = yMin; y < yMax; y++)
            for (int x = xMin; x < xMax; x++)
                texture.SetBumpMappingLevel(x, y, SelectedLevel);

        RequestInvalidate?.Invoke(this, EventArgs.Empty);
        _editor.BumpmapsChange();
    }

    [RelayCommand]
    private void Confirm()
    {
        DialogResult = true;
    }
}
