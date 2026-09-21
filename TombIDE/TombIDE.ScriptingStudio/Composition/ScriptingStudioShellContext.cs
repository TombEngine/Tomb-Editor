#nullable enable

using System;
using TombIDE.Shared.Messaging.Scripting;

namespace TombIDE.ScriptingStudio.Composition;

/// <summary>
/// Scoped shell-input bridge initialized exactly once by the shell factory
/// from the IDE-derived project context. Resolving before initialization or
/// initializing twice fails deterministically.
/// </summary>
internal sealed class ScriptingStudioShellContext
{
	private IScriptingProjectContext? _projectContext;
	private bool _isInitialized;

	/// <summary>
	/// Gets the project context for the current shell scope.
	/// Throws <see cref="InvalidOperationException"/> if the context has not been initialized.
	/// </summary>
	public IScriptingProjectContext ProjectContext
	{
		get
		{
			if (!_isInitialized || _projectContext is null)
			{
				throw new InvalidOperationException(
					"ScriptingStudioShellContext has not been initialized. " +
					"The shell factory must initialize the context before any scoped service resolves it.");
			}

			return _projectContext;
		}
	}

	/// <summary>
	/// Initializes the context exactly once with the given project context.
	/// Throws <see cref="InvalidOperationException"/> if already initialized.
	/// </summary>
	public void Initialize(IScriptingProjectContext projectContext)
	{
		ArgumentNullException.ThrowIfNull(projectContext);

		if (_isInitialized)
		{
			throw new InvalidOperationException(
				"ScriptingStudioShellContext has already been initialized. " +
				"Each shell scope must have its own context and must not be re-initialized.");
		}

		_projectContext = projectContext;
		_isInitialized = true;
	}
}
