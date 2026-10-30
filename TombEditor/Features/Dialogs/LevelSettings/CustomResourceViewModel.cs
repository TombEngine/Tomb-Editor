#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using System;
using System.Windows.Media.Imaging;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF;
using TombLib.WPF.Imaging;
using TombLib.WPF.Services;
using LevelSettingsData = TombLib.LevelData.LevelSettings;

namespace TombEditor.Features.Dialogs.LevelSettings;

/// <summary>
/// Automatic or custom raw texture (font, sky, TR5 extra sprites) with a live preview,
/// mirroring a panel of the "Sky and font" page of the previous level settings form.
/// </summary>
public partial class CustomResourceViewModel : ObservableObject
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly LevelSettingsData _settings;
    private readonly string _defaultFile;
    private readonly string _browseTitle;
    private readonly Func<string?> _getPath;
    private readonly Action<string?> _setPath;
    private readonly Func<string?, ImageC> _loadImage;

    [ObservableProperty] private BitmapSource? _preview;
    [ObservableProperty] private bool _isPreviewValid = true;

    public CustomResourceViewModel(
        LevelSettingsData settings,
        string defaultFile,
        string autoLabel,
        string browseTitle,
        Func<string?> getPath,
        Action<string?> setPath,
        Func<string?, ImageC> loadImage)
    {
        _settings = settings;
        _defaultFile = defaultFile;
        AutoLabel = autoLabel;
        _browseTitle = browseTitle;
        _getPath = getPath;
        _setPath = setPath;
        _loadImage = loadImage;

        RefreshPreview();
    }

    /// <summary>Gets the caption of the "use default file" option.</summary>
    public string AutoLabel { get; }

    /// <summary>Gets or sets whether a custom file is used instead of the default resource.</summary>
    public bool IsCustom
    {
        get => !string.IsNullOrEmpty(_getPath());
        set
        {
            if (value == IsCustom)
                return;

            _setPath(value ? LevelSettingsData.VariableCreate(VariableType.LevelDirectory) + LevelSettingsData.Dir + _defaultFile : null);
            NotifyPathChanged();
        }
    }

    /// <summary>Gets or sets whether the default resource is used; unchecking is ignored so radio buttons can bind both properties.</summary>
    public bool IsAuto
    {
        get => !IsCustom;
        set
        {
            if (value)
                IsCustom = false;
        }
    }

    /// <summary>Gets or sets the displayed path; edits are ignored while the default resource is used.</summary>
    public string Path
    {
        get => IsCustom ? _getPath() ?? string.Empty : LevelSettingsData.VariableCreate(VariableType.LevelDirectory) + LevelSettingsData.Dir + _defaultFile;
        set
        {
            if (!IsCustom || string.IsNullOrEmpty(value) || value == _getPath())
                return;

            _setPath(value);
            NotifyPathChanged();
        }
    }

    public string AbsolutePath => _settings.MakeAbsolute(_getPath()) ?? string.Empty;

    /// <summary>Re-evaluates path dependent state after a path variable changed; the default resource never depends on one.</summary>
    public void Refresh()
    {
        OnPropertyChanged(nameof(AbsolutePath));

        if (IsCustom)
            RefreshPreview();
    }

    [RelayCommand]
    private void Browse()
    {
        string? result = LevelFileDialog.BrowseFile(WPFUtils.GetWindowOwner(), _settings, _getPath(), _browseTitle, LevelSettingsData.FileFormatsLoadRawExtraTexture, VariableType.LevelDirectory, false);

        if (result is null)
            return;

        _setPath(result);
        NotifyPathChanged();
    }

    private void NotifyPathChanged()
    {
        OnPropertyChanged(nameof(IsCustom));
        OnPropertyChanged(nameof(IsAuto));
        OnPropertyChanged(nameof(Path));
        OnPropertyChanged(nameof(AbsolutePath));
        RefreshPreview();
    }

    private void RefreshPreview()
    {
        try
        {
            string? path = _settings.MakeAbsolute(_getPath());
            Preview = ImageSourceFactory.FromImage(_loadImage(string.IsNullOrEmpty(path) ? null : path));
            IsPreviewValid = true;
        }
        catch (Exception exception)
        {
            Logger.Info(exception, "Unable to load texture preview.");
            Preview = null;
            IsPreviewValid = false;
        }
    }
}
