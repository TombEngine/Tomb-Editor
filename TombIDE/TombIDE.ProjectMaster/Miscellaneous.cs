using System.Windows.Forms;
using TombIDE.ProjectMaster.Services;
using TombIDE.Shared;

namespace TombIDE.ProjectMaster;

public partial class Miscellaneous : UserControl
{
	private readonly IUIResourceService _uiResourceService;

	public Miscellaneous() : this(new UIResourceService())
	{ }

	public Miscellaneous(IUIResourceService uiResourceService)
	{
		InitializeComponent();

		_uiResourceService = uiResourceService;
	}

	public void Initialize(IDE ide)
	{
		panel_GameLabel.BackgroundImage = _uiResourceService.GetLevelPanelIcon(ide.Project.GameVersion);

		section_ProjectInfo.Initialize(ide);
	}
}
