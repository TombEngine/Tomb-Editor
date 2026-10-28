#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF;
using TombLib.WPF.Services;

namespace TombEditor.Features.DockableViews.TexturePanel;

public partial class TexturePanelViewModel : ObservableObject
{
    private static readonly float[] _tileSizes = { 1, 2, 4, 8, 16, 32, 64, 128, 256 };

    private readonly Editor _editor;
    private bool _suppressEditorSync;
    private bool _disposed;

    public ObservableCollection<LevelTexture> Textures { get; } = new();
    public ObservableCollection<string> BlendModeNames { get; } = new();
    public IReadOnlyList<float> TileSizes => _tileSizes;

    private LevelTexture? _selectedTexture;
    public LevelTexture? SelectedTexture
    {
        get => _selectedTexture;
        set
        {
            if (!SetProperty(ref _selectedTexture, value))
                return;

            UpdateButtonStates();
            RequestResetVisibleTexture?.Invoke(this, value);

            if (_suppressEditorSync)
                return;

            _editor.SelectedLevelTextureChanged(value);
        }
    }

    private int _selectedBlendModeIndex = -1;
    public int SelectedBlendModeIndex
    {
        get => _selectedBlendModeIndex;
        set
        {
            if (!SetProperty(ref _selectedBlendModeIndex, value))
                return;

            if (_suppressEditorSync || value < 0)
                return;

            var area = _editor.SelectedTexture;
            area.BlendMode = TextureExtensions.ToBlendMode(value);
            _editor.SelectedTexture = area;
        }
    }

    private float _selectedTileSize = 32f;
    public float SelectedTileSize
    {
        get => _selectedTileSize;
        set
        {
            if (!SetProperty(ref _selectedTileSize, value))
                return;
            if (_suppressEditorSync)
                return;
            EditorActions.SetSelectionTileSize(value);
        }
    }

    private bool _isMaterialsVisible;
    public bool IsMaterialsVisible
    {
        get => _isMaterialsVisible;
        private set => SetProperty(ref _isMaterialsVisible, value);
    }

    private bool _areMainButtonsEnabled;
    public bool AreMainButtonsEnabled
    {
        get => _areMainButtonsEnabled;
        private set
        {
            if (SetProperty(ref _areMainButtonsEnabled, value))
            {
                ((RelayCommand)DeleteTextureCommand).NotifyCanExecuteChanged();
                ((RelayCommand)BrowseTextureCommand).NotifyCanExecuteChanged();
                ((RelayCommand)MaterialEditorCommand).NotifyCanExecuteChanged();
            }
        }
    }

    private bool _isTextureSoundsEnabled;
    public bool IsTextureSoundsEnabled
    {
        get => _isTextureSoundsEnabled;
        private set
        {
            if (SetProperty(ref _isTextureSoundsEnabled, value))
                ((RelayCommand)TextureSoundsCommand).NotifyCanExecuteChanged();
        }
    }

    private bool _isBumpMapsEnabled;
    public bool IsBumpMapsEnabled
    {
        get => _isBumpMapsEnabled;
        private set
        {
            if (SetProperty(ref _isBumpMapsEnabled, value))
                ((RelayCommand)BumpMapsCommand).NotifyCanExecuteChanged();
        }
    }

    public event EventHandler<LevelTexture?>? RequestResetVisibleTexture;
    public event EventHandler<TextureArea>? RequestShowTexture;
    public event EventHandler? RequestInvalidate;

    public ICommand AddTextureCommand { get; }
    public ICommand DeleteTextureCommand { get; }
    public ICommand BrowseTextureCommand { get; }
    public ICommand RotateCommand { get; }
    public ICommand MirrorCommand { get; }
    public ICommand SetDoubleSidedCommand { get; }
    public ICommand EditAnimationRangesCommand { get; }
    public ICommand TextureSoundsCommand { get; }
    public ICommand BumpMapsCommand { get; }
    public ICommand MaterialEditorCommand { get; }

    public TexturePanelViewModel(Editor editor)
    {
        _editor = editor;
        _editor.EditorEventRaised += OnEditorEventRaised;

        AddTextureCommand = CommandHandler.GetCommand("AddTexture",
            () => new CommandArgs { Editor = _editor, Window = WPFUtils.GetWindowOwner() });

        RotateCommand = CommandHandler.GetCommand("RotateTexture",
            () => new CommandArgs { Editor = _editor, Window = WPFUtils.GetWindowOwner() });

        MirrorCommand = CommandHandler.GetCommand("MirrorTexture",
            () => new CommandArgs { Editor = _editor, Window = WPFUtils.GetWindowOwner() });

        SetDoubleSidedCommand = CommandHandler.GetCommand("SetTextureDoubleSided",
            () => new CommandArgs { Editor = _editor, Window = WPFUtils.GetWindowOwner() });

        EditAnimationRangesCommand = CommandHandler.GetCommand("EditAnimationRanges",
            () => new CommandArgs { Editor = _editor, Window = WPFUtils.GetWindowOwner() });

        DeleteTextureCommand = new RelayCommand(DeleteSelected, () => AreMainButtonsEnabled);
        BrowseTextureCommand = new RelayCommand(BrowseSelected, () => AreMainButtonsEnabled);
        TextureSoundsCommand = new RelayCommand(OpenTextureSounds, () => IsTextureSoundsEnabled);
        BumpMapsCommand = new RelayCommand(OpenBumpMaps, () => IsBumpMapsEnabled);
        MaterialEditorCommand = new RelayCommand(OpenMaterialEditor, () => AreMainButtonsEnabled);
    }

    public void Cleanup()
    {
        if (_disposed)
            return;
        _disposed = true;
        _editor.EditorEventRaised -= OnEditorEventRaised;
    }

    private void OnEditorEventRaised(IEditorEvent obj)
    {
        if (obj is Editor.InitEvent || obj is Editor.GameVersionChangedEvent || obj is Editor.LevelChangedEvent)
        {
            RebuildBlendModes();
            UpdateButtonStates();
        }

        if (obj is Editor.LevelChangedEvent)
        {
            RebuildTextureList(preserveSelection: false);
        }

        if (obj is Editor.LoadedTexturesChangedEvent loaded)
        {
            RebuildTextureList(preserveSelection: true);

            if (loaded.NewToSelect is LevelTexture nts)
                SelectedTexture = Textures.FirstOrDefault(t => t == nts);

            RequestInvalidate?.Invoke(this, EventArgs.Empty);
        }

        if (obj is Editor.SelectedTexturesChangedEvent e)
        {
            _suppressEditorSync = true;
            try
            {
                if (e.Current.Texture is LevelTexture toSelect)
                    SelectedTexture = toSelect;
                ApplyTextureControls(e.Current);
            }
            finally
            {
                _suppressEditorSync = false;
            }
        }

        if (obj is Editor.SelectTextureAndCenterViewEvent center)
        {
            _suppressEditorSync = true;
            try
            {
                if (center.Texture.Texture is LevelTexture tex)
                    SelectedTexture = tex;
                ApplyTextureControls(center.Texture);
                RequestShowTexture?.Invoke(this, center.Texture);
            }
            finally
            {
                _suppressEditorSync = false;
            }
        }

        if (obj is Editor.ConfigurationChangedEvent cfg)
        {
            if (_tileSizes.Contains(_editor.Configuration.TextureMap_TileSelectionSize))
            {
                _suppressEditorSync = true;
                try
                {
                    SelectedTileSize = _editor.Configuration.TextureMap_TileSelectionSize;
                }
                finally { _suppressEditorSync = false; }
            }
        }
    }

    private void RebuildTextureList(bool preserveSelection)
    {
        if (_editor.Level?.Settings is not { } settings)
        {
            Textures.Clear();
            return;
        }

        var previous = preserveSelection ? SelectedTexture : null;

        _suppressEditorSync = true;
        try
        {
            Textures.Clear();
            foreach (var tex in settings.Textures)
                Textures.Add(tex);

            SelectedTexture = previous is not null && Textures.Contains(previous)
                ? previous
                : Textures.FirstOrDefault();
        }
        finally
        {
            _suppressEditorSync = false;
        }
    }

    private void RebuildBlendModes()
    {
        if (_editor.Level?.Settings is not { } settings)
            return;

        _suppressEditorSync = true;
        try
        {
            BlendModeNames.Clear();
            foreach (var name in TextureExtensions.BlendModeUserNames(settings))
                BlendModeNames.Add(name);

            ApplyTextureControls(_editor.SelectedTexture);
        }
        finally
        {
            _suppressEditorSync = false;
        }
    }

    private void ApplyTextureControls(TextureArea texture)
    {
        _suppressEditorSync = true;
        try
        {
            int newIndex = texture.BlendMode.ToUserIndex();
            SelectedBlendModeIndex = newIndex < BlendModeNames.Count ? newIndex : -1;
        }
        finally
        {
            _suppressEditorSync = false;
        }
    }

    private void UpdateButtonStates()
    {
        AreMainButtonsEnabled = SelectedTexture is not null;

        var level = _editor.Level;
        if (level is null)
        {
            IsTextureSoundsEnabled = false;
            IsBumpMapsEnabled = false;
            IsMaterialsVisible = false;
            return;
        }

        IsTextureSoundsEnabled = AreMainButtonsEnabled && level.Settings.GameVersion.Native() >= TRVersion.Game.TR3;
        IsBumpMapsEnabled = AreMainButtonsEnabled &&
            (level.Settings.GameVersion.Native() == TRVersion.Game.TR4 || level.IsTombEngine);
        IsMaterialsVisible = level.IsTombEngine;
    }

    private void DeleteSelected()
    {
        if (SelectedTexture is { } tex)
            EditorActions.RemoveTexture(WPFUtils.GetWindowOwner(), tex);
    }

    private void BrowseSelected()
    {
        if (SelectedTexture is { } tex && _editor.Level?.Settings is { } settings)
            EditorActions.ReloadResource(WPFUtils.GetWindowOwner(), settings, tex);
    }

    private void OpenTextureSounds()
    {
        if (SelectedTexture is not { } tex)
            return;

        var vm = new TombEditor.Features.Dialogs.FootStepSounds.FootStepSoundsWindowViewModel(tex, _editor);
        var dialog = new TombEditor.Features.Dialogs.FootStepSounds.FootStepSoundsWindow { DataContext = vm };
        dialog.SetOwner(WPFUtils.GetWindowOwner());
        dialog.ShowDialog();
    }

    private void OpenBumpMaps()
    {
        if (SelectedTexture is not { } tex)
            return;

        var vm = new TombEditor.Features.Dialogs.BumpMaps.BumpMapsWindowViewModel(tex, _editor);
        var dialog = new TombEditor.Features.Dialogs.BumpMaps.BumpMapsWindow { DataContext = vm };
        dialog.SetOwner(WPFUtils.GetWindowOwner());
        dialog.ShowDialog();
    }

    private void OpenMaterialEditor()
    {
        if (_editor.Level?.Settings is not { } settings)
            return;

        var list = Textures.Cast<Texture>();
        var result = MaterialEditorDialog.Show(list, _editor.Configuration, SelectedTexture);
        if (result.MaterialChanged && !string.IsNullOrEmpty(result.MaterialFileName))
        {
            _editor.SendMessage(
                "Material settings for selected texture were saved to " + result.MaterialFileName + ".",
                PopupType.Info);
        }
    }
}
