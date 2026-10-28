using System.ComponentModel;

namespace TombLib.WPF.Dialogs;

/// <summary>Opens editor views and file pickers with their owning WPF view.</summary>
public interface IDialogService
{
    /// <summary>Shows a registered modal view for a view model.</summary>
    bool? ShowDialog(INotifyPropertyChanged owner, IModalDialogViewModel dialog);

    /// <summary>Closes the open window displaying a view model.</summary>
    void Close(INotifyPropertyChanged viewModel);

    /// <summary>Shows a file picker and updates its settings when accepted.</summary>
    bool? ShowOpenFileDialog(INotifyPropertyChanged owner, OpenFileDialogSettings settings);
}
