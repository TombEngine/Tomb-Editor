#nullable enable

using System;
using System.Collections.Generic;
using System.Windows;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Shell;

/// <summary>
/// Owns the WPF status bar view and editor-context display.
/// Must not own command dispatch.
/// </summary>
public interface IStatusBarService : IDisposable
{
	/// <summary>
	/// Gets the status bar view as a WPF <see cref="FrameworkElement"/>.
	/// </summary>
	FrameworkElement StatusBarView { get; }

	/// <summary>
	/// Updates the status bar context for the given editor and document mode.
	/// </summary>
	void SetStatusStripContext(
		IEditorControl? editor,
		DocumentMode documentMode,
		IReadOnlyList<StudioStatusStripSegment> documentSegments);
}
