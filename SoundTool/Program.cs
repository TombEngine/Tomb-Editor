#nullable enable

using DarkUI.Config;
using Microsoft.Extensions.DependencyInjection;
using SoundTool.Services;
using SoundTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using TombLib.Utils;
using TombLib.WPF;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;
using TombLib.Wad.Catalog;

namespace SoundTool;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var configuration = new Configuration().LoadOrUseDefault<Configuration>();
        using var mutex = new Mutex(false, "{CB5908BE-8C0C-435E-886D-3B3B67308FB0}");
        var (archive, referenceLevel) = ParseArguments(args);
        bool ownsMutex = false;
        try
        {
            ownsMutex = mutex.WaitOne(TimeSpan.Zero, true);
        }
        catch (AbandonedMutexException)
        {
            ownsMutex = true;
        }

        if (!configuration.SoundTool_AllowMultipleInstances && !ownsMutex)
        {
            if (archive is not null)
                SingleInstanceManagement.Send(Process.GetCurrentProcess(), [Path.GetExtension(archive)], archive);
            else
                SingleInstanceManagement.Bump(Process.GetCurrentProcess());

            return;
        }

        try
        {
            Run(configuration, archive, referenceLevel);
        }
        finally
        {
            if (ownsMutex)
                mutex.ReleaseMutex();
        }
    }

    private static void Run(Configuration configuration, string? archive, string? referenceLevel)
    {
        Colors.Brightness = configuration.UI_FormColor_Brightness / 100.0f;
        using var logging = new Logging(configuration.Log_MinLevel, configuration.Log_WriteToFile, configuration.Log_ArchiveN, []);
        var services = WPFInitializer.InitializeWPF();
        Localizer.Instance.LoadFile("SoundTool");
        services.AddSingleton(configuration);
        services.AddSingleton<ISoundToolDialogs, SoundToolDialogs>();
        services.AddSingleton<MainViewModel>();
        using var provider = services.BuildServiceProvider();
        ServiceLocator.Configure(provider);
        var application = Application.Current;
        application.DispatcherUnhandledException += (_, e) => {
            logging.HandleException(e.Exception);
            provider.GetRequiredService<IMessageService>().ShowError(e.Exception.Message, "SoundTool");
            e.Handled = true;
        };

        try
        {
            if (!DefaultPaths.CheckCatalog(DefaultPaths.EngineCatalogsDirectory))
                return;

            TrCatalog.LoadCatalog(DefaultPaths.EngineCatalogsDirectory);
            var viewModel = provider.GetRequiredService<MainViewModel>();
            var window = new MainWindow(configuration, viewModel);
            application.MainWindow = window;
            application.ShutdownMode = ShutdownMode.OnMainWindowClose;
            window.ContentRendered += Initialize;
            application.Run(window);

            void Initialize(object? sender, EventArgs e)
            {
                window.ContentRendered -= Initialize;
                viewModel.Initialize(archive, referenceLevel);
            }
        }
        catch (Exception exception)
        {
            logging.HandleException(exception);
            provider.GetRequiredService<IMessageService>().ShowError(exception.ToString(), "SoundTool startup failed");
        }
        finally
        {
            application.Shutdown();
        }
    }

    private static (string? Archive, string? ReferenceLevel) ParseArguments(IEnumerable<string> args)
    {
        string? archive = null;
        string? referenceLevel = null;
        bool referenceNext = false;
        foreach (var argument in args)
        {
            if (argument.Equals("-r", StringComparison.OrdinalIgnoreCase))
            {
                referenceNext = true;
                continue;
            }

            if (!File.Exists(argument))
                continue;

            var extension = Path.GetExtension(argument);
            if (referenceNext && extension.Equals(".prj2", StringComparison.OrdinalIgnoreCase))
                referenceLevel = argument;
            else if (!referenceNext && (extension.Equals(".xml", StringComparison.OrdinalIgnoreCase) || extension.Equals(".txt", StringComparison.OrdinalIgnoreCase)))
                archive = argument;

            referenceNext = false;
        }

        return (archive, referenceLevel);
    }
}
