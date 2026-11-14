#nullable enable

using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TombEditor.Tests;

[TestClass]
public class LayoutConfigurationTests
{
    [TestMethod]
    public void LegacyDockingStateRoundTripsWithoutLoadingLegacyTypes()
    {
        const string xml = "<NamedLayout><Name>Saved layout</Name><State Version=\"2\"><Groups><Group Dock=\"Left\"><Content Key=\"Rooms\" /></Group></Groups></State><AvalonDockState>WPF layout</AvalonDockState></NamedLayout>";
        var serializer = new XmlSerializer(typeof(NamedLayout));
        using var input = new StringReader(xml);
        var layout = serializer.Deserialize(input) as NamedLayout ?? throw new AssertFailedException("The layout was not deserialized.");
        var state = layout.LegacyState ?? throw new AssertFailedException("The legacy docking state was lost.");
        var originalState = state.OuterXml;

        using var output = new StringWriter();
        serializer.Serialize(output, layout);
        using var roundTripInput = new StringReader(output.ToString());
        var roundTrip = serializer.Deserialize(roundTripInput) as NamedLayout ?? throw new AssertFailedException("The layout was not deserialized.");

        Assert.AreEqual(originalState, roundTrip.LegacyState?.OuterXml);
        Assert.AreEqual("Saved layout", roundTrip.Name);
        Assert.AreEqual("WPF layout", roundTrip.AvalonDockState);
    }

    [TestMethod]
    public void CloningLayoutCopiesLegacyStateIndependently()
    {
        var document = new XmlDocument();
        document.LoadXml("<State><Group Dock=\"Left\"><Content Key=\"Rooms\" /></Group></State>");
        var layout = new NamedLayout { LegacyState = document.DocumentElement, AvalonDockState = "WPF layout" };

        var clone = layout.Clone();
        var originalState = layout.LegacyState ?? throw new AssertFailedException("The legacy docking state was lost.");
        var clonedState = clone.LegacyState ?? throw new AssertFailedException("The legacy docking state was not cloned.");
        Assert.AreNotSame(originalState, clonedState);
        Assert.AreEqual(originalState.OuterXml, clonedState.OuterXml);
        clonedState.SetAttribute("Modified", "true");
        Assert.IsFalse(originalState.HasAttribute("Modified"));
        Assert.AreEqual("WPF layout", clone.AvalonDockState);
    }
}
