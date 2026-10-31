#nullable enable

using System.Windows;
using TombLib.WPF;

namespace TombLib.WPF.Views;

/// <summary>Displays native WPF material maps and typed properties.</summary>
public partial class MaterialEditorWindow : Window
{
    /// <summary>Creates the view and connects its modal view model.</summary>
    public MaterialEditorWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
    }
}
