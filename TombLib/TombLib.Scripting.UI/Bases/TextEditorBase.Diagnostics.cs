using ICSharpCode.AvalonEdit.Rendering;
using Nickelony.IDEKit.AvalonEdit.Diagnostics;
using Nickelony.IDEKit.AvalonEdit.LanguageFeatures.Diagnostics;
using Nickelony.IDEKit.Core.Diagnostics;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.UI.Rendering;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.UI.Bases;

public abstract partial class TextEditorBase
{
	/// <summary>
	/// Occurs after the diagnostics owned by this editor change.
	/// </summary>
	public event EventHandler? DiagnosticsChanged;

	/// <summary>
	/// Sets the diagnostics displayed for the current document.
	/// </summary>
	/// <param name="diagnostics">The diagnostics to display.</param>
	public void SetDiagnostics(IReadOnlyList<TextDiagnostic> diagnostics)
	{
		EnsureNotDisposed();

		_diagnosticToolTipService.SetDiagnostics(diagnostics);
		DiagnosticsChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Clears all diagnostics currently displayed for the document.
	/// </summary>
	public void ClearDiagnostics()
	{
		EnsureNotDisposed();

		if (_diagnosticToolTipService.ClearDiagnostics())
			DiagnosticsChanged?.Invoke(this, EventArgs.Empty);
	}

	internal void InvalidateDiagnosticLayer()
	{
		TextArea.TextView.InvalidateLayer(KnownLayer.Background);
		TextArea.TextView.InvalidateLayer(KnownLayer.Selection);
		TextArea.TextView.InvalidateLayer(KnownLayer.Caret);
		TextArea.TextView.InvalidateVisual();
	}

	private void HandleErrorToolTips(MouseEventArgs e)
	{
		int hoveredOffset = GetOffsetFromPoint(e.GetPosition(this));

		if (hoveredOffset == -1)
			return;

		TryShowDiagnosticToolTip(hoveredOffset);
	}

	/// <summary>
	/// Attempts to retrieve diagnostic information for the given offset.
	/// </summary>
	/// <param name="hoveredOffset">The document offset to inspect.</param>
	/// <param name="diagnosticInfo">The diagnostic information, when found.</param>
	/// <param name="allowLineFallback">Whether to fall back to a line-wide diagnostic.</param>
	/// <returns><see langword="true"/> if diagnostic information was found; otherwise <see langword="false"/>.</returns>
	protected bool TryGetDiagnosticInfo(int hoveredOffset, [NotNullWhen(true)] out TextDiagnostic? diagnosticInfo, bool allowLineFallback = true)
		=> _diagnosticToolTipService.TryGetDiagnosticInfo(Document, hoveredOffset, IntelliSenseEnabled && LiveErrorUnderlining, allowLineFallback, out diagnosticInfo);

	/// <summary>
	/// Shows a diagnostic tooltip with the given diagnostic information.
	/// </summary>
	/// <param name="diagnosticInfo">The diagnostic information to display.</param>
	public void ShowDiagnosticToolTip(TextDiagnostic diagnosticInfo)
	{
		EnsureNotDisposed();

		TextEditorToolTipHelper.GetDiagnosticToolTipColors(diagnosticInfo.Severity, out SolidColorBrush border, out SolidColorBrush background);
		ShowToolTip(diagnosticInfo.Message, border, background, TextEditorColorPalette.ToolTipForeground);
	}

	/// <summary>
	/// Attempts to show a diagnostic tooltip for the given offset.
	/// </summary>
	/// <param name="hoveredOffset">The document offset to inspect.</param>
	/// <returns><see langword="true"/> if a diagnostic tooltip was shown; otherwise <see langword="false"/>.</returns>
	protected bool TryShowDiagnosticToolTip(int hoveredOffset)
	{
		EnsureNotDisposed();

		if (!TryGetDiagnosticInfo(hoveredOffset, out TextDiagnostic? diagnosticInfo))
			return false;

		ShowDiagnosticToolTip(diagnosticInfo);
		return true;
	}

	private IReadOnlyList<TextDiagnosticSegment> CreateDiagnosticSegments()
	{
		if (!IntelliSenseEnabled || !LiveErrorUnderlining)
			return [];

		// The projection (and the renderer wiring) is provided by the AvalonEdit IntelliSense package.
		return TextDiagnosticSegmentFactory.Create(Diagnostics);
	}
}
