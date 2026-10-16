#nullable enable

using System;
using TombIDE.ScriptingStudio.Shell;

namespace TombIDE.ScriptingStudio.Workbench;

/// <summary>
/// Groups the delegate-based options for <see cref="ScriptingMessageService"/>,
/// reducing constructor parameter count.
/// </summary>
public sealed class ScriptingMessageServiceOptions
{
	/// <summary>
	/// Gets a callback that captures the current AvalonDock layout XML.
	/// </summary>
	public Func<string> GetDockLayoutXml { get; init; } = () => string.Empty;

	/// <summary>
	/// Gets a callback that shows the compiler logs pane.
	/// </summary>
	public Action ShowCompilerLogsPane { get; init; } = () => { };

	/// <summary>
	/// Gets a callback that updates the compiler logs display.
	/// </summary>
	public Action<string> UpdateCompilerLogs { get; init; } = _ => { };

	/// <summary>
	/// Gets a callback that returns whether compiler logs should be shown after a build.
	/// </summary>
	public Func<bool> ShowCompilerLogsAfterBuild { get; init; } = () => false;

	/// <summary>
	/// Gets a callback that returns whether the new include method should be used.
	/// </summary>
	public Func<bool> UseNewIncludeMethod { get; init; } = () => false;

	/// <summary>
	/// Gets the host operations bridge for outbound calls to the TombIDE shell.
	/// </summary>
	public IScriptingHostOperations HostOperations { get; init; } = DefaultScriptingHostOperations.Instance;
}
