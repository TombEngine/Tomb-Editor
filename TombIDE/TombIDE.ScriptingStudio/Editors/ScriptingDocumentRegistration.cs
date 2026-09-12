#nullable enable

using Nickelony.IDEKit.IntelliSense.DocumentSymbols;
using System;
using System.Collections.Generic;
using System.Linq;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.DocumentOutline;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Editors;

public enum ScriptingDocumentConfigurationKind
{
	None,
	ClassicScript,
	GameFlowScript,
	TRX,
	Lua
}

public sealed class ScriptingDocumentContributions
{
	public ScriptingDocumentContributions(
		ScriptingSettingsPageKind? settingsPageKind,
		ScriptingDocumentConfigurationKind configurationKind,
		IStudioDocumentCommandSurfaceProvider? commandSurfaceProvider = null,
		Func<ITextDocumentSymbolProvider?>? outlineProviderFactory = null,
		IStudioDocumentStatusStripProvider? statusStripProvider = null)
	{
		SettingsPageKind = settingsPageKind;
		ConfigurationKind = configurationKind;
		CommandSurfaceProvider = commandSurfaceProvider;
		OutlineProviderFactory = outlineProviderFactory;
		StatusStripProvider = statusStripProvider;
	}

	public ScriptingSettingsPageKind? SettingsPageKind { get; }

	public ScriptingDocumentConfigurationKind ConfigurationKind { get; }

	public IStudioDocumentCommandSurfaceProvider? CommandSurfaceProvider { get; }

	public Func<ITextDocumentSymbolProvider?>? OutlineProviderFactory { get; }

	public IStudioDocumentStatusStripProvider? StatusStripProvider { get; }

	public static ScriptingDocumentContributions None { get; } = new(null, ScriptingDocumentConfigurationKind.None);

}

public sealed class ScriptingDocumentRegistration
{
	public ScriptingDocumentRegistration(
		EditorType editorType,
		DocumentMode documentMode,
		Func<string, bool> supportsFile,
		Func<string, bool> isDefaultForFile,
		Func<Version, IEditorControl> factory,
		ScriptingDocumentContributions contributions,
		bool isFallback = false,
		IReadOnlyList<DocumentMode>? supportedDocumentModes = null)
	{
		EditorType = editorType;
		DocumentMode = documentMode;
		SupportsFile = supportsFile ?? throw new ArgumentNullException(nameof(supportsFile));
		IsDefaultForFile = isDefaultForFile ?? throw new ArgumentNullException(nameof(isDefaultForFile));
		Factory = factory ?? throw new ArgumentNullException(nameof(factory));
		ArgumentNullException.ThrowIfNull(contributions);
		Contributions = contributions;
		IsFallback = isFallback;
		SupportedDocumentModes = supportedDocumentModes?.ToArray() ?? [documentMode];
	}

	public EditorType EditorType { get; }

	public DocumentMode DocumentMode { get; }

	public Func<Version, IEditorControl> Factory { get; }

	public Func<string, bool> IsDefaultForFile { get; }

	public Func<string, bool> SupportsFile { get; }

	public ScriptingDocumentContributions Contributions { get; }

	public bool IsFallback { get; }

	public IReadOnlyList<DocumentMode> SupportedDocumentModes { get; }
}