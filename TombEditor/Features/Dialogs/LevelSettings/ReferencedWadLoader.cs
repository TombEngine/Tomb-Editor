#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TombLib.LevelData;
using TombLib.Utils;
using LevelSettingsData = TombLib.LevelData.LevelSettings;

namespace TombEditor.Features.Dialogs.LevelSettings;

/// <summary>Loads wad files together with the sound catalogs that sit next to them.</summary>
public static class ReferencedWadLoader
{
    /// <summary>Wads and companion sound catalogs loaded from a list of paths.</summary>
    public sealed record LoadResult(IReadOnlyList<ReferencedWad> Wads, IReadOnlyList<ReferencedSoundCatalog> SoundCatalogs);

    /// <summary>Loads all wads concurrently, then loads the matching catalogs (.sfx next to .wad, .xml next to .wad2) that exist.</summary>
    public static LoadResult Load(LevelSettingsData settings, IReadOnlyList<string> paths, IDialogHandler dialogHandler)
    {
        var wads = new ReferencedWad[paths.Count];
        Parallel.For(0, paths.Count, i => wads[i] = new ReferencedWad(settings, paths[i], dialogHandler));

        var catalogs = new ReferencedSoundCatalog?[paths.Count];
        Parallel.For(0, paths.Count, i =>
        {
            string? catalogPath = GetCompanionCatalogPath(paths[i]);

            if (catalogPath is not null && File.Exists(catalogPath))
                catalogs[i] = new ReferencedSoundCatalog(settings, catalogPath, dialogHandler);
        });

        return new LoadResult(wads, [.. catalogs.OfType<ReferencedSoundCatalog>()]);
    }

    private static string? GetCompanionCatalogPath(string wadPath)
    {
        string? extension = Path.GetExtension(wadPath)?.ToLowerInvariant();
        string companionExtension = extension switch
        {
            ".wad" => ".sfx",
            ".wad2" => ".xml",
            _ => string.Empty
        };

        return companionExtension.Length == 0 ? null : Path.ChangeExtension(wadPath, companionExtension);
    }
}
