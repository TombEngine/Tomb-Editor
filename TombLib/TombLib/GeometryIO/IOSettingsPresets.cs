using System.Collections.Generic;
using TombLib.LevelData;

namespace TombLib.GeometryIO;

public static class IOSettingsPresets
{
	public static List<IOGeometrySettingsPreset> GeometryImportSettingsPresets { get; }
	public static List<IOGeometrySettingsPreset> GeometryExportSettingsPresets { get; }
	public static List<IOGeometrySettingsPreset> AnimationSettingsPresets { get; }

	static IOSettingsPresets()
	{
		GeometryExportSettingsPresets = new List<IOGeometrySettingsPreset>
		{
			new("Normal scale", new IOGeometrySettings
			{
				Export = true,
				Scale = Level.SectorSizeUnit,
				FlipUV_V = true
			}),
			new("Keep original TR scale", new IOGeometrySettings
			{
				Export = true,
				Scale = 1.0f,
				FlipUV_V = true
			})
		};

		GeometryImportSettingsPresets = new List<IOGeometrySettingsPreset>
		{
			// Generic
			new ("Normal scale to TR scale", new IOGeometrySettings
			{
				Scale = Level.SectorSizeUnit,
			}),

			// Metasequoia
			new("Metasequoia MQO unscaled", new IOGeometrySettings
			{
				Scale = 1.0f,
				FlipZ = true,
				InvertFaces = false,
				FlipUV_V = false,
				PremultiplyUV = true,
				WrapUV = true,
				UseVertexColor = true
			}),
			new("Metasequoia MQO Scale 1024", new IOGeometrySettings
			{
				Scale = Level.SectorSizeUnit,
				FlipZ = true,
				InvertFaces = false,
				FlipUV_V = false,
				PremultiplyUV = true,
				WrapUV = true,
				UseVertexColor = true
			}),
			new("Metasequoia MQO Scale 1/1024", new IOGeometrySettings
			{
				Scale = 1.0f / Level.SectorSizeUnit,
				FlipZ = true,
				InvertFaces = false,
				FlipUV_V = false,
				PremultiplyUV = true,
				WrapUV = true,
				UseVertexColor = true
			}),
			new("Metasequoia OBJ", new IOGeometrySettings
			{
				Scale = Level.SectorSizeUnit,
				FlipZ = true,
				InvertFaces = true,
				FlipUV_V = true,
				PremultiplyUV = true,
				WrapUV = true
			}),
			new("Metasequoia PLY", new IOGeometrySettings
			{
				Scale = Level.SectorSizeUnit,
				SwapYZ = true,
				FlipZ = false,
				FlipUV_V = true,
				PremultiplyUV = true,
				WrapUV = true
			}),

			// Blender
			new("Blender DAE", new IOGeometrySettings
			{
				Scale = Level.SectorSizeUnit,
				FlipZ = true,
				FlipUV_V = true,
				PremultiplyUV = true,
				WrapUV = true
			}),
			new("Blender PLY", new IOGeometrySettings
			{
				Scale = Level.SectorSizeUnit,
				SwapYZ = true,
				FlipZ = false,
				FlipUV_V = true,
				PremultiplyUV = true,
				WrapUV = true
			}),

			// 3ds Max
			new("3ds Max FBX", new IOGeometrySettings
			{
				Scale = 1.0f,
				FlipZ = true,
				FlipUV_V = true,
				InvertFaces = true,
				PremultiplyUV = true,
				WrapUV = true
			}),
			new("3ds Max OBJ", new IOGeometrySettings
			{
				Scale = 1.0f,
				FlipZ = true,
				FlipUV_V = true,
				InvertFaces = true,
				PremultiplyUV = true,
				WrapUV = true
			}),
			new("TRViewer 3DS", new IOGeometrySettings
			{
				Scale = 1.0f,
				FlipZ = false,
				SwapYZ = true,
				FlipUV_V = true,
				InvertFaces = true,
				PremultiplyUV = true,
				WrapUV = true
			})
		};

		AnimationSettingsPresets = new List<IOGeometrySettingsPreset>
		{
			new("3dsmax COLLADA", new IOGeometrySettings
			{
				ProcessAnimations = true,
				ProcessGeometry = false,
				SwapYZ = true
			}),
			new("3dsmax Filmbox (FBX)", new IOGeometrySettings
			{
				ProcessAnimations = true,
				ProcessGeometry = false
			})
		};
	}
}
