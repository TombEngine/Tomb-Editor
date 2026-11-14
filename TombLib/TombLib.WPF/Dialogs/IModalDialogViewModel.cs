using System.ComponentModel;

namespace TombLib.WPF.Dialogs;

/// <summary>Reports a modal editor dialog's result through property notifications.</summary>
public interface IModalDialogViewModel : INotifyPropertyChanged
{
    /// <summary>Gets the result, or null while awaiting a choice.</summary>
    bool? DialogResult { get; }
}
