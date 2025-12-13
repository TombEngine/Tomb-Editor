using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Forms;
using DarkUI.Controls;
using DarkUI.Forms;
using TombEditor.Forms;
using TombEditor.ToolWindows;
using TombLib;
using TombLib.Controls;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using TombLib.Utils;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorEnums.Extensions;
using TombEditor.Controls;

namespace TombEditor
{
    public enum CommandType
    {
        General,
        File,
        Edit,
        Rooms,
        Geometry,
        Sectors,
        Objects,
        Textures,
        Lighting,
        View,
        Windows,
        Settings
    }

    public class CommandObj
    {
        public string Name { get; set; }
        public string FriendlyName { get; set; }
        public Action<CommandArgs> Execute { get; set; }
        public CommandType Type { get; set; }
    }

    public class CommandArgs
    {
        public Editor Editor;
        public IWin32Window Window;
        public Keys KeyData = Keys.None;
    }

    public static class CommandHandler
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static List<CommandObj> _commands = new List<CommandObj>();
        public static IEnumerable<CommandObj> Commands => _commands;

        public static CommandObj GetCommand(string name)
        {
            CommandObj command = _commands.FirstOrDefault(cmd => cmd.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (command == null)
                throw new KeyNotFoundException("Command with name '" + name + "' not found.");
            return command;
        }

        public static void ExecuteHotkey(CommandArgs args)
        {
            var hotkeyForCommands = args.Editor.Configuration.UI_Hotkeys.Where(set => set.Value.Contains(args.KeyData));
            foreach (var hotkeyForCommand in hotkeyForCommands)
                GetCommand(hotkeyForCommand.Key).Execute?.Invoke(args);
        }

        public static void AssignCommandsToControls(Editor editor, Control parent, ToolTip toolTip = null, bool onlyToolTips = false)
        {
            var controls = WinFormsUtils.AllSubControls(parent).Where(c => c is DarkButton || c is DarkCheckBox || c is DarkPanel).ToList();
            foreach (var control in controls)
            {
                if (!string.IsNullOrEmpty(control.Tag?.ToString()))
                {
                    var command = GetCommand(control.Tag.ToString());

                    if (command != null)
                    {
                        var hotkeyLabel = string.Join(", ", editor.Configuration.UI_Hotkeys[control.Tag.ToString()]);
                        var label = command.FriendlyName + (string.IsNullOrEmpty(hotkeyLabel) ? "" : " (" + hotkeyLabel + ")");

                        if(!onlyToolTips)
                            control.Click += (sender, e) => { command.Execute?.Invoke(new CommandArgs { Editor = editor, Window = parent.FindForm() }); };

                        if (toolTip != null && !string.IsNullOrEmpty(label))
                            toolTip.SetToolTip(control, label);
                    }
                }
            }
        }

        private static void GenericDirectionalControlCommand(CommandArgs args, SectorVerticalPart surface, int increment, bool smooth, bool oppositeDiagonal)
        {
            if (args.Editor.HighlightedSplit != 0)
            {
                if (surface.IsOnFloor())
                {
                    if (args.Editor.HighlightedSplit == 1)
                        surface = SectorVerticalPart.QA;
                    else
                        surface = SectorVerticalPartExtensions.GetExtraFloorSplit(args.Editor.HighlightedSplit - 2);
                }
                else if (surface.IsOnCeiling())
                {
                    if (args.Editor.HighlightedSplit == 1)
                        surface = SectorVerticalPart.WS;
                    else
                        surface = SectorVerticalPartExtensions.GetExtraCeilingSplit(args.Editor.HighlightedSplit - 2);
                }
            }

            if (args.Editor.LastSelection == LastSelectionType.Sector && args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
            {
                EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, surface, increment, smooth, oppositeDiagonal);
            }
            else if (args.Editor.LastSelection == LastSelectionType.SpatialObject && (surface == SectorVerticalPart.QA || surface == SectorVerticalPart.WS) && !oppositeDiagonal && !smooth)
            {
                if (args.Editor.SelectedObject is PositionBasedObjectInstance && surface == SectorVerticalPart.QA)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(0, increment, 0), new Vector3(), true);
                else if (args.Editor.SelectedObject is GhostBlockInstance)
                {
                    ((GhostBlockInstance)args.Editor.SelectedObject).Move(increment, surface == SectorVerticalPart.QA);
                    args.Editor.RoomSectorPropertiesChange(args.Editor.SelectedRoom);
                }
            }
        }

        private static void AddCommand(string commandName, string friendlyName, CommandType type, Action<CommandArgs> command)
        {
            if (_commands.Any(cmd => cmd.Name.Equals(commandName, StringComparison.InvariantCultureIgnoreCase)))
                throw new InvalidOperationException("You cannot add multiple commands with the same name.");

            command += delegate { logger.Info(commandName); };
            _commands.Add(new CommandObj() { Name = commandName, FriendlyName = friendlyName, Execute = command, Type = type });
        }

        static CommandHandler()
        {
            AddCommand("CancelAnyAction", "Cancel any action", CommandType.General, delegate (CommandArgs args)
            {
                if (args.Editor.Action != null)
                    args.Editor.Action = null;
                else
                {
                    args.Editor.SelectedSectors = SectorSelection.None;
                    args.Editor.SelectedObject = null;
                    args.Editor.SelectedRooms = new[] { args.Editor.SelectedRoom };
                    args.Editor.HighlightedSplit = 0;
                }
            });

            AddCommand("Switch2DMode", "Switch to 2D map", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.Mode = EditorMode.Map2D;
            });

            AddCommand("SwitchGeometryMode", "Switch to Geometry mode", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.Mode = EditorMode.Geometry;
            });

            AddCommand("SwitchFaceEditMode", "Switch to Face Edit mode", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.Mode = EditorMode.FaceEdit;
            });

            AddCommand("SwitchLightingMode", "Switch to Lighting mode", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.Mode = EditorMode.Lighting;
            });

            AddCommand("ResetCamera", "Reset camera position", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.ResetCamera();
            });

            AddCommand("AddTrigger", "Add trigger...", CommandType.Objects, delegate (CommandArgs args)
            {
                if (EditorActions.CheckForRoomAndSectorSelection(args.Window))
                {
                    if (Control.ModifierKeys.HasFlag(Keys.Shift) || args.Editor.SelectedObject == null ||
                        !(args.Editor.SelectedObject is PositionBasedObjectInstance))
                        EditorActions.AddTrigger(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Window, args.Editor.BookmarkedObject);
                    else
                        EditorActions.AddTrigger(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Window, args.Editor.SelectedObject);
                }
            });

            AddCommand("AddTriggerWithBookmark", "Add trigger with bookmarked object", CommandType.Objects, delegate (CommandArgs args)
            {
                if (EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    EditorActions.AddTrigger(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Window, args.Editor.BookmarkedObject);
            });

            AddCommand("AddPortal", "Add portal", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    try
                    {
                        EditorActions.AddPortal(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Window);
                    }
                    catch (Exception exc)
                    {
                        logger.Error(exc, "Unable to create portal");
                        args.Editor.SendMessage("Unable to create portal. \nException: " + exc.Message, PopupType.Error);
                    }
            });

            AddCommand("RenameObject", "Rename object...", CommandType.Objects, delegate (CommandArgs args)
            {
                EditorActions.RenameObject(args.Editor.SelectedObject, args.Window);
            });

            AddCommand("EditObject", "Edit object properties...", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject != null)
                    EditorActions.EditObject(args.Editor.SelectedObject, args.Window);
            });

            AddCommand("EditObjectColor", "Edit object color", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject.CanBeColored())
                    EditorActions.EditColor(args.Window, (IColorable)args.Editor.SelectedObject);
            });

            AddCommand("SwitchBlendMode", "Switch blending mode", CommandType.Textures, delegate (CommandArgs args)
            {
                var texture = args.Editor.SelectedTexture;
                if (texture.BlendMode == BlendMode.Normal)
                    texture.BlendMode = BlendMode.Additive;
                else if (texture.BlendMode == BlendMode.Additive)
                    texture.BlendMode = BlendMode.Subtract;
                else if (texture.BlendMode == BlendMode.Subtract)
                    texture.BlendMode = BlendMode.Exclude;
                else if (texture.BlendMode == BlendMode.Exclude)
                    texture.BlendMode = BlendMode.Screen;
                else if (texture.BlendMode == BlendMode.Screen)
                    texture.BlendMode = BlendMode.Lighten;
                else if (texture.BlendMode == BlendMode.Lighten)
                    texture.BlendMode = BlendMode.Normal;
                args.Editor.SelectedTexture = texture;
            });

            AddCommand("SetBlendModeNormal", "Set blending mode: Normal", CommandType.Textures, delegate (CommandArgs args)
            {
                var texture = args.Editor.SelectedTexture;
                texture.BlendMode = BlendMode.Normal;
                args.Editor.SelectedTexture = texture;
            });

            AddCommand("SetBlendModeAdd", "Set blending mode: Add", CommandType.Textures, delegate (CommandArgs args)
            {
                var texture = args.Editor.SelectedTexture;
                texture.BlendMode = BlendMode.Additive;
                args.Editor.SelectedTexture = texture;
            });

            AddCommand("SetBlendModeSubtract", "Set blending mode: Subtract", CommandType.Textures, delegate (CommandArgs args)
            {
                var texture = args.Editor.SelectedTexture;
                texture.BlendMode = BlendMode.Subtract;
                args.Editor.SelectedTexture = texture;
            });

            AddCommand("SetBlendModeExclude", "Set blending mode: Exclude", CommandType.Textures, delegate (CommandArgs args)
            {
                var texture = args.Editor.SelectedTexture;
                texture.BlendMode = BlendMode.Exclude;
                args.Editor.SelectedTexture = texture;
            });

            AddCommand("SetBlendModeScreen", "Set blending mode: Screen", CommandType.Textures, delegate (CommandArgs args)
            {
                var texture = args.Editor.SelectedTexture;
                texture.BlendMode = BlendMode.Screen;
                args.Editor.SelectedTexture = texture;
            });

            AddCommand("SetBlendModeLighten", "Set blending mode: Lighten", CommandType.Textures, delegate (CommandArgs args)
            {
                var texture = args.Editor.SelectedTexture;
                texture.BlendMode = BlendMode.Lighten;
                args.Editor.SelectedTexture = texture;
            });

            AddCommand("SetTextureDoubleSided", "Set double-sided attribute", CommandType.Textures, delegate (CommandArgs args)
            {
                var texture = args.Editor.SelectedTexture;
                texture.DoubleSided = !texture.DoubleSided;
                args.Editor.SelectedTexture = texture;
            });

            AddCommand("SetTextureInvisible", "Set invisibility attribute", CommandType.Textures, delegate (CommandArgs args)
            {
                args.Editor.SelectedTexture = TextureArea.Invisible;
            });

            AddCommand("ChangeTextureSelectionTileSize", "Change texture selection tile size", CommandType.Textures, delegate (CommandArgs args)
            {
                EditorActions.SetSelectionTileSize(args.Editor.Configuration.TextureMap_TileSelectionSize * 2);
            });

            AddCommand("RotateObjectLeft", "Rotate object left", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject != null)
                    EditorActions.RotateObject(args.Editor.SelectedObject, RotationAxis.Y, -1);
            });

            AddCommand("RotateObjectRight", "Rotate object right", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject != null)
                    EditorActions.RotateObject(args.Editor.SelectedObject, RotationAxis.Y, 1);
            });

            AddCommand("RotateObjectUp", "Rotate object up", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject != null)
                    EditorActions.RotateObject(args.Editor.SelectedObject, RotationAxis.X, 1);
            });

            AddCommand("RotateObjectDown", "Rotate object down", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject != null)
                    EditorActions.RotateObject(args.Editor.SelectedObject, RotationAxis.X, -1);
            });

            AddCommand("MoveObjectLeft", "Move object left (4 clicks)", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(-Level.SectorSizeUnit, 0, 0), new Vector3(), true);
            });

            AddCommand("MoveObjectRight", "Move object right (4 clicks)", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(Level.SectorSizeUnit, 0, 0), new Vector3(), true);
            });

            AddCommand("MoveObjectForward", "Move object forward (4 clicks)", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(0, 0, Level.SectorSizeUnit), new Vector3(), true);
            });

            AddCommand("MoveObjectBack", "Move object back (4 clicks)", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(0, 0, -Level.SectorSizeUnit), new Vector3(), true);
            });

            AddCommand("MoveObjectUp", "Move object up", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(0, Level.SectorSizeUnit, 0), new Vector3(), true);
            });

            AddCommand("MoveObjectDown", "Move object down", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(0, -Level.SectorSizeUnit, 0), new Vector3(), true);
            });

            AddCommand("SelectFloorBelowObject", "Select floor below current object", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.SelectFloorBelowObject(args.Editor.SelectedObject as PositionBasedObjectInstance);
                else
                    args.Editor.SendMessage("Please select an object.", PopupType.Error);
            });

            AddCommand("SelectPreviousRoom", "Select previous room", CommandType.Rooms, delegate (CommandArgs args)
            {
                var prevRoom = args.Editor.PreviousRoom;

                if (prevRoom != null)
                    args.Editor.SelectRoom(prevRoom);
                else
                    args.Editor.SendMessage("There is no previous room specified or previous room was deleted.", PopupType.Info);
            });

            AddCommand("MoveRoomLeft", "Move room left", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.MoveSelectedRooms(new VectorInt3(-1, 0, 0));
            });

            AddCommand("MoveRoomRight", "Move room right", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.MoveSelectedRooms(new VectorInt3(1, 0, 0));
            });

            AddCommand("MoveRoomForward", "Move room forward", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.MoveSelectedRooms(new VectorInt3(0, 0, 1));
            });

            AddCommand("MoveRoomBack", "Move room back", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.MoveSelectedRooms(new VectorInt3(0, 0, -1));
            });

            AddCommand("MoveRoomUp", "Move room up", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.MoveSelectedRooms(new VectorInt3(0, args.Editor.IncrementReference, 0));
            });

            AddCommand("MoveRoomDown", "Move room down", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.MoveSelectedRooms(new VectorInt3(0, -args.Editor.IncrementReference, 0));
            });

            AddCommand("MoveRoomUp4Clicks", "Move room up (4 clicks)", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.MoveSelectedRooms(new VectorInt3(0, args.Editor.IncrementReference * 4, 0));
            });

            AddCommand("MoveRoomDown4Clicks", "Move room down (4 clicks)", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.MoveSelectedRooms(new VectorInt3(0, -args.Editor.IncrementReference * 4, 0));
            });

            AddCommand("RaiseQA1Click", "Raise selected floor or item (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.QA, args.Editor.IncrementReference, false, false);
            });

            AddCommand("RaiseQA4Click", "Raise selected floor or item (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.QA, args.Editor.IncrementReference * 4, false, false);
            });

            AddCommand("LowerQA1Click", "Lower selected floor or item (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.QA, -args.Editor.IncrementReference, false, false);
            });

            AddCommand("LowerQA4Click", "Lower selected floor or item (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.QA, -args.Editor.IncrementReference * 4, false, false);
            });

            AddCommand("RaiseWS1Click", "Raise selected ceiling (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.WS, args.Editor.IncrementReference, false, false);
            });

            AddCommand("RaiseWS4Click", "Raise selected ceiling (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.WS, args.Editor.IncrementReference * 4, false, false);
            });

            AddCommand("LowerWS1Click", "Lower selected ceiling (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.WS, -args.Editor.IncrementReference, false, false);
            });

            AddCommand("LowerWS4Click", "Lower selected ceiling (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.WS, -args.Editor.IncrementReference * 4, false, false);
            });

            AddCommand("RaiseED1Click", "Raise selected floor split (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.Floor2, args.Editor.IncrementReference, false, false);
            });

            AddCommand("RaiseED4Click", "Raise selected floor split (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.Floor2, args.Editor.IncrementReference * 4, false, false);
            });

            AddCommand("LowerED1Click", "Lower selected floor split (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.Floor2, -args.Editor.IncrementReference, false, false);
            });

            AddCommand("LowerED4Click", "Lower selected floor split (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.Floor2, -args.Editor.IncrementReference * 4, false, false);
            });

            AddCommand("RaiseRF1Click", "Raise selected ceiling split (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.Ceiling2, args.Editor.IncrementReference, false, false);
            });

            AddCommand("RaiseRF4Click", "Raise selected ceiling split (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.Ceiling2, args.Editor.IncrementReference * 4, false, false);
            });

            AddCommand("LowerRF1Click", "Lower selected ceiling split (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.Ceiling2, -args.Editor.IncrementReference, false, false);
            });

            AddCommand("LowerRF4Click", "Lower selected ceiling split (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.Ceiling2, -args.Editor.IncrementReference * 4, false, false);
            });

            AddCommand("RaiseQA1ClickSmooth", "Smoothly raise selected floor (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.QA, args.Editor.IncrementReference, true, false);
            });

            AddCommand("RaiseQA4ClickSmooth", "Smoothly raise selected floor (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.QA, args.Editor.IncrementReference * 4, true, false);
            });

            AddCommand("LowerQA1ClickSmooth", "Smoothly lower selected floor (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.QA, -args.Editor.IncrementReference, true, false);
            });

            AddCommand("LowerQA4ClickSmooth", "Smoothly lower selected floor (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.QA, -args.Editor.IncrementReference * 4, true, false);
            });

            AddCommand("RaiseWS1ClickSmooth", "Smoothly raise selected ceiling (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.WS, args.Editor.IncrementReference, true, false);
            });

            AddCommand("RaiseWS4ClickSmooth", "Smoothly raise selected ceiling (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.WS, args.Editor.IncrementReference * 4, true, false);
            });

            AddCommand("LowerWS1ClickSmooth", "Smoothly lower selected ceiling (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.WS, -args.Editor.IncrementReference, true, false);
            });

            AddCommand("LowerWS4ClickSmooth", "Smoothly lower selected ceiling (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.WS, -args.Editor.IncrementReference * 4, true, false);
            });

            AddCommand("RaiseED1ClickSmooth", "Smoothly raise selected floor split (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.Floor2, args.Editor.IncrementReference, true, false);
            });

            AddCommand("RaiseED4ClickSmooth", "Smoothly raise selected floor split (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.Floor2, args.Editor.IncrementReference * 4, true, false);
            });

            AddCommand("LowerED1ClickSmooth", "Smoothly lower selected floor split (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.Floor2, -args.Editor.IncrementReference, true, false);
            });

            AddCommand("LowerED4ClickSmooth", "Smoothly lower selected floor split (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.Floor2, -args.Editor.IncrementReference * 4, true, false);
            });

            AddCommand("RaiseRF1ClickSmooth", "Smoothly raise selected ceiling split (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.Ceiling2, args.Editor.IncrementReference, true, false);
            });

            AddCommand("RaiseRF4ClickSmooth", "Smoothly raise selected ceiling split (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.Ceiling2, args.Editor.IncrementReference * 4, true, false);
            });

            AddCommand("LowerRF1ClickSmooth", "Smoothly lower selected ceiling split (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.Ceiling2, -args.Editor.IncrementReference, true, false);
            });

            AddCommand("LowerRF4ClickSmooth", "Smoothly lower selected ceiling split (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.Ceiling2, -args.Editor.IncrementReference * 4, true, false);
            });

            AddCommand("RaiseYH1Click", "Raise selected floor diagonal step (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.QA, args.Editor.IncrementReference, false, true);
            });

            AddCommand("RaiseYH4Click", "Raise selected floor diagonal step (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.QA, args.Editor.IncrementReference * 4, false, true);
            });

            AddCommand("LowerYH1Click", "Lower selected floor diagonal step (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.QA, -args.Editor.IncrementReference, false, true);
            });

            AddCommand("LowerYH4Click", "Lower selected floor diagonal step (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.QA, -args.Editor.IncrementReference * 4, false, true);
            });

            AddCommand("RaiseUJ1Click", "Raise selected ceiling diagonal step (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.WS, args.Editor.IncrementReference, false, true);
            });

            AddCommand("RaiseUJ4Click", "Raise selected ceiling diagonal step (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.WS, args.Editor.IncrementReference * 4, false, true);
            });

            AddCommand("LowerUJ1Click", "Lower selected ceiling diagonal step (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.WS, -args.Editor.IncrementReference, false, true);
            });

            AddCommand("LowerUJ4Click", "Lower selected ceiling diagonal step (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, SectorVerticalPart.WS, -args.Editor.IncrementReference * 4, false, true);
            });

            AddCommand("RotateObject5", "Rotate object (5 degrees)", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject != null)
                    EditorActions.RotateObject(args.Editor.SelectedObject, RotationAxis.Y, 5.0f);
            });

            AddCommand("RotateObject45", "Rotate object (45 degrees)", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject != null)
                    EditorActions.RotateObject(args.Editor.SelectedObject, RotationAxis.Y, 45.0f);
            });

            AddCommand("MoveObjectToCurrentRoom", "Move object to current room", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectToOtherRoom((PositionBasedObjectInstance)args.Editor.SelectedObject, args.Editor.SelectedRoom);
            });

            AddCommand("RelocateCamera", "Relocate camera", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionRelocateCamera();
            });

            AddCommand("RotateTexture", "Rotate selected texture", CommandType.Textures, delegate (CommandArgs args)
            {
                TextureArea newTexture = args.Editor.SelectedTexture;
                newTexture.Rotate(1, newTexture.TextureIsTriangle);
                args.Editor.SelectedTexture = newTexture;
            });

            AddCommand("MirrorTexture", "Mirror selected texture", CommandType.Textures, delegate (CommandArgs args)
            {
                TextureArea newTexture = args.Editor.SelectedTexture;
                Swap.Do(ref newTexture.TexCoord0, ref newTexture.TexCoord3);
                Swap.Do(ref newTexture.TexCoord1, ref newTexture.TexCoord2);
                args.Editor.SelectedTexture = newTexture;
            });

            AddCommand("NewLevel", "New level", CommandType.File, delegate (CommandArgs args)
            {
                if (!EditorActions.ContinueOnFileDrop(args.Window, "New level"))
                    return;

                args.Editor.Level = Level.CreateSimpleLevel(args.Editor.Configuration.Editor_DefaultProjectGameVersion);
                GC.Collect(); // Clean up memory

                // Make border wall grids, as in dxtre3d
                if (args.Editor.Configuration.Editor_GridNewRoom)
                    EditorActions.GridWallsSquares(args.Editor.Level.Rooms[0], args.Editor.Level.Rooms[0].LocalArea, false, false);
            });

            AddCommand("OpenLevel", "Open existing level...", CommandType.File, delegate (CommandArgs args)
            {
                EditorActions.OpenLevel(args.Window);
            });

            AddCommand("SaveLevel", "Save level", CommandType.File, delegate (CommandArgs args)
            {
                EditorActions.SaveLevel(args.Window, false);
            });

            AddCommand("SaveLevelAs", "Save level as...", CommandType.File, delegate (CommandArgs args)
            {
                EditorActions.SaveLevel(args.Window, true);
            });

            AddCommand("ImportPrj", "Import TRLE level...", CommandType.File, delegate (CommandArgs args)
            {
                EditorActions.OpenLevelPrj(args.Window);
            });

            AddCommand("ConvertLevelToTombEngine", "Convert level to TombEngine...", CommandType.File, delegate (CommandArgs args)
            {
                EditorActions.ConvertLevelToTombEngine(args.Window);
            });

            AddCommand("BuildLevel", "Build level", CommandType.File, delegate (CommandArgs args)
            {
                EditorActions.BuildLevel(false, args.Window);
            });

            AddCommand("BuildAndPlay", "Build level and play", CommandType.File, delegate (CommandArgs args)
            {
                EditorActions.BuildLevelAndPlay(args.Window);
            });

            AddCommand("BuildAndPlayPreview", "Build level and play in preview mode", CommandType.File, delegate (CommandArgs args)
            {
                EditorActions.BuildLevelAndPlay(args.Window, true);
            });

            AddCommand("Cut", "Cut", CommandType.Edit, delegate (CommandArgs args)
            {
                GetCommand("Copy").Execute.Invoke(args);

                if (args.Editor.Mode == EditorMode.Map2D)
                {
                    if (args.Editor.SelectedObject == null)
                        EditorActions.DeleteRooms(args.Editor.SelectedRooms);
                }
                else
                {
                    if (args.Editor.SelectedObject == null && args.Editor.SelectedSectors.Valid)
                    {
                        EditorActions.FlattenRoomArea(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, null, false, true, false);
                        EditorActions.SetSurfaceWithoutUpdate(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, false);
                        EditorActions.SetSurfaceWithoutUpdate(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, true);
                        EditorActions.FlattenRoomArea(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, null, true, true, true, true);
                    }
                    else if (args.Editor.SelectedObject != null && args.Editor.SelectedObject is ICopyable)
                        EditorActions.DeleteObject(args.Editor.SelectedObject);
                }
            });

            AddCommand("Copy", "Copy", CommandType.Edit, delegate (CommandArgs args)
            {
                if (args.Editor.Mode != EditorMode.Map2D)
                {
                    if (args.Editor.SelectedObject != null)
                        EditorActions.TryCopyObject(args.Editor.SelectedObject, args.Window);
                    else if (args.Editor.SelectedSectors.Valid)
                        EditorActions.TryCopySectors(args.Editor.SelectedSectors, args.Window);
                }
                else
                    Clipboard.SetDataObject(new RoomClipboardData(args.Editor), true);
            });

            AddCommand("Paste", "Paste", CommandType.Edit, delegate (CommandArgs args)
            {
                if (args.Editor.Mode != EditorMode.Map2D)
                {
                    if (Clipboard.ContainsData(typeof(ObjectClipboardData).FullName))
                    {
                        var data = Clipboard.GetDataObject().GetData(typeof(ObjectClipboardData)) as ObjectClipboardData;
                        if (data == null)
                            args.Editor.SendMessage("Clipboard contains no object data.", PopupType.Error);
                        else
                        {
                            var obj = data.MergeGetSingleObject(args.Editor);
                            if (obj is TriggerInstance)
                            {
                                if (args.Editor.SelectedSectors == SectorSelection.None)
                                    args.Editor.SendMessage("Select sectors to paste trigger.", PopupType.Error);
                                else
                                    EditorActions.AddTrigger(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, (TriggerInstance)obj);
                            }
                            else if (obj is ISpatial)
                                args.Editor.Action = new EditorActionPlace(false, (level, room) => data.MergeGetSingleObject(args.Editor));
                            else
                            {
                                args.Editor.SendMessage("Clipboard level data is invalid. Nothing was pasted.", PopupType.Warning);
                                Clipboard.Clear();
                            }
                        }
                    }
                    else if (args.Editor.SelectedSectors.Valid && Clipboard.ContainsData(typeof(SectorsClipboardData).FullName))
                    {
                        var data = Clipboard.GetDataObject().GetData(typeof(SectorsClipboardData)) as SectorsClipboardData;
                        if (data == null)
                            args.Editor.SendMessage("Clipboard contains no sector data.", PopupType.Error);
                        else
                            EditorActions.TryPasteSectors(data, args.Window);
                    }
                }
                else
                {
                    var roomClipboardData = Clipboard.GetDataObject().GetData(typeof(RoomClipboardData)) as RoomClipboardData;
                    if (roomClipboardData == null)
                        args.Editor.SendMessage("Clipboard contains no room data.", PopupType.Error);
                    else
                        roomClipboardData.MergeInto(args.Editor, new VectorInt2());
                }
            });

            AddCommand("StampObject", "Stamp object", CommandType.Objects, delegate (CommandArgs args)
            {
                EditorActions.TryStampObject(args.Editor.SelectedObject, args.Window);
            });

            AddCommand("Delete", "Delete", CommandType.Edit, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject != null)
                {
                    if (args.Editor.Mode != EditorMode.Map2D || (args.Editor.SelectedObject is PortalInstance || args.Editor.SelectedObject is TriggerInstance))
                    {
                        EditorActions.DeleteObject(args.Editor.SelectedObject, args.Window);
                        return;
                    }
                }

                EditorActions.DeleteRooms(args.Editor.SelectedRooms, args.Window);
            });

            AddCommand("DeleteMissingObjects", "Delete missing objects", CommandType.Edit, delegate (CommandArgs args)
            {
                if (DarkMessageBox.Show(args.Window, "Do you want to delete all missing objects in all rooms?\nThis action can't be undone and will also remove associated triggers.",
                                       "Delete all missing objects", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    foreach (var room in args.Editor.Level.ExistingRooms)
                    {
                        var objects = room.Objects.Where(obj => obj is PositionBasedObjectInstance &&
                                      ((obj is ImportedGeometryInstance && ((ImportedGeometryInstance)obj).Model == null) ||
                                       (obj is MoveableInstance && args.Editor.Level.Settings.WadTryGetMoveable(((MoveableInstance)obj).WadObjectId) == null) ||
                                       (obj is StaticInstance && args.Editor.Level.Settings.WadTryGetStatic(((StaticInstance)obj).WadObjectId) == null))).ToList();

                        if (objects.Count > 0)
                            for (int i = objects.Count - 1; i >= 0; i--)
                            {
                                var obj = objects[i];
                                EditorActions.DeleteObjectWithoutUpdate(obj);
                                objects.RemoveAt(i);
                            }
                    }
                }
            });

            AddCommand("DeleteAllObjects", "Delete objects in selected rooms", CommandType.Edit, delegate (CommandArgs args)
            {
                if (DarkMessageBox.Show(args.Window, "Do you want to delete all objects in selected rooms? This action can't be undone.",
                                       "Delete all objects", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {

                    foreach (var room in args.Editor.SelectedRooms)
                    {
                        var objects = room.Objects.Where(ob => ob is PositionBasedObjectInstance && !(ob is LightInstance)).ToList();
                        if (objects.Count > 0)
                            for (int i = objects.Count - 1; i >= 0; i--)
                            {
                                var obj = objects[i];
                                EditorActions.DeleteObjectWithoutUpdate(obj);
                                objects.RemoveAt(i);
                            }
                    }
                }
            });

            AddCommand("DeleteAllTriggers", "Delete triggers in selected rooms", CommandType.Edit, delegate (CommandArgs args)
            {
                if (DarkMessageBox.Show(args.Window, "Do you want to delete all triggers in selected rooms? This action can't be undone.",
                                       "Delete all triggers", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    foreach (var room in args.Editor.SelectedRooms)
                    {
                        var triggers = room.Triggers.ToList();
                        if (triggers.Count > 0)
                            for (int i = triggers.Count - 1; i >= 0; i--)
                            {
                                var obj = triggers[i];
                                EditorActions.DeleteObjectWithoutUpdate(obj);
                                triggers.RemoveAt(i);
                            }
                    }
                }
            });

            AddCommand("SelectAll", "Select all", CommandType.Edit, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Map2D)
                    args.Editor.SelectRooms(args.Editor.Level.ExistingRooms, true);
                else
                    args.Editor.SelectedSectors = new SectorSelection { Area = args.Editor.SelectedRoom.LocalArea };
            });

            AddCommand("BookmarkObject", "Bookmark object", CommandType.Objects, delegate (CommandArgs args)
            {
                EditorActions.BookmarkObject(args.Editor.SelectedObject);
            });

            AddCommand("SelectBookmarkedObject", "Select bookmarked object", CommandType.Objects, delegate (CommandArgs args)
            {
                args.Editor.SelectedObject = args.Editor.BookmarkedObject;
            });

            AddCommand("Search", "Search...", CommandType.Edit, delegate (CommandArgs args)
            {
                var existingWindow = Application.OpenForms[nameof(FormSearch)];
                if (existingWindow == null)
                {
                    var searchForm = new FormSearch(args.Editor);
                    searchForm.Show(args.Window);
                }
                else
                    existingWindow.Focus();
            });

            AddCommand("EditVolumeEventSets", "Edit volume event sets...", CommandType.Edit, delegate (CommandArgs args)
            {
                EditorActions.EditEventSets(args.Window, false);
            });

            AddCommand("EditGlobalEventSets", "Edit global event sets...", CommandType.Edit, delegate (CommandArgs args)
            {
                EditorActions.EditEventSets(args.Window, true);
            });

            AddCommand("SearchAndReplaceObjects", "Search and replace objects...", CommandType.Edit, delegate (CommandArgs args)
            {
                EditorActions.ReplaceObject(args.Window);
            });

            AddCommand("AddNewRoom", "Add new room", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.Level.Rooms == null)
                    return;

                for (int i = 0; i < Level.MaxNumberOfRooms; i++)
                {
                    if (args.Editor.Level.Rooms[i] != null)
                        continue;
                    else
                    {
                        EditorActions.MakeNewRoom(i);
                        return;
                    }
                }

                args.Editor.SendMessage("Maximum amount of rooms reached. Can't create new room.", PopupType.Error);
            });

            AddCommand("DeleteRooms", "Delete rooms", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Map2D)
                    EditorActions.DeleteRooms(args.Editor.SelectedRooms, args.Window);
                else
                    EditorActions.DeleteRooms(new[] { args.Editor.SelectedRoom }, args.Window);
            });

            AddCommand("DuplicateRoom", "Duplicate rooms", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.DuplicateRoom(args.Window);
            });

            AddCommand("Redo", "Redo", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.UndoManager.Redo();
            });

            AddCommand("Undo", "Undo", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.UndoManager.Undo();
            });

            AddCommand("SelectConnectedRooms", "Select connected rooms", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.SelectConnectedRooms();
            });

            AddCommand("RotateRoomsClockwise", "Rotate rooms clockwise", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.TransformRooms(new RectTransformation { QuadrantRotation = -1 }, args.Window);
            });

            AddCommand("RotateRoomsCounterClockwise", "Rotate rooms counterclockwise", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.TransformRooms(new RectTransformation { QuadrantRotation = 1 }, args.Window);
            });

            AddCommand("MirrorRoomsX", "Mirror rooms on X axis", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.TransformRooms(new RectTransformation { MirrorX = true }, args.Window);
            });

            AddCommand("MirrorRoomsZ", "Mirror rooms on Z axis", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.TransformRooms(new RectTransformation { MirrorX = true, QuadrantRotation = 2 }, args.Window);
            });

            AddCommand("LockRoom", "Lock room position", CommandType.Rooms, delegate (CommandArgs args)
            {
                args.Editor.SelectedRoom.Properties.Locked = !args.Editor.SelectedRoom.Properties.Locked;
                args.Editor.RoomPropertiesChange(args.Editor.SelectedRoom);
            });

            AddCommand("HideRoom", "Hide room", CommandType.Rooms, delegate (CommandArgs args)
            {
                args.Editor.SelectedRoom.Properties.Hidden = !args.Editor.SelectedRoom.Properties.Hidden;
                args.Editor.RoomPropertiesChange(args.Editor.SelectedRoom);
            });

            AddCommand("SplitRoom", "Split room", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.SplitRoom(args.Window);
            });

            AddCommand("CropRoom", "Crop room...", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.CropRoom(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Window);
            });

            AddCommand("NewRoomUp", "New room up", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.CreateAdjoiningRoom(args.Editor.SelectedRoom, args.Editor.SelectedSectors, PortalDirection.Ceiling, true, Room.DefaultHeight);
            });

            AddCommand("NewRoomDown", "New room down", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.CreateAdjoiningRoom(args.Editor.SelectedRoom, args.Editor.SelectedSectors, PortalDirection.Floor, true, Room.DefaultHeight);
            });

            AddCommand("NewRoomLeft", "New room left", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.CreateAdjoiningRoom(args.Editor.SelectedRoom, args.Editor.SelectedSectors, PortalDirection.WallNegativeX, true, 12);
            });

            AddCommand("NewRoomRight", "New room right", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.CreateAdjoiningRoom(args.Editor.SelectedRoom, args.Editor.SelectedSectors, PortalDirection.WallPositiveX, true, 12);
            });

            AddCommand("NewRoomFront", "New room front", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.CreateAdjoiningRoom(args.Editor.SelectedRoom, args.Editor.SelectedSectors, PortalDirection.WallPositiveZ, true, 12);
            });

            AddCommand("NewRoomBack", "New room back", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.CreateAdjoiningRoom(args.Editor.SelectedRoom, args.Editor.SelectedSectors, PortalDirection.WallNegativeZ, true, 12);
            });

            AddCommand("MergeRoomsHorizontally", "Merge rooms horizontally", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.MergeRoomsHorizontally(args.Editor.SelectedRooms, args.Window);
            });

            AddCommand("ExportRooms", "Export rooms...", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.ExportRooms(args.Editor.SelectedRooms, args.Window);
            });

            AddCommand("ImportRooms", "Import rooms...", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.ImportRooms(args.Window);
            });


            AddCommand("EditAmbientLight", "Edit ambient light...", CommandType.Rooms, delegate (CommandArgs args)
            {
                var room = args.Editor.SelectedRoom;
                var undo = new RoomPropertyUndoInstance(args.Editor.UndoManager, room);

                using (var colorDialog = new RealtimeColorDialog(
                    args.Editor.Configuration.ColorDialog_Position.X,
                    args.Editor.Configuration.ColorDialog_Position.Y,
                    c =>
                    {
                        room.Properties.AmbientLight = c.ToFloat3Color() * 2.0f;
                        args.Editor.SelectedRoom.RebuildLighting(args.Editor.Configuration.Rendering3D_HighQualityLightPreview);
                        args.Editor.RoomPropertiesChange(room);
                    }, args.Editor.Configuration.UI_ColorScheme))
                {
                    colorDialog.Color = (room.Properties.AmbientLight * 0.5f).ToWinFormsColor();
                    var oldLightColor = colorDialog.Color;

                    if (colorDialog.ShowDialog(args.Window) != DialogResult.OK)
                        colorDialog.Color = oldLightColor;

                    var newColor = colorDialog.Color.ToFloat3Color() * 2.0f;
                    if (args.Editor.Level.Settings.GameVersion.Native() < TRVersion.Game.TR3)
                    {
                        if (!colorDialog.Color.IsGrayscale())
                            args.Editor.SendMessage("Only grayscale lighting is possible for this game version.", PopupType.Info);
                        newColor = Vector3.Clamp(new Vector3(newColor.GetLuma()), Vector3.Zero, Vector3.One);
                    }
                    room.Properties.AmbientLight = newColor;

                    args.Editor.Configuration.ColorDialog_Position = colorDialog.Position;
                }

                args.Editor.UndoManager.Push(undo);
                args.Editor.SelectedRoom.RebuildLighting(args.Editor.Configuration.Rendering3D_HighQualityLightPreview);
                args.Editor.RoomPropertiesChange(room);
            });

            AddCommand("ApplyRoomProperties", "Apply room properties...", CommandType.Rooms, delegate (CommandArgs args)
            {
                var existingWindow = Application.OpenForms[nameof(FormRoomProperties)];
                if (existingWindow == null)
                {
                    var propForm = new FormRoomProperties(args.Editor);
                    propForm.Show(args.Window);
                }
                else
                    existingWindow.Focus();
            });

            AddCommand("AddWad", "Add wad...", CommandType.Objects, delegate (CommandArgs args)
            {
                EditorActions.AddWad(args.Window);
            });

            AddCommand("RemoveWads", "Remove all wads", CommandType.Objects, delegate (CommandArgs args)
            {
                EditorActions.RemoveWads(args.Window);
            });

            AddCommand("ReloadWads", "Reload all wads", CommandType.Objects, delegate (CommandArgs args)
            {
                EditorActions.ReloadWads(args.Window);
            });

            AddCommand("ReloadSounds", "Reload all sound catalogs", CommandType.Objects, delegate (CommandArgs args)
            {
                EditorActions.ReloadSounds(args.Window);
            });

            AddCommand("AddCamera", "Add camera", CommandType.Objects, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new CameraInstance());
            });

            AddCommand("AddSprite", "Add room sprite", CommandType.Objects, delegate (CommandArgs args)
            {
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion.Native() <= TRVersion.Game.TR2, "Room sprite"))
                    return;

                args.Editor.Action = new EditorActionPlace(false, (l, r) => new SpriteInstance());
            });

            AddCommand("AddFlybyCamera", "Add flyby camera", CommandType.Objects, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new FlybyCameraInstance(args.Editor.SelectedObject));
            });

            AddCommand("AddSink", "Add sink", CommandType.Objects, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new SinkInstance());
            });

            AddCommand("AddMemo", "Add memo", CommandType.Objects, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new MemoInstance());
            });

            AddCommand("AddGhostBlock", "Add ghost block", CommandType.Objects, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new GhostBlockInstance());
            });

            AddCommand("AddSoundSource", "Add sound source", CommandType.Objects, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new SoundSourceInstance());
            });

            AddCommand("AddImportedGeometry", "Add imported geometry", CommandType.Objects, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new ImportedGeometryInstance() { Model = args.Editor.ChosenImportedGeometry });
            });

            AddCommand("AddBoxVolumeInSelectedArea", "Add box volume in selected area", CommandType.Objects, delegate (CommandArgs args)
            {
                if (!EditorActions.VersionCheck(args.Editor.Level.IsTombEngine, "Volume"))
                    return;
                EditorActions.AddBoxVolumeInSelectedArea(args.Window);
            });

            AddCommand("AddBoxVolume", "Add box volume", CommandType.Objects, delegate (CommandArgs args)
            {
                if (!EditorActions.VersionCheck(args.Editor.Level.IsTombEngine, "Volume"))
                    return;
                EditorActions.AddVolume(VolumeShape.Box);
            });

            AddCommand("AddSphereVolume", "Add sphere volume", CommandType.Objects, delegate (CommandArgs args)
            {
                if (!EditorActions.VersionCheck(args.Editor.Level.IsTombEngine, "Volume"))
                    return;
                EditorActions.AddVolume(VolumeShape.Sphere);
            });

            AddCommand("AddItem", "Add item", CommandType.Objects, delegate (CommandArgs args)
            {
                var currentItem = EditorActions.GetCurrentItemWithMessage();
                if (currentItem == null)
                    return;

                if (!currentItem.Value.IsStatic && args.Editor.SelectedRoom.Alternated && args.Editor.SelectedRoom.AlternateRoom == null)
                {
                    args.Editor.SendMessage("You can't add moveables to a flipped room.", PopupType.Info);
                    return;
                }

                args.Editor.Action = new EditorActionPlace(false, (r, l) => ItemInstance.FromItemType(currentItem.Value));
            });

            AddCommand("LocateItem", "Locate item", CommandType.Objects, delegate (CommandArgs args)
            {
                EditorActions.FindItem();
            });

            AddCommand("MoveLara", "Move Lara here", CommandType.Objects, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;

                EditorActions.MoveLara(args.Window, args.Editor.SelectedRoom, args.Editor.SelectedSectors.Start);
            });

            AddCommand("AssignAndClipboardScriptId", "Assign and copy script ID to clipboard", CommandType.Objects, delegate (CommandArgs args)
            {
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion >= TRVersion.Game.TRNG, "This feature"))
                    return;

                if (args.Editor.SelectedObject == null)
                {
                    args.Editor.SendMessage("Select an object first.", PopupType.Warning);
                    return;
                }

                var selectedObj = args.Editor.SelectedObject as IHasScriptID;
                if (selectedObj == null)
                {
                    args.Editor.SendMessage("The selected object does not have a script ID.", PopupType.Warning);
                    return;
                }
                if (selectedObj.ScriptId == null)
                    selectedObj.AllocateNewScriptId();
                Clipboard.SetText(selectedObj.ScriptId.Value.ToString());
                args.Editor.SendMessage("Script ID for selected object is " + selectedObj.ScriptId + ".\nCopied to clipboard.", PopupType.Info);
                args.Editor.ObjectChange(args.Editor.SelectedObject, ObjectChangeType.Change);
            });

            AddCommand("GenerateObjectNames", "Generate Lua names for unnamed objects", CommandType.Objects, delegate (CommandArgs args)
            {
                if (!EditorActions.VersionCheck(args.Editor.Level.IsTombEngine, "Object naming"))
                    return;

                int count = 0;

                foreach (var obj in args.Editor.Level.GetAllObjects().OfType<PositionAndScriptBasedObjectInstance>())
                    if (string.IsNullOrEmpty(obj.LuaName))
                    {
                        obj.AllocateNewLuaName();
                        count++;
                    }

                if (args.Editor.SelectedObject != null)
                    args.Editor.ObjectChange(args.Editor.SelectedObject, ObjectChangeType.Change);

                if (count > 0)
                    args.Editor.SendMessage("Generated Lua names for " + count + " unnamed objects in level.", PopupType.Info);
                else
                    args.Editor.SendMessage("No unnamed objects were found in level. No names were generated.", PopupType.Info);
            });

            AddCommand("SplitSectorObjectOnSelection", "Split sector based object on selection", CommandType.Objects, delegate (CommandArgs args)
            {
                EditorActions.SplitSectorObjectOnSelection(args.Editor.SelectedObject as SectorBasedObjectInstance);
            });

            AddCommand("AddTexture", "Add texture...", CommandType.Textures, delegate (CommandArgs args)
            {
                EditorActions.AddTexture(args.Window);
            });

            AddCommand("RemoveTextures", "Remove all textures", CommandType.Textures, delegate (CommandArgs args)
            {
                EditorActions.RemoveTextures(args.Window);
            });

            AddCommand("UnloadTextures", "Unload all textures", CommandType.Textures, delegate (CommandArgs args)
            {
                EditorActions.UnloadTextures(args.Window);
            });

            AddCommand("ReloadTextures", "Reload all textures", CommandType.Textures, delegate (CommandArgs args)
            {
                foreach (var texture in args.Editor.Level.Settings.Textures)
                    texture.Reload(args.Editor.Level.Settings);
                args.Editor.LoadedTexturesChange();
            });

            AddCommand("ConvertTexturesToPNG", "Convert all textures to PNG", CommandType.Textures, delegate (CommandArgs args)
            {
                if (args.Editor.Level == null || args.Editor.Level.Settings.Textures.Count == 0)
                {
                    args.Editor.SendMessage("No texture loaded. Nothing to convert.", PopupType.Error);
                    return;
                }

                foreach (LevelTexture texture in args.Editor.Level.Settings.Textures)
                {
                    if (texture.LoadException != null)
                    {
                        args.Editor.SendMessage("The texture that should be converted to *.png could not be loaded. " + texture.LoadException?.Message, PopupType.Error);
                        return;
                    }

                    string currentTexturePath = args.Editor.Level.Settings.MakeAbsolute(texture.Path);
                    string pngFilePath = Path.Combine(Path.GetDirectoryName(currentTexturePath), Path.GetFileNameWithoutExtension(currentTexturePath) + ".png");

                    if (File.Exists(pngFilePath))
                    {
                        if (DarkMessageBox.Show(args.Window,
                                "There is already a file at \"" + pngFilePath + "\". Continue and overwrite the file?",
                                "File already exists", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                            return;
                    }

                    texture.Image.SaveToFile(pngFilePath);

                    args.Editor.SendMessage("TGA texture map was converted to PNG without errors and saved at \"" + pngFilePath + "\".", PopupType.Info);
                    texture.SetPath(args.Editor.Level.Settings, pngFilePath);
                }
                args.Editor.LoadedTexturesChange();
            });

            AddCommand("RemapTexture", "Remap texture...", CommandType.Textures, delegate (CommandArgs args)
            {
                using (var form = new FormTextureRemap(args.Editor))
                    form.ShowDialog(args.Window);
            });

            AddCommand("SearchTextures", "Search textures...", CommandType.Textures, delegate (CommandArgs args)
            {
                var existingWindow = Application.OpenForms[nameof(FormFindTextures)];
                if (existingWindow == null)
                {
                    var findUntexturedForm = new FormFindTextures(args.Editor);
                    findUntexturedForm.Show(args.Window);
                }
                else
                    existingWindow.Focus();
            });

            AddCommand("TextureFloor", "Texture floor", CommandType.Textures, delegate (CommandArgs args)
            {
                EditorActions.TexturizeAll(args.Editor.SelectedRoom, args.Editor.SelectedSectors, args.Editor.SelectedTexture, SectorFaceType.Floor);
            });

            AddCommand("TextureCeiling", "Texture ceiling", CommandType.Textures, delegate (CommandArgs args)
            {
                EditorActions.TexturizeAll(args.Editor.SelectedRoom, args.Editor.SelectedSectors, args.Editor.SelectedTexture, SectorFaceType.Ceiling);
            });

            AddCommand("TextureWalls", "Texture walls", CommandType.Textures, delegate (CommandArgs args)
            {
                EditorActions.TexturizeAll(args.Editor.SelectedRoom, args.Editor.SelectedSectors, args.Editor.SelectedTexture, SectorFaceType.Wall);
            });

            AddCommand("ClearAllTexturesInRoom", "Clear all textures in room", CommandType.Textures, delegate (CommandArgs args)
            {
                var emptyTexture = new TextureArea() { Texture = null };
                EditorActions.TexturizeAll(args.Editor.SelectedRoom, args.Editor.SelectedSectors, emptyTexture, SectorFaceType.Floor);
                EditorActions.TexturizeAll(args.Editor.SelectedRoom, args.Editor.SelectedSectors, emptyTexture, SectorFaceType.Ceiling);
                EditorActions.TexturizeAll(args.Editor.SelectedRoom, args.Editor.SelectedSectors, emptyTexture, SectorFaceType.Wall);
            });

            AddCommand("ClearAllTexturesInLevel", "Clear all textures in level", CommandType.Textures, delegate (CommandArgs args)
            {
                EditorActions.ClearAllTexturesInLevel(args.Editor.Level);
            });

            AddCommand("EditAnimationRanges", "Edit animation ranges...", CommandType.Textures, delegate (CommandArgs args)
            {
                var existingWindow = Application.OpenForms[nameof(FormAnimatedTextures)];

                if (existingWindow == null)
                {
                    var context = new TombEditorAnimatedTexturesContext(args.Editor);
                    var form = new FormAnimatedTextures(
                        new PanelTextureMapForAnimations(),
                        context,
                        args.Editor.Configuration
                    );

                    form.Show(args.Window);
                }
                else
                    existingWindow.Focus();
            });

            AddCommand("SmoothRandomFloorUp", "Smooth random floor up", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.SmoothRandom(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, 1, SectorVerticalPart.QA);
            });

            AddCommand("SmoothRandomFloorDown", "Smooth random floor down", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.SmoothRandom(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, -1, SectorVerticalPart.QA);
            });

            AddCommand("SmoothRandomCeilingUp", "Smooth random ceiling up", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.SmoothRandom(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, 1, SectorVerticalPart.WS);
            });

            AddCommand("SmoothRandomCeilingDown", "Smooth random ceiling down", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.SmoothRandom(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, -1, SectorVerticalPart.WS);
            });

            AddCommand("SharpRandomFloorUp", "Sharp random floor up", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.SharpRandom(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, 1, SectorVerticalPart.QA);
            });

            AddCommand("SharpRandomFloorDown", "Sharp random floor down", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.SharpRandom(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, -1, SectorVerticalPart.QA);
            });

            AddCommand("SharpRandomCeilingUp", "Sharp random ceiling up", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.SharpRandom(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, 1, SectorVerticalPart.WS);
            });

            AddCommand("SharpRandomCeilingDown", "Sharp random ceiling down", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.SharpRandom(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, -1, SectorVerticalPart.WS);
            });

            AddCommand("RealignFloorToStepHeight", "Re-align floor to step height", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.RealignToStepHeight(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, SectorVerticalPart.QA, args.Editor.IncrementReference);
            });

            AddCommand("RealignCeilingToStepHeight", "Re-align ceiling to step height", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.RealignToStepHeight(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, SectorVerticalPart.WS, args.Editor.IncrementReference);
            });

            AddCommand("ConvertFloorToQuads", "Convert floor to quads", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.ConvertAreaToQuads(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, SectorVerticalPart.QA, args.Editor.IncrementReference);
            });

            AddCommand("ConvertCeilingToQuads", "Convert ceiling to quads", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.ConvertAreaToQuads(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, SectorVerticalPart.WS, args.Editor.IncrementReference);
            });

            AddCommand("SmoothFloor", "Smooth floor", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.SmoothArea(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, SectorVerticalPart.QA, args.Editor.IncrementReference);
            });

            AddCommand("SmoothCeiling", "Smooth ceiling", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.SmoothArea(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, SectorVerticalPart.WS, args.Editor.IncrementReference);
            });

            AddCommand("AverageFloor", "Average floor", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.AverageSectors(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, SectorVerticalPart.QA, args.Editor.IncrementReference);
            });

            AddCommand("AverageCeiling", "Average ceiling", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.AverageSectors(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, SectorVerticalPart.WS, args.Editor.IncrementReference);
            });

            AddCommand("FlipFloorSplit", "Flip floor split in selected area", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.FlipFloorSplit(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area);
            });

            AddCommand("FlipCeilingSplit", "Flip ceiling split in selected area", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.FlipCeilingSplit(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area);
            });

            AddCommand("GridWallsIn3", "Grid walls in 3", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    EditorActions.GridWalls(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area);
            });

            AddCommand("GridWallsIn5", "Grid walls in 5", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    EditorActions.GridWalls(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, true);
            });

            AddCommand("GridWallsIn3Squares", "Grid walls in 3 (squares)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    EditorActions.GridWallsSquares(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area);
            });

            AddCommand("GridWallsIn5Squares", "Grid walls in 5 (squares)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    EditorActions.GridWallsSquares(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, true);
            });

            AddCommand("EditLevelSettings", "Level settings...", CommandType.Settings, delegate (CommandArgs args)
            {
                using (var form = new FormLevelSettings(args.Editor))
                    form.ShowDialog(args.Window);
            });

            AddCommand("EditOptions", "Editor options...", CommandType.Settings, delegate (CommandArgs args)
            {
                using (var form = new FormOptions(args.Editor))
                    form.ShowDialog(args.Window);
            });

            AddCommand("StartWadTool", "Start Wad Tool...", CommandType.Settings, delegate (CommandArgs args)
            {
                try
                {
                    if (!string.IsNullOrEmpty(args.Editor.Level.Settings.LevelFilePath))
                        Process.Start(DefaultPaths.WadToolExecutable, "-r \"" + args.Editor.Level.Settings.LevelFilePath + "\"");
                    else
                        Process.Start(DefaultPaths.WadToolExecutable);
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Error while starting Wad Tool.");
                    args.Editor.SendMessage("Error while starting Wad Tool.", PopupType.Error);
                }
            });

            AddCommand("StartSoundTool", "Start Sound Tool...", CommandType.Settings, delegate (CommandArgs args)
            {
                try
                {
                    if (!string.IsNullOrEmpty(args.Editor.Level.Settings.LevelFilePath))
                        Process.Start(DefaultPaths.SoundToolExecutable, "-r \"" + args.Editor.Level.Settings.MakeAbsolute(args.Editor.Level.Settings.LevelFilePath) + "\"");
                    else
                        Process.Start(DefaultPaths.SoundToolExecutable);
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Error while starting Sound Tool.");
                    args.Editor.SendMessage("Error while starting Sound Tool.", PopupType.Error);
                }
            });

            AddCommand("EditKeyboardLayout", "Edit keyboard layout...", CommandType.Settings, delegate (CommandArgs args)
            {
                using (var f = new FormKeyboardLayout(args.Editor))
                    f.ShowDialog(args.Window);
            });

            AddCommand("SwitchTool1", "Switch tool 1", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(0);
            });

            AddCommand("SwitchTool2", "Switch tool 2", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(1);
            });

            AddCommand("SwitchTool3", "Switch tool 3", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(2);
            });

            AddCommand("SwitchTool4", "Switch tool 4", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(3);
            });

            AddCommand("SwitchTool5", "Switch tool 5", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(4);
            });

            AddCommand("SwitchTool6", "Switch tool 6", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(5);
            });

            AddCommand("SwitchTool7", "Switch tool 7", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(6);
            });

            AddCommand("SwitchTool8", "Switch tool 8", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(7);
            });

            AddCommand("SwitchTool9", "Switch tool 9", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(8);
            });

            AddCommand("SwitchTool10", "Switch tool 10", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(9);
            });

            AddCommand("SwitchTool11", "Switch tool 11", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(10);
            });

            AddCommand("SwitchTool12", "Switch tool 12", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(11);
            });

            AddCommand("SwitchTool13", "Switch tool 13", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(12);
            });

            AddCommand("SwitchTool14", "Switch tool 14", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(13);
            });

            AddCommand("QuitEditor", "Quit editor", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.Quit();
            });

            AddCommand("ShowTriggerList", "Show trigger list", CommandType.Windows, (CommandArgs args) => args.Editor.ToggleToolWindow(typeof(TriggerList)));
            AddCommand("ShowRoomOptions", "Show room options", CommandType.Windows, (CommandArgs args) => args.Editor.ToggleToolWindow(typeof(RoomOptions)));
            AddCommand("ShowItemBrowser", "Show item browser", CommandType.Windows, (CommandArgs args) => args.Editor.ToggleToolWindow(typeof(ItemBrowser)));
            AddCommand("ShowImportedGeometryBrowser", "Show imported geometry browser", CommandType.Windows, (CommandArgs args) => args.Editor.ToggleToolWindow(typeof(ImportedGeometryBrowser)));
            AddCommand("ShowSectorOptions", "Show sector options", CommandType.Windows, (CommandArgs args) => args.Editor.ToggleToolWindow(typeof(SectorOptions)));
            AddCommand("ShowLighting", "Show lighting", CommandType.Windows, (CommandArgs args) => args.Editor.ToggleToolWindow(typeof(Lighting)));
            AddCommand("ShowPalette", "Show palette", CommandType.Windows, (CommandArgs args) => args.Editor.ToggleToolWindow(typeof(Palette)));
            AddCommand("ShowTexturePanel", "Show texture panel", CommandType.Windows, (CommandArgs args) => args.Editor.ToggleToolWindow(typeof(TexturePanel)));
            AddCommand("ShowObjectList", "Show object list", CommandType.Windows, (CommandArgs args) => args.Editor.ToggleToolWindow(typeof(ObjectList)));
            AddCommand("ShowToolPalette", "Show tool palette", CommandType.Windows, (CommandArgs args) => args.Editor.ToggleToolWindow(typeof(ToolPalette)));

            AddCommand("ShowStatistics", "Statistics display", CommandType.Windows, delegate (CommandArgs args)
            {
                args.Editor.Configuration.UI_ShowStats = !args.Editor.Configuration.UI_ShowStats;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawPortals", "Draw portals", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowPortals = !args.Editor.Configuration.Rendering3D_ShowPortals;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawHorizon", "Draw horizon", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowHorizon = !args.Editor.Configuration.Rendering3D_ShowHorizon;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawRoomNames", "Draw room names", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowRoomNames = !args.Editor.Configuration.Rendering3D_ShowRoomNames;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawIllegalSlopes", "Draw illegal slopes", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowIllegalSlopes = !args.Editor.Configuration.Rendering3D_ShowIllegalSlopes;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawMoveables", "Draw moveables", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowMoveables = !args.Editor.Configuration.Rendering3D_ShowMoveables;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawStatics", "Draw statics", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowStatics = !args.Editor.Configuration.Rendering3D_ShowStatics;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawImportedGeometry", "Draw imported geometry", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowImportedGeometry = !args.Editor.Configuration.Rendering3D_ShowImportedGeometry;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawGhostBlocks", "Draw ghost blocks", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowGhostBlocks = !args.Editor.Configuration.Rendering3D_ShowGhostBlocks;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawVolumes", "Draw volumes", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowVolumes = !args.Editor.Configuration.Rendering3D_ShowVolumes;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawBoundingBoxes", "Draw bounding boxes", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowBoundingBoxes = !args.Editor.Configuration.Rendering3D_ShowBoundingBoxes;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawOtherObjects", "Draw other objects", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowOtherObjects = !args.Editor.Configuration.Rendering3D_ShowOtherObjects;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawLightRadius", "Draw light radius", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowLightRadius = !args.Editor.Configuration.Rendering3D_ShowLightRadius;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawSlideDirections", "Draw slide directions", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowSlideDirections = !args.Editor.Configuration.Rendering3D_ShowSlideDirections;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawExtraBlendingModes", "Draw extra blending modes", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowExtraBlendingModes = !args.Editor.Configuration.Rendering3D_ShowExtraBlendingModes;
                args.Editor.ConfigurationChange();
            });

            AddCommand("HideTransparentFaces", "Toggle in-editor transparency", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_HideTransparentFaces = !args.Editor.Configuration.Rendering3D_HideTransparentFaces;
                args.Editor.ConfigurationChange();
            });

            AddCommand("BilinearFilter", "Toggle bilinear filtering", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_BilinearFilter = !args.Editor.Configuration.Rendering3D_BilinearFilter;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawWhiteTextureLightingOnly", "Draw untextured in Lighting Mode", CommandType.View, delegate (CommandArgs args) 
            {
                args.Editor.Configuration.Rendering3D_ShowLightingWhiteTextureOnly = !args.Editor.Configuration.Rendering3D_ShowLightingWhiteTextureOnly;
                args.Editor.ConfigurationChange();
            });

            AddCommand("ShowRealTintForObjects", "Show real tint for objects", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowRealTintForObjects = !args.Editor.Configuration.Rendering3D_ShowRealTintForObjects;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DisableGeometryPicking", "Disable geometry picking", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_DisablePickingForImportedGeometry = !args.Editor.Configuration.Rendering3D_DisablePickingForImportedGeometry;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DisableHiddenRoomPicking", "Disable hidden room picking", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_DisablePickingForHiddenRooms = !args.Editor.Configuration.Rendering3D_DisablePickingForHiddenRooms;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawAllRooms", "Draw all rooms", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowAllRooms = !args.Editor.Configuration.Rendering3D_ShowAllRooms;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawCardinalDirections", "Draw cardinal directions", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowCardinalDirections = !args.Editor.Configuration.Rendering3D_ShowCardinalDirections;
                args.Editor.ConfigurationChange();
            });

            AddCommand("SamplePaletteFromTextures", "Sample palette from textures", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Palette_TextureSamplingMode = !args.Editor.Configuration.Palette_TextureSamplingMode;
                args.Editor.ConfigurationChange();
            });

            AddCommand("ResetPalette", "Reset palette to defaults", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.RaiseEvent(new Editor.ResetPaletteEvent());
            });

            AddCommand("ToggleFlipMap", "Toggle flip map", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRoom.Alternated)
                {
                    if (args.Editor.SelectedRoom.AlternateRoom != null && args.Editor.SelectedRoom != args.Editor.SelectedRoom.AlternateRoom)
                        args.Editor.SelectedRoom = args.Editor.SelectedRoom.AlternateRoom;
                    else if (args.Editor.SelectedRoom.AlternateBaseRoom != null && args.Editor.SelectedRoom != args.Editor.SelectedRoom.AlternateBaseRoom)
                        args.Editor.SelectedRoom = args.Editor.SelectedRoom.AlternateBaseRoom;
                }
            });

            AddCommand("ToggleNoOpacity", "Toggle no opacity", CommandType.Textures, delegate (CommandArgs args)
            {
                EditorActions.SetPortalOpacity(PortalOpacity.None, args.Window);
            });

            AddCommand("ToggleOpacity", "Textured and solid ('Toggle Opacity 1')", CommandType.Textures, delegate (CommandArgs args)
            {
                EditorActions.SetPortalOpacity(PortalOpacity.SolidFaces, args.Window);
            });

            AddCommand("ToggleOpacity2", "Textured and traversable ('Toggle Opacity 2')", CommandType.Textures, delegate (CommandArgs args)
            {
                EditorActions.SetPortalOpacity(PortalOpacity.TraversableFaces, args.Window);
            });

			AddCommand("ToggleClassicPortalMirror", "Toggle classic portal mirror effect", CommandType.Rooms, delegate (CommandArgs args)
			{
				EditorActions.ToggleClassicPortalMirror(args.Window);
			});

			AddCommand("AddPointLight", "Add point light", CommandType.Lighting, delegate (CommandArgs args)
            {
                EditorActions.PlaceLight(LightType.Point);
            });

            AddCommand("AddShadow", "Add shadow", CommandType.Lighting, delegate (CommandArgs args)
            {
                EditorActions.PlaceLight(LightType.Shadow);
            });

            AddCommand("AddSunLight", "Add sun light", CommandType.Lighting, delegate (CommandArgs args)
            {
                EditorActions.PlaceLight(LightType.Sun);
            });

            AddCommand("AddSpotLight", "Add directional (spot) light", CommandType.Lighting, delegate (CommandArgs args)
            {
                EditorActions.PlaceLight(LightType.Spot);
            });

            AddCommand("AddEffectLight", "Add effect light", CommandType.Lighting, delegate (CommandArgs args)
            {
                EditorActions.PlaceLight(LightType.Effect);
            });

            AddCommand("AddFogBulb", "Add fog bulb", CommandType.Lighting, delegate (CommandArgs args)
            {
                EditorActions.PlaceLight(LightType.FogBulb);
            });

            AddCommand("EditObjectTransform", "Edit object transform", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject is not PositionBasedObjectInstance)
                {
                    args.Editor.SendMessage("Select a position-based object first.", PopupType.Warning);
                    return;
                }

                using (var form = new FormTransform(args.Editor.SelectedObject as PositionBasedObjectInstance))
                {
                    if (form.ShowDialog(args.Window) == DialogResult.Cancel)
                        return;

                    args.Editor.ObjectChange(args.Editor.SelectedObject, ObjectChangeType.Change);
                }
            });

            AddCommand("EditRoomName", "Edit room name", CommandType.Rooms, delegate (CommandArgs args)
            {
                using (var form = new FormInputBox("Edit room's name", "Insert the name of this room:", args.Editor.SelectedRoom.Name))
                {
                    if (form.ShowDialog(args.Window) == DialogResult.Cancel)
                        return;

                    args.Editor.SelectedRoom.Name = form.Result;
                    args.Editor.RoomPropertiesChange(args.Editor.SelectedRoom);
                    args.Editor.RoomListChange();
                }
            });

            AddCommand("SetFloor", "Set floor", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.SetFloor(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area);
            });

            AddCommand("SetCeiling", "Set ceiling", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.SetCeiling(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area);
            });

            AddCommand("SetWall", "Set wall", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.SetWall(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area);
            });

            AddCommand("SetBox", "Set box sector property", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.ToggleSectorFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, SectorFlags.Box);
            });

            AddCommand("SetDeath", "Set death sector property", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.ToggleSectorFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, SectorFlags.DeathFire);
            });

            AddCommand("SetMonkeyswing", "Set monkeyswing sector property", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion.Native() >= TRVersion.Game.TR3, "Monkeyswing"))
                    return;
                EditorActions.ToggleSectorFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, SectorFlags.Monkey);
            });

            AddCommand("SetClimbPositiveZ", "Climb on North sector side", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion.SupportsClimbing(), "Climbing"))
                    return;
                EditorActions.ToggleSectorFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, SectorFlags.ClimbPositiveZ);
            });

            AddCommand("SetClimbPositiveX", "Climb on East sector side", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion.SupportsClimbing(), "Climbing"))
                    return;
                EditorActions.ToggleSectorFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, SectorFlags.ClimbPositiveX);
            });

            AddCommand("SetClimbNegativeZ", "Climb on South sector side", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion.SupportsClimbing(), "Climbing"))
                    return;
                EditorActions.ToggleSectorFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, SectorFlags.ClimbNegativeZ);
            });

            AddCommand("SetClimbNegativeX", "Climb on West sector side", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion.SupportsClimbing(), "Climbing"))
                    return;
                EditorActions.ToggleSectorFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, SectorFlags.ClimbNegativeX);
            });

            AddCommand("SetNotWalkable", "Set non-walkable floor", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.ToggleSectorFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, SectorFlags.NotWalkableFloor);
            });

            AddCommand("SetDiagonalFloorStep", "Set or rotate diagonal floor step", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.SetDiagonalFloorSplit(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area);
            });

            AddCommand("SetDiagonalCeilingStep", "Set or rotate diagonal ceiling step", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.SetDiagonalCeilingSplit(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area);
            });

            AddCommand("SetDiagonalWall", "Set or rotate diagonal wall", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.SetDiagonalWall(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area);
            });

            AddCommand("SetBeetleCheckpoint", "Set beetle checkpoint / minecart right (TR3)", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion.Native() >= TRVersion.Game.TR3, "This flag"))
                    return;
                EditorActions.ToggleSectorFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, SectorFlags.Beetle);
            });

            AddCommand("SetTriggerTriggerer", "Set trigger triggerer / minecart left (TR3)", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion.Native() >= TRVersion.Game.TR3, "This flag"))
                    return;
                EditorActions.ToggleSectorFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, SectorFlags.TriggerTriggerer);
            });

            AddCommand("ToggleForceFloorSolid", "Force solid floor", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.ToggleForceFloorSolid(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area);
            });

            AddCommand("AddGhostBlocksToSelection", "Add ghost blocks to selected area", CommandType.Objects, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndSectorSelection(args.Window))
                    return;
                EditorActions.AddGhostBlocks(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area);
            });

            AddCommand("SetRoomOutside", "Set to outside", CommandType.Rooms, delegate (CommandArgs args)
            {
                if(args.Editor.SelectedRoom != null )
                {
                    args.Editor.SelectedRoom.Properties.FlagOutside = !args.Editor.SelectedRoom.Properties.FlagOutside;
                    args.Editor.RoomPropertiesChange(args.Editor.SelectedRoom);
                }
            });

            AddCommand("SetRoomSkybox", "Set skybox", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRoom != null)
                {
                    args.Editor.SelectedRoom.Properties.FlagHorizon = !args.Editor.SelectedRoom.Properties.FlagHorizon;
                    args.Editor.RoomPropertiesChange(args.Editor.SelectedRoom);
                }
            });

            AddCommand("SetRoomNoLensflare", "Disable global lensflare", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion.Native() >= TRVersion.Game.TR4, "NL flag"))
                    return;
                if (args.Editor.SelectedRoom != null)
                {
                    args.Editor.SelectedRoom.Properties.FlagNoLensflare = !args.Editor.SelectedRoom.Properties.FlagNoLensflare;
                    args.Editor.RoomPropertiesChange(args.Editor.SelectedRoom);
                }
            });

            AddCommand("SetRoomNoPathfinding", "Exclude from pathfinding", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRoom != null)
                {
                    args.Editor.SelectedRoom.Properties.FlagExcludeFromPathFinding = !args.Editor.SelectedRoom.Properties.FlagExcludeFromPathFinding;
                    args.Editor.RoomPropertiesChange(args.Editor.SelectedRoom);
                }
            });

            AddCommand("SetRoomCold", "Set room to cold (TRNG only)", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion >= TRVersion.Game.TRNG, "Cold flag"))
                    return;
                if (args.Editor.SelectedRoom != null)
                {
                    args.Editor.SelectedRoom.Properties.FlagCold = !args.Editor.SelectedRoom.Properties.FlagCold;
                    args.Editor.RoomPropertiesChange(args.Editor.SelectedRoom);
                }
            });

            AddCommand("SetRoomDamage", "Set room to damage (TRNG only)", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion >= TRVersion.Game.TRNG, "Damage flag"))
                    return;
                if (args.Editor.SelectedRoom != null)
                {
                    args.Editor.SelectedRoom.Properties.FlagDamage = !args.Editor.SelectedRoom.Properties.FlagDamage;
                    args.Editor.RoomPropertiesChange(args.Editor.SelectedRoom);
                }
            });

            AddCommand("FlattenFloor", "Flatten floor", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRoom != null && args.Editor.SelectedSectors.ValidOrNone)
                    EditorActions.FlattenRoomArea(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Valid ? args.Editor.SelectedSectors.Area : args.Editor.SelectedRoom.LocalArea.Inflate(-1), null, false, false, true);
            });

            AddCommand("FlattenCeiling", "Flatten ceiling", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRoom != null && args.Editor.SelectedSectors.ValidOrNone)
                    EditorActions.FlattenRoomArea(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Valid ? args.Editor.SelectedSectors.Area : args.Editor.SelectedRoom.LocalArea.Inflate(-1), null, true, false, true);
            });

            AddCommand("ResetGeometry", "Reset all geometry", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRoom != null && args.Editor.SelectedSectors.ValidOrNone)
                {
                    EditorActions.FlattenRoomArea(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Valid ? args.Editor.SelectedSectors.Area : args.Editor.SelectedRoom.LocalArea.Inflate(-1), null, false, true, false);
                    EditorActions.FlattenRoomArea(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Valid ? args.Editor.SelectedSectors.Area : args.Editor.SelectedRoom.LocalArea.Inflate(-1), null, true, true, true, true);
                }
            });

            AddCommand("ToggleFlyMode", "Toggle fly mode", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.SendMessage("Push WASD keys to move around.\nPush ESC to exit fly mode.", PopupType.Info);
                args.Editor.ToggleFlyMode(!args.Editor.FlyMode);
            });

            AddCommand("SelectSkyRooms", "Select sky rooms", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.SelectSkyRooms();
            });

            AddCommand("SelectWaterRooms", "Select water rooms", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.SelectWaterRooms();
            });

            AddCommand("SelectOutsideRooms", "Select outside rooms", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.SelectOutsideRooms();
            });

            AddCommand("SelectQuicksandRooms", "Select quicksand rooms", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.SelectQuicksandRooms();
            });

            AddCommand("SelectRoomsByTags", "Select rooms by tags", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.SelectRoomsByTags(args.Window);
            });

            AddCommand("SetStaticMeshesColorToRoomLight", "Set room static meshes color to room color", CommandType.Objects, delegate (CommandArgs args)
            {
                EditorActions.SetStaticMeshesColorToRoomAmbientLight();
            });

            AddCommand("SetStaticMeshesColor", "Set room static meshes color", CommandType.Objects, delegate (CommandArgs args)
            {
                EditorActions.SetStaticMeshesColor(args.Window);
            });

            AddCommand("MakeQuickItemGroup", "Make quick itemgroup", CommandType.Objects, delegate (CommandArgs args)
            {
                if (!EditorActions.VersionCheck(args.Editor.Level.IsNG, "Item grouping"))
                    return;
                EditorActions.MakeQuickItemGroup(args.Window);
            });

            AddCommand("SelectAllObjectsInArea", "Select all objects in selected area", CommandType.Objects, delegate (CommandArgs args)
            {
                EditorActions.SelectObjectsInArea(args.Window, args.Editor.SelectedSectors);
            });

            AddCommand("InPlaceSearchRooms", "Room in-place search", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.ActivateDefaultControl(nameof(RoomOptions));
            });

            AddCommand("InPlaceSearchItems", "Item in-place search", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.ActivateDefaultControl(nameof(ItemBrowser));
            });

            AddCommand("InPlaceSearchTextures", "Texture in-place search", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.ActivateDefaultControl(nameof(TexturePanel));
            });

            AddCommand("InPlaceSearchImportedGeometry", "Imported geometry in-place search", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.ActivateDefaultControl(nameof(ImportedGeometryBrowser));
            });

            AddCommand("SearchMenus", "Search menu entries", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.ActivateDefaultControl(nameof(FormMain));
            });

            AddCommand("DeleteAllLights", "Delete lights in selected rooms", CommandType.Edit, delegate (CommandArgs args) 
            {
                if (DarkMessageBox.Show(args.Window, "Do you want to delete all lights in level? This action can't be undone.",
                                   "Delete all lights", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;

				foreach (var room in args.Editor.SelectedRooms)
                {
					var objects = room.Objects.Where(ob => ob is LightInstance).ToList();
					if (objects.Count > 0)
						for (int i = objects.Count - 1; i >= 0; i--)
                        {
							var obj = objects[i];
							EditorActions.DeleteObjectWithoutUpdate(obj);
							objects.RemoveAt(i);
						}
				}
			});

			AddCommand("GetObjectStatistics", "Copy object statistics into clipboard", CommandType.Objects, delegate (CommandArgs args) 
            {
				SortedDictionary<WadMoveableId,uint> moveablesCount = new SortedDictionary<WadMoveableId, uint>();
				SortedDictionary<WadStaticId,uint> staticsCount = new SortedDictionary<WadStaticId, uint>();
				int totalMoveablesCount;
				int totalStaticsCount;

				EditorActions.GetObjectStatistics(args.Editor, moveablesCount, staticsCount, out totalMoveablesCount, out totalStaticsCount);

				var sb = new StringBuilder();
				foreach (var kvp in moveablesCount) {
					string name = TrCatalog.GetMoveableName(args.Editor.Level.Settings.GameVersion, kvp.Key.TypeId);
					sb.AppendLine(name + "\t\t\tx" + kvp.Value);
				}
				sb.AppendLine("----------------------------------------------------");
				foreach (var kvp in staticsCount) {
					string name = TrCatalog.GetStaticName(args.Editor.Level.Settings.GameVersion, kvp.Key.TypeId);
					sb.AppendLine(name + "\t\t\tx" + kvp.Value);
				}
				sb.AppendLine("----------------------------------------------------");
				sb.AppendLine("Total Moveables :\t\t\t" + totalMoveablesCount);
				sb.AppendLine("Total Statics :\t\t\t" + totalStaticsCount);
				Clipboard.SetText(sb.ToString());
				args.Editor.SendMessage("Object statistics copied into clipboard!", PopupType.Info);
			});

            AddCommand("MoveObjectLeftPrecise", "Move object left (8 units)", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(-8, 0, 0), new Vector3(), true);
            });

            AddCommand("MoveObjectRightPrecise", "Move object right (8 units)", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(8, 0, 0), new Vector3(), true);
            });

            AddCommand("MoveObjectForwardPrecise", "Move object forward (8 units)", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(0, 0, 8), new Vector3(), true);
            });

            AddCommand("MoveObjectBackPrecise", "Move object back (8 units)", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(0, 0, -8), new Vector3(), true);
            });

            AddCommand("MoveObjectUpPrecise", "Move object up (8 units)", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(0, 8, 0), new Vector3(), true);
            });

            AddCommand("MoveObjectDownPrecise", "Move object down (8 units)", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(0, -8, 0), new Vector3(), true);
            });

            AddCommand("IncreaseStepHeight", "Increase step height", CommandType.General, delegate (CommandArgs args)
            {
                int previous = args.Editor.Configuration.Editor_StepHeight;

                args.Editor.Configuration.Editor_StepHeight = args.Editor.Configuration.Editor_StepHeight switch
                {
                    32 => 64,
                    64 => 128,
                    _ => 256
                };

                args.Editor.RaiseEvent(new Editor.StepHeightChangedEvent { Previous = previous, Current = args.Editor.Configuration.Editor_StepHeight });
            });

            AddCommand("DecreaseStepHeight", "Decrease step height", CommandType.General, delegate (CommandArgs args)
            {
                int previous = args.Editor.Configuration.Editor_StepHeight;

                args.Editor.Configuration.Editor_StepHeight = args.Editor.Configuration.Editor_StepHeight switch
                {
                    256 => 128,
                    128 => 64,
                    _ => 32
                };

                args.Editor.RaiseEvent(new Editor.StepHeightChangedEvent { Previous = previous, Current = args.Editor.Configuration.Editor_StepHeight });
            });

            AddCommand("HighlightSplit1", "Highlight split 1", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.HighlightedSplit = 1;
            });

            AddCommand("HighlightSplit2", "Highlight split 2", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.HighlightedSplit = 2;
            });

            AddCommand("HighlightSplit3", "Highlight split 3", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.HighlightedSplit = 3;
            });

            AddCommand("HighlightSplit4", "Highlight split 4", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.HighlightedSplit = 4;
            });

            AddCommand("HighlightSplit5", "Highlight split 5", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.HighlightedSplit = 5;
            });

            AddCommand("HighlightSplit6", "Highlight split 6", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.HighlightedSplit = 6;
            });

            AddCommand("HighlightSplit7", "Highlight split 7", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.HighlightedSplit = 7;
            });

            AddCommand("HighlightSplit8", "Highlight split 8", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.HighlightedSplit = 8;
            });

            AddCommand("HighlightSplit9", "Highlight split 9", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.HighlightedSplit = 9;
            });

            _commands = _commands.OrderBy(o => o.Type).ToList();
        }
    }
}
