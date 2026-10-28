#nullable enable

using DarkUI.Config;
using DarkUI.WPF;
using Microsoft.Win32;
using SoundTool.ViewModels;
using SoundTool.Views;
using System;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using TombLib.Utils;
using TombLib.WPF.Features.About;
using TombLib.WPF.Features.AnimatedTextures;

namespace SoundTool.Services;

/// <summary>Uses native Windows file dialogs and themed WPF application dialogs.</summary>
public sealed class SoundToolDialogs : ISoundToolDialogs
{
    private Window Owner => Application.Current.MainWindow;

    public string? OpenFile(string title, string filter, string? initialPath = null)
    {
        var dialog = new OpenFileDialog { Title = title, Filter = filter };
        SetPath(dialog, initialPath);
        return dialog.ShowDialog(Owner) == true ? dialog.FileName : null;
    }

    public string[] OpenSamples()
    {
        var dialog = new OpenFileDialog { Title = "Choose samples", Filter = "Waveform audio|*.wav", Multiselect = true };
        return dialog.ShowDialog(Owner) == true ? dialog.FileNames : [];
    }

    public string? SaveFile(string title, string filter, string? initialPath = null)
    {
        var dialog = new SaveFileDialog { Title = title, Filter = filter, AddExtension = true };
        SetPath(dialog, initialPath);
        return dialog.ShowDialog(Owner) == true ? dialog.FileName : null;
    }

    public string? SelectFolder(string title, string? initialPath = null)
    {
        using var dialog = new BrowseFolderDialog
        {
            Title = title,
            InitialFolder = initialPath ?? string.Empty
        };
        var owner = new NativeOwner(new WindowInteropHelper(Owner).Handle);
        return dialog.ShowDialog(owner) == System.Windows.Forms.DialogResult.OK ? dialog.Folder : null;
    }

    public string? Prompt(string title, string label, string value = "")
    {
        var viewModel = new InputDialogViewModel(title, label, value);
        var dialog = new InputDialog { Owner = Owner, DataContext = viewModel };
        return dialog.ShowDialog() == true ? viewModel.Value : null;
    }

    public void ShowOptions(Configuration configuration)
    {
        var viewModel = new OptionsViewModel(configuration);
        var dialog = new OptionsWindow { Owner = Owner, DataContext = viewModel };
        if (dialog.ShowDialog() != true)
            return;

        configuration.SoundTool_AllowMultipleInstances = viewModel.AllowMultipleInstances;
        configuration.UI_FormColor_Brightness = viewModel.Brightness;
        Colors.Brightness = viewModel.Brightness / 100.0f;
        ThemeManager.Apply(Defaults.CurrentTheme);
        configuration.SaveTry();
    }

    public void ShowAbout()
    {
        var artwork = new BitmapImage(new Uri("pack://application:,,,/SoundTool;component/Resources/misc/misc_AboutScreen_800.png"));
        new AboutWindow(artwork) { Owner = Owner }.ShowDialog();
    }

    public void Close() => Owner.Close();

    private sealed record NativeOwner(IntPtr Handle) : System.Windows.Forms.IWin32Window;

    private static void SetPath(FileDialog dialog, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        if (Directory.Exists(path))
            dialog.InitialDirectory = path;
        else
        {
            dialog.FileName = Path.GetFileName(path);
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                dialog.InitialDirectory = directory;
        }
    }
}
