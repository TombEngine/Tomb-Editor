using System.IO;
using System.Numerics;
using System.Xml.Serialization;
using TombLib.Utils;

namespace TombLib.LevelData
{
	public enum MaterialType : byte
	{
		Default,
		Reflective,
		SkyboxReflective
	}

	public class MaterialData
	{
		public MaterialType Type { get; set; }
		public string ColorMap { get; set; }
		public string NormalMap { get; set; }
		public string HeightMap { get; set; }
		public string SpecularMap { get; set; }
		public string RoughnessMap { get; set; }
		public string AmbientOcclusionMap { get; set; }
		public string AlphaMaskMap { get; set; }
		public string AdditionalColorMap { get; set; }
		public string EmissiveMap { get; set; }

		public Vector4 Parameters0 { get; set; }
		public Vector4 Parameters1 { get; set; }
		public Vector4 Parameters2 { get; set; }
		public Vector4 Parameters3 { get; set; }

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
			// Default material has:
			// Normal intensity = 1.0
			// Specular intensity = 1.0f
			// Glow intensity = 1.0f
			Type = MaterialType.Default;
			Parameters0 = new Vector4(1.0f, 1.0f, 1.0f, 0.0f);
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

			return materialData;
		}
	}
}
