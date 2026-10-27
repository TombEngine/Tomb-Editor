using System.Collections.Generic;

// Represents a single property definition loaded from an XML property catalog.
// Used to describe what properties are available for a given object type and
// to provide metadata for the property grid UI (name, description, type, default).

namespace TombLib.LuaProperties
{
    /// <summary>
    /// Defines a single property that can appear in the Lua property grid.
    /// Loaded from XML catalog files in "Catalogs/TEN Property Catalogs".
    /// </summary>
    public class LuaPropertyDefinition
    {
        /// <summary>
        /// User-friendly display name shown in the property grid's label column.
        /// Example: "Melee damage"
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Internal Lua API property name used in SetProperty/GetProperty calls.
        /// Example: "meleeDamage"
        /// </summary>
        public string InternalName { get; set; } = string.Empty;

        /// <summary>
        /// Tooltip description for the property.
        /// Example: "A damage value applied in melee combat"
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// When true, it means that this property supersedes legacy OCB functionality.
        /// If an ItemInstance has a non-zero OCB value and any of its properties
        /// has this flag set, the editor will warn the user to reset OCB to 0.
        /// Controlled by the "replacesOCB" attribute in XML catalogs.
        /// </summary>
        public bool ReplacesOCB { get; set; } = false;

        /// <summary>
        /// The Lua value type of this property.
        /// </summary>
        public LuaPropertyType Type { get; set; } = LuaPropertyType.Float;

        /// <summary>
        /// Optional category grouping for the property grid.
        /// Properties without a category are displayed at the top level.
        /// Example: "Battle", "Logic", "Physics"
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Default value in boxed Lua format.
        /// Must be compatible with the declared Type.
        /// </summary>
        public string DefaultValue { get; set; } = string.Empty;

        /// <summary>
        /// Optional minimum value for numeric properties.
        /// Used by Int, Float, Vec2 and Vec3 editors.
        /// </summary>
        public double MinValue { get; set; } = (double)int.MinValue;

        /// <summary>
        /// Optional maximum value for numeric properties.
        /// Used by Int, Float, Vec2 and Vec3 editors.
        /// </summary>
        public double MaxValue { get; set; } = (double)int.MaxValue;

        /// <summary>
        /// For Color properties: whether the alpha channel is editable.
        /// When false, the alpha field is hidden in the property grid.
        /// Controlled by the "hasAlpha" attribute in XML catalogs.
        /// </summary>
        public bool HasAlpha { get; set; } = false;

        /// <summary>
        /// For Enum properties: the ordered list of entry names.
        /// Index 0 corresponds to Lua integer value 0, index 1 to 1, etc.
        /// Populated from the <c>entries</c> XML attribute or child &lt;entry&gt; nodes.
        /// </summary>
        public List<string> EnumValues { get; set; } = new List<string>();

        /// <summary>
        /// Returns true if the definition has all required fields filled in.
        /// </summary>
        public bool IsValid => !string.IsNullOrWhiteSpace(InternalName) && !string.IsNullOrWhiteSpace(DisplayName);

        public override string ToString() => $"{DisplayName} ({InternalName}: {Type})";
    }
}
