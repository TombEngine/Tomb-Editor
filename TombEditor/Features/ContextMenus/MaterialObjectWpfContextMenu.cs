#nullable enable

using Clipboard = System.Windows.Clipboard;
using static TombEditor.Features.ContextMenus.WpfContextMenuHelper;
using System;
using System.Windows.Controls;
using TombLib.LevelData;
using TombLib.Services;

namespace TombEditor.Features.ContextMenus;

internal static class MaterialObjectWpfContextMenu
{
    public static ContextMenu Show(Editor editor, IWindowOwner owner,
                                   ObjectInstance targetObject,
                                   System.Drawing.Point screenPoint)
    {
        var menu = new ContextMenu();

        // ----- ScriptID / rename (TombEngine + NG only) -----------------
        if (targetObject is IHasScriptID hasScriptId)
        {
            if (editor.Level.IsNG && targetObject == editor.SelectedObject)
            {
                var startString = hasScriptId.ScriptId.HasValue
                    ? "(ScriptID = " + hasScriptId.ScriptId + ") Copy "
                    : "Assign and copy ";
                menu.Items.Add(Item(startString + "script ID to clipboard", null,
                    () => CommandHandler.GetCommand("AssignAndClipboardScriptId")
                                        .Execute(new CommandArgs { Editor = editor, Window = owner })));
                menu.Items.Add(Sep());
            }

            if (targetObject is IHasLuaName luaTarget && luaTarget.SupportsLuaName())
                menu.Items.Add(Item("Rename object", "General/edit",
                    () => EditorActions.RenameObject(targetObject, owner)));
        }

        if (targetObject is not (LightInstance or GhostBlockInstance))
            menu.Items.Add(Item("Edit object", "General/edit",
                () => EditorActions.EditObject(targetObject, owner)));

        if (targetObject is FlybyCameraInstance flyby)
            menu.Items.Add(Item("Preview flyby sequence", "Objects/movie_projector",
                () => editor.ToggleCameraPreview(true, flyby)));

        if (targetObject is CameraInstance cam)
            menu.Items.Add(Item("Preview camera", "Objects/movie_projector",
                () => editor.ToggleCameraPreview(true, cam)));

        menu.Items.Add(Item("Copy",   "General/copy_link",     () => EditorActions.TryCopyObject(targetObject, owner)));
        menu.Items.Add(Item("Clone",  "Actions/rubber_stamp",  () => EditorActions.TryStampObject(targetObject, owner)));
        menu.Items.Add(Item("Delete", "Toolbox/Eraser",        () => EditorActions.DeleteObject(targetObject, owner)));
        menu.Items.Add(Item("Bookmark object", null,           () => EditorActions.BookmarkObject(targetObject)));

        if (targetObject is IReplaceable)
            menu.Items.Add(Item("Replace object...", null,
                () =>
                {
                    editor.SelectedObject = targetObject;
                    EditorActions.ReplaceObject(owner, true);
                }));

        AppendSeparatorIfNeeded(menu);

        // ----- Static-mesh merge --------------------------------------
        if (targetObject is StaticInstance stat)
        {
            bool isMerged = editor.Level.Settings.AutoStaticMeshMergeContainsStaticMesh(
                editor.Level.Settings.WadTryGetStatic(stat.WadObjectId));

            menu.Items.Add(Item("Merge into room geometry", null,
                () =>
                {
                    if (!isMerged)
                        editor.Level.Settings.AutoStaticMeshMerges.Add(
                            new AutoStaticMeshMergeEntry(stat.WadObjectId.TypeId, true, false, false, false, editor.Level.Settings));
                    else
                        editor.Level.Settings.AutoStaticMeshMerges.RemoveAll(item => item.meshId == stat.WadObjectId.TypeId);
                    editor.MergedStaticsChange();
                },
                isChecked: isMerged));
        }

        // ----- Imported geometry --------------------------------------
        if (targetObject is ImportedGeometryInstance geo)
        {
            menu.Items.Add(Item("Hide in editor", "General/Invisible",
                () =>
                {
                    geo.Hidden = !geo.Hidden;
                    editor.ObjectChange(geo, ObjectChangeType.Change);
                },
                isChecked: geo.Hidden));

            menu.Items.Add(Item("Reload imported geometry", "Actions/refresh",
                () =>
                {
                    editor.Level.Settings.ImportedGeometryUpdate(geo.Model, geo.Model.Info);
                    editor.LoadedImportedGeometriesChange();
                }));
        }

        // ----- Position-based actions ---------------------------------
        if (targetObject is PositionBasedObjectInstance pos)
        {
            AppendSeparatorIfNeeded(menu);

            menu.Items.Add(Item("Select floor below current object", null,
                () => EditorActions.SelectFloorBelowObject(pos)));

            if (targetObject.Room != editor.SelectedRoom)
                menu.Items.Add(Item("Move object to current room", null,
                    () => EditorActions.MoveObjectToOtherRoom(pos, editor.SelectedRoom)));

            menu.Items.Add(Item("Edit object transform", null,
                () => CommandHandler.GetCommand("EditObjectTransform")
                                    .Execute(new CommandArgs { Editor = editor, Window = owner })));

            menu.Items.Add(Item("Copy position to clipboard", null,
                () =>
                {
                    var wp = pos.WorldPosition;
                    wp.Y = -wp.Y;
                    Clipboard.SetText(wp.ToString().Trim('<', '>'));
                }));
        }

        if (targetObject is IHasLuaName luaObject && luaObject.SupportsLuaName())
        {
            menu.Items.Add(Item("Copy Lua name to clipboard", null,
                () =>
                {
                    if (string.IsNullOrEmpty(luaObject.LuaName))
                        luaObject.AllocateNewLuaName();
                    Clipboard.SetText(luaObject.LuaName);
                }));
        }

        // ----- Rotation reset -----------------------------------------
        if (targetObject is PositionBasedObjectInstance posR
            && (targetObject is IRotateableY || targetObject is IRotateableYX || targetObject is IRotateableYXRoll))
        {
            menu.Items.Add(Item("Reset rotation (all axes)", "Actions/center_direction",
                () => EditorActions.ResetObjectRotation(posR)));

            if (targetObject is IRotateableY ry && ry.RotationY != 0.0f)
                menu.Items.Add(Item("Reset rotation (Y axis)", null,
                    () => EditorActions.ResetObjectRotation(posR, RotationAxis.Y)));

            if (targetObject is IRotateableYX ryx && ryx.RotationX != 0.0f)
                menu.Items.Add(Item("Reset rotation (X axis)", null,
                    () => EditorActions.ResetObjectRotation(posR, RotationAxis.X)));

            if (targetObject is IRotateableYXRoll ryxr && ryxr.Roll != 0.0f)
                menu.Items.Add(Item("Reset rotation (Roll axis)", null,
                    () => EditorActions.ResetObjectRotation(posR, RotationAxis.Roll)));
        }

        // ----- Scale reset --------------------------------------------
        var size  = (targetObject as ISizeable)?.Size.Length();
        var scale = (targetObject as IScaleable)?.Scale;
        if (targetObject is PositionBasedObjectInstance posS
            && ((scale.HasValue && scale != 1.0f) || (size.HasValue && size != Math.Sqrt(3))))
        {
            menu.Items.Add(Item("Reset scale", null,
                () => EditorActions.ResetObjectScale(posS)));
        }

        // ----- Triggers pointing at this object -----------------------
        var triggers = editor.Level.GetAllTriggersPointingToObject(targetObject);
        if (triggers.Count != 0)
        {
            AppendSeparatorIfNeeded(menu);
            foreach (var trigger in triggers)
            {
                menu.Items.Add(Item("Trigger in room " + trigger.Room.Name, null,
                    () =>
                    {
                        editor.SelectRoom(trigger.Room);
                        editor.MoveCameraToSector(trigger.Area.Start);
                        editor.SelectedSectors = new SectorSelection { Area = trigger.Area };
                    }));
            }
        }

        // Trim a trailing separator if the conditional branches left one.
        while (menu.Items.Count > 0 && menu.Items[menu.Items.Count - 1] is Separator)
            menu.Items.RemoveAt(menu.Items.Count - 1);

        Open(menu, screenPoint);
        return menu;
    }

    private static void AppendSeparatorIfNeeded(ContextMenu menu)
    {
        if (menu.Items.Count == 0) return;
        if (menu.Items[menu.Items.Count - 1] is Separator) return;
        menu.Items.Add(Sep());
    }
}
