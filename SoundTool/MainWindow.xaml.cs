#nullable enable

using SoundTool.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using TombLib.Utils;
using TombLib.Wad;

namespace SoundTool;

/// <summary>WPF shell responsible only for window lifecycle and native message routing.</summary>
public partial class MainWindow : Window
{
    private readonly Configuration _configuration;
    private readonly MainViewModel _viewModel;

    public MainWindow(Configuration configuration, MainViewModel viewModel)
    {
        _configuration = configuration;
        _viewModel = viewModel;
        InitializeComponent();
        DataContext = viewModel;
        RestorePlacement();
        Closing += WindowClosing;
        SourceInitialized += (_, _) => HwndSource.FromHwnd(new WindowInteropHelper(this).Handle)?.AddHook(WindowMessage);
        viewModel.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(MainViewModel.SelectedSound) && viewModel.SelectedSound is not null)
                SoundList.ScrollIntoView(viewModel.SelectedSound);
        };
    }

    private void SoundsSelectionChanged(object sender, SelectionChangedEventArgs e) =>
        _viewModel.SelectedSounds = [.. ((DataGrid)sender).SelectedItems.OfType<SoundInfoViewModel>()];

    private void SamplesSelectionChanged(object sender, SelectionChangedEventArgs e) =>
        _viewModel.SelectedSamples = [.. ((DataGrid)sender).SelectedItems.OfType<SampleViewModel>()];

    private void SoundListDoubleClick(object sender, MouseButtonEventArgs e) => _viewModel.PlayCommand.Execute(null);

    private void WindowClosing(object? sender, CancelEventArgs e)
    {
        if (!_viewModel.CanClose())
        {
            e.Cancel = true;
            return;
        }

        var bounds = WindowState == WindowState.Normal ? new Rect(Left, Top, Width, Height) : RestoreBounds;
        _configuration.Window_FormMain_Position = new System.Drawing.Point((int)bounds.Left, (int)bounds.Top);
        _configuration.Window_FormMain_Size = new System.Drawing.Size((int)bounds.Width, (int)bounds.Height);
        _configuration.Window_FormMain_Maximized = WindowState == WindowState.Maximized;
        _configuration.SaveTry();
        WadSoundPlayer.StopSample();
    }

    private void RestorePlacement()
    {
        var size = _configuration.Window_FormMain_Size;
        Width = Math.Max(MinWidth, size.Width);
        Height = Math.Max(MinHeight, size.Height);
        var position = _configuration.Window_FormMain_Position;
        if (position.X >= 0 && position.Y >= 0)
        {
            var bounds = new Rect(position.X, position.Y, Width, Height);
            var desktop = new Rect(SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop,
                SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
            if (desktop.IntersectsWith(bounds))
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
                Left = position.X;
                Top = position.Y;
            }
        }

        if (_configuration.Window_FormMain_Maximized)
            WindowState = WindowState.Maximized;
    }

    private IntPtr WindowMessage(IntPtr handle, int messageId, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (messageId != SingleInstanceManagement.WM_COPYDATA && messageId != SingleInstanceManagement.WM_SHOWWINDOW)
            return IntPtr.Zero;

        if (WindowState == WindowState.Minimized)
            WindowState = WindowState.Normal;

        Activate();
        if (messageId == SingleInstanceManagement.WM_COPYDATA)
        {
            var filename = SingleInstanceManagement.Catch(messageId, lParam);
            if (filename is not null)
                Dispatcher.BeginInvoke(new Action(() => _viewModel.Open(filename)));
        }

        handled = true;
        return IntPtr.Zero;
    }
}
