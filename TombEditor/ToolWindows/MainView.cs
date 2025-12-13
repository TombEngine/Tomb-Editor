using DarkUI.Controls;
using DarkUI.Docking;
using System;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Forms;
using TombLib.Utils;
using System.Linq;
using System.Collections.Generic;
using TombEditor.Forms;
using DarkUI.Config;
using TombLib.Wad.Catalog;

namespace TombEditor.ToolWindows
{
    public partial class MainView : DarkDocument
    {
        private readonly Editor _editor;
        private readonly List<ToolStripItem> _toolstripButtons = new List<ToolStripItem>();
        private readonly PopUpInfo popup = new PopUpInfo();

        public MainView()
        {
            InitializeComponent();

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;

            foreach (var button in toolStrip.Items)
                _toolstripButtons.Add((ToolStripItem)button);

            ClipboardEvents.ClipboardChanged += ClipboardEvents_ClipboardChanged;
            ClipboardEvents_ClipboardChanged(this, EventArgs.Empty);

            GenerateToolStripCommands(toolStrip.Items);
            UpdateToolStripLayout();
            RefreshControls(_editor.Configuration);
            UpdateStatistics();
        }

        public void InitializeRendering(RenderingDevice device)
        {
            panel3D.InitializeRendering(device, _editor.Configuration.Rendering3D_Antialias, (TombLib.Controls.ObjectRenderingQuality)_editor.Configuration.Rendering3D_ObjectQuality);
        }

        public void AddToolbox(DarkFloatingToolbox toolbox)
        {
            if(!panel3D.Contains(toolbox))
                panel3D.Controls.Add(toolbox);
        }

        public void RemoveToolbox(DarkFloatingToolbox toolbox)
        {
            if (panel3D.Controls.Contains(toolbox))
                panel3D.Controls.Remove(toolbox);
        }

        public void MoveObjectRelative(PositionBasedObjectInstance instance, Vector3 pos, Vector3 precision = new Vector3(), bool canGoOutsideRoom = false)
        {
            if (panel3D.Camera.RotationY < Math.PI * (1.0 / 4.0))
                EditorActions.MoveObjectRelative(instance, pos, precision, canGoOutsideRoom);
            else if (panel3D.Camera.RotationY < Math.PI * (3.0 / 4.0))
                EditorActions.MoveObjectRelative(instance, new Vector3(pos.Z, pos.Y, -pos.X), new Vector3(precision.Z, precision.Y, -precision.X), canGoOutsideRoom);
            else if (panel3D.Camera.RotationY < Math.PI * (5.0 / 4.0))
                EditorActions.MoveObjectRelative(instance, new Vector3(-pos.X, pos.Y, -pos.Z), new Vector3(-precision.X, precision.Y, -precision.Z), canGoOutsideRoom);
            else if (panel3D.Camera.RotationY < Math.PI * (7.0 / 4.0))
                EditorActions.MoveObjectRelative(instance, new Vector3(-pos.Z, pos.Y, pos.X), new Vector3(-precision.Z, precision.Y, precision.X), canGoOutsideRoom);
            else
                EditorActions.MoveObjectRelative(instance, pos, precision, canGoOutsideRoom);
        }

        public void MoveRoomRelative(Room room, VectorInt3 pos)
        {
            if (panel3D.Camera.RotationY < Math.PI * (1.0 / 4.0))
                EditorActions.MoveRooms(pos, room.Versions);
            else if (panel3D.Camera.RotationY < Math.PI * (3.0 / 4.0))
                EditorActions.MoveRooms(new VectorInt3(pos.Z, pos.Y, -pos.X), room.Versions); // valid
            else if (panel3D.Camera.RotationY < Math.PI * (5.0 / 4.0))
                EditorActions.MoveRooms(new VectorInt3(-pos.X, pos.Y, -pos.Z), room.Versions); // valid
            else if (panel3D.Camera.RotationY < Math.PI * (7.0 / 4.0))
                EditorActions.MoveRooms(new VectorInt3(-pos.Z, pos.Y, pos.X), room.Versions);
            else
                EditorActions.MoveRooms(pos, room.Versions);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _editor.EditorEventRaised -= EditorEventRaised;
                ClipboardEvents.ClipboardChanged -= ClipboardEvents_ClipboardChanged;
            }
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.StepHeightChangedEvent)
                UpdateStepHeightCombo();

            if (obj is Editor.StatisticsChangedEvent ||
                obj is Editor.ConfigurationChangedEvent)
            {
                UpdateStatistics();

                if (obj is Editor.ConfigurationChangedEvent)
                    panelStepHeightOptions.Visible = _editor.IsPreciseGeometryAllowed;
            }

            if (obj is Editor.ConfigurationChangedEvent)
            {
                var o = (Editor.ConfigurationChangedEvent)obj;

                if (o.UpdateKeyboardShortcuts)
                    GenerateToolStripCommands(toolStrip.Items, true);

                if (o.UpdateToolbarLayout)
                {
                    UpdateToolStripLayout();
                    GenerateToolStripCommands(toolStrip.Items, true);
                }

                RefreshControls(_editor.Configuration);
            }
            
            // Gray out menu options that do not apply
            if (obj is Editor.SelectedObjectChangedEvent)
            {
                ObjectInstance selectedObject = _editor.SelectedObject;
                butCopy.Enabled = selectedObject is ISpatial;
                butStamp.Enabled = selectedObject is ISpatial;
            }

            // Update editor mode
            if (obj is Editor.ModeChangedEvent)
            {
                ClipboardEvents_ClipboardChanged(this, EventArgs.Empty);

                EditorMode mode = ((Editor.ModeChangedEvent)obj).Current;
                butCenterCamera.Enabled = mode != EditorMode.Map2D;
                but2D.Checked = mode == EditorMode.Map2D;
                but3D.Checked = mode == EditorMode.Geometry;
                butLightingMode.Checked = mode == EditorMode.Lighting;
                butFaceEdit.Checked = mode == EditorMode.FaceEdit;

                panel2DMap.Visible = mode == EditorMode.Map2D;
                panel3D.Visible = mode == EditorMode.FaceEdit || mode == EditorMode.Geometry || mode == EditorMode.Lighting;
            }

            // Update flipmap toolbar button
            if (obj is Editor.SelectedRoomChangedEvent ||
                _editor.IsSelectedRoomEvent(obj as Editor.RoomPropertiesChangedEvent))
            {
                butFlipMap.Enabled = _editor.SelectedRoom.Alternated;
                butFlipMap.Checked = _editor.SelectedRoom.AlternateBaseRoom != null;
            }

            // Update portal opacity controls
            if (obj is Editor.ObjectChangedEvent ||
                obj is Editor.SelectedObjectChangedEvent)
            {
                var portal = _editor.SelectedObject as PortalInstance;
                butOpacityNone.Enabled =
                butOpacitySolidFaces.Enabled =
                butOpacityTraversableFaces.Enabled = portal != null;
				butMirror.Enabled = portal != null && _editor.Level.IsTombEngine;

                butOpacityNone.Checked = portal != null && portal.Opacity == PortalOpacity.None;
                butOpacitySolidFaces.Checked = portal != null && portal.Opacity == PortalOpacity.SolidFaces;
                butOpacityTraversableFaces.Checked = portal != null && portal.Opacity == PortalOpacity.TraversableFaces;

				butMirror.Checked = portal != null && portal.Effect == PortalEffectType.ClassicMirror;
            }

            // Dismiss any messages
            if (obj is Editor.LevelChangedEvent)
            {
                popup.Hide();
            }

            // Update version-specific controls
            if (obj is Editor.InitEvent ||
                obj is Editor.LevelChangedEvent ||
                obj is Editor.GameVersionChangedEvent)
            {
                butAddBoxVolume.Enabled    = _editor.Level.IsTombEngine;
                butAddSphereVolume.Enabled = _editor.Level.IsTombEngine;
                butDrawVolumes.Enabled     = _editor.Level.IsTombEngine; // We may safely hide it because it's not customizable
                butAddSprite.Enabled       = _editor.Level.Settings.GameVersion.Native() <= TRVersion.Game.TR2;

                panelStepHeightOptions.Visible = _editor.IsPreciseGeometryAllowed;
                UpdateStepHeightCombo();
            }

            if (obj is Editor.MessageEvent)
            {
                var msg = (Editor.MessageEvent)obj;
                PopUpInfo.Show(popup, msg.ForceInMainWindow ? null : FindForm(), panel3D, msg.Message, msg.Type);
            }

            if (obj is Editor.UndoStackChangedEvent)
            {
                var state = (Editor.UndoStackChangedEvent)obj;
                butUndo.Enabled = state.UndoPossible;
                butUndo.Image = state.UndoPossible && !state.UndoReversible ? Properties.Resources.general_undo_irreversible_16 : Properties.Resources.general_undo_16;
                butRedo.Enabled = state.RedoPossible;
            }

            // Suspend / resume rendering
            if (obj is Editor.SuspendRenderingEvent)
                panel3D.AllowRendering = false;
            if (obj is Editor.ResumeRenderingEvent)
                panel3D.AllowRendering = true;
        }

        private void ClipboardEvents_ClipboardChanged(object sender, EventArgs e)
        {
            butPaste.Enabled = _editor.Mode != EditorMode.Map2D && Clipboard.ContainsData(typeof(ObjectClipboardData).FullName);
        }
        
        private void RefreshControls(Configuration settings)
        {
            if (!settings.Rendering3D_ShowStatics && panel3D.ShowStatics)
                if (_editor.SelectedObject is StaticInstance) _editor.SelectedObject = null;

            if (!settings.Rendering3D_ShowMoveables && panel3D.ShowMoveables)
                if (_editor.SelectedObject is MoveableInstance) _editor.SelectedObject = null;

            if ((!settings.Rendering3D_ShowImportedGeometry && panel3D.ShowImportedGeometry) ||
                (!settings.Rendering3D_DisablePickingForImportedGeometry && panel3D.DisablePickingForImportedGeometry))
                if (_editor.SelectedObject is ImportedGeometryInstance) _editor.SelectedObject = null;

            if (!settings.Rendering3D_ShowGhostBlocks && panel3D.ShowGhostBlocks)
                if (_editor.SelectedObject is GhostBlockInstance) _editor.SelectedObject = null;

            if (!settings.Rendering3D_ShowVolumes && panel3D.ShowVolumes)
                if (_editor.SelectedObject is VolumeInstance) _editor.SelectedObject = null;

            if (!settings.Rendering3D_ShowOtherObjects && panel3D.ShowOtherObjects)
                if (_editor.SelectedObject is LightInstance ||
                    _editor.SelectedObject is CameraInstance ||
                    _editor.SelectedObject is FlybyCameraInstance ||
                    _editor.SelectedObject is SinkInstance ||
                    _editor.SelectedObject is SoundSourceInstance)
                    _editor.SelectedObject = null;

            panel3D.ShowPortals = butDrawPortals.Checked = settings.Rendering3D_ShowPortals;
            panel3D.ShowHorizon = butDrawHorizon.Checked = settings.Rendering3D_ShowHorizon;
            panel3D.ShowAllRooms = butDrawAllRooms.Checked = settings.Rendering3D_ShowAllRooms;
            panel3D.ShowRoomNames = butDrawRoomNames.Checked = settings.Rendering3D_ShowRoomNames;
            panel3D.ShowCardinalDirections = butDrawCardinalDirections.Checked = settings.Rendering3D_ShowCardinalDirections;
            panel3D.ShowIllegalSlopes = butDrawIllegalSlopes.Checked = settings.Rendering3D_ShowIllegalSlopes;
            panel3D.ShowMoveables = butDrawMoveables.Checked = settings.Rendering3D_ShowMoveables;
            panel3D.ShowStatics = butDrawStatics.Checked = settings.Rendering3D_ShowStatics;
            panel3D.ShowImportedGeometry = butDrawImportedGeometry.Checked = settings.Rendering3D_ShowImportedGeometry;
            panel3D.ShowGhostBlocks = butDrawGhostBlocks.Checked = settings.Rendering3D_ShowGhostBlocks;
            panel3D.ShowOtherObjects = butDrawOther.Checked = settings.Rendering3D_ShowOtherObjects;
            panel3D.ShowVolumes = butDrawVolumes.Checked = settings.Rendering3D_ShowVolumes;
            panel3D.ShowBoundingBoxes = butDrawBoundingBoxes.Checked = settings.Rendering3D_ShowBoundingBoxes;
            panel3D.ShowSlideDirections = butDrawSlideDirections.Checked = settings.Rendering3D_ShowSlideDirections;
            panel3D.ShowExtraBlendingModes = butDrawExtraBlendingModes.Checked = settings.Rendering3D_ShowExtraBlendingModes;
            panel3D.HideTransparentFaces = butHideTransparentFaces.Checked = settings.Rendering3D_HideTransparentFaces;
            panel3D.BilinearFilter = butBilinearFilter.Checked = settings.Rendering3D_BilinearFilter;
            panel3D.DisablePickingForImportedGeometry = butDisableGeometryPicking.Checked = settings.Rendering3D_DisablePickingForImportedGeometry;
            panel3D.DisablePickingForHiddenRooms = butDisableHiddenRoomPicking.Checked = settings.Rendering3D_DisablePickingForHiddenRooms;
            panel3D.ShowLightMeshes = butDrawLightRadius.Checked = settings.Rendering3D_ShowLightRadius;
            panel3D.ShowLightingWhiteTextureOnly = butDrawWhiteLighting.Checked = settings.Rendering3D_ShowLightingWhiteTextureOnly;
            panel3D.ShowRealTintForObjects = butDrawStaticTint.Checked = settings.Rendering3D_ShowRealTintForObjects;

            panel3D.Invalidate();

            panelStats.Visible = settings.UI_ShowStats;
        }

        private void UpdateToolStripLayout()
        {
            toolStrip.Items.Clear();

            foreach (var item in _editor.Configuration.UI_ToolbarButtons)
            {
                if (item == "|")
                    toolStrip.Items.Add(new ToolStripSeparator());
                else
                {
                    var key = "but" + item;
                    var btn = _toolstripButtons.FirstOrDefault(but => but.Name == key);
                    if (btn != null) toolStrip.Items.Add(btn);
                }
            }

            toolStrip.Invalidate(); // Just in case
        }

        private void GenerateToolStripCommands(ToolStripItemCollection items, bool onlyLabels = false)
        {
            foreach (object obj in items)
            {
                ToolStripDropDownButton subMenu = obj as ToolStripDropDownButton;

                if (subMenu != null && subMenu.HasDropDownItems)
                    GenerateToolStripCommands(subMenu.DropDownItems, onlyLabels);
                else
                {
                    var control = obj as ToolStripItem;
                    if (!string.IsNullOrEmpty(control.Tag?.ToString()))
                    {
                        var command = CommandHandler.GetCommand(control.Tag.ToString());
                        if (command != null)
                        {
                            if (!onlyLabels)
                                control.Click += (sender, e) => { command.Execute?.Invoke(new CommandArgs { Editor = _editor, Window = this }); };

                            var hotkeyLabel = string.Join(", ", _editor.Configuration.UI_Hotkeys[control.Tag.ToString()]);
                            var label = command.FriendlyName + (string.IsNullOrEmpty(hotkeyLabel) ? "" : " (" + hotkeyLabel + ")");

                            if (control is ToolStripMenuItem)
                                control.Text = label;
                            else
                                control.ToolTipText = label;
                        }
                    }
                }
            }
        }

        private void UpdateStepHeightCombo()
        {
            comboStepHeight.SelectedIndex = _editor.Configuration.Editor_StepHeight switch
            {
                32 => 0,
                64 => 1,
                128 => 2,
                256 => 3,
                _ => -1
            };
        }

        private void comboStepHeight_SelectedIndexChanged(object sender, EventArgs e)
        {
            _editor.Configuration.Editor_StepHeight = comboStepHeight.SelectedIndex switch
            {
                0 => 32,
                1 => 64,
                2 => 128,
                _ => 256
            };
        }

        private void toolStrip_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            var cMenu = new DarkContextMenu();
            cMenu.Items.Add(new ToolStripMenuItem("Customize...", null, (o, e2) =>
            {
                using (var f = new FormToolBarLayout(_editor, _toolstripButtons))
                    f.ShowDialog(this.FindForm());
            }));
            cMenu.Show(Cursor.Position, ToolStripDropDownDirection.BelowRight);
        }

        private void panel2DMap_DragEnter(object sender, DragEventArgs e)
        {
            if (EditorActions.DragDropFileSupported(e, true))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void panel2DMap_DragDrop(object sender, DragEventArgs e)
        {
            EditorActions.DragDropCommonFiles(e, FindForm());
        }

        private void UpdateStatistics()
        {
            if (_editor == null || _editor.Level == null || !_editor.Configuration.UI_ShowStats)
                return;

            var summary = _editor.Stats;
            var settings = _editor.Level.Settings;
            var lStats = summary.LevelStats;
            var rStats = summary.RoomStats;

            var limitWarning = string.Empty;

            tbStats.BackColor = Colors.GreyBackground;
            tbStats.ClearUndo();
            tbStats.Clear();

            // Room block

            tbStats.SelectionColor = Colors.DisabledText;
            tbStats.AppendText("Rooms: ");

            if (_editor.Level.VerticallyConnectedRooms.Count > TrCatalog.GetLimit(settings.GameVersion, Limit.RoomSafeCount))
            {
                tbStats.SelectionColor = Colors.BlueHighlight;
                limitWarning = "Vertically connected room count is exceeded.";
            }
            else if (_editor.Level.ExistingRooms.Count > TrCatalog.GetLimit(settings.GameVersion, Limit.RoomMaxCount))
            {
                tbStats.SelectionColor = Colors.BlueHighlight;
                limitWarning = "Maximum room count is exceeded.";
            }
            else
                tbStats.SelectionColor = Colors.DisabledText;

            tbStats.AppendText(summary.RoomCount + "  ");

            // Object block

            tbStats.SelectionColor = Colors.DisabledText;
            tbStats.AppendText("Objects: " + rStats.MoveableCount + " / ");

            if (lStats.MoveableCount > TrCatalog.GetLimit(settings.GameVersion, Limit.ItemSafeCount))
            {
                tbStats.SelectionColor = Colors.BlueHighlight;
                limitWarning += (string.IsNullOrEmpty(limitWarning) ? "" : "\n") + "Safe object count is exceeded.";
            }
            else if (lStats.MoveableCount > TrCatalog.GetLimit(settings.GameVersion, Limit.ItemMaxCount))
            {
                tbStats.SelectionColor = Colors.BlueHighlight;
                limitWarning += (string.IsNullOrEmpty(limitWarning) ? "" : "\n") + "Maximum object count is exceeded.";
            }
            else
                tbStats.SelectionColor = Colors.DisabledText;

            tbStats.AppendText(lStats.MoveableCount + "  ");

            // Statics / triggers block

            tbStats.SelectionColor = Colors.DisabledText;
            tbStats.AppendText("Statics: " + rStats.StaticCount + " / " + lStats.StaticCount + "  ");
            tbStats.AppendText("Triggers: " + rStats.TriggerCount + " / " + lStats.TriggerCount + "  ");

            // Lights block (we show only dynamic lights, because static lights are not in fact lights)

            tbStats.AppendText("Lights: ");

            if (rStats.DynLightCount > TrCatalog.GetLimit(settings.GameVersion, Limit.RoomLightCount))
            {
                tbStats.SelectionColor = Colors.BlueHighlight;
                limitWarning += (string.IsNullOrEmpty(limitWarning) ? "" : "\n") + "Maximum light count in current room is exceeded.";
            }
            else
                tbStats.SelectionColor = Colors.DisabledText;

            tbStats.AppendText(rStats.DynLightCount.ToString());
            tbStats.SelectionColor = Colors.DisabledText;
            tbStats.AppendText(" / " + lStats.DynLightCount + "  ");

            // Misc block

            tbStats.AppendText("Cameras: " + rStats.CameraCount + " / " + lStats.CameraCount + "  ");
            tbStats.AppendText("Flybys: " + rStats.FlybyCount + " / " + lStats.FlybyCount + "  ");

            // Room geometry block

            tbStats.AppendText("\nRoom vertices / faces: ");

            if (rStats.VertexCount > TrCatalog.GetLimit(settings.GameVersion, Limit.RoomVertexCount))
            {
                tbStats.SelectionColor = Colors.BlueHighlight;
                limitWarning += (string.IsNullOrEmpty(limitWarning) ? "" : "\n") + "Room vertex count is exceeded.";
            }
            else
                tbStats.SelectionColor = Colors.DisabledText;

            tbStats.AppendText(rStats.VertexCount + " / ");

            if (rStats.FaceCount > TrCatalog.GetLimit(settings.GameVersion, Limit.RoomFaceCount))
            {
                tbStats.SelectionColor = Colors.BlueHighlight;
                limitWarning += (string.IsNullOrEmpty(limitWarning) ? "" : "\n") + "Room face count is exceeded.";
            }
            else
                tbStats.SelectionColor = Colors.DisabledText;

            tbStats.AppendText(rStats.FaceCount + "  ");

            tbStats.SelectionColor = Colors.DisabledText;
            tbStats.AppendText("Last level output: ");

            if (summary.BoxCount.HasValue) // Don't add boxes / overlaps / texinfos if level wasnt compiled yet
            {
                // Boxes block
                if (summary.BoxCount >= TrCatalog.GetLimit(settings.GameVersion, Limit.BoxLimit))
                {
                    tbStats.SelectionColor = Colors.BlueHighlight;
                    limitWarning += (string.IsNullOrEmpty(limitWarning) ? "" : "\n") + "Box count is exceeded. Reduce level complexity.";
                }
                else
                    tbStats.SelectionColor = Colors.DisabledText;

                tbStats.AppendText((summary.BoxCount.HasValue ? summary.BoxCount.Value.ToString() : "?") + " boxes, ");

                // Overlaps block
                if (summary.OverlapCount >= TrCatalog.GetLimit(settings.GameVersion, Limit.OverlapLimit) - 2)
                {
                    tbStats.SelectionColor = Colors.BlueHighlight;
                    limitWarning += (string.IsNullOrEmpty(limitWarning) ? "" : "\n") + "Overlap count is exceeded. Reduce level complexity.";
                }
                else
                    tbStats.SelectionColor = Colors.DisabledText;

                tbStats.AppendText((summary.OverlapCount.HasValue ? summary.OverlapCount.Value.ToString() : "?") + " overlaps, ");

                // TexInfos block
                if (summary.TextureCount > TrCatalog.GetLimit(settings.GameVersion, Limit.TexInfos))
                {
                    tbStats.SelectionColor = Colors.BlueHighlight;
                    limitWarning += (string.IsNullOrEmpty(limitWarning) ? "" : "\n") + "TexInfo count is exceeded. Simplify level texturing.";
                }
                else
                    tbStats.SelectionColor = Colors.DisabledText;

                tbStats.AppendText((summary.TextureCount.HasValue ? summary.TextureCount.Value.ToString() : "?") + " texinfos");
            }
            else
                tbStats.AppendText("compile level");

            if (tbStats.AutoSize) 
                tbStats.AutoSize = false; // HACK: prevent further size updates

            toolTip.SetToolTip(tbStats, limitWarning);
        }
    }
}
