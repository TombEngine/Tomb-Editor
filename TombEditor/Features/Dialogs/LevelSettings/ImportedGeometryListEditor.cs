#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TombLib.GeometryIO;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;
using TombLib.WPF.ViewModels;
using LevelSettingsData = TombLib.LevelData.LevelSettings;

namespace TombEditor.Features.Dialogs.LevelSettings;

/// <summary>Adds and removes imported geometries of a settings clone while keeping the grid rows in sync with it.</summary>
public sealed class ImportedGeometryListEditor(
    LevelSettingsData settings,
    ObservableCollection<ImportedGeometryRow> rows,
    Editor editor,
    IDialogService dialogService,
    IMessageService messageService)
{
    private static string GetText(string key) => Localizer.Instance["TombEditor.ImportedGeometry." + key];

    /// <summary>Asks for 3D files and presets, then adds the chosen files as new imported geometries.</summary>
    public void Add(INotifyPropertyChanged ownerViewModel)
    {
        var owner = WPFUtils.GetWindowOwner();
        string initialDirectory = PathC.GetDirectoryNameTry(settings.LevelFilePath);
        var paths = LevelFileDialog.BrowseFiles(owner, null, initialDirectory, GetText("BrowseFilesTitle"), BaseGeometryImporter.FileExtensions);

        var importInfos = new List<KeyValuePair<ImportedGeometry, ImportedGeometryInfo>>();
        var configuration = editor.Configuration;

        foreach (string path in paths)
        {
            if (!path.IsANSI())
            {
                UserDialogs.NonANSIFilePathError(owner);
                continue;
            }

            var ioViewModel = new GeometryIOSettingsWindowViewModel(IOSettingsPresets.GeometryImportSettingsPresets);
            ioViewModel.SelectPreset(configuration?.GeometryIO_LastUsedGeometryImportPresetName);

            if (dialogService.ShowDialog(ownerViewModel, ioViewModel) != true)
                continue;

            if (configuration is not null)
                configuration.GeometryIO_LastUsedGeometryImportPresetName = ioViewModel.SelectedPreset?.Name;

            var info = new ImportedGeometryInfo(settings.MakeRelative(path, VariableType.LevelDirectory), ioViewModel.GetCurrentSettings());
            importInfos.Add(new KeyValuePair<ImportedGeometry, ImportedGeometryInfo>(new ImportedGeometry(), info));
        }

        if (importInfos.Count == 0)
            return;

        settings.ImportedGeometryUpdate(importInfos);
        settings.ImportedGeometries.AddRange(importInfos.Select(entry => entry.Key));

        foreach (var entry in importInfos)
            rows.Add(new ImportedGeometryRow(settings, entry.Key));
    }

    /// <summary>Removes the selected rows after the user confirms; the settings list is kept in sync.</summary>
    public void Delete(IList? selection)
    {
        var selectedRows = selection?.OfType<ImportedGeometryRow>().ToList() ?? [];

        if (selectedRows.Count == 0)
            return;

        string message = string.Format(GetText("DeleteConfirmation"), selectedRows.Count);

        if (!messageService.ShowConfirmation(message, GetText("DeleteConfirmationTitle"), false, true))
            return;

        var removedGeometries = selectedRows.Select(row => row.Object).ToHashSet();
        settings.ImportedGeometries.RemoveAll(removedGeometries.Contains);

        foreach (var row in selectedRows)
            rows.Remove(row);
    }
}
