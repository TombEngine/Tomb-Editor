using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace TombLib.WPF.Dialogs;

/// <summary>Hosts modal editor views and native file pickers using WPF ownership.</summary>
/// <param name="views">Provides shared and application-specific view factories.</param>
public sealed class DialogService(DialogViewRegistry views) : IDialogService
{
    private readonly Dictionary<INotifyPropertyChanged, Window> _dialogs = new(ReferenceEqualityComparer.Instance);

    /// <summary>Shows the view model's registered window and releases its ownership tracking on close.</summary>
    public bool? ShowDialog(INotifyPropertyChanged owner, IModalDialogViewModel dialog)
    {
        var window = views.Create(dialog.GetType());
        window.DataContext = dialog;
        window.Owner = FindOwner(owner);
        window.HookModalAutoClose();
        _dialogs.Add(dialog, window);

        try
        {
            return window.ShowDialog();
        }
        finally
        {
            _dialogs.Remove(dialog);
        }
    }

    /// <summary>Closes a view model's existing window without affecting unrelated windows.</summary>
    public void Close(INotifyPropertyChanged viewModel)
    {
        var window = _dialogs.GetValueOrDefault(viewModel) ?? FindViewWindow(viewModel);

        if (window is { IsVisible: true })
            window.Close();
    }

    /// <summary>Shows a WPF file picker owned by the view model's window.</summary>
    public bool? ShowOpenFileDialog(INotifyPropertyChanged owner, OpenFileDialogSettings settings)
    {
        var dialog = new OpenFileDialog
        {
            CheckFileExists = settings.CheckFileExists,
            FileName = settings.FileName,
            Filter = settings.Filter,
            InitialDirectory = settings.InitialDirectory,
            Multiselect = settings.Multiselect,
            Title = settings.Title
        };
        var window = FindOwner(owner);
        bool? accepted = window is null ? dialog.ShowDialog() : dialog.ShowDialog(window);

        if (accepted is true)
        {
            settings.FileName = dialog.FileName;
            settings.FileNames = dialog.FileNames;
        }

        return accepted;
    }

    private static Window? FindOwner(INotifyPropertyChanged viewModel)
        => FindViewWindow(viewModel) ?? WPFUtils.GetOwnerWindow(IntPtr.Zero);

    private static Window? FindViewWindow(INotifyPropertyChanged viewModel)
        => WPFUtils.OpenWindows.FirstOrDefault(window => ContainsViewModel(window, viewModel));

    private static bool ContainsViewModel(DependencyObject view, INotifyPropertyChanged viewModel)
    {
        if (view is FrameworkElement element && ReferenceEquals(element.DataContext, viewModel))
            return true;

        if (view is not Visual && view is not Visual3D)
            return false;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(view); i++)
        {
            if (ContainsViewModel(VisualTreeHelper.GetChild(view, i), viewModel))
                return true;
        }

        return false;
    }
}
