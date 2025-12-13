using System.Collections.Generic;

namespace TombLib.LevelData.VisualScripting
{
    // Currently there is just 2 types of nodes: condition and action.

    public enum NodeType
    {
        Action,
        Condition
    }

    // Condition type specifies how condition node will check condition.
    // Condition type should be internally processed by lua comparer which
    // should be called in condition function itself. For this reason, 
    // condition function declaration must feature argument which will pass
    // condition type to it.

    public enum ConditionType
    {
        Equal,
        NotEqual,
        Less,
        LessOrEqual,
        More,
        MoreOrEqual
    }

    // For action node, argument type specifies how argument will be displayed
    // in UI and how incorrect values will be filtered out.

    public enum ArgumentType
    {
        Boolean,
        Numerical,
        Vector2,
        Vector3,
        String,
        Color,
        Time,
        LuaScript,       // Listable
        VolumeEventSets, // Listable
        GlobalEventSets, // Listable
        VolumeEvents,    // Listable
        GlobalEvents,    // Listable
        Moveables,       // Listable
        Statics,         // Listable
        Cameras,         // Listable
        Sinks,           // Listable
        FlybyCameras,    // Listable
        Volumes,         // Listable
        Rooms,           // Listable
        SoundEffects,    // Listable
        SoundTracks,     // Listable
        SpriteSlots,     // Listable
        Videos,          // Listable
        WadSlots,        // Listable
        Enumeration,
        CompareOperator
    }

    public class ArgumentLayout
    {
        public string Name = string.Empty;
        public ArgumentType Type = ArgumentType.Numerical;
        public List<string> CustomEnumeration = new List<string>();
        public string DefaultValue = string.Empty;
        public string Description = string.Empty;
        public bool NewLine = false;
        public float Width = 100.0f;
    }

    public class NodeFunction
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Section { get; set; }
        public bool Conditional { get; set; }
        public string Signature { get; set; }
        public List<ArgumentLayout> Arguments { get; private set; } = new List<ArgumentLayout>();

        public override string ToString() => Name;
        public override int GetHashCode() => (Name + Conditional.ToString() + Description + Signature + Arguments.Count.ToString()).GetHashCode();
    }
}
