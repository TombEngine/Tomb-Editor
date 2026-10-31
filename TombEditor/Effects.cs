using NLog;
using SharpDX.Toolkit.Graphics;
using System;
using System.IO;
using System.Reflection;
using TombLib.Services;
using TombLib.WPF;

namespace TombEditor
{
    class Effects
    {
        private static Assembly _editorAssembly;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static string _path;
        public static GraphicsDevice GraphicsDevice { get; set; }
        public static Effect Picking { get; set; }

        public static bool Initialize(GraphicsDevice device)
        {
            _editorAssembly = Assembly.GetExecutingAssembly();
            _path = Path.GetDirectoryName(_editorAssembly.Location);
            GraphicsDevice = device;

            try
            {
                Picking = LoadEffect("Picking");
                if (Picking == null)
                    return false;
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Could not load effect file.");
                UserDialogs.Show("Could not load effect file. " + Environment.NewLine + exc.Message, "Error", DialogButtons.OK, DialogIcon.Error);
                return false;
            }

            return true;
        }

        private static Effect LoadEffect(string name)
        {
            EffectCompilerResult result = EffectCompiler.CompileFromFile(_path + "\\Editor\\" + name + ".fx");

            if (result.HasErrors)
            {
                string errors = "";

                foreach (SharpDX.Toolkit.Diagnostics.LogMessage err in result.Logger.Messages)
                    errors += err + Environment.NewLine;

                LogManager.GetCurrentClassLogger().Log(LogLevel.Error, "Could not compile effect '" + name + ".fx'");
                UserDialogs.Show("Could not compile effect '" + name + ".fx'" + Environment.NewLine + errors, "Error", DialogButtons.OK, DialogIcon.Error);
                return null;
            }

            Effect effect = new Effect(GraphicsDevice, result.EffectData);
            return effect;
        }
    }
}
