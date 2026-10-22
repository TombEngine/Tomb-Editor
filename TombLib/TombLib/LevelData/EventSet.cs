using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using TombLib.IO;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;

namespace TombLib.LevelData
{
    public enum EventSetMode
    {
        LevelScript, NodeEditor
    }
    
    public enum EventType
    {
        OnVolumeEnter,
        OnVolumeInside,
        OnVolumeLeave,
        OnLoop,
        OnLoadGame,
        OnSaveGame,
        OnLevelStart,
        OnLevelEnd,
        OnUseItem,
        OnFreeze,
		OnPickup,
		OnVehicleEnter,
		OnVehicleLeave
	}

    public class Event : ICloneable, IEquatable<Event>
    {
        private const int _noCallCounter = -1;

        public EventSetMode Mode = EventSetMode.NodeEditor;
        public string Function { get; set; } = string.Empty;
        public string Argument { get; set; } = string.Empty;
        public Vector2 NodePosition = new Vector2(float.MaxValue);
        public List<TriggerNode> Nodes { get; set; } = new List<TriggerNode>();

        public bool Empty => (Mode == EventSetMode.NodeEditor && Nodes.Count == 0) || (Mode == EventSetMode.LevelScript && string.IsNullOrEmpty(Function));

        public int CallCounter { get; set; } = 0; // How many times event can be called
        public bool Enabled { get; set; } = true; // Is event enabled or not

        public static List<EventType> GlobalEventTypes => Enum.GetValues(typeof(EventType)).Cast<EventType>().Where(t => t > EventType.OnVolumeLeave).ToList();
        public static List<EventType> VolumeEventTypes => Enum.GetValues(typeof(EventType)).Cast<EventType>().Where(t => t <= EventType.OnVolumeLeave).ToList();

        object ICloneable.Clone()
        {
            return Clone();
        }

        public Event Clone()
        {
            var evt = (Event)MemberwiseClone();

            evt.Argument = Argument;
            evt.Function = Function;
            evt.Enabled = Enabled;
            evt.CallCounter = CallCounter;
            evt.Mode = Mode;
            evt.NodePosition = NodePosition;
            evt.Nodes = new List<TriggerNode>();
            Nodes.ForEach(n => evt.Nodes.Add(n.Clone()));

            return evt;
        }

        public bool Equals(Event other)
        {
            return
                Mode == other.Mode &&
                Function == other.Function &&
                Argument == other.Argument &&
                Enabled == other.Enabled &&
                CallCounter == other.CallCounter &&
                NodePosition == other.NodePosition &&
                Nodes.Count == other.Nodes.Count &&
                Nodes.TrueForAll(n => n.GetHashCode() == other.Nodes[Nodes.IndexOf(n)].GetHashCode());
        }

        public string GenerateFunctionName(List<EventSet> eventSets)
        {
            var belongedSet = eventSets.FirstOrDefault(s => s.Events.Values.Any(e => e == this));
            if (belongedSet == null)
                return "UNKNOWN";

            var trimmedName = LuaSyntax.ReservedFunctionPrefix +
                              eventSets.IndexOf(belongedSet).ToString().PadLeft(4, '0') + "_" +
                              Regex.Replace(belongedSet.Name, "[^A-Za-z0-9]", string.Empty);

            return trimmedName + "_" + belongedSet.Events.First(e => e.Value == this).Key.ToString();
        }

        public void Write(BinaryWriterEx writer, List<EventSet> eventSets)
        {
            writer.Write((int)Mode);

            if (Mode == EventSetMode.NodeEditor)
            {
                var funcName = GenerateFunctionName(eventSets);
                writer.Write(Nodes.Count > 0 ? funcName : string.Empty);
                writer.Write(ScriptingUtils.ParseNodes(Nodes, funcName));
            }
            else
            {
                writer.Write(Function);
                writer.Write(Argument.Replace("\\n", "\n")); // Unconvert newline shortcut
            }

            int callCount = (CallCounter != 0 ? CallCounter : _noCallCounter);
            writer.Write(callCount);
            writer.Write(Enabled);
        }
    }

    public class VolumeEventSet : EventSet, IEquatable<VolumeEventSet>
    {
        public VolumeActivators Activators;

        public VolumeEventSet() : base()
        {
            Activators = VolumeActivators.Player;
            LastUsedEvent = EventType.OnVolumeEnter;

            foreach (var eventType in Event.VolumeEventTypes)
                Events.Add(eventType, new Event());
        }

        public string GetDescription()
        {
            string result = "Event set '" + Name;

            if (Activators != VolumeActivators.None)
            {
                result += "', Activated by: " +
                         ((Activators & VolumeActivators.Player) != 0 ? "Lara, " : "") +
                         ((Activators & VolumeActivators.NPCs) != 0 ? "NPCs, " : "") +
                         ((Activators & VolumeActivators.OtherMoveables) != 0 ? "Other objects, " : "") +
                         ((Activators & VolumeActivators.Statics) != 0 ? "Statics, " : "") +
                         ((Activators & VolumeActivators.Flybys) != 0 ? "Flybys, " : "");
                result = result.Substring(0, result.Length - 2) + " \n";
            }

            return result;
        }

        public bool Equals(VolumeEventSet other)
        {
            return base.Equals(other) && (Activators == other.Activators);
        }

        public new void Write(BinaryWriterEx writer, List<EventSet> eventSets)
        {
            writer.Write(Name);
            writer.Write((int)Activators);
            base.Write(writer, eventSets);
        }
    }

    public class GlobalEventSet : EventSet, IEquatable<GlobalEventSet>
    {
        public GlobalEventSet() : base()
        {
            LastUsedEvent = EventType.OnLoop;

            foreach (var eventType in Event.GlobalEventTypes)
                Events.Add(eventType, new Event());
        }

        public new void Write(BinaryWriterEx writer, List<EventSet> eventSets)
        {
            writer.Write(Name);
            base.Write(writer, eventSets);
        }

        public bool Equals(GlobalEventSet other)
        {
            return base.Equals(other);
        }
    }

    public abstract class EventSet : ICloneable, IEquatable<EventSet>
    {
        public EventType LastUsedEvent;
        public string Name;
        public string Folder = string.Empty;

        // Every volume's events can be reduced to these three.
        // If resulting volume should be one-shot trigger, we'll only use "OnEnter" event.

        public Dictionary<EventType, Event> Events = new Dictionary<EventType, Event>();

        public EventSet()
        {
            Name = string.Empty;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public EventSet Clone()
        {
            var set = (EventSet)MemberwiseClone();

            set.Events = new Dictionary<EventType, Event>();
            foreach (var evt in Events)
                set.Events.Add(evt.Key, evt.Value.Clone());

            return set;
        }

        public void Write(BinaryWriterEx writer, List<EventSet> eventSets)
        {
            var nonEmptyEvents = Events.Where(e => !e.Value.Empty).ToList();

            writer.Write(nonEmptyEvents.Count);

            foreach (var evt in nonEmptyEvents)
            {
                writer.Write((int)evt.Key);
                evt.Value.Write(writer, eventSets);
            }
        }

        public bool Equals(EventSet other)
        {
            if (other == null || GetType() != other.GetType())
                return false;

            bool setsAreEqual = true;

            foreach (var evt in Events)
            {
                if (!other.Events.ContainsKey(evt.Key) ||
                    !evt.Value.Equals(other.Events[evt.Key]))
                {
                    setsAreEqual = false;
                    break;
                }
            }

            return Name == other.Name && setsAreEqual;
        }
    }
}
