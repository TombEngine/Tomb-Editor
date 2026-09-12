using Nickelony.IDEKit.IntelliSense.Diagnostics;
using Nickelony.IDEKit.IntelliSense.Hover;
using Nickelony.IDEKit.IntelliSense.Navigation;
using Nickelony.IDEKit.IntelliSense.Signatures;

namespace TombLib.Tests.Requests;

/// <summary>
/// Tests that the public request constructors validate offsets and expose the
/// supplied values.
/// </summary>
[TestClass]
public class TextRequestValidationTests
{
    // ---------------------------------------------------------------------------
    // TextHoverRequest
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void HoverRequest_NegativeOffset_Throws()
        => Assert.ThrowsException<ArgumentOutOfRangeException>(() => new TextHoverRequest("text", -1));

    [TestMethod]
    public void HoverRequest_OffsetBeyondTextLength_Throws()
        => Assert.ThrowsException<ArgumentOutOfRangeException>(() => new TextHoverRequest("text", 5));

    [TestMethod]
    public void HoverRequest_OffsetAtTextLength_IsValid()
    {
        var request = new TextHoverRequest("text", 4);

        Assert.AreEqual("text", request.DocumentText);
        Assert.AreEqual(4, request.HoveredOffset);
    }

    [TestMethod]
    public void HoverRequest_EqualRequests_AreEqual()
    {
        var first = new TextHoverRequest("text", 2);
        var second = new TextHoverRequest("text", 2);

        Assert.AreEqual(first, second);
        Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
    }

    [TestMethod]
    public void HoverRequest_DifferentOffset_AreNotEqual()
    {
        var first = new TextHoverRequest("text", 2);
        var second = new TextHoverRequest("text", 3);

        Assert.AreNotEqual(first, second);
    }

    // ---------------------------------------------------------------------------
    // TextSignatureHelpRequest
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void SignatureHelpRequest_NegativeOffset_Throws()
        => Assert.ThrowsException<ArgumentOutOfRangeException>(() => new TextSignatureHelpRequest("text", -1));

    [TestMethod]
    public void SignatureHelpRequest_OffsetBeyondTextLength_Throws()
        => Assert.ThrowsException<ArgumentOutOfRangeException>(() => new TextSignatureHelpRequest("text", 5));

    [TestMethod]
    public void SignatureHelpRequest_OffsetAtTextLength_IsValid()
    {
        var request = new TextSignatureHelpRequest("text", 4);

        Assert.AreEqual(4, request.CaretOffset);
    }

    // ---------------------------------------------------------------------------
    // TextDiagnosticsRequest
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void DiagnosticsRequest_ValidConstruction_ExposesValues()
    {
        var version = new Version(1, 2, 3);
        var request = new TextDiagnosticsRequest("text", version);

        Assert.AreEqual("text", request.DocumentText);
        Assert.AreSame(version, request.EngineVersion);
    }

    // ---------------------------------------------------------------------------
    // TextDefinitionRequest
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void DefinitionRequest_DefaultIdentifier_IsNull()
    {
        var request = new TextDefinitionRequest("text", "Level");

        Assert.IsNull(request.Identifier);
    }

    [TestMethod]
    public void DefinitionRequest_Identifier_PassesThrough()
    {
        TextDefinitionDiscriminator identifier = new TestDiscriminator();
        var request = new TextDefinitionRequest("text", "Level", identifier);

        Assert.AreSame(identifier, request.Identifier);
    }

    private sealed record TestDiscriminator : TextDefinitionDiscriminator;
}
