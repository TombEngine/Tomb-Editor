using System;
using System.ComponentModel;
using System.Windows;

namespace TombLib.WPF.Dialogs;

/// <summary>Closes a modal window when its current view model supplies a result.</summary>
internal sealed class ModalDialogBinding
{
    private readonly Window _window;
    private IModalDialogViewModel? _viewModel;

    public ModalDialogBinding(Window window)
    {
        _window = window;
        window.DataContextChanged += OnDataContextChanged;
        window.Closed += OnClosed;
        Attach(window.DataContext as IModalDialogViewModel);
    }

    private void Attach(IModalDialogViewModel? viewModel)
    {
        if (_viewModel is not null)
            _viewModel.PropertyChanged -= OnPropertyChanged;

        _viewModel = viewModel;

        if (_viewModel is not null)
            _viewModel.PropertyChanged += OnPropertyChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        => Attach(e.NewValue as IModalDialogViewModel);

    private void OnClosed(object? sender, EventArgs e)
    {
        Attach(null);
        _window.DataContextChanged -= OnDataContextChanged;
        _window.Closed -= OnClosed;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.PropertyName) && e.PropertyName != nameof(IModalDialogViewModel.DialogResult))
            return;

        if (ReferenceEquals(sender, _viewModel) && _viewModel?.DialogResult is { } result)
            _window.Dispatcher.Invoke(() => ApplyResult(sender, result));
    }

    private void ApplyResult(object? viewModel, bool result)
    {
        if (!ReferenceEquals(viewModel, _viewModel))
            return;

        try
        {
            _window.DialogResult = result;
        }
        catch (InvalidOperationException)
        {
            _window.Close();
        }
    }
}
