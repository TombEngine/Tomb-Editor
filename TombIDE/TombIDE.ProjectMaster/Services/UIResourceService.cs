using System.Drawing;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster.Services;

public sealed class UIResourceService : IUIResourceService
{
	public Image? GetLevelPanelIcon(TRVersion.Game gameVersion) => gameVersion switch
	{
		TRVersion.Game.TombEngine => Properties.Resources.TEN_LVL,
		TRVersion.Game.TRNG => Properties.Resources.TRNG_LVL,
		TRVersion.Game.TR4 => Properties.Resources.TR4_LVL,
		TRVersion.Game.TR3 => Properties.Resources.TR3_LVL,
		TRVersion.Game.TR2 or TRVersion.Game.TR2X => Properties.Resources.TR2_LVL,
		TRVersion.Game.TR1 or TRVersion.Game.TR1X => Properties.Resources.TR1_LVL,
		_ => null
	};
}
