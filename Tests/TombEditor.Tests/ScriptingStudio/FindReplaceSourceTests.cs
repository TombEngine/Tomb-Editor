using TombIDE.ScriptingStudio.FindAndReplace;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class FindReplaceSourceTests
{
    [TestMethod]
    public void DefaultsToEmptyName()
    {
        var source = new FindReplaceSource();

        Assert.AreEqual(string.Empty, source.Name);
        Assert.AreEqual(0, source.Count);
    }

    [TestMethod]
    public void CarriesNameAndItems()
    {
        var source = new FindReplaceSource("script.lua")
        {
            new FindReplaceItem(3, "local value = 1", "value", 0)
        };

        Assert.AreEqual("script.lua", source.Name);
        Assert.AreEqual(1, source.Count);
        Assert.AreEqual(3, source[0].LineNumber);
        Assert.AreEqual("local value = 1", source[0].LineText);
        Assert.AreEqual("value", source[0].MatchSegmentText);
        Assert.AreEqual(0, source[0].MatchSegmentIndex);
    }

    [TestMethod]
    public void AddRangeAndClear_UpdateItems()
    {
        var source = new FindReplaceSource();

        source.AddRange([new FindReplaceItem(1, "a", "a", 0), new FindReplaceItem(2, "b", "b", 0)]);

        Assert.AreEqual(2, source.Count);

        source.Clear();

        Assert.AreEqual(0, source.Count);
    }

    [TestMethod]
    public void Indexer_OutOfRange_Throws()
    {
        var source = new FindReplaceSource();

        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _ = source[0]);

        source.Add(new FindReplaceItem(1, "a", "a", 0));

        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _ = source[1]);
    }

    [TestMethod]
    public void FindReplaceItem_InvalidLineNumberOrSegmentIndex_Throws()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new FindReplaceItem(0, "a", "a", 0));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new FindReplaceItem(1, "a", "a", -1));
    }
}
