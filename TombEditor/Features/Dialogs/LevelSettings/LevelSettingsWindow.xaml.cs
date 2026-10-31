#nullable enable

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TombEditor.Features.Dialogs.TexturePreview;
using TombEditor.Features.Dialogs.WadPreview;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.LevelSettings;

public partial class LevelSettingsWindow : Window
{
    private Window? _preview;

    public LevelSettingsWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        Loaded += (_, _) => WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormLevelSettings");
        Closed += (_, _) =>
        {
            _preview?.Close();
            (DataContext as IDisposable)?.Dispose();
        };
        DataContextChanged += OnDataContextChanged;
    }

    // The NG sound area columns are only meaningful for TRNG levels.
    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not LevelSettingsWindowViewModel viewModel)
            return;

        UpdateAreaColumns(viewModel);
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(LevelSettingsWindowViewModel.IsTRNG))
                UpdateAreaColumns(viewModel);
        };
    }

    private void UpdateAreaColumns(LevelSettingsWindowViewModel viewModel)
    {
        var visibility = viewModel.IsTRNG ? Visibility.Visible : Visibility.Collapsed;
        AreaColumn.Visibility = visibility;
        OriginalIdColumn.Visibility = visibility;
    }

    // Right-clicking a cell selects it first so the context menu copies that cell.
    private void PathVariables_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject source)
            return;

        var cell = source as DataGridCell ?? source.FindAncestor<DataGridCell>();

        if (cell is null)
            return;

        PathVariablesGrid.SelectedCells.Clear();
        PathVariablesGrid.SelectedCells.Add(new DataGridCellInfo(cell));
    }

    private void CopyPathVariable_Click(object sender, RoutedEventArgs e)
    {
        if (PathVariablesGrid.SelectedCells is not [{ Item: PathVariableRow row } cell, ..])
            return;

        Clipboard.SetText(cell.Column == PathVariablesGrid.Columns[0] ? row.Variable : row.Value);
    }

    private void ShowTexturePreview_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: TextureRow row })
            ShowPreview(sender, new TexturePreviewWindow { DataContext = new TexturePreviewWindowViewModel(row.Texture) });
    }

    private void ShowWadPreview_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: WadRow { Wad.Wad: { } wad } })
            ShowPreview(sender, new WadPreviewWindow(Editor.Instance.RenderingDevice, Editor.Instance) { DataContext = new WadPreviewWindowViewModel(wad) });
    }

    /// <summary>Shows a preview next to the clicked cell; it closes when it loses focus and replaces any previous one.</summary>
    private void ShowPreview(object anchor, Window preview)
    {
        _preview?.Close();

        var origin = ((FrameworkElement)anchor).PointToScreen(new Point(0, 0));
        var scale = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformFromDevice.M11 ?? 1.0;

        preview.Owner = this;
        preview.WindowStartupLocation = WindowStartupLocation.Manual;
        preview.Left = origin.X * scale;
        preview.Top = origin.Y * scale;
        preview.Closed += (_, _) =>
        {
            if (ReferenceEquals(_preview, preview))
                _preview = null;
        };

        _preview = preview;
        preview.Show();
    }
}
