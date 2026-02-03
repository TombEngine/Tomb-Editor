using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Forms;
using TombIDE.ProjectMaster.Services;
using TombIDE.ProjectMaster.Services.Plugins;
using TombIDE.ProjectMaster.Services.Plugins.Deployment;
using TombIDE.ProjectMaster.Services.Plugins.Discovery;
using TombIDE.ProjectMaster.Services.Plugins.Initialization;
using TombIDE.ProjectMaster.Services.Plugins.Installation;
using TombIDE.ProjectMaster.Services.Plugins.Models;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ProjectMaster;

public partial class PluginManager : UserControl
{
	private IDE _ide = null!;

	private readonly IPluginServiceFactory _pluginServiceFactory;
	private readonly IUIResourceService _uiResourceService;

	private IPluginDiscoveryService? _discoveryService;
	private IPluginInstallationService? _installationService;
	private IPluginDeploymentService? _deploymentService;
	private IPluginInitializationService? _initializationService;

	#region Initialization

	public PluginManager() : this(new PluginServiceFactory(), new UIResourceService())
	{ }

	public PluginManager(IPluginServiceFactory pluginServiceFactory, IUIResourceService uiResourceService)
	{
		InitializeComponent();

		_pluginServiceFactory = pluginServiceFactory;
		_uiResourceService = uiResourceService;
	}

	public void Initialize(IDE ide)
	{
		if (!ide.Project.SupportsPlugins)
			return;

		_ide = ide;

		// Initialize services for the current game version
		_discoveryService = _pluginServiceFactory.GetDiscoveryService(_ide.Project.GameVersion);
		_installationService = _pluginServiceFactory.GetInstallationService(_ide.Project.GameVersion);
		_deploymentService = _pluginServiceFactory.GetDeploymentService(_ide.Project.GameVersion);
		_initializationService = _pluginServiceFactory.GetInitializationService(_ide.Project.GameVersion);

		// Verify if plugins are supported for this game version
		if (_discoveryService is null)
			return;

		panel_GameLabel.BackgroundImage = _uiResourceService.GetLevelPanelIcon(_ide.Project.GameVersion);

		// Initialize plugin directory if needed
		_initializationService?.InitializePluginDirectory(_ide.Project); // Refresh the plugin list
		UpdatePlugins();
	}

	#endregion Initialization

	#region Events

	private void button_OpenArchive_Click(object sender, EventArgs e)
	{
		using var dialog = new OpenFileDialog
		{
			Title = "Select the archive of the plugin you want to install",
			Filter = "ZIP Archives|*.zip"
		};

		if (dialog.ShowDialog(this) == DialogResult.OK)
			InstallPlugin(new PluginInstallationSource(dialog.FileName, PluginInstallationSourceType.Archive));
	}

	private void button_OpenFolder_Click(object sender, EventArgs e)
	{
		using var dialog = new FolderBrowserDialog
		{
			Description = "Select the folder of the plugin you want to install"
		};

		if (dialog.ShowDialog(this) == DialogResult.OK)
			InstallPlugin(new PluginInstallationSource(dialog.SelectedPath, PluginInstallationSourceType.Folder));
	}

	private void button_Remove_Click(object sender, EventArgs e)
	{
		if (!TryGetValidSelectedPlugin(out var selectedPluginInfo))
		{
			UpdatePlugins();
			return;
		}

		DialogResult result = DarkMessageBox.Show(this,
			$"Are you sure you want to remove the following plugin from your project:\n" +
			$"\"{selectedPluginInfo.Name}\" ?\n" +
			"This action will move the plugin directory into the recycle bin.",
			"Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

		if (result == DialogResult.Yes)
		{
			try
			{
				_installationService?.RemovePlugin(_ide.Project, selectedPluginInfo);
				UpdatePlugins();
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}

	private void button_OpenDirectory_Click(object sender, EventArgs e)
	{
		if (!TryGetValidSelectedPlugin(out var selectedPluginInfo))
		{
			UpdatePlugins();
			return;
		}

		SharedMethods.OpenInExplorer(selectedPluginInfo.DirectoryPath);
	}

	private void button_Download_Click(object sender, EventArgs e)
	{
		Process.Start(new ProcessStartInfo()
		{
			FileName = "https://www.tombraiderforums.com/showpost.php?p=7636390",
			UseShellExecute = true
		});
	}

	private void button_Refresh_Click(object sender, EventArgs e)
		=> UpdatePlugins();

	private void treeView_SelectedNodesChanged(object sender, EventArgs e)
	{
		button_OpenDirectory.Enabled = treeView.SelectedNodes.Count > 0;
		button_Remove.Enabled = treeView.SelectedNodes.Count > 0;
		menuItem_OpenDirectory.Enabled = treeView.SelectedNodes.Count > 0;
		menuItem_Remove.Enabled = treeView.SelectedNodes.Count > 0;

		if (treeView.SelectedNodes.Count > 0)
			UpdatePluginInfoOverview();
	}

	#endregion Events

	#region Methods

	private bool TryGetValidSelectedPlugin([NotNullWhen(true)] out PluginInfo? pluginInfo)
	{
		pluginInfo = null;

		if (treeView.SelectedNodes.Count == 0)
			return false;

		var selectedPluginInfo = (PluginInfo)treeView.SelectedNodes[0].Tag;

		if (!File.Exists(selectedPluginInfo.DllFilePath))
			return false;

		pluginInfo = selectedPluginInfo;
		return true;
	}

	private void UpdatePlugins()
	{
		treeView.SelectedNodes.Clear();
		treeView.Nodes.Clear();
		treeView.Invalidate();

		if (_discoveryService is null)
			return;

		foreach (var plugin in _discoveryService.DiscoverPlugins(_ide.Project))
		{
			var node = new DarkTreeNodeEx(plugin.Name, "~/" + plugin.DllFileName) { Tag = plugin };
			treeView.Nodes.Add(node);
		}

		_deploymentService?.DeployPlugins(_ide.Project);
		_deploymentService?.SynchronizeScriptReferences(_ide.Project);

		_ide.RaiseEvent(new IDE.ScriptEditor_ReloadSyntaxHighlightingEvent());

		UpdatePluginInfoOverview();
		treeView.Invalidate();
	}

	private void InstallPlugin(PluginInstallationSource source)
	{
		try
		{
			var installedPlugin = _installationService?.InstallPlugin(_ide.Project, source);

			if (installedPlugin is not null)
			{
				string pluginString = Path.GetFileNameWithoutExtension(installedPlugin.DllFileName);

				_ide.ScriptEditor_AddNewPluginEntry(pluginString);
				_ide.ScriptEditor_AddNewNGString(pluginString);
			}

			UpdatePlugins();
		}
		catch (Exception ex)
		{
			DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private void UpdatePluginInfoOverview()
	{
		if (treeView.SelectedNodes.Count == 0)
		{
			panel_Logo.BackgroundImage = null;
			label_NoInfo.Visible = true;

			textBox_Title.Text = string.Empty;
			textBox_DLLName.Text = string.Empty;

			richTextBox_Description.Text = string.Empty;

			return;
		}

		label_NoInfo.Visible = false;

		if (!TryGetValidSelectedPlugin(out var selectedPluginInfo))
		{
			UpdatePlugins();
			return;
		}

		textBox_Title.Text = selectedPluginInfo.Name;
		textBox_DLLName.Text = selectedPluginInfo.DllFileName;

		try
		{
			if (selectedPluginInfo.Logo is not null)
			{
				panel_Logo.BackgroundImage = selectedPluginInfo.Logo;
				label_NoLogo.Visible = false;
			}
			else
			{
				panel_Logo.BackgroundImage = null;
				label_NoLogo.Visible = true;
			}

			richTextBox_Description.Text = selectedPluginInfo.Description ?? string.Empty;
		}
		catch (Exception ex)
		{
			DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	#endregion Methods
}
