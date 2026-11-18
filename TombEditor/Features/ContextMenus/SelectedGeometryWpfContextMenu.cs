#nullable enable

using Clipboard = System.Windows.Clipboard;
using static TombEditor.Features.ContextMenus.WpfContextMenuHelper;
using System.Windows.Controls;
using TombLib;
using TombLib.LevelData;
using TombLib.Services;

namespace TombEditor.Features.ContextMenus;

internal static class SelectedGeometryWpfContextMenu
{
    public static ContextMenu Show(Editor editor, IWindowOwner owner, Room targetRoom,
                                   RectangleInt2 targetArea, VectorInt2 targetSector,
                                   System.Drawing.Point screenPoint)
    {
        var menu = new ContextMenu();

        menu.Items.Add(Item("Paste object", "General/clipboard",
            () => EditorActions.PasteObject(targetSector, targetRoom),
            enabled: Clipboard.ContainsData(typeof(ObjectClipboardData).FullName)));

        menu.Items.Add(Item("Select objects", null,
            () => EditorActions.SelectObjectsInArea(owner, editor.SelectedSectors)));

        menu.Items.Add(Sep());

        menu.Items.Add(Item("Move Lara", null,
            () => EditorActions.MoveLara(owner, targetRoom, targetSector)));

        menu.Items.Add(Item("Move Object", "General/target",
            () =>
            {
                if (editor.SelectedObject is PositionBasedObjectInstance obj)
                    EditorActions.MoveObject(obj, targetRoom, targetSector);
            },
            enabled: editor.SelectedObject is PositionBasedObjectInstance
                     && editor.SelectedObject is not ObjectGroup));

        menu.Items.Add(Sep());

        menu.Items.Add(Item("Add trigger", null,
            () => CommandHandler.GetCommand("AddTrigger").Execute(new CommandArgs { Editor = editor, Window = owner })));
        menu.Items.Add(Item("Add portal", null,
            () => CommandHandler.GetCommand("AddPortal").Execute(new CommandArgs { Editor = editor, Window = owner })));

        menu.Items.Add(Sep());

        menu.Items.Add(Item("Add camera",            "Objects/Camera",          () => EditorActions.PlaceObject(targetRoom, targetSector, new CameraInstance())));
        menu.Items.Add(Item("Add flyby camera",      "Objects/movie_projector", () => EditorActions.PlaceObject(targetRoom, targetSector, new FlybyCameraInstance(editor.SelectedObject))));
        menu.Items.Add(Item("Add sink",              "Objects/tornado",         () => EditorActions.PlaceObject(targetRoom, targetSector, new SinkInstance())));
        menu.Items.Add(Item("Add sound source",      "Objects/speaker",         () => EditorActions.PlaceObject(targetRoom, targetSector, new SoundSourceInstance())));
        menu.Items.Add(Item("Add imported geometry", "Objects/custom-geometry", () => EditorActions.PlaceObject(targetRoom, targetSector, new ImportedGeometryInstance())));
        menu.Items.Add(Item("Add memo",              "Objects/Memo",            () => EditorActions.PlaceObject(targetRoom, targetSector, new MemoInstance())));

        menu.Items.Add(Sep());

        menu.Items.Add(Item("Crop room",  "General/crop", () => EditorActions.CropRoom(targetRoom, targetArea, owner)));
        menu.Items.Add(Item("Split room", "Actions/Split", () => EditorActions.SplitRoom(owner)));

        Open(menu, screenPoint);
        return menu;
    }
}
