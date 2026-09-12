using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.AvalonEdit.Documents;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace TombLib.Scripting.UI.Diagnostics;

internal sealed class TextDiagnosticToolTipService
{
	private readonly Action? _onDiagnosticsChanged;
	private IReadOnlyList<TextEditorDiagnostic> _diagnostics = [];

	public TextDiagnosticToolTipService(Action? onDiagnosticsChanged = null) => _onDiagnosticsChanged = onDiagnosticsChanged;

	public IReadOnlyList<TextEditorDiagnostic> Diagnostics => _diagnostics;

	public void SetDiagnostics(IReadOnlyList<TextEditorDiagnostic>? diagnostics)
	{
		_diagnostics = diagnostics ?? [];
		_onDiagnosticsChanged?.Invoke();
	}

	public bool ClearDiagnostics()
	{
		if (_diagnostics.Count == 0)
			return false;

		_diagnostics = [];
		_onDiagnosticsChanged?.Invoke();
		return true;
	}

	public bool TryGetDiagnosticInfo(
		TextDocument document,
		int hoveredOffset,
		bool liveErrorUnderlining,
		bool allowLineFallback,
		[NotNullWhen(true)] out TextEditorDiagnostic? info)
	{
		ArgumentNullException.ThrowIfNull(document);

		info = null;

		if (!liveErrorUnderlining || _diagnostics.Count == 0)
			return false;

		DocumentLine line = document.GetLineByOffset(document.ClampOffset(hoveredOffset));
		IReadOnlyList<TextEditorDiagnostic> hoveredDiagnostics = DiagnosticHitTester.SelectHoverDiagnostics(
			_diagnostics,
			hoveredOffset,
			allowLineFallback,
			line.Offset,
			Math.Max(line.EndOffset, line.Offset + 1));

		if (hoveredDiagnostics.Count == 0)
			return false;

		TextEditorDiagnosticSeverity severity = hoveredDiagnostics.Min(diagnostic => diagnostic.Severity);

		string? message = DiagnosticHitTester.BuildCombinedMessage(hoveredDiagnostics, GetSeverityLabel);

		if (string.IsNullOrWhiteSpace(message))
			return false;

		info = new TextEditorDiagnostic(
			severity,
			message,
			hoveredDiagnostics.Min(diagnostic => diagnostic.StartOffset),
			hoveredDiagnostics.Max(diagnostic => diagnostic.EndOffset));
		return true;
	}

	private static string GetSeverityLabel(TextEditorDiagnosticSeverity severity)
		=> severity.GetLabel();
}
