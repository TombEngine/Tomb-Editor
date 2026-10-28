#nullable enable

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TombEditor.Features.Dialogs.ToolBarLayout;
using TombLib.Icons;

namespace TombEditor.Features.Toolbar;

/// <summary>
/// The editor's main button toolbar. Buttons are generated at runtime from
/// <c>Configuration.UI_ToolbarButtons</c> via <see cref="ToolbarButtonRegistry"/> and rebuilt when the
/// Customize dialog changes that list, so customization actually affects the layout (the previous
/// equivalent is <c>MainView.UpdateToolStripLayout</c>).
/// </summary>
public partial class EditorToolbarView : UserControl
{
    // "Draw objects" dropdown sub-items: (command, persisted config flag). Mirrors butDrawObjects.
    private static readonly (string Command, string Flag)[] DrawObjectsItems =
    {
        ("DrawMoveables", "Rendering3D_ShowMoveables"),
        ("DrawStatics", "Rendering3D_ShowStatics"),
        ("DrawImportedGeometry", "Rendering3D_ShowImportedGeometry"),
        ("DrawGhostBlocks", "Rendering3D_ShowGhostBlocks"),
        ("DrawVolumes", "Rendering3D_ShowVolumes"),
        ("DrawBoundingBoxes", "Rendering3D_ShowBoundingBoxes"),
        ("DrawOtherObjects", "Rendering3D_ShowOtherObjects"),
        ("DrawLightRadius", "Rendering3D_ShowLightRadius"),
    };

    public EditorToolbarView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Editor.Instance.EditorEventRaised -= OnEditorEventRaised;
        Editor.Instance.EditorEventRaised += OnEditorEventRaised;
        BuildToolbar();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
        => Editor.Instance.EditorEventRaised -= OnEditorEventRaised;

    private void OnEditorEventRaised(IEditorEvent obj)
    {
        if (obj is Editor.ConfigurationChangedEvent { UpdateToolbarLayout: true })
            BuildToolbar();
    }

    private void BuildToolbar()
    {
        ToolBarTray.ToolBars.Clear();

        ToolBar? current = null;

        foreach (var token in Editor.Instance.Configuration.UI_ToolbarButtons)
        {
            if (token == ToolbarButtonRegistry.SeparatorToken)
            {
                current = null; // the next button opens a fresh ToolBar group (a visual separator)
                continue;
            }

            if (!ToolbarButtonRegistry.TryGet(token, out var spec))
                continue;

            FrameworkElement? element = spec.Kind == ToolbarButtonKind.DrawObjectsMenu
                ? CreateDrawObjectsMenu()
                : ToolbarButtonRegistry.CreateButton(spec);

            if (element is null)
                continue;

            current ??= AddToolBar();
            current.Items.Add(element);
        }
    }

    private ToolBar AddToolBar()
    {
        var toolBar = new ToolBar();
        ToolBarTray.ToolBars.Add(toolBar);
        return toolBar;
    }

    private FrameworkElement CreateDrawObjectsMenu()
    {
        // A Menu hosted in a ToolBar gets restyled with the system ToolBar.MenuStyleKey style
        // (white popup, immune to the DarkUI theme). Use a regular toolbar Button that opens a
        // ContextMenu instead - ContextMenus pick up the DarkUI implicit style everywhere.
        // Icon + small down-arrow (same glyph the DarkUI ComboBox uses), so the button reads as
        // a dropdown like the legacy ToolStripDropDownButton did.
        var arrow = new System.Windows.Shapes.Path
        {
            Width = 7,
            Stretch = Stretch.Uniform,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(3, 0, 0, 0),
        };
        arrow.SetResourceReference(System.Windows.Shapes.Path.DataProperty, "WideArrowDown");
        arrow.SetResourceReference(System.Windows.Shapes.Shape.FillProperty, "Brush_Text");

        var button = new Button
        {
            ToolTip = "Draw objects",
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new Image { Source = IconSources.Load("Actions/DrawObjects") },
                    arrow,
                },
            },
        };

        var menu = new ContextMenu
        {
            PlacementTarget = button,
            Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom,
        };

        foreach (var (command, flag) in DrawObjectsItems)
        {
            var item = new MenuItem { Tag = flag };
            EditorMenu.SetCommand(item, command);
            menu.Items.Add(item);
        }

        menu.Opened += DrawObjectsMenu_Opened;
        button.Click += (_, _) => menu.IsOpen = true;
        return button;
    }

    private void DrawObjectsMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is not ContextMenu dropdown)
            return;

        foreach (var item in dropdown.Items.OfType<MenuItem>())
        {
            if (item.Tag is not string flagName || string.IsNullOrEmpty(flagName))
                continue;

            var prop = typeof(Configuration).GetProperty(flagName);
            if (prop is null || prop.PropertyType != typeof(bool))
                continue;

            item.IsCheckable = true;
            item.IsChecked = (bool)prop.GetValue(Editor.Instance.Configuration)!;
        }
    }

    private void CustomizeToolbar_Click(object sender, RoutedEventArgs e)
    {
        // Edit the live UI_ToolbarButtons. Applying raises ConfigurationChangedEvent(UpdateToolbarLayout:
        // true), which rebuilds this toolbar. The dialog universe is the set of known toolbar tokens.
        var vm = new ToolBarLayoutWindowViewModel(Editor.Instance, ToolbarButtonRegistry.AllTokens.ToList());
        var dialog = new ToolBarLayoutWindow
        {
            DataContext = vm,
            Owner = Window.GetWindow(this),
        };
        dialog.ShowDialog();
    }
}
