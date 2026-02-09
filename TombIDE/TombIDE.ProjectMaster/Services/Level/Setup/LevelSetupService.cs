using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombIDE.Shared.NewStructure;
using TombIDE.Shared.NewStructure.Implementations;
using TombIDE.Shared.SharedClasses;
using TombLib;
using TombLib.LevelData;
using TombLib.LevelData.IO;
using TombLib.Utils;

namespace TombIDE.ProjectMaster.Services.Level.Setup;

public sealed class LevelSetupService : ILevelSetupService
{
	public LevelSetupResult CreateLevel(IGameProject targetProject, LevelSetupOptions options)
	{
		string? levelName = LevelNameHelper.ValidateLevelName(options.LevelName)
			?? throw new ArgumentException("You must enter a valid name for your level.");

		if (!levelName.IsANSI())
			throw new ArgumentException("The level name contains illegal characters. Please use only English characters and numbers.");

		string? dataFileName = LevelNameHelper.ValidateDataFileName(options.DataFileName)
			?? throw new ArgumentException("You must specify a valid custom PRJ2 / DATA file name.");

		if (!dataFileName.IsANSI())
			throw new ArgumentException("The data file name contains illegal characters. Please use only English characters and numbers.");

		string levelFolderPath = Path.Combine(targetProject.LevelsDirectoryPath, levelName);

		// Create the level folder and ensure it's empty
		if (!Directory.Exists(levelFolderPath))
			Directory.CreateDirectory(levelFolderPath);

		if (Directory.EnumerateFileSystemEntries(levelFolderPath).Any())
		{
			throw new ArgumentException("A folder with the same name as the \"Level name\" already exists in\n" +
				"the project's /Levels/ folder and it's not empty.");
		}

		ILevelProject createdLevel = new LevelProject(levelName, levelFolderPath);
		ScriptGenerationResult? generatedScript = null;

		// Create the .prj2 file
		CreatePrj2File(targetProject, createdLevel, dataFileName);

		if (options.GenerateScript)
			generatedScript = ScriptGenerator.GenerateScripts(levelName, dataFileName, targetProject.GameVersion, options.AmbientSoundId, options.EnableHorizon);

		var result = new LevelSetupResult
		{
			CreatedLevel = createdLevel,
			GeneratedScript = generatedScript
		};

		createdLevel.Save();

		return result;
	}

	private static void CreatePrj2File(IGameProject targetProject, ILevelProject level, string dataFileName)
	{
		var prj2Level = TombLib.LevelData.Level.CreateSimpleLevel();

		string prj2FilePath = Path.Combine(level.DirectoryPath, dataFileName) + ".prj2";
		string exeFilePath = targetProject.GetEngineExecutableFilePath();
		string engineDirectory = targetProject.GetEngineRootDirectoryPath();
		string dataFilePath = Path.Combine(engineDirectory, "data", dataFileName + targetProject.DataFileExtension);

		prj2Level.Settings.LevelFilePath = prj2FilePath;

		prj2Level.Settings.GameDirectory = prj2Level.Settings.MakeRelative(engineDirectory, VariableType.LevelDirectory);
		prj2Level.Settings.GameExecutableFilePath = prj2Level.Settings.MakeRelative(exeFilePath, VariableType.LevelDirectory);
		prj2Level.Settings.ScriptDirectory = prj2Level.Settings.MakeRelative(targetProject.GetScriptRootDirectory(), VariableType.LevelDirectory);
		prj2Level.Settings.GameLevelFilePath = prj2Level.Settings.MakeRelative(dataFilePath, VariableType.LevelDirectory);
		prj2Level.Settings.GameVersion = targetProject.GameVersion is TRVersion.Game.TR1 ? TRVersion.Game.TR1X : targetProject.GameVersion;

		prj2Level.Settings.WadSoundPaths.Clear();
		prj2Level.Settings.WadSoundPaths.Add(new WadSoundPath(LevelSettings.VariableCreate(VariableType.LevelDirectory) + LevelSettings.Dir + ".." + LevelSettings.Dir + ".." + LevelSettings.Dir + "Sounds"));

		if (targetProject.GameVersion.Native() <= TRVersion.Game.TR3)
		{
			prj2Level.Settings.AgressiveTexturePacking = true;
			prj2Level.Settings.TexturePadding = 1;
		}

		if (targetProject.GameVersion == TRVersion.Game.TombEngine)
			prj2Level.Settings.TenLuaScriptFile = Path.Combine(LevelSettings.VariableCreate(VariableType.ScriptDirectory), "Levels", LevelSettings.VariableCreate(VariableType.LevelName) + ".lua");

		prj2Level.Settings.LoadDefaultSoundCatalog();

		string? defaultWadPath = targetProject.GameVersion switch
		{
			TRVersion.Game.TombEngine => Path.Combine(targetProject.DirectoryPath, "Assets", "Wads", "TombEngine.wad2"),
			_ => null
		};

		if (defaultWadPath is not null && File.Exists(defaultWadPath))
			prj2Level.Settings.LoadWad(defaultWadPath);

		var texturePath = Path.Combine(targetProject.DirectoryPath, "Assets", "Textures", "default.png");

		if (File.Exists(texturePath))
		{
			prj2Level.Settings.Textures.Add(new LevelTexture(prj2Level.Settings, texturePath));

			var texture = new TextureArea
			{
				Texture = prj2Level.Settings.Textures[0],
				TexCoord0 = new VectorInt2(0, 0)
			};

			texture.TexCoord1 = new VectorInt2(texture.Texture.Image.Width, 0);
			texture.TexCoord2 = new VectorInt2(texture.Texture.Image.Width, texture.Texture.Image.Height);
			texture.TexCoord3 = new VectorInt2(0, texture.Texture.Image.Height);
			prj2Level.Settings.DefaultTexture = texture;
		}

		Prj2Writer.SaveToPrj2(prj2FilePath, prj2Level);
	}
}
