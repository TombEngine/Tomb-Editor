#nullable enable

using System.Windows;
using TombLib.LevelData;
using TombLib.WPF;
using TombLib.WPF.Services;

namespace TombEditor.Features.Dialogs.ImportedGeometryEditor;

public partial class ImportedGeometryWindow : Window
{
    public ImportedGeometryWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        Loaded += (_, _) => WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormImportedGeometry");
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is ImportedGeometryWindowViewModel oldViewModel)
            oldViewModel.EditMaterialsRequested -= OnEditMaterialsRequested;

        if (e.NewValue is ImportedGeometryWindowViewModel newViewModel)
            newViewModel.EditMaterialsRequested += OnEditMaterialsRequested;
    }

    private void OnEditMaterialsRequested(object? sender, ImportedGeometry model)
    {
        MaterialEditorDialog.Show(model.Textures, Editor.Instance.Configuration);
    }
}
