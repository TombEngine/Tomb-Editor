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
    public void DiagnosticsRequest_ValidConstruction_ExposesDocumentText()
    {
        var request = new TextDiagnosticsRequest("text");

        Assert.AreEqual("text", request.DocumentText);
    }

    // ---------------------------------------------------------------------------
    // TextDefinitionRequest
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void DefinitionRequest_DefaultDiscriminator_IsNull()
    {
        var request = new TextDefinitionRequest("text", "Level");

        Assert.IsNull(request.Discriminator);
    }

    [TestMethod]
    public void DefinitionRequest_Discriminator_PassesThrough()
    {
        TextDefinitionDiscriminator discriminator = new TestDiscriminator();
        var request = new TextDefinitionRequest("text", "Level", discriminator);

        Assert.AreSame(discriminator, request.Discriminator);
    }

    private sealed record TestDiscriminator : TextDefinitionDiscriminator;
}
