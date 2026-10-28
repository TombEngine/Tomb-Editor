#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using TombLib.Utils;
using TombLib.WPF;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.DockableViews.ResourcePreview;

/// <summary>Shows load failures and resource replacement actions beside native previews.</summary>
public partial class ResourcePreviewViewModel : ObservableObject, IDisposable
{
    private readonly Editor _editor;
    private readonly bool _importedGeometry;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty] private bool _isAvailable;
    [ObservableProperty] private string _message = string.Empty;

    public ResourcePreviewViewModel(Editor editor, bool importedGeometry, ILocalizationService? localizationService = null)
    {
        _editor = editor;
        _importedGeometry = importedGeometry;
        _localizationService = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);
        _editor.EditorEventRaised += OnEditorEvent;
        Refresh();
    }

    private void OnEditorEvent(IEditorEvent editorEvent)
    {
        if (editorEvent is Editor.LoadedWadsChangedEvent or Editor.LoadedImportedGeometriesChangedEvent or Editor.LevelChangedEvent or Editor.InitEvent)
            Refresh();
    }

    private void Refresh()
    {
        var settings = _editor.Level.Settings;
        IsAvailable = _importedGeometry
            ? settings.ImportedGeometries.Any(geometry => geometry.LoadException is null)
            : settings.Wads.Any(wad => wad.LoadException is null);

        if (IsAvailable)
        {
            Message = string.Empty;
            return;
        }

        var failedWad = settings.Wads.FirstOrDefault(wad => wad.LoadException is not null);
        var failedGeometry = settings.ImportedGeometries.FirstOrDefault(geometry => geometry.LoadException is not null);
        var exception = _importedGeometry ? failedGeometry?.LoadException : failedWad?.LoadException;
        string? path = _importedGeometry ? failedGeometry?.Info.Path : failedWad?.Path;

        if (exception is null || path is null)
        {
            Message = _localizationService[_importedGeometry ? "LoadGeometry" : "LoadWad"];
            return;
        }

        string absolutePath = settings.MakeAbsolute(path);
        string fileName = PathC.GetFileNameWithoutExtensionTry(absolutePath) ?? string.Empty;
        Message = _localizationService.Format(PathC.IsFileNotFoundException(exception) ? "FileNotFound" : "LoadFailed", fileName)
            + Environment.NewLine + _localizationService["ChooseReplacement"] + Environment.NewLine + absolutePath;
    }

    [RelayCommand]
    private void Reload()
    {
        var owner = WPFUtils.GetWindowOwner();
        var settings = _editor.Level.Settings;

        if (_importedGeometry)
        {
            var geometry = settings.ImportedGeometries.FirstOrDefault(resource => resource.LoadException is not null);

            if (geometry is not null)
                EditorActions.ReloadResource(owner, settings, geometry, true);
            else
                EditorActions.AddImportedGeometry(owner);
        }
        else
        {
            var wad = settings.Wads.FirstOrDefault(resource => resource.LoadException is not null);

            if (wad is not null)
                EditorActions.ReloadResource(owner, settings, wad);
            else
                EditorActions.AddWad(owner);
        }
    }

    /// <summary>Releases the editor subscription.</summary>
    public void Dispose() => _editor.EditorEventRaised -= OnEditorEvent;
}
