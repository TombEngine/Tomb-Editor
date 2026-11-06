#nullable enable

using DarkUI.WPF.Config;
using DarkUI.WPF.CustomControls;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using TombLib.Services;
using TombLib.Services.Abstract;
using TombLib.Utils;
using TombLib.Wad.Catalog;
using TombLib.WPF;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace WadTool;

internal static class Program
{
    /// <summary>Starts the WPF application with the requested WAD and reference level.</summary>
    [STAThread]
    public static void Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var initialEvents = new List<LogEventInfo>();
        var configuration = new Configuration().LoadOrUseDefault<Configuration>(initialEvents);
        Colors.Brightness = configuration.UI_FormColor_Brightness / 100.0f;

        var services = WPFInitializer.InitializeWPF();
        services.AddSingleton<ICustomGeometrySettingsPresetIOService, CustomGeometrySettingsPresetIOService>();
        services.AddSingleton<IColorPickerService, ColorPickerService>();
        using var provider = services.BuildServiceProvider();
        ServiceLocator.Configure(provider);
        using var log = new Logging(configuration.Log_MinLevel, configuration.Log_WriteToFile, configuration.Log_ArchiveN, initialEvents);
        configuration.SaveTry();
        if (!DefaultPaths.CheckCatalog(DefaultPaths.EngineCatalogsDirectory, message => provider.GetRequiredService<IMessageService>().ShowError(message, "WadTool")))
            return;

        TrCatalog.LoadCatalog(DefaultPaths.EngineCatalogsDirectory);
        var application = Application.Current ?? throw new InvalidOperationException("WPF initialization did not create an application.");
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(application.Dispatcher));
        UseLeftAlignedNumericFields(application);
        Run(application, configuration, args, log, provider.GetRequiredService<IMessageService>());
        application.Shutdown();

        // The shared rendering device owns a foreground thread after the application shuts down.
        Environment.Exit(0);
    }

    // The WinForms WadTool shows the values of its numeric fields left aligned (Tomb Editor right aligned them).
    private static void UseLeftAlignedNumericFields(Application application)
    {
        var style = new Style(typeof(NumericUpDown), application.TryFindResource(typeof(NumericUpDown)) as Style);
        style.Setters.Add(new Setter(NumericUpDown.TextAlignmentProperty, TextAlignment.Left));
        application.Resources[typeof(NumericUpDown)] = style;
    }

    private static void Run(Application application, Configuration configuration, string[] args, Logging log, IMessageService messages)
    {
        void ReportException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            log.HandleException(e.Exception);
            messages.ShowError(e.Exception.ToString(), "WadTool - unhandled exception");
            e.Handled = true;
        }

        application.DispatcherUnhandledException += ReportException;
        using var tool = new WadToolClass(configuration);
        try
        {
            var startup = ParseArguments(args);
            var mainWindow = new MainWindow(tool);
            mainWindow.ContentRendered += (_, _) => {
                if (startup.ReferenceLevel is not null)
                    WadActions.LoadReferenceLevel(tool, mainWindow.GetWin32Window(), startup.ReferenceLevel);
                if (startup.Wad is not null)
                    WadActions.LoadWad(tool, mainWindow.GetWin32Window(), true, startup.Wad);
            };
            application.Run(mainWindow);
        }
        catch (Exception exception)
        {
            log.HandleException(exception);
            messages.ShowError(exception.ToString(), "WadTool - startup failure");
        }
        finally
        {
            application.DispatcherUnhandledException -= ReportException;
        }
    }

    private static (string? Wad, string? ReferenceLevel) ParseArguments(IEnumerable<string> arguments)
    {
        string? wad = null;
        string? referenceLevel = null;
        bool loadReferenceLevel = false;
        foreach (var argument in arguments)
        {
            if (argument.Equals("-r", StringComparison.OrdinalIgnoreCase))
            {
                loadReferenceLevel = true;
                continue;
            }

            if (!File.Exists(argument))
                continue;

            if (loadReferenceLevel)
            {
                if (argument.EndsWith("prj2", StringComparison.OrdinalIgnoreCase))
                    referenceLevel = argument;
            }
            else
            {
                wad = argument;
            }

            loadReferenceLevel = false;
        }

        return (wad, referenceLevel);
    }
}
