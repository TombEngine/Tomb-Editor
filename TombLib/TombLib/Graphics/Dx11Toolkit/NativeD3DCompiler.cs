using System;
using System.Runtime.InteropServices;

namespace TombLib.Graphics.Dx11Toolkit
{
    /// <summary>
    /// Minimal P/Invoke wrappers for d3dcompiler_47.dll functions and related COM interfaces.
    /// Replaces SharpDX.D3DCompiler dependency with direct native calls.
    /// </summary>
    internal static unsafe class NativeD3DCompiler
    {
        // ── D3DCompile / D3DReflect ──────────────────────────────────

        private const uint D3DCOMPILE_OPTIMIZATION_LEVEL3 = (1 << 15);
        private const uint D3DCOMPILE_PACK_MATRIX_ROW_MAJOR = (1 << 3);

        [DllImport("d3dcompiler_47.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int D3DCompile(
            void* pSrcData, nuint srcDataSize,
            [MarshalAs(UnmanagedType.LPStr)] string pSourceName,
            void* pDefines, void* pInclude,
            [MarshalAs(UnmanagedType.LPStr)] string pEntrypoint,
            [MarshalAs(UnmanagedType.LPStr)] string pTarget,
            uint flags1, uint flags2,
            void** ppCode, void** ppErrorMsgs);

        [DllImport("d3dcompiler_47.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int D3DReflect(
            void* pSrcData, nuint srcDataSize,
            Guid* riid, void** ppReflector);

        // D3D_COMPILE_STANDARD_FILE_INCLUDE = (void*)1 — tells D3DCompile
        // to handle #include directives relative to the source file path.
        private static readonly void* STANDARD_FILE_INCLUDE = (void*)1;

        // IID_ID3D11ShaderReflection {8d536ca1-0cca-4956-a837-786963755584}
        private static Guid IID_ID3D11ShaderReflection = new Guid(0x8d536ca1, 0x0cca, 0x4956, 0xa8, 0x37, 0x78, 0x69, 0x63, 0x75, 0x55, 0x84);

        // ── ID3DBlob helpers (vtable: 0=QI, 1=AddRef, 2=Release, 3=GetBufferPointer, 4=GetBufferSize) ──

        private static void* BlobGetBufferPointer(void* blob)
        {
            var vtbl = *(void***)blob;
            return ((delegate* unmanaged[Stdcall]<void*, void*>)vtbl[3])(blob);
        }

        private static nuint BlobGetBufferSize(void* blob)
        {
            var vtbl = *(void***)blob;
            return ((delegate* unmanaged[Stdcall]<void*, nuint>)vtbl[4])(blob);
        }

        private static void BlobRelease(void* blob)
        {
            if (blob != null)
            {
                var vtbl = *(void***)blob;
                ((delegate* unmanaged[Stdcall]<void*, uint>)vtbl[2])(blob);
            }
        }

        // ── Public compilation API ───────────────────────────────────

        /// <summary>Compiles HLSL source using d3dcompiler_47.dll.</summary>
        /// <returns>Compiled bytecode, or null on failure (errorMessage populated).</returns>
        public static byte[] Compile(string hlslSource, string entryPoint, string profile,
            string sourceName, out string errorMessage)
        {
            errorMessage = null;
            byte[] sourceBytes = System.Text.Encoding.UTF8.GetBytes(hlslSource + '\0');

            void* codeBlob = null;
            void* errorBlob = null;

            try
            {
                int hr;
                fixed (byte* pSource = sourceBytes)
                {
                    hr = D3DCompile(pSource, (nuint)(sourceBytes.Length - 1),
                        sourceName, null, STANDARD_FILE_INCLUDE,
                        entryPoint, profile,
                        D3DCOMPILE_OPTIMIZATION_LEVEL3 | D3DCOMPILE_PACK_MATRIX_ROW_MAJOR, 0,
                        &codeBlob, &errorBlob);
                }

                if (hr < 0 || codeBlob == null)
                {
                    if (errorBlob != null)
                    {
                        var errPtr = (byte*)BlobGetBufferPointer(errorBlob);
                        var errLen = (int)BlobGetBufferSize(errorBlob);
                        errorMessage = System.Text.Encoding.UTF8.GetString(errPtr, errLen).TrimEnd('\0');
                    }
                    else
                    {
                        errorMessage = $"D3DCompile failed with HRESULT 0x{hr:X8}";
                    }
                    return null;
                }

                var ptr = (byte*)BlobGetBufferPointer(codeBlob);
                var size = (int)BlobGetBufferSize(codeBlob);
                var bytecode = new byte[size];
                Marshal.Copy((IntPtr)ptr, bytecode, 0, size);
                return bytecode;
            }
            finally
            {
                BlobRelease(codeBlob);
                BlobRelease(errorBlob);
            }
        }

        // ── Shader Reflection ────────────────────────────────────────

        /// <summary>Creates a shader reflection instance from compiled bytecode.</summary>
        public static ShaderReflectionWrapper Reflect(byte[] bytecode)
        {
            void* reflector = null;
            fixed (byte* pBytecode = bytecode)
            fixed (Guid* pIID = &IID_ID3D11ShaderReflection)
            {
                int hr = D3DReflect(pBytecode, (nuint)bytecode.Length, pIID, &reflector);
                if (hr < 0 || reflector == null)
                    throw new InvalidOperationException($"D3DReflect failed with HRESULT 0x{hr:X8}");
            }
            return new ShaderReflectionWrapper(reflector);
        }

        // ── Reflection wrappers ──────────────────────────────────────

        /// <summary>Wraps ID3D11ShaderReflection COM interface.</summary>
        internal class ShaderReflectionWrapper : IDisposable
        {
            private void* _ptr;

            internal ShaderReflectionWrapper(void* ptr) { _ptr = ptr; }

            // ID3D11ShaderReflection vtable (d3d11shader.h):
            //  0: QueryInterface (IUnknown)
            //  1: AddRef         (IUnknown)
            //  2: Release        (IUnknown)
            //  3: GetDesc
            //  4: GetConstantBufferByIndex
            //  5: GetConstantBufferByName
            //  6: GetResourceBindingDesc
            //  7+: GetInputParameterDesc, GetOutputParameterDesc, etc.

            public ShaderDesc GetDesc()
            {
                var vtbl = *(void***)_ptr;
                ShaderDesc desc;
                int hr = ((delegate* unmanaged[Stdcall]<void*, ShaderDesc*, int>)vtbl[3])(_ptr, &desc);
                if (hr < 0) throw new InvalidOperationException($"GetDesc failed: 0x{hr:X8}");
                return desc;
            }

            public ConstantBufferReflection GetConstantBuffer(int index)
            {
                var vtbl = *(void***)_ptr;
                void* cb = ((delegate* unmanaged[Stdcall]<void*, uint, void*>)vtbl[4])(_ptr, (uint)index);
                return new ConstantBufferReflection(cb);
            }

            public ShaderInputBindDesc GetResourceBindingDesc(int index)
            {
                var vtbl = *(void***)_ptr;
                ShaderInputBindDesc desc;
                int hr = ((delegate* unmanaged[Stdcall]<void*, uint, ShaderInputBindDesc*, int>)vtbl[6])(_ptr, (uint)index, &desc);
                if (hr < 0) throw new InvalidOperationException($"GetResourceBindingDesc failed: 0x{hr:X8}");
                return desc;
            }

            public void Dispose()
            {
                if (_ptr != null)
                {
                    var vtbl = *(void***)_ptr;
                    ((delegate* unmanaged[Stdcall]<void*, uint>)vtbl[2])(_ptr); // Release
                    _ptr = null;
                }
            }
        }

        /// <summary>Wraps ID3D11ShaderReflectionConstantBuffer (not IUnknown-derived).</summary>
        internal readonly struct ConstantBufferReflection
        {
            private readonly void* _ptr;
            internal ConstantBufferReflection(void* ptr) { _ptr = ptr; }

            // ID3D11ShaderReflectionConstantBuffer vtable (d3d11shader.h):
            // 0: GetDesc
            // 1: GetVariableByIndex
            // 2: GetVariableByName

            public ShaderBufferDesc GetDesc()
            {
                var vtbl = *(void***)_ptr;
                ShaderBufferDesc desc;
                int hr = ((delegate* unmanaged[Stdcall]<void*, ShaderBufferDesc*, int>)vtbl[0])(_ptr, &desc);
                if (hr < 0) throw new InvalidOperationException($"CBuffer GetDesc failed: 0x{hr:X8}");
                return desc;
            }

            public VariableReflection GetVariable(int index)
            {
                var vtbl = *(void***)_ptr;
                void* v = ((delegate* unmanaged[Stdcall]<void*, uint, void*>)vtbl[1])(_ptr, (uint)index);
                return new VariableReflection(v);
            }
        }

        /// <summary>Wraps ID3D11ShaderReflectionVariable (not IUnknown-derived).</summary>
        internal readonly struct VariableReflection
        {
            private readonly void* _ptr;
            internal VariableReflection(void* ptr) { _ptr = ptr; }

            // ID3D11ShaderReflectionVariable vtable (d3d11shader.h):
            // 0: GetDesc
            // 1: GetType
            // 2: GetBuffer

            public ShaderVariableDesc GetDesc()
            {
                var vtbl = *(void***)_ptr;
                ShaderVariableDesc desc;
                int hr = ((delegate* unmanaged[Stdcall]<void*, ShaderVariableDesc*, int>)vtbl[0])(_ptr, &desc);
                if (hr < 0) throw new InvalidOperationException($"Variable GetDesc failed: 0x{hr:X8}");
                return desc;
            }
        }

        // ── Native structs ───────────────────────────────────────────

        [StructLayout(LayoutKind.Sequential)]
        internal struct ShaderDesc
        {
            public uint Version;
            public byte* Creator;
            public uint Flags;
            public uint ConstantBuffers;
            public uint BoundResources;
            public uint InputParameters;
            public uint OutputParameters;
            public uint InstructionCount;
            public uint TempRegisterCount;
            public uint TempArrayCount;
            public uint DefCount;
            public uint DclCount;
            public uint TextureNormalInstructions;
            public uint TextureLoadInstructions;
            public uint TextureCompInstructions;
            public uint TextureBiasInstructions;
            public uint TextureGradientInstructions;
            public uint FloatInstructionCount;
            public uint IntInstructionCount;
            public uint UintInstructionCount;
            public uint StaticFlowControlCount;
            public uint DynamicFlowControlCount;
            public uint MacroInstructionCount;
            public uint ArrayInstructionCount;
            public uint CutInstructionCount;
            public uint EmitInstructionCount;
            public uint GSOutputTopology;
            public uint GSMaxOutputVertexCount;
            public uint InputPrimitive;
            public uint PatchConstantParameters;
            public uint cGSInstanceCount;
            public uint cControlPoints;
            public uint HSOutputPrimitive;
            public uint HSPartitioning;
            public uint TessellatorDomain;
            public uint cBarrierInstructions;
            public uint cInterlockedInstructions;
            public uint cTextureStoreInstructions;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct ShaderBufferDesc
        {
            public byte* Name;           // LPCSTR
            public uint Type;            // D3D_CBUFFER_TYPE (0=CT_CBUFFER, 1=CT_TBUFFER)
            public uint Variables;
            public uint Size;
            public uint uFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct ShaderVariableDesc
        {
            public byte* Name;           // LPCSTR
            public uint StartOffset;
            public uint Size;
            public uint uFlags;
            public void* DefaultValue;
            public uint StartTexture;
            public uint TextureSize;
            public uint StartSampler;
            public uint SamplerSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct ShaderInputBindDesc
        {
            public byte* Name;           // LPCSTR
            public uint Type;            // D3D_SHADER_INPUT_TYPE (2=SIT_TEXTURE)
            public uint BindPoint;
            public uint BindCount;
            public uint uFlags;
            public uint ReturnType;
            public uint Dimension;
            public uint NumSamples;
        }

        // Helper to read LPCSTR (null-terminated ANSI string) from a native pointer.
        internal static string PtrToStringAnsi(byte* ptr)
        {
            if (ptr == null) return null;
            int len = 0;
            while (ptr[len] != 0) len++;
            return System.Text.Encoding.ASCII.GetString(ptr, len);
        }
    }
}
