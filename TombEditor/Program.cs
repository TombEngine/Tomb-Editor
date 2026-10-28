using DarkUI.Config;
using DarkUI.Win32;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TombLib.Forms.Services;
using TombLib.LevelData;
using TombLib.NG;
using TombLib.Services;
using TombLib.Services.Abstract;
using TombLib.Utils;
using TombLib.Wad.Catalog;
using TombLib.WPF;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor
{
    public static class Program
    {
        static Mutex mutex = new Mutex(true, "{84867F76-232B-442B-9B10-DC72C8288839}");

        [STAThread]
        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            string startFile = null;
            string batchFile = null;
            bool doBatchCompile = false;
            BatchCompileList batchList = null;

            if (args.Length >= 1)
            {
                // Open files on start
                if (args[0].EndsWith(".prj",  StringComparison.InvariantCultureIgnoreCase) ||
                    args[0].EndsWith(".prj2", StringComparison.InvariantCultureIgnoreCase))
                    startFile = args[0];

                // Batch-compile levels
                if (args[0].EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase))
                {
                    batchFile = args[0];
                    batchList = BatchCompileList.ReadFromXml(batchFile);
                    doBatchCompile = batchList?.Files.Count > 0;
                }
            }

            // Load configuration
            var initialEvents = new List<LogEventInfo>();
            var configuration = new Configuration().LoadOrUseDefault<Configuration>(initialEvents);
            configuration.EnsureDefaults();

            // Update DarkUI configuration
            Colors.Brightness = configuration.UI_FormColor_Brightness / 100.0f;

            var services = WPFInitializer.InitializeWPF(configuration.UI_WpfTheme);
            services.AddSingleton<ICustomGeometrySettingsPresetIOService, CustomGeometrySettingsPresetIOService>();
            services.AddSingleton<IColorPickerService>(_ => new ColorPickerService(() => configuration.UI_ColorScheme));
            ServiceLocator.Configure(services.BuildServiceProvider());

            if (configuration.Editor_AllowMultipleInstances || doBatchCompile ||
                mutex.WaitOne(TimeSpan.Zero, true))
            {
                // Setup logging
                using (var log = new Logging(configuration.Log_MinLevel, configuration.Log_WriteToFile, configuration.Log_ArchiveN, initialEvents))
                {
                    // Create configuration file
                    configuration.SaveTry();

                    // Setup application
                    Application.EnableVisualStyles();
                    Application.SetDefaultFont(new System.Drawing.Font("Segoe UI", 8.25f));
                    Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                    Application.ThreadException += (sender, e) =>
                    {
                        log.HandleException(e.Exception);
                        using (var dialog = new ThreadExceptionDialog(e.Exception))
                            if (dialog.ShowDialog() == DialogResult.Abort)
                                Environment.Exit(1);
                    };
                    Application.AddMessageFilter(new ControlScrollFilter());

                    // The Editor captures SynchronizationContext.Current and uses Send() to
                    // marshal events back to the UI thread. The WPF shell has no WinForms message
                    // loop, so a WindowsFormsSynchronizationContext would deadlock Send(); bind
                    // to the WPF dispatcher (already created by WPFInitializer) instead.
                    var dispatcher = System.Windows.Application.Current?.Dispatcher
                        ?? System.Windows.Threading.Dispatcher.CurrentDispatcher;
                    SynchronizationContext.SetSynchronizationContext(
                        new System.Windows.Threading.DispatcherSynchronizationContext(dispatcher));

                    if (!DefaultPaths.CheckCatalog(DefaultPaths.EngineCatalogsDirectory))
                        Environment.Exit(1);

                    // Load catalogs
                    try
                    {
                        TrCatalog.LoadCatalog(DefaultPaths.EngineCatalogsDirectory);
                        NgCatalog.LoadCatalog(Path.Combine(DefaultPaths.CatalogsDirectory, "NgCatalog.xml"));
                    }
                    catch (Exception ex)
                    {
                        log.HandleException(ex);
                        MessageBox.Show("An error occured while loading one of the catalog files.\nFiles may be missing or corrupted. Check the log file for details.");
                        Environment.Exit(1);
                    }

                    // Create the rendering device up front, as imported geometry cannot be loaded without it.
                    // Editor does not create it on construction, and batch compile mode never creates the main form that would request it.
                    _ = TombLib.Graphics.DeviceManager.DefaultDeviceManager;

                    // Run
                    Editor editor = new Editor(SynchronizationContext.Current, configuration);

                    // Run editor normally if no batch compile is pending.
                    // Otherwise, don't load main form and jump straight to batch-compiling levels.

                    if (doBatchCompile)
                    {
                        EditorActions.BuildInBatch(editor, batchList, batchFile);
                    }
                    else
                    {
                        // Default WPF shell. Catalogs/Editor are fully loaded above, so the window
                        // can already observe Editor events on first paint.
                        var wpfApp = System.Windows.Application.Current;

                        // Shared editor code shows DarkMessageBox (WinForms); route it onto the
                        // WPF CMessageBox so message boxes match the shell.
                        WpfMessageBoxBridge.Install();

                        // Without this handler, exceptions thrown during MainWindow construction or
                        // any later WPF-dispatched callback are swallowed silently — the process
                        // dies but neither the console nor NLog see anything. Route them through
                        // the standard logger and surface them so we can actually debug them.
                        wpfApp.DispatcherUnhandledException += (sender, e) =>
                        {
                            log.HandleException(e.Exception);
                            MessageBox.Show(e.Exception.ToString(), "Tomb Editor — unhandled exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            e.Handled = true;
                        };

                        try
                        {
                            var mainWindow = new MainWindow(editor);

                            // Open the file passed on the command line (or the last project, when
                            // configured) once the window has rendered, so progress/error dialogs
                            // have a live owner. ContentRendered fires once after the first layout.
                            mainWindow.ContentRendered += (_, _) =>
                            {
                                if (!string.IsNullOrEmpty(startFile))
                                {
                                    if (startFile.EndsWith(".prj", StringComparison.InvariantCultureIgnoreCase))
                                        EditorActions.OpenLevelPrj(mainWindow.GetWin32Window(), startFile);
                                    else
                                        EditorActions.OpenLevel(mainWindow.GetWin32Window(), startFile);
                                }
                                else if (editor.Configuration.Editor_OpenLastProjectOnStartup &&
                                    Properties.Settings.Default.RecentProjects != null &&
                                    Properties.Settings.Default.RecentProjects.Count > 0 &&
                                    File.Exists(Properties.Settings.Default.RecentProjects[0]))
                                {
                                    EditorActions.OpenLevel(mainWindow.GetWin32Window(), Properties.Settings.Default.RecentProjects[0]);
                                }
                            };

                            wpfApp.Run(mainWindow);
                        }
                        catch (Exception ex)
                        {
                            log.HandleException(ex);
                            MessageBox.Show(ex.ToString(), "Tomb Editor — startup failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        wpfApp.Shutdown();
                    }
                }
            }
            else if (startFile != null) // Send opening file to existing editor instance
                SingleInstanceManagement.Send(Process.GetCurrentProcess(), new List<string>() { ".prj2" }, startFile);
            else // Just bring editor to top, if user tries to launch another copy
                SingleInstanceManagement.Bump(Process.GetCurrentProcess());

            // The hosted native rendering device (DeviceManager singleton) keeps a foreground thread
            // alive, so the process would otherwise linger after the main window closes. Configuration
            // is already persisted (Editor.SaveTry on close) and logs are flushed, so terminate now.
            Environment.Exit(0);
        }
    }
}
