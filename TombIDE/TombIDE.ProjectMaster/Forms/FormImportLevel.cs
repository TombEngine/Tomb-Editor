using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombIDE.ProjectMaster.Services.Level.Import;
using TombIDE.Shared;
using TombIDE.Shared.NewStructure;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ProjectMaster
{
	public partial class FormImportLevel : DarkForm, IProgressReportingForm
	{
		public ILevelProject? ImportedLevel { get; private set; }
		public ScriptGenerationResult? GeneratedScript { get; private set; }

		private readonly IGameProject _targetProject;
		private readonly ILevelImportService _importService;
		private readonly string _sourcePrj2FilePath;

		#region Initialization

		public FormImportLevel(IGameProject targetProject, string prj2FilePath, ILevelImportService importService)
		{
			_targetProject = targetProject;
			_importService = importService;
			_sourcePrj2FilePath = prj2FilePath;

			InitializeComponent();

			ConfigureUI();
		}

		private void ConfigureUI()
		{
			// Setup path display
			textBox_Prj2Path.BackColor = Color.FromArgb(48, 48, 48);
			textBox_Prj2Path.Text = _sourcePrj2FilePath;

			textBox_LevelName.Text = Path.GetFileNameWithoutExtension(_sourcePrj2FilePath);

			// Configure script generation visibility
			if (!GameVersionHelper.IsScriptGenerationSupported(_targetProject))
			{
				checkBox_GenerateSection.Checked = checkBox_GenerateSection.Visible = false;
				panel_ScriptSettings.Visible = false;
				panel_04.Visible = false;
			}
			else if (!GameVersionHelper.IsHorizonSettingAvailable(_targetProject))
			{
				checkBox_EnableHorizon.Visible = false;
				panel_ScriptSettings.Height -= 30;
			}

			// Set default ambient sound
			int defaultSoundId = GameVersionHelper.GetDefaultAmbientSoundId(_targetProject);

			if (defaultSoundId > 0)
				numeric_SoundID.Value = defaultSoundId;
		}

		#endregion Initialization

		#region ILevelImportProgress

		void IProgressReportingForm.SetTotalProgress(int total)
		{
			progressBar.Visible = true;
			progressBar.BringToFront();
			progressBar.Maximum = total;
			progressBar.Value = 0;
		}

		void IProgressReportingForm.IncrementProgress(int value)
			=> progressBar.Increment(value);

		#endregion ILevelImportProgress

		#region Level importing methods

		private void button_Import_Click(object sender, EventArgs e)
		{
			button_Import.Enabled = false;

			try
			{
				var importMode = GetSelectedImportMode();
				bool shouldUpdateSettings = false;

				// For KeepInPlace mode, ask the user if they want to update settings
				if (importMode == LevelImportMode.KeepInPlace)
				{
					DialogResult dialogResult = DarkMessageBox.Show(this,
						"Do you want to update the \"Game\" settings of all the .prj2 files in the\n" +
						"specified folder to match the project settings?",
						"Update settings?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

					shouldUpdateSettings = dialogResult == DialogResult.Yes;
				}

				var options = new LevelImportOptions
				{
					LevelName = textBox_LevelName.Text,
					SourcePrj2FilePath = _sourcePrj2FilePath,
					DataFileName = textBox_CustomFileName.Text,
					ImportMode = importMode,
					SelectedFilePaths = GetSelectedFilePaths(),
					GenerateScript = checkBox_GenerateSection.Checked,
					AmbientSoundId = (int)numeric_SoundID.Value,
					EnableHorizon = checkBox_EnableHorizon.Checked,
					UpdatePrj2SettingsForExternalLevel = shouldUpdateSettings
				};

				LevelImportResult result = _importService.ImportLevel(_targetProject, options, this);

				ImportedLevel = result.ImportedLevel;
				GeneratedScript = result.GeneratedScript;
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

				button_Import.Enabled = true;
				DialogResult = DialogResult.None;
			}
		}

		private LevelImportMode GetSelectedImportMode()
		{
			if (radioButton_SpecifiedCopy.Checked)
				return LevelImportMode.CopySpecifiedFile;
			else if (radioButton_SelectedCopy.Checked)
				return LevelImportMode.CopySelectedFiles;
			else
				return LevelImportMode.KeepInPlace;
		}

		private IReadOnlyList<string> GetSelectedFilePaths()
		{
			if (!radioButton_SelectedCopy.Checked)
				return [];

			return treeView.SelectedNodes
				.Cast<DarkTreeNode>()
				.Select(node => node.Tag?.ToString() ?? string.Empty)
				.Where(filePath => !string.IsNullOrEmpty(filePath))
				.ToList();
		}

		#endregion Level importing methods

		#region UI Event Handlers

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

		private void radioButton_SpecifiedCopy_CheckedChanged(object sender, EventArgs e)
		{
			if (!radioButton_SpecifiedCopy.Checked)
				return;

			textBox_Prj2Path.Text = _sourcePrj2FilePath;
			textBox_Prj2Path.BackColor = Color.FromArgb(48, 48, 48);

			ClearAndDisableTreeView();
		}

		private void radioButton_SelectedCopy_CheckedChanged(object sender, EventArgs e)
		{
			if (!radioButton_SelectedCopy.Checked)
				return;

			string directoryPath = Path.GetDirectoryName(_sourcePrj2FilePath) ?? string.Empty;

			textBox_Prj2Path.Text = directoryPath;
			textBox_Prj2Path.BackColor = Color.FromArgb(64, 80, 96);

			EnableAndFillTreeView(directoryPath);
		}

		private void radioButton_SpecificKeep_CheckedChanged(object sender, EventArgs e)
		{
			if (!radioButton_FolderKeep.Checked)
				return;

			string directoryPath = Path.GetDirectoryName(_sourcePrj2FilePath) ?? string.Empty;

			textBox_Prj2Path.Text = directoryPath;
			textBox_Prj2Path.BackColor = Color.FromArgb(64, 80, 96);

			ClearAndDisableTreeView();
		}

		private void button_SelectAll_Click(object sender, EventArgs e)
		{
			treeView.SelectNodes(treeView.Nodes);
			treeView.Focus();
		}

		private void button_DeselectAll_Click(object sender, EventArgs e)
		{
			treeView.SelectedNodes.Clear();
			treeView.Invalidate();
		}

		private void ClearAndDisableTreeView()
		{
			treeView.SelectedNodes.Clear();
			treeView.Nodes.Clear();
			treeView.Invalidate();

			treeView.Enabled = false;
			button_SelectAll.Enabled = false;
			button_DeselectAll.Enabled = false;
		}

		private void EnableAndFillTreeView(string directoryPath)
		{
			treeView.Enabled = true;
			button_SelectAll.Enabled = true;
			button_DeselectAll.Enabled = true;

			treeView.Nodes.Clear();

			foreach (string file in Prj2Helper.GetValidFiles(directoryPath))
			{
				var node = new DarkTreeNode
				{
					Text = Path.GetFileName(file),
					Tag = file,
				};

				treeView.Nodes.Add(node);
			}
		}

		#endregion UI Event Handlers

		#region Script section generating

		private void checkBox_GenerateSection_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBox_GenerateSection.Checked)
			{
				panel_ScriptSettings.Visible = true;
				panel_04.Height = 108;
				Height = 626;
			}
			else
			{
				panel_ScriptSettings.Visible = false;
				panel_04.Height = 35;
				Height = 553;
			}
		}

		private void button_OpenAudioFolder_Click(object sender, EventArgs e)
			=> SharedMethods.OpenInExplorer(Path.Combine(_targetProject.GetEngineRootDirectoryPath(), "audio"));

		#endregion Script section generating
	}
}
