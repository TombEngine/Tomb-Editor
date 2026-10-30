#nullable enable

using System.Windows;
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
        Closed += (_, _) => _preview?.Close();
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