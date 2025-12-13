using System.Windows.Forms;
using TombIDE.Shared;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster
{
	public partial class SectionProjectSettings : UserControl
	{
		public SectionProjectSettings()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			settings_SpecialFunctions.Initialize(ide);
			settings_ProjectInfo.Initialize(ide);
			settings_Icon.Initialize(ide);
			settings_SplashScreen.Initialize(ide);

			if (ide.Project.GameVersion is TRVersion.Game.TR4 or TRVersion.Game.TRNG)
			{
				settings_StartupImage.Initialize(ide);
				settings_Logo.Initialize(ide);
			}
			else
			{
				settings_StartupImage.Visible = false;
				settings_Logo.Visible = false;
			}
		}
	}
}
