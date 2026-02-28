using DarkUI.Forms;
using TombLib.Graphics.Dx11Toolkit;
using System;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Forms;
using TombLib.Graphics;
using TombLib.Wad;
using WadTool.Controls;
using TombLib.Utils;
using System.Linq;
using TombLib.Wad.Catalog;

namespace WadTool
{
    public partial class FormStaticEditor : DarkForm
    {
        private readonly Wad2 _wad;
        private readonly WadStatic _static;
        private readonly WadToolClass _tool;
        private readonly GraphicsDevice _device;
        private bool _doChangesInLighting = false;

        // Info
        private readonly PopUpInfo popup = new PopUpInfo();

        public FormStaticEditor(WadToolClass tool, DeviceManager deviceManager, Wad2 wad, WadStatic staticMesh)
        {
            _doChangesInLighting = false;

            InitializeComponent();

            _wad = wad;
            _tool = tool;
            _device = deviceManager.___LegacyDevice;

            _static = staticMesh.Clone();
            panelRendering.InitializeRendering(tool, deviceManager);

            cmbSoundID.Items.Add("Default");
            cmbSoundID.Items.AddRange(WadSounds.GetFormattedList(_tool.ReferenceLevel, _wad.GameVersion).ToArray());

            panelRendering.Configuration = _tool.Configuration;
            panelRendering.Static = _static;
            panelRendering.DrawGrid = true;
            panelRendering.DrawGizmo = true;
            panelRendering.DrawLights = true;
            UpdateVisibilityBoxUI();
            UpdateCollisionBoxUI();
            UpdatePositionUI();
            UpdateLightsList();
            UpdateLightUI();
            UpdateShatterUI();

            numAmbient.Value = (decimal)_static.AmbientLight;

            _tool.EditorEventRaised += Tool_EditorEventRaised;

            _doChangesInLighting = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
                _tool.EditorEventRaised -= Tool_EditorEventRaised;
            }
            base.Dispose(disposing);
        }

        private void Tool_EditorEventRaised(IEditorEvent obj)
        {
            if (obj is WadToolClass.StaticSelectedLightChangedEvent)
                UpdateLightUI();

            if (obj is WadToolClass.StaticLightsChangedEvent)
            {
                UpdateLightsList();
                UpdateLightUI();
                _static.Version = DataVersion.GetNext();
                panelRendering.Invalidate();
            }

            if (obj is WadToolClass.MessageEvent)
            {
                var m = obj as WadToolClass.MessageEvent;
                PopUpInfo.Show(popup, this, panelRendering, m.Message, m.Type);
            }
        }

        private void butSaveChanges_Click(object sender, EventArgs e)
        {
            // Check if we need to transform mesh
            var transform = panelRendering.GizmoTransform;
            if (transform != Matrix4x4.Identity)
            {
                for (int i = 0; i < _static.Mesh.VertexPositions.Count; i++)
                {
                    var position = MathC.HomogenousTransform(_static.Mesh.VertexPositions[i], transform);
                    _static.Mesh.VertexPositions[i] = new Vector3(position.X, position.Y, position.Z);
                }

                for (int i = 0; i < _static.Mesh.VertexNormals.Count; i++)
                {
                    var normal = MathC.HomogenousTransform(_static.Mesh.VertexNormals[i], transform);
                    _static.Mesh.VertexNormals[i] = new Vector3(normal.X, normal.Y, normal.Z);
                }
            }

            // Assign the edited mesh to original static mesh
            _wad.Statics.Remove(_static.Id);
            _wad.Statics.Add(_static.Id, _static);

            _static.Version = DataVersion.GetNext();

            _tool.ToggleUnsavedChanges();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void UpdateLightsList()
        {
            lstLights.Nodes.Clear();
            foreach (var light in _static.Lights)
            {
                var node = new DarkUI.Controls.DarkTreeNode("Light #" + _static.Lights.IndexOf(light));
                node.Tag = light;
                lstLights.Nodes.Add(node);
            }
        }

        private void UpdateVisibilityBoxUI()
        {
            nudVisBoxMinX.Value = (decimal)_static.VisibilityBox.Minimum.X;
            nudVisBoxMinY.Value = (decimal)_static.VisibilityBox.Minimum.Y;
            nudVisBoxMinZ.Value = (decimal)_static.VisibilityBox.Minimum.Z;
            nudVisBoxMaxX.Value = (decimal)_static.VisibilityBox.Maximum.X;
            nudVisBoxMaxY.Value = (decimal)_static.VisibilityBox.Maximum.Y;
            nudVisBoxMaxZ.Value = (decimal)_static.VisibilityBox.Maximum.Z;
        }

        private void EditMesh()
        {
            if (panelRendering.Static == null || panelRendering.Static.Mesh == null)
                return;

            using (var form = new FormMeshEditor(_tool, DeviceManager.DefaultDeviceManager, _tool.DestinationWad, _static.Mesh.Clone()))
            {
                if (form.ShowDialog() == DialogResult.Cancel)
                    return;

                _static.Mesh = form.SelectedMesh.Clone();
                panelRendering.UpdateMesh();
                panelRendering.Invalidate();
                UpdateLightUI();
            }
        }

        private void butCalculateVisibilityBox_Click(object sender, EventArgs e)
        {
            _static.VisibilityBox = _static.Mesh.CalculateBoundingBox(panelRendering.GizmoTransform);
            UpdateVisibilityBoxUI();
            panelRendering.Invalidate();
        }

        private void UpdateCollisionBoxUI()
        {
            nudColBoxMinX.Value = (decimal)Math.Min(_static.CollisionBox.Minimum.X, (float)nudColBoxMinX.Maximum);
            nudColBoxMinY.Value = (decimal)Math.Min(_static.CollisionBox.Minimum.Y, (float)nudColBoxMinY.Maximum);
            nudColBoxMinZ.Value = (decimal)Math.Min(_static.CollisionBox.Minimum.Z, (float)nudColBoxMinZ.Maximum);
            nudColBoxMaxX.Value = (decimal)Math.Min(_static.CollisionBox.Maximum.X, (float)nudColBoxMaxX.Maximum);
            nudColBoxMaxY.Value = (decimal)Math.Min(_static.CollisionBox.Maximum.Y, (float)nudColBoxMaxY.Maximum);
            nudColBoxMaxZ.Value = (decimal)Math.Min(_static.CollisionBox.Maximum.Z, (float)nudColBoxMaxZ.Maximum);
        }

        private void butCalculateCollisionBox_Click(object sender, EventArgs e)
        {
            _static.CollisionBox = _static.Mesh.CalculateBoundingBox(panelRendering.GizmoTransform);
            UpdateCollisionBoxUI();
            panelRendering.Invalidate();
        }

        private void cbVisibilityBox_CheckedChanged(object sender, EventArgs e)
        {
            panelRendering.DrawVisibilityBox = cbVisibilityBox.Checked;
            panelRendering.Invalidate();
        }

        private void cbCollisionBox_CheckedChanged(object sender, EventArgs e)
        {
            panelRendering.DrawCollisionBox = cbCollisionBox.Checked;
            panelRendering.Invalidate();
        }

        private void cbDrawGrid_CheckedChanged(object sender, EventArgs e)
        {
            panelRendering.DrawGrid = cbDrawGrid.Checked;
            panelRendering.Invalidate();
        }

        private void cbDrawGizmo_CheckedChanged(object sender, EventArgs e)
        {
            panelRendering.DrawGizmo = cbDrawGizmo.Checked;
            panelRendering.Invalidate();
        }

        private void butResetTranslation_Click(object sender, EventArgs e)
        {
            panelRendering.StaticPosition = Vector3.Zero;
            panelRendering.Invalidate();
        }

        private void butResetRotation_Click(object sender, EventArgs e)
        {
            panelRendering.StaticRotation = Vector3.Zero;
            panelRendering.Invalidate();
        }

        private void butResetScale_Click(object sender, EventArgs e)
        {
            panelRendering.StaticScale = 1.0f;
            panelRendering.Invalidate();
        }

        private void butAddLight_Click(object sender, EventArgs e)
        {
            panelRendering.Action = PanelRenderingStaticEditor.StaticEditorAction.PlaceLight;
        }

        public void UpdateLightUI()
        {
            if (panelRendering.SelectedLight != null)
            {
                numIntensity.Enabled = true;
                numIntensity.Value = (decimal)panelRendering.SelectedLight?.Intensity;
                numRadius.Enabled = true;
                numRadius.Value = (decimal)panelRendering.SelectedLight?.Radius;
            }
            else
            {
                numIntensity.Enabled = false;
                numRadius.Enabled = false;
            }

            comboLightType.SelectedIndex = (int)_static.Mesh.LightingType;
        }

        public void UpdateShatterUI()
        {
            if (_wad.GameVersion != TombLib.LevelData.TRVersion.Game.TombEngine)
            {
                cmbSoundID.Enabled = 
                butPlay.Enabled = 
                cbShatterable.Enabled = 
                butResetShatterAttribs.Enabled = false;
            }
            else
            {
                cbShatterable.Enabled =
                butResetShatterAttribs.Enabled = true;
                cmbSoundID.Enabled = cbShatterable.Checked;
                butPlay.Enabled = (_tool.ReferenceLevel != null) && cbShatterable.Checked;
            }

            cbShatterable.Checked = _static.Shatter;
            cmbSoundID.SelectedIndex = cmbSoundID.Items.Count - 1 > _static.ShatterSoundID ? _static.ShatterSoundID + 1 : -1;
        }

        private void numIntensity_ValueChanged(object sender, EventArgs e)
        {
            if (!_doChangesInLighting || panelRendering.SelectedLight == null)
                return;

            panelRendering.SelectedLight.Intensity = (float)numIntensity.Value;
            panelRendering.UpdateLights();
        }

        private void numInnerRange_ValueChanged(object sender, EventArgs e)
        {
            if (!_doChangesInLighting || panelRendering.SelectedLight == null)
                return;

            panelRendering.SelectedLight.Radius = (float)numRadius.Value;
            panelRendering.UpdateLights();
        }

        private void numAmbient_ValueChanged(object sender, EventArgs e)
        {
            if (!_doChangesInLighting)
                return;

            _static.AmbientLight = (short)numAmbient.Value;
            panelRendering.UpdateLights();
        }

        private void cbDrawLights_CheckedChanged(object sender, EventArgs e)
        {
            panelRendering.DrawLights = cbDrawLights.Checked;
            panelRendering.Invalidate();
        }

        private void FormStaticEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
                _static.Version = DataVersion.GetNext();
        }

        private void lstLights_Click(object sender, EventArgs e)
        {
            if (lstLights.SelectedNodes.Count == 0)
                return;

            var node = lstLights.SelectedNodes[0];
            panelRendering.SelectedLight = (WadLight)node.Tag;
            panelRendering.Invalidate();
        }

        private void butImportMeshFromFile_Click(object sender, EventArgs e)
        {
            var mesh = WadActions.ImportMesh(_tool, this);
            if (mesh == null)
                return;

            _static.Mesh = mesh;
            _static.VisibilityBox = _static.Mesh.CalculateBoundingBox(panelRendering.GizmoTransform);
            _static.CollisionBox = _static.Mesh.CalculateBoundingBox(panelRendering.GizmoTransform);
            _static.Version = DataVersion.GetNext();

            panelRendering.Invalidate();
            UpdatePositionUI();
            UpdateCollisionBoxUI();
            UpdateVisibilityBoxUI();
            UpdateLightUI();
        }

        private void nudVisBoxMinX_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudVisBoxMinX.Value;

            _static.VisibilityBox = new BoundingBox(new Vector3(result,
                                                                       _static.VisibilityBox.Minimum.Y,
                                                                       _static.VisibilityBox.Minimum.Z),
                                                           new Vector3(_static.VisibilityBox.Maximum.X,
                                                                       _static.VisibilityBox.Maximum.Y,
                                                                       _static.VisibilityBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void nudVisBoxMinY_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudVisBoxMinY.Value;

            _static.VisibilityBox = new BoundingBox(new Vector3(_static.VisibilityBox.Minimum.X,
                                                                       result,
                                                                       _static.VisibilityBox.Minimum.Z),
                                                           new Vector3(_static.VisibilityBox.Maximum.X,
                                                                       _static.VisibilityBox.Maximum.Y,
                                                                       _static.VisibilityBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void nudVisBoxMinZ_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudVisBoxMinZ.Value;

            _static.VisibilityBox = new BoundingBox(new Vector3(_static.VisibilityBox.Minimum.X,
                                                                       _static.VisibilityBox.Minimum.Y,
                                                                       result),
                                                           new Vector3(_static.VisibilityBox.Maximum.X,
                                                                       _static.VisibilityBox.Maximum.Y,
                                                                       _static.VisibilityBox.Maximum.Z));
            panelRendering.Invalidate();
        }



        private void nudVisBoxMaxX_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudVisBoxMaxX.Value;

            _static.VisibilityBox = new BoundingBox(new Vector3(_static.VisibilityBox.Minimum.X,
                                                                     _static.VisibilityBox.Minimum.Y,
                                                                     _static.VisibilityBox.Minimum.Z),
                                                         new Vector3(result,
                                                                     _static.VisibilityBox.Maximum.Y,
                                                                     _static.VisibilityBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void nudVisBoxMaxY_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudVisBoxMaxY.Value;

            _static.VisibilityBox = new BoundingBox(new Vector3(_static.VisibilityBox.Minimum.X,
                                                                     _static.VisibilityBox.Minimum.Y,
                                                                     _static.VisibilityBox.Minimum.Z),
                                                         new Vector3(_static.VisibilityBox.Maximum.X,
                                                                     result,
                                                                     _static.VisibilityBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void nudVisBoxMaxZ_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudVisBoxMaxZ.Value;

            _static.VisibilityBox = new BoundingBox(new Vector3(_static.VisibilityBox.Minimum.X,
                                                                     _static.VisibilityBox.Minimum.Y,
                                                                     _static.VisibilityBox.Minimum.Z),
                                                         new Vector3(_static.VisibilityBox.Maximum.X,
                                                                     _static.VisibilityBox.Maximum.Y,
                                                                     result));
            panelRendering.Invalidate();
        }

        private void nudColBoxMinX_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudColBoxMinX.Value;

            _static.CollisionBox = new BoundingBox(new Vector3(result,
                                                                     _static.CollisionBox.Minimum.Y,
                                                                     _static.CollisionBox.Minimum.Z),
                                                         new Vector3(_static.CollisionBox.Maximum.X,
                                                                     _static.CollisionBox.Maximum.Y,
                                                                     _static.CollisionBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void nudColBoxMinY_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudColBoxMinY.Value;

            _static.CollisionBox = new BoundingBox(new Vector3(_static.CollisionBox.Minimum.X,
                                                                     result,
                                                                     _static.CollisionBox.Minimum.Z),
                                                         new Vector3(_static.CollisionBox.Maximum.X,
                                                                     _static.CollisionBox.Maximum.Y,
                                                                     _static.CollisionBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void nudColBoxMinZ_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudColBoxMinZ.Value;

            _static.CollisionBox = new BoundingBox(new Vector3(_static.CollisionBox.Minimum.X,
                                                                     _static.CollisionBox.Minimum.Y,
                                                                     result),
                                                         new Vector3(_static.CollisionBox.Maximum.X,
                                                                     _static.CollisionBox.Maximum.Y,
                                                                     _static.CollisionBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void nudColBoxMaxX_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudColBoxMaxX.Value;

            _static.CollisionBox = new BoundingBox(new Vector3(_static.CollisionBox.Minimum.X,
                                                                     _static.CollisionBox.Minimum.Y,
                                                                     _static.CollisionBox.Minimum.Z),
                                                         new Vector3(result,
                                                                     _static.CollisionBox.Maximum.Y,
                                                                     _static.CollisionBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void nudColBoxMaxY_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudColBoxMaxY.Value;

            _static.CollisionBox = new BoundingBox(new Vector3(_static.CollisionBox.Minimum.X,
                                                                     _static.CollisionBox.Minimum.Y,
                                                                     _static.CollisionBox.Minimum.Z),
                                                         new Vector3(_static.CollisionBox.Maximum.X,
                                                                     result,
                                                                     _static.CollisionBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void nudColBoxMaxZ_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudColBoxMaxZ.Value;

            _static.CollisionBox = new BoundingBox(new Vector3(_static.CollisionBox.Minimum.X,
                                                                     _static.CollisionBox.Minimum.Y,
                                                                     _static.CollisionBox.Minimum.Z),
                                                         new Vector3(_static.CollisionBox.Maximum.X,
                                                                     _static.CollisionBox.Maximum.Y,
                                                                     result));
            panelRendering.Invalidate();
        }

        private void cbDrawNormals_CheckedChanged(object sender, EventArgs e)
        {
            panelRendering.DrawNormals = cbDrawNormals.Checked;
            panelRendering.Invalidate();
        }

        private void butDeleteLight_Click(object sender, EventArgs e)
        {
            if (!_doChangesInLighting)
                return;

            if (panelRendering.SelectedLight == null)
                return;

            WadLight light = panelRendering.SelectedLight;

            var node = lstLights.SelectedNodes[0];
            panelRendering.SelectedLight = null;
            panelRendering.DeleteLight(light);
            panelRendering.UpdateLights();
            UpdateLightsList();
            UpdateLightUI();
            panelRendering.Invalidate();
        }

        private void butRecalcNormals_Click(object sender, EventArgs e)
        {
            if (_static.Mesh != null)
            {
                _static.Mesh.CalculateNormals();
                panelRendering.UpdateLights();
                panelRendering.Invalidate();
            }
        }

        private void comboLightType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboLightType.SelectedIndex == (int)WadMeshLightingType.Normals)
            {
                _static.Mesh.LightingType = WadMeshLightingType.Normals;
                _static.Lights.Clear();
                butAddLight.Enabled = false;
                butDeleteLight.Enabled = false;
                numIntensity.Enabled = false;
                numRadius.Enabled = false;
                numAmbient.Enabled = false;
                lstLights.Enabled = false;
                UpdateLightsList();
                UpdateLightUI();
                panelRendering.Invalidate();
            }
            else
            {
                _static.Mesh.LightingType = WadMeshLightingType.VertexColors;
                butAddLight.Enabled = true;
                butDeleteLight.Enabled = true;
                numIntensity.Enabled = true;
                numRadius.Enabled = true;
                numAmbient.Enabled = true;
                lstLights.Enabled = true;
                UpdateLightUI();
                panelRendering.Invalidate();
            }
        }

        private void nudPosX_ValueChanged(object sender, EventArgs e)
        {
            panelRendering.StaticPosition = new Vector3((float)nudPosX.Value, panelRendering.StaticPosition.Y, panelRendering.StaticPosition.Z);
            panelRendering.Invalidate();
        }

        private void nudPosY_ValueChanged(object sender, EventArgs e)
        {
            panelRendering.StaticPosition = new Vector3(panelRendering.StaticPosition.X, (float)nudPosY.Value, panelRendering.StaticPosition.Z);
            panelRendering.Invalidate();
        }

        private void nudPosZ_ValueChanged(object sender, EventArgs e)
        {

            panelRendering.StaticPosition = new Vector3(panelRendering.StaticPosition.X, panelRendering.StaticPosition.Y, (float)nudPosZ.Value);
            panelRendering.Invalidate();
        }

        public void UpdatePositionUI()
        {
            nudPosX.Value = (decimal)panelRendering.StaticPosition.X;
            nudPosY.Value = (decimal)panelRendering.StaticPosition.Y;
            nudPosZ.Value = (decimal)panelRendering.StaticPosition.Z;
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void butClearCollisionBox_Click(object sender, EventArgs e)
        {
            _static.CollisionBox = new BoundingBox();
            UpdateCollisionBoxUI();
            panelRendering.Invalidate();
        }

        private void butClearVisibilityBox_Click(object sender, EventArgs e)
        {
            _static.VisibilityBox = new BoundingBox();
            UpdateVisibilityBoxUI();
            panelRendering.Invalidate();
        }

        private void butExportMeshToFile_Click(object sender, EventArgs e)
        {
            WadActions.ExportMesh(_static.Mesh, _tool, this);
        }

        private void butEditMesh_Click(object sender, EventArgs e) => EditMesh();

        private void panelRendering_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                EditMesh();
        }

        private void cbShatterable_CheckedChanged(object sender, EventArgs e)
        {
            _static.Shatter = cbShatterable.Checked;
            UpdateShatterUI();
        }

        private void cmbSoundID_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSoundID.SelectedIndex == -1)
                return;

            _static.ShatterSoundID = cmbSoundID.SelectedIndex - 1;
        }

        private void butPlay_Click(object sender, EventArgs e)
        {
            if (_tool.ReferenceLevel == null || _tool.ReferenceLevel.Settings.GlobalSoundMap.Count == 0)
                return;

            // HACK: Old shatter sound is hardcoded in both original engines and TEN.
            int soundID = cmbSoundID.SelectedIndex > 0 ? cmbSoundID.SelectedIndex - 1 : 347;

            var soundInfo = _tool.ReferenceLevel.Settings.GlobalSoundMap.FirstOrDefault(soundInfo_ => soundInfo_.Id == soundID);
            if (soundInfo != null)
                try { WadSoundPlayer.PlaySoundInfo(_tool.ReferenceLevel, soundInfo); }
                catch (Exception) { }
        }

        private void butResetShatterAttribs_Click(object sender, EventArgs e)
        {
            _static.Shatter = TrCatalog.IsStaticShatterable(_wad.GameVersion, _static.Id.TypeId);
            _static.ShatterSoundID = -1;
            UpdateShatterUI();
        }
    }
}
