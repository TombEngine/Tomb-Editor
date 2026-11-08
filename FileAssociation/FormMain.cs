using DarkUI.Forms;
using MiniFileAssociation;
using System;

namespace FileAssociation
{
	public partial class FormMain : DarkForm
	{
		private bool _wasPRJ2Checked = false;
		private bool _wasWAD2Checked = false;
		private bool _wasTRPROJChecked = false;
		private bool _wasPRJChecked = false;
		private bool _wasWADChecked = false;
		public FormMain()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			_wasPRJ2Checked = checkBox_prj2.Checked = Association.IsAssociatedWith(".prj2", DefaultPaths.TombEditorExecutable);
			_wasWAD2Checked = checkBox_wad2.Checked = Association.IsAssociatedWith(".wad2", DefaultPaths.WadToolExecutable);
			_wasTRPROJChecked = checkBox_trproj.Checked = Association.IsAssociatedWith(".trproj", DefaultPaths.TombIDEExecutable);
			_wasPRJChecked = checkBox_prj.Checked = Association.IsAssociatedWith(".prj", DefaultPaths.TombEditorExecutable);
			_wasWADChecked = checkBox_wad.Checked = Association.IsAssociatedWith(".wad", DefaultPaths.WadToolExecutable);

			button_Apply.Enabled = false;

			base.OnLoad(e);
		}

		private void button_Apply_Click(object sender, EventArgs e)
		{
			if (checkBox_prj2.Checked)
				Program.AssociatePRJ2();
			else
				Association.ClearAssociations(".prj2");

			if (checkBox_wad2.Checked)
				Program.AssociateWAD2();
			else
				Association.ClearAssociations(".wad2");

			if (checkBox_trproj.Checked)
				Program.AssociateTRPROJ();
			else
				Association.ClearAssociations(".trproj");

			if (checkBox_prj.Checked)
				Program.AssociatePRJ();
			else
				Association.ClearAssociations(".prj");
			
			if (checkBox_wad.Checked)
				Program.AssociateWAD();
			else
				Association.ClearAssociations(".wad");
			
			_wasPRJ2Checked = checkBox_prj2.Checked;
			_wasWAD2Checked = checkBox_wad2.Checked;
			_wasTRPROJChecked = checkBox_trproj.Checked;
			_wasPRJChecked = checkBox_prj.Checked;
			_wasWADChecked = checkBox_wad.Checked;

			button_Apply.Enabled = false;
		}

		private void button_Close_Click(object sender, EventArgs e)
			=> Close();

		private void checkBox_CheckedChanged(object sender, EventArgs e)
			=> button_Apply.Enabled = checkBox_prj2.Checked != _wasPRJ2Checked
				|| checkBox_wad2.Checked != _wasWAD2Checked
				|| checkBox_trproj.Checked != _wasTRPROJChecked
				|| checkBox_prj.Checked != _wasPRJChecked
				|| checkBox_wad.Checked != _wasWADChecked;
	}
}
