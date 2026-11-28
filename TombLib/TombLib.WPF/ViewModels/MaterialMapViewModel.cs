#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IOPath = System.IO.Path;
using Microsoft.Win32;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using TombLib.Utils;
using TombLib.WPF;
using TombLib.WPF.Imaging;

namespace TombLib.WPF.ViewModels;

/// <summary>Edits one map path and owns its immutable preview without locking the source file.</summary>
public partial class MaterialMapViewModel : ObservableObject
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly string? _baseDirectory;

    [ObservableProperty] private string _path = string.Empty;
    [ObservableProperty] private ImageSource? _preview;
    [ObservableProperty] private string _error = string.Empty;

    /// <summary>Gets the material map's serialized property name.</summary>
    public string Key { get; }

    /// <summary>Gets the localized map label.</summary>
    public string Label => Localizer.Instance[$"TombLib.MaterialEditorWindow.{Key}"];

    /// <summary>Gets whether the base color map is fixed to the selected texture.</summary>
    public bool IsReadOnly { get; }

    private bool CanEdit => !IsReadOnly;

    /// <summary>Creates a map row.</summary>
    public MaterialMapViewModel(string key, string path, bool isReadOnly = false, string? texturePath = null)
    {
        Key = key;
        IsReadOnly = isReadOnly;
        _baseDirectory = string.IsNullOrWhiteSpace(texturePath) ? null : IOPath.GetDirectoryName(IOPath.GetFullPath(texturePath));
        Path = path;
    }

    partial void OnPathChanged(string value)
    {
        Preview = null;
        Error = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
            return;

        try
        {
            var resolvedPath = _baseDirectory is null ? value : IOPath.GetFullPath(value, _baseDirectory);
            if (!File.Exists(resolvedPath))
            {
                Error = new FileNotFoundException(Label, resolvedPath).Message;
                return;
            }

            Preview = ImageSourceFactory.FromImage(ImageC.FromFile(resolvedPath));
        }
        catch (Exception exception)
        {
            Logger.Warn(exception, "Could not preview material map {0}.", value);
            Error = exception.Message;
        }
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private void Browse()
    {
        var dialog = new OpenFileDialog { Filter = ImageC.FileExtensions.GetFilter(), CheckFileExists = true, Title = Label };
        var owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        bool? accepted = owner is null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
        if (accepted is true)
            Path = dialog.FileName;
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private void Clear() => Path = string.Empty;
}
