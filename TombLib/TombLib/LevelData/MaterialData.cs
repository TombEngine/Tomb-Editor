using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Xml.Serialization;
using TombLib.Utils;

namespace TombLib.LevelData
{
	public class MaterialData
	{
		public const int PropertyCount = MaterialPropertyDefinition.MaxPropertyCount;
		public const string DefaultMaterialName = "DefaultMaterial";

		[XmlIgnore]
		public int Type { get; set; }

		[XmlElement("Type")]
		public string SerializedType
		{
			get => Type.ToString(CultureInfo.InvariantCulture);
			set => MaterialCatalog.ParseMaterialType(this, value);
		}

		public string Name { get; set; }
		public string ColorMap { get; set; }
		public string NormalMap { get; set; }
		public string HeightMap { get; set; }
		public string SpecularMap { get; set; }
		public string RoughnessMap { get; set; }
		public string AmbientOcclusionMap { get; set; }
		public string AlphaMaskMap { get; set; }
		public string AdditionalColorMap { get; set; }
		public string EmissiveMap { get; set; }

		public string[] Properties { get; set; }

		[XmlIgnore]
		public bool IsNormalMapFound { get; private set; }
		[XmlIgnore]
		public bool IsHeightMapFound { get; private set; }
		[XmlIgnore]
		public bool IsSpecularMapFound { get; private set; }
		[XmlIgnore]
		public bool IsRoughnessMapFound { get; private set; }
		[XmlIgnore]
		public bool IsAmbientOcclusionMapFound { get; private set; }
		[XmlIgnore]
		public bool IsAlphaMaskMapFound { get; private set; }
		[XmlIgnore]
		public bool IsAdditionalColorMapFound { get; private set; }
		[XmlIgnore]
		public bool IsEmissiveMapFound { get; private set; }

		[XmlIgnore]
		public string XmlMaterialFileName { get; set; }

		public MaterialData()
		{
			Type = 0;
			Name = string.Empty;
			Properties = new string[PropertyCount];
			ApplyDefinitionDefaults();
		}

		public static MaterialData ReadFromXml(string filename)
		{
			var materialData = XmlUtils.ReadXmlFile<MaterialData>(filename);

			materialData.XmlMaterialFileName = filename;

			// Make all paths absolute.
			if (materialData is null)
				return null;

			var basePath = Path.GetDirectoryName(filename);

			(string path, bool found) LoadPath(string path)
			{
				if (string.IsNullOrEmpty(path))
					return (path, false);

				if (!PathC.IsTrulyAbsolutePath(path))
					path = Path.Combine(basePath, path);

				bool found = File.Exists(path);
				return (path, found);
			}

			(materialData.ColorMap, _) = LoadPath(materialData.ColorMap);
			(materialData.NormalMap, materialData.IsNormalMapFound) = LoadPath(materialData.NormalMap);
			(materialData.HeightMap, materialData.IsHeightMapFound) = LoadPath(materialData.HeightMap);
			(materialData.EmissiveMap, materialData.IsEmissiveMapFound) = LoadPath(materialData.EmissiveMap);
			(materialData.SpecularMap, materialData.IsSpecularMapFound) = LoadPath(materialData.SpecularMap);
			(materialData.RoughnessMap, materialData.IsRoughnessMapFound) = LoadPath(materialData.RoughnessMap);
			(materialData.AmbientOcclusionMap, materialData.IsAmbientOcclusionMapFound) = LoadPath(materialData.AmbientOcclusionMap);

			materialData.Normalize();

			return materialData;
		}

		public static bool SaveToXml(string filename, MaterialData materialData)
		{
			if (materialData == null)
				return false;

			string baseDir = Path.GetDirectoryName(filename);

			string MakeRelative(string mapPath)
			{
				if (string.IsNullOrEmpty(mapPath) || !File.Exists(mapPath))
					return string.Empty;

				var relativePath = PathC.GetRelativePath(baseDir, mapPath);
				if (relativePath is not null)
					return relativePath;
				else
					return string.Empty;
			}

			materialData.ColorMap = MakeRelative(materialData.ColorMap);
			materialData.NormalMap = MakeRelative(materialData.NormalMap);
			materialData.HeightMap = MakeRelative(materialData.HeightMap);
			materialData.SpecularMap = MakeRelative(materialData.SpecularMap);
			materialData.EmissiveMap = MakeRelative(materialData.EmissiveMap);
			materialData.AmbientOcclusionMap = MakeRelative(materialData.AmbientOcclusionMap);
			materialData.RoughnessMap = MakeRelative(materialData.RoughnessMap);
			materialData.Normalize();

			XmlUtils.WriteXmlFile(filename, materialData);
			return true;
		}

		public static MaterialData TrySidecarLoadOrLoadExisting(string textureAbsolutePath)
		{
			if (string.IsNullOrEmpty(textureAbsolutePath))
				return null;

			string baseDir = Path.GetDirectoryName(textureAbsolutePath);
			string baseName = Path.GetFileNameWithoutExtension(textureAbsolutePath);
			string ext = Path.GetExtension(textureAbsolutePath);

			string externalMaterialDataPath = Path.Combine(baseDir, baseName + ".xml");

			// If XML material file exists, just load it.
			if (!string.IsNullOrEmpty(externalMaterialDataPath) && File.Exists(externalMaterialDataPath))
				return ReadFromXml(externalMaterialDataPath);

			var materialData = new MaterialData { ColorMap = textureAbsolutePath };

			// Clear textures which are not found in this case.
			// Instead of XML, keep paths and set to false their IsXYZFound properties 
			// so we can show the problem in material editor.
			(string path, bool found) CreateSidecar(string suffix)
			{
				string path = Path.Combine(baseDir, baseName + suffix + ext);
				return File.Exists(path) ? (path, true) : (string.Empty, false);
			}

			// Build sidecar maps.
			(materialData.NormalMap, materialData.IsNormalMapFound) = CreateSidecar("_N");
			(materialData.HeightMap, materialData.IsHeightMapFound) = CreateSidecar("_H");
			(materialData.SpecularMap, materialData.IsSpecularMapFound) = CreateSidecar("_S");
			(materialData.AmbientOcclusionMap, materialData.IsAmbientOcclusionMapFound) = CreateSidecar("_AO");
			(materialData.RoughnessMap, materialData.IsRoughnessMapFound) = CreateSidecar("_R");
			(materialData.EmissiveMap, materialData.IsEmissiveMapFound) = CreateSidecar("_E");
			materialData.Normalize();

			return materialData;
		}

		public static MaterialData TryLoadForTexture(Texture texture, string textureAbsolutePath = null)
		{
			var resolvedTexturePath = textureAbsolutePath ?? texture?.AbsolutePath;
			if (string.IsNullOrEmpty(resolvedTexturePath) && texture != null)
				resolvedTexturePath = texture.Image.FileName;

			return TrySidecarLoadOrLoadExisting(resolvedTexturePath);
		}

		public MaterialTypeDefinition GetMaterialDefinition()
		{
			return MaterialCatalog.GetDefinition(Type);
		}

		public Vector4 GetPropertyVector(int index)
		{
			var property = GetPropertyDefinition(index);
			if (property == null || property.Type == MaterialPropertyType.None)
				return Vector4.Zero;

			return MaterialCatalog.UnboxValue(property.Type, Properties[index]);
		}

		public MaterialPropertyDefinition GetPropertyDefinition(int index)
		{
			if (index < 0 || index >= PropertyCount)
				return null;

			return GetMaterialDefinition().Properties[index];
		}

		public void SetPropertyValue(int index, string value)
		{
			if (index < 0 || index >= PropertyCount)
				return;

			Properties[index] = value ?? string.Empty;
		}

		public void Normalize()
		{
			Properties ??= new string[PropertyCount];

			if (Properties.Length != PropertyCount)
			{
				var properties = Properties;
				Array.Resize(ref properties, PropertyCount);
				Properties = properties;
			}

			if (string.IsNullOrWhiteSpace(Name))
				Name = GetDefaultMaterialName();

			var definition = GetMaterialDefinition();

			for (int i = 0; i < PropertyCount; i++)
			{
				var property = definition.Properties[i];
				if (property == null || property.Type == MaterialPropertyType.None)
				{
					Properties[i] = string.Empty;
					continue;
				}

				if (string.IsNullOrWhiteSpace(Properties[i]))
					Properties[i] = MaterialCatalog.GetDefaultValue(property);
			}
		}

		public void ApplyDefinitionDefaults()
		{
			var definition = MaterialCatalog.GetDefinition(Type);
			for (int i = 0; i < PropertyCount; i++)
			{
				var property = definition.Properties[i];
				Properties[i] = MaterialCatalog.GetDefaultValue(property);
			}
		}

		private string GetDefaultMaterialName()
		{
			if (!string.IsNullOrWhiteSpace(ColorMap))
				return Path.GetFileNameWithoutExtension(ColorMap);

			return DefaultMaterialName;
		}
	}
}
