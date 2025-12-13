using TombLib.LevelData;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorEnums.Extensions;

namespace TombLib.Test;

[TestClass]
public class SectorFaceExtensionsTests
{
	[TestMethod]
	public void GetVertical()
	{
		// Floor

		Assert.AreEqual(SectorVerticalPart.QA, SectorFace.Wall_PositiveZ_QA.GetVertical());
		Assert.AreEqual(SectorVerticalPart.QA, SectorFace.Wall_NegativeZ_QA.GetVertical());
		Assert.AreEqual(SectorVerticalPart.QA, SectorFace.Wall_NegativeX_QA.GetVertical());
		Assert.AreEqual(SectorVerticalPart.QA, SectorFace.Wall_PositiveX_QA.GetVertical());
		Assert.AreEqual(SectorVerticalPart.QA, SectorFace.Wall_Diagonal_QA.GetVertical());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("FloorSplit2")))
			Assert.AreEqual(SectorVerticalPart.Floor2, face.GetVertical());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("FloorSplit3")))
			Assert.AreEqual(SectorVerticalPart.Floor3, face.GetVertical());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("FloorSplit4")))
			Assert.AreEqual(SectorVerticalPart.Floor4, face.GetVertical());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("FloorSplit5")))
			Assert.AreEqual(SectorVerticalPart.Floor5, face.GetVertical());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("FloorSplit6")))
			Assert.AreEqual(SectorVerticalPart.Floor6, face.GetVertical());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("FloorSplit7")))
			Assert.AreEqual(SectorVerticalPart.Floor7, face.GetVertical());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("FloorSplit8")))
			Assert.AreEqual(SectorVerticalPart.Floor8, face.GetVertical());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("FloorSplit9")))
			Assert.AreEqual(SectorVerticalPart.Floor9, face.GetVertical());

		// Ceiling

		Assert.AreEqual(SectorVerticalPart.WS, SectorFace.Wall_PositiveZ_WS.GetVertical());
		Assert.AreEqual(SectorVerticalPart.WS, SectorFace.Wall_NegativeZ_WS.GetVertical());
		Assert.AreEqual(SectorVerticalPart.WS, SectorFace.Wall_NegativeX_WS.GetVertical());
		Assert.AreEqual(SectorVerticalPart.WS, SectorFace.Wall_PositiveX_WS.GetVertical());
		Assert.AreEqual(SectorVerticalPart.WS, SectorFace.Wall_Diagonal_WS.GetVertical());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("CeilingSplit2")))
			Assert.AreEqual(SectorVerticalPart.Ceiling2, face.GetVertical());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("CeilingSplit3")))
			Assert.AreEqual(SectorVerticalPart.Ceiling3, face.GetVertical());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("CeilingSplit4")))
			Assert.AreEqual(SectorVerticalPart.Ceiling4, face.GetVertical());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("CeilingSplit5")))
			Assert.AreEqual(SectorVerticalPart.Ceiling5, face.GetVertical());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("CeilingSplit6")))
			Assert.AreEqual(SectorVerticalPart.Ceiling6, face.GetVertical());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("CeilingSplit7")))
			Assert.AreEqual(SectorVerticalPart.Ceiling7, face.GetVertical());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("CeilingSplit8")))
			Assert.AreEqual(SectorVerticalPart.Ceiling8, face.GetVertical());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("CeilingSplit9")))
			Assert.AreEqual(SectorVerticalPart.Ceiling9, face.GetVertical());

		// null results

		Assert.IsNull(SectorFace.Floor.GetVertical());
		Assert.IsNull(SectorFace.Floor_Triangle2.GetVertical());

		Assert.IsNull(SectorFace.Ceiling.GetVertical());
		Assert.IsNull(SectorFace.Ceiling_Triangle2.GetVertical());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("Middle")))
			Assert.IsNull(face.GetVertical());

		Assert.IsNull(SectorFace.Count.GetVertical());
	}

	[TestMethod]
	public void GetFaceType()
	{
		// Floor

		Assert.AreEqual(SectorFaceType.Floor, SectorFace.Wall_PositiveZ_QA.GetFaceType());
		Assert.AreEqual(SectorFaceType.Floor, SectorFace.Wall_NegativeZ_QA.GetFaceType());
		Assert.AreEqual(SectorFaceType.Floor, SectorFace.Wall_NegativeX_QA.GetFaceType());
		Assert.AreEqual(SectorFaceType.Floor, SectorFace.Wall_PositiveX_QA.GetFaceType());
		Assert.AreEqual(SectorFaceType.Floor, SectorFace.Wall_Diagonal_QA.GetFaceType());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("FloorSplit")))
			Assert.AreEqual(SectorFaceType.Floor, face.GetFaceType());

		// Ceiling

		Assert.AreEqual(SectorFaceType.Ceiling, SectorFace.Wall_PositiveZ_WS.GetFaceType());
		Assert.AreEqual(SectorFaceType.Ceiling, SectorFace.Wall_NegativeZ_WS.GetFaceType());
		Assert.AreEqual(SectorFaceType.Ceiling, SectorFace.Wall_NegativeX_WS.GetFaceType());
		Assert.AreEqual(SectorFaceType.Ceiling, SectorFace.Wall_PositiveX_WS.GetFaceType());
		Assert.AreEqual(SectorFaceType.Ceiling, SectorFace.Wall_Diagonal_WS.GetFaceType());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("CeilingSplit")))
			Assert.AreEqual(SectorFaceType.Ceiling, face.GetFaceType());

		// Wall

		Assert.AreEqual(SectorFaceType.Wall, SectorFace.Wall_PositiveZ_Middle.GetFaceType());
		Assert.AreEqual(SectorFaceType.Wall, SectorFace.Wall_NegativeZ_Middle.GetFaceType());
		Assert.AreEqual(SectorFaceType.Wall, SectorFace.Wall_NegativeX_Middle.GetFaceType());
		Assert.AreEqual(SectorFaceType.Wall, SectorFace.Wall_PositiveX_Middle.GetFaceType());
		Assert.AreEqual(SectorFaceType.Wall, SectorFace.Wall_Diagonal_Middle.GetFaceType());

		// Exceptions

		Assert.ThrowsException<ArgumentException>(() => SectorFace.Floor.GetFaceType());
		Assert.ThrowsException<ArgumentException>(() => SectorFace.Floor_Triangle2.GetFaceType());

		Assert.ThrowsException<ArgumentException>(() => SectorFace.Ceiling.GetFaceType());
		Assert.ThrowsException<ArgumentException>(() => SectorFace.Ceiling_Triangle2.GetFaceType());

		Assert.ThrowsException<ArgumentException>(() => SectorFace.Count.GetFaceType());
	}

	[TestMethod]
	public void GetDirection()
	{
		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("PositiveZ")))
			Assert.AreEqual(Direction.PositiveZ, face.GetDirection());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("NegativeZ")))
			Assert.AreEqual(Direction.NegativeZ, face.GetDirection());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("NegativeX")))
			Assert.AreEqual(Direction.NegativeX, face.GetDirection());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("PositiveX")))
			Assert.AreEqual(Direction.PositiveX, face.GetDirection());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("Diagonal")))
			Assert.AreEqual(Direction.Diagonal, face.GetDirection());

		Assert.AreEqual(Direction.None, SectorFace.Floor.GetDirection());
		Assert.AreEqual(Direction.None, SectorFace.Floor_Triangle2.GetDirection());

		Assert.AreEqual(Direction.None, SectorFace.Ceiling.GetDirection());
		Assert.AreEqual(Direction.None, SectorFace.Ceiling_Triangle2.GetDirection());

		Assert.AreEqual(Direction.None, SectorFace.Count.GetDirection());
	}

	[TestMethod]
	public void IsWall()
	{
		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("Wall")))
			Assert.IsTrue(face.IsWall());

		Assert.IsFalse(SectorFace.Floor.IsWall());
		Assert.IsFalse(SectorFace.Floor_Triangle2.IsWall());

		Assert.IsFalse(SectorFace.Ceiling.IsWall());
		Assert.IsFalse(SectorFace.Ceiling_Triangle2.IsWall());
	}

	[TestMethod]
	public void IsNonWall()
	{
		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("Wall")))
			Assert.IsFalse(face.IsNonWall());

		Assert.IsTrue(SectorFace.Floor.IsNonWall());
		Assert.IsTrue(SectorFace.Floor_Triangle2.IsNonWall());

		Assert.IsTrue(SectorFace.Ceiling.IsNonWall());
		Assert.IsTrue(SectorFace.Ceiling_Triangle2.IsNonWall());
	}

	[TestMethod]
	public void IsNonDiagonalWall()
	{
		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("Wall")))
			Assert.AreEqual(!face.ToString().Contains("Diagonal"), face.IsNonDiagonalWall());
	}

	[TestMethod]
	public void IsPositiveX()
	{
		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("PositiveX")))
			Assert.IsTrue(face.IsPositiveX());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => !f.ToString().Contains("PositiveX")))
			Assert.IsFalse(face.IsPositiveX());
	}

	[TestMethod]
	public void IsNegativeX()
	{
		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("NegativeX")))
			Assert.IsTrue(face.IsNegativeX());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => !f.ToString().Contains("NegativeX")))
			Assert.IsFalse(face.IsNegativeX());
	}

	[TestMethod]
	public void IsPositiveZ()
	{
		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("PositiveZ")))
			Assert.IsTrue(face.IsPositiveZ());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => !f.ToString().Contains("PositiveZ")))
			Assert.IsFalse(face.IsPositiveZ());
	}

	[TestMethod]
	public void IsNegativeZ()
	{
		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("NegativeZ")))
			Assert.IsTrue(face.IsNegativeZ());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => !f.ToString().Contains("NegativeZ")))
			Assert.IsFalse(face.IsNegativeZ());
	}

	[TestMethod]
	public void IsDiagonal()
	{
		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("Diagonal")))
			Assert.IsTrue(face.IsDiagonal());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => !f.ToString().Contains("Diagonal")))
			Assert.IsFalse(face.IsDiagonal());
	}

	[TestMethod]
	public void IsFloorWall()
	{
		Assert.IsTrue(SectorFace.Wall_PositiveZ_QA.IsFloorWall());
		Assert.IsTrue(SectorFace.Wall_NegativeZ_QA.IsFloorWall());
		Assert.IsTrue(SectorFace.Wall_NegativeX_QA.IsFloorWall());
		Assert.IsTrue(SectorFace.Wall_PositiveX_QA.IsFloorWall());
		Assert.IsTrue(SectorFace.Wall_Diagonal_QA.IsFloorWall());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("FloorSplit")))
			Assert.IsTrue(face.IsFloorWall());

		Assert.IsFalse(SectorFace.Wall_PositiveZ_WS.IsFloorWall());
		Assert.IsFalse(SectorFace.Wall_NegativeZ_WS.IsFloorWall());
		Assert.IsFalse(SectorFace.Wall_NegativeX_WS.IsFloorWall());
		Assert.IsFalse(SectorFace.Wall_PositiveX_WS.IsFloorWall());
		Assert.IsFalse(SectorFace.Wall_Diagonal_WS.IsFloorWall());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("CeilingSplit")))
			Assert.IsFalse(face.IsFloorWall());

		Assert.IsFalse(SectorFace.Floor.IsFloorWall());
		Assert.IsFalse(SectorFace.Floor_Triangle2.IsFloorWall());

		Assert.IsFalse(SectorFace.Ceiling.IsFloorWall());
		Assert.IsFalse(SectorFace.Ceiling_Triangle2.IsFloorWall());
	}

	[TestMethod]
	public void IsCeilingWall()
	{
		Assert.IsTrue(SectorFace.Wall_PositiveZ_WS.IsCeilingWall());
		Assert.IsTrue(SectorFace.Wall_NegativeZ_WS.IsCeilingWall());
		Assert.IsTrue(SectorFace.Wall_NegativeX_WS.IsCeilingWall());
		Assert.IsTrue(SectorFace.Wall_PositiveX_WS.IsCeilingWall());
		Assert.IsTrue(SectorFace.Wall_Diagonal_WS.IsCeilingWall());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("CeilingSplit")))
			Assert.IsTrue(face.IsCeilingWall());

		Assert.IsFalse(SectorFace.Wall_PositiveZ_QA.IsCeilingWall());
		Assert.IsFalse(SectorFace.Wall_NegativeZ_QA.IsCeilingWall());
		Assert.IsFalse(SectorFace.Wall_NegativeX_QA.IsCeilingWall());
		Assert.IsFalse(SectorFace.Wall_PositiveX_QA.IsCeilingWall());
		Assert.IsFalse(SectorFace.Wall_Diagonal_QA.IsCeilingWall());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("FloorSplit")))
			Assert.IsFalse(face.IsCeilingWall());

		Assert.IsFalse(SectorFace.Floor.IsCeilingWall());
		Assert.IsFalse(SectorFace.Floor_Triangle2.IsCeilingWall());

		Assert.IsFalse(SectorFace.Ceiling.IsCeilingWall());
		Assert.IsFalse(SectorFace.Ceiling_Triangle2.IsCeilingWall());
	}

	[TestMethod]
	public void IsMiddleWall()
	{
		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("Middle")))
			Assert.IsTrue(face.IsMiddleWall());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => !f.ToString().Contains("Middle")))
			Assert.IsFalse(face.IsMiddleWall());
	}

	[TestMethod]
	public void IsFloor()
	{
		Assert.IsTrue(SectorFace.Floor.IsFloor());
		Assert.IsTrue(SectorFace.Floor_Triangle2.IsFloor());

		Assert.IsFalse(SectorFace.Ceiling.IsFloor());
		Assert.IsFalse(SectorFace.Ceiling_Triangle2.IsFloor());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("Wall")))
			Assert.IsFalse(face.IsFloor());
	}

	[TestMethod]
	public void IsCeiling()
	{
		Assert.IsTrue(SectorFace.Ceiling.IsCeiling());
		Assert.IsTrue(SectorFace.Ceiling_Triangle2.IsCeiling());

		Assert.IsFalse(SectorFace.Floor.IsCeiling());
		Assert.IsFalse(SectorFace.Floor_Triangle2.IsCeiling());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("Wall")))
			Assert.IsFalse(face.IsCeiling());
	}

	[TestMethod]
	public void IsExtraFloorSplit()
	{
		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("_Floor")))
			Assert.IsTrue(face.IsExtraFloorSplit());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => !f.ToString().Contains("_Floor")))
			Assert.IsFalse(face.IsExtraFloorSplit());
	}

	[TestMethod]
	public void IsExtraCeilingSplit()
	{
		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("_Ceiling")))
			Assert.IsTrue(face.IsExtraCeilingSplit());

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => !f.ToString().Contains("_Ceiling")))
			Assert.IsFalse(face.IsExtraCeilingSplit());
	}

	[TestMethod]
	public void IsSpecificFloorSplit()
	{
		// True

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("PositiveZ_Floor")))
			Assert.IsTrue(face.IsSpecificFloorSplit(Direction.PositiveZ));

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("NegativeZ_Floor")))
			Assert.IsTrue(face.IsSpecificFloorSplit(Direction.NegativeZ));

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("NegativeX_Floor")))
			Assert.IsTrue(face.IsSpecificFloorSplit(Direction.NegativeX));

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("PositiveX_Floor")))
			Assert.IsTrue(face.IsSpecificFloorSplit(Direction.PositiveX));

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("Diagonal_Floor")))
			Assert.IsTrue(face.IsSpecificFloorSplit(Direction.Diagonal));

		// False

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => !f.ToString().Contains("PositiveZ_Floor")))
			Assert.IsFalse(face.IsSpecificFloorSplit(Direction.PositiveZ));

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => !f.ToString().Contains("NegativeZ_Floor")))
			Assert.IsFalse(face.IsSpecificFloorSplit(Direction.NegativeZ));

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => !f.ToString().Contains("NegativeX_Floor")))
			Assert.IsFalse(face.IsSpecificFloorSplit(Direction.NegativeX));

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => !f.ToString().Contains("PositiveX_Floor")))
			Assert.IsFalse(face.IsSpecificFloorSplit(Direction.PositiveX));

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => !f.ToString().Contains("Diagonal_Floor")))
			Assert.IsFalse(face.IsSpecificFloorSplit(Direction.Diagonal));
	}

	[TestMethod]
	public void IsSpecificCeilingSplit()
	{
		// True

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("PositiveZ_Ceiling")))
			Assert.IsTrue(face.IsSpecificCeilingSplit(Direction.PositiveZ));

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("NegativeZ_Ceiling")))
			Assert.IsTrue(face.IsSpecificCeilingSplit(Direction.NegativeZ));

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("NegativeX_Ceiling")))
			Assert.IsTrue(face.IsSpecificCeilingSplit(Direction.NegativeX));

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("PositiveX_Ceiling")))
			Assert.IsTrue(face.IsSpecificCeilingSplit(Direction.PositiveX));

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => f.ToString().Contains("Diagonal_Ceiling")))
			Assert.IsTrue(face.IsSpecificCeilingSplit(Direction.Diagonal));

		// False

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => !f.ToString().Contains("PositiveZ_Ceiling")))
			Assert.IsFalse(face.IsSpecificCeilingSplit(Direction.PositiveZ));

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => !f.ToString().Contains("NegativeZ_Ceiling")))
			Assert.IsFalse(face.IsSpecificCeilingSplit(Direction.NegativeZ));

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => !f.ToString().Contains("NegativeX_Ceiling")))
			Assert.IsFalse(face.IsSpecificCeilingSplit(Direction.NegativeX));

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => !f.ToString().Contains("PositiveX_Ceiling")))
			Assert.IsFalse(face.IsSpecificCeilingSplit(Direction.PositiveX));

		foreach (SectorFace face in Enum.GetValues<SectorFace>().Where(f => !f.ToString().Contains("Diagonal_Ceiling")))
			Assert.IsFalse(face.IsSpecificCeilingSplit(Direction.Diagonal));
	}

	[TestMethod]
	public void GetExtraFloorSplitFace()
	{
		// PositiveZ

		Assert.AreEqual(SectorFace.Wall_PositiveZ_Floor2, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.PositiveZ, 0));
		Assert.AreEqual(SectorFace.Wall_PositiveZ_Floor3, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.PositiveZ, 1));
		Assert.AreEqual(SectorFace.Wall_PositiveZ_Floor4, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.PositiveZ, 2));
		Assert.AreEqual(SectorFace.Wall_PositiveZ_Floor5, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.PositiveZ, 3));
		Assert.AreEqual(SectorFace.Wall_PositiveZ_Floor6, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.PositiveZ, 4));
		Assert.AreEqual(SectorFace.Wall_PositiveZ_Floor7, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.PositiveZ, 5));
		Assert.AreEqual(SectorFace.Wall_PositiveZ_Floor8, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.PositiveZ, 6));
		Assert.AreEqual(SectorFace.Wall_PositiveZ_Floor9, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.PositiveZ, 7));

		// NegativeZ

		Assert.AreEqual(SectorFace.Wall_NegativeZ_Floor2, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.NegativeZ, 0));
		Assert.AreEqual(SectorFace.Wall_NegativeZ_Floor3, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.NegativeZ, 1));
		Assert.AreEqual(SectorFace.Wall_NegativeZ_Floor4, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.NegativeZ, 2));
		Assert.AreEqual(SectorFace.Wall_NegativeZ_Floor5, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.NegativeZ, 3));
		Assert.AreEqual(SectorFace.Wall_NegativeZ_Floor6, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.NegativeZ, 4));
		Assert.AreEqual(SectorFace.Wall_NegativeZ_Floor7, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.NegativeZ, 5));
		Assert.AreEqual(SectorFace.Wall_NegativeZ_Floor8, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.NegativeZ, 6));
		Assert.AreEqual(SectorFace.Wall_NegativeZ_Floor9, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.NegativeZ, 7));

		// NegativeX

		Assert.AreEqual(SectorFace.Wall_NegativeX_Floor2, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.NegativeX, 0));
		Assert.AreEqual(SectorFace.Wall_NegativeX_Floor3, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.NegativeX, 1));
		Assert.AreEqual(SectorFace.Wall_NegativeX_Floor4, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.NegativeX, 2));
		Assert.AreEqual(SectorFace.Wall_NegativeX_Floor5, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.NegativeX, 3));
		Assert.AreEqual(SectorFace.Wall_NegativeX_Floor6, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.NegativeX, 4));
		Assert.AreEqual(SectorFace.Wall_NegativeX_Floor7, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.NegativeX, 5));
		Assert.AreEqual(SectorFace.Wall_NegativeX_Floor8, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.NegativeX, 6));
		Assert.AreEqual(SectorFace.Wall_NegativeX_Floor9, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.NegativeX, 7));

		// PositiveX

		Assert.AreEqual(SectorFace.Wall_PositiveX_Floor2, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.PositiveX, 0));
		Assert.AreEqual(SectorFace.Wall_PositiveX_Floor3, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.PositiveX, 1));
		Assert.AreEqual(SectorFace.Wall_PositiveX_Floor4, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.PositiveX, 2));
		Assert.AreEqual(SectorFace.Wall_PositiveX_Floor5, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.PositiveX, 3));
		Assert.AreEqual(SectorFace.Wall_PositiveX_Floor6, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.PositiveX, 4));
		Assert.AreEqual(SectorFace.Wall_PositiveX_Floor7, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.PositiveX, 5));
		Assert.AreEqual(SectorFace.Wall_PositiveX_Floor8, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.PositiveX, 6));
		Assert.AreEqual(SectorFace.Wall_PositiveX_Floor9, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.PositiveX, 7));

		// Diagonal

		Assert.AreEqual(SectorFace.Wall_Diagonal_Floor2, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.Diagonal, 0));
		Assert.AreEqual(SectorFace.Wall_Diagonal_Floor3, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.Diagonal, 1));
		Assert.AreEqual(SectorFace.Wall_Diagonal_Floor4, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.Diagonal, 2));
		Assert.AreEqual(SectorFace.Wall_Diagonal_Floor5, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.Diagonal, 3));
		Assert.AreEqual(SectorFace.Wall_Diagonal_Floor6, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.Diagonal, 4));
		Assert.AreEqual(SectorFace.Wall_Diagonal_Floor7, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.Diagonal, 5));
		Assert.AreEqual(SectorFace.Wall_Diagonal_Floor8, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.Diagonal, 6));
		Assert.AreEqual(SectorFace.Wall_Diagonal_Floor9, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.Diagonal, 7));

		// Exceptions

		Assert.ThrowsException<ArgumentException>(() => SectorFaceExtensions.GetExtraFloorSplitFace(Direction.None, 0));
	}

	[TestMethod]
	public void GetExtraCeilingSplitFace()
	{
		// PositiveZ

		Assert.AreEqual(SectorFace.Wall_PositiveZ_Ceiling2, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.PositiveZ, 0));
		Assert.AreEqual(SectorFace.Wall_PositiveZ_Ceiling3, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.PositiveZ, 1));
		Assert.AreEqual(SectorFace.Wall_PositiveZ_Ceiling4, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.PositiveZ, 2));
		Assert.AreEqual(SectorFace.Wall_PositiveZ_Ceiling5, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.PositiveZ, 3));
		Assert.AreEqual(SectorFace.Wall_PositiveZ_Ceiling6, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.PositiveZ, 4));
		Assert.AreEqual(SectorFace.Wall_PositiveZ_Ceiling7, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.PositiveZ, 5));
		Assert.AreEqual(SectorFace.Wall_PositiveZ_Ceiling8, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.PositiveZ, 6));
		Assert.AreEqual(SectorFace.Wall_PositiveZ_Ceiling9, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.PositiveZ, 7));

		// NegativeZ

		Assert.AreEqual(SectorFace.Wall_NegativeZ_Ceiling2, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.NegativeZ, 0));
		Assert.AreEqual(SectorFace.Wall_NegativeZ_Ceiling3, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.NegativeZ, 1));
		Assert.AreEqual(SectorFace.Wall_NegativeZ_Ceiling4, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.NegativeZ, 2));
		Assert.AreEqual(SectorFace.Wall_NegativeZ_Ceiling5, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.NegativeZ, 3));
		Assert.AreEqual(SectorFace.Wall_NegativeZ_Ceiling6, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.NegativeZ, 4));
		Assert.AreEqual(SectorFace.Wall_NegativeZ_Ceiling7, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.NegativeZ, 5));
		Assert.AreEqual(SectorFace.Wall_NegativeZ_Ceiling8, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.NegativeZ, 6));
		Assert.AreEqual(SectorFace.Wall_NegativeZ_Ceiling9, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.NegativeZ, 7));

		// NegativeX

		Assert.AreEqual(SectorFace.Wall_NegativeX_Ceiling2, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.NegativeX, 0));
		Assert.AreEqual(SectorFace.Wall_NegativeX_Ceiling3, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.NegativeX, 1));
		Assert.AreEqual(SectorFace.Wall_NegativeX_Ceiling4, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.NegativeX, 2));
		Assert.AreEqual(SectorFace.Wall_NegativeX_Ceiling5, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.NegativeX, 3));
		Assert.AreEqual(SectorFace.Wall_NegativeX_Ceiling6, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.NegativeX, 4));
		Assert.AreEqual(SectorFace.Wall_NegativeX_Ceiling7, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.NegativeX, 5));
		Assert.AreEqual(SectorFace.Wall_NegativeX_Ceiling8, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.NegativeX, 6));
		Assert.AreEqual(SectorFace.Wall_NegativeX_Ceiling9, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.NegativeX, 7));

		// PositiveX

		Assert.AreEqual(SectorFace.Wall_PositiveX_Ceiling2, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.PositiveX, 0));
		Assert.AreEqual(SectorFace.Wall_PositiveX_Ceiling3, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.PositiveX, 1));
		Assert.AreEqual(SectorFace.Wall_PositiveX_Ceiling4, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.PositiveX, 2));
		Assert.AreEqual(SectorFace.Wall_PositiveX_Ceiling5, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.PositiveX, 3));
		Assert.AreEqual(SectorFace.Wall_PositiveX_Ceiling6, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.PositiveX, 4));
		Assert.AreEqual(SectorFace.Wall_PositiveX_Ceiling7, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.PositiveX, 5));
		Assert.AreEqual(SectorFace.Wall_PositiveX_Ceiling8, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.PositiveX, 6));
		Assert.AreEqual(SectorFace.Wall_PositiveX_Ceiling9, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.PositiveX, 7));

		// Diagonal

		Assert.AreEqual(SectorFace.Wall_Diagonal_Ceiling2, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.Diagonal, 0));
		Assert.AreEqual(SectorFace.Wall_Diagonal_Ceiling3, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.Diagonal, 1));
		Assert.AreEqual(SectorFace.Wall_Diagonal_Ceiling4, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.Diagonal, 2));
		Assert.AreEqual(SectorFace.Wall_Diagonal_Ceiling5, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.Diagonal, 3));
		Assert.AreEqual(SectorFace.Wall_Diagonal_Ceiling6, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.Diagonal, 4));
		Assert.AreEqual(SectorFace.Wall_Diagonal_Ceiling7, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.Diagonal, 5));
		Assert.AreEqual(SectorFace.Wall_Diagonal_Ceiling8, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.Diagonal, 6));
		Assert.AreEqual(SectorFace.Wall_Diagonal_Ceiling9, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.Diagonal, 7));

		// Exceptions

		Assert.ThrowsException<ArgumentException>(() => SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.None, 0));
	}
}
