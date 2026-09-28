using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.AvalonEdit.Documents;
using Nickelony.IDEKit.Core.Diagnostics;
using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace TombLib.Scripting.UI.Diagnostics;

internal sealed class TextDiagnosticToolTipService
{
	private readonly Action? _onDiagnosticsChanged;
	private IReadOnlyList<TextDiagnostic> _diagnostics = [];

	public TextDiagnosticToolTipService(Action? onDiagnosticsChanged = null) => _onDiagnosticsChanged = onDiagnosticsChanged;

	public IReadOnlyList<TextDiagnostic> Diagnostics => _diagnostics;

	public void SetDiagnostics(IReadOnlyList<TextDiagnostic>? diagnostics)
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
		[NotNullWhen(true)] out TextDiagnostic? info)
	{
		ArgumentNullException.ThrowIfNull(document);

		info = null;

		if (!liveErrorUnderlining || _diagnostics.Count == 0)
			return false;

		DocumentLine line = document.GetLineByOffset(document.ClampOffset(hoveredOffset));
		IReadOnlyList<TextDiagnostic> hoveredDiagnostics = SelectHoverDiagnostics(
			_diagnostics,
			hoveredOffset,
			allowLineFallback ? new TextRange(line.Offset, Math.Max(line.Length, 1)) : null);

		if (hoveredDiagnostics.Count == 0)
			return false;

		TextDiagnosticSeverity severity = hoveredDiagnostics.Min(diagnostic => diagnostic.Severity);

		string? message = BuildCombinedMessage(hoveredDiagnostics);

		if (string.IsNullOrWhiteSpace(message))
			return false;

		info = new TextDiagnostic(
			severity,
			message,
			hoveredDiagnostics.Min(diagnostic => diagnostic.StartOffset),
			hoveredDiagnostics.Max(diagnostic => diagnostic.EndOffset));
		return true;
	}

	// The host-side hover selection policy: an exact-offset hit wins; the containing-line range is only
	// consulted when the exact offset has no diagnostic. Both primitives live in the shared package and
	// are composed here, because the composite policy is a tooltip presentation decision.
	private static IReadOnlyList<TextDiagnostic> SelectHoverDiagnostics(
		IReadOnlyList<TextDiagnostic> diagnostics,
		int hoveredOffset,
		TextRange? lineFallbackRange)
	{
		IReadOnlyList<TextDiagnostic> exactMatches = DiagnosticHitTester.GetDiagnosticsAtOffset(diagnostics, hoveredOffset);

		if (exactMatches.Count > 0 || lineFallbackRange is not { } fallbackRange)
			return exactMatches;

		return DiagnosticHitTester.GetDiagnosticsForRange(diagnostics, fallbackRange.Offset, fallbackRange.EndOffset);
	}

	private static string? BuildCombinedMessage(IReadOnlyList<TextDiagnostic> diagnostics)
	{
		string message = string.Join(
			"\n\n",
			diagnostics
				.Select(FormatDiagnostic)
				.Where(text => !string.IsNullOrWhiteSpace(text))
				.Distinct(StringComparer.Ordinal));

		return string.IsNullOrWhiteSpace(message) ? null : message;
	}

	private static string FormatDiagnostic(TextDiagnostic diagnostic)
	{
		string message = TextDiagnosticMessageFormatter.FormatMessage(diagnostic, GetSeverityLabel);

		if (string.IsNullOrWhiteSpace(message))
			return message;

		string attribution = GetSourceAttribution(diagnostic);

		return attribution.Length == 0 ? message : message + "\n\n" + attribution;
	}

	private static string GetSourceAttribution(TextDiagnostic diagnostic)
	{
		string? source = diagnostic.Source;
		string? code = diagnostic.Code;

		if (source is null && code is null)
			return string.Empty;

		if (source is null)
			return "(" + code + ")";

		return code is null ? source : source + " (" + code + ")";
	}

	private static string GetSeverityLabel(TextDiagnosticSeverity severity)
		=> TextDiagnosticMessageFormatter.GetLabel(severity);
}
