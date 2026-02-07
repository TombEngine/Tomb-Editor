using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using TombIDE.ProjectMaster.Services.Level.Setup;
using TombIDE.Shared.NewStructure;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster
{
	public partial class FormLevelSetup : DarkForm
	{
		public ILevelProject? CreatedLevel { get; private set; }
		public IReadOnlyList<string> GeneratedScriptLines { get; private set; } = [];

		private readonly IGameProject _targetProject;
		private readonly ILevelSetupService _levelSetupService;

		#region Initialization

		public FormLevelSetup(IGameProject targetProject, ILevelSetupService levelSetupService)
		{
			_targetProject = targetProject;
			_levelSetupService = levelSetupService;

			InitializeComponent();

			ConfigureUIForGameVersion();
		}

		private void ConfigureUIForGameVersion()
		{
			if (!GameVersionHelper.IsScriptGenerationSupported(_targetProject))
			{
				checkBox_GenerateSection.Checked = checkBox_GenerateSection.Visible = false;
				panel_ScriptSettings.Visible = false;
			}
			else if (!GameVersionHelper.IsHorizonSettingAvailable(_targetProject))
			{
				checkBox_EnableHorizon.Visible = false;
				panel_ScriptSettings.Height -= 35;
			}

			if (_targetProject.GameVersion is TRVersion.Game.TombEngine)
			{
				checkBox_GenerateSection.Text = "Generate Lua script";
			}

			int defaultSoundId = GameVersionHelper.GetDefaultAmbientSoundId(_targetProject);

			if (defaultSoundId > 0)
				numeric_SoundID.Value = defaultSoundId;
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
				textBox_CustomFileName.Text = LevelNameHelper.SuggestDataFileName(textBox_LevelName.Text);
		}

		private void checkBox_CustomFileName_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBox_CustomFileName.Checked)
			{
				textBox_CustomFileName.Enabled = true;
			}
			else
			{
				textBox_CustomFileName.Enabled = false;
				textBox_CustomFileName.Text = LevelNameHelper.SuggestDataFileName(textBox_LevelName.Text);
			}
		}

		private void textBox_CustomFileName_TextChanged(object sender, EventArgs e)
		{
			int cachedCaretPosition = textBox_CustomFileName.SelectionStart;

			textBox_CustomFileName.Text = LevelNameHelper.MakeValidVariableName(textBox_CustomFileName.Text);
			textBox_CustomFileName.SelectionStart = cachedCaretPosition;
		}

		private void button_Create_Click(object sender, EventArgs e)
		{
			button_Create.Enabled = false;

			try
			{
				var options = new LevelSetupOptions
				{
					LevelName = textBox_LevelName.Text,
					DataFileName = textBox_CustomFileName.Text,
					GenerateScript = checkBox_GenerateSection.Checked,
					AmbientSoundId = (int)numeric_SoundID.Value,
					EnableHorizon = checkBox_EnableHorizon.Checked
				};

				LevelSetupResult result = _levelSetupService.CreateLevel(_targetProject, options);

				CreatedLevel = result.CreatedLevel;
				GeneratedScriptLines = result.GeneratedScriptLines;
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
