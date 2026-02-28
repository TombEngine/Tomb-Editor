using System;

namespace TombLib.Graphics.Dx11Toolkit
{
    /// <summary>
    /// Attribute for declaring vertex element semantics on struct fields.
    /// Uses our own DxgiFormat façade enum (no SharpDX dependency).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class VertexElementAttribute : Attribute
    {
        public string SemanticName { get; }
        public int SemanticIndex { get; }
        public DxgiFormat Format { get; }
        public int AlignedByteOffset { get; }

        public VertexElementAttribute(string semanticName, int semanticIndex, DxgiFormat format, int alignedByteOffset)
        {
            SemanticName = semanticName;
            SemanticIndex = semanticIndex;
            Format = format;
            AlignedByteOffset = alignedByteOffset;
        }
    }

    /// <summary>
    /// Describes a vertex element for programmatic input layout creation.
    /// </summary>
    public struct VertexElement
    {
        public string SemanticName { get; }
        public int SemanticIndex { get; }
        public DxgiFormat Format { get; }
        public int AlignedByteOffset { get; }

        public VertexElement(string semanticNameWithIndex, int alignedByteOffset, DxgiFormat format)
        {
            // Parse "POSITION0" → ("POSITION", 0)
            int idx = semanticNameWithIndex.Length - 1;
            while (idx >= 0 && char.IsDigit(semanticNameWithIndex[idx]))
                idx--;
            SemanticName = semanticNameWithIndex.Substring(0, idx + 1);
            SemanticIndex = idx < semanticNameWithIndex.Length - 1
                ? int.Parse(semanticNameWithIndex.Substring(idx + 1))
                : 0;
            Format = format;
            AlignedByteOffset = alignedByteOffset;
        }
    }
}
