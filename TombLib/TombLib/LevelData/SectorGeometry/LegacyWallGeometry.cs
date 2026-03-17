using System.Collections.Generic;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorEnums.Extensions;

namespace TombLib.LevelData.SectorGeometry;

/// <summary>
/// Provides legacy (PRJ-era) wall geometry calculations that only support a single extra floor split (ED)
/// and a single extra ceiling split (RF). Used during PRJ-to-PRJ2 file conversion.
/// <para>
/// Unlike <see cref="SectorWallData.GetVerticalFloorPartFaces"/>, this class does not iterate over
/// multiple extra splits and uses simplified clamping logic matching the original editor behavior.
/// </para>
/// </summary>
public static class LegacyWallGeometry
{
	/// <summary>
	/// Returns the vertical floor part faces using legacy logic.
	/// Supports at most one extra floor split (ED) at index 0 of <see cref="SectorWallData.ExtraFloorSplits"/>.
	/// </summary>
	/// <param name="wallData">The wall data to generate faces from.</param>
	/// <param name="isAnyWall">Whether the sector is classified as any type of wall.</param>
	public static IReadOnlyList<SectorFaceData> GetVerticalFloorPartFaces(SectorWallData wallData, bool isAnyWall)
	{
		var result = new List<SectorFaceData>();
		bool edVisible = false;

		int yQaA = wallData.QA.StartY,
			yQaB = wallData.QA.EndY,
			yFloorA = wallData.Start.MinY,
			yFloorB = wallData.End.MinY,
			yCeilingA = wallData.Start.MaxY,
			yCeilingB = wallData.End.MaxY,
			yEdA = wallData.ExtraFloorSplits[0].StartY,
			yEdB = wallData.ExtraFloorSplits[0].EndY,
			yA, yB;

		SectorFace
			qaFace = SectorFaceExtensions.GetQaFace(wallData.Direction),
			edFace = SectorFaceExtensions.GetExtraFloorSplitFace(wallData.Direction, 0);

		// Always check these
		if (yQaA >= yCeilingA && yQaB >= yCeilingB)
		{
			yQaA = yCeilingA;
			yQaB = yCeilingB;
		}

		// Following checks are only for wall's faces
		if (isAnyWall)
		{
			if ((yQaA > yFloorA && yQaB < yFloorB) || (yQaA < yFloorA && yQaB > yFloorB))
			{
				yQaA = yFloorA;
				yQaB = yFloorB;
			}

			if ((yQaA > yCeilingA && yQaB < yCeilingB) || (yQaA < yCeilingA && yQaB > yCeilingB))
			{
				yQaA = yCeilingA;
				yQaB = yCeilingB;
			}
		}

		if (yQaA == yFloorA && yQaB == yFloorB)
			return result; // Empty list

		// Check for extra ED split
		yA = yFloorA;
		yB = yFloorB;

		if (yEdA >= yA && yEdB >= yB && yQaA >= yEdA && yQaB >= yEdB && !(yEdA == yA && yEdB == yB))
		{
			edVisible = true;
			yA = yEdA;
			yB = yEdB;
		}

		SectorFaceData? qaFaceData = SectorFaceData.CreateVerticalFloorFaceData(qaFace, (wallData.Start.X, wallData.Start.Z), (wallData.End.X, wallData.End.Z), new(yQaA, yQaB), new(yA, yB));

		if (qaFaceData.HasValue)
			result.Add(qaFaceData.Value);

		if (edVisible)
		{
			SectorFaceData? edFaceData = SectorFaceData.CreateVerticalFloorFaceData(edFace, (wallData.Start.X, wallData.Start.Z), (wallData.End.X, wallData.End.Z), new(yEdA, yEdB), new(yFloorA, yFloorB));

			if (edFaceData.HasValue)
				result.Add(edFaceData.Value);
		}

		return result;
	}

	/// <summary>
	/// Returns the vertical ceiling part faces using legacy logic.
	/// Supports at most one extra ceiling split (RF) at index 0 of <see cref="SectorWallData.ExtraCeilingSplits"/>.
	/// </summary>
	/// <param name="wallData">The wall data to generate faces from.</param>
	/// <param name="isAnyWall">Whether the sector is classified as any type of wall.</param>
	public static IReadOnlyList<SectorFaceData> GetVerticalCeilingPartFaces(SectorWallData wallData, bool isAnyWall)
	{
		var result = new List<SectorFaceData>();
		bool rfVisible = false;

		int yWsA = wallData.WS.StartY,
			yWsB = wallData.WS.EndY,
			yFloorA = wallData.Start.MinY,
			yFloorB = wallData.End.MinY,
			yCeilingA = wallData.Start.MaxY,
			yCeilingB = wallData.End.MaxY,
			yRfA = wallData.ExtraCeilingSplits[0].StartY,
			yRfB = wallData.ExtraCeilingSplits[0].EndY,
			yA, yB;

		SectorFace
			wsFace = SectorFaceExtensions.GetWsFace(wallData.Direction),
			rfFace = SectorFaceExtensions.GetExtraCeilingSplitFace(wallData.Direction, 0);

		// Always check these
		if (yWsA <= yFloorA && yWsB <= yFloorB)
		{
			yWsA = yFloorA;
			yWsB = yFloorB;
		}

		// Following checks are only for wall's faces
		if (isAnyWall)
		{
			if ((yWsA > yCeilingA && yWsB < yCeilingB) || (yWsA < yCeilingA && yWsB > yCeilingB))
			{
				yWsA = yCeilingA;
				yWsB = yCeilingB;
			}

			if ((yWsA > yFloorA && yWsB < yFloorB) || (yWsA < yFloorA && yWsB > yFloorB))
			{
				yWsA = yFloorA;
				yWsB = yFloorB;
			}
		}

		if (yWsA == yCeilingA && yWsB == yCeilingB)
			return result; // Empty list

		// Check for extra RF split
		yA = yCeilingA;
		yB = yCeilingB;

		if (yRfA <= yA && yRfB <= yB && yWsA <= yRfA && yWsB <= yRfB && !(yRfA == yA && yRfB == yB))
		{
			rfVisible = true;
			yA = yRfA;
			yB = yRfB;
		}

		SectorFaceData? wsFaceData = SectorFaceData.CreateVerticalCeilingFaceData(wsFace, (wallData.Start.X, wallData.Start.Z), (wallData.End.X, wallData.End.Z), new(yWsA, yWsB), new(yA, yB));

		if (wsFaceData.HasValue)
			result.Add(wsFaceData.Value);

		if (rfVisible)
		{
			SectorFaceData? rfFaceData = SectorFaceData.CreateVerticalCeilingFaceData(rfFace, (wallData.Start.X, wallData.Start.Z), (wallData.End.X, wallData.End.Z), new(yRfA, yRfB), new(yCeilingA, yCeilingB));

			if (rfFaceData.HasValue)
				result.Add(rfFaceData.Value);
		}

		return result;
	}

	/// <summary>
	/// Returns the vertical middle wall face using legacy clamping logic.
	/// QA is clamped above the floor and WS is clamped below the ceiling.
	/// </summary>
	/// <param name="wallData">The wall data to generate the middle face from.</param>
	public static SectorFaceData? GetVerticalMiddlePartFace(SectorWallData wallData)
	{
		int yQaA = wallData.QA.StartY,
			yQaB = wallData.QA.EndY,
			yWsA = wallData.WS.StartY,
			yWsB = wallData.WS.EndY,
			yFloorA = wallData.Start.MinY,
			yFloorB = wallData.End.MinY,
			yCeilingA = wallData.Start.MaxY,
			yCeilingB = wallData.End.MaxY,
			yA, yB, yC, yD;

		SectorFace middleFace = SectorFaceExtensions.GetMiddleFace(wallData.Direction);

		yA = yWsA >= yCeilingA ? yCeilingA : yWsA;
		yB = yWsB >= yCeilingB ? yCeilingB : yWsB;
		yD = yQaA <= yFloorA ? yFloorA : yQaA;
		yC = yQaB <= yFloorB ? yFloorB : yQaB;

		return SectorFaceData.CreateVerticalMiddleFaceData(middleFace, (wallData.Start.X, wallData.Start.Z), (wallData.End.X, wallData.End.Z), new(yC, yD), new(yA, yB));
	}
}
