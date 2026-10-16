#nullable enable

using System;
using System.Collections.Generic;
using System.Windows;
using Nickelony.IDEKit.KeyBindings;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.ToolStrips;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;

namespace TombIDE.ScriptingStudio.Shell;

internal sealed class ToolBarService : IToolBarService
{
	private static readonly UICommand[] ShellOwnedCommands =
	[
		UICommand.Settings,
		UICommand.UseNewInclude,
		UICommand.ShowLogsAfterBuild,
		UICommand.RestoreDefaultLayout,
		UICommand.ToolStrip,
		UICommand.StatusStrip
	];

	private readonly StudioToolStrip _toolStrip;
	private readonly ScriptingWorkspaceProfile _workspaceProfile;

	public ToolBarService(
		ScriptingWorkspaceProfile workspaceProfile,
		IKeyBindingService<UICommand> keyBindingService)
	{
		ArgumentNullException.ThrowIfNull(workspaceProfile);
		ArgumentNullException.ThrowIfNull(keyBindingService);

		_workspaceProfile = workspaceProfile;
		_toolStrip = new StudioToolStrip
		{
			KeyBindingService = keyBindingService,
			WorkspaceContributionItems = workspaceProfile.ToolStripContributions
		};

		_toolStrip.ItemClicked += HandleCommandInvoked;
		_toolStrip.RebuildWorkspaceItems();

		ApplyWorkspaceVisibility();
	}

	public event EventHandler<StudioCommandInvokedEventArgs>? CommandInvoked;

	public FrameworkElement ToolBarView => _toolStrip.View;

	public void SetCommandChecked(UICommand command, bool isChecked)
		=> _toolStrip.SetCommandChecked(command, isChecked);

	public void SetCommandEnabled(UICommand command, bool isEnabled)
		=> _toolStrip.SetCommandEnabled(command, isEnabled);

	public void SetCommandVisible(UICommand command, bool isVisible)
		=> _toolStrip.SetCommandVisible(command, isVisible);

	public void SetCommandText(UICommand command, string text)
		=> _toolStrip.SetCommandText(command, text);

	public void SetCommandToolTip(UICommand command, string toolTipText)
		=> _toolStrip.SetCommandToolTip(command, toolTipText);

	public void SetDocumentCommandSurface(
		DocumentMode documentMode,
		IReadOnlyList<StudioToolStripItem> toolStripItems)
	{
		_toolStrip.DocumentModeContributionItems = toolStripItems ?? [];
		_toolStrip.DocumentMode = documentMode;
		_toolStrip.RebuildDocumentModeItems();
	}

	public void UpdateCommandEnabledStates(Func<UICommand, bool> canExecuteCommand)
	{
		ArgumentNullException.ThrowIfNull(canExecuteCommand);

		foreach (UICommand command in Enum.GetValues<UICommand>())
		{
			bool isEnabled = IsShellOwnedCommand(command)
				? IsCommandEnabled(command)
				: canExecuteCommand(command);

			_toolStrip.SetCommandEnabled(command, isEnabled);
		}
	}

	public void Dispose()
		=> _toolStrip.ItemClicked -= HandleCommandInvoked;

	private void ApplyWorkspaceVisibility()
	{
		foreach (UICommand command in Enum.GetValues<UICommand>())
		{
			bool isVisible = command switch
			{
				UICommand.ToolStrip => _workspaceProfile.SupportsView(UICommand.ToolStrip),
				UICommand.StatusStrip => _workspaceProfile.SupportsView(UICommand.StatusStrip),
				UICommand.UseNewInclude => _workspaceProfile.Kind == ScriptingWorkspaceKind.ClassicScript,
				UICommand.Build => _workspaceProfile.SupportsBuild,
				UICommand.ShowLogsAfterBuild => _workspaceProfile.SupportsBuild,
				UICommand.ScriptingDocumentation => _workspaceProfile.SupportsDocumentation,
				_ => true
			};

			_toolStrip.SetCommandVisible(command, isVisible);
		}
	}

	private void HandleCommandInvoked(object? sender, StudioCommandInvokedEventArgs e)
		=> CommandInvoked?.Invoke(this, e);

	private bool IsShellOwnedCommand(UICommand command)
		=> Array.IndexOf(ShellOwnedCommands, command) >= 0;

	private bool IsCommandEnabled(UICommand command) => command switch
	{
		UICommand.Settings => true,
		UICommand.UseNewInclude => _workspaceProfile.Kind == ScriptingWorkspaceKind.ClassicScript,
		UICommand.ShowLogsAfterBuild => _workspaceProfile.SupportsBuild,
		UICommand.RestoreDefaultLayout => true,
		UICommand.ToolStrip => _workspaceProfile.SupportsView(UICommand.ToolStrip),
		UICommand.StatusStrip => _workspaceProfile.SupportsView(UICommand.StatusStrip),
		_ => false
	};
}
