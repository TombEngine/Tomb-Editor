#nullable enable

using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using TombLib.Utils;

namespace TombLib.WPF.Features.About;

/// <summary>Shows application version information, credits, and header artwork.</summary>
public partial class AboutWindow : Window
{
    public AboutWindow(ImageSource aboutScreen)
    {
        InitializeComponent();

        Title = "About " + (Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "Tomb Editor");

        headerImage.Source = aboutScreen;

        string bitness = Environment.Is64BitProcess ? "64-bit" : "32-bit";
        versionText.Text = "Version " + Assembly.GetEntryAssembly()?.GetName().Version?.ToString() +
            " (.NET " + Logging.FrameworkVersion + ", " + bitness + ")";
    }

    private void Ok_Click(object sender, RoutedEventArgs e) => Close();

    private void Link_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Hyperlink link || link.Tag is not string url)
            return;

        try
        {
            Process.Start(new ProcessStartInfo("http://www." + url + "/") { UseShellExecute = true });
        }
        catch
        {
            // ignored
        }
    }
}
