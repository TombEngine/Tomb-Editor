#nullable enable

using Microsoft.Extensions.DependencyInjection;
using System;
using TombIDE.ScriptingStudio.Host;
using TombIDE.ScriptingStudio.Settings;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.Shared;
using TombIDE.Shared.Messaging;
using TombIDE.Shared.Messaging.Scripting;
using Nickelony.IDEKit.Workspace.Documents;

namespace TombIDE.ScriptingStudio.Composition;

/// <summary>
/// Creates hostable ScriptingStudio shell instances.
/// This is the only temporary Scripting Studio constructor that accepts <see cref="IDE"/>;
/// it must not pass IDE beyond this boundary.
/// </summary>
public interface IScriptingStudioShellFactory
{
	/// <summary>
	/// Creates a shell instance for the supplied IDE context.
	/// </summary>
	/// <param name="ide">The current TombIDE instance.</param>
	/// <returns>A hostable shell instance.</returns>
	IScriptingStudioShell Create(IDE ide);
}

internal sealed class ScriptingStudioShellFactory : IScriptingStudioShellFactory
{
	private readonly IServiceProvider _serviceProvider;

	public ScriptingStudioShellFactory(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
	}

	public IScriptingStudioShell Create(IDE ide)
	{
		ArgumentNullException.ThrowIfNull(ide);

		AsyncServiceScope shellScope = _serviceProvider.CreateAsyncScope();

		try
		{
			IScriptingProjectContext projectContext = new IdeScriptingProjectContext(ide);

			ScriptingStudioShellContext context =
				shellScope.ServiceProvider.GetRequiredService<ScriptingStudioShellContext>();
			context.Initialize(projectContext);

			RootShellViewModel viewModel =
				shellScope.ServiceProvider.GetRequiredService<RootShellViewModel>();
			IWorkspaceDocumentManager documentManager =
				shellScope.ServiceProvider.GetRequiredService<IWorkspaceDocumentManager>();
			IUiDispatcherService uiDispatcher =
				shellScope.ServiceProvider.GetRequiredService<IUiDispatcherService>();

			return new ScriptingStudioShell(shellScope, viewModel, documentManager, uiDispatcher);
		}
		catch
		{
			shellScope.DisposeAsync().AsTask().GetAwaiter().GetResult();
			throw;
		}
	}
}
