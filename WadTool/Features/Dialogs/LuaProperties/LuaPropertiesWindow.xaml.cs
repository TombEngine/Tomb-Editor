#nullable enable

using System.Windows;
using TombLib.WPF;

namespace WadTool.Features.Dialogs.LuaProperties;

public partial class LuaPropertiesWindow : Window
{
    public LuaPropertiesWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        Loaded += OnLoaded;
    }

    // Match the legacy DarkListView.SelectItem() + EnsureVisible():
    // scroll the initially selected object into view on first show.
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (objectsList.SelectedItem is not null)
            objectsList.ScrollIntoView(objectsList.SelectedItem);
    }
}
