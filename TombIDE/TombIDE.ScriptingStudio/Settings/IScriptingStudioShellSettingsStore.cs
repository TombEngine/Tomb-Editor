#nullable enable

using NLog;
using Nickelony.IDEKit.KeyBindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Docking;
using TombLib.Utils;

namespace TombIDE.ScriptingStudio.Settings;

/// <summary>
/// Stores and loads workspace-level shell settings, including dock layout,
/// editor configuration, and shortcut overrides.
/// </summary>
public interface IScriptingStudioShellSettingsStore
{
	/// <summary>
	/// Creates a new default settings instance for the specified workspace profile.
	/// </summary>
	ScriptingStudioShellWorkspaceSettings CreateDefault(ScriptingWorkspaceProfile workspaceProfile);

	/// <summary>
	/// Loads the persisted settings for the specified workspace profile.
	/// </summary>
	ScriptingStudioShellWorkspaceSettings Load(ScriptingWorkspaceProfile workspaceProfile);

	/// <summary>
	/// Loads persisted settings for a workspace kind using the supplied default layout when no settings document exists.
	/// </summary>
	ScriptingStudioShellWorkspaceSettings Load(ScriptingWorkspaceKind workspaceKind, DockPanelState defaultLayout);

	/// <summary>
	/// Gets whether Lua is enabled for the specified primary workspace kind.
	/// </summary>
	bool IsLuaEnabled(ScriptingWorkspaceKind workspaceKind);

	/// <summary>
	/// Saves the specified settings for the given workspace kind.
	/// </summary>
	void Save(ScriptingWorkspaceKind workspaceKind, ScriptingStudioShellWorkspaceSettings settings);

	/// <summary>
	/// Atomically saves only the shortcut overrides for a workspace,
	/// preserving all other settings (layout, editor config, etc.).
	/// Returns <see langword="true"/> when the save succeeded.
	/// </summary>
	bool SaveShortcutOverrides(ScriptingWorkspaceKind workspaceKind, KeyBindingOverrideCollection overrides);
}

public sealed class ScriptingStudioShellWorkspaceSettings
{
	public string AvalonDockLayoutXml { get; set; } = string.Empty;

	public DockPanelState DockPanelState { get; set; } = new();

	public bool InfoBoxAlwaysOnTop { get; set; } = true;

	public bool InfoBoxCloseTabsOnClose { get; set; }

	/// <summary>
	/// Gets or sets whether the primary workspace also hosts Lua documents.
	/// The capability is evaluated when the shell is created and takes effect on the next shell session.
	/// </summary>
	public bool LuaEnabled { get; set; }

	public bool IsStatusStripVisible { get; set; } = true;

	public bool IsToolStripVisible { get; set; } = true;

	public bool ReindentOnSave { get; set; }

	public bool ShowCompilerLogsAfterBuild { get; set; } = true;

	public bool UseNewIncludeMethod { get; set; } = true;

	public KeyBindingOverrideCollection ShortcutOverrides { get; set; } = new();

	public ScriptingStudioShellWorkspaceSettings Clone() => new()
	{
		AvalonDockLayoutXml = AvalonDockLayoutXml ?? string.Empty,
		DockPanelState = DockPanelStateCloneHelper.Clone(DockPanelState),
		InfoBoxAlwaysOnTop = InfoBoxAlwaysOnTop,
		InfoBoxCloseTabsOnClose = InfoBoxCloseTabsOnClose,
		LuaEnabled = LuaEnabled,
		IsStatusStripVisible = IsStatusStripVisible,
		IsToolStripVisible = IsToolStripVisible,
		ReindentOnSave = ReindentOnSave,
		ShowCompilerLogsAfterBuild = ShowCompilerLogsAfterBuild,
		UseNewIncludeMethod = UseNewIncludeMethod,
		ShortcutOverrides = CloneShortcutOverrides(ShortcutOverrides)
	};

	private static KeyBindingOverrideCollection CloneShortcutOverrides(KeyBindingOverrideCollection source)
	{
		var clone = new KeyBindingOverrideCollection { Version = source.Version };

		foreach (KeyBindingOverrideEntry entry in source.Overrides)
		{
			clone.Overrides.Add(new KeyBindingOverrideEntry
			{
				CommandId = entry.CommandId,
				Bindings = entry.Bindings
					.Select(b => new KeyBindingSettings { KeyName = b.KeyName, Modifiers = b.Modifiers })
					.ToList()
			});
		}

		return clone;
	}
}

public sealed class ScriptingStudioShellSettingsDocument
{
	public ScriptingStudioShellWorkspaceSettings ClassicScript { get; set; } = new();

	public ScriptingStudioShellWorkspaceSettings GameFlowScript { get; set; } = new();

	public ScriptingStudioShellWorkspaceSettings Lua { get; set; } = new() { LuaEnabled = true };

	public ScriptingStudioShellWorkspaceSettings TRX { get; set; } = new();

	public ScriptingStudioShellWorkspaceSettings GetWorkspace(ScriptingWorkspaceKind workspaceKind) => workspaceKind switch
	{
		ScriptingWorkspaceKind.ClassicScript => ClassicScript,
		ScriptingWorkspaceKind.GameFlowScript => GameFlowScript,
		ScriptingWorkspaceKind.TRX => TRX,
		ScriptingWorkspaceKind.Lua => Lua,
		_ => throw new NotSupportedException($"Unsupported scripting workspace kind: {workspaceKind}.")
	};

	public void SetWorkspace(ScriptingWorkspaceKind workspaceKind, ScriptingStudioShellWorkspaceSettings settings)
	{
		ArgumentNullException.ThrowIfNull(settings);

		switch (workspaceKind)
		{
			case ScriptingWorkspaceKind.ClassicScript:
				ClassicScript = settings;
				break;

			case ScriptingWorkspaceKind.GameFlowScript:
				GameFlowScript = settings;
				break;

			case ScriptingWorkspaceKind.TRX:
				TRX = settings;
				break;

			case ScriptingWorkspaceKind.Lua:
				Lua = settings;
				break;

			default:
				throw new NotSupportedException($"Unsupported scripting workspace kind: {workspaceKind}.");
		}
	}
}

internal sealed class XmlScriptingStudioShellSettingsStore : IScriptingStudioShellSettingsStore
{
	private readonly string _settingsPath;

	public XmlScriptingStudioShellSettingsStore(string? settingsPath = null)
	{
		_settingsPath = settingsPath ?? DefaultSettingsPath;
	}

	public ScriptingStudioShellWorkspaceSettings CreateDefault(ScriptingWorkspaceProfile workspaceProfile)
	{
		ArgumentNullException.ThrowIfNull(workspaceProfile);

		return new ScriptingStudioShellWorkspaceSettings
		{
			DockPanelState = DockPanelStateCloneHelper.Clone(workspaceProfile.DefaultLayout)
		};
	}

	public ScriptingStudioShellWorkspaceSettings Load(ScriptingWorkspaceProfile workspaceProfile)
	{
		ArgumentNullException.ThrowIfNull(workspaceProfile);

		ScriptingStudioShellSettingsDocument document = LoadDocument();
		if (!File.Exists(_settingsPath))
		{
			ScriptingStudioShellWorkspaceSettings defaultSettings = CreateDefault(workspaceProfile);
			document.SetWorkspace(workspaceProfile.Kind, defaultSettings.Clone());
			SaveDocument(document);
			return defaultSettings;
		}

		ScriptingStudioShellWorkspaceSettings workspaceSettings = document.GetWorkspace(workspaceProfile.Kind).Clone();
		return workspaceSettings;
	}

	public ScriptingStudioShellWorkspaceSettings Load(ScriptingWorkspaceKind workspaceKind, DockPanelState defaultLayout)
	{
		ArgumentNullException.ThrowIfNull(defaultLayout);

		ScriptingStudioShellSettingsDocument document = LoadDocument();
		if (!File.Exists(_settingsPath))
		{
			ScriptingStudioShellWorkspaceSettings defaultSettings = new()
			{
				DockPanelState = DockPanelStateCloneHelper.Clone(defaultLayout)
			};
			document.SetWorkspace(workspaceKind, defaultSettings.Clone());
			SaveDocument(document);
			return defaultSettings;
		}

		ScriptingStudioShellWorkspaceSettings workspaceSettings = document.GetWorkspace(workspaceKind).Clone();
		return workspaceSettings;
	}

	public void Save(ScriptingWorkspaceKind workspaceKind, ScriptingStudioShellWorkspaceSettings settings)
	{
		ArgumentNullException.ThrowIfNull(settings);

		ScriptingStudioShellSettingsDocument document = LoadDocument();
		document.SetWorkspace(workspaceKind, settings.Clone());
		SaveDocument(document);
	}

	public bool IsLuaEnabled(ScriptingWorkspaceKind workspaceKind)
		=> LoadDocument().GetWorkspace(workspaceKind).LuaEnabled;

	private ScriptingStudioShellSettingsDocument LoadDocument()
	{
		try
		{
			if (!File.Exists(_settingsPath))
				return new ScriptingStudioShellSettingsDocument();

			return XmlUtils.ReadXmlFile<ScriptingStudioShellSettingsDocument>(_settingsPath);
		}
		catch (Exception) when (File.Exists(_settingsPath))
		{
			return new ScriptingStudioShellSettingsDocument();
		}
	}

	private void SaveDocument(ScriptingStudioShellSettingsDocument document)
	{
		string? directoryPath = Path.GetDirectoryName(_settingsPath);
		if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
			Directory.CreateDirectory(directoryPath);

		XmlUtils.WriteXmlFile(_settingsPath, document);
	}

	public bool SaveShortcutOverrides(ScriptingWorkspaceKind workspaceKind, KeyBindingOverrideCollection overrides)
	{
		ArgumentNullException.ThrowIfNull(overrides);

		try
		{
			ScriptingStudioShellSettingsDocument document = LoadDocument();
			ScriptingStudioShellWorkspaceSettings workspaceSettings = document.GetWorkspace(workspaceKind);
			workspaceSettings.ShortcutOverrides = overrides;
			document.SetWorkspace(workspaceKind, workspaceSettings);

			// Write to a temporary file first, then atomically replace.
			string tempPath = _settingsPath + ".tmp";
			XmlUtils.WriteXmlFile(tempPath, document);
			File.Move(tempPath, _settingsPath, overwrite: true);

			return true;
		}
		catch (Exception ex)
		{
			LogManager.GetCurrentClassLogger().Error(ex, "Failed to save shortcut overrides for {0}.", workspaceKind);
			return false;
		}
	}

	private static string DefaultSettingsPath => Path.Combine(DefaultPaths.ConfigsDirectory, "TombIDEScriptingStudioAddonSettings.xml");
}

internal static class DockPanelStateCloneHelper
{
	public static DockPanelState Clone(DockPanelState? state)
	{
		if (state is null)
			return new DockPanelState();

		var clone = new DockPanelState();

		foreach (DockRegionState region in state.Regions ?? [])
			clone.Regions.Add(Clone(region));

		return clone;
	}

	private static DockGroupState Clone(DockGroupState state) => new()
	{
		Contents = new List<string>(state.Contents ?? []),
		Order = state.Order,
		Size = state.Size,
		VisibleContent = state.VisibleContent ?? string.Empty
	};

	private static DockRegionState Clone(DockRegionState state)
	{
		var clone = new DockRegionState(state.Area, state.Size);

		foreach (DockGroupState group in state.Groups ?? [])
			clone.Groups.Add(Clone(group));

		return clone;
	}
}
