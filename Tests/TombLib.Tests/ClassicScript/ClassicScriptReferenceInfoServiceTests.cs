using TombLib.Scripting.ClassicScript.Navigation;

namespace TombLib.Tests;

[TestClass]
public class ClassicScriptReferenceInfoServiceTests
{
	private readonly ClassicScriptReferenceInfoService _service = new();

	[TestMethod]
	public void GetReferenceInfo_KnownOldCommand_ReturnsDescriptionWithoutMissingMessage()
	{
		ClassicScriptReferenceInfo info = _service.GetReferenceInfo("Legend", ReferenceType.OldCommand);

		Assert.IsFalse(string.IsNullOrWhiteSpace(info.Description));
		Assert.IsNull(info.MissingDescriptionMessage);
	}

	[TestMethod]
	public void GetReferenceInfo_UnknownMnemonicConstant_ReturnsFlagMissingMessage()
	{
		ClassicScriptReferenceInfo info = _service.GetReferenceInfo("unknown_flag", ReferenceType.MnemonicConstant);

		Assert.AreEqual(string.Empty, info.Description);
		Assert.AreEqual("No description found for the UNKNOWN_FLAG flag.", info.MissingDescriptionMessage);
	}

	[TestMethod]
	public void GetReferenceInfo_UnknownHexValue_ReturnsContextualMissingMessage()
	{
		ClassicScriptReferenceInfo info = _service.GetReferenceInfo("$123", ReferenceType.MnemonicConstant);

		Assert.AreEqual(string.Empty, info.Description);
		Assert.AreEqual("Couldn't identify the hexadecimal value for the given context.", info.MissingDescriptionMessage);
	}
}