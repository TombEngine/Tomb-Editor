using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows;
using System.Windows.Media;
using TombLib.Scripting.ClassicScript.Services;

namespace TombLib.Scripting.ClassicScript.Highlighting;

/// <summary>
/// Renders section separator lines in the ClassicScript editor text view.
/// </summary>
public sealed class SectionRenderer : IBackgroundRenderer
{
	private static readonly Pen DefaultSectionBorderPen = CreateFrozenPen(Colors.Silver);

	private readonly ClassicScriptEditor _editor;
	private readonly IClassicScriptLineService _lineService;
	private Pen _sectionBorderPen = DefaultSectionBorderPen;

	// Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="SectionRenderer"/> class.
	/// </summary>
	/// <param name="editor">The editor the renderer belongs to.</param>
	/// <param name="lineService">The line service used to identify section header lines.</param>
	public SectionRenderer(ClassicScriptEditor editor, IClassicScriptLineService lineService)
	{
		_editor = editor;
		_lineService = lineService;
	}

	/// <summary>
	/// Gets the layer in which the section lines are drawn.
	/// </summary>
	public KnownLayer Layer => KnownLayer.Caret;

	// Scheme

	/// <summary>
	/// Updates the section border color from the editor's color scheme.
	/// </summary>
	public void UpdateSectionColor(string htmlColor)
	{
		if (ColorConverter.ConvertFromString(htmlColor) is Color color)
			_sectionBorderPen = CreateFrozenPen(color);
	}

	private static Pen CreateFrozenPen(Color color)
	{
		var brush = new SolidColorBrush(color);
		brush.Freeze();

		var pen = new Pen(brush, 0.5);
		pen.Freeze();
		return pen;
	}

	// Drawing

	/// <summary>
	/// Draws section separator lines under visible section headers.
	/// </summary>
	/// <param name="textView">The text view to draw in.</param>
	/// <param name="drawingContext">The drawing context.</param>
	public void Draw(TextView textView, DrawingContext drawingContext)
	{
		// Only visible lines can be on screen, so scanning them keeps the per-frame cost bounded.
		foreach (VisualLine visualLine in textView.VisualLines)
		{
			DocumentLine line = visualLine.FirstDocumentLine;
			string lineText = _editor.Document.GetText(line.Offset, line.Length);

			if (!_lineService.IsSectionHeaderLine(lineText))
				continue;

			var segment = new TextSegment { StartOffset = line.Offset, EndOffset = line.EndOffset };

			foreach (Rect rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment, true))
				drawingContext.DrawLine(_sectionBorderPen, new Point(rect.Location.X, rect.Location.Y), new Point(textView.ActualWidth, rect.Location.Y));
		}
	}
}
