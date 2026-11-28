#nullable enable

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WadTool.Tests;

[TestClass]
public class LocalizationKeyTests
{
    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
            directory = directory.Parent;

        Assert.IsNotNull(directory, "Repository root not found.");
        return directory.FullName;
    }

    [TestMethod]
    public void EveryAbsoluteWadToolKeyUsedInXamlExists()
    {
        var root = FindRepositoryRoot();
        var json = File.ReadAllText(Path.Combine(root, "TombLib", "TombLib.WPF", "Resources", "Localization", "EN", "WadTool.json"));
        using var document = JsonDocument.Parse(json);

        var missing = Directory.GetFiles(Path.Combine(root, "WadTool"), "*.xaml", SearchOption.AllDirectories)
            .Where(file => !file.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
            .SelectMany(file => Regex.Matches(File.ReadAllText(file), @"Localize ~WadTool\.(\w+)\.(\w+)").Select(match => (Section: match.Groups[1].Value, Key: match.Groups[2].Value)))
            .Distinct()
            .Where(entry => !document.RootElement.TryGetProperty(entry.Section, out var section) || !section.TryGetProperty(entry.Key, out _))
            .Select(entry => entry.Section + "." + entry.Key)
            .ToList();

        Assert.AreEqual(0, missing.Count, "Missing localization keys: " + string.Join(", ", missing));
    }

    [TestMethod]
    public void NoSectionNameEndsWithATrimmedSuffix()
    {
        var root = FindRepositoryRoot();
        var json = File.ReadAllText(Path.Combine(root, "TombLib", "TombLib.WPF", "Resources", "Localization", "EN", "WadTool.json"));
        using var document = JsonDocument.Parse(json);

        var dead = document.RootElement.EnumerateObject()
            .Select(property => property.Name)
            .Where(name => name.EndsWith("Window") || name.EndsWith("View") || name.EndsWith("Page") || name.EndsWith("Dialog"))
            .ToList();

        Assert.AreEqual(0, dead.Count, "Sections that the lookup can never match: " + string.Join(", ", dead));
    }
}
