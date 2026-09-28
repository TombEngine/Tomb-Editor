using System.Security.Cryptography;
using System.Text;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Descriptions;

namespace TombLib.Tests;

[TestClass]
public class ClassicScriptDescriptionCatalogServiceTests
{
	private readonly ClassicScriptDescriptionCatalogService _service = new();

	[TestMethod]
	public void GetNewCommandDescription_KnownCommand_ReturnsFullDescription()
	{
		string description = _service.GetNewCommandDescription("AddEffect");

		Assert.IsFalse(string.IsNullOrEmpty(description));
		StringAssert.Contains(description, "Syntax: AddEffect=");
		Assert.AreEqual("88B2C1BB64FDC612BD69D8C9ADEF4530174018DEEDB46A03E89540DA6EDDD0C5", Sha256(description));
	}

	[TestMethod]
	public void GetOldCommandDescription_KnownCommand_ReturnsDescription()
	{
		string description = _service.GetOldCommandDescription("Legend");

		Assert.IsFalse(string.IsNullOrEmpty(description));
		StringAssert.Contains(description, "Syntax: Legend=");
	}

	[TestMethod]
	public void GetMnemonicConstantDescription_KnownConstant_ReturnsDescription()
	{
		string description = _service.GetMnemonicConstantDescription("ADD_BLOOD");

		Assert.IsFalse(string.IsNullOrEmpty(description));
		StringAssert.Contains(description, "Add blood");
	}

	[TestMethod]
	public void GetOcbDescription_KnownOcb_ReturnsDescription()
	{
		string description = _service.GetOcbDescription("NEW_Kayak");

		Assert.IsFalse(string.IsNullOrEmpty(description));
		StringAssert.Contains(description, "OCB 1 = Add a mist wake");
	}

	[TestMethod]
	public void GetDescription_IsCaseInsensitive()
	{
		string lower = _service.GetNewCommandDescription("addeffect");
		string upper = _service.GetNewCommandDescription("ADDEFFECT");

		Assert.AreEqual(upper, lower);
		Assert.IsFalse(string.IsNullOrEmpty(lower));
	}

	[TestMethod]
	public void GetDescription_AppliesArchiveKeyNormalization()
	{
		// Space to underscore, as the archive reader did when building info_<keyword>.txt.
		string spaced = _service.GetMnemonicConstantDescription("ADD BLOOD");
		string underscored = _service.GetMnemonicConstantDescription("ADD_BLOOD");

		// Leading underscores are trimmed.
		string underscoredLookup = _service.GetMnemonicConstantDescription("_ADD_BLOOD");

		Assert.AreEqual(underscored, spaced);
		Assert.IsFalse(string.IsNullOrEmpty(spaced));
		Assert.AreEqual(underscored, underscoredLookup);
	}

	[TestMethod]
	public void GetDescription_MissingKey_ReturnsEmptyString()
	{
		Assert.AreEqual(string.Empty, _service.GetNewCommandDescription("definitely_not_a_real_command"));
		Assert.AreEqual(string.Empty, _service.GetMnemonicConstantDescription("NOT_A_REAL_CONSTANT"));
	}

	[TestMethod]
	public void GetMnemonicConstantDescription_HeaderOnlySection_ReturnsEmptyString()
	{
		// These entries are intentionally empty in the archive (consecutive headers).
		Assert.AreEqual(string.Empty, _service.GetMnemonicConstantDescription("CL_BLINKING_WHITE"));
		Assert.AreEqual(string.Empty, _service.GetMnemonicConstantDescription("DTF_NONE"));
	}

	[TestMethod]
	public void GetMnemonicConstantDescription_NonAsciiEntry_PreservesReplacementCharacters()
	{
		// Source bytes are Windows-1252; decoded as UTF-8 they render as U+FFFD today.
		// Phase 1 preserves this exactly; Phase 4 restores the real characters.
		string description = _service.GetMnemonicConstantDescription("EDGX_RECORDING_DEMO");

		Assert.IsFalse(string.IsNullOrEmpty(description));
		Assert.AreEqual(2, CountOccurrences(description, "\uFFFD"));
	}

	[TestMethod]
	public void CatalogFiles_ContainExpectedSectionCountsWithoutDuplicates()
	{
		AssertSectionCount("MnemonicConstants.md", 1029);
		AssertSectionCount("NewCommands.md", 58);
		AssertSectionCount("OCBs.md", 70);
		AssertSectionCount("OldCommands.md", 39);
	}

	private static void AssertSectionCount(string fileName, int expectedCount)
	{
		string path = ClassicScriptResourcePaths.GetResourcePath("Descriptions", fileName);
		string[] lines = File.ReadAllText(path).Replace("\r\n", "\n").Split('\n');

		string[] headers = lines.Where(line => line.StartsWith("## ", StringComparison.Ordinal)).ToArray();

		Assert.AreEqual(expectedCount, headers.Length, $"{fileName} section count.");
		Assert.AreEqual(headers.Length, headers.Distinct(StringComparer.OrdinalIgnoreCase).Count(), $"{fileName} duplicate headers.");
	}

	private static int CountOccurrences(string text, string value)
	{
		int count = 0;
		int index = 0;

		while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
		{
			count++;
			index += value.Length;
		}

		return count;
	}

	private static string Sha256(string text)
		=> Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
}
