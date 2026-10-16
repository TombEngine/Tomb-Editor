#nullable enable

using System;

namespace TombIDE.ScriptingStudio.Composition;

/// <summary>
/// Scoped bridge for delegates that connect RootShellViewModel settings
/// to WorkbenchPaneService. RootShellViewModel sets the delegates after
/// construction; DocumentWorkbenchService reads them.
/// This will be replaced by explicit settings interfaces in Phase 5/6.
/// </summary>
internal sealed class ShellWorkbenchSettings
{
	public Func<bool> GetInfoBoxAlwaysOnTop { get; set; } = () => false;

	public Action<bool> SetInfoBoxAlwaysOnTop { get; set; } = _ => { };

	public Func<bool> GetInfoBoxCloseTabsOnClose { get; set; } = () => false;

	public Action<bool> SetInfoBoxCloseTabsOnClose { get; set; } = _ => { };

	public Func<bool> ShowCompilerLogsAfterBuild { get; set; } = () => false;

	public Func<bool> UseNewIncludeMethod { get; set; } = () => false;
}
