using TombLib.LevelData.SectorEnums;

namespace TombLib.LevelData.SectorStructs;

/// <summary>
/// Represents the offset which needs to be traversed to reach a specific edge of another sector.
/// </summary>
/// <param name="OffsetX">The X-axis displacement needed to reach the adjacent sector.</param>
/// <param name="OffsetZ">The Z-axis displacement needed to reach the adjacent sector.</param>
/// <param name="Edge">The specific edge of the target sector that is being referenced.</param>
/// <remarks>
/// This structure is typically used for sector-to-sector navigation and connectivity in level layouts,
/// helping to determine neighboring relationships between sectors.
/// </remarks>
public record struct SectorEdgeOffset(int OffsetX, int OffsetZ, SectorEdge Edge);
