using Nickelony.IDEKit.Core.Diagnostics;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using System;

namespace TombLib.Scripting.UI.Diagnostics;

/// <summary>
/// Formats diagnostic messages for editor tooltips in the shell's presentation style: a severity
/// label line followed by the raw message, with built-in labels recognized so a pre-formatted
/// message is never prefixed twice. The shared IntelliSense package intentionally ships selection
/// only, so these presentation rules live with the host.
/// </summary>
internal static class TextDiagnosticMessageFormatter
{
	/// <summary>
	/// Gets the non-localized display label for the supplied severity, falling back to
	/// <c>"Diagnostic"</c> for <see cref="TextDiagnosticSeverity.None"/> and undefined values.
	/// </summary>
	/// <param name="severity">The severity value.</param>
	/// <returns>The display label for the supplied severity.</returns>
	public static string GetLabel(TextDiagnosticSeverity severity) => severity switch
	{
		TextDiagnosticSeverity.Error => "Error",
		TextDiagnosticSeverity.Warning => "Warning",
		TextDiagnosticSeverity.Information => "Information",
		TextDiagnosticSeverity.Hint => "Hint",
		_ => "Diagnostic"
	};

	/// <summary>
	/// Formats a diagnostic message, prefixing the mapped severity label when the message does not
	/// already begin with that label or with the built-in label of the diagnostic's own severity. A
	/// <see langword="null"/> or blank label emits the raw message.
	/// </summary>
	/// <remarks>
	/// A message that already starts with the mapped label or with the built-in label of the
	/// diagnostic's own severity (both matched case-insensitively and followed by a colon) is
	/// returned unchanged, so a pre-formatted message is never prefixed twice. A prefix that names
	/// a different severity does not suppress the mapped label; the conflicting prefix stays in the
	/// message after the label line.
	/// </remarks>
	/// <param name="diagnostic">The diagnostic to format.</param>
	/// <param name="severityLabel">
	/// A function that maps a severity to its label, or <see langword="null"/> to emit raw messages.
	/// The returned label is trimmed of surrounding whitespace and of a trailing colon.
	/// </param>
	/// <returns>The formatted message, or an empty string when the diagnostic message is blank.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="diagnostic"/> is <see langword="null"/>.
	/// </exception>
	public static string FormatMessage(
		TextDiagnostic diagnostic,
		Func<TextDiagnosticSeverity, string>? severityLabel = null)
	{
		ArgumentNullException.ThrowIfNull(diagnostic);

		if (string.IsNullOrWhiteSpace(diagnostic.Message))
			return string.Empty;

		if (severityLabel is null)
			return diagnostic.Message;

		string label = (severityLabel(diagnostic.Severity) ?? string.Empty).Trim().TrimEnd(':').Trim();

		if (label.Length == 0 || IsSeverityPrefixed(diagnostic.Message, label, diagnostic.Severity))
			return diagnostic.Message;

		return label + ":\n" + diagnostic.Message;
	}

	private static bool IsSeverityPrefixed(
		string message,
		string label,
		TextDiagnosticSeverity severity)
		=> message.StartsWith(label + ":", StringComparison.OrdinalIgnoreCase)
			|| message.StartsWith(GetLabel(severity) + ":", StringComparison.OrdinalIgnoreCase);
}
