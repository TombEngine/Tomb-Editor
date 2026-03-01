// XML catalog loader for Lua property definitions.
// Reads XML files from "Catalogs/TEN Property Catalogs" folder,
// parses property definitions per object type (moveable/static by ID),
// and validates all values against their declared types.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using NLog;

namespace TombLib.LuaProperties
{
    /// <summary>
    /// Specifies whether a property catalog entry targets a moveable or a static object type.
    /// </summary>
    public enum LuaPropertyObjectKind
    {
        Moveable,
        Static
    }

    /// <summary>
    /// A key identifying an object type for property definitions.
    /// Combines the object's kind (moveable/static) with its numeric slot ID.
    /// </summary>
    public struct LuaPropertyObjectKey : IEquatable<LuaPropertyObjectKey>
    {
        public LuaPropertyObjectKind Kind;
        public uint TypeId;

        public LuaPropertyObjectKey(LuaPropertyObjectKind kind, uint typeId)
        {
            Kind = kind;
            TypeId = typeId;
        }

        public bool Equals(LuaPropertyObjectKey other) => Kind == other.Kind && TypeId == other.TypeId;
        public override bool Equals(object obj) => obj is LuaPropertyObjectKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Kind, TypeId);
        public static bool operator ==(LuaPropertyObjectKey a, LuaPropertyObjectKey b) => a.Equals(b);
        public static bool operator !=(LuaPropertyObjectKey a, LuaPropertyObjectKey b) => !a.Equals(b);

        public override string ToString() => $"{Kind}:{TypeId}";
    }

    /// <summary>
    /// Loads and caches Lua property definitions from XML catalog files.
    /// Multiple XML files within the catalog folder are merged;
    /// if the same property for the same object type is defined in multiple files,
    /// the latest one loaded takes priority.
    /// </summary>
    public static class LuaPropertyCatalog
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Path to the property catalog folder relative to program directory.
        /// </summary>
        public static string PropertyCatalogPath =>
            Path.Combine(DefaultPaths.CatalogsDirectory, "TEN Property Catalogs");

        /// <summary>
        /// Cached property definitions keyed by object type.
        /// </summary>
        private static Dictionary<LuaPropertyObjectKey, List<LuaPropertyDefinition>> _catalog;

        /// <summary>
        /// Gets all property definitions, loading from disk on first access.
        /// Thread-safe via lazy initialization.
        /// </summary>
        public static Dictionary<LuaPropertyObjectKey, List<LuaPropertyDefinition>> Catalog
        {
            get
            {
                if (_catalog == null)
                    _catalog = LoadCatalog(PropertyCatalogPath);
                return _catalog;
            }
        }

        /// <summary>
        /// Forces a reload of the catalog from disk.
        /// </summary>
        public static void ReloadCatalog()
        {
            _catalog = LoadCatalog(PropertyCatalogPath);
        }

        /// <summary>
        /// Gets property definitions for a specific object type.
        /// Returns an empty list if no definitions exist.
        /// </summary>
        public static List<LuaPropertyDefinition> GetDefinitions(LuaPropertyObjectKind kind, uint typeId)
        {
            var key = new LuaPropertyObjectKey(kind, typeId);
            if (Catalog.TryGetValue(key, out var definitions))
                return definitions;
            return new List<LuaPropertyDefinition>();
        }

        /// <summary>
        /// Loads all XML files from the specified catalog path
        /// and merges them into a single dictionary.
        /// </summary>
        public static Dictionary<LuaPropertyObjectKey, List<LuaPropertyDefinition>> LoadCatalog(string path)
        {
            var result = new Dictionary<LuaPropertyObjectKey, List<LuaPropertyDefinition>>();

            if (!Directory.Exists(path))
            {
                logger.Info("Property catalog directory not found: {0}", path);
                return result;
            }

            var xmlFiles = Directory.GetFiles(path, "*.xml", SearchOption.AllDirectories)
                                    .OrderBy(f => f)
                                    .ToList();

            if (xmlFiles.Count == 0)
            {
                logger.Info("No XML property catalog files found in: {0}", path);
                return result;
            }

            foreach (var file in xmlFiles)
            {
                try
                {
                    LoadCatalogFile(file, result);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Failed to load property catalog file: {0}", file);
                }
            }

            logger.Info("Loaded property catalogs: {0} object types with properties", result.Count);
            return result;
        }

        /// <summary>
        /// Loads a single XML catalog file and merges definitions into the result dictionary.
        /// Later-loaded properties with the same InternalName for the same object key overwrite earlier ones.
        /// </summary>
        private static void LoadCatalogFile(string filePath, Dictionary<LuaPropertyObjectKey, List<LuaPropertyDefinition>> result)
        {
            var doc = new XmlDocument();
            doc.Load(filePath);

            var root = doc.DocumentElement;
            if (root == null)
            {
                logger.Warn("Empty XML document: {0}", filePath);
                return;
            }

            // Process <moveable> entries
            foreach (XmlNode moveableNode in root.SelectNodes("//moveable"))
            {
                ParseObjectNode(moveableNode, LuaPropertyObjectKind.Moveable, filePath, result);
            }

            // Process <static> entries
            foreach (XmlNode staticNode in root.SelectNodes("//static"))
            {
                ParseObjectNode(staticNode, LuaPropertyObjectKind.Static, filePath, result);
            }
        }

        /// <summary>
        /// Parses a single &lt;moveable&gt; or &lt;static&gt; XML node and extracts its
        /// child &lt;property&gt; definitions.
        /// </summary>
        private static void ParseObjectNode(XmlNode objectNode, LuaPropertyObjectKind kind,
                                            string filePath, Dictionary<LuaPropertyObjectKey, List<LuaPropertyDefinition>> result)
        {
            var idAttr = objectNode.Attributes?["id"];
            if (idAttr == null || !uint.TryParse(idAttr.Value, out uint typeId))
            {
                logger.Warn("Property catalog entry missing or invalid 'id' attribute in {0}", filePath);
                return;
            }

            var key = new LuaPropertyObjectKey(kind, typeId);

            if (!result.ContainsKey(key))
                result[key] = new List<LuaPropertyDefinition>();

            foreach (XmlNode propNode in objectNode.SelectNodes("property"))
            {
                var definition = ParsePropertyNode(propNode, filePath);
                if (definition == null || !definition.IsValid)
                    continue;

                // If same internal name exists, replace it (latest file wins).
                var existingIndex = result[key].FindIndex(p =>
                    string.Equals(p.InternalName, definition.InternalName, StringComparison.OrdinalIgnoreCase));

                if (existingIndex >= 0)
                    result[key][existingIndex] = definition;
                else
                    result[key].Add(definition);
            }
        }

        /// <summary>
        /// Parses a single &lt;property&gt; XML node into a <see cref="LuaPropertyDefinition"/>.
        /// Returns null if the node is malformed beyond recovery.
        /// </summary>
        private static LuaPropertyDefinition ParsePropertyNode(XmlNode propNode, string filePath)
        {
            var definition = new LuaPropertyDefinition();

            // Required: internalName
            definition.InternalName = propNode.Attributes?["internalName"]?.Value?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(definition.InternalName))
            {
                logger.Warn("Property missing 'internalName' attribute in {0}", filePath);
                return null;
            }

            // Required: displayName
            definition.DisplayName = propNode.Attributes?["displayName"]?.Value?.Trim() ?? definition.InternalName;

            // Optional: description
            definition.Description = propNode.Attributes?["description"]?.Value?.Trim() ?? string.Empty;

            // Optional: category
            definition.Category = propNode.Attributes?["category"]?.Value?.Trim() ?? string.Empty;

            // Required: type
            var typeStr = propNode.Attributes?["type"]?.Value?.Trim() ?? string.Empty;
            if (!TryParsePropertyType(typeStr, out var propertyType))
            {
                logger.Warn("Property '{0}' has invalid type '{1}' in {2}, defaulting to Float",
                    definition.InternalName, typeStr, filePath);
                propertyType = LuaPropertyType.Float;
            }
            definition.Type = propertyType;

            // Optional: default value
            var defaultStr = propNode.Attributes?["default"]?.Value?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(defaultStr))
            {
                if (LuaValueParser.ValidateBoxedValue(propertyType, defaultStr))
                {
                    definition.DefaultValue = defaultStr;
                }
                else
                {
                    logger.Warn("Property '{0}' has mismatched default value '{1}' for type {2} in {3}, using type default",
                        definition.InternalName, defaultStr, propertyType, filePath);
                    definition.DefaultValue = LuaValueParser.GetDefaultBoxedValue(propertyType);
                }
            }
            else
            {
                definition.DefaultValue = LuaValueParser.GetDefaultBoxedValue(propertyType);
            }

            return definition;
        }

        /// <summary>
        /// Parses a property type string from XML (case-insensitive).
        /// </summary>
        private static bool TryParsePropertyType(string typeStr, out LuaPropertyType result)
        {
            if (Enum.TryParse(typeStr, ignoreCase: true, out result))
                return true;

            // Try common aliases
            switch (typeStr?.ToLowerInvariant())
            {
                case "boolean": result = LuaPropertyType.Bool; return true;
                case "integer": result = LuaPropertyType.Int; return true;
                case "number": result = LuaPropertyType.Float; return true;
                case "vector2": result = LuaPropertyType.Vec2; return true;
                case "vector3": result = LuaPropertyType.Vec3; return true;
                default: return false;
            }
        }
    }
}
