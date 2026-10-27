using DarkUI.Forms;
using System;
using System.Windows.Forms;
using TombIDE.ProjectMaster.Services.Level.Rename;
using TombIDE.Shared;

namespace TombIDE.ProjectMaster
{
	public partial class FormRenameLevel : DarkForm
	{
		private readonly IDE _ide;
		private readonly ILevelRenameService _levelRenameService;
		private readonly LevelRenameState _initialState;

		#region Initialization

		public FormRenameLevel(IDE ide, ILevelRenameService levelRenameService)
		{
			_ide = ide;
			_levelRenameService = levelRenameService;

			InitializeComponent();

			_initialState = _levelRenameService.GetInitialRenameState(
				_ide.SelectedLevel,
				_ide.Project,
				_ide.ScriptEditor_IsScriptDefined,
				_ide.ScriptEditor_IsStringDefined);

			ApplyInitialState();
		}

		private void ApplyInitialState()
		{
			// Configure directory rename checkbox
			checkBox_RenameDirectory.Text = _initialState.DirectoryRenameText;
			checkBox_RenameDirectory.Enabled = _initialState.CanRenameDirectory;
			checkBox_RenameDirectory.Checked = _initialState.CanRenameDirectory && _initialState.ShouldRenameDirectory;

			// Configure script rename checkbox
			checkBox_RenameScriptEntry.Text = _initialState.ScriptRenameText;
			checkBox_RenameScriptEntry.Enabled = _initialState.CanRenameScript;
			checkBox_RenameScriptEntry.Checked = _initialState.CanRenameScript && _initialState.ShouldRenameScript;

			// Configure error labels
			label_ScriptError.Visible = _initialState.ShowScriptError;
			label_LanguageError.Visible = _initialState.ShowLanguageError;

			// Adjust form height based on error state
			AdjustFormHeight();
		}

		private void AdjustFormHeight()
		{
			bool showNoErrors = !_initialState.ShowScriptError && !_initialState.ShowLanguageError;
			bool showOneError = _initialState.ShowScriptError != _initialState.ShowLanguageError;

			if (showNoErrors)
				Height = 193;
			else if (showOneError)
				Height = 212;

			// Default height for both errors
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			textBox_NewName.Text = _ide.SelectedLevel.Name;
			textBox_NewName.SelectAll();
		}

		#endregion Initialization

		#region Events

		private void button_Apply_Click(object sender, EventArgs e)
		{
			try
			{
				var options = new LevelRenameOptions
				{
					NewName = textBox_NewName.Text,
					RenameDirectory = checkBox_RenameDirectory.Checked,
					RenameScriptEntry = checkBox_RenameScriptEntry.Checked
				};

				LevelRenameResult result = _levelRenameService.RenameLevel(
					_ide.SelectedLevel,
					_ide.Project,
					options);

				if (!result.ChangesMade)
				{
					DialogResult = DialogResult.Cancel;
					return;
				}

				if (result.ScriptRenameNeeded && result.OldName is not null && result.NewName is not null)
					_ide.ScriptEditor_RenameLevel(result.OldName, result.NewName);

				_ide.RaiseEvent(new IDE.SelectedLevelSettingsChangedEvent());
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				DialogResult = DialogResult.None;
			}
		}

		private void textBox_NewName_TextChanged(object sender, EventArgs e)
		{
			LevelRenameState state = _levelRenameService.GetRenameStateForText(
				_ide.SelectedLevel,
				_ide.Project,
				textBox_NewName.Text,
				_initialState.ShowScriptError,
				_initialState.ShowLanguageError);

			// Update directory checkbox
			if (_initialState.CanRenameDirectory) // Only update if initially allowed (not external)
			{
				checkBox_RenameDirectory.Enabled = state.CanRenameDirectory;
				checkBox_RenameDirectory.Checked = state.ShouldRenameDirectory;
			}

			// Update script checkbox
			checkBox_RenameScriptEntry.Enabled = state.CanRenameScript;
			checkBox_RenameScriptEntry.Checked = state.ShouldRenameScript;

			// Update apply button
			button_Apply.Enabled = _levelRenameService.CanApply(textBox_NewName.Text);
		}

		#endregion Events
	}
}
