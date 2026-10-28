#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using TombLib.WPF.Dialogs;

namespace TombLib.WPF.Features.AnimatedTextures
{
    /// <summary>
    /// A text prompt raised by a view-model (which cannot show windows itself); the view answers it
    /// by showing an <see cref="InputDialog"/> and calling <see cref="Accept"/> on confirmation.
    /// </summary>
    public sealed class TextInputRequest
    {
        public TextInputRequest(string title, string label, string value, Action<string> accept)
        {
            Title = title;
            Label = label;
            Value = value;
            Accept = accept;
        }

        public string Title { get; }
        public string Label { get; }
        public string Value { get; }

        /// <summary>Called with the edited text when the user confirms the prompt.</summary>
        public Action<string> Accept { get; }
    }

    /// <summary>View-model for <see cref="InputDialog"/>: title, label and the editable value.</summary>
    public partial class InputDialogViewModel : ObservableObject, IModalDialogViewModel
    {
        [ObservableProperty] private bool? _dialogResult;
        [ObservableProperty] private string _value;

        public InputDialogViewModel(string title, string label, string value)
        {
            Title = title;
            Label = label;
            _value = value ?? string.Empty;
        }

        public string Title { get; }
        public string Label { get; }

        [RelayCommand]
        private void Ok() => DialogResult = true;
    }
}
