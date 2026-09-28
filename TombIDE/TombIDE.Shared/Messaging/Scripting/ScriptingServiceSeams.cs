#nullable enable

using System;
using TombIDE.Shared.NewStructure;

namespace TombIDE.Shared.Messaging.Scripting;

/// <summary>
/// Exposes the narrow project-state surface needed by the rebuilt scripting host.
/// </summary>
public interface IScriptingProjectContext
{
	/// <summary>
	/// Gets the current TombIDE project.
	/// </summary>
	IGameProject Project { get; }

	/// <summary>
	/// Gets the currently selected level when one exists.
	/// </summary>
	ILevelProject? SelectedLevel { get; }

	/// <summary>
	/// Gets the current project script root directory.
	/// </summary>
	string ScriptRootDirectoryPath { get; }

	/// <summary>
	/// Gets the current project levels directory.
	/// </summary>
	string LevelsDirectoryPath { get; }
}

/// <summary>
/// IDE-backed implementation of <see cref="IScriptingProjectContext"/>.
/// </summary>
public sealed class IdeScriptingProjectContext : IScriptingProjectContext
{
	private readonly IDE _ide;

	/// <summary>
	/// Initializes a new project-context adapter.
	/// </summary>
	/// <param name="ide">The IDE instance to adapt.</param>
	public IdeScriptingProjectContext(IDE ide)
	{
		_ide = ide ?? throw new ArgumentNullException(nameof(ide));
	}

	/// <inheritdoc/>
	public IGameProject Project
		=> _ide.Project ?? throw new InvalidOperationException("The TombIDE project has not been initialized.");

	/// <inheritdoc/>
	public ILevelProject? SelectedLevel => _ide.SelectedLevel;

	/// <inheritdoc/>
	public string ScriptRootDirectoryPath
		=> _ide.Project is null ? string.Empty : _ide.Project.GetScriptRootDirectory();

	/// <inheritdoc/>
	public string LevelsDirectoryPath
		=> _ide.Project is null ? string.Empty : _ide.Project.LevelsDirectoryPath;
}

/// <summary>
/// Exposes the narrow lifecycle actions needed by the rebuilt scripting host.
/// </summary>
public interface IScriptingLifecycleService
{
	/// <summary>
	/// Requests a synchronous can-close decision.
	/// </summary>
	bool CanClose();

	/// <summary>
	/// Requests host shutdown.
	/// </summary>
	void RequestClose();
}

/// <summary>
/// IDE-backed implementation of <see cref="IScriptingLifecycleService"/>.
/// </summary>
public sealed class IdeScriptingLifecycleService : IScriptingLifecycleService
{
	private readonly IDE _ide;

	/// <summary>
	/// Initializes a new lifecycle adapter.
	/// </summary>
	/// <param name="ide">The IDE instance to adapt.</param>
	public IdeScriptingLifecycleService(IDE ide)
	{
		_ide = ide ?? throw new ArgumentNullException(nameof(ide));
	}

	/// <inheritdoc/>
	public bool CanClose() => _ide.CanClose();

	/// <inheritdoc/>
	public void RequestClose() => _ide.RequestProgramClose();
}
