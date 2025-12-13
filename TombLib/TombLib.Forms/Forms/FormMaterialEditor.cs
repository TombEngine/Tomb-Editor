using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombLib.Forms
{
	public partial class FormMaterialEditor : DarkForm
	{
		public string MaterialFileName { get; set; }
		public bool MaterialChanged => _saveXml;

		private MaterialData _materialData;
		private string _texturePath;

		private readonly Color _correctColor;
		private readonly Color _wrongColor;

		private bool _saveXml = false;
		private bool _loading = false;

		private List<Texture> _textureList;

		public FormMaterialEditor(Texture texture, ConfigurationBase configuration) : this(new List<Texture> { texture }, configuration) { }
		public FormMaterialEditor(IEnumerable<Texture> textureList, ConfigurationBase configuration, Texture selectedTexture = null)
		{
			InitializeComponent();

			_correctColor = tbNormalMapPath.BackColor;
			_wrongColor = _correctColor.MixWith(Color.DarkRed, 0.55);

			_textureList = textureList.ToList();

			// Populate material type combobox.
			foreach (MaterialType matType in Enum.GetValues(typeof(MaterialType)))
				comboMaterialType.Items.Add(matType.ToString().SplitCamelcase());

			if (_textureList is null || !_textureList.Any())
			{
				panelTextureSelect.Enabled = false;
			}
			else
			{
				panelTextureSelect.Enabled = true;

				foreach (var texture in _textureList)
					comboTexture.Items.Add(texture.AbsolutePath);

				if (selectedTexture == null)
					comboTexture.SelectedIndex = 0;
				else if (_textureList.Contains(selectedTexture))
					comboTexture.SelectedIndex = _textureList.IndexOf(selectedTexture);
			}

			// Set window property handlers.
			ConfigurationBase.ConfigureWindow(this, configuration);
		}

		private void LoadMaterialInUI()
		{
			if (_materialData is null)
				return;

			_loading = true;

			SetTexturePath(tbColorMapPath, picPreviewColorMap, _materialData.ColorMap);
			SetTexturePath(tbNormalMapPath, picPreviewNormalMap, _materialData.NormalMap);
			SetTexturePath(tbHeightMapPath, picPreviewHeightMap, _materialData.HeightMap);
			SetTexturePath(tbSpecularMapPath, picPreviewSpecularMap, _materialData.SpecularMap);
			SetTexturePath(tbAmbientOcclusionMapPath, picPreviewAmbientOcclusionMap, _materialData.AmbientOcclusionMap);
			SetTexturePath(tbEmissiveMapPath, picPreviewEmissiveMap, _materialData.EmissiveMap);
			SetTexturePath(tbRoughnessMapPath, picPreviewRoughnessMap, _materialData.RoughnessMap);

			if (!string.IsNullOrEmpty(_materialData.NormalMap))
				tbNormalMapPath.BackColor = (_materialData.IsNormalMapFound ? _correctColor : _wrongColor);

			if (!string.IsNullOrEmpty(_materialData.HeightMap))
				tbHeightMapPath.BackColor = (_materialData.IsHeightMapFound ? _correctColor : _wrongColor);

			if (!string.IsNullOrEmpty(_materialData.AmbientOcclusionMap))
				tbAmbientOcclusionMapPath.BackColor = (_materialData.IsAmbientOcclusionMapFound ? _correctColor : _wrongColor);

			if (!string.IsNullOrEmpty(_materialData.SpecularMap))
				tbSpecularMapPath.BackColor = (_materialData.IsSpecularMapFound ? _correctColor : _wrongColor);

			if (!string.IsNullOrEmpty(_materialData.EmissiveMap))
				tbEmissiveMapPath.BackColor = (_materialData.IsEmissiveMapFound ? _correctColor : _wrongColor);

			if (!string.IsNullOrEmpty(_materialData.RoughnessMap))
				tbRoughnessMapPath.BackColor = (_materialData.IsRoughnessMapFound ? _correctColor : _wrongColor);

			lblXmlMaterialFile.Text = string.IsNullOrEmpty(_materialData.XmlMaterialFileName) ? string.Empty :
				"Material settings file: " + Path.GetFileName(_materialData.XmlMaterialFileName);

			comboMaterialType.SelectedIndex = (int)_materialData.Type;
			LoadMaterialProperties();
			UpdateUI();

			_loading = false;
		}

		private void LoadTexturePreview(string path, PictureBox pictureBox)
		{
			if (string.IsNullOrEmpty(path))
			{
				pictureBox.Image?.Dispose();
				pictureBox.Image = null;
				pictureBox.BackgroundImage = TombLib.Properties.Resources.misc_TransparentBackground;
			}
			else
			{
				try
				{
					if (!string.IsNullOrEmpty(path))
					{
						pictureBox.Image?.Dispose();
						pictureBox.Image = ImageC.FromFile(path).ToBitmap();
						pictureBox.BackgroundImage = TombLib.Properties.Resources.misc_TransparentBackground;
						pictureBox.Tag = null;
						pictureBox.BackColor = _correctColor;
					}
				}
				catch (Exception exc)
				{
					pictureBox.Image = null;
					pictureBox.BackgroundImage = null;
					pictureBox.Tag = exc;
					pictureBox.BackColor = _wrongColor;
				}
			}
		}

		private void LoadMaterialProperties()
		{
			var materialType = (MaterialType)comboMaterialType.SelectedIndex;

			switch (materialType)
			{
				case MaterialType.Default:
					tabcontainerParameters.SelectedIndex = 0;
					nmNormalMapStrength.Value = (decimal)_materialData.Parameters0.X;
					nmSpecularIntensity.Value = (decimal)(_materialData.Parameters0.Y);
					break;
			}
		}

		private void SaveMaterialProperties()
		{
			if (!_saveXml)
				return;

			string externalMaterialDataPath = Path.Combine(
						Path.GetDirectoryName(_texturePath),
						Path.GetFileNameWithoutExtension(_texturePath) + ".xml");

			var materialData = new MaterialData();

			materialData.Type = (MaterialType)comboMaterialType.SelectedIndex;

			materialData.ColorMap = _texturePath;
			materialData.NormalMap = tbNormalMapPath.Text;
			materialData.HeightMap = tbHeightMapPath.Text;
			materialData.SpecularMap = tbSpecularMapPath.Text;
			materialData.EmissiveMap = tbEmissiveMapPath.Text;
			materialData.AmbientOcclusionMap = tbAmbientOcclusionMapPath.Text;
			materialData.RoughnessMap = tbRoughnessMapPath.Text;

			switch (materialData.Type)
			{
				case MaterialType.Default:
					materialData.Parameters0 = new Vector4(
							(float)nmNormalMapStrength.Value,
							(float)nmSpecularIntensity.Value,
							0.0f,
							0.0f);
					break;
			}

			bool emptyMaterial = 
				materialData.Type == MaterialType.Default &&
				string.IsNullOrEmpty(materialData.NormalMap) &&
				string.IsNullOrEmpty(materialData.HeightMap) &&
				string.IsNullOrEmpty(materialData.SpecularMap) &&
				string.IsNullOrEmpty(materialData.EmissiveMap) &&
				string.IsNullOrEmpty(materialData.AmbientOcclusionMap) &&
				string.IsNullOrEmpty(materialData.RoughnessMap);

			try
			{
				File.Delete(externalMaterialDataPath);

				if (emptyMaterial)
				{
					MaterialFileName = null;
					return;
				}

				MaterialData.SaveToXml(externalMaterialDataPath, materialData);
				MaterialFileName = externalMaterialDataPath;
			}
			catch (Exception)
			{
				DarkMessageBox.Show(this, $"An error occurred while saving XML material file to '{externalMaterialDataPath}'.",
					"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void UpdateUI()
		{
			butClearNormalMap.Enabled = !string.IsNullOrEmpty(tbNormalMapPath.Text);
			butClearHeightMap.Enabled = !string.IsNullOrEmpty(tbHeightMapPath.Text);
			butClearAmbientOcclusionMap.Enabled = !string.IsNullOrEmpty(tbAmbientOcclusionMapPath.Text);
			butClearEmissiveMap.Enabled = !string.IsNullOrEmpty(tbEmissiveMapPath.Text);
			butClearSpecularMap.Enabled = !string.IsNullOrEmpty(tbSpecularMapPath.Text);
			butClearRoughnessMap.Enabled = !string.IsNullOrEmpty(tbRoughnessMapPath.Text);
		}

		private void SetTexturePath(TextBox textBox, PictureBox previewBox, string texturePath)
		{
			textBox.Text = texturePath;
			textBox.SelectionStart = textBox.Text.Length;
			textBox.ScrollToCaret();
			textBox.BackColor = _correctColor;
			LoadTexturePreview(texturePath, previewBox);
		}

		private void BrowseTexture(TextBox textBox, PictureBox previewBox)
		{
			var texturePath = LevelFileDialog.BrowseFile(this, "Browse texture", ImageC.FileExtensions, false);

			if (!string.IsNullOrEmpty(texturePath))
			{
				SetTexturePath(textBox, previewBox, texturePath);
				UpdateUI();
				_saveXml = true;
			}
		}

		private void ClearTexture(TextBox textBox, PictureBox previewBox, string mapName)
		{
			textBox.Text = string.Empty;
			textBox.BackColor = _correctColor;
			LoadTexturePreview(string.Empty, previewBox);
			UpdateUI();
			_saveXml = true;
		}

		private void butCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void butOK_Click(object sender, EventArgs e)
		{
			SaveMaterialProperties();
			DialogResult = DialogResult.OK;
			Close();
		}

		private void comboMaterialType_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_loading)
				return;

			LoadMaterialProperties();
			_saveXml = true;
		}

		private void comboTexture_SelectedIndexChanged(object sender, EventArgs e)
		{
			var texture = _textureList.ElementAt(comboTexture.SelectedIndex);

			if (_saveXml)
			{
				if (DarkMessageBox.Show(this, "Save changes to current material?", "Confirm changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					SaveMaterialProperties();

				_saveXml = false;
			}

			MaterialData material;
			try
			{
				material = MaterialData.TrySidecarLoadOrLoadExisting(texture.AbsolutePath);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, "There was an error while loading the selected material. Using default.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				material = new MaterialData() { ColorMap = texture.AbsolutePath };
				_saveXml = true;
			}

			_materialData = material;
			_texturePath = material.ColorMap;

			LoadMaterialInUI();
		}

		private void butBrowseNormalMap_Click(object sender, EventArgs e) => BrowseTexture(tbNormalMapPath, picPreviewNormalMap);
		private void butBrowseHeightMap_Click(object sender, EventArgs e) => BrowseTexture(tbHeightMapPath, picPreviewHeightMap);
		private void butBrowseAmbientOcclusionMap_Click(object sender, EventArgs e) => BrowseTexture(tbAmbientOcclusionMapPath, picPreviewAmbientOcclusionMap);
		private void butBrowseEmissiveMap_Click(object sender, EventArgs e) => BrowseTexture(tbEmissiveMapPath, picPreviewEmissiveMap);
		private void butBrowseSpecularMap_Click(object sender, EventArgs e) => BrowseTexture(tbSpecularMapPath, picPreviewSpecularMap);
		private void butBrowseRoughnessMap_Click(object sender, EventArgs e) => BrowseTexture(tbRoughnessMapPath, picPreviewRoughnessMap);

		private void butClearNormalMap_Click(object sender, EventArgs e) => ClearTexture(tbNormalMapPath, picPreviewNormalMap, "normal");
		private void butClearHeightMap_Click(object sender, EventArgs e) => ClearTexture(tbHeightMapPath, picPreviewHeightMap, "height");
		private void butClearAmbientOcclusionMap_Click(object sender, EventArgs e) => ClearTexture(tbAmbientOcclusionMapPath, picPreviewAmbientOcclusionMap, "ambient occlusion");
		private void butClearEmissiveMap_Click(object sender, EventArgs e) => ClearTexture(tbEmissiveMapPath, picPreviewEmissiveMap, "emissive");
		private void butClearSpecularMap_Click(object sender, EventArgs e) => ClearTexture(tbSpecularMapPath, picPreviewSpecularMap, "specular");
		private void butClearRoughnessMap_Click(object sender, EventArgs e) => ClearTexture(tbRoughnessMapPath, picPreviewRoughnessMap, "roughness");

		private void nmNormalMapStrength_ValueChanged(object sender, EventArgs e) => _saveXml = true;
		private void nmSpecularIntensity_ValueChanged(object sender, EventArgs e) => _saveXml = true;
	}
}
