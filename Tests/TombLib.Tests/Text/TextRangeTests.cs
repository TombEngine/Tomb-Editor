using Nickelony.IDEKit.Core.Text;

namespace TombLib.Tests.Text;

/// <summary>
/// Tests for <see cref="TextRange"/>.
/// </summary>
[TestClass]
public class TextRangeTests
{
    [TestMethod]
    public void Constructor_ValidOffsets_CreatesRange()
    {
        var range = new TextRange(5, 10);

        Assert.AreEqual(5, range.Offset);
        Assert.AreEqual(10, range.Length);
        Assert.AreEqual(15, range.EndOffset);
    }

    [TestMethod]
    public void Constructor_ZeroLength_IsEmpty()
    {
        var range = new TextRange(3, 0);

        Assert.IsTrue(range.IsEmpty);
        Assert.AreEqual(3, range.EndOffset);
    }

    [TestMethod]
    public void Constructor_NegativeOffset_Throws()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new TextRange(-1, 5));
    }

    [TestMethod]
    public void Constructor_NegativeLength_Throws()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new TextRange(0, -1));
    }

    [TestMethod]
    public void GetText_ReturnsSubstring()
    {
        var range = new TextRange(2, 3);

        Assert.AreEqual("llo", range.GetText("hello"));
    }

    [TestMethod]
    public void GetText_EmptyRange_ReturnsEmpty()
    {
        var range = new TextRange(2, 0);

        Assert.AreEqual(string.Empty, range.GetText("hello"));
    }

    [TestMethod]
    public void GetText_BeyondLength_Throws()
    {
        var range = new TextRange(3, 5);

        Assert.ThrowsException<ArgumentOutOfRangeException>(() => range.GetText("hello"));
    }

    [TestMethod]
    public void Equals_SameValues_ReturnsTrue()
    {
        var a = new TextRange(1, 2);
        var b = new TextRange(1, 2);

        Assert.IsTrue(a.Equals(b));
        Assert.IsTrue(a == b);
        Assert.IsFalse(a != b);
    }

    [TestMethod]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var a = new TextRange(1, 2);
        var b = new TextRange(1, 3);

        Assert.IsFalse(a.Equals(b));
        Assert.IsFalse(a == b);
        Assert.IsTrue(a != b);
    }

    [TestMethod]
    public void Equals_Object_ReturnsCorrectResult()
    {
        var range = new TextRange(1, 2);

        Assert.IsTrue(range.Equals((object)new TextRange(1, 2)));
        Assert.IsFalse(range.Equals("not a range"));
        Assert.IsFalse(range.Equals(null));
    }

    [TestMethod]
    public void GetHashCode_SameValues_HaveSameHashCode()
    {
        var a = new TextRange(5, 10);
        var b = new TextRange(5, 10);

        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void ToString_ReturnsReadableFormat()
    {
        var range = new TextRange(3, 7);

        string result = range.ToString();

        Assert.IsTrue(result.Contains("3"));
        Assert.IsTrue(result.Contains("10"));
    }

    [TestMethod]
    public void GetText_OffsetBeyondLength_NoOverflow_Throws()
    {
        var range = new TextRange(int.MaxValue, 0);

        Assert.ThrowsException<ArgumentOutOfRangeException>(() => range.GetText("x"));
    }

    [TestMethod]
    public void GetText_OffsetPlusLengthWouldOverflow_Throws()
    {
        var range = new TextRange(int.MaxValue - 1, int.MaxValue);

        Assert.ThrowsException<ArgumentOutOfRangeException>(() => range.GetText("x"));
    }
}
