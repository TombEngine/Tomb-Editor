#nullable enable

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WadTool.Tests;

[TestClass]
public class AnimationEditorLocalizationTests
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
    public void EveryAnimationEditorKeyUsedInXamlExists()
    {
        var root = FindRepositoryRoot();
        var json = File.ReadAllText(Path.Combine(root, "TombLib", "TombLib.WPF", "Resources", "Localization", "EN", "WadTool.json"));
        using var document = JsonDocument.Parse(json);
        var section = document.RootElement.GetProperty("AnimationEditor");

        var xamlFiles = Directory.GetFiles(Path.Combine(root, "WadTool", "Features", "DockableViews", "AnimationEditor"), "*.xaml")
            .Append(Path.Combine(root, "WadTool", "Features", "Dialogs", "AnimationEditor", "AnimationEditorWindow.xaml"));

        var missing = xamlFiles
            .SelectMany(file => Regex.Matches(File.ReadAllText(file), @"Localize ~WadTool\.AnimationEditor\.(\w+)").Select(match => match.Groups[1].Value))
            .Distinct()
            .Where(key => !section.TryGetProperty(key, out _))
            .ToList();

        Assert.AreEqual(0, missing.Count, "Missing localization keys: " + string.Join(", ", missing));
    }
}
