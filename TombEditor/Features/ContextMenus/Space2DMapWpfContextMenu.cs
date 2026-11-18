#nullable enable

using Clipboard = System.Windows.Clipboard;
using static TombEditor.Features.ContextMenus.WpfContextMenuHelper;
using System.Numerics;
using System.Windows.Controls;
using TombLib;
using TombLib.Services;
using TombLib.Utils;

namespace TombEditor.Features.ContextMenus;

internal static class Space2DMapWpfContextMenu
{
    public static ContextMenu Show(Editor editor, IWindowOwner owner,
                                   Vector2 position, System.Drawing.Point screenPoint)
    {
        var menu = new ContextMenu();

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

        Open(menu, screenPoint);
        return menu;
    }
}
