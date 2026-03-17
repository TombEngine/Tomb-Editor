using System.Windows.Forms;
using System;
using TombLib.LevelData;

namespace TombEditor.Controls.ContextMenus
{
    class MaterialObjectContextMenu : BaseContextMenu
    {
        public MaterialObjectContextMenu(Editor editor, IWin32Window owner, ObjectInstance targetObject)
            : base(editor, owner)
        {
            if (targetObject is IHasScriptID)
            {
                if (_editor.Level.IsNG && targetObject == editor.SelectedObject)
                {
                    var obj = (targetObject as IHasScriptID);
                    var startString = obj.ScriptId.HasValue ?
                        "(ScriptID = " + obj.ScriptId.ToString() + ") Copy " : "Assign and copy ";

                    Items.Add(new ToolStripMenuItem(startString + "script ID to clipboard", null, (o, e) =>
                    {
                        CommandHandler.GetCommand("AssignAndClipboardScriptId").Execute(new CommandArgs { Editor = editor, Window = owner });
                    }));
                    Items.Add(new ToolStripSeparator());
                }

                if (_editor.Level.IsTombEngine)
                {
                    Items.Add(new ToolStripMenuItem("Rename object", Properties.Resources.general_edit_16, (o, e) =>
                    {
                        EditorActions.RenameObject(targetObject, owner);
                    }));
                }
            }

            if (!(targetObject is LightInstance || targetObject is GhostBlockInstance))
            { 
                Items.Add(new ToolStripMenuItem("Edit object", Properties.Resources.general_edit_16, (o, e) =>
                {
                    EditorActions.EditObject(targetObject, owner);
                }));
            }

            if (targetObject is FlybyCameraInstance flybyTarget)
            {
                Items.Add(new ToolStripMenuItem("Preview flyby sequence", Properties.Resources.objects_movie_projector_16, (o, e) =>
                {
                    editor.ToggleCameraPreview(true, flybySequence: flybyTarget.Sequence);
                }));
            }

            if (targetObject is CameraInstance cameraTarget)
            {
                Items.Add(new ToolStripMenuItem("Preview camera", Properties.Resources.objects_movie_projector_16, (o, e) =>
                {
                    editor.ToggleCameraPreview(true, camera: cameraTarget);
                }));
            }

            Items.Add(new ToolStripMenuItem("Copy", Properties.Resources.general_copy_link_16, (o, e) =>
            {
                EditorActions.TryCopyObject(targetObject, owner);
            }));

            Items.Add(new ToolStripMenuItem("Clone", Properties.Resources.actions_rubber_stamp_16, (o, e) =>
            {
                EditorActions.TryStampObject(targetObject, owner);
            }));

            Items.Add(new ToolStripMenuItem("Delete", Properties.Resources.toolbox_Eraser_16, (o, e) =>
            {
                EditorActions.DeleteObject(targetObject, owner);
            }));

            Items.Add(new ToolStripMenuItem("Bookmark object", null, (o, e) =>
            {
                EditorActions.BookmarkObject(targetObject);
            }));

            if (targetObject is IReplaceable)
                Items.Add(new ToolStripMenuItem("Replace object...", null, (o, e) =>
                {
                    _editor.SelectedObject = targetObject;
                    EditorActions.ReplaceObject(owner, true);
                }));

            if (Items.Count > 2 && !(Items[Items.Count - 2] is ToolStripSeparator))
                Items.Add(new ToolStripSeparator());

            if (targetObject is StaticInstance)
            {
                var stat = (StaticInstance)targetObject;
                bool isMerged = editor.Level.Settings.AutoStaticMeshMergeContainsStaticMesh(editor.Level.Settings.WadTryGetStatic(stat.WadObjectId));

                Items.Add(new ToolStripMenuItem("Merge into room geometry", null, (o, e) =>
                {
                    if (!isMerged)
                        editor.Level.Settings.AutoStaticMeshMerges.Add(new AutoStaticMeshMergeEntry(stat.WadObjectId.TypeId, true, false, false, false, editor.Level.Settings));
                    else
                        editor.Level.Settings.AutoStaticMeshMerges.RemoveAll(item => item.meshId == stat.WadObjectId.TypeId);

                    _editor.MergedStaticsChange();
                })
                { Checked = isMerged });
            }

            if (targetObject is ImportedGeometryInstance)
            {
                var geo = (ImportedGeometryInstance)targetObject;

                Items.Add(new ToolStripMenuItem("Hide in editor", Properties.Resources.toolbox_Invisible_16, (o, e) =>
                {
                    geo.Hidden = !geo.Hidden;
                    _editor.ObjectChange(geo, ObjectChangeType.Change);
                })
                { Checked = geo.Hidden });

                Items.Add(new ToolStripMenuItem("Reload imported geometry", Properties.Resources.actions_refresh_16, (o, e) =>
                {
                    _editor.Level.Settings.ImportedGeometryUpdate(geo.Model, geo.Model.Info);
                    _editor.LoadedImportedGeometriesChange();
                }));
            }

            if (targetObject is PositionBasedObjectInstance)
            {
                if (!(Items[Items.Count-1] is ToolStripSeparator))
                    Items.Add(new ToolStripSeparator());

                Items.Add(new ToolStripMenuItem("Select floor below current object", null, (o, e) =>
                {
                    EditorActions.SelectFloorBelowObject((PositionBasedObjectInstance)targetObject);
                }));

                if (targetObject.Room != _editor.SelectedRoom)
                {
                    Items.Add(new ToolStripMenuItem("Move object to current room", null, (o, e) =>
                    {
                        EditorActions.MoveObjectToOtherRoom((PositionBasedObjectInstance)targetObject, _editor.SelectedRoom);
                    }));
                }

                Items.Add(new ToolStripMenuItem("Edit object transform", null, (o, e) =>
                {
                    CommandHandler.GetCommand("EditObjectTransform").Execute(new CommandArgs { Editor = editor, Window = owner });
                }));

                Items.Add(new ToolStripMenuItem("Copy position to clipboard", null, (o, e) =>
                {
                    var pos = (targetObject as PositionBasedObjectInstance).WorldPosition;
                    pos.Y = -pos.Y;
                    Clipboard.SetText(pos.ToString().Trim(new char[] {'<', '>'}));
                }));
            }

            if (targetObject is PositionAndScriptBasedObjectInstance && _editor.Level.Settings.GameVersion == TRVersion.Game.TombEngine)
            {
                Items.Add(new ToolStripMenuItem("Copy Lua name to clipboard", null, (o, e) =>
                {
                    var scriptObj = targetObject as PositionAndScriptBasedObjectInstance;
                    if (string.IsNullOrEmpty(scriptObj.LuaName))
                        scriptObj.AllocateNewLuaName();

                    Clipboard.SetText((targetObject as PositionAndScriptBasedObjectInstance).LuaName);
                }));
            }

            if (targetObject is PositionBasedObjectInstance && (targetObject is IRotateableY || targetObject is IRotateableYX || targetObject is IRotateableYXRoll))
            {
                Items.Add(new ToolStripMenuItem("Reset rotation (all axes)", Properties.Resources.actions_center_direction_16, (o, e) =>
                {
                    EditorActions.ResetObjectRotation((PositionBasedObjectInstance)targetObject);
                }));

                if (targetObject is IRotateableY && (targetObject as IRotateableY).RotationY != 0.0f)
                {
                    Items.Add(new ToolStripMenuItem("Reset rotation (Y axis)", null, (o, e) =>
                    {
                        EditorActions.ResetObjectRotation((PositionBasedObjectInstance)targetObject, RotationAxis.Y);
                    }));
                }

                if (targetObject is IRotateableYX && (targetObject as IRotateableYX).RotationX != 0.0f)
                {
                    Items.Add(new ToolStripMenuItem("Reset rotation (X axis)", null, (o, e) =>
                    {
                        EditorActions.ResetObjectRotation((PositionBasedObjectInstance)targetObject, RotationAxis.X);
                    }));
                }

                if (targetObject is IRotateableYXRoll && (targetObject as IRotateableYXRoll).Roll != 0.0f)
                {
                    Items.Add(new ToolStripMenuItem("Reset rotation (Roll axis)", null, (o, e) =>
                    {
                        EditorActions.ResetObjectRotation((PositionBasedObjectInstance)targetObject, RotationAxis.Roll);
                    }));
                }
            }

            var size = (targetObject as ISizeable)?.Size.Length();
            var scale = (targetObject as IScaleable)?.Scale;
            if (targetObject is PositionBasedObjectInstance &&
               ((scale.HasValue && scale != 1.0f) || (size.HasValue && size != Math.Sqrt(3))))
            {
                Items.Add(new ToolStripMenuItem("Reset scale", null, (o, e) =>
                {
                    EditorActions.ResetObjectScale((PositionBasedObjectInstance)targetObject);
                }));
            }

            // Get all triggers pointing to target object
            var triggers = _editor.Level.GetAllTriggersPointingToObject(targetObject);
            if (triggers.Count != 0)
            {
                if (!(Items[Items.Count - 1] is ToolStripSeparator))
                    Items.Add(new ToolStripSeparator());

                foreach (var trigger in triggers)
                {
                    var triggerItem = new ToolStripMenuItem("Trigger in room " + trigger.Room.Name,
                        null,
                        (o, e) =>
                        {
                            _editor.SelectRoom(trigger.Room);
                            _editor.MoveCameraToSector(trigger.Area.Start);
                            _editor.SelectedSectors = new SectorSelection() { Area = trigger.Area };
                        });
                    Items.Add(triggerItem);
                }
            }

            if (Items[Items.Count - 1] is ToolStripSeparator)
                Items.RemoveAt(Items.Count - 1);
        }
    }
}
