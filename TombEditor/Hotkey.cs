using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using TombLib.Utils;

namespace TombEditor
{
    public struct Hotkey : IEquatable<Hotkey>, IComparable<Hotkey>
    {
        private const uint MAPVK_VK_TO_CHAR = 2;
        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        // Remap weird ToString mappings for certain characters
        public static Dictionary<Keys, string> FriendlyKeyNameTable = new Dictionary<Keys, string>
        {
            { Keys.PageDown, "PageDown" },
            { Keys.Return, "Enter" },
            { Keys.Add, "NumPadPlus" },
            { Keys.Subtract, "NumPadMinus" },
            { Keys.Multiply, "NumPadMultiply" },
            { Keys.Divide, "NumPadDivide" },
            { Keys.Decimal, "NumPadDecimal" },
            { Keys.Scroll, "ScrollLock" },
            { Keys.Back, "Backspace" },
            { Keys.Capital, "CapsLock" },
            { Keys.Oemtilde, GetOemKeyName(Keys.Oemtilde) },
            { Keys.OemQuestion, GetOemKeyName(Keys.OemQuestion) },
            { Keys.Oemplus, GetOemKeyName(Keys.Oemplus) },
            { Keys.OemMinus, GetOemKeyName(Keys.OemMinus) },
            { Keys.Oemcomma, GetOemKeyName(Keys.Oemcomma) },
            { Keys.OemPeriod, GetOemKeyName(Keys.OemPeriod) },
            { Keys.OemOpenBrackets, GetOemKeyName(Keys.OemOpenBrackets) },
            { Keys.OemCloseBrackets, GetOemKeyName(Keys.OemCloseBrackets) },
            { Keys.Oem1, GetOemKeyName(Keys.Oem1) },
            { Keys.Oem5, GetOemKeyName(Keys.Oem5) },
            { Keys.Oem7, GetOemKeyName(Keys.Oem7) },
            { Keys.Oem8, GetOemKeyName(Keys.Oem8) },
            { Keys.OemBackslash, GetOemKeyName(Keys.OemBackslash) },
            { Keys.D0, "0" },
            { Keys.D1, "1" },
            { Keys.D2, "2" },
            { Keys.D3, "3" },
            { Keys.D4, "4" },
            { Keys.D5, "5" },
            { Keys.D6, "6" },
            { Keys.D7, "7" },
            { Keys.D8, "8" },
            { Keys.D9, "9" }
        };

        public Keys Keys { get; set; }

        public Keys MainKey
        {
            get { return Keys & Keys.KeyCode; }
            set { Keys = (value & Keys.KeyCode) | (Keys & ~Keys.KeyCode); }
        }

        public override string ToString()
        {
            string result = "";
            result += (Keys & Keys.Control) != Keys.None ? "Ctrl+" : "";
            result += (Keys & Keys.Shift) != Keys.None ? "Shift+" : "";
            result += (Keys & Keys.Alt) != Keys.None ? "Alt+" : "";
            if(MainKey != Keys.None)
                result += FriendlyKeyNameTable.ContainsKey(MainKey) ? FriendlyKeyNameTable[MainKey] : MainKey.ToString();
            return result;
        }

        public static Hotkey FromString(string str)
        {
            Hotkey result = new Hotkey();
            string[] keyNames = str.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string keyName in keyNames)
            {
                if (keyName.Equals("Ctrl", StringComparison.InvariantCultureIgnoreCase) ||
                    keyName.Equals("Control", StringComparison.InvariantCultureIgnoreCase))
                    result.Keys |= Keys.Control;
                else if (keyName.Equals("Shift", StringComparison.InvariantCultureIgnoreCase))
                    result.Keys |= Keys.Shift;
                else if (keyName.Equals("Alt", StringComparison.InvariantCultureIgnoreCase))
                    result.Keys |= Keys.Alt;
                else
                    result.MainKey = FriendlyKeyNameTable.ContainsValue(keyName) ? FriendlyKeyNameTable.First(n => n.Value == keyName).Key : (Keys)Enum.Parse(typeof(Keys), keyName, true);
            }
            return result;
        }

        public bool Equals(Hotkey other) => Keys == other.Keys;
        public override bool Equals(object other) => other is Hotkey && Equals((Hotkey)other);
        public override int GetHashCode() => (int)MainKey;
        public static implicit operator Hotkey(Keys keys) => new Hotkey { Keys = keys };

        public int CompareTo(Hotkey other)
        {
            return ((uint)Keys).CompareTo((uint)other.Keys);
        }

        private static string GetOemKeyName(Keys key)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                case PlatformID.WinCE:
                    break;
                default:
                    return key.ToString();
            }

            return ((char)(ushort)MapVirtualKey((uint)key, MAPVK_VK_TO_CHAR)).ToString();
        }
    }

    public class HotkeySets : IXmlSerializable, ICloneable, IEnumerable<KeyValuePair<string, SortedSet<Hotkey>>>
    {
        private readonly SortedList<string, SortedSet<Hotkey>> _list = new SortedList<string, SortedSet<Hotkey>>(StringComparer.InvariantCultureIgnoreCase);

        public HotkeySets()
            : this(KeyboardLayoutDetector.KeyboardLayout)
        { }
        public HotkeySets(KeyboardLayout keyboardLayout, bool generateEmptyInstead = false)
        {
            // Generate entries for all commands
            // We also want them to be in the XML.
            foreach (CommandObj command in CommandHandler.Commands)
                _list.Add(command.Name, new SortedSet<Hotkey>());

            // Generate keyboard layout
            if (!generateEmptyInstead)
                GenerateDefault(keyboardLayout);
        }

        private HotkeySets(HotkeySets other)
        {
            _list = new SortedList<string, SortedSet<Hotkey>>(
                other._list.DicSelect(e => new SortedSet<Hotkey>(e.Value)), StringComparer.InvariantCultureIgnoreCase);
        }

        public SortedSet<Hotkey> this[string key]
        {
            get { return _list[key]; }
            set { _list[key] = value; }
        }

        public SortedSet<Hotkey> this[CommandObj command]
        {
            get { return _list[command.Name]; }
            set { _list[command.Name] = value; }
        }

        public int Count => _list.Count;
        public HotkeySets Clone() => new HotkeySets(this);
        object ICloneable.Clone() => Clone();

        public IEnumerator<KeyValuePair<string, SortedSet<Hotkey>>> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        XmlSchema IXmlSerializable.GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
            string startNodeName = reader.Name;
            if (reader.IsEmptyElement)
                return;
            while (reader.Name != startNodeName || reader.NodeType != XmlNodeType.EndElement)
            {
                reader.Read();
                if (reader.NodeType != XmlNodeType.Element)
                    continue;

                string commandName = reader.GetAttribute("CommandName");
                string hotkeys = reader.GetAttribute("Hotkeys");

                if (hotkeys.Equals("UseDefault", StringComparison.InvariantCultureIgnoreCase))
                    continue;
                if (!_list.ContainsKey(commandName))
                    continue;
                string unused;
                this[commandName] = ParseHotkeys(reader.GetAttribute("Hotkeys"), out unused);
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteComment("An example hotkey would be 'Ctrl+C'. You can specify multiple hotkeys per command like this 'Shift+Ctrl+T, Alt+I'.");
            writer.WriteComment("You may combine any key with Ctrl, Shift or Alt, but other combinations are not allowed.");
            writer.WriteComment("For a comprehensive list of possible keys look here: https://msdn.microsoft.com/en-us/library/system.windows.forms.keys(v=vs.110).aspx#Members ");
            HotkeySets @default = new HotkeySets();
            foreach (var commandAndHotkey in _list)
            {
                bool isDefault = commandAndHotkey.Value.SetEquals(@default[commandAndHotkey.Key]);
                writer.WriteStartElement("HotkeySet");
                writer.WriteAttributeString("CommandName", commandAndHotkey.Key);
                writer.WriteAttributeString("Hotkeys", isDefault ? "UseDefault" : string.Join(", ", commandAndHotkey.Value));
                writer.WriteEndElement();
            }
        }

        public static SortedSet<Hotkey> ParseHotkeys(string str, out string errorMessage)
        {
            SortedSet<Hotkey> hotkeys = new SortedSet<Hotkey>();
            string[] keyGroups = str.Split(new[] { ", " }, StringSplitOptions.None);
            foreach (string keyGroup in keyGroups)
                if (!string.IsNullOrWhiteSpace(keyGroup))
                    try
                    {
                        hotkeys.Add(Hotkey.FromString(keyGroup));
                    }
                    catch (Exception)
                    {
                        errorMessage = "Invalid key combination '" + keyGroup + "'";
                        return null;
                    }
            errorMessage = null;
            return hotkeys;
        }

        private void GenerateDefault(KeyboardLayout layout)
        {
            Keys Q = Keys.Q;
            Keys A = Keys.A;
            Keys W = Keys.W;
            Keys Y = Keys.Y;
            Keys Z = Keys.Z;

            if (layout == KeyboardLayout.Azerty)
            {
                Q = Keys.A;
                W = Keys.Z;
                A = Keys.Q;
                Z = Keys.W;
            }
            else if (layout == KeyboardLayout.Qwertz)
            {
                Y = Keys.Z;
                Z = Keys.Y;
            }

            this["CancelAnyAction"] = new SortedSet<Hotkey> { Keys.Escape };
            this["Switch2DMode"] = new SortedSet<Hotkey> { Keys.F1 };
            this["SwitchGeometryMode"] = new SortedSet<Hotkey> { Keys.F2 };
            this["SwitchFaceEditMode"] = new SortedSet<Hotkey> { Keys.F3 };
            this["SwitchLightingMode"] = new SortedSet<Hotkey> { Keys.F4 };
            this["SwitchObjectPlacementMode"] = new SortedSet<Hotkey> { Keys.F7 };
            this["ResetCamera"] = new SortedSet<Hotkey> { Keys.F6 };
            this["AddTrigger"] = new SortedSet<Hotkey> { Keys.T };
            this["AddTriggerWithBookmark"] = new SortedSet<Hotkey> { Keys.T | Keys.Shift };
            this["AddPortal"] = new SortedSet<Hotkey> { Keys.P };
            this["EditObject"] = new SortedSet<Hotkey> { Keys.O };
            this["EditObjectTransform"] = new SortedSet<Hotkey> { Keys.T | Keys.Control };
            this["RenameObject"] = new SortedSet<Hotkey> { Keys.N };
            this["SwitchBlendMode"] = new SortedSet<Hotkey> { Keys.NumPad1 | Keys.Shift, Keys.D1 | Keys.Shift };
            this["SetTextureDoubleSided"] = new SortedSet<Hotkey> { Keys.NumPad2 | Keys.Shift, Keys.D2 | Keys.Shift };
            this["SetTextureInvisible"] = new SortedSet<Hotkey> { Keys.NumPad3 | Keys.Shift, Keys.D3 | Keys.Shift };
            this["RotateObjectLeft"] = new SortedSet<Hotkey> { Keys.Left | Keys.Shift };
            this["RotateObjectRight"] = new SortedSet<Hotkey> { Keys.Right | Keys.Shift };
            this["RotateObjectUp"] = new SortedSet<Hotkey> { Keys.Up | Keys.Shift };
            this["RotateObjectDown"] = new SortedSet<Hotkey> { Keys.Down | Keys.Shift };
            this["MoveObjectLeft"] = new SortedSet<Hotkey> { Keys.Left | Keys.Control };
            this["MoveObjectRight"] = new SortedSet<Hotkey> { Keys.Right | Keys.Control };
            this["MoveObjectForward"] = new SortedSet<Hotkey> { Keys.Up | Keys.Control };
            this["MoveObjectBack"] = new SortedSet<Hotkey> { Keys.Down | Keys.Control };
            this["MoveObjectUp"] = new SortedSet<Hotkey> { Keys.PageUp | Keys.Control };
            this["MoveObjectDown"] = new SortedSet<Hotkey> { Keys.PageDown | Keys.Control };
            this["SelectPreviousRoom"] = new SortedSet<Hotkey> { Keys.Control | Keys.Tab };
            this["MoveRoomLeft"] = new SortedSet<Hotkey> { Keys.Left | Keys.Alt };
            this["MoveRoomRight"] = new SortedSet<Hotkey> { Keys.Right | Keys.Alt };
            this["MoveRoomForward"] = new SortedSet<Hotkey> { Keys.Up | Keys.Alt };
            this["MoveRoomBack"] = new SortedSet<Hotkey> { Keys.Down | Keys.Alt };
            this["MoveRoomUp"] = new SortedSet<Hotkey> { Keys.PageUp | Keys.Alt };
            this["MoveRoomDown"] = new SortedSet<Hotkey> { Keys.PageDown | Keys.Alt };
            this["RaiseQA1Click"] = new SortedSet<Hotkey> { Q };
            this["RaiseQA4Click"] = new SortedSet<Hotkey> { Q | Keys.Shift };
            this["LowerQA1Click"] = new SortedSet<Hotkey> { A };
            this["LowerQA4Click"] = new SortedSet<Hotkey> { A | Keys.Shift };
            this["RaiseQA1ClickSmooth"] = new SortedSet<Hotkey> { Q | Keys.Alt };
            this["RaiseQA4ClickSmooth"] = new SortedSet<Hotkey> { Q | Keys.Alt | Keys.Shift };
            this["LowerQA1ClickSmooth"] = new SortedSet<Hotkey> { A | Keys.Alt };
            this["LowerQA4ClickSmooth"] = new SortedSet<Hotkey> { A | Keys.Alt | Keys.Shift };
            this["RaiseWS1Click"] = new SortedSet<Hotkey> { W };
            this["RaiseWS4Click"] = new SortedSet<Hotkey> { W | Keys.Shift };
            this["LowerWS1Click"] = new SortedSet<Hotkey> { Keys.S };
            this["LowerWS4Click"] = new SortedSet<Hotkey> { Keys.S | Keys.Shift };
            this["RaiseWS1ClickSmooth"] = new SortedSet<Hotkey> { W | Keys.Alt };
            this["RaiseWS4ClickSmooth"] = new SortedSet<Hotkey> { W | Keys.Alt | Keys.Shift };
            this["LowerWS1ClickSmooth"] = new SortedSet<Hotkey> { Keys.S | Keys.Alt };
            this["LowerWS4ClickSmooth"] = new SortedSet<Hotkey> { Keys.S | Keys.Alt | Keys.Shift };
            this["RaiseED1Click"] = new SortedSet<Hotkey> { Keys.E };
            this["RaiseED4Click"] = new SortedSet<Hotkey> { Keys.E | Keys.Shift };
            this["LowerED1Click"] = new SortedSet<Hotkey> { Keys.D };
            this["LowerED4Click"] = new SortedSet<Hotkey> { Keys.D | Keys.Shift };
            this["RaiseED1ClickSmooth"] = new SortedSet<Hotkey> { Keys.E | Keys.Alt };
            this["RaiseED4ClickSmooth"] = new SortedSet<Hotkey> { Keys.E | Keys.Alt | Keys.Shift };
            this["LowerED1ClickSmooth"] = new SortedSet<Hotkey> { Keys.D | Keys.Alt };
            this["LowerED4ClickSmooth"] = new SortedSet<Hotkey> { Keys.D | Keys.Alt | Keys.Shift };
            this["RaiseRF1Click"] = new SortedSet<Hotkey> { Keys.R };
            this["RaiseRF4Click"] = new SortedSet<Hotkey> { Keys.R | Keys.Shift };
            this["LowerRF1Click"] = new SortedSet<Hotkey> { Keys.F };
            this["LowerRF4Click"] = new SortedSet<Hotkey> { Keys.F | Keys.Shift };
            this["RaiseRF1ClickSmooth"] = new SortedSet<Hotkey> { Keys.R | Keys.Alt };
            this["RaiseRF4ClickSmooth"] = new SortedSet<Hotkey> { Keys.R | Keys.Alt | Keys.Shift };
            this["LowerRF1ClickSmooth"] = new SortedSet<Hotkey> { Keys.F | Keys.Alt };
            this["LowerRF4ClickSmooth"] = new SortedSet<Hotkey> { Keys.F | Keys.Alt | Keys.Shift };
            this["RaiseYH1Click"] = new SortedSet<Hotkey> { Y };
            this["RaiseYH4Click"] = new SortedSet<Hotkey> { Y | Keys.Shift };
            this["LowerYH1Click"] = new SortedSet<Hotkey> { Keys.H };
            this["LowerYH4Click"] = new SortedSet<Hotkey> { Keys.H | Keys.Shift };
            this["RaiseUJ1Click"] = new SortedSet<Hotkey> { Keys.U };
            this["RaiseUJ4Click"] = new SortedSet<Hotkey> { Keys.U | Keys.Shift };
            this["LowerUJ1Click"] = new SortedSet<Hotkey> { Keys.J };
            this["LowerUJ4Click"] = new SortedSet<Hotkey> { Keys.J | Keys.Shift };
            this["RotateObject5"] = new SortedSet<Hotkey> { Keys.R | Keys.Control };
            this["RotateObject45"] = new SortedSet<Hotkey> { Keys.R | Keys.Control | Keys.Shift };
            this["RotateTexture"] = new SortedSet<Hotkey> { Keys.OemMinus, Keys.Oemplus, Keys.Oem3, Keys.Oem5 }; // US keyboard key in documentation: OemMinus      US-keyboard key: Oem3       German-keyboard key: Oem5
            this["MirrorTexture"] = new SortedSet<Hotkey> { Keys.OemMinus | Keys.Shift, Keys.Oemplus | Keys.Shift, Keys.Oem3 | Keys.Shift, Keys.Oem5 | Keys.Shift };
            this["NewLevel"] = new SortedSet<Hotkey> { Keys.N | Keys.Control | Keys.Shift };
            this["OpenLevel"] = new SortedSet<Hotkey> { Keys.O | Keys.Control };
            this["SaveLevel"] = new SortedSet<Hotkey> { Keys.S | Keys.Control };
            this["SaveLevelAs"] = new SortedSet<Hotkey> { Keys.S | Keys.Control | Keys.Shift };
            this["BuildLevel"] = new SortedSet<Hotkey> { Keys.F5 | Keys.Shift };
            this["BuildAndPlay"] = new SortedSet<Hotkey> { Keys.F5 };
            this["Cut"] = new SortedSet<Hotkey> { Keys.X | Keys.Control };
            this["Copy"] = new SortedSet<Hotkey> { Keys.C | Keys.Control };
            this["Paste"] = new SortedSet<Hotkey> { Keys.V | Keys.Control };
            this["StampObject"] = new SortedSet<Hotkey> { Keys.B | Keys.Control };
            this["Delete"] = new SortedSet<Hotkey> { Keys.Delete };
            this["SelectAll"] = new SortedSet<Hotkey> { A | Keys.Control };
            this["Search"] = new SortedSet<Hotkey> { Keys.F | Keys.Control };
            this["SearchAndReplaceObjects"] = new SortedSet<Hotkey> { Keys.F | Keys.Control | Keys.Shift };
            this["DeleteRooms"] = new SortedSet<Hotkey> { Keys.D | Keys.Control | Keys.Shift | Keys.Alt };
            this["DuplicateRoom"] = new SortedSet<Hotkey> { Keys.U | Keys.Control | Keys.Shift | Keys.Alt };
            this["SelectConnectedRooms"] = new SortedSet<Hotkey> { Keys.C | Keys.Control | Keys.Shift | Keys.Alt };
            this["RotateRoomsClockwise"] = new SortedSet<Hotkey> { Keys.F1 | Keys.Control };
            this["RotateRoomsCounterClockwise"] = new SortedSet<Hotkey> { Keys.F2 | Keys.Control };
            this["MirrorRoomsX"] = new SortedSet<Hotkey> { Keys.F3 | Keys.Control };
            this["MirrorRoomsZ"] = new SortedSet<Hotkey> { Keys.F4 | Keys.Control };
            this["SplitRoom"] = new SortedSet<Hotkey> { Keys.S | Keys.Control | Keys.Shift | Keys.Alt };
            this["CropRoom"] = new SortedSet<Hotkey> { Keys.O | Keys.Control | Keys.Shift | Keys.Alt };
            this["NewRoomUp"] = new SortedSet<Hotkey> { Keys.U | Keys.Control | Keys.Shift };
            this["NewRoomDown"] = new SortedSet<Hotkey> { Keys.D | Keys.Control | Keys.Shift };
            this["AddCamera"] = new SortedSet<Hotkey> { Keys.C | Keys.Alt };
            this["AddFlybyCamera"] = new SortedSet<Hotkey> { Keys.M | Keys.Alt };
            this["AddSink"] = new SortedSet<Hotkey> { Keys.K | Keys.Alt };
            this["AddSoundSource"] = new SortedSet<Hotkey> { Keys.X | Keys.Alt };
            this["AddImportedGeometry"] = new SortedSet<Hotkey> { Keys.I | Keys.Alt };
            this["AddBoxVolumeInSelectedArea"] = new SortedSet<Hotkey> { Keys.V };
            this["AddBoxVolume"] = new SortedSet<Hotkey> { Keys.V | Keys.Alt };
            this["AddSphereVolume"] = new SortedSet<Hotkey> { Keys.V | Keys.Shift };
            this["MoveLara"] = new SortedSet<Hotkey> { Keys.M | Keys.Control };
            this["SplitSectorObjectOnSelection"] = new SortedSet<Hotkey> { Keys.O | Keys.Alt };
            this["TextureFloor"] = new SortedSet<Hotkey> { Keys.T | Keys.Control | Keys.Alt };
            this["TextureCeiling"] = new SortedSet<Hotkey> { Keys.V | Keys.Control | Keys.Alt };
            this["TextureWalls"] = new SortedSet<Hotkey> { W | Keys.Control | Keys.Alt };
            this["AverageFloor"] = new SortedSet<Hotkey> { Keys.I | Keys.Control | Keys.Alt };
            this["AverageCeiling"] = new SortedSet<Hotkey> { Keys.J | Keys.Control | Keys.Alt };
            this["GridWallsIn3"] = new SortedSet<Hotkey> { Keys.D3 | Keys.Control };
            this["GridWallsIn5"] = new SortedSet<Hotkey> { Keys.D5 | Keys.Control };
            this["QuitEditor"] = new SortedSet<Hotkey> { Keys.F4 | Keys.Alt };
            this["RemapTexture"] = new SortedSet<Hotkey> { Keys.R | Keys.Control | Keys.Alt };
            this["SmoothRandomFloorUp"] = new SortedSet<Hotkey> { Keys.A | Keys.Control | Keys.Alt };
            this["SmoothRandomFloorDown"] = new SortedSet<Hotkey> { Keys.B | Keys.Control | Keys.Alt };
            this["SmoothRandomCeilingUp"] = new SortedSet<Hotkey> { Keys.C | Keys.Control | Keys.Alt };
            this["SmoothRandomCeilingDown"] = new SortedSet<Hotkey> { Keys.D | Keys.Control | Keys.Alt };
            this["SharpRandomFloorUp"] = new SortedSet<Hotkey> { Keys.E | Keys.Control | Keys.Alt };
            this["SharpRandomFloorDown"] = new SortedSet<Hotkey> { Keys.F | Keys.Control | Keys.Alt };
            this["SharpRandomCeilingUp"] = new SortedSet<Hotkey> { Keys.G | Keys.Control | Keys.Alt };
            this["SharpRandomCeilingDown"] = new SortedSet<Hotkey> { Keys.H | Keys.Control | Keys.Alt };
            this["RelocateCamera"] = new SortedSet<Hotkey> { Keys.Alt | Z };
            this["Undo"] = new SortedSet<Hotkey> { Keys.Control | Z };
            this["Redo"] = new SortedSet<Hotkey> { Keys.Control | Y };
            this["ToggleFlyMode"] = new SortedSet<Hotkey> { Keys.Shift | Z };
            this["InPlaceSearchRooms"] = new SortedSet<Hotkey> { Keys.Shift | Keys.F1 };
            this["InPlaceSearchItems"] = new SortedSet<Hotkey> { Keys.Shift | Keys.F2 };
            this["InPlaceSearchImportedGeometry"] = new SortedSet<Hotkey> { Keys.Shift | Keys.F3 };
            this["InPlaceSearchTextures"] = new SortedSet<Hotkey> { Keys.Shift | Keys.F4 };
            this["ShowTexturePanel"] = new SortedSet<Hotkey> { Keys.Alt | Keys.T };
            this["IncreaseStepHeight"] = new SortedSet<Hotkey> { Keys.OemCloseBrackets };
            this["DecreaseStepHeight"] = new SortedSet<Hotkey> { Keys.OemOpenBrackets };
            this["HighlightSplit1"] = new SortedSet<Hotkey> { Keys.D1 };
            this["HighlightSplit2"] = new SortedSet<Hotkey> { Keys.D2 };
            this["HighlightSplit3"] = new SortedSet<Hotkey> { Keys.D3 };
            this["HighlightSplit4"] = new SortedSet<Hotkey> { Keys.D4 };
            this["HighlightSplit5"] = new SortedSet<Hotkey> { Keys.D5 };
            this["HighlightSplit6"] = new SortedSet<Hotkey> { Keys.D6 };
            this["HighlightSplit7"] = new SortedSet<Hotkey> { Keys.D7 };
            this["HighlightSplit8"] = new SortedSet<Hotkey> { Keys.D8 };
            this["HighlightSplit9"] = new SortedSet<Hotkey> { Keys.D9 };

            // Check for conflicts
            var hotkeyList = _list
                .SelectMany(command => command.Value.Select(hotkey => new KeyValuePair<Hotkey, string>(hotkey, command.Key)))
                .OrderBy(hotkeyCommand => hotkeyCommand.Key).ToList();
            string conflicts = "";
            for (int i = 1; i < hotkeyList.Count; ++i)
                if (hotkeyList[i].Key.Equals(hotkeyList[i - 1].Key))
                    conflicts += "\n'" + hotkeyList[i].Key + "': '" + hotkeyList[i].Value + "' and '" + hotkeyList[i - 1].Value + "'";
            if (!string.IsNullOrEmpty(conflicts)) // Use normal message box here because it can handle lot's of text. (Usually won't be shown anyway)
                MessageBox.Show("Please report this to the development team.\nThere are conflicts in the default hotkey set:" + conflicts, "Hotkey conflicts", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
