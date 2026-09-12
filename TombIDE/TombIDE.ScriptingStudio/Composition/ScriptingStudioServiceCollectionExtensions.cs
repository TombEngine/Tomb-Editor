#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MvvmDialogs;
using NLog.Extensions.Logging;
using Nickelony.IDEKit.KeyBindings;
using System;
using System.Windows.Input;
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
		services.AddScoped<IWorkspaceFileSystem>(_ => new WorkspaceFileCodec());
		services.AddScoped<IWorkspaceDocumentStore>(sp =>
			new WorkspaceDocumentStore(sp.GetRequiredService<IWorkspaceFileSystem>()));
		services.AddScoped<IWorkspaceDocumentManager>(sp =>
			new WorkspaceDocumentManager(
				sp.GetRequiredService<IWorkspaceDocumentStore>(),
				sp.GetRequiredService<IUiDispatcherService>().Invoke));

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
			new(UICommand.NewFile, nameof(UICommand.NewFile), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.N, ModifierKeys.Control)),
			new(UICommand.Save, nameof(UICommand.Save), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.S, ModifierKeys.Control)),
			new(UICommand.SaveAll, nameof(UICommand.SaveAll), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.S, ModifierKeys.Control | ModifierKeys.Shift)),
			new(UICommand.Build, nameof(UICommand.Build), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.F9, ModifierKeys.None)),
			new(UICommand.Exit, nameof(UICommand.Exit), isRemappable: false, isHostReserved: true,
				new KeyCombo(Key.F4, ModifierKeys.Alt)),
			new(UICommand.Undo, nameof(UICommand.Undo), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.Z, ModifierKeys.Control)),
			new(UICommand.Redo, nameof(UICommand.Redo), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.Y, ModifierKeys.Control)),
			new(UICommand.Cut, nameof(UICommand.Cut), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.X, ModifierKeys.Control)),
			new(UICommand.Copy, nameof(UICommand.Copy), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.C, ModifierKeys.Control)),
			new(UICommand.Paste, nameof(UICommand.Paste), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.V, ModifierKeys.Control)),
			new(UICommand.Find, nameof(UICommand.Find), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.F, ModifierKeys.Control),
				new KeyCombo(Key.H, ModifierKeys.Control)),
			new(UICommand.SelectAll, nameof(UICommand.SelectAll), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.A, ModifierKeys.Control)),
			new(UICommand.Reindent, nameof(UICommand.Reindent), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.R, ModifierKeys.Control)),
			new(UICommand.TrimWhiteSpace, nameof(UICommand.TrimWhiteSpace), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.R, ModifierKeys.Control | ModifierKeys.Shift)),
			new(UICommand.ToggleComment, nameof(UICommand.ToggleComment), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.OemQuestion, ModifierKeys.Control)),
			new(UICommand.CommentOut, nameof(UICommand.CommentOut), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.C, ModifierKeys.Control | ModifierKeys.Shift)),
			new(UICommand.Uncomment, nameof(UICommand.Uncomment), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.U, ModifierKeys.Control | ModifierKeys.Shift)),
			new(UICommand.ToggleBookmark, nameof(UICommand.ToggleBookmark), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.B, ModifierKeys.Control)),
			new(UICommand.PrevBookmark, nameof(UICommand.PrevBookmark), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.OemComma, ModifierKeys.Control)),
			new(UICommand.NextBookmark, nameof(UICommand.NextBookmark), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.OemPeriod, ModifierKeys.Control)),
			new(UICommand.ClearBookmarks, nameof(UICommand.ClearBookmarks), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.B, ModifierKeys.Control | ModifierKeys.Shift)),
			new(UICommand.PrevSection, nameof(UICommand.PrevSection), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.Left, ModifierKeys.Control)),
			new(UICommand.NextSection, nameof(UICommand.NextSection), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.Right, ModifierKeys.Control)),
			new(UICommand.ClearString, nameof(UICommand.ClearString), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.Delete, ModifierKeys.None)),
			new(UICommand.RemoveLastString, nameof(UICommand.RemoveLastString), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.Delete, ModifierKeys.Control)),
			new(UICommand.NavigateBack, nameof(UICommand.NavigateBack), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.Left, ModifierKeys.Alt)),
			new(UICommand.NavigateForward, nameof(UICommand.NavigateForward), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.Right, ModifierKeys.Alt)),
			new(UICommand.GoToDefinition, nameof(UICommand.GoToDefinition), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.F12, ModifierKeys.None)),
			new(UICommand.FindReferences, nameof(UICommand.FindReferences), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.F12, ModifierKeys.Shift)),
			new(UICommand.RenameSymbol, nameof(UICommand.RenameSymbol), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.F2, ModifierKeys.None)),
			new(UICommand.TypeFirstAvailableId, nameof(UICommand.TypeFirstAvailableId), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.F1, ModifierKeys.None)),
			new(UICommand.NewFileAtCaret, nameof(UICommand.NewFileAtCaret), isRemappable: true, isHostReserved: false,
				new KeyCombo(Key.F5, ModifierKeys.Control))
		};

		return new CommandCatalog<UICommand>(descriptors);
	}
}
