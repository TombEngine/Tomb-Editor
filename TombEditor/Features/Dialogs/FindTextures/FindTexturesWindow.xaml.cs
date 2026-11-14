#nullable enable

using System;
using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.FindTextures;

public partial class FindTexturesWindow : Window
{
    public FindTexturesWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        Loaded += (_, _) => WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormFindTextures");
        Closed += OnClosed;
    }

    private void OnClosed(object? sender, EventArgs e)
        => (DataContext as FindTexturesWindowViewModel)?.Cleanup();
}
