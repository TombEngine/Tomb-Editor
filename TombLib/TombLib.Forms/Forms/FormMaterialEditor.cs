using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombLib.Controls.VisualScripting;
using TombLib.LevelData;
using TombLib.LevelData.VisualScripting;
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
		private readonly ToolTip _propertyToolTip = new ToolTip();
		private readonly List<MaterialTypeDefinition> _materialDefinitions = new List<MaterialTypeDefinition>();
		private readonly List<ArgumentEditor> _propertyEditors = new List<ArgumentEditor>();
		private readonly List<DarkUI.Controls.DarkLabel> _propertyLabels;

		private bool _saveXml = false;
		private bool _loading = false;

		private List<Texture> _textureList;

		public FormMaterialEditor(Texture texture, ConfigurationBase configuration) : this(new List<Texture> { texture }, configuration) { }
		public FormMaterialEditor(IEnumerable<Texture> textureList, ConfigurationBase configuration, Texture selectedTexture = null)
		{
			InitializeComponent();

			_correctColor = tbNormalMapPath.BackColor;
			_wrongColor = _correctColor.MixWith(Color.DarkRed, 0.55);
			_propertyLabels = new List<DarkUI.Controls.DarkLabel> { lblProp1, lblProp2, lblProp3, lblProp4 };
			_propertyEditors.AddRange(new[] { propertyEditor1, propertyEditor2, propertyEditor3, propertyEditor4 });

			_textureList = textureList.ToList();
			darkTextBox1.ReadOnly = false;
			darkTextBox1.TextChanged += darkTextBox1_TextChanged;

			PopulateMaterialTypes();

			if (_textureList is null || !_textureList.Any())
			{
				panelTextureSelect.Enabled = false;
			}
			else
			{
				panelTextureSelect.Enabled = true;

				foreach (var texture in _textureList)
					comboTexture.Items.Add(GetTexturePath(texture));

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
			_materialData.Normalize();

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

			darkTextBox1.Text = _materialData.Name;
			SelectMaterialType(_materialData.Type);
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
				catch (Exception ex)
				{
					pictureBox.Image = null;
					pictureBox.BackgroundImage = null;
					pictureBox.Tag = ex;
					pictureBox.BackColor = _wrongColor;
				}
			}
		}

		private void LoadMaterialProperties()
		{
			var materialType = GetSelectedMaterialType();
			if (materialType == null)
				return;

			for (int i = 0; i < _propertyEditors.Count; i++)
				ConfigurePropertyEditor(i, materialType.Properties[i]);
		}

		private void SaveMaterialProperties()
		{
			if (!_saveXml)
				return;

			SaveMaterialValuesFromUi();

			string externalMaterialDataPath = Path.Combine(
						Path.GetDirectoryName(_texturePath),
						Path.GetFileNameWithoutExtension(_texturePath) + ".xml");

			var materialData = CreateMaterialSnapshot();

			bool emptyMaterial = IsMaterialDefault(materialData);

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

			ApplySelectedMaterialType();
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
				material = MaterialData.TryLoadForTexture(texture, GetTexturePath(texture));
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, "There was an error while loading the selected material. Using default.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				material = new MaterialData() { ColorMap = GetTexturePath(texture) };
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

		private void darkTextBox1_TextChanged(object sender, EventArgs e)
		{
			if (_loading || _materialData == null)
				return;

			_materialData.Name = darkTextBox1.Text;
			_saveXml = true;
		}

		private void PropertyEditor_ValueChanged(object sender, EventArgs e)
		{
			if (_loading || _materialData == null)
				return;

			SaveMaterialValuesFromUi();
			_saveXml = true;
		}

		private void PopulateMaterialTypes()
		{
			comboMaterialType.Items.Clear();
			_materialDefinitions.Clear();

			foreach (var definition in MaterialCatalog.Definitions.OrderBy(definition => definition.Id))
			{
				_materialDefinitions.Add(definition);
				comboMaterialType.Items.Add(definition.Name);
			}
		}

		private void ConfigurePropertyEditor(int index, MaterialPropertyDefinition definition)
		{
			var editor = _propertyEditors[index];
			var label = _propertyLabels[index];

			if (definition == null || !definition.IsDefined)
			{
				label.Text = "(Not available)";
				editor.SetArgumentType(CreateUnavailableLayout(), null);
				editor.Enabled = label.Enabled = false;
				editor.Text = string.Empty;
				editor.SetToolTip(_propertyToolTip, string.Empty);
			}
			else
			{
				label.Text = definition.Name + ":";
				editor.Enabled = label.Enabled = true;
				editor.SetArgumentType(CreateArgumentLayout(definition), null);
				editor.Text = _materialData.Properties[index];
				editor.SetToolTip(_propertyToolTip, definition.Description ?? string.Empty);
			}
		}

		private void ApplySelectedMaterialType()
		{
			var selectedType = GetSelectedMaterialType();
			if (selectedType == null || _materialData == null)
				return;

			SaveMaterialValuesFromUi();

			var previousDefinition = _materialData.GetMaterialDefinition();
			var previousValues = (string[])_materialData.Properties.Clone();

			_materialData.Type = selectedType.Id;
			_materialData.Normalize();

			for (int i = 0; i < MaterialData.PropertyCount; i++)
			{
				var nextProperty = selectedType.Properties[i];
				if (nextProperty == null || !nextProperty.IsDefined)
				{
					_materialData.Properties[i] = string.Empty;
					continue;
				}

				var previousProperty = previousDefinition.Properties[i];
				if (previousProperty != null && previousProperty.Type == nextProperty.Type && !string.IsNullOrWhiteSpace(previousValues[i]))
					_materialData.Properties[i] = previousValues[i];
				else
					_materialData.Properties[i] = MaterialCatalog.GetDefaultValue(nextProperty);
			}
		}

		private void SaveMaterialValuesFromUi()
		{
			if (_materialData == null)
				return;

			_materialData.Name = darkTextBox1.Text;

			for (int i = 0; i < MaterialData.PropertyCount; i++)
			{
				var definition = _materialData.GetPropertyDefinition(i);
				if (definition == null || !definition.IsDefined || !_propertyEditors[i].Enabled)
					_materialData.Properties[i] = string.Empty;
				else
					_materialData.Properties[i] = _propertyEditors[i].Text;
			}
		}

		private MaterialData CreateMaterialSnapshot()
		{
			var materialData = new MaterialData
			{
				Type = GetSelectedMaterialType()?.Id ?? _materialData?.Type ?? 0,
				Name = darkTextBox1.Text,
				ColorMap = _texturePath,
				NormalMap = tbNormalMapPath.Text,
				HeightMap = tbHeightMapPath.Text,
				SpecularMap = tbSpecularMapPath.Text,
				EmissiveMap = tbEmissiveMapPath.Text,
				AmbientOcclusionMap = tbAmbientOcclusionMapPath.Text,
				RoughnessMap = tbRoughnessMapPath.Text,
				Properties = (string[])_materialData.Properties.Clone()
			};

			materialData.Normalize();
			return materialData;
		}

		private MaterialTypeDefinition GetSelectedMaterialType()
		{
			if (comboMaterialType.SelectedIndex < 0 || comboMaterialType.SelectedIndex >= _materialDefinitions.Count)
				return null;

			return _materialDefinitions[comboMaterialType.SelectedIndex];
		}

		private void SelectMaterialType(int type)
		{
			var index = _materialDefinitions.FindIndex(definition => definition.Id == type);
			if (index < 0)
			{
				var definition = MaterialCatalog.GetDefinition(type);
				_materialDefinitions.Add(definition);
				comboMaterialType.Items.Add(definition.Name);
				index = _materialDefinitions.Count - 1;
			}

			comboMaterialType.SelectedIndex = index;
		}

		private bool IsMaterialDefault(MaterialData materialData)
		{
			if (materialData.Type != 0)
				return false;

			if (!string.Equals(materialData.Name, Path.GetFileNameWithoutExtension(_texturePath), StringComparison.OrdinalIgnoreCase))
				return false;

			if (!string.IsNullOrEmpty(materialData.NormalMap) ||
				!string.IsNullOrEmpty(materialData.HeightMap) ||
				!string.IsNullOrEmpty(materialData.SpecularMap) ||
				!string.IsNullOrEmpty(materialData.EmissiveMap) ||
				!string.IsNullOrEmpty(materialData.AmbientOcclusionMap) ||
				!string.IsNullOrEmpty(materialData.RoughnessMap))
			{
				return false;
			}

			var defaultDefinition = MaterialCatalog.GetDefinition(materialData.Type);
			for (int i = 0; i < MaterialData.PropertyCount; i++)
			{
				var definition = defaultDefinition.Properties[i];
				var currentValue = materialData.Properties[i] ?? string.Empty;
				var defaultValue = MaterialCatalog.GetDefaultValue(definition);
				if (!string.Equals(currentValue, defaultValue, StringComparison.Ordinal))
					return false;
			}

			return true;
		}

		private ArgumentLayout CreateArgumentLayout(MaterialPropertyDefinition property)
		{
			var layout = new ArgumentLayout
			{
				Name = property.Name,
				Description = property.Description ?? string.Empty,
				DefaultValue = property.DefaultValue ?? string.Empty
			};

			switch (property.Type)
			{
				case MaterialPropertyType.Bool:
					layout.Type = ArgumentType.Boolean;
					break;

				case MaterialPropertyType.Int:
					layout.Type = ArgumentType.Numerical;
					layout.CustomEnumeration.AddRange(CreateNumericLayoutParameters(property, false));
					break;

				case MaterialPropertyType.Float:
					layout.Type = ArgumentType.Numerical;
					layout.CustomEnumeration.AddRange(CreateNumericLayoutParameters(property, true));
					break;

				case MaterialPropertyType.Vec2:
					layout.Type = ArgumentType.Vector2;
					layout.CustomEnumeration.AddRange(CreateNumericLayoutParameters(property, true));
					break;

				case MaterialPropertyType.Vec3:
					layout.Type = ArgumentType.Vector3;
					layout.CustomEnumeration.AddRange(CreateNumericLayoutParameters(property, true));
					break;

				case MaterialPropertyType.Color:
					layout.Type = ArgumentType.Color;
					break;

				default:
					layout.Type = ArgumentType.String;
					break;
			}

			return layout;
		}

		private string[] CreateNumericLayoutParameters(MaterialPropertyDefinition property, bool isFractional)
		{
			var min = property.MinValue ?? -1000000.0f;
			var max = property.MaxValue ?? 1000000.0f;

			if (min > max)
				(min, max) = (max, min);

			if (isFractional)
			{
				return new[]
				{
					min.ToString(CultureInfo.InvariantCulture),
					max.ToString(CultureInfo.InvariantCulture),
					"3",
					"0.1",
					"1.0"
				};
			}

			return new[]
			{
				min.ToString(CultureInfo.InvariantCulture),
				max.ToString(CultureInfo.InvariantCulture),
				"0",
				"1",
				"10"
			};
		}

		private ArgumentLayout CreateUnavailableLayout()
		{
			return new ArgumentLayout
			{
				Type = ArgumentType.Numerical,
				Description = string.Empty,
				DefaultValue = string.Empty
			};
		}

		private string GetTexturePath(Texture texture)
		{
			if (texture == null)
				return string.Empty;

			return texture.AbsolutePath ?? texture.Image.FileName ?? string.Empty;
		}
	}
}
