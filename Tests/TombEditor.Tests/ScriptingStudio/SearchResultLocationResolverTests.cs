using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.Core.Navigation;
using Nickelony.IDEKit.Core.Text;
using TombIDE.ScriptingStudio.FindAndReplace;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class SearchResultLocationResolverTests
{
    [TestMethod]
    public void Resolve_SelectsRequestedMatchOccurrence()
    {
        var document = new TextDocument("beta beta");

        (SearchResultLocationStatus status, NavigationLocation? location) = SearchResultLocationResolver.Resolve(
            document,
            "path.lua",
            new FindReplaceItem(1, "beta beta", "beta", 1));

        Assert.AreEqual(SearchResultLocationStatus.MatchLocated, status);
        Assert.IsNotNull(location);

        Assert.AreEqual("path.lua", location.Value.FilePath);
        Assert.AreEqual(5, location.Value.CaretOffset);
        Assert.AreEqual(5, location.Value.SelectionStart);
        Assert.AreEqual(4, location.Value.SelectionLength);
        Assert.AreEqual(1, location.Value.PreferredDocumentLine);
    }

    [TestMethod]
    public void Resolve_OrdinalIgnoreCase_SelectsRequestedMatchOccurrence()
    {
        var document = new TextDocument("BETA beta");

        (SearchResultLocationStatus status, NavigationLocation? location) = SearchResultLocationResolver.Resolve(
            document,
            "path.lua",
            new FindReplaceItem(1, "BETA beta", "beta", 1),
            StringComparison.OrdinalIgnoreCase);

        Assert.AreEqual(SearchResultLocationStatus.MatchLocated, status);
        Assert.IsNotNull(location);

        Assert.AreEqual(5, location.Value.CaretOffset);
        Assert.AreEqual(5, location.Value.SelectionStart);
        Assert.AreEqual(4, location.Value.SelectionLength);
        Assert.AreEqual(1, location.Value.PreferredDocumentLine);
    }

    [TestMethod]
    public void Resolve_LineOutsideDocument_ReturnsLineNotFound()
    {
        var document = new TextDocument("beta beta");

        (SearchResultLocationStatus status, NavigationLocation? location) = SearchResultLocationResolver.Resolve(
            document,
            "path.lua",
            new FindReplaceItem(2, "beta beta", "beta", 0));

        Assert.AreEqual(SearchResultLocationStatus.LineNotFound, status);
        Assert.IsNull(location);
    }

    [TestMethod]
    public void Resolve_OutOfRangeMatchIndex_ReturnsLineStartFallback()
    {
        var document = new TextDocument("alpha beta\r\ngamma beta");

        (SearchResultLocationStatus status, NavigationLocation? location) = SearchResultLocationResolver.Resolve(
            document,
            "path.lua",
            new FindReplaceItem(2, "gamma beta", "beta", 5));

        Assert.AreEqual(SearchResultLocationStatus.LineStartFallback, status);
        Assert.IsNotNull(location);

        Assert.AreEqual(12, location.Value.CaretOffset);
        Assert.AreEqual(12, location.Value.SelectionStart);
        Assert.AreEqual(0, location.Value.SelectionLength);
        Assert.AreEqual(2, location.Value.PreferredDocumentLine);
    }

    [TestMethod]
    public void Resolve_NoMatchOnLine_ReturnsLineStartFallback()
    {
        var document = new TextDocument("alpha beta\r\ngamma beta");

        (SearchResultLocationStatus status, NavigationLocation? location) = SearchResultLocationResolver.Resolve(
            document,
            "path.lua",
            new FindReplaceItem(2, "gamma beta", "zzz", 0));

        Assert.AreEqual(SearchResultLocationStatus.LineStartFallback, status);
        Assert.IsNotNull(location);

        Assert.AreEqual(12, location.Value.CaretOffset);
        Assert.AreEqual(0, location.Value.SelectionLength);
    }

    [TestMethod]
    public void Resolve_MetacharacterMatchText_IsMatchedLiterally()
    {
        var document = new TextDocument("foo(a) bar foo(a)");

        (SearchResultLocationStatus status, NavigationLocation? location) = SearchResultLocationResolver.Resolve(
            document,
            "path.lua",
            new FindReplaceItem(1, "foo(a) bar foo(a)", "foo(a)", 1));

        Assert.AreEqual(SearchResultLocationStatus.MatchLocated, status);
        Assert.IsNotNull(location);

        Assert.AreEqual(11, location.Value.CaretOffset);
        Assert.AreEqual(11, location.Value.SelectionStart);
        Assert.AreEqual(6, location.Value.SelectionLength);
        Assert.AreEqual(1, location.Value.PreferredDocumentLine);
    }

    [TestMethod]
    public void Resolve_ParenthesisMatchText_DoesNotThrow()
    {
        var document = new TextDocument("foo(a) bar");

        // A lone '(' is invalid regex, but the match text is treated as a literal.
        (SearchResultLocationStatus status, NavigationLocation? location) = SearchResultLocationResolver.Resolve(
            document,
            "path.lua",
            new FindReplaceItem(1, "foo(a) bar", "(", 0));

        Assert.AreEqual(SearchResultLocationStatus.MatchLocated, status);
        Assert.IsNotNull(location);

        Assert.AreEqual(3, location.Value.CaretOffset);
        Assert.AreEqual(3, location.Value.SelectionStart);
        Assert.AreEqual(1, location.Value.SelectionLength);
    }

    [TestMethod]
    public void Resolve_EmptyMatchText_ReturnsLineStartFallback()
    {
        var document = new TextDocument("alpha beta");

        (SearchResultLocationStatus status, NavigationLocation? location) = SearchResultLocationResolver.Resolve(
            document,
            "path.lua",
            new FindReplaceItem(1, "alpha beta", string.Empty, 0));

        Assert.AreEqual(SearchResultLocationStatus.LineStartFallback, status);
        Assert.IsNotNull(location);

        Assert.AreEqual(0, location.Value.CaretOffset);
        Assert.AreEqual(0, location.Value.SelectionLength);
        Assert.AreEqual(1, location.Value.PreferredDocumentLine);
    }

    [TestMethod]
    public void Resolve_WithMatchRange_SelectsExactRange()
    {
        var document = new TextDocument("alpha beta\r\ngamma delta");

        // The stored segment text does not match the line, but the exact range does; the range wins.
        (SearchResultLocationStatus status, NavigationLocation? location) = SearchResultLocationResolver.Resolve(
            document,
            "path.lua",
            new FindReplaceItem(2, "gamma delta", "stale", 0)
            {
                MatchRangeInLine = new TextRange(6, 5)
            });

        Assert.AreEqual(SearchResultLocationStatus.MatchLocated, status);
        Assert.IsNotNull(location);
        Assert.AreEqual(12 + 6, location.Value.CaretOffset);
        Assert.AreEqual(5, location.Value.SelectionLength);
        Assert.AreEqual(2, location.Value.PreferredDocumentLine);
    }

    [TestMethod]
    public void Resolve_MatchRangeDoesNotFitLine_FallsBackToLiteralMatch()
    {
        var document = new TextDocument("beta beta");

        (SearchResultLocationStatus status, NavigationLocation? location) = SearchResultLocationResolver.Resolve(
            document,
            "path.lua",
            new FindReplaceItem(1, "beta beta", "beta", 1)
            {
                MatchRangeInLine = new TextRange(50, 4)
            });

        Assert.AreEqual(SearchResultLocationStatus.MatchLocated, status);
        Assert.IsNotNull(location);
        Assert.AreEqual(5, location.Value.CaretOffset);
        Assert.AreEqual(4, location.Value.SelectionLength);
    }
}
