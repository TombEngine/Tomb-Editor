#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TombLib.Forms.ViewModels;
using TombLib.Forms.Views;
using TombLib.Utils;
using TombLib.WPF;

namespace TombLib.Forms.Services;

/// <summary>Opens the shared WPF material editor with the application's active window as owner.</summary>
public static class MaterialEditorDialog
{
    /// <summary>Edits sidecars and reports persisted changes, including saves made before cancellation.</summary>
    public static MaterialEditorResult Show(IEnumerable<Texture> textures, ConfigurationBase configuration, Texture? selectedTexture = null)
    {
        var viewModel = new MaterialEditorWindowViewModel(textures, selectedTexture);
        var window = new MaterialEditorWindow { DataContext = viewModel };
        var owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(candidate => candidate.IsActive);
        if (owner is not null)
            window.Owner = owner;

        WindowConfiguration.ConfigureWindow(window, configuration, "FormMaterialEditor");
        bool accepted = window.ShowDialog() is true;
        return new MaterialEditorResult(accepted, viewModel.MaterialChanged, viewModel.MaterialFileName);
    }
}
