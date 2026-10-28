#nullable enable

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using TombLib.WPF;
using TombLib.WPF.Services;
using WadTool.Features.Dialogs.Options;

namespace WadTool.Tests;

[TestClass]
[DoNotParallelize]
public class OptionsLocalizationTests
{
    [TestMethod]
    public void EnglishOptionsSchemaResolvesAllVisibleLabels()
    {
        Assert.IsTrue(Localizer.Instance.LoadLanguage("EN"));
        using var tool = new WadToolClass(new Configuration());
        var localization = new LocalizationService();
        var viewModel = new OptionsWindowViewModel(tool, localizationService: localization);

        Assert.AreEqual("WadTool options", localization["Title"]);
        Assert.AreEqual("Search:", localization["Search"]);
        Assert.AreEqual("Reset page to defaults", localization["ResetPageToDefaults"]);
        string[] expectedTabs = ["General", "Gizmo"];
        string[] actualTabs = [.. viewModel.Tabs.Select(tab => tab.Header)];
        CollectionAssert.AreEqual(expectedTabs, actualTabs);

        var groups = viewModel.Tabs.SelectMany(tab => tab.Groups);
        var labels = groups.Select(group => group.Header).Concat(groups.SelectMany(group => group.Items).Select(item => item.Label));
        foreach (var label in labels)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(label));
            Assert.IsFalse(label.Contains('_'), $"An Options label uses an untranslated resource key: {label}");
        }
    }
}
