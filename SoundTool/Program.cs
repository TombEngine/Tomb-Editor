using DarkUI.Config;
using DarkUI.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace SoundTool
{
    static class Program
    {
        static Mutex mutex = new Mutex(true, "{CB5908BE-8C0C-435E-886D-3B3B67308FB0}");

        [STAThread]
        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var extensions = new List<string>() { ".xml", ".txt" };

            string startFile = null;
            string refLevel  = null;

            if (args.Length > 0) // Open files on start
            {
                bool loadAsRefLevel = false;

                foreach (var arg in args)
                {
                    if (arg.Equals("-r", StringComparison.InvariantCultureIgnoreCase))
                        loadAsRefLevel = true;
                    else
                    {
                        if (!File.Exists(arg))
                            continue; // No file and no valid argument, don't even try to load anything

                        if (loadAsRefLevel)
                        {
                            if (arg.EndsWith("prj2", StringComparison.InvariantCultureIgnoreCase))
                                refLevel = arg;
                        }
                        else
                            foreach (var ext in extensions)
                            {
                                if (arg.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase))
                                    startFile = arg;
                            }

                        loadAsRefLevel = false; // Reset arg mode if no expected path was found next to it
                    }
                }
            }

            // Load configuration
            var configuration = new Configuration().LoadOrUseDefault<Configuration>();

            // Update DarkUI configuration
            Colors.Brightness = configuration.UI_FormColor_Brightness / 100.0f;

            if (configuration.SoundTool_AllowMultipleInstances || 
                mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetDefaultFont(new System.Drawing.Font("Segoe UI", 8.25f));
                Application.SetCompatibleTextRenderingDefault(false);
                Application.AddMessageFilter(new ControlScrollFilter());

                if (!DefaultPaths.CheckCatalog(DefaultPaths.EngineCatalogsDirectory))
                    Environment.Exit(1);

                TrCatalog.LoadCatalog(DefaultPaths.EngineCatalogsDirectory);

                using (FormMain form = new FormMain(configuration, startFile, refLevel))
                {
                    form.Show();
                    Application.Run(form);
                }
            }
            else if (startFile != null)
                SingleInstanceManagement.Send(Process.GetCurrentProcess(), extensions, startFile);
            else
                SingleInstanceManagement.Bump(Process.GetCurrentProcess());
        }
    }
}
