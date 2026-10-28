#nullable enable

using System.Numerics;
using TombLib;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.ColorEditing;

/// <summary>Applies live ambient lighting edits while retaining cancel and undo behavior.</summary>
internal static class AmbientLightEditor
{
    public static void Edit(Editor editor)
    {
        var room = editor.SelectedRoom;
        var originalColor = room.Properties.AmbientLight;
        var undo = new RoomPropertyUndoInstance(editor.UndoManager, room);
        var picker = ServiceLocator.ResolveService<IColorPickerService>();
        var selectedColor = picker.PickColor(originalColor * 0.5f, color => Apply(editor, room, color * 2.0f));

        if (selectedColor is not { } color)
        {
            Apply(editor, room, originalColor);
            return;
        }

        var ambientColor = color * 2.0f;

        if (editor.Level.Settings.GameVersion.Native() < TRVersion.Game.TR3)
        {
            if (color.X != color.Y || color.Y != color.Z)
                editor.SendMessage("Only grayscale lighting is possible for this game version.", PopupType.Info);

            ambientColor = Vector3.Clamp(new Vector3(ambientColor.GetLuma()), Vector3.Zero, Vector3.One);
        }

        editor.UndoManager.Push(undo);
        Apply(editor, room, ambientColor);
    }

    private static void Apply(Editor editor, Room room, Vector3 color)
    {
        room.Properties.AmbientLight = color;
        editor.UpdateRoomLighting(room);
        editor.RoomPropertiesChange(room);
    }
}
