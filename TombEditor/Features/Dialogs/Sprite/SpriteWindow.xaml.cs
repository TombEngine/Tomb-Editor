#nullable enable

using System;
using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.Sprite;

public partial class SpriteWindow : Window
{
    public SpriteWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        Loaded += OnLoaded;
        Closed += OnClosed;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormSprite");

    private void OnClosed(object? sender, EventArgs e)
    {
        if (DataContext is SpriteWindowViewModel viewModel)
            viewModel.Detach();
    }
}
