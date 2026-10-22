using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Xml;
using TombLib.LuaProperties;
using TombLib.Utils;

namespace TombLib.LevelData
{
    public enum MaterialPropertyType
    {
        None = 0,
        Bool = 1,
        Int = 2,
        Float = 3,
        Vec2 = 4,
        Vec3 = 5,
        Color = 6
    }

    public sealed class MaterialPropertyDefinition
    {
        public const int MaxPropertyCount = 4;

        public string Name { get; set; }
        public string Description { get; set; }
        public MaterialPropertyType Type { get; set; }
        public string DefaultValue { get; set; }
        public float? MinValue { get; set; }
        public float? MaxValue { get; set; }

        public bool IsDefined => Type != MaterialPropertyType.None;

        public MaterialPropertyDefinition Clone()
        {
            return new MaterialPropertyDefinition
            {
                Name = Name,
                Description = Description,
                Type = Type,
                DefaultValue = DefaultValue,
                MinValue = MinValue,
                MaxValue = MaxValue
            };
        }
    }

    public sealed class MaterialTypeDefinition
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public MaterialPropertyDefinition[] Properties { get; } = new MaterialPropertyDefinition[MaterialPropertyDefinition.MaxPropertyCount];

        public MaterialTypeDefinition()
        {
            for (int i = 0; i < Properties.Length; i++)
                Properties[i] = new MaterialPropertyDefinition();
        }

        public MaterialTypeDefinition Clone()
        {
            var result = new MaterialTypeDefinition
            {
                Id = Id,
                Name = Name
            };

            for (int i = 0; i < Properties.Length; i++)
                result.Properties[i] = Properties[i]?.Clone() ?? new MaterialPropertyDefinition();

            return result;
        }
    }

    public static class MaterialCatalog
    {
        private const int MaxPropertyCount = MaterialPropertyDefinition.MaxPropertyCount;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly Dictionary<string, int> LegacyMaterialTypeMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "Default", 0 },
            { "Reflective", 1 },
            { "SkyboxReflective", 2 }
        };

        private static Dictionary<int, MaterialTypeDefinition> _definitions;

        public static string CatalogPath => Path.Combine(DefaultPaths.CatalogsDirectory, "TEN Material Catalogs");

        public static IReadOnlyCollection<MaterialTypeDefinition> Definitions
        {
            get
            {
                EnsureLoaded();
                return _definitions.Values.OrderBy(definition => definition.Id).Select(definition => definition.Clone()).ToArray();
            }
        }

        public static void ReloadCatalog()
        {
            _definitions = LoadCatalog(CatalogPath);
        }

        public static MaterialTypeDefinition GetDefinition(int type)
        {
            EnsureLoaded();

            if (_definitions.TryGetValue(type, out var definition))
                return definition.Clone();

            return CreateFallbackDefinition(type);
        }

        public static bool TryGetDefinition(int type, out MaterialTypeDefinition definition)
        {
            EnsureLoaded();

            if (_definitions.TryGetValue(type, out var foundDefinition))
            {
                definition = foundDefinition.Clone();
                return true;
            }

            definition = null;
            return false;
        }

        public static void ParseMaterialType(MaterialData refData, string value)
        {
            EnsureLoaded();

            if (string.IsNullOrWhiteSpace(value))
            {
                refData.Type = 0;
                return;
            }

            value = value.Trim();

            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var typeId))
            {
                refData.Type = typeId;
                return;
            }

            if (LegacyMaterialTypeMap.TryGetValue(value, out typeId))
            {
                refData.Type = typeId;
                refData.ApplyDefinitionDefaults(); // Reapply default property definitions for legacy materials.
                return;
            }

            var definition = _definitions.Values.FirstOrDefault(candidate => candidate.Name.Equals(value, StringComparison.OrdinalIgnoreCase));
            refData.Type = definition?.Id ?? 0;
        }

        public static string GetMaterialTypeName(int type)
        {
            EnsureLoaded();

            if (_definitions.TryGetValue(type, out var definition) && !string.IsNullOrWhiteSpace(definition.Name))
                return definition.Name;

            return $"Material {type}";
        }

        public static bool IsReflectiveType(int type)
        {
            return type == 1 || type == 2;
        }

        public static string GetDefaultValue(MaterialPropertyDefinition property)
        {
            if (property == null || property.Type == MaterialPropertyType.None)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(property.DefaultValue))
                return property.DefaultValue;

            return BoxValue(property.Type, Vector4.Zero);
        }

        public static string BoxValue(MaterialPropertyType type, Vector4 value)
        {
            switch (type)
            {
                case MaterialPropertyType.Bool:
                    return LuaValueParser.BoxBool(value.X != 0.0f);

                case MaterialPropertyType.Int:
                    return LuaValueParser.BoxInt((int)MathF.Round(value.X));

                case MaterialPropertyType.Float:
                    return LuaValueParser.BoxFloat(value.X);

                case MaterialPropertyType.Vec2:
                    return LuaValueParser.BoxVec2(value.X, value.Y);

                case MaterialPropertyType.Vec3:
                    return LuaValueParser.BoxVec3(value.X, value.Y, value.Z);

                case MaterialPropertyType.Color:
                    return LuaValueParser.BoxColor(ToByte(value.X), ToByte(value.Y), ToByte(value.Z), ToByte(value.W));

                default:
                    return string.Empty;
            }
        }

        public static Vector4 UnboxValue(MaterialPropertyType type, string value)
        {
            switch (type)
            {
                case MaterialPropertyType.Bool:
                    return new Vector4(LuaValueParser.UnboxBool(value) ? 1.0f : 0.0f, 0.0f, 0.0f, 0.0f);

                case MaterialPropertyType.Int:
                    return new Vector4(LuaValueParser.UnboxInt(value), 0.0f, 0.0f, 0.0f);

                case MaterialPropertyType.Float:
                    return new Vector4(LuaValueParser.UnboxFloat(value), 0.0f, 0.0f, 0.0f);

                case MaterialPropertyType.Vec2:
                    var vec2 = LuaValueParser.UnboxVec2(value);
                    return new Vector4(vec2[0], vec2[1], 0.0f, 0.0f);

                case MaterialPropertyType.Vec3:
                    var vec3 = LuaValueParser.UnboxVec3(value);
                    return new Vector4(vec3[0], vec3[1], vec3[2], 0.0f);

                case MaterialPropertyType.Color:
                    var color = LuaValueParser.UnboxColor(value);
                    return new Vector4(color[0], color[1], color[2], color[3]);

                default:
                    return Vector4.Zero;
            }
        }

        private static void EnsureLoaded()
        {
            if (_definitions == null)
                _definitions = LoadCatalog(CatalogPath);
        }

        private static Dictionary<int, MaterialTypeDefinition> LoadCatalog(string path)
        {
            var result = new Dictionary<int, MaterialTypeDefinition>();

            if (!Directory.Exists(path))
            {
                logger.Info("Material catalog directory not found: {0}", path);
                return result;
            }

            var xmlFiles = Directory.GetFiles(path, "*.xml", SearchOption.AllDirectories).OrderBy(file => file).ToList();
            foreach (var filePath in xmlFiles)
            {
                try
                {
                    LoadCatalogFile(filePath, result);
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Failed to load material catalog file: {0}", filePath);
                }
            }

            return result;
        }

        private static void LoadCatalogFile(string filePath, Dictionary<int, MaterialTypeDefinition> result)
        {
            var document = new XmlDocument();
            document.Load(filePath);

            var root = document.DocumentElement;
            if (root == null)
                return;

            foreach (XmlNode materialNode in root.SelectNodes("material"))
            {
                var idText = materialNode.Attributes?["id"]?.Value;
                if (!int.TryParse(idText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var materialId))
                {
                    logger.Warn("Material catalog entry missing valid id in {0}", filePath);
                    continue;
                }

                var definition = result.ContainsKey(materialId) ? result[materialId] : new MaterialTypeDefinition { Id = materialId };
                definition.Name = materialNode.Attributes?["name"]?.Value ?? GetFallbackName(materialId);

                foreach (XmlNode propertyNode in materialNode.SelectNodes("property"))
                {
                    var slotText = propertyNode.Attributes?["slot"]?.Value;
                    if (!int.TryParse(slotText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var slot) || slot < 0 || slot >= MaxPropertyCount)
                    {
                        logger.Warn("Material catalog property has invalid slot in {0}", filePath);
                        continue;
                    }

                    definition.Properties[slot] = ParsePropertyDefinition(propertyNode);
                }

                result[materialId] = definition;
            }
        }

        private static MaterialPropertyDefinition ParsePropertyDefinition(XmlNode propertyNode)
        {
            var result = new MaterialPropertyDefinition();

            result.Name = propertyNode.Attributes?["name"]?.Value ?? string.Empty;
            result.Description = propertyNode.Attributes?["description"]?.Value ?? string.Empty;

            var typeText = propertyNode.Attributes?["type"]?.Value;
            if (!Enum.TryParse(typeText, true, out MaterialPropertyType propertyType))
                propertyType = MaterialPropertyType.None;

            result.Type = propertyType;
            result.DefaultValue = NormalizeDefaultValue(propertyType, propertyNode.Attributes?["defaultValue"]?.Value);
            result.MinValue = ParseOptionalSingle(propertyNode.Attributes?["minValue"]?.Value);
            result.MaxValue = ParseOptionalSingle(propertyNode.Attributes?["maxValue"]?.Value);

            return result;
        }

        private static string NormalizeDefaultValue(MaterialPropertyType type, string value)
        {
            if (type == MaterialPropertyType.None)
                return string.Empty;

            if (string.IsNullOrWhiteSpace(value))
                return BoxValue(type, Vector4.Zero);

            try
            {
                var vector = UnboxValue(type, value);
                return BoxValue(type, vector);
            }
            catch (Exception)
            {
                return BoxValue(type, Vector4.Zero);
            }
        }

        private static float? ParseOptionalSingle(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                return result;

            return null;
        }

        private static MaterialTypeDefinition CreateFallbackDefinition(int type)
        {
            return new MaterialTypeDefinition
            {
                Id = type,
                Name = GetFallbackName(type)
            };
        }

        private static string GetFallbackName(int type)
        {
            if (LegacyMaterialTypeMap.ContainsValue(type))
                return LegacyMaterialTypeMap.First(entry => entry.Value == type).Key.SplitCamelcase();

            return $"Material {type}";
        }

        private static byte ToByte(float value)
        {
            var roundedValue = MathF.Round(value);
            roundedValue = Math.Clamp(roundedValue, 0.0f, 255.0f);
            return (byte)roundedValue;
        }
    }
}