using TombLib.LevelData;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorEnums.Extensions;

namespace TombLib.Test;

[TestClass]
public class SectorVerticalPartExtensionsTests
{
	[TestMethod]
	public void IsOnFloor()
	{
		Assert.IsTrue(SectorVerticalPart.QA.IsOnFloor());
		Assert.IsFalse(SectorVerticalPart.WS.IsOnFloor());

		foreach (SectorVerticalPart vertical in Enum.GetValues<SectorVerticalPart>().Where(f => f.ToString().Contains("FloorSplit")))
			Assert.IsTrue(vertical.IsOnFloor());

		foreach (SectorVerticalPart vertical in Enum.GetValues<SectorVerticalPart>().Where(f => f.ToString().Contains("CeilingSplit")))
			Assert.IsFalse(vertical.IsOnFloor());
	}

	[TestMethod]
	public void IsOnCeiling()
	{
		Assert.IsTrue(SectorVerticalPart.WS.IsOnCeiling());
		Assert.IsFalse(SectorVerticalPart.QA.IsOnCeiling());

		foreach (SectorVerticalPart vertical in Enum.GetValues<SectorVerticalPart>().Where(f => f.ToString().Contains("CeilingSplit")))
			Assert.IsTrue(vertical.IsOnCeiling());

		foreach (SectorVerticalPart vertical in Enum.GetValues<SectorVerticalPart>().Where(f => f.ToString().Contains("FloorSplit")))
			Assert.IsFalse(vertical.IsOnCeiling());
	}

	[TestMethod]
	public void IsExtraFloorSplit()
	{
		Assert.IsFalse(SectorVerticalPart.QA.IsExtraFloorSplit());
		Assert.IsFalse(SectorVerticalPart.WS.IsExtraFloorSplit());

		foreach (SectorVerticalPart vertical in Enum.GetValues<SectorVerticalPart>().Where(f => f.ToString().Contains("FloorSplit")))
			Assert.IsTrue(vertical.IsExtraFloorSplit());

		foreach (SectorVerticalPart vertical in Enum.GetValues<SectorVerticalPart>().Where(f => f.ToString().Contains("CeilingSplit")))
			Assert.IsFalse(vertical.IsExtraFloorSplit());
	}

	[TestMethod]
	public void IsExtraCeilingSplit()
	{
		Assert.IsFalse(SectorVerticalPart.QA.IsExtraCeilingSplit());
		Assert.IsFalse(SectorVerticalPart.WS.IsExtraCeilingSplit());

		foreach (SectorVerticalPart vertical in Enum.GetValues<SectorVerticalPart>().Where(f => f.ToString().Contains("CeilingSplit")))
			Assert.IsTrue(vertical.IsExtraCeilingSplit());

		foreach (SectorVerticalPart vertical in Enum.GetValues<SectorVerticalPart>().Where(f => f.ToString().Contains("FloorSplit")))
			Assert.IsFalse(vertical.IsExtraCeilingSplit());
	}

	[TestMethod]
	public void IsExtraSplit()
	{
		Assert.IsFalse(SectorVerticalPart.QA.IsExtraSplit());
		Assert.IsFalse(SectorVerticalPart.WS.IsExtraSplit());

		foreach (SectorVerticalPart vertical in Enum.GetValues<SectorVerticalPart>().Where(f => f.ToString().Contains("Split")))
			Assert.IsTrue(vertical.IsExtraSplit());
	}

	[TestMethod]
	public void GetExtraFloorSplit()
	{
		Assert.AreEqual(SectorVerticalPart.Floor2, SectorVerticalPartExtensions.GetExtraFloorSplit(0));
		Assert.AreEqual(SectorVerticalPart.Floor3, SectorVerticalPartExtensions.GetExtraFloorSplit(1));
		Assert.AreEqual(SectorVerticalPart.Floor4, SectorVerticalPartExtensions.GetExtraFloorSplit(2));
		Assert.AreEqual(SectorVerticalPart.Floor5, SectorVerticalPartExtensions.GetExtraFloorSplit(3));
		Assert.AreEqual(SectorVerticalPart.Floor6, SectorVerticalPartExtensions.GetExtraFloorSplit(4));
		Assert.AreEqual(SectorVerticalPart.Floor7, SectorVerticalPartExtensions.GetExtraFloorSplit(5));
		Assert.AreEqual(SectorVerticalPart.Floor8, SectorVerticalPartExtensions.GetExtraFloorSplit(6));
		Assert.AreEqual(SectorVerticalPart.Floor9, SectorVerticalPartExtensions.GetExtraFloorSplit(7));
	}

	[TestMethod]
	public void GetExtraCeilingSplit()
	{
		Assert.AreEqual(SectorVerticalPart.Ceiling2, SectorVerticalPartExtensions.GetExtraCeilingSplit(0));
		Assert.AreEqual(SectorVerticalPart.Ceiling3, SectorVerticalPartExtensions.GetExtraCeilingSplit(1));
		Assert.AreEqual(SectorVerticalPart.Ceiling4, SectorVerticalPartExtensions.GetExtraCeilingSplit(2));
		Assert.AreEqual(SectorVerticalPart.Ceiling5, SectorVerticalPartExtensions.GetExtraCeilingSplit(3));
		Assert.AreEqual(SectorVerticalPart.Ceiling6, SectorVerticalPartExtensions.GetExtraCeilingSplit(4));
		Assert.AreEqual(SectorVerticalPart.Ceiling7, SectorVerticalPartExtensions.GetExtraCeilingSplit(5));
		Assert.AreEqual(SectorVerticalPart.Ceiling8, SectorVerticalPartExtensions.GetExtraCeilingSplit(6));
		Assert.AreEqual(SectorVerticalPart.Ceiling9, SectorVerticalPartExtensions.GetExtraCeilingSplit(7));
	}

	[TestMethod]
	public void GetExtraSplitIndex()
	{
		Assert.AreEqual(0, SectorVerticalPart.Floor2.GetExtraSplitIndex());
		Assert.AreEqual(1, SectorVerticalPart.Floor3.GetExtraSplitIndex());
		Assert.AreEqual(2, SectorVerticalPart.Floor4.GetExtraSplitIndex());
		Assert.AreEqual(3, SectorVerticalPart.Floor5.GetExtraSplitIndex());
		Assert.AreEqual(4, SectorVerticalPart.Floor6.GetExtraSplitIndex());
		Assert.AreEqual(5, SectorVerticalPart.Floor7.GetExtraSplitIndex());
		Assert.AreEqual(6, SectorVerticalPart.Floor8.GetExtraSplitIndex());
		Assert.AreEqual(7, SectorVerticalPart.Floor9.GetExtraSplitIndex());

		Assert.AreEqual(0, SectorVerticalPart.Ceiling2.GetExtraSplitIndex());
		Assert.AreEqual(1, SectorVerticalPart.Ceiling3.GetExtraSplitIndex());
		Assert.AreEqual(2, SectorVerticalPart.Ceiling4.GetExtraSplitIndex());
		Assert.AreEqual(3, SectorVerticalPart.Ceiling5.GetExtraSplitIndex());
		Assert.AreEqual(4, SectorVerticalPart.Ceiling6.GetExtraSplitIndex());
		Assert.AreEqual(5, SectorVerticalPart.Ceiling7.GetExtraSplitIndex());
		Assert.AreEqual(6, SectorVerticalPart.Ceiling8.GetExtraSplitIndex());
		Assert.AreEqual(7, SectorVerticalPart.Ceiling9.GetExtraSplitIndex());
	}
}
