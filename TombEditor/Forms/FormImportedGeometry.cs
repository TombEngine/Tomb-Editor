using System;
using System.Numerics;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.Forms
{
	public partial class FormImportedGeometry : DarkForm
	{
		public LevelSettings OldLevelSettings { get; }
		public LevelSettings NewLevelSettings { get; }

		private readonly ImportedGeometryInstance _instance;
		private ImportedGeometry.UniqueIDType _currentModel; // Refer to the current geometry by ID to identify it on old and new level settings.
		private Vector3 oldColor;

		public FormImportedGeometry(ImportedGeometryInstance instance, LevelSettings levelSettings)
		{
			InitializeComponent();
			_instance = instance;
			_currentModel = instance.Model?.UniqueID;
			oldColor = instance.Color;
			OldLevelSettings = levelSettings;
			NewLevelSettings = levelSettings.Clone();
			importedGeometryManager.LevelSettings = NewLevelSettings;
			comboLightingModel.SelectedIndex = (int)_instance.LightingModel;
			panelColor.BackColor = (_instance.Color * 0.5f).ToWinFormsColor();
			cbSharpEdges.Checked = _instance.SharpEdges;
			cbHide.Checked = _instance.Hidden;
			cbAlphaTest.Enabled = levelSettings.GameVersion == TRVersion.Game.TombEngine;
			if (cbAlphaTest.Enabled)
				cbAlphaTest.Checked = _instance.UseAlphaTestInsteadOfAlphaBlend;

			// Set window property handlers
			Configuration.ConfigureWindow(this, Editor.Instance.Configuration);
		}

		private void UpdateCurrentModelDisplay()
		{
			ImportedGeometry currentModelObj = NewLevelSettings.ImportedGeometryFromID(_currentModel);
			if (currentModelObj == null)
				importedGeometryLabel.Text = "None";
			else
				importedGeometryLabel.Text = currentModelObj.Info.Name + "   (" + currentModelObj.Info.Path + ")";
		}

		private void Assign()
		{
			if (importedGeometryManager.SelectedImportedGeometry != null)
			{
				_currentModel = importedGeometryManager.SelectedImportedGeometry.UniqueID;
				UpdateCurrentModelDisplay();
			}
		}

		private void butAssign_Click(object sender, EventArgs e) => Assign();
		private void importedGeometryManager_MouseDoubleClick(object sender, MouseEventArgs e) => Assign();

		private void butOk_Click(object sender, EventArgs e)
		{
			_instance.Model = OldLevelSettings.ImportedGeometryFromID(_currentModel) ?? NewLevelSettings.ImportedGeometryFromID(_currentModel);
			_instance.LightingModel = (ImportedGeometryLightingModel)comboLightingModel.SelectedIndex;
			_instance.Color = panelColor.BackColor.ToFloat3Color() * 2.0f;
			_instance.SharpEdges = cbSharpEdges.Checked;
			_instance.Hidden = cbHide.Checked;
			_instance.UseAlphaTestInsteadOfAlphaBlend = cbAlphaTest.Checked;
			DialogResult = DialogResult.OK;
			Close();
		}

		private void butCancel_Click(object sender, EventArgs e)
		{
			_instance.Color = oldColor;
			DialogResult = DialogResult.Cancel;
			Close();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			if (_currentModel != null && NewLevelSettings != null)
				importedGeometryManager.SelectedImportedGeometry = NewLevelSettings.ImportedGeometryFromID(_currentModel);
			UpdateCurrentModelDisplay();
		}

		private void panelColor_Click(object sender, EventArgs e)
		{
			EditorActions.EditColor(this, _instance, (Vector3 newColor) =>
			{
				panelColor.BackColor = newColor.ToWinFormsColor();
			});
		}

		private void butEditMaterials_Click(object sender, EventArgs e)
		{
			var currentModelObj = NewLevelSettings.ImportedGeometryFromID(_currentModel);
			if (currentModelObj == null)
			{
				DarkMessageBox.Show(this, "You need to assign a model before opening the Material Editor.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (currentModelObj.Textures.Count == 0 || currentModelObj.LoadException != null)
			{
				DarkMessageBox.Show(this, "Assigned model has no textures or was not loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			using (var form = new FormMaterialEditor(currentModelObj.Textures, Editor.Instance.Configuration))
			{
				form.ShowDialog();
			}
		}
	}
}
