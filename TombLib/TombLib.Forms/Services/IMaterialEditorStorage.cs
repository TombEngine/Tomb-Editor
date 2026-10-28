#nullable enable

using TombLib.LevelData;
using TombLib.Utils;

namespace TombLib.Forms.Services;

/// <summary>Loads and persists material sidecars independently of their editor view.</summary>
public interface IMaterialEditorStorage
{
    /// <summary>Loads a texture's existing sidecar or inferred material maps.</summary>
    MaterialData Load(Texture texture, string texturePath);

    /// <summary>Saves the material atomically without changing the supplied document, removing redundant default sidecars.</summary>
    string? Save(string texturePath, MaterialData material);
}
