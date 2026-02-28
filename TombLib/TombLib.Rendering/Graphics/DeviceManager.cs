using TombLib.Graphics.Dx11Toolkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using TombLib.Rendering;

namespace TombLib.Graphics
{
    public class DeviceManager : IDisposable
    {
        // to be removed
        public static DeviceManager DefaultDeviceManager = new DeviceManager();

        //public RenderingDevice Device;
        public RenderingDevice Device;
        public GraphicsDevice ___LegacyDevice { get; set; }
        public Dictionary<string, Effect> ___LegacyEffects { get; } = new Dictionary<string, Effect>();
        public SpriteFont ___LegacyFont { get; set; }

        public DeviceManager()
        {
            Device = new Rendering.DirectX11.Dx11RenderingDevice();

            // Recreate legacy environment
            {
                // Bridge: wrap the Silk.NET native device pointer for legacy code
                var nativePtr = ((Rendering.DirectX11.Dx11RenderingDevice)Device).NativeDevicePointer;
                ___LegacyDevice = GraphicsDevice.New(nativePtr);
                LevelData.ImportedGeometry.Device = ___LegacyDevice;

                // Load legacy effects
                string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location) + "\\Rendering\\Legacy";

                if (!Directory.Exists(dir))
                {
                    MessageBox.Show("Shader files are missing. Please reinstall Tomb Editor.", "Error", MessageBoxButtons.OK);
                    throw new FileNotFoundException();
                }

                IEnumerable<string> effectFiles = Directory.EnumerateFiles(dir, "*.fx");
                foreach (string fileName in effectFiles)
                {
                    string effectName = Path.GetFileNameWithoutExtension(fileName);
                    EffectCompilerResult effect = EffectCompiler.CompileFromFile(fileName);
                    if (effect.HasErrors)
                    {
                        string errors = "";
                        foreach (var err in effect.Logger.Messages)
                            errors += err + Environment.NewLine;
                        throw new Exception("Could not compile effect '" + fileName + "'" + Environment.NewLine + errors);
                    }
                    ___LegacyEffects.Add(effectName, EffectCompiler.CreateEffect(___LegacyDevice, effect));
                }

                // Load legacy font
                SpriteFontData fontData = SpriteFontData.Load(ResourcesC.ResourcesC.font);
                fontData.DefaultCharacter = '\n'; // Don't crash on uncommon Unicode values
                ___LegacyFont = SpriteFont.New(___LegacyDevice, fontData);
            }
        }

        public void Dispose()
        {
            foreach (var effect in ___LegacyEffects.Values)
                effect?.Dispose();
            ___LegacyEffects.Clear();

            ___LegacyFont?.Dispose();
            ___LegacyFont = null;

            LevelData.ImportedGeometry.Device = null;

            ___LegacyDevice?.Dispose();
            ___LegacyDevice = null;

            Device?.Dispose();
            Device = null;
        }
    }
}
