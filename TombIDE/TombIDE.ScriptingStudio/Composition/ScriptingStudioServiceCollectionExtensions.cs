#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MvvmDialogs;
using NLog.Extensions.Logging;
using Nickelony.IDEKit.KeyBindings;
using System;
using System.Threading.Tasks;
using TombIDE.ScriptingStudio.ClassicScript;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.DocumentOutline;
using TombIDE.ScriptingStudio.FileExplorer;
using TombIDE.ScriptingStudio.FindAndReplace;
using TombIDE.ScriptingStudio.Host;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Settings;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Workbench;
using TombIDE.ScriptingStudio.Workspace;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Messaging;
using TombIDE.Shared.Messaging.Scripting;
using TombIDE.Shared.SharedClasses;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Diagnostics;
using TombLib.Scripting.ClassicScript.Hover;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Navigation;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Signatures;
using TombLib.Scripting.ClassicScript.Syntaxes;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.GameFlowScript.Completion;
using TombLib.Scripting.GameFlowScript.Hover;
using TombLib.Scripting.GameFlowScript.Navigation;
using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.TRX;
using TombLib.Scripting.TRX.Completion;
using TombLib.Scripting.TRX.Hover;
using TombLib.Scripting.TRX.Navigation;
using TombLib.Scripting.TRX.Services;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Editing;
using Nickelony.IDEKit.Workspace.Documents;
using TombLib.WPF.Services.Abstract;
using Nickelony.IDEKit.Workspace.Documents.FileSystem;

namespace TombIDE.ScriptingStudio.Composition;

/// <summary>
/// Registers the services needed to host the active ScriptingStudio shell path.
/// </summary>
public static partial class ScriptingStudioServiceCollectionExtensions
{
	public static IServiceCollection AddScriptingStudioHostComposition(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		// Route Microsoft.Extensions.Logging (used by the Nickelony language server packages) into the app's NLog targets.
		services.AddLogging(builder => builder.AddNLog());

		services.AddSingleton<IMessenger>(_ => new WeakReferenceMessenger());
		services.AddTransient<IUiDispatcherService>(_ => SynchronizationContextUiDispatcherService.FromCurrentContext());
		services.AddSingleton(CreateDefaultCommandCatalog);
		services.AddSingleton<StudioStatusStripContributionService>();
		services.AddTransient<IScriptingStudioShellFactory, ScriptingStudioShellFactory>();
		services.AddScoped<IWorkspaceFileSystem>(_ => new RecycleBinWorkspaceFileSystem(new LocalWorkspaceFileSystem()));
		services.AddScoped<IWorkspaceDocumentStore>(sp =>
			new WorkspaceDocumentStore(sp.GetRequiredService<IWorkspaceFileSystem>()));
		services.AddScoped<IWorkspaceDocumentManager>(sp =>
		{
			// The manager consumes an asynchronous dispatch seam: every view access is awaited through
			// this delegate. The host dispatcher executes the action synchronously on the UI context,
			// so completing the task immediately preserves the previous semantics; the manager never
			// depends on the task being incomplete.
			IUiDispatcherService dispatcher = sp.GetRequiredService<IUiDispatcherService>();
			return new WorkspaceDocumentManager(
				sp.GetRequiredService<IWorkspaceDocumentStore>(),
				action =>
				{
					dispatcher.Invoke(action);
					return Task.CompletedTask;
				});
		});

		AddClassicScriptServices(services);
		AddGameFlowServices(services);
		AddTrxServices(services);
		AddLuaServices(services);

		AddScriptingStudioShellServices(services);

		return services;
	}

	private static CommandCatalog<UICommand> CreateDefaultCommandCatalog(IServiceProvider _)
	{
		var descriptors = new CommandDescriptor<UICommand>[]
		{
			new(UICommand.NewFile, nameof(UICommand.NewFile), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.N, KeyModifiers.Control)),
			new(UICommand.Save, nameof(UICommand.Save), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.S, KeyModifiers.Control)),
			new(UICommand.SaveAll, nameof(UICommand.SaveAll), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.S, KeyModifiers.Control | KeyModifiers.Shift)),
			new(UICommand.Build, nameof(UICommand.Build), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.F9, KeyModifiers.None)),
			new(UICommand.Exit, nameof(UICommand.Exit), CommandRemappingPolicy.HostReserved,
				new KeyCombo(KeyCode.F4, KeyModifiers.Alt)),
			new(UICommand.Undo, nameof(UICommand.Undo), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.Z, KeyModifiers.Control)),
			new(UICommand.Redo, nameof(UICommand.Redo), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.Y, KeyModifiers.Control)),
			new(UICommand.Cut, nameof(UICommand.Cut), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.X, KeyModifiers.Control)),
			new(UICommand.Copy, nameof(UICommand.Copy), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.C, KeyModifiers.Control)),
			new(UICommand.Paste, nameof(UICommand.Paste), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.V, KeyModifiers.Control)),
			new(UICommand.Find, nameof(UICommand.Find), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.F, KeyModifiers.Control),
				new KeyCombo(KeyCode.H, KeyModifiers.Control)),
			new(UICommand.SelectAll, nameof(UICommand.SelectAll), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.A, KeyModifiers.Control)),
			new(UICommand.Reindent, nameof(UICommand.Reindent), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.R, KeyModifiers.Control)),
			new(UICommand.TrimWhiteSpace, nameof(UICommand.TrimWhiteSpace), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.R, KeyModifiers.Control | KeyModifiers.Shift)),
			new(UICommand.ToggleComment, nameof(UICommand.ToggleComment), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.Slash, KeyModifiers.Control)),
			new(UICommand.CommentOut, nameof(UICommand.CommentOut), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.C, KeyModifiers.Control | KeyModifiers.Shift)),
			new(UICommand.Uncomment, nameof(UICommand.Uncomment), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.U, KeyModifiers.Control | KeyModifiers.Shift)),
			new(UICommand.ToggleBookmark, nameof(UICommand.ToggleBookmark), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.B, KeyModifiers.Control)),
			new(UICommand.PrevBookmark, nameof(UICommand.PrevBookmark), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.Comma, KeyModifiers.Control)),
			new(UICommand.NextBookmark, nameof(UICommand.NextBookmark), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.Period, KeyModifiers.Control)),
			new(UICommand.ClearBookmarks, nameof(UICommand.ClearBookmarks), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.B, KeyModifiers.Control | KeyModifiers.Shift)),
			new(UICommand.PrevSection, nameof(UICommand.PrevSection), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.Left, KeyModifiers.Control)),
			new(UICommand.NextSection, nameof(UICommand.NextSection), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.Right, KeyModifiers.Control)),
			new(UICommand.ClearString, nameof(UICommand.ClearString), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.Delete, KeyModifiers.None)),
			new(UICommand.RemoveLastString, nameof(UICommand.RemoveLastString), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.Delete, KeyModifiers.Control)),
			new(UICommand.NavigateBack, nameof(UICommand.NavigateBack), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.Left, KeyModifiers.Alt)),
			new(UICommand.NavigateForward, nameof(UICommand.NavigateForward), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.Right, KeyModifiers.Alt)),
			new(UICommand.GoToDefinition, nameof(UICommand.GoToDefinition), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.F12, KeyModifiers.None)),
			new(UICommand.FindReferences, nameof(UICommand.FindReferences), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.F12, KeyModifiers.Shift)),
			new(UICommand.RenameSymbol, nameof(UICommand.RenameSymbol), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.F2, KeyModifiers.None)),
			new(UICommand.TypeFirstAvailableId, nameof(UICommand.TypeFirstAvailableId), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.F1, KeyModifiers.None)),
			new(UICommand.NewFileAtCaret, nameof(UICommand.NewFileAtCaret), CommandRemappingPolicy.Remappable,
				new KeyCombo(KeyCode.F5, KeyModifiers.Control))
		};

		return new CommandCatalog<UICommand>(descriptors);
	}
}
