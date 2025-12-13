using DarkUI.Docking;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using TombLib.Utils;

namespace TombIDE.Shared
{
	public class IDEConfiguration
	{
		/* TombIDE */

		public bool Start_OpenMaximized { get; set; } = false;
		public Size Start_WindowSize { get; set; } = new Size(512, 512);
		public bool IDE_OpenMaximized { get; set; } = false;
		public Size IDE_WindowSize { get; set; } = new Size(1070, 640);

		public string RememberedProject { get; set; }

		public List<string> PinnedProgramPaths { get; set; } = new List<string>();

		/* ProjectMaster */

		public bool ViewFileNames { get; set; } = true;
		public bool ViewFullFolderPaths { get; set; } = false;
		public bool LightModePreviewEnabled { get; set; } = false;
		public bool StandardAspectRatioPreviewEnabled { get; set; } = false;
		public bool PluginsPanelHidden { get; set; } = false;

		/// <summary>
		/// This can only be changed by directly modifying the .xml file
		/// </summary>
		public string ExternalLevelPrefix { get; set; } = "(Ext.) ";

		/* ScriptEditor */

		public bool UseNewIncludeMethod { get; set; } = true;
		public bool ShowCompilerLogsAfterBuild { get; set; } = true;
		public bool ReindentOnSave { get; set; } = false;

		public bool InfoBox_AlwaysOnTop { get; set; } = true;
		public bool InfoBox_CloseTabsOnClose { get; set; } = false;

		public DockPanelState CS_DockPanelState { get; set; } = DefaultLayouts.ClassicScriptLayout;
		public DockPanelState GFL_DockPanelState { get; set; } = DefaultLayouts.GameFlowScriptLayout;
		public DockPanelState T1M_DockPanelState { get; set; } = DefaultLayouts.Tomb1MainLayout;
		public DockPanelState Lua_DockPanelState { get; set; } = DefaultLayouts.LuaLayout;

		public string VSCodePath { get; set; } = string.Empty;
		public bool DoNotAskToInstallLuaExtension { get; set; }

		public static string DefaultPath => Path.Combine(DefaultPaths.ConfigsDirectory, "TombIDEConfiguration.xml");

		public void Save(Stream stream)
			=> XmlUtils.WriteXmlFile(stream, this);

		public void Save(string path)
		{
			if (!Directory.Exists(DefaultPaths.ConfigsDirectory))
				Directory.CreateDirectory(DefaultPaths.ConfigsDirectory);

			XmlUtils.WriteXmlFile(path, this);
		}

		public void Save()
			=> Save(DefaultPath);

		public static IDEConfiguration Load(Stream stream)
		{
			try
			{
				return XmlUtils.ReadXmlFile<IDEConfiguration>(stream);
			}
			catch (Exception)
			{
				return new IDEConfiguration();
			}
		}

		public static IDEConfiguration Load(string filePath)
		{
			try
			{
				return XmlUtils.ReadXmlFile<IDEConfiguration>(filePath);
			}
			catch (Exception)
			{
				return new IDEConfiguration();
			}
		}

		public static IDEConfiguration Load()
			=> Load(DefaultPath);
	}
}
