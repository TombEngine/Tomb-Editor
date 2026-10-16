using TombLib.Scripting.UI.Diagnostics;

namespace TombLib.Tests;

[TestClass]
public class TextEditorRequestIdentityTests
{
	[TestMethod]
	public void DefaultValuesAreNullOrZero()
	{
		var identity = default(TextEditorRequestIdentity);

		Assert.IsNull(identity.LogicalDocumentId);
		Assert.AreEqual(0, identity.DocumentVersion);
		Assert.AreEqual(0, identity.SessionGeneration);
	}

	[TestMethod]
	public void IsValueEqual()
	{
		var first = new TextEditorRequestIdentity("doc", 3, 7);
		var second = new TextEditorRequestIdentity("doc", 3, 7);

		Assert.AreEqual(first, second);
		Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
	}

	[TestMethod]
	public void AcceptsLongVersionValues()
	{
		var identity = new TextEditorRequestIdentity("doc", long.MaxValue, long.MaxValue);

		Assert.AreEqual(long.MaxValue, identity.DocumentVersion);
		Assert.AreEqual(long.MaxValue, identity.SessionGeneration);
	}
}
