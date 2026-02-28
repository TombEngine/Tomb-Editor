using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TombLib.Graphics.Dx11Toolkit
{
    /// <summary>
    /// Describes the vertex input layout for the Input Assembler stage.
    /// Stores managed InputElementDesc data; the native ID3D11InputLayout is created
    /// in EffectPass.Apply() using the VS bytecode.
    /// </summary>
    public unsafe class VertexInputLayout
    {
        // Managed representation of input elements (semantic + format + offset + slot).
        internal InputElementInfo[] Elements { get; }
        internal int LayoutHash { get; }

        private VertexInputLayout(InputElementInfo[] elements)
        {
            Elements = elements;

            unchecked
            {
                int hash = 17;
                foreach (var e in elements)
                {
                    hash = hash * 31 + e.SemanticName.GetHashCode();
                    hash = hash * 31 + e.SemanticIndex;
                    hash = hash * 31 + (int)e.Format;
                    hash = hash * 31 + e.AlignedByteOffset;
                    hash = hash * 31 + e.Slot;
                }
                LayoutHash = hash;
            }
        }

        /// <summary>Creates a VertexInputLayout by reflecting [VertexElement] on Buffer&lt;T&gt;.</summary>
        public static VertexInputLayout FromBuffer<T>(int slot, Buffer<T> buffer) where T : struct
            => FromType<T>(slot);

        /// <summary>Non-generic overload (unsupported — use FromBuffer&lt;T&gt; or FromType&lt;T&gt;).</summary>
        public static VertexInputLayout FromBuffer(int slot, Buffer buffer)
            => throw new InvalidOperationException("Cannot determine vertex type from non-generic Buffer. Use FromBuffer<T> or FromType<T>.");

        /// <summary>Creates a VertexInputLayout from explicit VertexElement descriptions.</summary>
        public static VertexInputLayout New(int slot, params VertexElement[] elements)
        {
            var infos = new InputElementInfo[elements.Length];
            int offset = 0;
            for (int i = 0; i < elements.Length; i++)
            {
                int byteOffset = elements[i].AlignedByteOffset >= 0 ? elements[i].AlignedByteOffset : offset;
                infos[i] = new InputElementInfo
                {
                    SemanticName = elements[i].SemanticName,
                    SemanticIndex = elements[i].SemanticIndex,
                    Format = elements[i].Format,
                    AlignedByteOffset = byteOffset,
                    Slot = slot,
                };
                offset = byteOffset + FormatSizeInBytes(elements[i].Format);
            }
            return new VertexInputLayout(infos);
        }

        /// <summary>Creates a VertexInputLayout by reflecting [VertexElement] attributes on type T.</summary>
        public static VertexInputLayout FromType<T>(int slot = 0) where T : struct
            => FromType(typeof(T), slot);

        internal static VertexInputLayout FromType(Type vertexType, int slot)
        {
            var fields = vertexType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var elements = new List<InputElementInfo>();
            int autoOffset = 0;

            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute<VertexElementAttribute>();
                if (attr == null)
                    continue;

                int byteOffset = attr.AlignedByteOffset >= 0 ? attr.AlignedByteOffset : autoOffset;
                elements.Add(new InputElementInfo
                {
                    SemanticName = attr.SemanticName,
                    SemanticIndex = attr.SemanticIndex,
                    Format = attr.Format,
                    AlignedByteOffset = byteOffset,
                    Slot = slot,
                });
                autoOffset = byteOffset + FormatSizeInBytes(attr.Format);
            }

            if (elements.Count == 0)
                elements = BuildFromStructLayout(vertexType, slot);

            return new VertexInputLayout(elements.ToArray());
        }

        /// <summary>
        /// Fallback heuristic: infers vertex semantics from field types when [VertexElement] attributes are absent.
        /// This may produce incorrect semantics for complex vertex layouts. Prefer using [VertexElement] attributes.
        /// </summary>
        private static List<InputElementInfo> BuildFromStructLayout(Type vertexType, int slot)
        {
            System.Diagnostics.Debug.WriteLine($"[VertexInputLayout] Warning: Type '{vertexType.Name}' has no [VertexElement] attributes. Using fallback struct-layout heuristic.");
            var elements = new List<InputElementInfo>();
            var fields = vertexType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            int offset = 0;
            int posIdx = 0, texIdx = 0, colIdx = 0;

            foreach (var field in fields)
            {
                DxgiFormat fmt;
                string semantic;
                int semIdx;
                var fieldType = field.FieldType;

                if (fieldType == typeof(System.Numerics.Vector4))
                { fmt = DxgiFormat.R32G32B32A32_Float; semantic = "COLOR"; semIdx = colIdx++; }
                else if (fieldType == typeof(System.Numerics.Vector3))
                { fmt = DxgiFormat.R32G32B32_Float; semantic = posIdx == 0 ? "POSITION" : "TEXCOORD"; semIdx = posIdx == 0 ? posIdx++ : texIdx++; }
                else if (fieldType == typeof(System.Numerics.Vector2))
                { fmt = DxgiFormat.R32G32_Float; semantic = "TEXCOORD"; semIdx = texIdx++; }
                else if (fieldType == typeof(float))
                { fmt = DxgiFormat.R32_Float; semantic = "TEXCOORD"; semIdx = texIdx++; }
                else
                { offset += Marshal.SizeOf(fieldType); continue; }

                elements.Add(new InputElementInfo
                {
                    SemanticName = semantic,
                    SemanticIndex = semIdx,
                    Format = fmt,
                    AlignedByteOffset = offset,
                    Slot = slot,
                });
                offset += Marshal.SizeOf(fieldType);
            }
            return elements;
        }

        /// <summary>
        /// Creates native InputElementDesc array with pinned semantic name strings.
        /// Caller must free the returned GCHandles.
        /// </summary>
        internal static InputElementDesc* CreateNativeElements(InputElementInfo[] elements,
            out GCHandle[] handles)
        {
            handles = new GCHandle[elements.Length];
            var native = (InputElementDesc*)Marshal.AllocHGlobal(
                sizeof(InputElementDesc) * elements.Length);

            for (int i = 0; i < elements.Length; i++)
            {
                byte[] nameBytes = System.Text.Encoding.ASCII.GetBytes(elements[i].SemanticName + '\0');
                handles[i] = GCHandle.Alloc(nameBytes, GCHandleType.Pinned);

                native[i] = new InputElementDesc
                {
                    SemanticName = (byte*)handles[i].AddrOfPinnedObject(),
                    SemanticIndex = (uint)elements[i].SemanticIndex,
                    Format = (Format)(int)elements[i].Format,
                    InputSlot = (uint)elements[i].Slot,
                    AlignedByteOffset = (uint)elements[i].AlignedByteOffset,
                    InputSlotClass = InputClassification.PerVertexData,
                    InstanceDataStepRate = 0,
                };
            }
            return native;
        }

        internal static void FreeNativeElements(InputElementDesc* native, GCHandle[] handles)
        {
            if (handles != null)
                foreach (var h in handles)
                    if (h.IsAllocated)
                        h.Free();
            if (native != null)
                Marshal.FreeHGlobal((IntPtr)native);
        }

        internal static int FormatSizeInBytes(DxgiFormat format)
        {
            switch (format)
            {
                case DxgiFormat.R32G32B32A32_Float:
                case DxgiFormat.R32G32B32A32_UInt:
                case DxgiFormat.R32G32B32A32_SInt:
                    return 16;
                case DxgiFormat.R32G32B32_Float:
                case DxgiFormat.R32G32B32_UInt:
                case DxgiFormat.R32G32B32_SInt:
                    return 12;
                case DxgiFormat.R32G32_Float:
                case DxgiFormat.R32G32_UInt:
                case DxgiFormat.R32G32_SInt:
                    return 8;
                case DxgiFormat.R32_Float:
                case DxgiFormat.R32_UInt:
                case DxgiFormat.R32_SInt:
                    return 4;
                case DxgiFormat.R16G16_Float:
                case DxgiFormat.R16G16_UNorm:
                    return 4;
                case DxgiFormat.R8G8B8A8_UNorm:
                    return 4;
                default:
                    return 4;
            }
        }

        /// <summary>Managed representation of a single input element.</summary>
        internal struct InputElementInfo
        {
            public string SemanticName;
            public int SemanticIndex;
            public DxgiFormat Format;
            public int AlignedByteOffset;
            public int Slot;
        }
    }
}
