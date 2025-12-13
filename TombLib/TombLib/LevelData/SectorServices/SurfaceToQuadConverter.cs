using System;
using System.Linq;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorEnums.Extensions;
using TombLib.LevelData.SectorStructs;

namespace TombLib.LevelData.SectorServices;

/// <summary>
/// Provides functionality to convert triangular sector surfaces to quad surfaces.
/// </summary>
public static class SurfaceToQuadConverter
{
	/// <summary>
	/// Converts all triangular surfaces in the specified area to quad surfaces.
	/// </summary>
	/// <param name="room">The room containing the sectors to convert.</param>
	/// <param name="area">The rectangular area within the room to process.</param>
	/// <param name="vertical">Specifies whether to convert floor or ceiling surfaces.</param>
	/// <param name="snapHeight">The height increment to snap values to.</param>
	/// <returns><see langword="true" /> if any surfaces were converted, <see langword="false" /> if no changes were made.</returns>
	public static bool ConvertAreaToQuads(Room room, RectangleInt2 area, SectorVerticalPart vertical, int snapHeight)
	{
		bool isFloor = vertical.IsOnFloor();

		// De-triangulate the area by making all sectors in the area have quad surfaces
		return AreaUtils.PerformActionOnArea(area, (x, z) =>
		{
			if (!room.GetSectorTry(x, z, out Sector sector) || sector.IsFullWall)
				return false;

			ref SectorSurface surface = ref isFloor ? ref sector.Floor : ref sector.Ceiling;

			if (surface.IsQuad)
				return false;

			if (isFloor)
				DeTriangulateFloor(ref surface, snapHeight);
			else
				DeTriangulateCeiling(ref surface, snapHeight);

			sector.FixHeights(vertical, snapHeight);
			return true;
		});
	}

	#region Floor

	private static void DeTriangulateFloor(ref SectorSurface surface, int snapHeight)
	{
		int[] heights = new int[] { surface.XnZp, surface.XpZp, surface.XpZn, surface.XnZn };

		int minCornerHeight = surface.Min;
		int minCornerCount = heights.Count(height => height == minCornerHeight);

		if (minCornerCount == 1)
			HandleSingleCornerFloorCase(ref surface, minCornerHeight, snapHeight);
		else if (minCornerCount == 2)
			HandleTwoCornerFloorCase(ref surface, minCornerHeight);
		else if (minCornerCount == 3)
			SetAllCorners(ref surface, minCornerHeight); // Set all corners to min height to create a flat floor
	}

	private static void HandleSingleCornerFloorCase(ref SectorSurface surface, int minCornerHeight, int snapHeight)
	{
		if (minCornerHeight == surface.XnZp)
		{
			// Slope from NW (lowest) to diagonal
			int average = GetNormalizedFloorAverage(surface.XnZp, surface.XpZn, snapHeight);
			bool willFormQuad = surface.XnZp + average == surface.XpZn - average;

			if (!willFormQuad)
				surface.XpZn = surface.XnZp + ((average - surface.XnZp) * 2);

			surface.XpZp = surface.XnZn = average;
		}
		else if (minCornerHeight == surface.XpZp)
		{
			// Slope from NE (lowest) to diagonal
			int average = GetNormalizedFloorAverage(surface.XpZp, surface.XnZn, snapHeight);
			bool willFormQuad = surface.XpZp + average == surface.XnZn - average;

			if (!willFormQuad)
				surface.XnZn = surface.XpZp + ((average - surface.XpZp) * 2);

			surface.XnZp = surface.XpZn = average;
		}
		else if (minCornerHeight == surface.XpZn)
		{
			// Slope from SE (lowest) to diagonal
			int average = GetNormalizedFloorAverage(surface.XpZn, surface.XnZp, snapHeight);
			bool willFormQuad = surface.XpZn + average == surface.XnZp - average;

			if (!willFormQuad)
				surface.XnZp = surface.XpZn + ((average - surface.XpZn) * 2);

			surface.XpZp = surface.XnZn = average;
		}
		else if (minCornerHeight == surface.XnZn)
		{
			// Slope from SW (lowest) to diagonal
			int average = GetNormalizedFloorAverage(surface.XnZn, surface.XpZp, snapHeight);
			bool willFormQuad = surface.XnZn + average == surface.XpZp - average;

			if (!willFormQuad)
				surface.XpZp = surface.XnZn + ((average - surface.XnZn) * 2);

			surface.XnZp = surface.XpZn = average;
		}
	}

	private static void HandleTwoCornerFloorCase(ref SectorSurface surface, int minCornerHeight)
	{
		// 2 corners at min height - check if they're adjacent or diagonal
		bool adjacentXn = surface.XnZp == minCornerHeight && surface.XnZn == minCornerHeight;
		bool adjacentXp = surface.XpZp == minCornerHeight && surface.XpZn == minCornerHeight;
		bool adjacentZp = surface.XnZp == minCornerHeight && surface.XpZp == minCornerHeight;
		bool adjacentZn = surface.XnZn == minCornerHeight && surface.XpZn == minCornerHeight;

		if (adjacentXn)
		{
			// Left edge is low
			int xSlope = Math.Max(minCornerHeight - surface.XpZp, minCornerHeight - surface.XpZn); // Use larger slope
			surface.XpZp = minCornerHeight - xSlope;
			surface.XpZn = minCornerHeight - xSlope;
		}
		else if (adjacentXp)
		{
			// Right edge is low
			int xSlope = Math.Max(minCornerHeight - surface.XnZp, minCornerHeight - surface.XnZn); // Use larger slope
			surface.XnZp = minCornerHeight - xSlope;
			surface.XnZn = minCornerHeight - xSlope;
		}
		else if (adjacentZp)
		{
			// Top edge is low
			int zSlope = Math.Max(minCornerHeight - surface.XnZn, minCornerHeight - surface.XpZn); // Use larger slope
			surface.XnZn = minCornerHeight - zSlope;
			surface.XpZn = minCornerHeight - zSlope;
		}
		else if (adjacentZn)
		{
			// Bottom edge is low
			int zSlope = Math.Max(minCornerHeight - surface.XnZp, minCornerHeight - surface.XpZp); // Use larger slope
			surface.XnZp = minCornerHeight - zSlope;
			surface.XpZp = minCornerHeight - zSlope;
		}
		else
		{
			// Diagonal corners at min height
			if (surface.XnZp == minCornerHeight && surface.XpZn == minCornerHeight)
			{
				// NW and SE corners are low
				int minOfHigherCorners = Math.Min(surface.XpZp, surface.XnZn);
				SetAllCorners(ref surface, minOfHigherCorners);
			}
			else if (surface.XpZp == minCornerHeight && surface.XnZn == minCornerHeight)
			{
				// NE and SW corners are low
				int minOfHigherCorners = Math.Min(surface.XnZp, surface.XpZn);
				SetAllCorners(ref surface, minOfHigherCorners);
			}
		}
	}

	/// <summary>
	/// Calculates a normalized average height for floor corners that snaps to the specified height increment.
	/// </summary>
	/// <param name="height1">First height value.</param>
	/// <param name="height2">Second height value.</param>
	/// <param name="snapHeight">The height increment to snap to.</param>
	/// <returns>The floor-normalized average height, snapped to the specified increment.</returns>
	private static int GetNormalizedFloorAverage(int height1, int height2, int snapHeight)
		=> (int)Math.Floor((height1 + height2) / 2.0 / snapHeight) * snapHeight;

	#endregion Floor

	#region Ceiling

	private static void DeTriangulateCeiling(ref SectorSurface surface, int snapHeight)
	{
		int[] heights = new int[] { surface.XnZp, surface.XpZp, surface.XpZn, surface.XnZn };

		int maxCornerHeight = surface.Max;
		int maxCornerCount = heights.Count(height => height == maxCornerHeight);

		if (maxCornerCount == 1)
			HandleSingleCornerCeilingCase(ref surface, maxCornerHeight, snapHeight);
		else if (maxCornerCount == 2)
			HandleTwoCornerCeilingCase(ref surface, maxCornerHeight);
		else if (maxCornerCount == 3)
			SetAllCorners(ref surface, maxCornerHeight); // Set all corners to max height to create a flat floor
	}

	private static void HandleSingleCornerCeilingCase(ref SectorSurface surface, int maxCornerHeight, int snapHeight)
	{
		if (maxCornerHeight == surface.XnZp)
		{
			// Slope from NW (highest) to diagonal
			int average = GetNormalizedCeilingAverage(surface.XnZp, surface.XpZn, snapHeight);
			bool willFormQuad = surface.XnZp - average == surface.XpZn + average;

			if (!willFormQuad)
				surface.XpZn = surface.XnZp - ((surface.XnZp - average) * 2);

			surface.XpZp = surface.XnZn = average;
		}
		else if (maxCornerHeight == surface.XpZp)
		{
			// Slope from NE (highest) to diagonal
			int average = GetNormalizedCeilingAverage(surface.XpZp, surface.XnZn, snapHeight);
			bool willFormQuad = surface.XpZp - average == surface.XnZn + average;

			if (!willFormQuad)
				surface.XnZn = surface.XpZp - ((surface.XpZp - average) * 2);

			surface.XnZp = surface.XpZn = average;
		}
		else if (maxCornerHeight == surface.XpZn)
		{
			// Slope from SE (highest) to diagonal
			int average = GetNormalizedCeilingAverage(surface.XpZn, surface.XnZp, snapHeight);
			bool willFormQuad = surface.XpZn - average == surface.XnZp + average;

			if (!willFormQuad)
				surface.XnZp = surface.XpZn - ((surface.XpZn - average) * 2);

			surface.XpZp = surface.XnZn = average;
		}
		else if (maxCornerHeight == surface.XnZn)
		{
			// Slope from SW (highest) to diagonal
			int average = GetNormalizedCeilingAverage(surface.XnZn, surface.XpZp, snapHeight);
			bool willFormQuad = surface.XnZn - average == surface.XpZp + average;

			if (!willFormQuad)
				surface.XpZp = surface.XnZn - ((surface.XnZn - average) * 2);

			surface.XnZp = surface.XpZn = average;
		}
	}

	private static void HandleTwoCornerCeilingCase(ref SectorSurface surface, int maxCornerHeight)
	{
		// 2 corners at max height - check if they're adjacent or diagonal
		bool adjacentXn = surface.XnZp == maxCornerHeight && surface.XnZn == maxCornerHeight;
		bool adjacentXp = surface.XpZp == maxCornerHeight && surface.XpZn == maxCornerHeight;
		bool adjacentZp = surface.XnZp == maxCornerHeight && surface.XpZp == maxCornerHeight;
		bool adjacentZn = surface.XnZn == maxCornerHeight && surface.XpZn == maxCornerHeight;

		if (adjacentXn)
		{
			// Left edge is high
			int xSlope = Math.Min(maxCornerHeight - surface.XpZp, maxCornerHeight - surface.XpZn); // Use smaller slope
			surface.XpZp = maxCornerHeight - xSlope;
			surface.XpZn = maxCornerHeight - xSlope;
		}
		else if (adjacentXp)
		{
			// Right edge is high
			int xSlope = Math.Min(maxCornerHeight - surface.XnZp, maxCornerHeight - surface.XnZn); // Use smaller slope
			surface.XnZp = maxCornerHeight - xSlope;
			surface.XnZn = maxCornerHeight - xSlope;
		}
		else if (adjacentZp)
		{
			// Top edge is high
			int zSlope = Math.Min(maxCornerHeight - surface.XnZn, maxCornerHeight - surface.XpZn); // Use smaller slope
			surface.XnZn = maxCornerHeight - zSlope;
			surface.XpZn = maxCornerHeight - zSlope;
		}
		else if (adjacentZn)
		{
			// Bottom edge is high
			int zSlope = Math.Min(maxCornerHeight - surface.XnZp, maxCornerHeight - surface.XpZp); // Use smaller slope
			surface.XnZp = maxCornerHeight - zSlope;
			surface.XpZp = maxCornerHeight - zSlope;
		}
		else
		{
			// Diagonal corners at max height
			if (surface.XnZp == maxCornerHeight && surface.XpZn == maxCornerHeight)
			{
				// NW and SE corners are high
				int maxOfLowerCorners = Math.Max(surface.XpZp, surface.XnZn);
				SetAllCorners(ref surface, maxOfLowerCorners);
			}
			else if (surface.XpZp == maxCornerHeight && surface.XnZn == maxCornerHeight)
			{
				// NE and SW corners are high
				int maxOfLowerCorners = Math.Max(surface.XnZp, surface.XpZn);
				SetAllCorners(ref surface, maxOfLowerCorners);
			}
		}
	}

	/// <summary>
	/// Calculates a normalized average height for ceiling corners that snaps to the specified height increment.
	/// </summary>
	/// <param name="height1">First height value.</param>
	/// <param name="height2">Second height value.</param>
	/// <param name="snapHeight">The height increment to snap to.</param>
	/// <returns>The ceiling-normalized average height, snapped to the specified increment.</returns>
	private static int GetNormalizedCeilingAverage(int height1, int height2, int snapHeight)
		=> (int)Math.Ceiling((height1 + height2) / 2.0 / snapHeight) * snapHeight;

	#endregion Ceiling

	/// <summary>
	/// Sets all four corners of a surface to the same height.
	/// </summary>
	/// <param name="surface">Reference to the surface to modify.</param>
	/// <param name="height">The height to set for all corners.</param>
	private static void SetAllCorners(ref SectorSurface surface, int height)
		=> surface.XnZp = surface.XpZp = surface.XpZn = surface.XnZn = height;
}
