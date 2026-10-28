#nullable enable

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombLib.Forms.Services;

/// <summary>Preserves the previous sidecar until its replacement has been written successfully.</summary>
public sealed class MaterialEditorStorage : IMaterialEditorStorage
{
    private static readonly PropertyInfo[] MapProperties = [.. typeof(MaterialData).GetProperties()
        .Where(property => property.PropertyType == typeof(string) && property.Name.EndsWith("Map", StringComparison.Ordinal))];

    /// <inheritdoc />
    public MaterialData Load(Texture texture, string texturePath)
    {
        var material = MaterialData.TryLoadForTexture(texture, texturePath) ?? new MaterialData { ColorMap = texturePath };
        if (string.IsNullOrWhiteSpace(texturePath))
            return material;

        var directory = GetDirectory(texturePath);
        foreach (var property in MapProperties)
        {
            if (property.GetValue(material) is string path && !string.IsNullOrEmpty(path))
                property.SetValue(material, Path.GetFullPath(path, directory));
        }

        return material;
    }

    /// <inheritdoc />
    public string? Save(string texturePath, MaterialData material)
    {
        var filePath = Path.ChangeExtension(texturePath, ".xml");
        var directory = GetDirectory(filePath);
        var document = CreateSerializableCopy(material, directory);
        if (IsDefault(texturePath, document, directory))
        {
            File.Delete(filePath);
            return null;
        }

        var temporaryPath = Path.Combine(directory, $".material-{Guid.NewGuid():N}.xml");
        try
        {
            XmlUtils.WriteXmlFile(temporaryPath, document);
            File.Move(temporaryPath, filePath, true);
            return filePath;
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    private static MaterialData CreateSerializableCopy(MaterialData material, string directory)
    {
        var document = new MaterialData { Type = material.Type, Name = material.Name, Properties = [.. material.Properties] };
        foreach (var property in MapProperties)
        {
            var path = property.GetValue(material) as string;
            property.SetValue(document, string.IsNullOrEmpty(path) ? string.Empty : Path.GetRelativePath(directory, Path.GetFullPath(path, directory)));
        }

        document.Normalize();
        return document;
    }

    private static string GetDirectory(string path) => Path.GetDirectoryName(Path.GetFullPath(path))
        ?? throw new InvalidOperationException("Material sidecar directory is unavailable.");

    private static bool IsDefault(string texturePath, MaterialData material, string directory)
    {
        if (material.Type != 0 || !string.Equals(material.Name, Path.GetFileNameWithoutExtension(texturePath), StringComparison.OrdinalIgnoreCase))
            return false;

        if (string.IsNullOrEmpty(material.ColorMap) ||
            !string.Equals(Path.GetFullPath(material.ColorMap, directory), Path.GetFullPath(texturePath), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (MapProperties.Where(property => property.Name != nameof(MaterialData.ColorMap))
            .Any(property => !string.IsNullOrEmpty(property.GetValue(material) as string)))
        {
            return false;
        }

        var definition = material.GetMaterialDefinition();
        return Enumerable.Range(0, MaterialData.PropertyCount)
            .All(index => string.Equals(material.Properties[index] ?? string.Empty, MaterialCatalog.GetDefaultValue(definition.Properties[index]), StringComparison.Ordinal));
    }
}
