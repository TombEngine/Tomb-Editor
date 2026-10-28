#nullable enable

using Clipboard = System.Windows.Clipboard;
using static TombEditor.Features.ContextMenus.WpfContextMenuHelper;
using System.Numerics;
using System.Windows.Controls;
using TombLib;
using TombLib.Services;
using TombLib.Utils;

namespace TombEditor.Features.ContextMenus;

internal static class SelectedRoomWpfContextMenu
{
    public static ContextMenu Show(Editor editor, IWindowOwner owner,
                                   Vector2 position, System.Drawing.Point screenPoint)
    {
        var menu = new ContextMenu();

        menu.Items.Add(Item("Export rooms...", "General/Export",
            () => EditorActions.ExportRooms(editor.SelectedRooms, owner)));

        menu.Items.Add(Sep());

        menu.Items.Add(Item("Delete rooms", "General/trash",
            () => EditorActions.DeleteRooms(editor.SelectedRooms, owner)));

        menu.Items.Add(Item("Copy rooms", "General/copy",
            () => Clipboard.SetDataObject(new RoomClipboardData(editor, position), true)));

        menu.Items.Add(Item("Paste rooms", "General/clipboard",
            () =>
            {
                var data = Clipboard.GetDataObject().GetData(typeof(RoomClipboardData)) as RoomClipboardData;
                if (data == null)
                    editor.SendMessage("Clipboard contains no room data.", PopupType.Error);
                else
                    data.MergeInto(editor, VectorInt2.FromRounded(position - data.DropPosition));
            },
            enabled: Clipboard.ContainsData(typeof(RoomClipboardData).FullName)));

        menu.Items.Add(Sep());

        menu.Items.Add(Item("Rotate rooms clockwise", null,
            () => EditorActions.TransformRooms(new RectTransformation { QuadrantRotation = -1 }, owner)));
        menu.Items.Add(Item("Rotate rooms counterclockwise", null,
            () => EditorActions.TransformRooms(new RectTransformation { QuadrantRotation = 1 }, owner)));
        menu.Items.Add(Item("Mirror rooms on X axis", null,
            () => EditorActions.TransformRooms(new RectTransformation { MirrorX = true }, owner)));
        menu.Items.Add(Item("Mirror rooms on Z axis", null,
            () => EditorActions.TransformRooms(new RectTransformation { MirrorX = true, QuadrantRotation = 2 }, owner)));
        menu.Items.Add(Item("Merge rooms horizontally", null,
            () => EditorActions.MergeRoomsHorizontally(editor.SelectedRooms, owner)));

        Open(menu, screenPoint);
        return menu;
    }
}
