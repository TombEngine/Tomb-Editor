using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DarkUI.Docking
{
    public class DockRegionState : ICloneable, IEquatable<DockRegionState>
    {
        #region Property Region

        public DarkDockArea Area { get; set; }

        public Size Size { get; set; }

        public List<DockGroupState> Groups { get; set; }

        #endregion

        #region Constructor Region

        public DockRegionState()
        {
            Groups = new List<DockGroupState>();
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

        #endregion

        #region Clone Region

        public DockRegionState Clone() => new DockRegionState
        {
            Area = Area,
            Size = Size,
            Groups = Groups.Select(g => g.Clone()).ToList()
        };

        object ICloneable.Clone() => Clone();

        #endregion

        #region Comparison Region

        public bool Equals(DockRegionState other) => Area == other.Area && Size == other.Size && Groups.SequenceEqual(other.Groups);
        public static bool operator ==(DockRegionState first, DockRegionState second) => first.Equals(second);
        public static bool operator !=(DockRegionState first, DockRegionState second) => !first.Equals(second);
        public override int GetHashCode() => base.GetHashCode();
        public override bool Equals(object obj) => Equals(obj as DockRegionState);

        #endregion
    }
}
