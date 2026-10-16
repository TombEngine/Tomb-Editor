using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.Core.Text;
using System;
using System.Text.RegularExpressions;
using TombLib.Scripting.ClassicScript.Services;
using Nickelony.IDEKit.AvalonEdit.Documents;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editing;

namespace TombLib.Scripting.ClassicScript.Writers;

/// <summary>
/// Writes ClassicScript language string entries into an open editor.
/// </summary>
public sealed class LanguageStringWriter
{
	private readonly IClassicScriptCommandService _commandService;

	/// <summary>
	/// Initializes a new instance of the <see cref="LanguageStringWriter"/> class.
	/// </summary>
	/// <param name="commandService">The command service used to locate document sections.</param>
	public LanguageStringWriter(IClassicScriptCommandService commandService)
		=> _commandService = commandService;

	/// <summary>
	/// Writes a new level name string for the given level name.
	/// </summary>
	/// <param name="textEditor">The editor to write into.</param>
	/// <param name="levelName">The level name to write.</param>
	public void WriteNewLevelNameString(TextEditorBase textEditor, string levelName)
	{
		if (!StockLevelNameWriter.TryAssignStockLevelNameStringSlot(textEditor, levelName))
			WriteNewNGString(textEditor, levelName);
	}

	/// <summary>
	/// Writes a new NG string at the end of the ExtraNG section.
	/// </summary>
	/// <param name="textEditor">The editor to write into.</param>
	/// <param name="ngString">The NG string value to write.</param>
	/// <returns><c>true</c> when the NG string was written; otherwise, <c>false</c>.</returns>
	public bool WriteNewNGString(TextEditorBase textEditor, string ngString)
	{
		ITextSnapshot source = new TextDocumentSnapshot(textEditor.Document);

		if (!IsNGStringAlreadyDefined(textEditor.Document, ngString))
		{
			int? extrangSectionStartLineNumber = _commandService.FindDocumentLineOfSection(source, "ExtraNG");

			if (extrangSectionStartLineNumber is null)
				return false;

			ITextLine extrangSectionStartLine = source.GetLineByNumber(extrangSectionStartLineNumber.Value);

			for (int i = textEditor.Document.LineCount; i >= extrangSectionStartLine.LineNumber; i--)
			{
				var line = textEditor.Document.GetLineByNumber(i);
				string lineText = textEditor.Document.GetText(line.Offset, line.Length);

				if (Regex.IsMatch(lineText, @"^\d+:"))
				{
					int prevNumber = int.Parse(Regex.Replace(lineText, @"^(\d+):.*$", "$1"));

					textEditor.InsertText(line.EndOffset, $"{Environment.NewLine}{prevNumber + 1}: {ngString}");

					textEditor.ScrollToLine(i + 1);
					return true;
				}
				else if (i == extrangSectionStartLine.LineNumber)
				{
					textEditor.InsertText(line.EndOffset, $"{Environment.NewLine}0: {ngString}");

					textEditor.ScrollToLine(i + 1);
					return true;
				}
			}
		}

		return false;
	}

	private bool IsNGStringAlreadyDefined(TextDocument document, string ngString)
	{
		var source = new TextDocumentSnapshot(document);
		int? extrangSectionStartLineNumber = _commandService.FindDocumentLineOfSection(source, "ExtraNG");

		if (extrangSectionStartLineNumber is null)
			return true;

		for (int i = extrangSectionStartLineNumber.Value + 1; i < document.LineCount; i++)
		{
			var line = document.GetLineByNumber(i);
			string lineText = document.GetText(line.Offset, line.Length);

			if (Regex.IsMatch(lineText, $@"^\d+:\s*{ngString}\s*(;.*)?$"))
				return true;
		}

		return false;
	}
}
