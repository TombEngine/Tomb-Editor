using Nickelony.IDEKit.IntelliSense.Hover;
using NLog;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using TombLib.Scripting.TRX.Models;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Scripting.TRX.Hover;

/// <summary>
/// Resolves hover information for GameFlow schema properties.
/// </summary>
public sealed class TRXGameFlowHoverService : ITextHoverProvider
{
	private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

	private static readonly Regex s_propertyNamePattern = new(@"""([^""]+)""(?=\s*:)", RegexOptions.Compiled);

	private readonly ITRXGameFlowSchemaService _schemaService;

	/// <summary>
	/// Initializes a new instance of the <see cref="TRXGameFlowHoverService"/> class.
	/// </summary>
	/// <param name="schemaService">The schema service used to resolve hover information.</param>
	public TRXGameFlowHoverService(ITRXGameFlowSchemaService schemaService) => _schemaService = schemaService;

	/// <inheritdoc/>
	public TextHoverInfo? GetHoverInfo(TextHoverRequest request)
	{
		try
		{
			var model = _schemaService.Model;

			if (model is null)
				return null;

			// Only JSON property-name positions are hoverable: a quoted key followed by a colon.
			string? propertyName = GetPropertyNameAtPosition(request.DocumentText, request.HoveredOffset);

			if (string.IsNullOrWhiteSpace(propertyName))
				return null;

			// Try to find schema information for this property.
			string? content = FindHoverInfo(model, propertyName);
			return string.IsNullOrWhiteSpace(content)
				? null
				: new TextHoverInfo(content, TextHoverContentKind.Markdown, propertyName);
		}
		catch (Exception exception)
		{
			s_log.Warn(exception, "Failed to resolve GameFlow hover information.");
			return null;
		}
	}

	private static string? GetPropertyNameAtPosition(string documentText, int offset)
	{
		if (offset < 0 || offset >= documentText.Length)
			return null;

		int lineStart = documentText.LastIndexOf('\n', offset) + 1;
		int lineEnd = documentText.IndexOf('\n', offset);

		if (lineEnd < 0)
			lineEnd = documentText.Length;
		else if (lineEnd > lineStart && documentText[lineEnd - 1] == '\r')
			lineEnd--;

		string lineText = documentText.Substring(lineStart, lineEnd - lineStart);
		int relativeOffset = offset - lineStart;

		// The character immediately after a matched property (the colon) is outside the property
		// range, so hovering the colon yields no information.
		foreach (Match match in s_propertyNamePattern.Matches(lineText))
		{
			if (relativeOffset >= match.Index && relativeOffset < match.Index + match.Length)
				return match.Groups[1].Value;
		}

		return null;
	}

	private static string? FindHoverInfo(TRXGameFlowSchemaModel model, string propertyName)
	{
		var property = model.Properties.FirstOrDefault(candidate => candidate.Name == propertyName);

		if (property is null)
			return null;

		return FormatPropertyInfo(property);
	}

	private static string FormatPropertyInfo(TRXGameFlowProperty property)
	{
		var info = $"`\"{property.Name}\"`";

		if (property.Types.Count > 0)
			info += $"\nType: `{string.Join(", ", property.Types)}`";

		if (!string.IsNullOrEmpty(property.Description))
			info += $"\n\n{property.Description}";

		return info;
	}
}
