using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;
using TombLib.Utils;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.Operation;

public partial class OperationDialogWindow : Window
{
    // Taskbar progress is a View concern: the VM only exposes Progress/State and this window
    // mirrors them onto the owning window's taskbar button, like the legacy FormOperationDialog
    // did with the main form's handle.
    private IntPtr _taskbarHwnd = IntPtr.Zero;
    private Window _taskbarWindow;

    public OperationDialogWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        Loaded += OnLoaded;
        Closing += OnClosing;
        Closed += OnClosed;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is OperationDialogWindowViewModel vm)
        {
            var interop = new WindowInteropHelper(this);
            vm.SetWindowHandle(interop.Handle);

            // The dialog is hidden from the taskbar, so build/open progress must be shown on the
            // owning (main) window's taskbar button instead of this dialog's own handle.
            _taskbarHwnd = interop.Owner != IntPtr.Zero ? interop.Owner : interop.Handle;
            _taskbarWindow = FindWindowByHandle(_taskbarHwnd);

            // Prefer WPF taskbar support, with the native owner handle as a fallback.
            if (_taskbarWindow is not null && _taskbarWindow.TaskbarItemInfo is null)
                _taskbarWindow.TaskbarItemInfo = new TaskbarItemInfo();

            vm.LogEntries.CollectionChanged += OnLogChanged;
            vm.PropertyChanged += OnVmPropertyChanged;

            // Match the legacy ctor: put an (empty) progress bar on the taskbar right away.
            if (vm.IsProgressVisible)
                SetTaskbarState(TaskbarItemProgressState.Normal);

            vm.Start();
        }
    }

    private void OnClosing(object sender, CancelEventArgs e)
    {
        if (DataContext is OperationDialogWindowViewModel vm)
        {
            if (!vm.TryRequestClose())
                e.Cancel = true;
        }
    }

    // Match the legacy FormClosing: always clear the taskbar progress once the dialog goes away.
    private void OnClosed(object sender, EventArgs e)
        => SetTaskbarState(TaskbarItemProgressState.None);

    // Match the legacy lstLog.ScrollToCaret(): auto-scroll on new log entry.
    private void OnLogChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && logList.Items.Count > 0)
            logList.ScrollIntoView(logList.Items[logList.Items.Count - 1]);
    }

    private void OnVmPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (sender is not OperationDialogWindowViewModel vm)
            return;

        switch (e.PropertyName)
        {
            // Mirror live progress onto the taskbar. Stop once the operation has ended so the
            // red error bar keeps its last value (the VM resets Progress to 0 on failure).
            case nameof(OperationDialogWindowViewModel.Progress) when vm.State == OperationState.Running:
                SetTaskbarValue(vm.Progress);
                break;

            // Error bar on failure, cleared otherwise; flash the taskbar button so a completed
            // build/load is noticed while the app is in the background (legacy behaviour).
            case nameof(OperationDialogWindowViewModel.State) when vm.State != OperationState.Running:
                SetTaskbarState(vm.State == OperationState.Failed
                    ? TaskbarItemProgressState.Error
                    : TaskbarItemProgressState.None);
                TaskbarProgress.FlashWindow(_taskbarHwnd);
                break;

            // Match the legacy butOk.Focus(): focus OK as soon as the operation reports success.
            case nameof(OperationDialogWindowViewModel.IsOkEnabled) when vm.IsOkEnabled:
                okButton.Focus();
                break;
        }
    }

    private static Window FindWindowByHandle(IntPtr hwnd)
    {
        if (Application.Current is { } app)
        {
            foreach (Window window in app.Windows)
            {
                if (new WindowInteropHelper(window).Handle == hwnd)
                    return window;
            }
        }

        return null;
    }

    private void SetTaskbarState(TaskbarItemProgressState state)
    {
        if (_taskbarWindow?.TaskbarItemInfo is { } taskbar)
        {
            taskbar.ProgressState = state;
        }
        else if (_taskbarHwnd != IntPtr.Zero)
        {
            TaskbarProgress.SetState(_taskbarHwnd, state switch
            {
                TaskbarItemProgressState.Normal => TaskbarProgress.TaskbarStates.Normal,
                TaskbarItemProgressState.Error => TaskbarProgress.TaskbarStates.Error,
                _ => TaskbarProgress.TaskbarStates.NoProgress,
            });
        }
    }

    private void SetTaskbarValue(int progress)
    {
        if (_taskbarWindow?.TaskbarItemInfo is { } taskbar)
        {
            // The shell switches to determinate mode implicitly on SetProgressValue, but
            // TaskbarItemInfo does not - make sure a state is set for operations created with
            // noProgressBar that still report numeric progress (parity with the legacy dialog).
            if (taskbar.ProgressState == TaskbarItemProgressState.None)
                taskbar.ProgressState = TaskbarItemProgressState.Normal;

            taskbar.ProgressValue = progress / 100.0;
        }
        else if (_taskbarHwnd != IntPtr.Zero)
        {
            TaskbarProgress.SetValue(_taskbarHwnd, progress, 100);
        }
    }
}
