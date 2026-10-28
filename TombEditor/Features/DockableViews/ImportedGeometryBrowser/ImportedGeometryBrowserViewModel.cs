#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TombEditor.Features.DockableViews.ResourcePreview;
using TombLib.LevelData;
using TombLib.WPF;
using TombLib.Wad;

namespace TombEditor.Features.DockableViews.ImportedGeometryBrowser;

public sealed class ImportedGeometryItem
{
    public ImportedGeometry Geometry { get; }
    public string DisplayName { get; }

    public ImportedGeometryItem(ImportedGeometry geometry, TRVersion.Game gameVersion)
    {
        Geometry = geometry;
        DisplayName = geometry.ToString(gameVersion);
    }
}

public partial class ImportedGeometryBrowserViewModel : ObservableObject
{
    private readonly Editor _editor;
    private bool _suppressEditorSync;
    private bool _disposed;

    /// <summary>Gets the preview loading state and resource replacement action.</summary>
    public ResourcePreviewViewModel Preview { get; }

    public ObservableCollection<ImportedGeometryItem> Geometries { get; } = new();

    private ImportedGeometryItem? _selectedGeometry;
    public ImportedGeometryItem? SelectedGeometry
    {
        get => _selectedGeometry;
        set
        {
            if (!SetProperty(ref _selectedGeometry, value))
                return;

            ((RelayCommand)PreviousCommand).NotifyCanExecuteChanged();
            ((RelayCommand)NextCommand).NotifyCanExecuteChanged();
            ((RelayCommand)LocateCommand).NotifyCanExecuteChanged();

            SelectedGeometryChanged?.Invoke(this, value?.Geometry);

            if (_suppressEditorSync)
                return;

            if (value?.Geometry is { } geo)
                _editor.ChosenItems = new IWadObject[] { geo };
        }
    }

    // Raised after the selection changes so the View can sync the WinForms preview's CurrentObject.
    public event EventHandler<ImportedGeometry?>? SelectedGeometryChanged;

    public ICommand AddCommand { get; }
    public ICommand PreviousCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand LocateCommand { get; }

    public ImportedGeometryBrowserViewModel(Editor editor)
    {
        _editor = editor;
        Preview = new ResourcePreviewViewModel(editor, importedGeometry: true);
        _editor.EditorEventRaised += OnEditorEventRaised;

        AddCommand = CommandHandler.GetCommand(
            "AddImportedGeometry",
            () => new CommandArgs { Editor = _editor, Window = WPFUtils.GetWin32WindowOwner() });

        PreviousCommand = new RelayCommand(SelectPrevious, () => Geometries.Count > 0);
        NextCommand = new RelayCommand(SelectNext, () => Geometries.Count > 0);
        LocateCommand = new RelayCommand(LocateSelected, () => SelectedGeometry is not null);
    }

    public void Cleanup()
    {
        if (_disposed)
            return;
        _disposed = true;
        Preview.Dispose();
        _editor.EditorEventRaised -= OnEditorEventRaised;
    }

    private void OnEditorEventRaised(IEditorEvent obj)
    {
        if (obj is Editor.LoadedImportedGeometriesChangedEvent || obj is Editor.GameVersionChangedEvent)
        {
            RebuildList();
            return;
        }

        if (obj is Editor.ChosenItemsChangedEvent itemsChanged)
        {
            var geo = itemsChanged.Current?.OfType<ImportedGeometry>().FirstOrDefault();
            if (geo is null)
                return;

            _suppressEditorSync = true;
            try
            {
                SelectedGeometry = Geometries.FirstOrDefault(g => g.Geometry == geo);
            }
            finally
            {
                _suppressEditorSync = false;
            }
        }
    }

    private void RebuildList()
    {
        _suppressEditorSync = true;
        try
        {
            var previousGeometry = SelectedGeometry?.Geometry;

            Geometries.Clear();

            if (_editor.Level?.Settings is not { } settings)
                return;

            var gameVersion = settings.GameVersion;
            foreach (var geo in settings.ImportedGeometries
                .Where(g => g.LoadException is null && g.DirectXModel is not null))
            {
                Geometries.Add(new ImportedGeometryItem(geo, gameVersion));
            }

            // Preserve current selection across rebuilds; fall back to first item.
            SelectedGeometry =
                Geometries.FirstOrDefault(g => g.Geometry == previousGeometry)
                ?? Geometries.FirstOrDefault();
        }
        finally
        {
            _suppressEditorSync = false;
            ((RelayCommand)PreviousCommand).NotifyCanExecuteChanged();
            ((RelayCommand)NextCommand).NotifyCanExecuteChanged();
        }
    }

    private void SelectPrevious()
    {
        if (Geometries.Count == 0)
            return;

        var index = SelectedGeometry is null ? 0 : Geometries.IndexOf(SelectedGeometry);
        SelectedGeometry = Geometries[(index - 1 + Geometries.Count) % Geometries.Count];
    }

    private void SelectNext()
    {
        if (Geometries.Count == 0)
            return;

        var index = SelectedGeometry is null ? -1 : Geometries.IndexOf(SelectedGeometry);
        SelectedGeometry = Geometries[(index + 1) % Geometries.Count];
    }

    private void LocateSelected()
    {
        if (SelectedGeometry?.Geometry is { } geo)
            EditorActions.FindImportedGeometry(geo);
    }
}
