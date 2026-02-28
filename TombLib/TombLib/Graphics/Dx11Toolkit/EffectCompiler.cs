using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace TombLib.Graphics.Dx11Toolkit
{
    /// <summary>Result of an effect compilation.</summary>
    public class EffectCompilerResult
    {
        public bool HasErrors { get; internal set; }
        public EffectCompilerLogger Logger { get; } = new EffectCompilerLogger();
        internal byte[] VSBytecode { get; set; }
        internal byte[] PSBytecode { get; set; }
        internal string VSEntryPoint { get; set; }
        internal string PSEntryPoint { get; set; }
        internal string VSProfile { get; set; }
        internal string PSProfile { get; set; }
        internal string TechniqueName { get; set; }
    }

    /// <summary>Logger for effect compilation messages.</summary>
    public class EffectCompilerLogger
    {
        public List<string> Messages { get; } = new List<string>();
    }

    /// <summary>
    /// Compiles .fx files containing HLSL shaders with technique declarations.
    /// Uses P/Invoke to d3dcompiler_47.dll (via NativeD3DCompiler) instead of SharpDX.D3DCompiler.
    /// </summary>
    public static unsafe class EffectCompiler
    {
        // Must handle one level of brace nesting (pass { ... } inside technique { ... })
        private static readonly Regex TechniqueRegex = new Regex(
            @"technique1[01]\s+(\w+)\s*\{((?:[^{}]+|\{[^{}]*\})*)\}",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        private static readonly Regex VSRegex = new Regex(
            @"SetVertexShader\s*\(\s*CompileShader\s*\(\s*(\w+)\s*,\s*(\w+)\s*\(\s*\)\s*\)\s*\)",
            RegexOptions.IgnoreCase);

        private static readonly Regex PSRegex = new Regex(
            @"SetPixelShader\s*\(\s*CompileShader\s*\(\s*(\w+)\s*,\s*(\w+)\s*\(\s*\)\s*\)\s*\)",
            RegexOptions.IgnoreCase);

        public static EffectCompilerResult CompileFromFile(string path)
        {
            string source = File.ReadAllText(path);
            return CompileFromSource(source, path);
        }

        public static EffectCompilerResult CompileFromSource(string source, string sourceName = "effect")
        {
            var result = new EffectCompilerResult();

            var techMatch = TechniqueRegex.Match(source);
            if (!techMatch.Success)
            {
                result.HasErrors = true;
                result.Logger.Messages.Add("No technique10/technique11 block found in shader source.");
                return result;
            }

            result.TechniqueName = techMatch.Groups[1].Value;
            string passBlock = techMatch.Groups[2].Value;

            var vsMatch = VSRegex.Match(passBlock);
            if (!vsMatch.Success)
            {
                result.HasErrors = true;
                result.Logger.Messages.Add("Could not find SetVertexShader(CompileShader(...)) in technique.");
                return result;
            }
            result.VSProfile = vsMatch.Groups[1].Value;
            result.VSEntryPoint = vsMatch.Groups[2].Value;

            var psMatch = PSRegex.Match(passBlock);
            if (!psMatch.Success)
            {
                result.HasErrors = true;
                result.Logger.Messages.Add("Could not find SetPixelShader(CompileShader(...)) in technique.");
                return result;
            }
            result.PSProfile = psMatch.Groups[1].Value;
            result.PSEntryPoint = psMatch.Groups[2].Value;

            // Strip technique block (not valid HLSL for D3DCompile)
            string hlslSource = TechniqueRegex.Replace(source, "");

            // Compile VS
            result.VSBytecode = NativeD3DCompiler.Compile(
                hlslSource, result.VSEntryPoint, result.VSProfile, sourceName, out string vsError);
            if (result.VSBytecode == null)
            {
                result.HasErrors = true;
                result.Logger.Messages.Add("VS compile error: " + vsError);
                return result;
            }

            // Compile PS
            result.PSBytecode = NativeD3DCompiler.Compile(
                hlslSource, result.PSEntryPoint, result.PSProfile, sourceName, out string psError);
            if (result.PSBytecode == null)
            {
                result.HasErrors = true;
                result.Logger.Messages.Add("PS compile error: " + psError);
                return result;
            }

            return result;
        }

        /// <summary>Creates an Effect from a compiled result and a GraphicsDevice.</summary>
        public static Effect CreateEffect(GraphicsDevice device, EffectCompilerResult compiled)
        {
            if (compiled.HasErrors)
                throw new InvalidOperationException("Cannot create effect from a failed compilation.");

            // Create shader objects
            ID3D11VertexShader* vs;
            fixed (byte* pVS = compiled.VSBytecode)
            {
                int hr = device.NativeDevice->CreateVertexShader(
                    pVS, (nuint)compiled.VSBytecode.Length, null, &vs);
                Marshal.ThrowExceptionForHR(hr);
            }

            ID3D11PixelShader* ps;
            fixed (byte* pPS = compiled.PSBytecode)
            {
                int hr = device.NativeDevice->CreatePixelShader(
                    pPS, (nuint)compiled.PSBytecode.Length, null, &ps);
                Marshal.ThrowExceptionForHR(hr);
            }

            // Use shader reflection to build cbuffer layouts and parameter maps
            var parameterMap = new Dictionary<string, EffectParameter>();
            var cbufferList = new List<CBufferData>();

            ReflectShader(device, compiled.VSBytecode, parameterMap, cbufferList);
            ReflectShader(device, compiled.PSBytecode, parameterMap, cbufferList);

            // Build technique
            var pass = new EffectPass(null, vs, ps, compiled.VSBytecode);
            var technique = new EffectTechnique(compiled.TechniqueName, new[] { pass });
            var effect = new Effect(device, new[] { technique }, cbufferList.ToArray(), parameterMap);

            // Fix up pass and parameter owners via reflection
            SetPassOwner(pass, effect);

            return effect;
        }

        private static void ReflectShader(GraphicsDevice device, byte[] bytecode,
            Dictionary<string, EffectParameter> parameterMap, List<CBufferData> cbufferList)
        {
            using var reflection = NativeD3DCompiler.Reflect(bytecode);
            var shaderDesc = reflection.GetDesc();

            // Reflect constant buffers
            for (uint i = 0; i < shaderDesc.ConstantBuffers; i++)
            {
                var cbReflection = reflection.GetConstantBuffer((int)i);
                var cbDesc = cbReflection.GetDesc();

                // Skip non-cbuffer types (like tbuffer)
                if (cbDesc.Type != 0) // 0 = CT_CBUFFER
                    continue;

                // Check if this cbuffer already exists (shared between VS and PS)
                string cbName = NativeD3DCompiler.PtrToStringAnsi(cbDesc.Name);
                int cbIndex = -1;
                for (int j = 0; j < cbufferList.Count; j++)
                {
                    if (cbufferList[j].Name == cbName)
                    {
                        cbIndex = j;
                        break;
                    }
                }
                if (cbIndex < 0)
                {
                    cbIndex = cbufferList.Count;
                    cbufferList.Add(new CBufferData(device.NativeDevice, (int)cbDesc.Size, cbName));
                }

                // Reflect variables
                for (uint v = 0; v < cbDesc.Variables; v++)
                {
                    var variable = cbReflection.GetVariable((int)v);
                    var varDesc = variable.GetDesc();
                    string varName = NativeD3DCompiler.PtrToStringAnsi(varDesc.Name);

                    if (!parameterMap.ContainsKey(varName))
                    {
                        parameterMap[varName] = new EffectParameter(null)
                        {
                            CBufferIndex = cbIndex,
                            ByteOffset = (int)varDesc.StartOffset,
                            ByteSize = (int)varDesc.Size,
                        };
                    }
                }
            }

            // Reflect bound resources (textures and samplers)
            for (uint i = 0; i < shaderDesc.BoundResources; i++)
            {
                var resDesc = reflection.GetResourceBindingDesc((int)i);
                string resName = NativeD3DCompiler.PtrToStringAnsi(resDesc.Name);

                // Type 2 = SIT_TEXTURE
                if (resDesc.Type == 2 && !parameterMap.ContainsKey(resName))
                {
                    parameterMap[resName] = new EffectParameter(null)
                    {
                        IsResource = true,
                        ResourceSlot = (int)resDesc.BindPoint,
                    };
                }

                // Type 3 = SIT_SAMPLER
                if (resDesc.Type == 3 && !parameterMap.ContainsKey(resName))
                {
                    parameterMap[resName] = new EffectParameter(null)
                    {
                        IsSampler = true,
                        SamplerSlot = (int)resDesc.BindPoint,
                    };
                }
            }
        }

        private static void SetPassOwner(EffectPass pass, Effect effect)
        {
            pass.SetOwner(effect);

            foreach (var kv in effect.ParameterMap)
            {
                kv.Value.SetOwner(effect);
            }
        }
    }
}
