#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows.Documents;
using System.Windows.Media;

namespace TombIDE.ScriptingStudio.Build;

public sealed partial class CompilerLogsViewModel : ObservableObject
{
	[ObservableProperty]
	private FlowDocument _document = CreateDocument(string.Empty);

	public void UpdateLogs(string text)
		=> Document = CreateDocument(text ?? string.Empty);

	private static FlowDocument CreateDocument(string text)
	{
		var document = new FlowDocument
		{
			PagePadding = new System.Windows.Thickness(0),
			TextAlignment = System.Windows.TextAlignment.Left
		};

		string normalizedText = (text ?? string.Empty).Replace("\r\n", "\n");
		string[] lines = normalizedText.Split('\n');

		for (int i = 0; i < lines.Length; i++)
		{
			var paragraph = new Paragraph
			{
				Margin = new System.Windows.Thickness(0),
				FontFamily = new FontFamily("Consolas"),
				FontSize = 12.0
			};

			Brush foreground = Brushes.Gainsboro;

			if (lines[i].Contains("ERROR:", StringComparison.Ordinal))
				foreground = Brushes.Red;
			else if (lines[i].Contains("Completed compilation", StringComparison.Ordinal))
				foreground = Brushes.LightGreen;

			paragraph.Inlines.Add(new Run(lines[i]) { Foreground = foreground });
			document.Blocks.Add(paragraph);
		}

		return document;
	}
}
