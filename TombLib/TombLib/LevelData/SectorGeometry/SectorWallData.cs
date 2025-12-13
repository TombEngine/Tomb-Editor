using System;
using System.Collections.Generic;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorEnums.Extensions;

namespace TombLib.LevelData.SectorGeometry;

/// <summary>
/// Represents the wall of a single sector.
/// </summary>
public readonly struct SectorWallData
{
	/// <summary>
	/// The direction the wall is facing.
	/// </summary>
	public readonly Direction Direction;

	/// <summary>
	/// Whether the wall can have overdraw for crossing geometry or not.
	/// <para>
	/// This can only be true if the following conditions are met:<br />
	/// - The sector is <u>NOT</u> a green wall tile,<br />
	/// - The sector is a gray room border tile with a wall portal which leads to another adjoining, non-wall sector,<br />
	/// - The portal opacity (if the portal is not <see langword="null" />) is <see cref="PortalOpacity.None" />.
	/// </para>
	/// </summary>
	public readonly bool CanOverdraw;

	/// <summary>
	/// The (X, Z) position and the minimum and maximum Y coordinate of the start corner of the wall.
	/// </summary>
	public readonly WallEndData Start;

	/// <summary>
	/// The (X, Z) position and the minimum and maximum Y coordinate of the end corner of the wall.
	/// </summary>
	public readonly WallEndData End;

	/// <summary>
	/// Main floor split of the wall.
	/// </summary>
	public readonly WallSplitData QA;

	/// <summary>
	/// Main ceiling split of the wall.
	/// </summary>
	public readonly WallSplitData WS;

	/// <summary>
	/// Extra floor splits of the wall.
	/// </summary>
	public readonly IReadOnlyList<WallSplitData> ExtraFloorSplits;

	/// <summary>
	/// Extra ceiling splits of the wall.
	/// </summary>
	public readonly IReadOnlyList<WallSplitData> ExtraCeilingSplits;

	public SectorWallData(Direction direction, WallEndData start, WallEndData end, WallSplitData qa, WallSplitData ws,
		IReadOnlyList<WallSplitData> extraFloorSplits, IReadOnlyList<WallSplitData> extraCeilingSplits, bool canOverdraw)
	{
		Direction = direction;
		Start = start;
		End = end;
		QA = qa;
		WS = ws;
		ExtraFloorSplits = extraFloorSplits;
		ExtraCeilingSplits = extraCeilingSplits;
		CanOverdraw = canOverdraw;
	}

	/// <summary>
	/// Returns true if the QA part of the wall is fully above the maximum Y coordinate of the wall.
	/// </summary>
	public readonly bool IsQaFullyAboveMaxY => QA.StartY >= Start.MaxY && QA.EndY >= End.MaxY;

	/// <summary>
	/// Returns true if the WS part of the wall is fully below the minimum Y coordinate of the wall.
	/// </summary>
	public readonly bool IsWsFullyBelowMinY => WS.StartY <= Start.MinY && WS.EndY <= End.MinY;

	/// <summary>
	/// Returns true if the diagonal wall direction can have a non-diagonal (square from top-down) floor part.
	/// </summary>
	public readonly bool CanHaveNonDiagonalFloorPart(DiagonalSplit diagonalFloorSplitOfSector)
		=> (diagonalFloorSplitOfSector is DiagonalSplit.XnZp && Direction is Direction.NegativeZ or Direction.PositiveX)
		|| (diagonalFloorSplitOfSector is DiagonalSplit.XpZn && Direction is Direction.NegativeX or Direction.PositiveZ)
		|| (diagonalFloorSplitOfSector is DiagonalSplit.XpZp && Direction is Direction.NegativeZ or Direction.NegativeX)
		|| (diagonalFloorSplitOfSector is DiagonalSplit.XnZn && Direction is Direction.PositiveZ or Direction.PositiveX);

	/// <summary>
	/// Returns true if the diagonal wall direction can have a non-diagonal (square from top-down) ceiling part.
	/// </summary>
	public readonly bool CanHaveNonDiagonalCeilingPart(DiagonalSplit diagonalCeilingSplitOfSector)
		=> (diagonalCeilingSplitOfSector is DiagonalSplit.XnZp && Direction is Direction.NegativeZ or Direction.PositiveX)
		|| (diagonalCeilingSplitOfSector is DiagonalSplit.XpZn && Direction is Direction.NegativeX or Direction.PositiveZ)
		|| (diagonalCeilingSplitOfSector is DiagonalSplit.XpZp && Direction is Direction.NegativeZ or Direction.NegativeX)
		|| (diagonalCeilingSplitOfSector is DiagonalSplit.XnZn && Direction is Direction.PositiveZ or Direction.PositiveX);

	/// <summary>
	/// Returns the vertical floor part of the wall (dark green), if it can be rendered.
	/// </summary>
	public readonly IReadOnlyList<SectorFaceData> GetVerticalFloorPartFaces(DiagonalSplit diagonalFloorSplit)
	{
		bool isQaFullyAboveMaxY = IsQaFullyAboveMaxY; // Technically should be classified as a wall if true
		bool canHaveNonDiagonalFloorPart = CanHaveNonDiagonalFloorPart(diagonalFloorSplit); // The wall bit under the flat floor triangle of a diagonal wall

		var faces = new List<SectorFaceData>();
		SectorFaceData? faceData;

		for (int extraSplitIndex = -1; extraSplitIndex < ExtraFloorSplits.Count; extraSplitIndex++)
		{
			bool isQA = extraSplitIndex == -1;

			(int yStartA, int yStartB) = isQA
				? (QA.StartY, QA.EndY) // Render QA face
				: (ExtraFloorSplits[extraSplitIndex].StartY, ExtraFloorSplits[extraSplitIndex].EndY); // Render extra split face

			SectorFace sectorFace = isQA
				? SectorFaceExtensions.GetQaFace(Direction) // QA face
				: SectorFaceExtensions.GetExtraFloorSplitFace(Direction, extraSplitIndex); // Extra split face

			// Start with the floor as a baseline for the bottom end of the face
			(int yEndA, int yEndB) = (Start.MinY, End.MinY);

			if (extraSplitIndex + 1 < ExtraFloorSplits.Count) // If a next floor split exists
			{
				int yNextSplitStart = ExtraFloorSplits[extraSplitIndex + 1].StartY,
					yNextSplitEnd = ExtraFloorSplits[extraSplitIndex + 1].EndY;

				if ((canHaveNonDiagonalFloorPart || isQaFullyAboveMaxY) && (yNextSplitStart > QA.StartY || yNextSplitEnd > QA.EndY))
					continue; // Skip it, since it's above the flat, walkable triangle of a diagonal wall

				if (yNextSplitStart >= Start.MinY && yNextSplitEnd >= End.MinY) // If next split is NOT in void below floor
				{
					// Make the next split the bottom end of the face
					yEndA = yNextSplitStart;
					yEndB = yNextSplitEnd;
				}
			}

			if (yStartA <= yEndA && yStartB <= yEndB)
				continue; // 0 or negative height face, don't render it

			faceData = SectorFaceData.CreateVerticalFloorFaceData(sectorFace, (Start.X, Start.Z), (End.X, End.Z), new(yStartA, yStartB), new(yEndA, yEndB));

			if (!faceData.HasValue)
			{
				// Try overdraw

				bool isEvenWithQA = yStartA == QA.StartY && yStartB == QA.EndY; // Whether the split has the same Y coordinates as QA
																				// if so, then it covers the whole floor part of the wall
				bool canOverdraw = isEvenWithQA && (CanOverdraw || canHaveNonDiagonalFloorPart);

				if (!canOverdraw)
					continue;

				// Find lowest point between split and baseline, then try and create an overdraw face out of it
				int lowest = Math.Min(Math.Min(yStartA, yStartB), Math.Min(yEndA, yEndB));
				faceData = SectorFaceData.CreateVerticalFloorFaceData(sectorFace, (Start.X, Start.Z), (End.X, End.Z), new(yStartA, yStartB), new(lowest, lowest));
			}

			if (faceData.HasValue)
				faces.Add(faceData.Value);
		}

		return faces;
	}

	/// <summary>
	/// Returns the vertical ceiling part of the wall (light green), if it can be rendered.
	/// </summary>
	public readonly IReadOnlyList<SectorFaceData> GetVerticalCeilingPartFaces(DiagonalSplit diagonalCeilingSplit)
	{
		bool isWsFullyBelowMinY = IsWsFullyBelowMinY; // Technically should be classified as a wall if true
		bool canHaveNonDiagonalCeilingPart = CanHaveNonDiagonalCeilingPart(diagonalCeilingSplit); // The wall bit over the flat ceiling triangle of a diagonal wall

		var faces = new List<SectorFaceData>();
		SectorFaceData? faceData;

		for (int extraSplitIndex = -1; extraSplitIndex < ExtraCeilingSplits.Count; extraSplitIndex++)
		{
			bool isWS = extraSplitIndex == -1;

			(int yStartA, int yStartB) = isWS
				? (WS.StartY, WS.EndY) // Render WS face
				: (ExtraCeilingSplits[extraSplitIndex].StartY, ExtraCeilingSplits[extraSplitIndex].EndY); // Render split face

			SectorFace sectorFace = isWS
				? SectorFaceExtensions.GetWsFace(Direction)
				: SectorFaceExtensions.GetExtraCeilingSplitFace(Direction, extraSplitIndex);

			// Start with the ceiling as a baseline for the top end of the face
			(int yEndA, int yEndB) = (Start.MaxY, End.MaxY);

			if (extraSplitIndex + 1 < ExtraCeilingSplits.Count) // If a next ceiling split exists
			{
				int yNextSplitStart = ExtraCeilingSplits[extraSplitIndex + 1].StartY,
					yNextSplitEnd = ExtraCeilingSplits[extraSplitIndex + 1].EndY;

				if ((canHaveNonDiagonalCeilingPart || isWsFullyBelowMinY) && (yNextSplitStart < WS.StartY || yNextSplitEnd < WS.EndY))
					continue; // Skip it, since it's below the flat ceiling triangle

				if (yNextSplitStart <= Start.MaxY && yNextSplitEnd <= End.MaxY) // If next split is NOT in void above ceiling
				{
					// Make the next split the top end of the face
					yEndA = yNextSplitStart;
					yEndB = yNextSplitEnd;
				}
			}

			if (yStartA >= yEndA && yStartB >= yEndB)
				continue; // 0 or negative height face, don't render it

			faceData = SectorFaceData.CreateVerticalCeilingFaceData(sectorFace, (Start.X, Start.Z), (End.X, End.Z), new(yStartA, yStartB), new(yEndA, yEndB));

			if (!faceData.HasValue)
			{
				// Try overdraw

				bool isEvenWithWS = yStartA == WS.StartY && yStartB == WS.EndY; // Whether the split has the same Y coordinates as WS
																				// if so, then it covers the whole ceiling part of the wall
				bool canOverdraw = isEvenWithWS && (CanOverdraw || canHaveNonDiagonalCeilingPart);

				if (!canOverdraw)
					continue;

				// Find highest point between split and baseline, then try and create an overdraw face out of it
				int highest = Math.Max(Math.Max(yStartA, yStartB), Math.Max(yEndA, yEndB));
				faceData = SectorFaceData.CreateVerticalCeilingFaceData(sectorFace, (Start.X, Start.Z), (End.X, End.Z), new(yStartA, yStartB), new(highest, highest));
			}

			if (faceData.HasValue)
				faces.Add(faceData.Value);
		}

		return faces;
	}

	/// <summary>
	/// Returns the vertical middle face of the wall, if it can be rendered.
	/// </summary>
	public readonly SectorFaceData? GetVerticalMiddleFace()
	{
		int yStartA = QA.StartY,
			yStartB = QA.EndY,
			yEndA = WS.StartY,
			yEndB = WS.EndY;

		if (!CanOverdraw)
		{
			int qaStartY = QA.StartY, qaEndY = QA.EndY,
				wsStartY = WS.StartY, wsEndY = WS.EndY;

			if (qaStartY < Start.MinY || qaEndY < End.MinY)
			{
				qaStartY = Start.MinY;
				qaEndY = End.MinY;
			}

			if (wsStartY > Start.MaxY || wsEndY > End.MaxY)
			{
				wsStartY = Start.MaxY;
				wsEndY = End.MaxY;
			}

			yStartA = qaStartY <= Start.MinY ? Start.MinY : qaStartY;
			yStartB = qaEndY <= End.MinY ? End.MinY : qaEndY;
			yEndA = wsStartY >= Start.MaxY ? Start.MaxY : wsStartY;
			yEndB = wsEndY >= End.MaxY ? End.MaxY : wsEndY;
		}

		SectorFace sectorFace = SectorFaceExtensions.GetMiddleFace(Direction);
		return SectorFaceData.CreateVerticalMiddleFaceData(sectorFace, (Start.X, Start.Z), (End.X, End.Z), new(yStartA, yStartB), new(yEndA, yEndB));
	}

	/// <summary>
	/// Normalizes the wall splits to prevent overdraw and to make sure the wall is rendered correctly, then returns the normalized wall.
	/// </summary>
	public readonly SectorWallData Normalize(DiagonalSplit diagonalFloorSplit, DiagonalSplit diagonalCeilingSplit)
	{
		WallSplitData
			qa = NormalizeFloorSplit(QA, diagonalFloorSplit, out bool isFloorSplitInFloorVoid),
			ws = NormalizeCeilingSplit(WS, diagonalCeilingSplit, out bool isCeilingSplitInCeilingVoid);

		List<WallSplitData>
			extraFloorSplits = new(),
			extraCeilingSplits = new();

		if (!isFloorSplitInFloorVoid)
		{
			for (int i = 0; i < ExtraFloorSplits.Count; i++)
			{
				WallSplitData normalizedSplit = NormalizeFloorSplit(ExtraFloorSplits[i], diagonalFloorSplit, out isFloorSplitInFloorVoid);

				if (isFloorSplitInFloorVoid)
					break; // Stop the loop, since the rest of the splits will also be in the void, therefore not rendered

				extraFloorSplits.Add(normalizedSplit);
			}
		}

		if (!isCeilingSplitInCeilingVoid)
		{
			for (int i = 0; i < ExtraCeilingSplits.Count; i++)
			{
				WallSplitData normalizedSplit = NormalizeCeilingSplit(ExtraCeilingSplits[i], diagonalCeilingSplit, out isCeilingSplitInCeilingVoid);

				if (isCeilingSplitInCeilingVoid)
					break; // Stop the loop, since the rest of the splits will also be in the void, therefore not rendered

				extraCeilingSplits.Add(normalizedSplit);
			}
		}

		return new SectorWallData(Direction, Start, End, qa, ws, extraFloorSplits, extraCeilingSplits, CanOverdraw);
	}

	/// <summary>
	/// Normalizes the floor split to prevent overdraw and to make sure the wall is rendered correctly, then returns the normalized floor split.
	/// </summary>
	private readonly WallSplitData NormalizeFloorSplit(WallSplitData split, DiagonalSplit diagonalFloorSplit, out bool isInFloorVoid)
	{
		isInFloorVoid = true;

		bool canHaveNonDiagonalFloorPart = CanHaveNonDiagonalFloorPart(diagonalFloorSplit); // The wall bit under the flat floor triangle of a diagonal wall
		bool isFaceInFloorVoid = split.StartY < Start.MinY || split.EndY < End.MinY || (split.StartY == Start.MinY && split.EndY == End.MinY);

		if (isFaceInFloorVoid && !CanOverdraw && !canHaveNonDiagonalFloorPart) // Part of overdraw prevention
			return split; // Return, since the rest of the splits will also be in the void

		isInFloorVoid = false;

		bool isQaFullyAboveMaxY = IsQaFullyAboveMaxY; // Technically should be classified as a wall if true

		bool isEitherPointAboveCeiling = split.StartY > Start.MaxY || split.EndY > End.MaxY; // If either start or end point are in the void above ceiling
		bool areBothPointsAboveCeiling = split.StartY >= Start.MaxY && split.EndY >= End.MaxY; // Are both start and end points in the void above ceiling

		(int startY, int endY) = (split.StartY, split.EndY);

		// Full walls can't have overdraw, so if either point is in void, then snap it to ceiling
		// Diagonal walls are an exception, since, even though they are walls, they have a flat floor bit, so we can allow overdraw
		if ((isEitherPointAboveCeiling && (!CanOverdraw || isQaFullyAboveMaxY) && !canHaveNonDiagonalFloorPart) || areBothPointsAboveCeiling)
		{
			// Snap points to ceiling
			startY = Start.MaxY;
			endY = End.MaxY;
		}
		else if (startY > QA.StartY || endY > QA.EndY) // If either split point is above QA
		{
			// Snap points to the heights of QA
			startY = QA.StartY;
			endY = QA.EndY;
		}

		return new WallSplitData(startY, endY);
	}

	/// <summary>
	/// Normalizes the ceiling split to prevent overdraw and to make sure the wall is rendered correctly, then returns the normalized ceiling split.
	/// </summary>
	private readonly WallSplitData NormalizeCeilingSplit(WallSplitData split, DiagonalSplit diagonalCeilingSplit, out bool isInCeilingVoid)
	{
		isInCeilingVoid = true;

		bool canHaveNonDiagonalCeilingPart = CanHaveNonDiagonalCeilingPart(diagonalCeilingSplit); // The wall bit over the flat ceiling triangle of a diagonal wall
		bool isFaceInCeilingVoid = split.StartY > Start.MaxY || split.EndY > End.MaxY || (split.StartY == Start.MaxY && split.EndY == End.MaxY);

		if (isFaceInCeilingVoid && !CanOverdraw && !canHaveNonDiagonalCeilingPart) // Part of overdraw prevention
			return split; // Return, since the rest of the splits will also be in the void

		isInCeilingVoid = false;

		bool isWsFullyBelowMinY = IsWsFullyBelowMinY; // Technically should be classified as a wall if true

		bool isEitherPointBelowFloor = split.StartY < Start.MinY || split.EndY < End.MinY; // If either start or end point are in the void below floor
		bool areBothPointsBelowFloor = split.StartY <= Start.MinY && split.EndY <= End.MinY; // Are both start and end points in the void below floor

		(int startY, int endY) = (split.StartY, split.EndY);

		// Full walls can't have overdraw, so if either point is in void, then snap it to floor
		// Diagonal walls are an exception, since, even though they are walls, they have a flat ceiling bit, so we can allow overdraw
		if ((isEitherPointBelowFloor && (!CanOverdraw || isWsFullyBelowMinY) && !canHaveNonDiagonalCeilingPart) || areBothPointsBelowFloor)
		{
			// Snap points to floor
			startY = Start.MinY;
			endY = End.MinY;
		}
		else if (startY < WS.StartY || endY < WS.EndY) // If either split point is below WS
		{
			// Snap points to the heights of WS
			startY = WS.StartY;
			endY = WS.EndY;
		}

		return new WallSplitData(startY, endY);
	}
}
