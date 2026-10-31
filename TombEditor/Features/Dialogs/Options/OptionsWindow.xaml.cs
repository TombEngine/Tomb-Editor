#nullable enable

using System;
using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.Options;

public partial class OptionsWindow : Window
{
    public OptionsWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        Closed += (_, _) => (DataContext as IDisposable)?.Dispose();
    }
}
