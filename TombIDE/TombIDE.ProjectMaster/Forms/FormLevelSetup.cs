using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombIDE.Shared.NewStructure;
using TombIDE.Shared.NewStructure.Implementations;
using TombIDE.Shared.SharedClasses;
using TombLib;
using TombLib.LevelData;
using TombLib.LevelData.IO;
using TombLib.Utils;

namespace TombIDE.ProjectMaster
{
	public partial class FormLevelSetup : DarkForm
	{
		public ILevelProject? CreatedLevel { get; private set; }
		public List<string> GeneratedScriptLines { get; private set; } = [];

		private IGameProject _targetProject;

		#region Initialization

		public FormLevelSetup(IGameProject targetProject)
		{
			_targetProject = targetProject;

			InitializeComponent();

			if (targetProject.GameVersion is TRVersion.Game.TR1 or TRVersion.Game.TR2X)
			{
				checkBox_GenerateSection.Checked = checkBox_GenerateSection.Visible = false;
				panel_ScriptSettings.Visible = false;
			}
			else if (targetProject.GameVersion is not TRVersion.Game.TR4 and not TRVersion.Game.TRNG and not TRVersion.Game.TombEngine)
			{
				checkBox_EnableHorizon.Visible = false;
				panel_ScriptSettings.Height -= 35;
			}

			if (targetProject.GameVersion is TRVersion.Game.TombEngine)
			{
				checkBox_GenerateSection.Text = "Generate Lua script";
			}

			if (_targetProject.GameVersion == TRVersion.Game.TR2)
				numeric_SoundID.Value = 33;
			else if (_targetProject.GameVersion == TRVersion.Game.TR3)
				numeric_SoundID.Value = 28;
		}

		#endregion Initialization

		#region Events

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			textBox_LevelName.Text = "New Level";
			textBox_LevelName.Focus();
			textBox_LevelName.SelectAll();
		}

		private void textBox_LevelName_TextChanged(object sender, EventArgs e)
		{
			if (!checkBox_CustomFileName.Checked)
				textBox_CustomFileName.Text = textBox_LevelName.Text.Trim().Replace(' ', '_');
		}

		private void checkBox_CustomFileName_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBox_CustomFileName.Checked)
				textBox_CustomFileName.Enabled = true;
			else
			{
				textBox_CustomFileName.Enabled = false;
				textBox_CustomFileName.Text = textBox_LevelName.Text.Trim().Replace(' ', '_');
			}
		}

		private void textBox_CustomFileName_TextChanged(object sender, EventArgs e)
		{
			int cachedCaretPosition = textBox_CustomFileName.SelectionStart;

			textBox_CustomFileName.Text = LevelHandling.MakeValidVariableName(textBox_CustomFileName.Text);
			textBox_CustomFileName.SelectionStart = cachedCaretPosition;
		}

		private void button_Create_Click(object sender, EventArgs e)
		{
			button_Create.Enabled = false;

			try
			{
				string levelName = PathHelper.RemoveIllegalPathSymbols(textBox_LevelName.Text.Trim());
				levelName = LevelHandling.RemoveIllegalNameSymbols(levelName);

				if (!levelName.IsANSI())
					throw new ArgumentException("The level name contains illegal characters. Please use only English characters and numbers.");

				if (string.IsNullOrWhiteSpace(levelName))
					throw new ArgumentException("You must enter a valid name for your level.");

				string dataFileName = LevelHandling.MakeValidVariableName(textBox_CustomFileName.Text.Trim());

				if (!dataFileName.IsANSI())
					throw new ArgumentException("The data file name contains illegal characters. Please use only English characters and numbers.");

				if (string.IsNullOrWhiteSpace(dataFileName))
					throw new ArgumentException("You must specify the custom PRJ2 / DATA file name.");

				string levelFolderPath = Path.Combine(_targetProject.LevelsDirectoryPath, levelName);

				// Create the level folder
				if (!Directory.Exists(levelFolderPath))
					Directory.CreateDirectory(levelFolderPath);

				if (Directory.EnumerateFileSystemEntries(levelFolderPath).ToArray().Length > 0) // 99% this will never accidentally happen
					throw new ArgumentException("A folder with the same name as the \"Level name\" already exists in\n" +
						"the project's /Levels/ folder and it's not empty.");

				ILevelProject createdLevel = new LevelProject(levelName, levelFolderPath);

				// Create a simple .prj2 file with pre-set project settings (game paths etc.)
				var level = Level.CreateSimpleLevel();

				string prj2FilePath = Path.Combine(createdLevel.DirectoryPath, dataFileName) + ".prj2";
				string exeFilePath = _targetProject.GetEngineExecutableFilePath();
				string engineDirectory = _targetProject.GetEngineRootDirectoryPath();

				string dataFilePath = Path.Combine(engineDirectory, "data", dataFileName + _targetProject.DataFileExtension);

				level.Settings.LevelFilePath = prj2FilePath;

				level.Settings.GameDirectory = level.Settings.MakeRelative(engineDirectory, VariableType.LevelDirectory);
				level.Settings.GameExecutableFilePath = level.Settings.MakeRelative(exeFilePath, VariableType.LevelDirectory);
				level.Settings.ScriptDirectory = level.Settings.MakeRelative(_targetProject.GetScriptRootDirectory(), VariableType.LevelDirectory);
				level.Settings.GameLevelFilePath = level.Settings.MakeRelative(dataFilePath, VariableType.LevelDirectory);
				level.Settings.GameVersion = _targetProject.GameVersion is TRVersion.Game.TR1 ? TRVersion.Game.TR1X : _targetProject.GameVersion; // Map TR1 to TR1X - we never supported vanilla TR1 in TombIDE

				level.Settings.WadSoundPaths.Clear();
				level.Settings.WadSoundPaths.Add(new WadSoundPath(LevelSettings.VariableCreate(VariableType.LevelDirectory) + LevelSettings.Dir + ".." + LevelSettings.Dir + ".." + LevelSettings.Dir + "Sounds"));

				if (_targetProject.GameVersion.Native() <= TRVersion.Game.TR3)
				{
					level.Settings.AgressiveTexturePacking = true;
					level.Settings.TexturePadding = 1;
				}

				if (_targetProject.GameVersion == TRVersion.Game.TombEngine)
					level.Settings.TenLuaScriptFile = Path.Combine(LevelSettings.VariableCreate(VariableType.ScriptDirectory), "Levels", LevelSettings.VariableCreate(VariableType.LevelName) + ".lua");

				level.Settings.LoadDefaultSoundCatalog();

				string? defaultWadPath = _targetProject.GameVersion switch
				{
					TRVersion.Game.TombEngine => Path.Combine(_targetProject.DirectoryPath, "Assets", "Wads", "TombEngine.wad2"),
					_ => null
				};

				if (defaultWadPath is not null && File.Exists(defaultWadPath))
					level.Settings.LoadWad(defaultWadPath);

                var texturePath = Path.Combine(_targetProject.DirectoryPath, "Assets", "Textures", "default.png");

                if (File.Exists(texturePath))
                {
                    level.Settings.Textures.Add(new LevelTexture(level.Settings, Path.Combine(_targetProject.DirectoryPath, "Assets", "Textures", "default.png")));
                    var texture = new TextureArea() { Texture = level.Settings.Textures[0] };

                    texture.TexCoord0 = new VectorInt2(0, 0);
                    texture.TexCoord1 = new VectorInt2(texture.Texture.Image.Width, 0);
                    texture.TexCoord2 = new VectorInt2(texture.Texture.Image.Width, texture.Texture.Image.Height);
                    texture.TexCoord3 = new VectorInt2(0, texture.Texture.Image.Height);
                    level.Settings.DefaultTexture = texture;
                }

                Prj2Writer.SaveToPrj2(prj2FilePath, level);

				if (checkBox_GenerateSection.Checked)
				{
					int ambientSoundID = (int)numeric_SoundID.Value;
					bool horizon = checkBox_EnableHorizon.Checked;

					// // // //
					GeneratedScriptLines = LevelHandling.GenerateScriptLines(levelName, dataFileName, _targetProject.GameVersion, ambientSoundID, horizon);
					// // // //
				}

				createdLevel.Save();

				// // // //
				CreatedLevel = createdLevel;
				// // // //
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

				button_Create.Enabled = true;

				DialogResult = DialogResult.None;
			}
		}

		private void checkBox_GenerateSection_CheckedChanged(object sender, EventArgs e)
		{
			panel_ScriptSettings.Enabled = checkBox_GenerateSection.Checked;
		}

		private void button_OpenAudioFolder_Click(object sender, EventArgs e)
		{
			string engineDirectory = _targetProject.GetEngineRootDirectoryPath();

			if (_targetProject.GameVersion is TRVersion.Game.TR1 or TRVersion.Game.TR2X)
				SharedMethods.OpenInExplorer(Path.Combine(engineDirectory, "music"));
			else
				SharedMethods.OpenInExplorer(Path.Combine(engineDirectory, "audio"));
		}

		#endregion Events
	}
}
