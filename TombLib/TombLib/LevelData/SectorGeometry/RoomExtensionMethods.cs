using System.Collections.Generic;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorEnums.Extensions;

namespace TombLib.LevelData.SectorGeometry;

/// <summary>
/// Extension methods on <see cref="Room"/> for computing per-sector wall geometry data
/// (<see cref="SectorWallData"/>) in each cardinal and diagonal direction.
/// <para>
/// Each method gathers the floor/ceiling heights from the sector and its neighbor (or adjoining
/// portal room), applies diagonal split corrections, and optionally normalizes the result to
/// prevent overdraw artifacts.
/// </para>
/// </summary>
public static class RoomExtensionMethods
{
	/// <summary>
	/// Computes the wall data for the +Z (north-facing) edge of sector (<paramref name="x"/>, <paramref name="z"/>).
	/// The wall runs from corner (x+1, z+1) to (x, z+1), facing toward the +Z neighbor.
	/// </summary>
	/// <param name="room">The room containing the sector.</param>
	/// <param name="x">Sector X coordinate.</param>
	/// <param name="z">Sector Z coordinate.</param>
	/// <param name="normalize">Whether to normalize splits to prevent overdraw.</param>
	public static SectorWallData GetPositiveZWallData(this Room room, int x, int z, bool normalize)
	{
		Sector sector = room.Sectors[x, z];
		Sector neighborSector = room.Sectors[x, z + 1];
		Sector adjoiningSector = null;

		int startMinY = neighborSector.Floor.XpZn,
			startMaxY = neighborSector.Ceiling.XpZn,

			endMinY = neighborSector.Floor.XnZn,
			endMaxY = neighborSector.Ceiling.XnZn,

			qaStartY = sector.Floor.XpZp,
			qaEndY = sector.Floor.XnZp,

			wsStartY = sector.Ceiling.XpZp,
			wsEndY = sector.Ceiling.XnZp;

		List<WallSplitData>
			extraFloorSplits = new(),
			extraCeilingSplits = new();

		for (int i = 0; i < sector.ExtraFloorSplits.Count; i++)
		{
			extraFloorSplits.Add(new WallSplitData(
				sector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XpZp),
				sector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XnZp)));
		}

		for (int i = 0; i < sector.ExtraCeilingSplits.Count; i++)
		{
			extraCeilingSplits.Add(new WallSplitData(
				sector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XpZp),
				sector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XnZp)));
		}

		if (sector.WallPortal is not null)
		{
			// Get the adjoining room of the portal
			Room adjoiningRoom = sector.WallPortal.AdjoiningRoom;

			if (room.Alternated &&
				room.AlternateBaseRoom is not null &&
				adjoiningRoom.Alternated &&
				adjoiningRoom.AlternateRoom is not null)
			{
				adjoiningRoom = adjoiningRoom.AlternateRoom;
			}

			// Now get the facing sector on the adjoining room and calculate the correct heights
			int facingX = x + (room.Position.X - adjoiningRoom.Position.X);

			adjoiningSector = adjoiningRoom.GetSectorTry(facingX, adjoiningRoom.NumZSectors - 2);

			int qAportal = adjoiningRoom.Position.Y + (adjoiningSector?.Floor.XpZp ?? 0);
			int qBportal = adjoiningRoom.Position.Y + (adjoiningSector?.Floor.XnZp ?? 0);

			if (adjoiningSector?.Floor.DiagonalSplit is DiagonalSplit.XpZn)
				qAportal = qBportal;
			else if (adjoiningSector?.Floor.DiagonalSplit is DiagonalSplit.XnZn)
				qBportal = qAportal;

			qaStartY = qAportal - room.Position.Y;
			qaEndY = qBportal - room.Position.Y;

			int wAportal = adjoiningRoom.Position.Y + (adjoiningSector?.Ceiling.XpZp ?? 0);
			int wBportal = adjoiningRoom.Position.Y + (adjoiningSector?.Ceiling.XnZp ?? 0);

			if (adjoiningSector?.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
				wAportal = wBportal;
			else if (adjoiningSector?.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
				wBportal = wAportal;

			wsStartY = wAportal - room.Position.Y;
			wsEndY = wBportal - room.Position.Y;

			WallSplitData newSplit;

			for (int i = 0; i < adjoiningSector?.ExtraFloorSplits.Count; i++)
			{
				newSplit = new WallSplitData(adjoiningRoom.Position.Y - room.Position.Y + adjoiningSector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XpZp),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningSector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XnZp));

				if (i >= extraFloorSplits.Count)
					extraFloorSplits.Add(newSplit);
				else
					extraFloorSplits[i] = newSplit;
			}

			for (int i = 0; i < adjoiningSector?.ExtraCeilingSplits.Count; i++)
			{
				newSplit = new WallSplitData(adjoiningRoom.Position.Y - room.Position.Y + adjoiningSector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XpZp),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningSector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XnZp));

				if (i >= extraCeilingSplits.Count)
					extraCeilingSplits.Add(newSplit);
				else
					extraCeilingSplits[i] = newSplit;
			}
		}

		if (sector.Floor.DiagonalSplit is DiagonalSplit.XpZn)
		{
			qaStartY = sector.Floor.XnZp;
			qaEndY = sector.Floor.XnZp;
		}
		else if (sector.Floor.DiagonalSplit is DiagonalSplit.XnZn)
		{
			qaStartY = sector.Floor.XpZp;
			qaEndY = sector.Floor.XpZp;
		}

		if (neighborSector.Floor.DiagonalSplit is DiagonalSplit.XnZp)
		{
			startMinY = neighborSector.Floor.XpZn;
			endMinY = neighborSector.Floor.XpZn;
		}
		else if (neighborSector.Floor.DiagonalSplit is DiagonalSplit.XpZp)
		{
			startMinY = neighborSector.Floor.XnZn;
			endMinY = neighborSector.Floor.XnZn;
		}

		if (sector.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
		{
			wsStartY = sector.Ceiling.XnZp;
			wsEndY = sector.Ceiling.XnZp;
		}
		else if (sector.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
		{
			wsStartY = sector.Ceiling.XpZp;
			wsEndY = sector.Ceiling.XpZp;
		}

		if (neighborSector.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
		{
			startMaxY = neighborSector.Ceiling.XpZn;
			endMaxY = neighborSector.Ceiling.XpZn;
		}
		else if (neighborSector.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
		{
			startMaxY = neighborSector.Ceiling.XnZn;
			endMaxY = neighborSector.Ceiling.XnZn;
		}

		var wall = new SectorWallData
		(
			direction: Direction.PositiveZ,

			start: new WallEndData
			(
				x: x + 1,
				z: z + 1,
				minY: startMinY,
				maxY: startMaxY
			),

			end: new WallEndData
			(
				x: x,
				z: z + 1,
				minY: endMinY,
				maxY: endMaxY
			),

			qa: new WallSplitData
			(
				startY: qaStartY,
				endY: qaEndY
			),

			ws: new WallSplitData
			(
				startY: wsStartY,
				endY: wsEndY
			),

			extraFloorSplits: extraFloorSplits,
			extraCeilingSplits: extraCeilingSplits,

			canOverdraw: CanOverdraw(sector, Direction.PositiveZ, adjoiningSector)
		);

		return normalize
			? wall.Normalize(sector.Floor.DiagonalSplit, sector.Ceiling.DiagonalSplit)
			: wall;
	}

	/// <summary>
	/// Computes the wall data for the -Z (south-facing) edge of sector (<paramref name="x"/>, <paramref name="z"/>).
	/// The wall runs from corner (x, z) to (x+1, z), facing toward the -Z neighbor.
	/// </summary>
	/// <param name="room">The room containing the sector.</param>
	/// <param name="x">Sector X coordinate.</param>
	/// <param name="z">Sector Z coordinate.</param>
	/// <param name="normalize">Whether to normalize splits to prevent overdraw.</param>
	public static SectorWallData GetNegativeZWallData(this Room room, int x, int z, bool normalize)
	{
		Sector sector = room.Sectors[x, z];
		Sector neighborSector = room.Sectors[x, z - 1];
		Sector adjoiningSector = null;

		int startMinY = neighborSector.Floor.XnZp,
			startMaxY = neighborSector.Ceiling.XnZp,

			endMinY = neighborSector.Floor.XpZp,
			endMaxY = neighborSector.Ceiling.XpZp,

			qaStartY = sector.Floor.XnZn,
			qaEndY = sector.Floor.XpZn,

			wsStartY = sector.Ceiling.XnZn,
			wsEndY = sector.Ceiling.XpZn;

		List<WallSplitData>
			extraFloorSplits = new(),
			extraCeilingSplits = new();

		for (int i = 0; i < sector.ExtraFloorSplits.Count; i++)
		{
			extraFloorSplits.Add(new WallSplitData(
				sector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XnZn),
				sector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XpZn)));
		}

		for (int i = 0; i < sector.ExtraCeilingSplits.Count; i++)
		{
			extraCeilingSplits.Add(new WallSplitData(
				sector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XnZn),
				sector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XpZn)));
		}

		if (sector.WallPortal is not null)
		{
			// Get the adjoining room of the portal
			Room adjoiningRoom = sector.WallPortal.AdjoiningRoom;

			if (room.Alternated &&
				room.AlternateBaseRoom is not null &&
				adjoiningRoom.Alternated &&
				adjoiningRoom.AlternateRoom is not null)
			{
				adjoiningRoom = adjoiningRoom.AlternateRoom;
			}

			// Now get the facing sector on the adjoining room and calculate the correct heights
			int facingX = x + (room.Position.X - adjoiningRoom.Position.X);

			adjoiningSector = adjoiningRoom.GetSectorTry(facingX, 1);

			int qAportal = adjoiningRoom.Position.Y + (adjoiningSector?.Floor.XnZn ?? 0);
			int qBportal = adjoiningRoom.Position.Y + (adjoiningSector?.Floor.XpZn ?? 0);

			if (adjoiningSector?.Floor.DiagonalSplit is DiagonalSplit.XnZp)
				qAportal = qBportal;
			else if (adjoiningSector?.Floor.DiagonalSplit is DiagonalSplit.XpZp)
				qBportal = qAportal;

			qaStartY = qAportal - room.Position.Y;
			qaEndY = qBportal - room.Position.Y;

			int wAportal = adjoiningRoom.Position.Y + (adjoiningSector?.Ceiling.XnZn ?? 0);
			int wBportal = adjoiningRoom.Position.Y + (adjoiningSector?.Ceiling.XpZn ?? 0);

			if (adjoiningSector?.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
				wAportal = wBportal;
			else if (adjoiningSector?.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
				wBportal = wAportal;

			wsStartY = wAportal - room.Position.Y;
			wsEndY = wBportal - room.Position.Y;

			WallSplitData newSplit;

			for (int i = 0; i < adjoiningSector?.ExtraFloorSplits.Count; i++)
			{
				newSplit = new WallSplitData(adjoiningRoom.Position.Y - room.Position.Y + adjoiningSector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XnZn),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningSector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XpZn));

				if (i >= extraFloorSplits.Count)
					extraFloorSplits.Add(newSplit);
				else
					extraFloorSplits[i] = newSplit;
			}

			for (int i = 0; i < adjoiningSector?.ExtraCeilingSplits.Count; i++)
			{
				newSplit = new WallSplitData(adjoiningRoom.Position.Y - room.Position.Y + adjoiningSector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XnZn),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningSector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XpZn));

				if (i >= extraCeilingSplits.Count)
					extraCeilingSplits.Add(newSplit);
				else
					extraCeilingSplits[i] = newSplit;
			}
		}

		if (sector.Floor.DiagonalSplit is DiagonalSplit.XpZp)
		{
			qaStartY = sector.Floor.XnZn;
			qaEndY = sector.Floor.XnZn;
		}
		else if (sector.Floor.DiagonalSplit is DiagonalSplit.XnZp)
		{
			qaStartY = sector.Floor.XpZn;
			qaEndY = sector.Floor.XpZn;
		}

		if (neighborSector.Floor.DiagonalSplit is DiagonalSplit.XpZn)
		{
			startMinY = neighborSector.Floor.XnZp;
			endMinY = neighborSector.Floor.XnZp;
		}
		else if (neighborSector.Floor.DiagonalSplit is DiagonalSplit.XnZn)
		{
			startMinY = neighborSector.Floor.XpZp;
			endMinY = neighborSector.Floor.XpZp;
		}

		if (sector.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
		{
			wsStartY = sector.Ceiling.XnZn;
			wsEndY = sector.Ceiling.XnZn;
		}
		else if (sector.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
		{
			wsStartY = sector.Ceiling.XpZn;
			wsEndY = sector.Ceiling.XpZn;
		}

		if (neighborSector.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
		{
			startMaxY = neighborSector.Ceiling.XnZp;
			endMaxY = neighborSector.Ceiling.XnZp;
		}
		else if (neighborSector.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
		{
			startMaxY = neighborSector.Ceiling.XpZp;
			endMaxY = neighborSector.Ceiling.XpZp;
		}

		var wall = new SectorWallData
		(
			direction: Direction.NegativeZ,

			start: new WallEndData
			(
				x: x,
				z: z,
				minY: startMinY,
				maxY: startMaxY
			),

			end: new WallEndData
			(
				x: x + 1,
				z: z,
				minY: endMinY,
				maxY: endMaxY
			),

			qa: new WallSplitData
			(
				startY: qaStartY,
				endY: qaEndY
			),

			ws: new WallSplitData
			(
				startY: wsStartY,
				endY: wsEndY
			),

			extraFloorSplits: extraFloorSplits,
			extraCeilingSplits: extraCeilingSplits,

			canOverdraw: CanOverdraw(sector, Direction.NegativeZ, adjoiningSector)
		);

		return normalize
			? wall.Normalize(sector.Floor.DiagonalSplit, sector.Ceiling.DiagonalSplit)
			: wall;
	}

	/// <summary>
	/// Computes the wall data for the +X (east-facing) edge of sector (<paramref name="x"/>, <paramref name="z"/>).
	/// The wall runs from corner (x+1, z) to (x+1, z+1), facing toward the +X neighbor.
	/// </summary>
	/// <param name="room">The room containing the sector.</param>
	/// <param name="x">Sector X coordinate.</param>
	/// <param name="z">Sector Z coordinate.</param>
	/// <param name="normalize">Whether to normalize splits to prevent overdraw.</param>
	public static SectorWallData GetPositiveXWallData(this Room room, int x, int z, bool normalize)
	{
		Sector sector = room.Sectors[x, z];
		Sector neighborSector = room.Sectors[x + 1, z];
		Sector adjoiningSector = null;

		int startMinY = neighborSector.Floor.XnZn,
			startMaxY = neighborSector.Ceiling.XnZn,

			endMinY = neighborSector.Floor.XnZp,
			endMaxY = neighborSector.Ceiling.XnZp,

			qaStartY = sector.Floor.XpZn,
			qaEndY = sector.Floor.XpZp,

			wsStartY = sector.Ceiling.XpZn,
			wsEndY = sector.Ceiling.XpZp;

		List<WallSplitData>
			extraFloorSplits = new(),
			extraCeilingSplits = new();

		for (int i = 0; i < sector.ExtraFloorSplits.Count; i++)
		{
			extraFloorSplits.Add(new WallSplitData(
				sector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XpZn),
				sector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XpZp)));
		}

		for (int i = 0; i < sector.ExtraCeilingSplits.Count; i++)
		{
			extraCeilingSplits.Add(new WallSplitData(
				sector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XpZn),
				sector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XpZp)));
		}

		if (sector.WallPortal is not null)
		{
			// Get the adjoining room of the portal
			Room adjoiningRoom = sector.WallPortal.AdjoiningRoom;

			if (room.Alternated &&
				room.AlternateBaseRoom is not null &&
				adjoiningRoom.Alternated &&
				adjoiningRoom.AlternateRoom is not null)
			{
				adjoiningRoom = adjoiningRoom.AlternateRoom;
			}

			// Now get the facing sector on the adjoining room and calculate the correct heights
			int facingZ = z + (room.Position.Z - adjoiningRoom.Position.Z);

			adjoiningSector = adjoiningRoom.GetSectorTry(adjoiningRoom.NumXSectors - 2, facingZ);

			int qAportal = adjoiningRoom.Position.Y + (adjoiningSector?.Floor.XpZn ?? 0);
			int qBportal = adjoiningRoom.Position.Y + (adjoiningSector?.Floor.XpZp ?? 0);

			if (adjoiningSector?.Floor.DiagonalSplit is DiagonalSplit.XnZn)
				qAportal = qBportal;
			else if (adjoiningSector?.Floor.DiagonalSplit is DiagonalSplit.XnZp)
				qBportal = qAportal;

			qaStartY = qAportal - room.Position.Y;
			qaEndY = qBportal - room.Position.Y;

			int wAportal = adjoiningRoom.Position.Y + (adjoiningSector?.Ceiling.XpZn ?? 0);
			int wBportal = adjoiningRoom.Position.Y + (adjoiningSector?.Ceiling.XpZp ?? 0);

			if (adjoiningSector?.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
				wAportal = wBportal;
			else if (adjoiningSector?.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
				wBportal = wAportal;

			wsStartY = wAportal - room.Position.Y;
			wsEndY = wBportal - room.Position.Y;

			WallSplitData newSplit;

			for (int i = 0; i < adjoiningSector?.ExtraFloorSplits.Count; i++)
			{
				newSplit = new WallSplitData(adjoiningRoom.Position.Y - room.Position.Y + adjoiningSector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XpZn),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningSector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XpZp));

				if (i >= extraFloorSplits.Count)
					extraFloorSplits.Add(newSplit);
				else
					extraFloorSplits[i] = newSplit;
			}

			for (int i = 0; i < adjoiningSector?.ExtraCeilingSplits.Count; i++)
			{
				newSplit = new WallSplitData(adjoiningRoom.Position.Y - room.Position.Y + adjoiningSector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XpZn),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningSector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XpZp));

				if (i >= extraCeilingSplits.Count)
					extraCeilingSplits.Add(newSplit);
				else
					extraCeilingSplits[i] = newSplit;
			}
		}

		if (sector.Floor.DiagonalSplit is DiagonalSplit.XnZn)
		{
			qaStartY = sector.Floor.XpZp;
			qaEndY = sector.Floor.XpZp;
		}
		else if (sector.Floor.DiagonalSplit is DiagonalSplit.XnZp)
		{
			qaStartY = sector.Floor.XpZn;
			qaEndY = sector.Floor.XpZn;
		}

		if (neighborSector.Floor.DiagonalSplit is DiagonalSplit.XpZn)
		{
			startMinY = neighborSector.Floor.XnZp;
			endMinY = neighborSector.Floor.XnZp;
		}
		else if (neighborSector.Floor.DiagonalSplit is DiagonalSplit.XpZp)
		{
			startMinY = neighborSector.Floor.XnZn;
			endMinY = neighborSector.Floor.XnZn;
		}

		if (sector.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
		{
			wsStartY = sector.Ceiling.XpZp;
			wsEndY = sector.Ceiling.XpZp;
		}
		else if (sector.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
		{
			wsStartY = sector.Ceiling.XpZn;
			wsEndY = sector.Ceiling.XpZn;
		}

		if (neighborSector.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
		{
			startMaxY = neighborSector.Ceiling.XnZp;
			endMaxY = neighborSector.Ceiling.XnZp;
		}
		else if (neighborSector.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
		{
			startMaxY = neighborSector.Ceiling.XnZn;
			endMaxY = neighborSector.Ceiling.XnZn;
		}

		var wall = new SectorWallData
		(
			direction: Direction.PositiveX,

			start: new WallEndData
			(
				x: x + 1,
				z: z,
				minY: startMinY,
				maxY: startMaxY
			),

			end: new WallEndData
			(
				x: x + 1,
				z: z + 1,
				minY: endMinY,
				maxY: endMaxY
			),

			qa: new WallSplitData
			(
				startY: qaStartY,
				endY: qaEndY
			),

			ws: new WallSplitData
			(
				startY: wsStartY,
				endY: wsEndY
			),

			extraFloorSplits: extraFloorSplits,
			extraCeilingSplits: extraCeilingSplits,

			canOverdraw: CanOverdraw(sector, Direction.PositiveX, adjoiningSector)
		);

		return normalize
			? wall.Normalize(sector.Floor.DiagonalSplit, sector.Ceiling.DiagonalSplit)
			: wall;
	}

	/// <summary>
	/// Computes the wall data for the -X (west-facing) edge of sector (<paramref name="x"/>, <paramref name="z"/>).
	/// The wall runs from corner (x, z+1) to (x, z), facing toward the -X neighbor.
	/// </summary>
	/// <param name="room">The room containing the sector.</param>
	/// <param name="x">Sector X coordinate.</param>
	/// <param name="z">Sector Z coordinate.</param>
	/// <param name="normalize">Whether to normalize splits to prevent overdraw.</param>
	public static SectorWallData GetNegativeXWallData(this Room room, int x, int z, bool normalize)
	{
		Sector sector = room.Sectors[x, z];
		Sector neighborSector = room.Sectors[x - 1, z];
		Sector adjoiningSector = null;

		int startMinY = neighborSector.Floor.XpZp,
			startMaxY = neighborSector.Ceiling.XpZp,

			endMinY = neighborSector.Floor.XpZn,
			endMaxY = neighborSector.Ceiling.XpZn,

			qaStartY = sector.Floor.XnZp,
			qaEndY = sector.Floor.XnZn,

			wsStartY = sector.Ceiling.XnZp,
			wsEndY = sector.Ceiling.XnZn;

		List<WallSplitData>
			extraFloorSplits = new(),
			extraCeilingSplits = new();

		for (int i = 0; i < sector.ExtraFloorSplits.Count; i++)
		{
			extraFloorSplits.Add(new WallSplitData(
				sector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XnZp),
				sector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XnZn)));
		}

		for (int i = 0; i < sector.ExtraCeilingSplits.Count; i++)
		{
			extraCeilingSplits.Add(new WallSplitData(
				sector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XnZp),
				sector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XnZn)));
		}

		if (sector.WallPortal is not null)
		{
			// Get the adjoining room of the portal
			Room adjoiningRoom = sector.WallPortal.AdjoiningRoom;

			if (room.Alternated &&
				room.AlternateBaseRoom is not null &&
				adjoiningRoom.Alternated &&
				adjoiningRoom.AlternateRoom is not null)
			{
				adjoiningRoom = adjoiningRoom.AlternateRoom;
			}

			// Now get the facing sector on the adjoining room and calculate the correct heights
			int facingZ = z + (room.Position.Z - adjoiningRoom.Position.Z);

			adjoiningSector = adjoiningRoom.GetSectorTry(1, facingZ);

			int qAportal = adjoiningRoom.Position.Y + (adjoiningSector?.Floor.XnZp ?? 0);
			int qBportal = adjoiningRoom.Position.Y + (adjoiningSector?.Floor.XnZn ?? 0);

			if (adjoiningSector?.Floor.DiagonalSplit is DiagonalSplit.XpZp)
				qAportal = qBportal;
			else if (adjoiningSector?.Floor.DiagonalSplit is DiagonalSplit.XpZn)
				qBportal = qAportal;

			qaStartY = qAportal - room.Position.Y;
			qaEndY = qBportal - room.Position.Y;

			int wAportal = adjoiningRoom.Position.Y + (adjoiningSector?.Ceiling.XnZp ?? 0);
			int wBportal = adjoiningRoom.Position.Y + (adjoiningSector?.Ceiling.XnZn ?? 0);

			if (adjoiningSector?.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
				wAportal = wBportal;
			else if (adjoiningSector?.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
				wBportal = wAportal;

			wsStartY = wAportal - room.Position.Y;
			wsEndY = wBportal - room.Position.Y;

			WallSplitData newSplit;

			for (int i = 0; i < adjoiningSector?.ExtraFloorSplits.Count; i++)
			{
				newSplit = new WallSplitData(adjoiningRoom.Position.Y - room.Position.Y + adjoiningSector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XnZp),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningSector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XnZn));

				if (i >= extraFloorSplits.Count)
					extraFloorSplits.Add(newSplit);
				else
					extraFloorSplits[i] = newSplit;
			}

			for (int i = 0; i < adjoiningSector?.ExtraCeilingSplits.Count; i++)
			{
				newSplit = new WallSplitData(adjoiningRoom.Position.Y - room.Position.Y + adjoiningSector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XnZp),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningSector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XnZn));

				if (i >= extraCeilingSplits.Count)
					extraCeilingSplits.Add(newSplit);
				else
					extraCeilingSplits[i] = newSplit;
			}
		}

		if (sector.Floor.DiagonalSplit is DiagonalSplit.XpZn)
		{
			qaStartY = sector.Floor.XnZp;
			qaEndY = sector.Floor.XnZp;
		}
		else if (sector.Floor.DiagonalSplit is DiagonalSplit.XpZp)
		{
			qaStartY = sector.Floor.XnZn;
			qaEndY = sector.Floor.XnZn;
		}

		if (neighborSector.Floor.DiagonalSplit is DiagonalSplit.XnZn)
		{
			startMinY = neighborSector.Floor.XpZp;
			endMinY = neighborSector.Floor.XpZp;
		}
		else if (neighborSector.Floor.DiagonalSplit is DiagonalSplit.XnZp)
		{
			startMinY = neighborSector.Floor.XpZn;
			endMinY = neighborSector.Floor.XpZn;
		}

		if (sector.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
		{
			wsStartY = sector.Ceiling.XnZp;
			wsEndY = sector.Ceiling.XnZp;
		}
		else if (sector.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
		{
			wsStartY = sector.Ceiling.XnZn;
			wsEndY = sector.Ceiling.XnZn;
		}

		if (neighborSector.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
		{
			startMaxY = neighborSector.Ceiling.XpZp;
			endMaxY = neighborSector.Ceiling.XpZp;
		}
		else if (neighborSector.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
		{
			startMaxY = neighborSector.Ceiling.XpZn;
			endMaxY = neighborSector.Ceiling.XpZn;
		}

		var wall = new SectorWallData
		(
			direction: Direction.NegativeX,

			start: new WallEndData
			(
				x: x,
				z: z + 1,
				minY: startMinY,
				maxY: startMaxY
			),

			end: new WallEndData
			(
				x: x,
				z: z,
				minY: endMinY,
				maxY: endMaxY
			),

			qa: new WallSplitData
			(
				startY: qaStartY,
				endY: qaEndY
			),

			ws: new WallSplitData
			(
				startY: wsStartY,
				endY: wsEndY
			),

			extraFloorSplits: extraFloorSplits,
			extraCeilingSplits: extraCeilingSplits,

			canOverdraw: CanOverdraw(sector, Direction.NegativeX, adjoiningSector)
		);

		return normalize
			? wall.Normalize(sector.Floor.DiagonalSplit, sector.Ceiling.DiagonalSplit)
			: wall;
	}

	/// <summary>
	/// Computes the wall data for a diagonal edge of sector (<paramref name="x"/>, <paramref name="z"/>).
	/// The diagonal direction is determined by the sector's floor or ceiling <see cref="DiagonalSplit"/>.
	/// Overdraw is always disabled for diagonal walls since the logic differs from cardinal walls.
	/// </summary>
	/// <param name="room">The room containing the sector.</param>
	/// <param name="x">Sector X coordinate.</param>
	/// <param name="z">Sector Z coordinate.</param>
	/// <param name="isDiagonalCeiling">If <see langword="true"/>, uses the ceiling's diagonal split;
	/// otherwise uses the floor's diagonal split.</param>
	/// <param name="normalize">Whether to normalize splits to prevent overdraw.</param>
	public static SectorWallData GetDiagonalWallData(this Room room, int x, int z, bool isDiagonalCeiling, bool normalize)
	{
		Sector sector = room.Sectors[x, z];

		int startX, startZ, endX, endZ,
			startMinY, startMaxY, endMinY, endMaxY,
			qaStartY, qaEndY, wsStartY, wsEndY;

		List<WallSplitData>
			extraFloorSplits = new(),
			extraCeilingSplits = new();

		switch (isDiagonalCeiling ? sector.Ceiling.DiagonalSplit : sector.Floor.DiagonalSplit)
		{
			case DiagonalSplit.XpZn:
				startX = x + 1;
				startZ = z + 1;

				endX = x;
				endZ = z;

				startMinY = isDiagonalCeiling
					? (sector.IsAnyWall ? sector.Floor.XnZp : sector.Floor.XpZp) // DiagonalCeiling
					: sector.Floor.XnZp; // DiagonalFloor

				startMaxY = isDiagonalCeiling
					? sector.Ceiling.XnZp // DiagonalCeiling
					: (sector.IsAnyWall ? sector.Ceiling.XnZp : sector.Ceiling.XpZp); // DiagonalFloor

				endMinY = isDiagonalCeiling
					? (sector.IsAnyWall ? sector.Floor.XnZp : sector.Floor.XnZn) // DiagonalCeiling
					: sector.Floor.XnZp; // DiagonalFloor

				endMaxY = isDiagonalCeiling
					? sector.Ceiling.XnZp // DiagonalCeiling
					: (sector.IsAnyWall ? sector.Ceiling.XnZp : sector.Ceiling.XnZn); // DiagonalFloor

				qaStartY = sector.Floor.XpZp;
				qaEndY = sector.Floor.XnZn;
				wsStartY = sector.Ceiling.XpZp;
				wsEndY = sector.Ceiling.XnZn;

				for (int i = 0; i < sector.ExtraFloorSplits.Count; i++)
				{
					extraFloorSplits.Add(new WallSplitData(
						sector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XpZp),
						sector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XnZn)));
				}

				for (int i = 0; i < sector.ExtraCeilingSplits.Count; i++)
				{
					extraCeilingSplits.Add(new WallSplitData(
						sector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XpZp),
						sector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XnZn)));
				}

				break;

			case DiagonalSplit.XnZn:
				startX = x + 1;
				startZ = z;

				endX = x;
				endZ = z + 1;

				startMinY = isDiagonalCeiling
					? (sector.IsAnyWall ? sector.Floor.XpZp : sector.Floor.XpZn) // DiagonalCeiling
					: sector.Floor.XpZp; // DiagonalFloor

				startMaxY = isDiagonalCeiling
					? sector.Ceiling.XpZp // DiagonalCeiling
					: (sector.IsAnyWall ? sector.Ceiling.XpZp : sector.Ceiling.XpZn); // DiagonalFloor

				endMinY = isDiagonalCeiling
					? (sector.IsAnyWall ? sector.Floor.XpZp : sector.Floor.XnZp) // DiagonalCeiling
					: sector.Floor.XpZp; // DiagonalFloor

				endMaxY = isDiagonalCeiling
					? sector.Ceiling.XpZp // DiagonalCeiling
					: (sector.IsAnyWall ? sector.Ceiling.XpZp : sector.Ceiling.XnZp); // DiagonalFloor

				qaStartY = sector.Floor.XpZn;
				qaEndY = sector.Floor.XnZp;
				wsStartY = sector.Ceiling.XpZn;
				wsEndY = sector.Ceiling.XnZp;

				for (int i = 0; i < sector.ExtraFloorSplits.Count; i++)
				{
					extraFloorSplits.Add(new WallSplitData(
						sector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XpZn),
						sector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XnZp)));
				}

				for (int i = 0; i < sector.ExtraCeilingSplits.Count; i++)
				{
					extraCeilingSplits.Add(new WallSplitData(
						sector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XpZn),
						sector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XnZp)));
				}

				break;

			case DiagonalSplit.XnZp:
				startX = x;
				startZ = z;

				endX = x + 1;
				endZ = z + 1;

				startMinY = isDiagonalCeiling
					? (sector.IsAnyWall ? sector.Floor.XpZn : sector.Floor.XnZn) // DiagonalCeiling
					: sector.Floor.XpZn; // DiagonalFloor

				startMaxY = isDiagonalCeiling
					? sector.Ceiling.XpZn // DiagonalCeiling
					: (sector.IsAnyWall ? sector.Ceiling.XpZn : sector.Ceiling.XnZn); // DiagonalFloor

				endMinY = isDiagonalCeiling
					? (sector.IsAnyWall ? sector.Floor.XpZn : sector.Floor.XpZp) // DiagonalCeiling
					: sector.Floor.XpZn; // DiagonalFloor

				endMaxY = isDiagonalCeiling
					? sector.Ceiling.XpZn // DiagonalCeiling
					: (sector.IsAnyWall ? sector.Ceiling.XpZn : sector.Ceiling.XpZp); // DiagonalFloor

				qaStartY = sector.Floor.XnZn;
				qaEndY = sector.Floor.XpZp;
				wsStartY = sector.Ceiling.XnZn;
				wsEndY = sector.Ceiling.XpZp;

				for (int i = 0; i < sector.ExtraFloorSplits.Count; i++)
				{
					extraFloorSplits.Add(new WallSplitData(
						sector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XnZn),
						sector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XpZp)));
				}

				for (int i = 0; i < sector.ExtraCeilingSplits.Count; i++)
				{
					extraCeilingSplits.Add(new WallSplitData(
						sector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XnZn),
						sector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XpZp)));
				}

				break;

			default:
				startX = x;
				startZ = z + 1;

				endX = x + 1;
				endZ = z;

				startMinY = isDiagonalCeiling
					? (sector.IsAnyWall ? sector.Floor.XnZn : sector.Floor.XnZp) // DiagonalCeiling
					: sector.Floor.XnZn; // DiagonalFloor

				startMaxY = isDiagonalCeiling
					? sector.Ceiling.XnZn // DiagonalCeiling
					: (sector.IsAnyWall ? sector.Ceiling.XnZn : sector.Ceiling.XnZp); // DiagonalFloor

				endMinY = isDiagonalCeiling
					? (sector.IsAnyWall ? sector.Floor.XnZn : sector.Floor.XpZn) // DiagonalCeiling
					: sector.Floor.XnZn; // DiagonalFloor

				endMaxY = isDiagonalCeiling
					? sector.Ceiling.XnZn // DiagonalCeiling
					: (sector.IsAnyWall ? sector.Ceiling.XnZn : sector.Ceiling.XpZn); // DiagonalFloor

				qaStartY = sector.Floor.XnZp;
				qaEndY = sector.Floor.XpZn;
				wsStartY = sector.Ceiling.XnZp;
				wsEndY = sector.Ceiling.XpZn;

				for (int i = 0; i < sector.ExtraFloorSplits.Count; i++)
				{
					extraFloorSplits.Add(new WallSplitData(
						sector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XnZp),
						sector.GetHeight(SectorVerticalPartExtensions.GetExtraFloorSplit(i), SectorEdge.XpZn)));
				}

				for (int i = 0; i < sector.ExtraCeilingSplits.Count; i++)
				{
					extraCeilingSplits.Add(new WallSplitData(
						sector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XnZp),
						sector.GetHeight(SectorVerticalPartExtensions.GetExtraCeilingSplit(i), SectorEdge.XpZn)));
				}

				break;
		}

		var wall = new SectorWallData
		(
			direction: Direction.Diagonal,

			start: new WallEndData
			(
				x: startX,
				z: startZ,
				minY: startMinY,
				maxY: startMaxY
			),

			end: new WallEndData
			(
				x: endX,
				z: endZ,
				minY: endMinY,
				maxY: endMaxY
			),

			qa: new WallSplitData
			(
				startY: qaStartY,
				endY: qaEndY
			),

			ws: new WallSplitData
			(
				startY: wsStartY,
				endY: wsEndY
			),

			extraFloorSplits: extraFloorSplits,
			extraCeilingSplits: extraCeilingSplits,

			canOverdraw: false // Dismissed for diagonal walls, the logic is different
		);

		return normalize
			? wall.Normalize(sector.Floor.DiagonalSplit, sector.Ceiling.DiagonalSplit)
			: wall;
	}

	/// <summary>
	/// Determines whether the wall at the given sector and direction is allowed to "overdraw" —
	/// i.e., extend face geometry beyond the strict floor/ceiling bounds.
	/// <para>
	/// Overdraw is allowed when:<br/>
	/// • The sector is not a wall (non-portal case), OR<br/>
	/// • The sector has a wall portal with <see cref="PortalOpacity.None"/>, and the adjoining
	///   sector is not a solid wall (or is a diagonal wall whose split faces toward this direction).
	/// </para>
	/// </summary>
	private static bool CanOverdraw(Sector sector, Direction direction, Sector adjoiningSector)
	{
		if (sector.WallPortal is not null)
		{
			if (sector.WallPortal.Opacity is not PortalOpacity.None)
				return false; // Do not allow overdraw on textured portals

			if (adjoiningSector is not null)
			{
				DiagonalSplit adjoiningDiagonalSplit = adjoiningSector.Floor.DiagonalSplit;
				bool isAdjoiningSectorADiagonalWall = adjoiningSector.IsAnyWall && adjoiningDiagonalSplit is not DiagonalSplit.None;

				if (isAdjoiningSectorADiagonalWall)
				{
					// Depending on the direction, we need to check the diagonal split of the adjoining sector, as 2 sides of a diagonal wall can be solid
					return direction switch
					{
						Direction.PositiveZ => adjoiningDiagonalSplit is DiagonalSplit.XnZn or DiagonalSplit.XpZn,
						Direction.PositiveX => adjoiningDiagonalSplit is DiagonalSplit.XnZn or DiagonalSplit.XnZp,
						Direction.NegativeZ => adjoiningDiagonalSplit is DiagonalSplit.XpZp or DiagonalSplit.XnZp,
						Direction.NegativeX => adjoiningDiagonalSplit is DiagonalSplit.XpZp or DiagonalSplit.XpZn,
						_ => false
					};
				}

				return !adjoiningSector.IsAnyWall;
			}
		}

		return !sector.IsAnyWall;
	}
}
