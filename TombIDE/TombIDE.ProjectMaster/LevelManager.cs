using DarkUI.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;
using TombIDE.ProjectMaster.Forms;
using TombIDE.ProjectMaster.Services;
using TombIDE.ProjectMaster.Services.EngineUpdate;
using TombIDE.ProjectMaster.Services.EngineVersion;
using TombIDE.ProjectMaster.Services.FileExtraction;
using TombIDE.ProjectMaster.Services.LevelCompile;
using TombIDE.Shared;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster;

public partial class LevelManager : UserControl
{
	private IDE _ide = null!;

	private readonly IEngineVersionService _engineVersionService;
	private readonly IEngineUpdateServiceFactory _engineUpdateServiceFactory;
	private readonly ILevelCompileService _levelBuildService;
	private readonly IUIResourceService _uiResourceService;

	public LevelManager() : this(null, null, null, null)
	{ }

	public LevelManager(
		IEngineVersionService? engineVersionService,
		IEngineUpdateServiceFactory? engineUpdateServiceFactory,
		ILevelCompileService? levelBuildService,
		IUIResourceService? uiResourceService)
	{
		InitializeComponent();

		// Initialize services with default implementations if not provided
		var fileExtractionService = new FileExtractionService();

		_engineUpdateServiceFactory = engineUpdateServiceFactory ?? new EngineUpdateServiceFactory(fileExtractionService);
		_engineVersionService = engineVersionService ?? new EngineVersionService(_engineUpdateServiceFactory);
		_levelBuildService = levelBuildService ?? new LevelCompileService();
		_uiResourceService = uiResourceService ?? new UIResourceService();
	}

	public void Initialize(IDE ide)
	{
		_ide = ide;
		_ide.IDEEventRaised += IDE_IDEEventRaised;

		panel_GameLabel.BackgroundImage = _uiResourceService.GetLevelPanelIcon(_ide.Project.GameVersion);

		UpdateVersionLabel();

		section_LevelList.Initialize(ide, _levelBuildService);
		section_LevelProperties.Initialize(ide);
	}

	private void IDE_IDEEventRaised(IIDEEvent obj)
	{
		if (obj is IDE.BeginEngineUpdateEvent)
			BeginEngineUpdate();
	}

	private void UpdateVersionLabel()
	{
		button_Update.Visible = false;

		if (_ide.Project.GameVersion is TRVersion.Game.TR4)
		{
			label_OutdatedState.Visible = false;
			label_EngineVersion.Text = "Engine Version: TRLE";
			return;
		}

		var versionInfo = _engineVersionService.GetVersionInfo(_ide.Project);

		string engineVersionString = versionInfo.CurrentVersion == new Version(0, 0) ? "Unknown" : versionInfo.CurrentVersion?.ToString() ?? "Unknown";
		label_EngineVersion.Text = $"Engine Version: {engineVersionString}";

		if (versionInfo.CurrentVersion is null || versionInfo.LatestVersion is null)
		{
			label_OutdatedState.Visible = false;
		}
		else
		{
			label_OutdatedState.Visible = true;

			if (versionInfo.IsOutdated)
			{
				label_OutdatedState.Text = $"(Outdated, Latest version is: {versionInfo.LatestVersion})";
				label_OutdatedState.ForeColor = Color.LightPink;

				// Show update button if auto-update is supported
				var updateService = _engineUpdateServiceFactory.GetUpdateService(_ide.Project.GameVersion);

				if (updateService is not null)
				{
					button_Update.Visible = true;

					if (!versionInfo.SupportsAutoUpdate)
					{
						button_Update.Enabled = false;
						button_Update.Text = versionInfo.AutoUpdateBlockReason;
						button_Update.Width = 300;
					}
					else
					{
						button_Update.Enabled = true;
						button_Update.Text = "Update Engine";
						button_Update.Width = 200;
					}
				}
			}
			else
			{
				label_OutdatedState.Text = "(Latest)";
				label_OutdatedState.ForeColor = Color.LightGreen;
			}
		}
	}

	private void button_RebuildAll_Click(object sender, EventArgs e)
	{
		if (!_levelBuildService.RebuildAllLevels(_ide.Project))
		{
			DarkMessageBox.Show(this,
				"There are no levels in the current project.",
				"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
	}

	private void button_Update_Click(object sender, EventArgs e)
		=> BeginEngineUpdate();

	private void BeginEngineUpdate()
	{
		var updateService = _engineUpdateServiceFactory.GetUpdateService(_ide.Project.GameVersion);

		if (updateService is null)
		{
			DarkMessageBox.Show(this, "Auto-update is not supported for this game version.",
				"Not Supported", MessageBoxButtons.OK, MessageBoxIcon.Information);

			return;
		}

		var versionInfo = _engineVersionService.GetVersionInfo(_ide.Project);

		if (versionInfo.CurrentVersion is null || versionInfo.LatestVersion is null)
		{
			DarkMessageBox.Show(this, "Could not determine the current or latest engine version.",
				"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

			return;
		}

		bool success = updateService.UpdateEngine(
			_ide.Project,
			versionInfo.CurrentVersion,
			versionInfo.LatestVersion,
			this);

		if (success)
			UpdateVersionLabel();
	}

	private void button_Publish_Click(object sender, EventArgs e)
	{
		using var form = new FormGameArchive(_ide);
		form.ShowDialog();
	}
}
