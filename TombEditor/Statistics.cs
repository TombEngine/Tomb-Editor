using System;

namespace TombEditor
{
    public class StatisticSummary : IEquatable<StatisticSummary>
    {
        private const string _compileStatsFormat = "Rooms: {0}, Boxes: {1}, Overlaps: {2}, Textures: {3}";

        public Statistics LevelStats = new Statistics() { Name = "Level" };
        public Statistics RoomStats  = new Statistics() { Name = "Room"  };

        public int RoomCount;
        public int? TextureCount = null;
        public int? BoxCount = null;
        public int? OverlapCount = null;

        public override string ToString() => string.Format(_compileStatsFormat, RoomCount, BoxCount, OverlapCount, TextureCount) + "\n" +
                                             LevelStats.ToString() + "\n" + RoomStats.ToString();

        public static bool operator ==(StatisticSummary first, StatisticSummary second) => first.ToString() == second.ToString();
        public static bool operator !=(StatisticSummary first, StatisticSummary second) => !(first == second);

        public bool Equals(StatisticSummary other) => other != null && this == other;
        public override bool Equals(object obj) => Equals(obj as StatisticSummary);
        public override int GetHashCode() => this.ToString().GetHashCode();
    }

    public class Statistics
    {
        const string _statsFormat = "{0} Statistic : Items: {1} Moveables: {2}, Statics: {3}, Triggers: {4}, DynLights: {5}, All Lights: {6}, Cameras: {7}, Flybys:{8}, Vertices:{9}, Faces:{10}";

        public string Name;

        public int MoveableCount = 0;
        public int StaticCount = 0;
        public int TriggerCount = 0;
        public int DynLightCount = 0;
        public int LightCount = 0;
        public int CameraCount = 0;
        public int FlybyCount = 0;
        public int VertexCount = 0;
        public int FaceCount = 0;

        public string GetDynLightCount(int lightLimit) => DynLightCount >= lightLimit ? "!!" + DynLightCount.ToString() + "!!" : DynLightCount.ToString();
        public string GetMoveableCountString(int limit) => MoveableCount >= limit ? "!!" + MoveableCount.ToString() + "!!" : MoveableCount.ToString();

        public override string ToString() => string.Format(_statsFormat, Name, MoveableCount + StaticCount, MoveableCount, StaticCount, TriggerCount, LightCount, DynLightCount, CameraCount, FlybyCount, VertexCount, FaceCount);
    }
}
