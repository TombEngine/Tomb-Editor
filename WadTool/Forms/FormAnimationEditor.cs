using DarkUI.Config;
using DarkUI.Controls;
using DarkUI.Extensions;
using DarkUI.Forms;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TombLib;
using TombLib.Forms;
using TombLib.GeometryIO;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Types;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace WadTool
{
    public partial class FormAnimationEditor : DarkForm
    {
        private enum SearchType
        {
            None,
            StateID,
            AnimNumber,
            Name,
            StateName
        }

        private bool _saved = false;
        private bool Saved
        {
            get { return _saved; }
            set
            {
                if (value == _saved) return;
                _saved = value;
                butTbSaveAllChanges.Enabled = !_saved;
                saveChangesToolStripMenuItem.Enabled = !_saved;
                Text = "Animation editor - " + _editor.Moveable.Id.ToString(_editor.Wad.GameVersion) + (!_saved ? "*" : "");
            }
        }

        private AnimationEditor _editor;  // Editor
        private DeviceManager _deviceManager; // Renderer

        // Player
        private Timer _timerPlayAnimation;
        private int _frameCount;
        private int _overallPlaybackCount = _materialIndexSwitchInterval; // To reset 1st time on playback
        private int _currentMaterialIndex;

        private static readonly int _materialIndexSwitchInterval = 30 * 3; // 3 seconds, 30 game frames
        private static readonly int _gridRecoveryWaitInterval = 30 * 2;    // 2 seconds
        private static readonly float _gridRecoveryStep = 1 / (30 * 0.5f); // 0.5 seconds

        private float _gridRecoveryCount;

        // Chained playback vars
        private int _chainedPlaybackSetPosRecoveryCount;
        private int _chainedPlaybackInitialAnim;
        private int _chainedPlaybackInitialCursorPos;
        private VectorInt2 _chainedPlaybackInitialSelection;
        // State change vars
        private int _chainedPlaybackIncomingAnimation = -1;
        private int _chainedPlaybackIncomingFrame = -1;
        private VectorInt2 _chainedPlaybackIncomingFrameRange = -VectorInt2.One;

        // Live update flag, used when transform is updated during playback or scrubbling
        private bool _allowUpdate = true;

        // Info
        private readonly PopUpInfo popup = new PopUpInfo();

        // Logger
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public FormAnimationEditor(WadToolClass tool, DeviceManager deviceManager, Wad2 wad, WadMoveableId id)
        {
            InitializeComponent();

            _editor = new AnimationEditor(tool, wad, id);
            _deviceManager = deviceManager;

            panelRendering.Configuration = _editor.Tool.Configuration;

            bool isTEN = _editor.Wad.GameVersion == TRVersion.Game.TombEngine;
            if (!isTEN && _editor.Tool.Configuration.AnimationEditor_SoundPreviewType > SoundPreviewType.Water)
                _editor.Tool.Configuration.AnimationEditor_SoundPreviewType = SoundPreviewType.Land;

            // Lock TEN-specific controls
            sectionBlending.Visible = isTEN;
            panelRootMotion.Visible = isTEN;

            // Update UI
            UpdateUIControls();
            UpdateReferenceLevelControls();
            PopulateComboStateID();

            // Initialize playback
            _timerPlayAnimation = new Timer() { Interval = 30 };
            _timerPlayAnimation.Tick += timerPlayAnimation_Tick;

            // Add custom event handler for direct editing of animcommands
            timeline.AnimCommandDoubleClick += new EventHandler<WadAnimCommand>(timeline_AnimCommandDoubleClick);

            // Initialize the panel
            WadMoveable skin;
            var skinId = new WadMoveableId(TrCatalog.GetMoveableSkin(_editor.Wad.GameVersion, id.TypeId));

            if (_editor.Wad.Moveables.ContainsKey(skinId))
                skin = _editor.Wad.Moveables[skinId];
            else
                skin = _editor.Wad.Moveables[id];

            panelRendering.InitializeRendering(_editor, _deviceManager, skin);

            RebuildAnimationsList();

            if (_editor.Animations.Count != 0)
                lstAnimations.SelectItem(0);

            // Update UI which is dependent on initial anim state
            UpdateTransformUI();
            PopulateMeshList();

            // Set window property handlers
            Configuration.ConfigureWindow(this, _editor.Tool.Configuration);

            _editor.Tool.EditorEventRaised += Tool_EditorEventRaised;

            Saved = true;
        }

        private void Tool_EditorEventRaised(IEditorEvent obj)
        {
            if (obj is WadToolClass.AnimationEditorMeshSelectedEvent ||
                obj is WadToolClass.AnimationEditorGizmoPickedEvent ||
                obj is WadToolClass.AnimationEditorAnimationChangedEvent ||
                obj is WadToolClass.AnimationEditorCurrentAnimationChangedEvent)
            {
                _editor.MadeChanges = false;
                UpdateTransformUI();
            }

            if (obj is WadToolClass.AnimationEditorAnimationChangedEvent ||
                obj is WadToolClass.AnimationEditorCurrentAnimationChangedEvent ||
                obj is WadToolClass.AnimationEditorAnimcommandChangedEvent)
            {
                timeline.Invalidate();
            }

            if (obj is WadToolClass.AnimationEditorMeshSelectedEvent)
            {
                var e = obj as WadToolClass.AnimationEditorMeshSelectedEvent;

                _allowUpdate = false;
                {
                    dgvBoundingMeshList.ClearSelection();

                    if (e != null && e.Mesh != null)
                    {
                        var index = e.Model.Meshes.IndexOf(e.Mesh);
                        dgvBoundingMeshList.Rows[index].Selected = true;
                        dgvBoundingMeshList.CurrentCell = dgvBoundingMeshList.Rows[index].Cells[0];
                    }
                }
                _allowUpdate = true;
            }

            if (obj is WadToolClass.AnimationEditorAnimationChangedEvent)
            {
                var e = obj as WadToolClass.AnimationEditorAnimationChangedEvent;
                if (e != null)
                {
                    var anim = lstAnimations.Items.FirstOrDefault(item => ((AnimationNode)item.Tag).Index == e.Animation.Index);
                    if (anim != null)
                    {
                        var index = lstAnimations.Items.IndexOf(anim);
                        var node = (AnimationNode)anim.Tag;

                        // Back-up cursor position
                        int cursorPos = -1;
                        if ((e.Animation.DirectXAnimation.KeyFrames.Count == node.DirectXAnimation.KeyFrames.Count) &&
                            (e.Animation.WadAnimation.FrameRate == node.WadAnimation.FrameRate))
                            cursorPos = timeline.Value;

                        lstAnimations.Items[index].Tag = e.Animation;
                        UpdateAnimLabel(((AnimationNode)anim.Tag));

                        if (e.Focus)
                        {
                            lstAnimations.SelectItem(index);

                            // Restore cursor position
                            if (cursorPos != -1)
                                timeline.Value = cursorPos;
                            else
                                timeline.Value = timeline.Minimum;
                        }

                        Saved = false;
                    }
                }
            }

            if (obj is WadToolClass.AnimationEditorGizmoPickedEvent)
            {
                Saved = false;
            }

            if (obj is WadToolClass.ReferenceLevelChangedEvent)
            {
                UpdateReferenceLevelControls();
            }

            if (obj is WadToolClass.UndoStackChangedEvent)
            {
                var stackEvent = (WadToolClass.UndoStackChangedEvent)obj;
                butTbUndo.Enabled = stackEvent.UndoPossible;
                butTbRedo.Enabled = stackEvent.RedoPossible;
                undoToolStripMenuItem.Enabled = stackEvent.UndoPossible;
                redoToolStripMenuItem.Enabled = stackEvent.RedoPossible;
                Saved = false;
            }

            if (obj is WadToolClass.AnimationEditorStateChangeEvent)
            {
                if (_timerPlayAnimation.Enabled && _editor.Tool.Configuration.AnimationEditor_ChainPlayback)
                {
                    var e = (WadToolClass.AnimationEditorStateChangeEvent)obj;

                    _chainedPlaybackIncomingAnimation = e.NextAnimation;
                    _chainedPlaybackIncomingFrame = e.NextFrame;
                    _chainedPlaybackIncomingFrameRange = e.FrameRange;
                }
            }

            if (obj is WadToolClass.MessageEvent)
            {
                var msg = (WadToolClass.MessageEvent)obj;
                PopUpInfo.Show(popup, this, panelRendering, msg.Message, msg.Type);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
                _editor.Tool.EditorEventRaised -= Tool_EditorEventRaised;
            }
            base.Dispose(disposing);
        }

        private void UpdateUIControls()
        {
            scrollGridToolStripMenuItem.Checked = _editor.Tool.Configuration.AnimationEditor_ScrollGrid;
            restoreGridHeightToolStripMenuItem.Checked = _editor.Tool.Configuration.AnimationEditor_RecoverGridAfterPositionChange;
            smoothAnimationsToolStripMenuItem.Checked = _editor.Tool.Configuration.AnimationEditor_SmoothAnimation;
            drawGizmoToolStripMenuItem.Checked = _editor.Tool.Configuration.AnimationEditor_ShowGizmo;
            drawGridToolStripMenuItem.Checked = _editor.Tool.Configuration.AnimationEditor_ShowGrid;
            drawCollisionBoxToolStripMenuItem.Checked = _editor.Tool.Configuration.AnimationEditor_ShowCollisionBox;
            drawSkinToolStripMenuItem.Checked = _editor.Tool.Configuration.AnimationEditor_ShowSkin;
            drawSkinToolStripMenuItem.Visible = _editor.Tool.DestinationWad.GameVersion == TRVersion.Game.TombEngine;

            if (_editor.Tool.Configuration.AnimationEditor_ChainPlayback)
                butTransportChained.Image = Properties.Resources.transport_chain_enabled_24;
            else
                butTransportChained.Image = Properties.Resources.transport_chain_disabled_24;

            if (_editor.Tool.ReferenceLevel != null &&
                _editor.Tool.Configuration.AnimationEditor_SoundPreview)
                butTransportSound.Image = Properties.Resources.transport_audio_24;
            else
                butTransportSound.Image = Properties.Resources.transport_mute_24;

            switch (_editor.Tool.Configuration.AnimationEditor_SoundPreviewType)
            {
                case SoundPreviewType.Land:
                    butTransportCondition.Image = Properties.Resources.transport_on_nothing_24;
                    break;
                case SoundPreviewType.LandWithMaterial:
                    butTransportCondition.Image = Properties.Resources.transport_on_land_24;
                    break;
                case SoundPreviewType.Water:
                    butTransportCondition.Image = Properties.Resources.transport_in_shallow_water_24;
                    break;
                case SoundPreviewType.Quicksand:
                    butTransportCondition.Image = Properties.Resources.transport_sand_24;
                    break;
                case SoundPreviewType.Underwater:
                    butTransportCondition.Image = Properties.Resources.transport_underwater_24;
                    break;
            }

            butTransportCondition.ToolTipText = "Sound preview type: " + _editor.Tool.Configuration.AnimationEditor_SoundPreviewType.ToString().SplitCamelcase();
        }

        private void UpdateReferenceLevelControls()
        {
            panelRendering.Level = _editor.Tool.ReferenceLevel;
            panelRendering.Invalidate();

            comboRoomList.ComboBox.Items.Clear();

            if (_editor.Tool.ReferenceLevel != null)
            {
                // Load rooms into the combo box
                comboRoomList.Enabled = true;
                comboRoomList.ComboBox.Items.Add("(select room)");

                foreach (var room in _editor.Tool.ReferenceLevel.Rooms)
                    if (room != null)
                        comboRoomList.ComboBox.Items.Add(room);

                // Enable sound transport
                butTransportSound.Enabled = true;
                butTransportCondition.Enabled = true;
            }
            else
            {
                comboRoomList.Enabled = false;

                // Disable sound transport
                butTransportSound.Enabled = false;
                butTransportCondition.Enabled = false;
            }
        }

        private void PopulateMeshList()
        {
            dgvBoundingMeshList.Rows.Clear();
            foreach (var bone in panelRendering.Model.Bones)
                dgvBoundingMeshList.Rows.Add(true, bone.Name);
        }

        private void SelectMeshesInMeshList(bool toggle = true)
        {
            foreach (DataGridViewRow row in dgvBoundingMeshList.Rows)
                row.Cells[0].Value = toggle;
        }

        private List<int> GetSelectedMeshList()
        {
            var result = new List<int>();
            foreach (DataGridViewRow row in dgvBoundingMeshList.Rows)
                if ((bool)row.Cells[0].Value)
                    result.Add(row.Index);

            return result;
        }

        private void RebuildAnimationsList()
        {
            lstAnimations.Items.Clear();

            // Try to filter by request

            int searchNumber = -1;
            var searchString = tbSearchAnimation.Text.Trim().ToLower();
            string finalSearchString = "";
            SearchType searchType = SearchType.None;

            if (!string.IsNullOrEmpty(searchString))
            {
                // Find possible tokens
                int tokenEndIndex = searchString.IndexOf(":");

                if (tokenEndIndex >= 0)
                {
                    string possibleToken = searchString.Substring(0, tokenEndIndex).ToLower();
                    if (!string.IsNullOrEmpty(possibleToken))
                    {
                        switch (possibleToken)
                        {
                            case "num":
                            case "n":
                            case "number":
                            case "anim":
                            case "animation":
                            case "a":
                                searchType = SearchType.AnimNumber;
                                break;

                            case "state":
                            case "s":
                            case "stateid":
                                searchType = SearchType.StateID;
                                break;

                            case "name":
                                searchType = SearchType.Name;
                                break;
                        }

                        // Force name search type if there's not a number after numerical token
                        if (!int.TryParse(searchString.Substring(tokenEndIndex + 1), out searchNumber) && searchType != SearchType.Name)
                        {
                            finalSearchString = searchString.Substring(tokenEndIndex + 1);
                            if (searchType == SearchType.StateID) searchType = SearchType.StateName;
                            else searchType = SearchType.Name;
                        }
                    }
                }
                else
                {
                    if (!int.TryParse(searchString, out searchNumber))
                    {
                        // If no numerical, always search as by name
                        if (searchType == SearchType.StateID) searchType = SearchType.StateName;
                        else searchType = SearchType.Name;
                        finalSearchString = searchString;
                    }
                    else
                        searchType = SearchType.StateID; // Otherwise prioritize state id
                }
            }

            // Scan for state ID by known names which are in combobox by this time
            if (searchType == SearchType.StateName)
            {
                var names = cmbStateID.Items.Cast<string>().ToList();
                var firstEntry = names.IndexOf(name => name.ToLower().Contains(finalSearchString));

                if (firstEntry >= 0)
                {
                    searchNumber = firstEntry;
                    searchType = SearchType.StateID; // Switch back to state ID search, as we found index.
                }
            }

            // Filter regarding request
            for (int i = 0; i < _editor.Animations.Count; i++)
            {
                switch (searchType)
                {
                    case SearchType.StateID:
                        if (searchNumber >= 0 && _editor.Animations[i].WadAnimation.StateId != searchNumber)
                            continue;
                        break;
                    case SearchType.AnimNumber:
                        if (searchNumber >= 0 && i != searchNumber)
                            continue;
                        break;
                    case SearchType.Name:
                        if (!_editor.Animations[i].WadAnimation.Name.ToLower().Contains(finalSearchString))
                            continue;
                        break;
                    default:
                        break;
                }

                lstAnimations.Items.Add(new DarkListItem() { Tag = _editor.Animations[i] });
                UpdateAnimLabel(_editor.Animations[i]);
            }

            // Try to restore selection, if failed, select first visible one
            if (_editor.Moveable.Animations.Count > 0)
            {
                if (_editor.CurrentAnim == null ||
                    (lstAnimations.Items.Count > 0 && UpdateAnimListSelection(_editor.CurrentAnim.Index) < 0))
                {
                    lstAnimations.SelectItem(0);
                    lstAnimations.EnsureVisible();
                }
            }
            else
                SelectAnimation(null); // No animations left in list, free last one remaining

            UpdateStatusLabel();
        }

        private void SelectAnimation(AnimationNode node)
        {
            bool sameAnimSameSize = _editor.CurrentAnim != null && node != null && _editor.CurrentAnim.Index == node.Index &&
                _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count == node.DirectXAnimation.KeyFrames.Count;

            var prevAnim = _editor.CurrentAnim;
            _editor.CurrentAnim = node;

            _allowUpdate = false;
            {
                if (node != null)
                {
                    tbName.Text = node.WadAnimation.Name;
                    nudFramerate.Value = node.WadAnimation.FrameRate;
                    nudEndFrame.Value = node.WadAnimation.EndFrame;
                    nudNextAnim.Value = node.WadAnimation.NextAnimation;
                    nudNextFrame.Value = node.WadAnimation.NextFrame;
                    nudStartVertVel.Value = (decimal)node.WadAnimation.StartVelocity;
                    nudEndVertVel.Value = (decimal)node.WadAnimation.EndVelocity;
                    nudStartHorVel.Value = (decimal)node.WadAnimation.StartLateralVelocity;
                    nudEndHorVel.Value = (decimal)node.WadAnimation.EndLateralVelocity;
                    nudBlendFrameCount.Value = (decimal)node.WadAnimation.BlendFrameCount;
                    bezierCurveEditor.Value = node.WadAnimation.BlendCurve;
                    cbBlendPreset.SelectedIndex = -1;

                    tbStateId.Text = node.WadAnimation.StateId.ToString();
                    UpdateStateChange();
                }

                timeline.Animation = node;

                OnKeyframesListChanged();

                // Preserve selection if anim is under same index and have same amount of frames
                if (!sameAnimSameSize)
                {
                    if (!_timerPlayAnimation.Enabled)
                        timeline.Value = 0;

                    SelectFrame();
                    timeline.ResetSelection();
                }
            }
            _allowUpdate = true;

            if (node?.DirectXAnimation.KeyFrames.Count > 0)
                panelRendering.Model.BuildAnimationPose(_editor.CurrentKeyFrame);

            // Continue scrolling the grid if we're in chain playback mode
            if (!(_editor.Tool.Configuration.AnimationEditor_ChainPlayback && _timerPlayAnimation.Enabled))
                panelRendering.GridPosition = Vector3.Zero;

            panelRendering.Invalidate();

            _editor.Tool.AnimationEditorCurrentAnimationChanged(prevAnim, _editor.CurrentAnim);
        }

        private void UpdateSelection()
        {
            _editor.Selection = timeline.Selection;
            UpdateStatusLabel();
        }

        private void SelectFrame(float k = -1)
        {
            if (!_editor.ValidAnimationAndFrames) return;

            int frameIndex = timeline.Value;

            if (frameIndex >= _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count)
                frameIndex = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1;

            _editor.CurrentFrameIndex = frameIndex;

            var keyFrame = _editor.CurrentAnim.DirectXAnimation.KeyFrames[frameIndex];

            if (k > 0)
            {
                var nextIndex = (frameIndex < _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1) ?
                                          frameIndex + 1 : frameIndex;
                k = Math.Min(k, 1);
                KeyFrame nextKeyFrame = _editor.CurrentAnim.DirectXAnimation.KeyFrames[nextIndex];
                panelRendering.Model.BuildAnimationPose(keyFrame, nextKeyFrame, k);
            }
            else
            {
                panelRendering.Model.BuildAnimationPose(keyFrame);
                OnKeyframesListChanged();
                UpdateTransformUI();
            }

            panelRendering.Invalidate();
            PreviewSounds();
        }

        private void SelectMesh(int index)
        {
            if (!_allowUpdate) return;

            if (index < 0)
                panelRendering.SelectedMesh = null;
            else
                panelRendering.SelectedMesh = panelRendering.Model.Meshes[index];

            UpdateTransformUI();
            panelRendering.Invalidate();
        }

        private void UpdateTransformUI()
        {
            if (_editor.CurrentKeyFrame == null)
                return;

            _allowUpdate = false;
            {
                var meshIndex = panelRendering.SelectedMesh == null ? 0 : panelRendering.Model.Meshes.IndexOf(panelRendering.SelectedMesh);

                nudRotX.Value = (decimal)MathC.RadToDeg(_editor.CurrentKeyFrame.Rotations[meshIndex].X);
                nudRotY.Value = (decimal)MathC.RadToDeg(_editor.CurrentKeyFrame.Rotations[meshIndex].Y);
                nudRotZ.Value = (decimal)MathC.RadToDeg(_editor.CurrentKeyFrame.Rotations[meshIndex].Z);
                nudTransX.Value = (decimal)_editor.CurrentKeyFrame.Translations[0].X;
                nudTransY.Value = (decimal)_editor.CurrentKeyFrame.Translations[0].Y;
                nudTransZ.Value = (decimal)_editor.CurrentKeyFrame.Translations[0].Z;
                if (cmbTransformMode.SelectedIndex == -1) cmbTransformMode.SelectedIndex = 0;

                nudBBoxMinX.Value = (decimal)_editor.CurrentKeyFrame.BoundingBox.Minimum.X;
                nudBBoxMinY.Value = (decimal)_editor.CurrentKeyFrame.BoundingBox.Minimum.Y;
                nudBBoxMinZ.Value = (decimal)_editor.CurrentKeyFrame.BoundingBox.Minimum.Z;
                nudBBoxMaxX.Value = (decimal)_editor.CurrentKeyFrame.BoundingBox.Maximum.X;
                nudBBoxMaxY.Value = (decimal)_editor.CurrentKeyFrame.BoundingBox.Maximum.Y;
                nudBBoxMaxZ.Value = (decimal)_editor.CurrentKeyFrame.BoundingBox.Maximum.Z;

                var boneName = panelRendering.Model.Bones[meshIndex].Name;
                if (string.IsNullOrEmpty(boneName)) boneName = "Bone " + meshIndex;
                panelTransform.SectionHeader = "Transform (" + boneName + ")";
            }
            _allowUpdate = true;
        }

        private void OnKeyframesListChanged()
        {
            if (_editor.ValidAnimationAndFrames)
            {
                _frameCount = timeline.Value * _editor.CurrentAnim.WadAnimation.FrameRate;
                timeline.Minimum = 0;
                timeline.Maximum = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1;
                nudEndFrame.Value = _editor.CurrentAnim.WadAnimation.EndFrame;
                UpdateStatusLabel();
            }
            else
                statusFrame.Text = string.Empty;
        }

        private void FixEndFrame(int delta)
        {
            var desiredEndFrame = Math.Max(0, _editor.CurrentAnim.WadAnimation.EndFrame + delta);
            _editor.CurrentAnim.WadAnimation.EndFrame = (ushort)desiredEndFrame;

            if (_editor.CurrentAnim.WadAnimation.EndFrame < 0)
                _editor.CurrentAnim.WadAnimation.EndFrame = 0;
        }

        private void CalculateAnimationBoundingBox(bool clear = false)
        {
            if (!_editor.ValidAnimationAndFrames) return;
            _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

            int start = 0;
            int end = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count;

            if (!timeline.SelectionIsEmpty)
            {
                start = timeline.Selection.X;
                end = timeline.Selection.Y + 1;
            }

            for (int i = start; i < end; i++)
                CalculateKeyframeBoundingBox(i, false, clear);

            // HACK: direct usage of DX keyframes all around WT anim editor
            // results in wrong animation pose after keyframe bb recalc. So we build it it once more.

            panelRendering.Model.BuildAnimationPose(_editor.CurrentKeyFrame);
            panelRendering.Invalidate();
        }

        private void ClearAnimationBoundingBox()
        {
            if (!_editor.ValidAnimationAndFrames) return;
            CalculateAnimationBoundingBox(true);
        }

        private void CalculateKeyframeBoundingBox(int index, bool undo, bool clear)
        {
            if (!_editor.ValidAnimationAndFrames) return;
            if (undo) _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

            var meshList = GetSelectedMeshList();
            var keyFrame = _editor.CurrentAnim.DirectXAnimation.KeyFrames[index];
            panelRendering.Model.BuildAnimationPose(keyFrame);

            if (clear || meshList.Count == 0)
                keyFrame.BoundingBox = new BoundingBox();
            else
                keyFrame.CalculateBoundingBox(panelRendering.Model, panelRendering.Skin, meshList);

            if (index == timeline.Value)
            {
                UpdateTransformUI();
                panelRendering.Invalidate();
            }
            Saved = false;
        }

        private void InflateFrameBoundingBox(int index, Vector3 value, bool undo)
        {
            if (!_editor.ValidAnimationAndFrames) return;
            if (value.Length() == 0f) return;

            if (undo) _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);
            _editor.CurrentAnim.DirectXAnimation.KeyFrames[index].BoundingBox = _editor.CurrentAnim.DirectXAnimation.KeyFrames[index].BoundingBox.Inflate(value);

            if (index == timeline.Value)
            {
                UpdateTransformUI();
                panelRendering.Invalidate();
            }
            Saved = false;
        }

        private void InflateAnimationBoundingBox(Vector3 value)
        {
            if (!_editor.ValidAnimationAndFrames) return;
            if (value.Length() == 0f) return;

            _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

            int start = 0;
            int end = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count;

            if (!timeline.SelectionIsEmpty)
            {
                start = timeline.Selection.X;
                end = timeline.Selection.Y + 1;
            }

            for (int i = start; i < end; i++)
                InflateFrameBoundingBox(i, value, false);
        }

        public void ResetEndFrame() => nudEndFrame.Value = _editor.GetRealNumberOfFrames() - 1;

        public void UpdateTransform()
        {
            if (!_allowUpdate || _editor.CurrentKeyFrame == null) return;

            var meshIndex = panelRendering.SelectedMesh == null ? 0 : panelRendering.Model.Meshes.IndexOf(panelRendering.SelectedMesh);

            _editor.UpdateTransform(meshIndex,
                new Vector3(MathC.DegToRad((float)nudRotX.Value), MathC.DegToRad((float)nudRotY.Value), MathC.DegToRad((float)nudRotZ.Value)),
                new Vector3((float)nudTransX.Value, (float)nudTransY.Value, (float)nudTransZ.Value));

            panelRendering.Model.BuildAnimationPose(_editor.CurrentKeyFrame);
            panelRendering.Invalidate();
        }

        private void AddNewAnimation()
        {
            var wadAnimation = new WadAnimation();
            wadAnimation.FrameRate = 1;
            wadAnimation.Name = "New Animation " + _editor.Animations.Count;

            var keyFrame = new WadKeyFrame();
            foreach (var bone in _editor.Moveable.Bones)
                keyFrame.Angles.Add(new WadKeyFrameRotation());
            wadAnimation.KeyFrames.Add(keyFrame);

            var dxAnimation = Animation.FromWad2(_editor.Moveable.Bones, wadAnimation);
            var node = new AnimationNode(wadAnimation, dxAnimation, _editor.Animations.Count);

            _editor.Animations.Add(node);
            lstAnimations.Items.Add(new DarkListItem() { Tag = node });
            UpdateAnimLabel(node);
            UpdateAnimListSelection(node.Index);

            tbName.Focus();

            Saved = false;
        }

        private void DeleteSelectedAnimations()
        {
            if (lstAnimations.SelectedItems.Count <= 0)
                return;

            if (DarkMessageBox.Show(this, "Do you really want to delete all selected animations?",
                                    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            var listToDelete = lstAnimations.SelectedItems.Select(i => (AnimationNode)i.Tag).ToList();

            for (int i = 0; i < listToDelete.Count; i++)
            {
                var anim = listToDelete[i];
                DeleteAnimation(anim, (i == listToDelete.Count - 1));
            }
        }

        private void DeleteAnimation()
        {
            if (_editor.CurrentAnim == null) return;

            if (DarkMessageBox.Show(this, "Do you really want to delete '" + _editor.CurrentAnim.WadAnimation.Name + "'?",
                                    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            DeleteAnimation(_editor.CurrentAnim);
        }

        private void DeleteAnimation(AnimationNode animToDelete, bool updateUI = true)
        {
            if (animToDelete == null || !_editor.Animations.Contains(animToDelete))
                return;

            int currentIndex = _editor.Animations.IndexOf(animToDelete);

            // Update all references
            for (int i = 0; i < _editor.Animations.Count; i++)
            {
                // Ignore the animation I'm deleting
                if (i == currentIndex)
                    continue;

                var animation = _editor.Animations[i];

                // Update own index
                if (animation.Index > currentIndex)
                    animation.Index--;

                // Update NextAnimation
                if (animation.WadAnimation.NextAnimation > currentIndex)
                    animation.WadAnimation.NextAnimation--;

                // Update state changes
                foreach (var stateChange in animation.WadAnimation.StateChanges)
                    foreach (var dispatch in stateChange.Dispatches)
                        if (dispatch.NextAnimation > currentIndex)
                            dispatch.NextAnimation--;
            }

            // Remove the animation
            _editor.Animations.Remove(animToDelete);

            if (updateUI)
            {
                // Update GUI
                RebuildAnimationsList();
                panelRendering.Invalidate();
                timeline.Invalidate();

                Saved = false;
            }
        }

        private void AddNewFrame(int index, bool undo)
        {
            if (_editor.CurrentAnim == null) return;
            if (undo) _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

            KeyFrame keyFrame;

            if (_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count == 0)
            {
                index = 0;
                keyFrame = new KeyFrame();
                foreach (var bone in _editor.Moveable.Bones)
                {
                    keyFrame.Rotations.Add(Vector3.Zero);
                    keyFrame.Quaternions.Add(Quaternion.Identity);
                    keyFrame.Translations.Add(bone.Translation);
                    keyFrame.TranslationsMatrices.Add(Matrix4x4.CreateTranslation(bone.Translation));
                }
            }
            else
                keyFrame = _editor.CurrentKeyFrame.Clone();

            _editor.CurrentAnim.DirectXAnimation.KeyFrames.Insert(index, keyFrame);
            OnKeyframesListChanged();
            FixEndFrame(1);
            Saved = false;
        }

        private void InsertMultipleFrames()
        {
            if (_editor.CurrentAnim == null) return;

            using (var form = new FormInputBox("Add frames", "How many frames to add?", "1"))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

                    int framesCount = 0;
                    if (!int.TryParse(form.Result, out framesCount) || framesCount <= 0)
                    {
                        popup.ShowError(panelRendering, "You must insert a number greater than 1");
                        return;
                    }

                    // Add frames
                    for (int i = 0; i < framesCount; i++)
                        AddNewFrame(timeline.Value + 1, false);
                }
            }
        }

        private void DeleteFrames(IWin32Window owner, bool undo, bool updateGUI) // No owner = no warnings!
        {
            if (!ValidAndSelected(owner != null)) return;

            if (owner != null &&
                DarkMessageBox.Show(this, "Do you really want to delete frame" +
                (timeline.SelectionSize == 1 ? " " + timeline.Value : "s " + timeline.Selection.X + "-" + timeline.Selection.Y) + "?",
                                    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            if (undo) _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

            int selectionStart = timeline.Selection.X; // Save last index
            int selectionEnd = timeline.Selection.Y;

            // Save cursor position to restore later. Only do it if cursor is outside selection.
            int cursorPos = (timeline.Value < timeline.Selection.X || timeline.Value > timeline.Selection.Y) ? timeline.Value : -1;

            // Remove the frames
            _editor.CurrentAnim.DirectXAnimation.KeyFrames.RemoveRange(timeline.Selection.X, timeline.SelectionSize);

            Saved = false;

            if (!updateGUI) return;

            FixEndFrame(-timeline.SelectionSize);

            // Update GUI
            timeline.ResetSelection();
            OnKeyframesListChanged();

            if (_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count != 0)
            {
                int insertEnd = selectionStart + _editor.ClipboardKeyFrames.Count - 1;

                if (cursorPos != -1)
                {
                    if (cursorPos < selectionStart)
                        timeline.Value = cursorPos;
                    else
                        timeline.Value = selectionStart + (cursorPos - selectionEnd - 1);
                }
                else
                    timeline.Value = selectionStart;
            }
            else
            {
                timeline.Value = 0;
                statusFrame.Text = string.Empty;
            }
            panelRendering.Invalidate();
        }

        private void CutFrames()
        {
            if (!ValidAndSelected()) return;

            CopyFrames(false);
            DeleteFrames(null, true, true);
        }

        private void CopyFrames(bool updateGUI = true)
        {
            if (!ValidAndSelected()) return;

            _editor.ClipboardKeyFrames.Clear();
            for (int i = timeline.Selection.X; i <= timeline.Selection.Y; i++)
                _editor.ClipboardKeyFrames.Add(_editor.CurrentAnim.DirectXAnimation.KeyFrames[i].Clone());

            if (updateGUI)
                timeline.Highlight(timeline.Selection.X, timeline.Selection.Y);
        }

        private void PasteFrames()
        {
            if (_editor.CurrentAnim == null) return;
            if (_editor.ClipboardKeyFrames == null || _editor.ClipboardKeyFrames.Count <= 0)
            {
                popup.ShowWarning(panelRendering, "Nothing to paste!");
                return;
            }

            _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

            int startIndex = timeline.SelectionIsEmpty ? timeline.Value : timeline.Selection.X; // Save last index
            int endIndex = timeline.SelectionIsEmpty ? timeline.Value : timeline.Selection.Y;

            // Save cursor position to restore later. Only do it if cursor is outside selection.
            int cursorPos = (timeline.SelectionIsEmpty || timeline.Value < timeline.Selection.X || timeline.Value > timeline.Selection.Y) ? timeline.Value : -1;

            // Replace if there is a selection.
            if (!timeline.SelectionIsEmpty)
                DeleteFrames(null, false, false);

            // Re-clone clipboard data to avoid corruption
            var pastedFrames = new List<KeyFrame>();
            _editor.ClipboardKeyFrames.ForEach(frame => pastedFrames.Add(frame.Clone()));
            _editor.CurrentAnim.DirectXAnimation.KeyFrames.InsertRange(startIndex, pastedFrames);
            OnKeyframesListChanged();
            FixEndFrame(pastedFrames.Count - timeline.SelectionSize);

            int insertEnd = startIndex + _editor.ClipboardKeyFrames.Count - 1;
            if (cursorPos != -1 && cursorPos <= timeline.Maximum)
            {
                if (cursorPos < startIndex)
                    timeline.Value = cursorPos;
                else
                    timeline.Value = insertEnd + (cursorPos - endIndex) + (timeline.SelectionIsEmpty ? 1 : 0);
            }
            else
                timeline.Value = insertEnd;

            timeline.ResetSelection();
            timeline.Highlight(startIndex, insertEnd);
            panelRendering.Invalidate();
            Saved = false;
        }

        private void CutAnimation()
        {
            if (_editor.CurrentAnim == null)
            {
                popup.ShowWarning(panelRendering, "No animation to cut!");
                return;
            }

            _editor.ClipboardNode = _editor.CurrentAnim;
            DeleteAnimation();
        }

        private void CopyAnimation()
        {
            if (_editor.CurrentAnim == null)
            {
                popup.ShowWarning(panelRendering, "No animation to copy!");
                return;
            }

            _editor.ClipboardNode = _editor.CurrentAnim.Clone();
        }

        private void PasteAnimation()
        {
            if (_editor.ClipboardNode == null || _editor.CurrentAnim == null)
            {
                popup.ShowWarning(panelRendering, "No animation to paste!");
                return;
            }

            int animationIndex = _editor.Animations.Count;
            var pastedAnim = _editor.ClipboardNode.Clone(animationIndex);
            _editor.Animations.Add(pastedAnim);
            pastedAnim.WadAnimation.Name += " - Copy";
            RebuildAnimationsList();
            UpdateAnimListSelection(animationIndex);
            Saved = false;
        }

        private void ReplaceAnimation()
        {
            if (_editor.ClipboardNode == null || _editor.CurrentAnim == null)
            {
                popup.ShowWarning(panelRendering, "No animation to replace!");
                return;
            }

            _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

            int animationIndex = _editor.CurrentAnim.Index;
            var pastedAnim = _editor.ClipboardNode.Clone(animationIndex);
            _editor.Animations[animationIndex] = pastedAnim;
            pastedAnim.WadAnimation.Name += " - Copy";
            RebuildAnimationsList();
            UpdateAnimListSelection(animationIndex);
            Saved = false;
        }

        private void SplitAnimation()
        {
            if (_editor.CurrentAnim == null)
            {
                popup.ShowWarning(panelRendering, "No animation to split!");
                return;
            }

            // I need at least 1 frame in the middle
            if (_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count < 3)
            {
                popup.ShowError(panelRendering, "You must have at least 3 frames for splitting the animation");
                return;
            }

            // Check if we have selected the first or the last frame
            int numFrames = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count;
            if (timeline.Value == 0 || timeline.Value == numFrames - 1)
            {
                popup.ShowError(panelRendering, "You can't set the first or the last frame for splitting the animation");
                return;
            }
            var newWadAnimation = _editor.CurrentAnim.WadAnimation.Clone();
            var newDirectXAnimation = _editor.CurrentAnim.DirectXAnimation.Clone();

            int numFrames1 = timeline.Value;
            int numFrames2 = numFrames - numFrames1;

            // Remove frames from the two animations
            _editor.CurrentAnim.DirectXAnimation.KeyFrames.RemoveRange(timeline.Value + 1, numFrames2 - 1);
            newDirectXAnimation.KeyFrames.RemoveRange(0, timeline.Value);

            // Add the new animation at the bottom of the list
            newWadAnimation.Name += " - splitted";
            _editor.Animations.Add(new AnimationNode(newWadAnimation, newDirectXAnimation, _editor.Animations.Count));

            // Update the GUI
            RebuildAnimationsList();
            if (UpdateAnimListSelection(_editor.Animations.Count - 1) < 0)
            {
                // Just update timeline if animation is invisible in the list
                timeline.ResetSelection();
                timeline.Value = 0;
                timeline.Invalidate();
            }

            Saved = false;
        }

        private void UpdateStateChange(bool getFromCombo = false)
        {
            if (_editor.CurrentAnim == null) return;

            bool decodeFailed = false;
            ushort oldValue = _editor.CurrentAnim.WadAnimation.StateId;
            ushort newValue = ushort.MaxValue; // Let's hope nobody will ever use state ID 65535...

            // ID decoding from textbox is not needed in case textbox field was forced manually (eg from combobox).
            if (getFromCombo)
                tbStateId.Text = (string)cmbStateID.SelectedItem;

            // Try to identify if there was a string input in textbox.
            if (getFromCombo || !ushort.TryParse(tbStateId.Text, out newValue))
            {
                string searchString = tbStateId.Text.Trim();
                int possibleID = -1;

                for (int i = 0; i < 2; i++)
                {
                    possibleID = TrCatalog.TryToGetStateID(_editor.Wad.GameVersion, _editor.Moveable.Id.TypeId, searchString);
                    if (possibleID >= 0) break;
                    // Try to do one more pass with spaces replaced by underlines
                    searchString = searchString.Replace(' ', '_');
                }

                // String input. Try to find appropriate name in trcatalog.
                if (possibleID >= 0)
                    newValue = (ushort)possibleID; // Name found, use found state ID
                else
                {
                    popup.ShowError(panelRendering, "State ID with given name wasn't found.");
                    decodeFailed = true;
                    newValue = oldValue; // Decoding failed, reset state ID to old value
                }
            }

            // Restore state name to textbox, if possible.
            if (newValue != ushort.MaxValue)
            {
                var possibleName = TrCatalog.GetStateName(_editor.Wad.GameVersion, _editor.Moveable.Id.TypeId, newValue);

                // Put name into textbox, if found among state numbers. Otherwise, use raw number.
                if (cmbStateID.Items.Contains(possibleName))
                    tbStateId.Text = possibleName;
                else
                    tbStateId.Text = newValue.ToString();
            }

            // Add legacy numerical ID as a tooltip
            if (!decodeFailed)
                toolTip1.SetToolTip(tbStateId, "State ID = " + newValue);

            // Don't update if not changed or parse failed
            if (oldValue == newValue || newValue == ushort.MaxValue)
                return;

            if (!_editor.MadeChanges)
            {
                _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);
                _editor.MadeChanges = true;
            }

            _editor.CurrentAnim.WadAnimation.StateId = newValue;
            Saved = false;
        }

        private void PopulateComboStateID()
        {
            cmbStateID.Items.Clear();
            tbStateId.AutocompleteWords.Clear();

            for (uint i = 0; i < 256; i++) // Max state value in TR5 was 137 but just in case...
            {
                string name = TrCatalog.GetStateName(_editor.Wad.GameVersion, _editor.Moveable.Id.TypeId, i);
                if (name.Contains("Unknown"))
                    continue;
                else
                {
                    cmbStateID.Items.Add(name);
                    tbStateId.AutocompleteWords.Add(name);
                }
            }
        }

        private void ValidateAnimationParameter() => _editor.MadeChanges = false;
        private void UpdateAnimationParameter(Control control)
        {
            if (!_allowUpdate || _editor.CurrentAnim == null || _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count == 0) return;

            float result = 0;
            if (control is DarkTextBox)
            {
                string toParse = ((DarkTextBox)control).Text.Replace(",", ".");
                if (!float.TryParse(toParse, out result))
                    result = 0;
            }
            else if (control is DarkNumericUpDown)
                result = (float)((DarkNumericUpDown)control).Value;

            var bb = _editor.CurrentAnim.DirectXAnimation.KeyFrames[timeline.Value].BoundingBox;

            float oldValue = 0;
            bool roundToByte = false;
            bool roundToShort = false;

            // Get actual old value
            switch (control.Name)
            {
                case nameof(nudNextAnim):
                    oldValue = _editor.CurrentAnim.WadAnimation.NextAnimation;
                    roundToShort = true;
                    break;
                case nameof(nudNextFrame):
                    oldValue = _editor.CurrentAnim.WadAnimation.NextFrame;
                    roundToShort = true;
                    break;
                case nameof(nudFramerate):
                    oldValue = _editor.CurrentAnim.WadAnimation.FrameRate;
                    roundToByte = true;
                    break;
                case nameof(nudEndFrame):
                    oldValue = _editor.CurrentAnim.WadAnimation.EndFrame;
                    roundToShort = true;
                    break;
                case nameof(nudStartVertVel):
                    oldValue = _editor.CurrentAnim.WadAnimation.StartVelocity;
                    break;
                case nameof(nudEndVertVel):
                    oldValue = _editor.CurrentAnim.WadAnimation.EndVelocity;
                    break;
                case nameof(nudStartHorVel):
                    oldValue = _editor.CurrentAnim.WadAnimation.StartLateralVelocity;
                    break;
                case nameof(nudEndHorVel):
                    oldValue = _editor.CurrentAnim.WadAnimation.EndLateralVelocity;
                    break;
                case nameof(nudBBoxMinX):
                    oldValue = bb.Minimum.X;
                    break;
                case nameof(nudBBoxMinY):
                    oldValue = bb.Minimum.Y;
                    break;
                case nameof(nudBBoxMinZ):
                    oldValue = bb.Minimum.Z;
                    break;
                case nameof(nudBBoxMaxX):
                    oldValue = bb.Maximum.X;
                    break;
                case nameof(nudBBoxMaxY):
                    oldValue = bb.Maximum.Y;
                    break;
                case nameof(nudBBoxMaxZ):
                    oldValue = bb.Maximum.Z;
                    break;
                case nameof(nudBlendFrameCount):
                    oldValue = _editor.CurrentAnim.WadAnimation.BlendFrameCount;
                    break;
            }

            // Clamp if necessary
            if (roundToByte)
                oldValue = (byte)MathC.Clamp(oldValue, 1, 255);
            else if (roundToShort)
                oldValue = (ushort)oldValue;

            // Fix visible values
            if (control is DarkTextBox) ((DarkTextBox)control).Text = result.ToString();
            else if (control is DarkNumericUpDown) ((DarkNumericUpDown)control).Value = (decimal)result;

            // Don't update if not changed
            if (oldValue == result)
                return;

            if (!_editor.MadeChanges)
            {
                _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);
                _editor.MadeChanges = true;
            }

            // Update actual values
            switch (control.Name)
            {
                case nameof(nudNextAnim):
                    _editor.CurrentAnim.WadAnimation.NextAnimation = (ushort)result;
                    break;
                case nameof(nudNextFrame):
                    _editor.CurrentAnim.WadAnimation.NextFrame = (ushort)result;
                    break;
                case nameof(nudFramerate):
                    nudEndFrame.Value = (decimal)Math.Round(_editor.CurrentAnim.WadAnimation.EndFrame /
                                                           (_editor.CurrentAnim.WadAnimation.FrameRate / (float)result));
                    _editor.CurrentAnim.WadAnimation.FrameRate = (byte)result;
                    break;
                case nameof(nudEndFrame):
                    _editor.CurrentAnim.WadAnimation.EndFrame = (ushort)result;
                    break;
                case nameof(nudStartVertVel):
                    _editor.CurrentAnim.WadAnimation.StartVelocity = result;
                    break;
                case nameof(nudEndVertVel):
                    _editor.CurrentAnim.WadAnimation.EndVelocity = result;
                    break;
                case nameof(nudStartHorVel):
                    _editor.CurrentAnim.WadAnimation.StartLateralVelocity = result;
                    break;
                case nameof(nudEndHorVel):
                    _editor.CurrentAnim.WadAnimation.EndLateralVelocity = result;
                    break;
                case nameof(nudBlendFrameCount):
                    _editor.CurrentAnim.WadAnimation.BlendFrameCount = (ushort)result;
                    break;
                case nameof(nudBBoxMinX):
                    EditBoundingBox(new Vector3(result, bb.Minimum.Y, bb.Minimum.Z), bb.Maximum);
                    break;
                case nameof(nudBBoxMinY):
                    EditBoundingBox(new Vector3(bb.Minimum.X, result, bb.Minimum.Z), bb.Maximum);
                    break;
                case nameof(nudBBoxMinZ):
                    EditBoundingBox(new Vector3(bb.Minimum.X, bb.Minimum.Y, result), bb.Maximum);
                    break;
                case nameof(nudBBoxMaxX):
                    EditBoundingBox(bb.Minimum, new Vector3(result, bb.Maximum.Y, bb.Maximum.Z));
                    break;
                case nameof(nudBBoxMaxY):
                    EditBoundingBox(bb.Minimum, new Vector3(bb.Maximum.X, result, bb.Maximum.Z));
                    break;
                case nameof(nudBBoxMaxZ):
                    EditBoundingBox(bb.Minimum, new Vector3(bb.Maximum.X, bb.Maximum.Y, result));
                    break;
            }

            Saved = false;
            timeline.Invalidate();
            panelRendering.Invalidate();
        }

        private void EditBoundingBox(Vector3 newMinimum, Vector3 newMaximum)
        {
            var bb = _editor.CurrentAnim.DirectXAnimation.KeyFrames[timeline.Value].BoundingBox;
            var deltaMin = newMinimum - bb.Minimum;
            var deltaMax = newMaximum - bb.Maximum;

            int start = timeline.Value;
            int end = timeline.Value;

            if (!_editor.SelectionIsEmpty)
            {
                start = _editor.Selection.X;
                end = _editor.Selection.Y;
            }

            for (int i = start; i <= end; i++)
            {
                var bb2 = _editor.CurrentAnim.DirectXAnimation.KeyFrames[i].BoundingBox;
                bb2.Minimum += deltaMin;
                bb2.Maximum += deltaMax;
                _editor.CurrentAnim.DirectXAnimation.KeyFrames[i].BoundingBox = bb2;
            }
        }

        private void InterpolateFrames(int frameIndex1, int frameIndex2, int numFrames, bool updateGUI = true)
        {
            var frame1 = _editor.CurrentAnim.DirectXAnimation.KeyFrames[frameIndex1];
            var frame2 = _editor.CurrentAnim.DirectXAnimation.KeyFrames[frameIndex2];

            // Cut existing frames between 1 and 2

            if (frameIndex2 - frameIndex1 > 1)
                _editor.CurrentAnim.DirectXAnimation.KeyFrames.RemoveRange(frameIndex1 + 1, frameIndex2 - frameIndex1 - 1);

            // Now calculate how many frames I must insert
            for (int i = 0; i < numFrames; i++)
            {
                var keyFrame = new KeyFrame();
                foreach (var bone in _editor.Moveable.Bones)
                {
                    keyFrame.Rotations.Add(Vector3.Zero);
                    keyFrame.Quaternions.Add(Quaternion.Identity);
                    keyFrame.Translations.Add(frame1.Translations[0] + bone.Translation);
                    keyFrame.TranslationsMatrices.Add(Matrix4x4.CreateTranslation(frame1.Translations[0] + bone.Translation));
                }
                _editor.CurrentAnim.DirectXAnimation.KeyFrames.Insert(frameIndex1 + 1 + i, keyFrame);
            }

            // Slerp factor
            float k = 1.0f / (numFrames + 1);

            // Now I have the right number of frames and I can do slerp
            for (int i = 0; i < numFrames; i++)
            {
                var keyframe = _editor.CurrentAnim.DirectXAnimation.KeyFrames[frameIndex1 + i + 1];

                // Lerp translation of root bone
                keyframe.Translations[0] = Vector3.Lerp(frame1.Translations[0], frame2.Translations[0], k * (i + 1));
                keyframe.TranslationsMatrices[0] = Matrix4x4.CreateTranslation(keyframe.Translations[0]);

                // Slerp of quaternions
                for (int j = 0; j < keyframe.Quaternions.Count; j++)
                {
                    keyframe.Quaternions[j] = Quaternion.Slerp(frame1.Quaternions[j], frame2.Quaternions[j], k * (i + 1));
                    keyframe.Rotations[j] = MathC.QuaternionToEuler(keyframe.Quaternions[j]);
                }

                // Nicely lerp bounding box as well
                var bbMin = Vector3.Lerp(frame1.BoundingBox.Minimum, frame2.BoundingBox.Minimum, k * (i + 1));
                var bbMax = Vector3.Lerp(frame1.BoundingBox.Maximum, frame2.BoundingBox.Maximum, k * (i + 1));
                keyframe.BoundingBox = new BoundingBox(bbMin, bbMax);
            }

            Saved = false;
            FixEndFrame(numFrames);

            if (updateGUI)
            {
                // All done! Now I reset a bit the GUI
                OnKeyframesListChanged();
                timeline.Highlight(frameIndex1, frameIndex1 + numFrames + 1);
                timeline.ResetSelection();
                timeline.Value = frameIndex1 + numFrames + 1;
                popup.ShowInfo(panelRendering, "Successfully inserted " + numFrames + " interpolated frames between frames " + frameIndex1 + " and " + frameIndex2);
            }
        }

        private void InterpolateFrames(int numFrames = -1)
        {
            if (!ValidAndSelected()) return;

            if (numFrames < 0)
            {
                var selection = MathC.Clamp((timeline.Selection.Y - timeline.Selection.X - 1), 0, int.MaxValue).ToString();
                using (var inputBox = new FormInputBox("Interpolation", "Enter number of interpolated frames:", selection) { Width = 300 })
                {
                    if (inputBox.ShowDialog(this) == DialogResult.Cancel)
                        return;

                    if (!int.TryParse(inputBox.Result, out numFrames))
                        numFrames = 3; // Default value
                }
            }
            else if (numFrames == 0)
            {
                popup.ShowError(panelRendering, "Interpolation requires at least 1 frame to insert");
                return;
            }

            _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

            int start = timeline.Selection.X;
            int end = (timeline.SelectionSize == 1 && timeline.Selection.Y == timeline.Maximum) ? timeline.Minimum : timeline.Selection.Y;

            InterpolateFrames(start, end, numFrames);
        }
        private void DeleteEveryNFrame(int n)
        {
            for (int i = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1; i >= 0; i -= n)
                _editor.CurrentAnim.DirectXAnimation.KeyFrames.RemoveAt(i);
        }

        private void InterpolateAnimation(int numFrames, bool fixAnimCommands, bool updateGUI = true)
        {
            int stepCount = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1;
            if (stepCount <= 0) return;

            if (numFrames <= 0) return;

            _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

            for (int i = 0; i < stepCount; i++)
            {
                int startFrame = i * (numFrames + 1);
                InterpolateFrames(startFrame, startFrame + 1, numFrames, false);
            }

            if (fixAnimCommands && _editor.CurrentAnim.WadAnimation.AnimCommands.Count > 0)
            {
                foreach (var ac in _editor.CurrentAnim.WadAnimation.AnimCommands)
                {
                    if (ac.Type >= WadAnimCommandType.PlaySound)
                        ac.Parameter1 *= (short)(numFrames + 1);
                }
            }

            Saved = false;

            if (updateGUI)
            {
                UpdateAnimListSelection(_editor.CurrentAnim.Index);
                timeline.Highlight();
            }
        }

        private void ReverseAnimation()
        {
            if (!_editor.ValidAnimationAndFrames) return;

            _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

            int start = 0;
            int end = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count;

            if (!_editor.SelectionIsEmpty)
            {
                start = _editor.Selection.X;
                end = _editor.Selection.Y;
            }

            _editor.CurrentAnim.DirectXAnimation.KeyFrames.Reverse(start, end - start);

            // Update animation commands
            foreach (var ac in _editor.CurrentAnim.WadAnimation.AnimCommands)
                if (ac.FrameBased && ac.Parameter1 >= start && ac.Parameter1 <= end)
                    ac.Parameter1 = (short)((_editor.CurrentAnim.WadAnimation.FrameRate * end) - (ac.Parameter1 - (_editor.CurrentAnim.WadAnimation.FrameRate * start)));

            Saved = false;

            timeline.Highlight(start, end);
            SelectFrame(); // We need to forcefully update current frame because frame number wasn't changed
            UpdateTransformUI();
        }
        private void MirrorAnimation()
        {
            if (!_editor.ValidAnimationAndFrames) return;

            int start = 0;
            int end = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1;

            if (!_editor.SelectionIsEmpty)
            {
                start = _editor.Selection.X;
                end = _editor.Selection.Y;
            }

            // FIXME: Try to account for messed up Y/Z rotation relation, but it won't help here anyway for now...
            // Calculate average rotation of the animation. If it's 90 degrees, let's flip Z axis.
            float accumulatedRotation = 0;
            foreach (var kf in _editor.CurrentAnim.DirectXAnimation.KeyFrames)
                accumulatedRotation += Math.Abs((Math.Round(MathC.RadToDeg(kf.Rotations[0]).Y / 90.0f) * 90.0f) % 180.0f) != 0 ? 1 : 0;

            accumulatedRotation /= _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count;
            bool flipZ = accumulatedRotation > 0.5f;

            var bonePairs = panelRendering.Model.GetBonePairs(flipZ);

            if (bonePairs == null || bonePairs.Count == 0)
            {
                popup.ShowError(panelRendering, "No valid bones were found for mirroring.");
                return;
            }

            if (flipZ)
                popup.ShowWarning(panelRendering, "Broken identity rotation detected. Impossible to mirror animation correctly.");

            _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

            for (int i = start; i <= end; i++)
            {
                var frame = _editor.CurrentAnim.DirectXAnimation.KeyFrames[i];

                frame.Translations[0] = new Vector3(-frame.Translations[0].X, frame.Translations[0].Y, frame.Translations[0].Z);

                // FIXME: There is a strange bug, bounding box sometimes corrupts in rendering panel and goes normal
                // after anim editor reopening.

                var newMin = new Vector3(-frame.BoundingBox.Minimum.X, frame.BoundingBox.Minimum.Y, frame.BoundingBox.Minimum.Z);
                var newMax = new Vector3(-frame.BoundingBox.Maximum.X, frame.BoundingBox.Maximum.Y, frame.BoundingBox.Maximum.Z);
                frame.BoundingBox = new BoundingBox(newMin, newMax);

                foreach (var pair in bonePairs)
                {
                    if (pair[1] != -1) // Paired bone
                    {
                        var r1 = MathC.RadToDeg(frame.Rotations[pair[0]]);
                        var r2 = MathC.RadToDeg(frame.Rotations[pair[1]]);

                        var r1Yrot = (360.0f - r1.Y) % 360.0f;
                        var r1Xrot = (360.0f - r1.X) % 360.0f;
                        var r1Zrot = (360.0f - r1.Z) % 360.0f;
                        var r2Yrot = (360.0f - r2.Y) % 360.0f;
                        var r2Xrot = (360.0f - r2.X) % 360.0f;
                        var r2Zrot = (360.0f - r2.Z) % 360.0f;

                        var nr1 = MathC.DegToRad(new Vector3(r2.X, r2Yrot, r2Zrot));
                        var nr2 = MathC.DegToRad(new Vector3(r1.X, r1Yrot, r1Zrot));

                        frame.Rotations[pair[0]] = nr1;
                        frame.Rotations[pair[1]] = nr2;

                        frame.Quaternions[pair[0]] = Quaternion.CreateFromYawPitchRoll(nr1.Y, nr1.X, nr1.Z);
                        frame.Quaternions[pair[1]] = Quaternion.CreateFromYawPitchRoll(nr2.Y, nr2.X, nr2.Z);
                    }
                    else // Non-paired bone
                    {
                        var r1 = MathC.RadToDeg(frame.Rotations[pair[0]]);

                        var r1Xrot = (360.0f - r1.X) % 360.0f;
                        var r1Yrot = (360.0f - r1.Y) % 360.0f;
                        var r1Zrot = (360.0f - r1.Z) % 360.0f;

                        r1 = MathC.DegToRad(new Vector3(r1.X, r1Yrot, r1Zrot));

                        frame.Rotations[pair[0]] = r1;
                        frame.Quaternions[pair[0]] = Quaternion.CreateFromYawPitchRoll(r1.Y, r1.X, r1.Z);
                    }
                }
            }

            // HACK: Bounce undo because otherwise bounding box and translation won't update correctly.
            // Most likely, the reason is somewhere in rendering panel.

            _editor.Tool.UndoManager.Undo();
            _editor.Tool.UndoManager.Redo();

            if (!flipZ && _editor.Moveable.Id.TypeId != 0 && _editor.Moveable.Meshes.Count > 3)
                popup.ShowInfo(panelRendering, "Animation mirroring may fail on certain non-Lara models.\nPlease check animation integrity.");

            Saved = false;

            timeline.Highlight(start, end);
            SelectFrame(); // We need to forcefully update current frame because frame number wasn't changed
            UpdateTransformUI();
        }

        private void EditAnimCommands(WadAnimCommand cmd = null)
        {
            if (_editor.CurrentAnim == null) return;

            var existingWindow = Application.OpenForms[nameof(FormAnimCommandsEditor)];
            if (existingWindow == null)
            {
                var acEditor = new FormAnimCommandsEditor(_editor, _editor.CurrentAnim);
                acEditor.Show(this);
                if (cmd != null)
                    acEditor.SelectCommand(cmd);
            }
            else
            {
                existingWindow.Focus();
                if (cmd != null)
                    ((FormAnimCommandsEditor)existingWindow).SelectCommand(cmd);
            }

            _editor.Tool.AnimationEditorAnimationChanged(_editor.CurrentAnim, false);
        }

        private void EditStateChanges(WadStateChange sch = null)
        {
            if (_editor.CurrentAnim == null) return;

            var existingWindow = Application.OpenForms[nameof(FormStateChangesEditor)];
            if (existingWindow == null)
            {
                var scEditor = new FormStateChangesEditor(_editor, _editor.CurrentAnim, sch);
                scEditor.Show(this);
            }
            else
                existingWindow.Focus();
        }

        private void FixAnimations(int mode)
        {
            var anims = new List<AnimationNode>();

            switch (mode)
            {
                case 0: anims.Add(_editor.CurrentAnim); break;
                case 1: anims.AddRange(lstAnimations.SelectedItems.Select(i => (AnimationNode)i.Tag)); break;
                case 2: anims.AddRange(_editor.Animations); break;
            }

            using (var form = new FormAnimationFixer(_editor, anims))
            {
                var result = form.ShowDialog();
                if (result == DialogResult.Ignore)
                    popup.ShowInfo(panelRendering, "No properties were selected or there was nothing to fix.\nNo changes were made.");
                else if (result == DialogResult.OK)
                {
                    var startMessage = (form.ChangedAnimations.Length < 50) ? "Animations (" + form.ChangedAnimations + ")" : "Multiple animations";
                    popup.ShowWarning(panelRendering, startMessage + " were fixed.\nPlease save your wad under new name and thoroughly test it.");
                    SelectAnimation(_editor.CurrentAnim);
                }
            }
        }

        private void ImportAnimations()
        {
            if (_editor.CurrentAnim == null)
                return;

            string path = LevelFileDialog.BrowseFile(this, "Select a file with animations",
                BaseGeometryImporter.AnimationFileExtensions, false);

            WadAnimation animation = null;

            if (path == null)
                return;

            bool containsMetadata = false;

            try
            {
                if (Path.GetExtension(path) == ".anim")
                {
                    containsMetadata = true;
                    animation = WadActions.ImportAnimationFromXml(_editor.Tool, path);
                }
                else if (Path.GetExtension(path) == ".trw")
                {
                    containsMetadata = true;
                    animation = WadActions.ImportAnimationFromTrw(path, _editor.CurrentAnim.Index);
                }
                else
                    animation = WadActions.ImportAnimationFromModel(_editor.Tool, this, _editor.Moveable.Bones.Count, path);
            }
            catch (Exception ex)
            {
                popup.ShowError(panelRendering, "Error while loading animation.\nException: " + ex.Message);
                return;
            }

            if (animation == null)
                return;

            // Don't replace animation name if it's empty
            if (animation.Name == new WadAnimation().Name)
                animation.Name = _editor.CurrentAnim.WadAnimation.Name;

            if (animation.KeyFrames[0].Angles.Count != _editor.Moveable.Bones.Count)
            {
                popup.ShowError(panelRendering, "You can only import an animation with the same number of bones as current moveable.");
                return;
            }

            _editor.CheckAnimationIntegrity(animation);
            _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

            if (containsMetadata)
            {
                _editor.CurrentAnim.WadAnimation = animation;
            }
            else
            {
                // Replace all frames of animation with new ones, this preserves metadata
                _editor.CurrentAnim.WadAnimation.KeyFrames.Clear();
                _editor.CurrentAnim.WadAnimation.KeyFrames.AddRange(animation.KeyFrames);
                _editor.CurrentAnim.WadAnimation.EndFrame = animation.EndFrame;
            }

            _editor.CurrentAnim.DirectXAnimation = Animation.FromWad2(_editor.Moveable.Bones, animation);

            UpdateAnimLabel(_editor.CurrentAnim);
            UpdateAnimListSelection(_editor.CurrentAnim.Index);

            timeline.Value = 0;
            panelRendering.Invalidate();
        }

        private void PlayAnimation()
        {
            _timerPlayAnimation.Enabled = !_timerPlayAnimation.Enabled;

            if (_timerPlayAnimation.Enabled)
            {
                if (_editor.CurrentAnim?.WadAnimation != null)
                {
                    // Backup current state if chained playback is about to begin
                    _chainedPlaybackInitialAnim = _editor.CurrentAnim.Index;
                    _chainedPlaybackInitialCursorPos = timeline.Value;
                    _chainedPlaybackInitialSelection = timeline.Selection;

                    if (_editor.CurrentAnim.WadAnimation.KeyFrames.Count > 1)
                        _frameCount = timeline.Value * _editor.CurrentAnim.WadAnimation.FrameRate;
                }

                butTransportPlay.Image = Properties.Resources.transport_stop_24;
            }
            else
            {
                if (_editor.Tool.Configuration.AnimationEditor_ChainPlayback &&
                    _editor.Tool.Configuration.AnimationEditor_RewindAfterChainPlayback)
                {
                    var origNode = _editor.Animations.FirstOrDefault(item => item.Index == _chainedPlaybackInitialAnim);

                    // Try to update UI if we've switched to different anim
                    if (origNode != null && origNode != _editor.CurrentAnim)
                    {
                        if (UpdateAnimListSelection(_chainedPlaybackInitialAnim) < 0)
                            SelectAnimation(origNode); // Update in UI failed, directly jump to original anim
                        else
                        {
                            // Restore selection and cursor
                            timeline.Value = _chainedPlaybackInitialCursorPos;
                            timeline.SelectionStart = _chainedPlaybackInitialSelection.X;
                            timeline.SelectionEnd = _chainedPlaybackInitialSelection.Y;
                        }
                    }
                    else if (origNode == _editor.CurrentAnim &&
                            _editor.CurrentAnim.WadAnimation.NextFrame >= (_editor.GetRealNumberOfFrames() - 1) &&
                            _editor.GetRealFrameNumber() >= _editor.CurrentAnim.WadAnimation.NextFrame)
                    {
                        // Just restore frame number so the timeline doesn't look stuck
                        timeline.Value = _chainedPlaybackInitialCursorPos;
                    }
                }

                butTransportPlay.Image = Properties.Resources.transport_play_24;
            }

            // Reset grid position and refresh view
            panelRendering.GridPosition = Vector3.Zero;
            panelRendering.Invalidate();

            // Translate global event
            _editor.Tool.TogglePlayback(_timerPlayAnimation.Enabled, _editor.Tool.Configuration.AnimationEditor_ChainPlayback);
        }

        private int UpdateAnimListSelection(int index)
        {
            var listItem = lstAnimations.Items.FirstOrDefault(item => ((AnimationNode)item.Tag).Index == index);
            if (listItem != null)
            {
                var listIndex = lstAnimations.Items.IndexOf(listItem);
                lstAnimations.SelectItem(listIndex);
                lstAnimations.EnsureVisible();
                return listIndex;
            }
            else
            {
                lstAnimations.SelectedIndices.Clear();
                lstAnimations.Invalidate();
                return -1;
            }
        }

        private void UpdateAnimLabel(AnimationNode anim, string label = null)
        {
            var listItem = lstAnimations.Items.FirstOrDefault(item => ((AnimationNode)item.Tag) == anim);
            if (listItem != null)
                listItem.Text = string.IsNullOrEmpty(label) ? ("(" + anim.Index + ") " + anim.WadAnimation.Name) : label;
        }


        private void SaveChanges()
        {
            _editor.SaveChanges();
            Saved = true;
        }

        private void UpdateStatusLabel()
        {
            if (_editor.CurrentAnim == null || _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count == 0)
            {
                statusFrame.Text = string.Empty;
                return;
            }

            string newLabel =
                "Frame: " + _frameCount + " / " + Math.Max(_editor.GetRealNumberOfFrames() - 1, 0) + "   " +
                "Keyframe: " + timeline.Value + " / " + Math.Max(_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1, 0);

            if (!timeline.SelectionIsEmpty)
            {
                newLabel += "   Selected keyframe";
                if (timeline.SelectionSize == 1)
                    newLabel += ": " + timeline.Selection.X;
                else
                    newLabel += "s: " + timeline.Selection.X + " to " + timeline.Selection.Y;
            }

            statusFrame.Text = newLabel;
        }

        private bool ValidAndSelected(bool prompt = true)
        {
            if (timeline.SelectionIsEmpty)
            {
                if (prompt) popup.ShowError(panelRendering, "No frames selected. Please select at least 1 frame.");
            }
            else if (_editor.CurrentAnim == null)
            {
                if (prompt) popup.ShowError(panelRendering, "No animation selected. Select animation to work with.");
            }
            else if (_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count == 0)
            {
                if (prompt) popup.ShowError(panelRendering, "Current animation contains no frames.");
            }
            else
                return true;

            return false;
        }

        private void BatchImport()
        {
            string path = LevelFileDialog.BrowseFolder(this, null, null, "Specify folder for batch import", null);

            if (path == null)
                return;

            var files = Directory.GetFiles(path).Where(f => Path.GetExtension(f) == ".anim").ToList();

            var undoList = new List<UndoRedoInstance>();

            bool updateSelection = false;
            bool? generateMissingAnims = null;
            int errorCount = 0;
            int importCount = 0;

            foreach (var file in files)
            {
                int number = -1;
                int.TryParse(Regex.Match(Path.GetFileName(file), @"\d+").Value, out number);

                if (number == -1)
                    continue; // No number in the beginning of the file

                try
                {
                    var animation = WadActions.ImportAnimationFromXml(_editor.Tool, file);

                    if (animation.KeyFrames[0].Angles.Count != _editor.Moveable.Bones.Count)
                        throw new Exception("Incorrect amount of bones in animation imported from file " + file);

                    if (_editor.Animations.Count <= number)
                    {
                        if (!generateMissingAnims.HasValue)
                            generateMissingAnims = DarkMessageBox.Show(this, "Batch contains animations with IDs spanning beyond existing animation count.\n" +
                                "Fill missing slots with empty animations (Yes) or skip missing slots (No)?", "Missing animation slots detected", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

                        if (generateMissingAnims.Value)
                            while (_editor.Animations.Count <= number)
                                _editor.Animations.Add(new AnimationNode(new WadAnimation() { Name = "Empty animation " + _editor.Animations.Count }, new Animation(), _editor.Animations.Count));
                        else
                            throw new Exception("Animation index is out of range for animation file " + file);
                    }

                    undoList.Add(new AnimationUndoInstance(_editor, _editor.Animations[number]));

                    _editor.Animations[number].WadAnimation = animation;
                    _editor.Animations[number].DirectXAnimation = Animation.FromWad2(_editor.Moveable.Bones, animation);

                    if (number == _editor.CurrentAnim.Index)
                        updateSelection = true;

                    importCount++;
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    errorCount++;
                    continue;
                }
            }

            _editor.Tool.UndoManager.Push(undoList);

            if (errorCount > 0)
                popup.ShowWarning(panelRendering, (importCount == 0 ? "No animations were imported.\n" : "Successfully imported " + importCount + " animations.\n") +
                                 errorCount + " animations were ignored. Check log file for details.");
            else
                popup.ShowInfo(panelRendering, "Successfully imported " + importCount + " animations.");

            RebuildAnimationsList();
            Saved = false;

            if (updateSelection)
            {
                UpdateAnimLabel(_editor.CurrentAnim);
                UpdateAnimListSelection(_editor.CurrentAnim.Index);
                timeline.Value = 0;
                panelRendering.Invalidate();
            }
        }

        private void BatchExport(bool all = false)
        {
            if (_editor.Animations.Count == 0)
            {
                popup.ShowError(panelRendering, "No animations present. Nothing to export.");
                return;
            }

            string path = LevelFileDialog.BrowseFolder(this, null, null, "Specify folder to save animations", null);

            if (path == null)
                return;

            int errorCount = 0;
            int exportCount = 0;

            foreach (var anim in (all ? _editor.Animations : lstAnimations.SelectedItems.Select(a => (AnimationNode)a.Tag)))
            {
                try
                {
                    var fileName = Path.Combine(path, anim.Index.ToString("D4") + "_" + anim.WadAnimation.Name + ".anim");

                    if (!WadActions.ExportAnimationToXml(_editor.Moveable, anim.WadAnimation, fileName))
                        throw new Exception("There was an error batch-exporting animation " + anim.WadAnimation.Name + ".");

                    exportCount++;
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    errorCount++;
                    continue;
                }
            }

            if (errorCount > 0)
                popup.ShowWarning(panelRendering, "Successfully exported " + exportCount + " animations.\n" +
                                 errorCount + " animations were ignored. Check log file for details.");
            else
                popup.ShowInfo(panelRendering, "Successfully exported " + exportCount + " animations.");
        }

        private void PreviewSounds()
        {
            if (!_editor.Tool.Configuration.AnimationEditor_SoundPreview || _editor.Tool.ReferenceLevel == null)
                return;

            // This counter is used to randomize material index every 3 seconds of playback.
            _overallPlaybackCount++;

            if (_overallPlaybackCount > _materialIndexSwitchInterval)
            {
                _overallPlaybackCount = 0;

                var listOfMaterialSounds = _editor.Tool.ReferenceLevel.Settings.GlobalSoundMap.Where(s => s.Name.IndexOf("FOOTSTEPS_", StringComparison.InvariantCultureIgnoreCase) >= 0).ToList();

                if (listOfMaterialSounds.Count > 1)
                    while (true)
                    {
                        var newMaterialIndex = (new Random()).Next(0, listOfMaterialSounds.Count() - 1);
                        if (listOfMaterialSounds.Count == 1 || newMaterialIndex != _currentMaterialIndex)
                        {
                            _currentMaterialIndex = listOfMaterialSounds[newMaterialIndex].Id;
                            break;
                        }
                    }
            }

            foreach (var ac in _editor.CurrentAnim.WadAnimation.AnimCommands)
            {
                int idToPlay = -1;
                var previewSoundType = _editor.Tool.Configuration.AnimationEditor_SoundPreviewType;

                if (ac.Type == WadAnimCommandType.PlaySound)
                {
                    idToPlay = ac.Parameter2;
                }
                else if (ac.Type == WadAnimCommandType.FlipEffect &&
                         previewSoundType == SoundPreviewType.LandWithMaterial &&
                         _editor.Wad.GameVersion.Native() >= TRVersion.Game.TR3)
                {
                    var flipID = ac.Parameter2;
                    if (flipID == 32 || flipID == 33)
                        idToPlay = _currentMaterialIndex;
                }

                if (idToPlay != -1 && ac.Parameter1 == _frameCount)
                {
                    var soundType = ac.Type == WadAnimCommandType.FlipEffect ? WadSoundEnvironmentType.Land : (WadSoundEnvironmentType)ac.Parameter3;

                    // Mute sound in substance.
                    if (ac.Type == WadAnimCommandType.FlipEffect &&
                        (previewSoundType == SoundPreviewType.Water || previewSoundType == SoundPreviewType.Quicksand || previewSoundType == SoundPreviewType.Underwater))
                        continue;
                    // Mute if sound type doesn't match preview sound type.
                    if (soundType == WadSoundEnvironmentType.Land && !(previewSoundType == SoundPreviewType.Land || previewSoundType == SoundPreviewType.LandWithMaterial))
                        continue;
                    if (soundType == WadSoundEnvironmentType.Water && previewSoundType != SoundPreviewType.Water)
                        continue;
                    if (soundType == WadSoundEnvironmentType.Quicksand && previewSoundType != SoundPreviewType.Quicksand)
                        continue;
                    if (soundType == WadSoundEnvironmentType.Underwater && previewSoundType != SoundPreviewType.Underwater)
                        continue;

                    var soundInfo = _editor.Tool.ReferenceLevel.Settings.GlobalSoundMap.FirstOrDefault(soundInfo_ => soundInfo_.Id == idToPlay);
                    if (soundInfo != null)
                    {
                        if (!_editor.Tool.ReferenceLevel.Settings.SelectedSounds.Contains(idToPlay))
                            popup.ShowWarning(panelRendering, "Sound info " + idToPlay + " is disabled in level settings.");
                        else
                            try
                            {
                                WadSoundPlayer.PlaySoundInfo(_editor.Tool.ReferenceLevel, soundInfo);
                            }
                            catch (Exception exc)
                            {
                                popup.ShowWarning(panelRendering, "Unable to play sound info " + idToPlay + ". Exception: \n" + exc.Message);
                            }
                    }
                    else
                        popup.ShowWarning(panelRendering, "Sound info " + idToPlay + " missing in reference project");
                }
            }
        }

        private void drawGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.Tool.Configuration.AnimationEditor_ShowGrid = !_editor.Tool.Configuration.AnimationEditor_ShowGrid;
            panelRendering.Invalidate();
        }

        private void drawGizmoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.Tool.Configuration.AnimationEditor_ShowGizmo = !_editor.Tool.Configuration.AnimationEditor_ShowGizmo;
            panelRendering.Invalidate();
        }

        private void drawSkinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.Tool.Configuration.AnimationEditor_ShowSkin = !_editor.Tool.Configuration.AnimationEditor_ShowSkin;
            panelRendering.Invalidate();
        }

        private void drawCollisionBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.Tool.Configuration.AnimationEditor_ShowCollisionBox = !_editor.Tool.Configuration.AnimationEditor_ShowCollisionBox;
            panelRendering.Invalidate();
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {
            string newName = tbName.Text.Trim();

            if (_editor.CurrentAnim != null && !string.IsNullOrEmpty(newName) && newName != _editor.CurrentAnim.WadAnimation.Name)
            {
                if (!_editor.MadeChanges)
                {
                    _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);
                    _editor.MadeChanges = true;
                }

                _editor.CurrentAnim.WadAnimation.Name = newName;
                UpdateAnimLabel(_editor.CurrentAnim);
                Saved = false;
            }
        }

        // Menu controls one-liners

        private void addNewAnimationToolStripMenuItem_Click(object sender, EventArgs e) => AddNewAnimation();
        private void deleteAnimationToolStripMenuItem_Click(object sender, EventArgs e) => DeleteAnimation();
        private void insertFrameAfterCurrentOneToolStripMenuItem_Click(object sender, EventArgs e) => AddNewFrame(timeline.Value + 1, true);
        private void insertFramesAfterCurrentToolStripMenuItem_Click(object sender, EventArgs e) => InsertMultipleFrames();
        private void cutFramesToolStripMenuItem_Click(object sender, EventArgs e) => CutFrames();
        private void copyFramesToolStripMenuItem_Click(object sender, EventArgs e) => CopyFrames();
        private void pasteFramesToolStripMenuItem_Click(object sender, EventArgs e) => PasteFrames();
        private void deleteFramesToolStripMenuItem_Click(object sender, EventArgs e) => DeleteFrames(this, true, true);
        private void cutAnimationToolStripMenuItem_Click(object sender, EventArgs e) => CutAnimation();
        private void copyAnimationToolStripMenuItem_Click(object sender, EventArgs e) => CopyAnimation();
        private void pasteAnimationToolStripMenuItem_Click(object sender, EventArgs e) => PasteAnimation();
        private void splitAnimationToolStripMenuItem_Click(object sender, EventArgs e) => SplitAnimation();
        private void reverseAnimationToolStripMenuItem_Click(object sender, EventArgs e) => ReverseAnimation();
        private void mirrorAnimationToolStripMenuItem_Click(object sender, EventArgs e) => MirrorAnimation();
        private void currentAnimationToolStripMenuItem_Click(object sender, EventArgs e) => FixAnimations(0);
        private void selectedAnimationsToolStripMenuItem_Click(object sender, EventArgs e) => FixAnimations(1);
        private void allAnimationsToolStripMenuItem_Click(object sender, EventArgs e) => FixAnimations(2);
        private void calculateBoundingBoxForCurrentFrameToolStripMenuItem_Click(object sender, EventArgs e) => CalculateKeyframeBoundingBox(timeline.Value, true, false);
        private void deleteCollisionBoxForCurrentFrameToolStripMenuItem_Click(object sender, EventArgs e) => CalculateKeyframeBoundingBox(timeline.Value, true, true);
        private void interpolateFramesToolStripMenuItem_Click(object sender, EventArgs e) => InterpolateFrames();
        private void saveChangesToolStripMenuItem_Click(object sender, EventArgs e) => SaveChanges();
        private void importToolStripMenuItem_Click(object sender, EventArgs e) => ImportAnimations();
        private void undoToolStripMenuItem_Click(object sender, EventArgs e) => _editor.Tool.UndoManager.Undo();
        private void redoToolStripMenuItem_Click(object sender, EventArgs e) => _editor.Tool.UndoManager.Redo();
        private void closeToolStripMenuItem_Click(object sender, EventArgs e) => Close();
        private void smoothAnimationsToolStripMenuItem_Click(object sender, EventArgs e) => _editor.Tool.Configuration.AnimationEditor_SmoothAnimation = !_editor.Tool.Configuration.AnimationEditor_SmoothAnimation;
        private void scrollGridToolStripMenuItem_Click(object sender, EventArgs e) => _editor.Tool.Configuration.AnimationEditor_ScrollGrid = !_editor.Tool.Configuration.AnimationEditor_ScrollGrid;
        private void restoreGridHeightToolStripMenuItem_Click(object sender, EventArgs e) => _editor.Tool.Configuration.AnimationEditor_RecoverGridAfterPositionChange = !_editor.Tool.Configuration.AnimationEditor_RecoverGridAfterPositionChange;
        private void importToolStripMenuItem1_Click(object sender, EventArgs e) => BatchImport();
        private void exportSelectedToolStripMenuItem_Click(object sender, EventArgs e) => BatchExport();
        private void exportAllToolStripMenuItem_Click_1(object sender, EventArgs e) => BatchExport(true);

        // Toolbox controls one-liners

        private void butTbAddAnimation_Click(object sender, EventArgs e) => AddNewAnimation();
        private void butTbDeleteAnimation_Click(object sender, EventArgs e) => DeleteAnimation();
        private void butTbAddFrame_Click(object sender, EventArgs e) => AddNewFrame(timeline.Value + 1, true);
        private void butTbCutFrame_Click(object sender, EventArgs e) => CutFrames();
        private void butTbCopyFrame_Click(object sender, EventArgs e) => CopyFrames();
        private void butTbPasteFrame_Click(object sender, EventArgs e) => PasteFrames();
        private void butTbDeleteFrame_Click(object sender, EventArgs e) => DeleteFrames(this, true, true);
        private void butTbCutAnimation_Click(object sender, EventArgs e) => CutAnimation();
        private void butTbCopyAnimation_Click(object sender, EventArgs e) => CopyAnimation();
        private void butTbPasteAnimation_Click(object sender, EventArgs e) => PasteAnimation();
        private void butTbReplaceAnimation_Click(object sender, EventArgs e) => ReplaceAnimation();
        private void butTbSplitAnimation_Click(object sender, EventArgs e) => SplitAnimation();
        private void butTbSaveChanges_Click(object sender, EventArgs e) => SaveChanges();
        private void butTbUndo_Click(object sender, EventArgs e) => _editor.Tool.UndoManager.Undo();
        private void butTbRedo_Click(object sender, EventArgs e) => _editor.Tool.UndoManager.Redo();
        private void butTbImport_Click(object sender, EventArgs e) => ImportAnimations();
        private void butTbResetCamera_Click(object sender, EventArgs e) => panelRendering.ResetCamera();

        // General controls one-liners

        private void butAddNewAnimation_Click(object sender, EventArgs e) => AddNewAnimation();
        private void butDeleteAnimation_Click(object sender, EventArgs e) => DeleteSelectedAnimations();
        private void butSearchByStateID_Click(object sender, EventArgs e) => RebuildAnimationsList();
        private void butCalculateAnimCollision_Click(object sender, EventArgs e) => CalculateAnimationBoundingBox();
        private void butClearAnimCollision_Click(object sender, EventArgs e) => ClearAnimationBoundingBox();
        private void butEditAnimCommands_Click(object sender, EventArgs e) => EditAnimCommands();
        private void butEditStateChanges_Click(object sender, EventArgs e) => EditStateChanges();

        private void butSelectAllMeshes_Click(object sender, EventArgs e) => SelectMeshesInMeshList();
        private void butSelectNoMeshes_Click(object sender, EventArgs e) => SelectMeshesInMeshList(false);
        private void butCalcBBoxAnim_Click(object sender, EventArgs e) => CalculateAnimationBoundingBox();
        private void butResetBBoxAnim_Click(object sender, EventArgs e) => ClearAnimationBoundingBox();
        private void butGrowBBox_Click(object sender, EventArgs e) => InflateAnimationBoundingBox(new Vector3((float)nudGrowX.Value, (float)nudGrowY.Value, (float)nudGrowZ.Value));
        private void butShrinkBBox_Click(object sender, EventArgs e) => InflateAnimationBoundingBox(new Vector3((float)-nudGrowX.Value, (float)-nudGrowY.Value, (float)-nudGrowZ.Value));

        // Property controls one-liners

        private void tbName_KeyDown(object sender, KeyEventArgs e) { if (e.KeyData == Keys.Enter) animParameter_Validated(this, e); }
        private void tbStateId_KeyDown(object sender, KeyEventArgs e) { if (e.KeyData == Keys.Enter) UpdateStateChange(); }
        private void nudFramerate_ValueChanged(object sender, EventArgs e) => UpdateAnimationParameter(nudFramerate);
        private void nudEndFrame_ValueChanged(object sender, EventArgs e) => UpdateAnimationParameter(nudEndFrame);
        private void nudNextAnim_ValueChanged(object sender, EventArgs e) => UpdateAnimationParameter(nudNextAnim);
        private void nudNextFrame_ValueChanged(object sender, EventArgs e) => UpdateAnimationParameter(nudNextFrame);
        private void nudStartVertVel_ValueChanged(object sender, EventArgs e) => UpdateAnimationParameter(nudStartVertVel);
        private void nudEndVertVel_ValueChanged(object sender, EventArgs e) => UpdateAnimationParameter(nudEndVertVel);
        private void nudStartHorVel_ValueChanged(object sender, EventArgs e) => UpdateAnimationParameter(nudStartHorVel);
        private void nudEndHorVel_ValueChanged(object sender, EventArgs e) => UpdateAnimationParameter(nudEndHorVel);
        private void nudBlendFrameCount_ValueChanged(object sender, EventArgs e) => UpdateAnimationParameter(nudBlendFrameCount);

        private void tbStateId_Validated(object sender, EventArgs e) => UpdateStateChange();
        private void animParameter_Validated(object sender, EventArgs e) => ValidateAnimationParameter();

        // Timeline & transport one-liners

        private void timeline_SelectionChanged(object sender, EventArgs e) => UpdateSelection();
        private void timeline_ValueChanged(object sender, EventArgs e) => SelectFrame();
        private void timeline_AnimCommandDoubleClick(object sender, WadAnimCommand ac) => EditAnimCommands(ac);

        private void butTransportPlay_Click(object sender, EventArgs e) => PlayAnimation();
        private void butTransportStart_Click(object sender, EventArgs e) => timeline.Value = timeline.Minimum;
        private void butTransportFrameBack_Click(object sender, EventArgs e) => timeline.Value--;
        private void butTransportFrameForward_Click(object sender, EventArgs e) => timeline.Value++;
        private void butTransportEnd_Click(object sender, EventArgs e) => timeline.Value = timeline.Maximum;

        // Transform one-liners

        private void nudRotX_ValueChanged(object sender, EventArgs e) => UpdateTransform();
        private void nudRotY_ValueChanged(object sender, EventArgs e) => UpdateTransform();
        private void nudRotZ_ValueChanged(object sender, EventArgs e) => UpdateTransform();
        private void nudRotX_KeyUp(object sender, KeyEventArgs e) => UpdateTransform();
        private void nudRotY_KeyUp(object sender, KeyEventArgs e) => UpdateTransform();
        private void nudRotZ_KeyUp(object sender, KeyEventArgs e) => UpdateTransform();
        private void nudTransX_ValueChanged(object sender, EventArgs e) => UpdateTransform();
        private void nudTransY_ValueChanged(object sender, EventArgs e) => UpdateTransform();
        private void nudTransZ_ValueChanged(object sender, EventArgs e) => UpdateTransform();
        private void nudTransX_KeyUp(object sender, KeyEventArgs e) => UpdateTransform();
        private void nudTransY_KeyUp(object sender, KeyEventArgs e) => UpdateTransform();
        private void nudTransZ_KeyUp(object sender, KeyEventArgs e) => UpdateTransform();

        private void nudBBoxMinX_ValueChanged(object sender, EventArgs e) => UpdateAnimationParameter(nudBBoxMinX);
        private void nudBBoxMinY_ValueChanged(object sender, EventArgs e) => UpdateAnimationParameter(nudBBoxMinY);
        private void nudBBoxMinZ_ValueChanged(object sender, EventArgs e) => UpdateAnimationParameter(nudBBoxMinZ);
        private void nudBBoxMaxX_ValueChanged(object sender, EventArgs e) => UpdateAnimationParameter(nudBBoxMaxX);
        private void nudBBoxMaxY_ValueChanged(object sender, EventArgs e) => UpdateAnimationParameter(nudBBoxMaxY);
        private void nudBBoxMaxZ_ValueChanged(object sender, EventArgs e) => UpdateAnimationParameter(nudBBoxMaxZ);

        // Context menus for timeline

        private void cmMarkInMenuItem_Click(object sender, EventArgs e) => timeline.SelectionStart = timeline.Value;
        private void cmMarkOutMenuItem_Click(object sender, EventArgs e) => timeline.SelectionEnd = timeline.Value;
        private void cnClearSelectionMenuItem_Click(object sender, EventArgs e) => timeline.ResetSelection();
        private void cmSelectAllMenuItem_Click(object sender, EventArgs e) => timeline.SelectAll();
        private void cmCreateAnimCommandMenuItem_Click(object sender, EventArgs e) => EditAnimCommands(new WadAnimCommand() { Type = WadAnimCommandType.PlaySound, Parameter1 = (short)timeline.FrameIndex });

        private void cmCreateStateChangeMenuItem_Click(object sender, EventArgs e)
        {
            WadStateChange sch = null;

            if (!timeline.SelectionIsEmpty)
            {
                sch = new WadStateChange();
                sch.Dispatches.Add(new WadAnimDispatch()
                {
                    InFrame = (ushort)timeline.SelectionStartFrameIndex,
                    OutFrame = (ushort)timeline.SelectionEndFrameIndex,
                    NextAnimation = 0,
                    NextFrameLow = 0
                });
            }

            EditStateChanges(sch);
        }

        private void bezierCurveEditor_ValueChanged(object sender, EventArgs e)
        {
            cbBlendPreset.SelectedIndex = -1;
            Saved = false;
        }

        private void timerPlayAnimation_Tick(object sender, EventArgs e)
        {
            if (_editor.CurrentAnim?.WadAnimation == null || _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count < 1)
                return;

            int realFrameNumber = _editor.GetRealNumberOfFrames();
            int realRangeNumber = _editor.CurrentAnim.WadAnimation.EndFrame > realFrameNumber - 1 ? realFrameNumber : _editor.CurrentAnim.WadAnimation.EndFrame + 1;

            _frameCount++;

            var nextIndex = _editor.CurrentAnim.WadAnimation.NextAnimation;
            var nextFrame = _editor.CurrentAnim.WadAnimation.NextFrame;
            var nextRange = new VectorInt2(realRangeNumber);

            // If state change data is present, make sure it complies to current animation params.

            if (_editor.Tool.Configuration.AnimationEditor_ChainPlayback &&
                _chainedPlaybackIncomingAnimation >= 0)
            {
                if (_chainedPlaybackIncomingAnimation < _editor.Animations.Count &&
                    _chainedPlaybackIncomingFrame >= 0 &&
                    _chainedPlaybackIncomingFrame < _editor.GetRealNumberOfFrames(_chainedPlaybackIncomingAnimation) &&
                    _chainedPlaybackIncomingFrameRange.X >= 0 &&
                    _chainedPlaybackIncomingFrameRange.Y >= _chainedPlaybackIncomingFrameRange.X &&
                    _chainedPlaybackIncomingFrameRange.Y <= realFrameNumber) // Some single-frame state changes refer to out-of-bounds frame 1
                {
                    nextIndex = (ushort)_chainedPlaybackIncomingAnimation;
                    nextFrame = (ushort)_chainedPlaybackIncomingFrame;
                    nextRange = _chainedPlaybackIncomingFrameRange;
                }
                else
                {
                    popup.ShowError(panelRendering, "Pending state change to animation #" + _chainedPlaybackIncomingAnimation + " had incorrect data and was ignored.");
                    _chainedPlaybackIncomingAnimation = -1;
                }
            }

            if (_frameCount >= nextRange.X && _frameCount <= nextRange.Y)
            {
                // Chain playback handling
                if (_editor.Tool.Configuration.AnimationEditor_ChainPlayback)
                {
                    // Clear state change anim, as we don't need it anymore
                    _chainedPlaybackIncomingAnimation = -1;

                    var nextNode = _editor.Animations.FirstOrDefault(item => item.Index == nextIndex);
                    if (nextNode != null)
                    {
                        // Take SetPosition animcommands into account
                        if (_editor.Tool.Configuration.AnimationEditor_ScrollGrid && _editor.CurrentAnim.WadAnimation.AnimCommands.Count > 0)
                        {
                            var setPosCommands = _editor.CurrentAnim.WadAnimation.AnimCommands.Where(cmd => cmd.Type == WadAnimCommandType.SetPosition);
                            if (setPosCommands.Count() > 0)
                            {
                                _chainedPlaybackSetPosRecoveryCount = 0; // Reset pending grid recovery
                            }
                        }
                        _editor.CurrentAnim.WadAnimation.AnimCommands
                            .Where(cmd => cmd.Type == WadAnimCommandType.SetPosition)
                            .ToList()
                            .ForEach(cmd =>
                            panelRendering.GridPosition += new Vector3(cmd.Parameter1, cmd.Parameter2, cmd.Parameter3));

                        // Only try to update if next anim is different
                        if (nextNode != _editor.CurrentAnim && UpdateAnimListSelection(nextIndex) < 0)
                            SelectAnimation(nextNode); // Update in UI failed, directly jump to anim

                        var maxFrameNumber = _editor.GetRealNumberOfFrames(nextIndex);
                        if (nextFrame >= maxFrameNumber)
                        {
                            _frameCount = 0;
                            popup.ShowWarning(panelRendering, "No frame " + nextFrame + " in animation " + nextIndex + ". Using first frame.");
                        }
                        else
                            _frameCount = nextFrame;
                    }
                    else
                        popup.ShowWarning(panelRendering, "Animation " + nextIndex + " wasn't found. Chain is broken.");
                }
                else
                    _frameCount = 0;
            }

            // Scroll the grid
            if (_editor.Tool.Configuration.AnimationEditor_ScrollGrid)
            {
                // Reset grid position if animation isn't looped
                if (!_editor.Tool.Configuration.AnimationEditor_ChainPlayback && _frameCount >= (realRangeNumber - 1) &&
                    _editor.CurrentAnim.WadAnimation.NextAnimation != _editor.CurrentAnim.Index)
                    panelRendering.GridPosition = Vector3.Zero;

                Vector3 startVel = new Vector3(_editor.CurrentAnim.WadAnimation.StartLateralVelocity, 0, _editor.CurrentAnim.WadAnimation.StartVelocity);
                Vector3 endVel = new Vector3(_editor.CurrentAnim.WadAnimation.EndLateralVelocity, 0, _editor.CurrentAnim.WadAnimation.EndVelocity);

                // HACK: Due to Core Design's programming habits, we need to force speed direction for certain states.

                if (_editor.Moveable.Id.TypeId == 0)
                {
                    switch (_editor.CurrentAnim.WadAnimation.StateId)
                    {
                        case 5:  // RUN_BACK
                        case 16: // WALK_BACK
                        case 23: // ROLL_BACK
                        case 25: // JUMP_BACK
                        case 32: // SLIDE_BACK
                            startVel.Z = -startVel.Z;
                            endVel.Z = -endVel.Z;
                            break;

                        case 21: // WALK_RIGHT
                        case 26: // JUMP_RIGHT
                        case 31: // SHIMMY_RIGHT
                        case 60: // LADDER_RIGHT
                        case 78: // MONKEY_LEFT
                            startVel = new Vector3(startVel.Z, 0, startVel.X);
                            endVel = new Vector3(endVel.Z, 0, endVel.X);
                            break;

                        case 22: // WALK_LEFT
                        case 27: // JUMP_LEFT
                        case 30: // SHIMMY_LEFT
                        case 58: // LADDER_LEFT
                        case 77: // MONKEY_LEFT
                            startVel = new Vector3(-startVel.Z, 0, -startVel.X);
                            endVel = new Vector3(-endVel.Z, 0, -endVel.X);
                            break;
                    }
                }

                var shiftX = Vector3.Lerp(startVel, endVel, (float)_frameCount / (float)realFrameNumber);
                panelRendering.GridPosition += shiftX;

                if (_editor.Tool.Configuration.AnimationEditor_RecoverGridAfterPositionChange)
                {
                    // Count recovery interval if we have shifted Y position.
                    if (panelRendering.GridPosition.Y != 0 &&
                        !_editor.CurrentAnim.WadAnimation.AnimCommands.Any(cmd => cmd.Type == WadAnimCommandType.SetPosition) &&
                        _editor.CurrentAnim.WadAnimation.NextAnimation == _editor.CurrentAnim.Index)
                        _chainedPlaybackSetPosRecoveryCount++;
                    else
                        _chainedPlaybackSetPosRecoveryCount = 0;

                    // Start restoring it
                    if (_chainedPlaybackSetPosRecoveryCount >= _gridRecoveryWaitInterval)
                    {
                        _gridRecoveryCount += _gridRecoveryStep;

                        Vector3 shiftY = new Vector3(panelRendering.GridPosition.X, 0.0f, panelRendering.GridPosition.Z);
                        if (_gridRecoveryCount < 1.0f)
                            shiftY.Y = (float)MathC.SmoothStep(panelRendering.GridPosition.Y, 0.0f, _gridRecoveryCount);
                        else
                        {
                            _gridRecoveryCount = 0.0f;
                            _chainedPlaybackSetPosRecoveryCount = 0;
                        }

                        panelRendering.GridPosition = shiftY;
                    }
                }
            }

            bool isKeyFrame = (_frameCount % (_editor.CurrentAnim.WadAnimation.FrameRate == 0 ? 1 : _editor.CurrentAnim.WadAnimation.FrameRate) == 0);

            // Update animation
            if (isKeyFrame)
            {
                int newFrameNumber = (int)Math.Round((double)(_frameCount / _editor.CurrentAnim.WadAnimation.FrameRate));

                if (newFrameNumber > timeline.Maximum)
                    timeline.Value = 0;
                else
                    timeline.Value = newFrameNumber;
            }
            else if (_editor.Tool.Configuration.AnimationEditor_SmoothAnimation)
            {
                float k = (float)_frameCount / (float)(_editor.CurrentAnim.WadAnimation.FrameRate == 0 ? 1 : _editor.CurrentAnim.WadAnimation.FrameRate);
                k = _frameCount == realFrameNumber - 1 ? 1.0f : k - (float)Math.Floor(k);
                SelectFrame(k);
            }

            UpdateStatusLabel();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_editor.CurrentAnim == null)
                return;

            string path = LevelFileDialog.BrowseFile(this, "Specify file to save animation",
                new List<FileFormat>() { new FileFormat("TombEditor XML", "anim") }, true);

            if (path == null)
                return;

            var animationToSave = _editor.GetSavedAnimation(_editor.CurrentAnim);

            if (!WadActions.ExportAnimationToXml(_editor.Moveable, animationToSave, path))
            {
                popup.ShowError(panelRendering, "Can't export current animation to XML file");
                return;
            }
        }

        private void exportAllToolStripMenuItem_Click(object sender, EventArgs e) => BatchExport(true);

        private void importAllToolStripMenuItem_Click(object sender, EventArgs e) => BatchImport();

        private void comboRoomList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboRoomList.ComboBox.SelectedIndex == 0)
            {
                panelRendering.Room = null;
                panelRendering.RoomPosition = Vector3.Zero;
            }
            else
            {
                panelRendering.Room = (Room)comboRoomList.ComboBox.SelectedItem;
                panelRendering.RoomPosition = panelRendering.Room.GetLocalCenter();
            }

            panelRendering.Invalidate();
        }

        private void butShowAll_Click(object sender, EventArgs e)
        {
            tbSearchAnimation.Text = "";
            RebuildAnimationsList();
        }

        private void formAnimationEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!Saved)
            {
                var result = DarkMessageBox.Show(this, "You have unsaved changes. Do you want to save changes to animations?",
                                                 "Confirm", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    _editor.SaveChanges();
                    DialogResult = DialogResult.OK;
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                else
                    DialogResult = DialogResult.Cancel;
            }

            _timerPlayAnimation.Stop();
            _timerPlayAnimation.Tick -= timerPlayAnimation_Tick;

            _editor.Tool.UndoManager.ClearAll();
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Don't process one-key and shift hotkeys if we're focused on control which allows text input
            if (WinFormsUtils.CurrentControlSupportsInput(this, keyData))
                return base.ProcessCmdKey(ref msg, keyData);

            switch (keyData)
            {
                case Keys.Escape:
                    SelectMesh(-1);
                    dgvBoundingMeshList.ClearSelection();
                    timeline.ResetSelection();
                    break;

                case Keys.Left: timeline.ValueLoopDec(); break;
                case Keys.Right: timeline.ValueLoopInc(); break;
                case Keys.Up: timeline.Value = timeline.Minimum; break;
                case Keys.Down: timeline.Value = timeline.Maximum; break;
                case Keys.Space: PlayAnimation(); break;
                case Keys.I: timeline.SelectionStart = timeline.Value; break;
                case Keys.O: timeline.SelectionEnd = timeline.Value; break;
                case Keys.Delete: DeleteFrames(this, true, true); break;

                case (Keys.Control | Keys.A): timeline.SelectAll(); break;
                case (Keys.Control | Keys.X): CutFrames(); break;
                case (Keys.Control | Keys.C): CopyFrames(); break;
                case (Keys.Control | Keys.V): PasteFrames(); break;
                case (Keys.Control | Keys.I): InterpolateFrames(); break;
                case (Keys.Control | Keys.S): SaveChanges(); break;
                case (Keys.Control | Keys.Z): _editor.Tool.UndoManager.Undo(); break;
                case (Keys.Control | Keys.Y): _editor.Tool.UndoManager.Redo(); break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void lstAnimations_SelectedIndicesChanged(object sender, EventArgs e)
        {
            if (lstAnimations.SelectedIndices.Count == 0)
                return;

            var node = (AnimationNode)lstAnimations.SelectedItem.Tag;
            SelectAnimation(node);
            _editor.CheckAnimationIntegrity(node.WadAnimation);
        }

        private void butTransportSound_Click(object sender, EventArgs e)
        {
            if (_editor.Tool.ReferenceLevel == null && !WadActions.LoadReferenceLevel(_editor.Tool, this)) return;

            _editor.Tool.Configuration.AnimationEditor_SoundPreview = !_editor.Tool.Configuration.AnimationEditor_SoundPreview;
            UpdateUIControls();
        }

        private void butTransportCondition_Click(object sender, EventArgs e)
        {
            if (_editor.Tool.ReferenceLevel == null && !WadActions.LoadReferenceLevel(_editor.Tool, this)) return;

            var previewType = _editor.Tool.Configuration.AnimationEditor_SoundPreviewType;
            bool isTEN = _editor.Wad.GameVersion == TRVersion.Game.TombEngine;

            if (previewType == SoundPreviewType.Land) _editor.Tool.Configuration.AnimationEditor_SoundPreviewType = SoundPreviewType.LandWithMaterial;
            else if (previewType == SoundPreviewType.LandWithMaterial) _editor.Tool.Configuration.AnimationEditor_SoundPreviewType = SoundPreviewType.Water;
            else if (previewType == SoundPreviewType.Water) _editor.Tool.Configuration.AnimationEditor_SoundPreviewType = isTEN ? SoundPreviewType.Quicksand : SoundPreviewType.Land;
            else if (isTEN && previewType == SoundPreviewType.Quicksand) _editor.Tool.Configuration.AnimationEditor_SoundPreviewType = SoundPreviewType.Underwater;
            else if (isTEN && previewType == SoundPreviewType.Underwater) _editor.Tool.Configuration.AnimationEditor_SoundPreviewType = SoundPreviewType.Land;

            UpdateUIControls();
        }

        private void tbSearchByStateID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                RebuildAnimationsList();
        }

        private void resampleAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormInputBox("Resample animation", "Enter resample multiplier (speed)", "2"))
            {
                form.Width = 300;
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

                    int result = 0;
                    if (!int.TryParse(form.Result, out result) || result <= 1)
                    {
                        popup.ShowError(panelRendering, "You must insert a number greater than 1");
                        return;
                    }

                    InterpolateAnimation(result - 1, true, true);
                }
            }
        }

        private void resampleAnimationToKeyframesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_editor.CurrentAnim.WadAnimation.FrameRate < 2)
            {
                popup.ShowError(panelRendering, "Animation is already at framerate 1. You need other framerate to resample.");
                return;
            }

            InterpolateAnimation(_editor.CurrentAnim.WadAnimation.FrameRate - 1, false, false);
            _editor.CurrentAnim.WadAnimation.FrameRate = 1;

            Saved = false;

            UpdateAnimListSelection(_editor.CurrentAnim.Index);
            timeline.Highlight();
        }

        private void transportChained_Click(object sender, EventArgs e)
        {
            _editor.Tool.Configuration.AnimationEditor_ChainPlayback = !_editor.Tool.Configuration.AnimationEditor_ChainPlayback;
            UpdateUIControls();

            // Translate global event
            _editor.Tool.TogglePlayback(_timerPlayAnimation.Enabled, _editor.Tool.Configuration.AnimationEditor_ChainPlayback);
        }

        private void lstAnimations_Click(object sender, EventArgs e)
        {
            if (_editor.CurrentAnim != null && lstAnimations.SelectedItem != null)
            {
                // Update saved state's anim number, in case chained playback is in effect
                _chainedPlaybackInitialAnim = _editor.CurrentAnim.Index;
                _chainedPlaybackInitialCursorPos = 0;
                _chainedPlaybackInitialSelection = new VectorInt2();

                // Reset grid position
                panelRendering.GridPosition = Vector3.Zero;
            }
        }

        private void cmbStateID_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbStateID.SelectedIndex != -1)
                UpdateStateChange(true);
        }

        private void DeleteEveryNthFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormInputBox("Delete every nth frame", "Enter number", "2"))
            {
                form.Width = 300;
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

                    int result = 0;
                    if (!int.TryParse(form.Result, out result) || result <= 1)
                    {
                        popup.ShowError(panelRendering, "You must insert a number greater than 1");
                        return;
                    }
                    DeleteEveryNFrame(result);
                }

                if (_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1 < timeline.Value)
                    timeline.Value = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1;

                UpdateAnimListSelection(_editor.CurrentAnim.Index);
                timeline.Highlight();
            }
        }

        private void findReplaceAnimcommandsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormReplaceAnimCommands(_editor))
            {
                form.ShowDialog(this);
                if (form.EditingWasDone) Saved = false;
                timeline.Invalidate(); // FIXME: To update current timeline. Use an event instead later.
            }
        }

        private void timeline_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                cmTimelineContextMenu.Show(Cursor.Position, ToolStripDropDownDirection.AboveRight);
        }

        private void panelRendering_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                panelRendering.ResetCamera();
        }

        private void panelRendering_MouseEnter(object sender, EventArgs e)
        {
            // Reset editing state so undo can be engaged
            _editor.MadeChanges = false;
        }

        private void panelRendering_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                UpdateTransformUI();
        }

        private void cmbTransformMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            _editor.TransformMode = (AnimTransformMode)cmbTransformMode.SelectedIndex;

            switch (_editor.TransformMode)
            {
                case AnimTransformMode.Smooth:
                    picTransformPreview.Image = Properties.Resources.transform_smooth.SetOpacity(Colors.Brightness);
                    break;
                case AnimTransformMode.SmoothReverse:
                    picTransformPreview.Image = Properties.Resources.transform_smooth_reverse.SetOpacity(Colors.Brightness);
                    break;
                case AnimTransformMode.Linear:
                    picTransformPreview.Image = Properties.Resources.transform_linear.SetOpacity(Colors.Brightness);
                    break;
                case AnimTransformMode.LinearReverse:
                    picTransformPreview.Image = Properties.Resources.transform_linear_reverse.SetOpacity(Colors.Brightness);
                    break;
                case AnimTransformMode.Symmetric:
                    picTransformPreview.Image = Properties.Resources.transform_symmetric.SetOpacity(Colors.Brightness);
                    break;
                case AnimTransformMode.SymmetricLinear:
                    picTransformPreview.Image = Properties.Resources.transform_symmetric_linear.SetOpacity(Colors.Brightness);
                    break;
                default:
                    picTransformPreview.Image = Properties.Resources.transform_simple.SetOpacity(Colors.Brightness);
                    break;
            }

            if (_allowUpdate && _editor.MadeChanges)
                UpdateTransform();
        }

        private void dgvBoundingMeshList_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvBoundingMeshList.SelectedRows.Count < 1) return;
            SelectMesh(dgvBoundingMeshList.SelectedRows[0].Index);
        }

        private void dgvBoundingMeshList_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0) return;
            dgvBoundingMeshList.Rows[e.RowIndex].Cells[0].Value = !(bool)dgvBoundingMeshList.Rows[e.RowIndex].Cells[0].Value;
        }

        private void FormAnimationEditor_Shown(object sender, EventArgs e)
        {
            // Hacks for textbox...
            tbStateId.AutoSize = false;
            tbStateId.Height = cmbStateID.Height;
        }

        private void SetCurrentTransportPreviewType(SoundPreviewType previewType)
        {
            if (_editor.Tool.ReferenceLevel == null && !WadActions.LoadReferenceLevel(_editor.Tool, this))
                return;

            _editor.Tool.Configuration.AnimationEditor_SoundPreviewType = previewType;
            UpdateUIControls();
        }

        private void cbRootPosX_CheckedChanged(object sender, EventArgs e)
        {
            // TODO: Where to put these flags?
        }

        private void cbBlendPreset_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cbBlendPreset.SelectedIndex)
            {
                case 0:
                    bezierCurveEditor.Value.Set(BezierCurve2.Linear);
                    break;

                case 1:
                    bezierCurveEditor.Value.Set(BezierCurve2.EaseIn);
                    break;

                case 2:
                    bezierCurveEditor.Value.Set(BezierCurve2.EaseOut);
                    break;

                case 3:
                    bezierCurveEditor.Value.Set(BezierCurve2.EaseInOut);
                    break;
            }

            bezierCurveEditor.UpdateUI();
        }
    }
}