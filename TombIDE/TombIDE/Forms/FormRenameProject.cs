using DarkUI.Forms;
using System;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared.NewStructure;
using TombIDE.Shared.SharedClasses;

namespace TombIDE
{
	public partial class FormRenameProject : DarkForm
	{
		private IGameProject _targetProject;

		#region Initialization

		public FormRenameProject(IGameProject targetProject)
		{
			_targetProject = targetProject;

			InitializeComponent();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			textBox_NewName.Text = _targetProject.Name;
			textBox_NewName.SelectAll();
		}

		#endregion Initialization

		#region Events

		private void button_Apply_Click(object sender, EventArgs e)
		{
			try
			{
				string newName = PathHelper.RemoveIllegalPathSymbols(textBox_NewName.Text.Trim());

				if (string.IsNullOrWhiteSpace(newName) || newName.Equals("engine", StringComparison.OrdinalIgnoreCase))
					throw new ArgumentException("Invalid name.");

				bool renameDirectory = checkBox_RenameDirectory.Checked;
				bool renameTrprojFile = checkBox_RenameTrproj.Checked;

				if (newName == _targetProject.Name)
				{
					// If the name hasn't changed, but the directory name is different and the user wants to rename it
					if (Path.GetFileName(_targetProject.DirectoryPath) != newName && renameDirectory)
					{
						if (!Path.GetFileName(_targetProject.DirectoryPath).Equals(newName, StringComparison.OrdinalIgnoreCase))
						{
							string newDirectory = Path.Combine(Path.GetDirectoryName(_targetProject.DirectoryPath), newName);

							if (Directory.Exists(newDirectory))
								throw new ArgumentException("A directory with the same name already exists in the parent directory.");
						}

						_targetProject.Rename(newName, true, renameTrprojFile);
						_targetProject.Save();
					}
					else if (renameTrprojFile)
					{
						_targetProject.Rename(newName, false, true);
						_targetProject.Save();
					}
					else
						DialogResult = DialogResult.Cancel;
				}
				else
				{
					string newDirectory = Path.Combine(Path.GetDirectoryName(_targetProject.DirectoryPath), newName);

					if (renameDirectory && Directory.Exists(newDirectory) && !newDirectory.Equals(_targetProject.DirectoryPath, StringComparison.OrdinalIgnoreCase))
						throw new ArgumentException("A directory with the same name already exists in the parent directory.");

					_targetProject.Rename(newName, renameDirectory, renameTrprojFile);
					_targetProject.Save();
				}
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				DialogResult = DialogResult.None;
			}
		}

		#endregion Events
	}
}
