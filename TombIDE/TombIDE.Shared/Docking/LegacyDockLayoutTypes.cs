#nullable enable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TombIDE.Shared.Docking;

public enum DarkDockArea
{
	None,
	Document,
	Left,
	Right,
	Bottom
}

public class DockGroupState : IEquatable<DockGroupState>
{
	public List<string> Contents { get; set; }

	public string VisibleContent { get; set; }

	public int Order { get; set; }

	public Size Size { get; set; }

	public DockGroupState()
	{
		Contents = [];
		VisibleContent = string.Empty;
		Order = 0;
		Size = new Size(100, 100);
	}

	public bool Equals(DockGroupState? other) =>
		other is not null &&
		VisibleContent == other.VisibleContent &&
		Order == other.Order &&
		Size == other.Size &&
		Contents.SequenceEqual(other.Contents);

	public static bool operator ==(DockGroupState? first, DockGroupState? second)
		=> ReferenceEquals(first, second) || first?.Equals(second) == true;

	public static bool operator !=(DockGroupState? first, DockGroupState? second)
		=> !(first == second);

	public override bool Equals(object? obj)
		=> Equals(obj as DockGroupState);

	public override int GetHashCode()
		=> HashCode.Combine(VisibleContent, Order, Size);
}

public class DockRegionState : IEquatable<DockRegionState>
{
	public DarkDockArea Area { get; set; }

	public Size Size { get; set; }

	public List<DockGroupState> Groups { get; set; }

	public DockRegionState()
	{
		Groups = [];
	}

	public DockRegionState(DarkDockArea area)
		: this()
	{
		Area = area;
	}

	public DockRegionState(DarkDockArea area, Size size)
		: this(area)
	{
		Size = size;
	}

	public bool Equals(DockRegionState? other)
		=> other is not null && Area == other.Area && Size == other.Size && Groups.SequenceEqual(other.Groups);

	public static bool operator ==(DockRegionState? first, DockRegionState? second)
		=> ReferenceEquals(first, second) || first?.Equals(second) == true;

	public static bool operator !=(DockRegionState? first, DockRegionState? second)
		=> !(first == second);

	public override bool Equals(object? obj)
		=> Equals(obj as DockRegionState);

	public override int GetHashCode()
		=> HashCode.Combine(Area, Size);
}

public class DockPanelState : IEquatable<DockPanelState>
{
	public List<DockRegionState> Regions { get; set; }

	public DockPanelState()
	{
		Regions = [];
	}

	public bool Equals(DockPanelState? other)
		=> other is not null && Regions.SequenceEqual(other.Regions);

	public static bool operator ==(DockPanelState? first, DockPanelState? second)
		=> ReferenceEquals(first, second) || first?.Equals(second) == true;

	public static bool operator !=(DockPanelState? first, DockPanelState? second)
		=> !(first == second);

	public override bool Equals(object? obj)
		=> Equals(obj as DockPanelState);

	public override int GetHashCode()
		=> Regions.Count;
}
