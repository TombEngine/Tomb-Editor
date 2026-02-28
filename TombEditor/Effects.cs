using NLog;
using TombLib.Graphics.Dx11Toolkit;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

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
                MessageBox.Show("Could not load effect file. " + Environment.NewLine + exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                foreach (string err in result.Logger.Messages)
                    errors += err + Environment.NewLine;

                LogManager.GetCurrentClassLogger().Log(LogLevel.Error, "Could not compile effect '" + name + ".fx'");
                MessageBox.Show("Could not compile effect '" + name + ".fx'" + Environment.NewLine + errors, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            Effect effect = EffectCompiler.CreateEffect(GraphicsDevice, result);
            return effect;
        }
    }
}
