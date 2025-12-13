using System;
using System.Xml.Serialization;

namespace TombLib.GeometryIO
{
    public record IOGeometrySettingsPreset
    {
        public string Name { get; set; }
        public IOGeometrySettings Settings { get; set; }

        [XmlIgnore]
        public bool IsCustom { get; set; }

        [Obsolete("For XML deserialization purposes only. Do not use directly.", true)]
        public IOGeometrySettingsPreset() : this(string.Empty, new IOGeometrySettings(), true)
        { }

        public IOGeometrySettingsPreset(string name, IOGeometrySettings settings, bool isCustom = false)
        {
            Name = name;
            Settings = settings;
            IsCustom = isCustom;
        }

        public override string ToString() => Name;
    }

    public record IOGeometryInternalSettings
    {
        [XmlIgnore] public bool Export { get; init; } = false;
        [XmlIgnore] public bool ExportRoom { get; init; } = false;
        [XmlIgnore] public bool ImportWadMesh { get; init; } = false;
        [XmlIgnore] public bool ProcessGeometry { get; init; } = true;
        [XmlIgnore] public bool ProcessUntexturedGeometry { get; set; } = false;
        [XmlIgnore] public bool ProcessAnimations { get; init; } = false;

        [XmlIgnore] public bool IsGeometryImportSettings => !Export && !ProcessAnimations;
        [XmlIgnore] public bool IsGeometryExportSettings => Export && !ProcessAnimations;
        [XmlIgnore] public bool IsAnimationSettings => ProcessAnimations;
    }

    public record IOGeometrySettings : IOGeometryInternalSettings
    {
        public bool SwapXY { get; set; } = false;
        public bool SwapXZ { get; set; } = false;
        public bool SwapYZ { get; set; } = false;
        public bool FlipX { get; set; } = false;
        public bool FlipY { get; set; } = false;
        public bool FlipZ { get; set; } = true;
        public bool FlipUV_V { get; set; } = true;
        public float Scale { get; set; } = 1.0f;
        public bool MappedUV { get; set; } = true;
        public bool WrapUV { get; set; } = true;
        public bool PremultiplyUV { get; set; } = true;
        public bool InvertFaces { get; set; } = false;
        public bool UseVertexColor { get; set; } = true;
        public bool SortByName { get; set; } = true;
        public bool PackTextures { get; set; } = true;
        public bool PadPackedTextures { get; set; } = true;
        public bool KeepTexturesExternal { get; set; } = false;
	}
}
