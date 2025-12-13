using System.Windows.Forms;
using TombIDE.Shared;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster
{
	public partial class Miscellaneous : UserControl
	{
		public Miscellaneous()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			switch (ide.Project.GameVersion)
			{
				case TRVersion.Game.TombEngine: panel_GameLabel.BackgroundImage = Properties.Resources.TEN_LVL; break;
				case TRVersion.Game.TRNG: panel_GameLabel.BackgroundImage = Properties.Resources.TRNG_LVL; break;
				case TRVersion.Game.TR4: panel_GameLabel.BackgroundImage = Properties.Resources.TR4_LVL; break;
				case TRVersion.Game.TR3: panel_GameLabel.BackgroundImage = Properties.Resources.TR3_LVL; break;
				case TRVersion.Game.TR2 or TRVersion.Game.TR2X: panel_GameLabel.BackgroundImage = Properties.Resources.TR2_LVL; break;
				case TRVersion.Game.TR1 or TRVersion.Game.TR1X: panel_GameLabel.BackgroundImage = Properties.Resources.TR1_LVL; break;
			}

			section_ProjectInfo.Initialize(ide);
		}
	}
}
