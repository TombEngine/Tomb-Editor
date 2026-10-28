#nullable enable

using DarkUI.Config;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using TombLib.Forms.Services;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.NG;
using TombLib.Services;
using TombLib.Services.Abstract;
using TombLib.Utils;
using TombLib.WPF;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;
using TombLib.Wad.Catalog;

namespace TombEditor;

public static class Program
{
    private static readonly Mutex InstanceMutex = new(true, "{84867F76-232B-442B-9B10-DC72C8288839}");

    [DllImport("user32.dll")]
    private static extern bool SetProcessDpiAwarenessContext(IntPtr dpiContext);

    [STAThread]
    public static void Main(string[] args)
    {
        SetProcessDpiAwarenessContext(new IntPtr(-4));
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        string? startFile = GetStartFile(args);
        string? batchFile = args.Length > 0 && args[0].EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase) ? args[0] : null;
        var batchList = batchFile is not null ? BatchCompileList.ReadFromXml(batchFile) : null;
        bool doBatchCompile = batchList?.Files.Count > 0;
        var initialEvents = new List<LogEventInfo>();
        var configuration = new Configuration().LoadOrUseDefault<Configuration>(initialEvents);
        configuration.EnsureDefaults();
        Colors.Brightness = configuration.UI_FormColor_Brightness / 100.0f;

        var services = WPFInitializer.InitializeWPF(configuration.UI_WpfTheme);
        services.AddSingleton<ICustomGeometrySettingsPresetIOService, CustomGeometrySettingsPresetIOService>();
        services.AddSingleton<IColorPickerService>(_ => new ColorPickerService(() => configuration.UI_ColorScheme));
        ServiceLocator.Configure(services.BuildServiceProvider());

        if (configuration.Editor_AllowMultipleInstances || doBatchCompile || InstanceMutex.WaitOne(TimeSpan.Zero, true))
        {
            using var log = new Logging(configuration.Log_MinLevel, configuration.Log_WriteToFile, configuration.Log_ArchiveN, initialEvents);
            configuration.SaveTry();
            var application = Application.Current ?? throw new InvalidOperationException("WPF initialization did not create an application.");
            var messages = ServiceLocator.ResolveService<IMessageService>();
            ConfigureExceptionHandling(application, log, messages);
            var synchronizationContext = new DispatcherSynchronizationContext(application.Dispatcher);
            SynchronizationContext.SetSynchronizationContext(synchronizationContext);
            LoadCatalogs(log, messages);

            // Imported geometry needs the device even when batch mode has no viewport.
            _ = DeviceManager.DefaultDeviceManager;
            using var editor = new Editor(synchronizationContext, configuration);

            if (doBatchCompile && batchList is not null && batchFile is not null)
                EditorActions.BuildInBatch(editor, batchList, batchFile);
            else
                RunEditor(application, editor, startFile, log, messages);
        }
        else if (startFile is not null)
        {
            SingleInstanceManagement.Send(Process.GetCurrentProcess(), [".prj2"], startFile);
        }
        else
        {
            SingleInstanceManagement.Bump(Process.GetCurrentProcess());
        }

        // The device manager's native foreground thread outlives the application dispatcher.
        Environment.Exit(0);
    }

    private static string? GetStartFile(string[] args)
    {
        if (args.Length == 0)
            return null;

        return args[0].EndsWith(".prj", StringComparison.InvariantCultureIgnoreCase)
            || args[0].EndsWith(".prj2", StringComparison.InvariantCultureIgnoreCase) ? args[0] : null;
    }

    private static void ConfigureExceptionHandling(Application application, Logging log, IMessageService messages)
    {
        application.DispatcherUnhandledException += (_, e) => {
            log.HandleException(e.Exception);
            e.Handled = true;

            if (!messages.ShowConfirmation(e.Exception + Environment.NewLine + Environment.NewLine + "Continue running Tomb Editor?",
                "Tomb Editor - unhandled exception", defaultValue: false))
            {
                Environment.Exit(1);
            }
        };
    }

    private static void LoadCatalogs(Logging log, IMessageService messages)
    {
        if (!Directory.Exists(DefaultPaths.EngineCatalogsDirectory))
        {
            messages.ShowError("Directory " + DefaultPaths.EngineCatalogsDirectory + " is missing. Make sure that Tomb Editor is installed correctly.");
            Environment.Exit(1);
        }

        try
        {
            TrCatalog.LoadCatalog(DefaultPaths.EngineCatalogsDirectory);
            NgCatalog.LoadCatalog(Path.Combine(DefaultPaths.CatalogsDirectory, "NgCatalog.xml"));
        }
        catch (Exception exception)
        {
            log.HandleException(exception);
            messages.ShowError("An error occurred while loading a catalog file. Files may be missing or corrupted. Check the log file for details.");
            Environment.Exit(1);
        }
    }

    private static void RunEditor(Application application, Editor editor, string? startFile, Logging log, IMessageService messages)
    {
        WpfMessageBoxBridge.Install();

        try
        {
            var mainWindow = new MainWindow(editor);
            mainWindow.ContentRendered += (_, _) => OpenStartupLevel(mainWindow, editor, startFile);
            application.Run(mainWindow);
        }
        catch (Exception exception)
        {
            log.HandleException(exception);
            messages.ShowError(exception.ToString(), "Tomb Editor - startup failure");
        }

        application.Shutdown();
    }

    private static void OpenStartupLevel(MainWindow mainWindow, Editor editor, string? startFile)
    {
        if (startFile is not null)
        {
            if (startFile.EndsWith(".prj", StringComparison.InvariantCultureIgnoreCase))
                EditorActions.OpenLevelPrj(mainWindow.GetWin32Window(), startFile);
            else
                EditorActions.OpenLevel(mainWindow.GetWin32Window(), startFile);

            return;
        }

        var recentProjects = Properties.Settings.Default.RecentProjects;

        if (editor.Configuration.Editor_OpenLastProjectOnStartup && recentProjects is { Count: > 0 } && File.Exists(recentProjects[0]))
            EditorActions.OpenLevel(mainWindow.GetWin32Window(), recentProjects[0]);
    }
}
