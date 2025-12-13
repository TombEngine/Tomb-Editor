#nullable enable

using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Windows.Media;
using TombLib.Scripting.Tomb1Main.Enums;

namespace TombLib.Scripting.Tomb1Main.Objects;

public class TRXCompletionData : ICompletionData
{
	private static readonly ImageSource? _arrayImage = new ImageSourceConverter()
		.ConvertFromString("pack://application:,,,/TombLib.Scripting.Tomb1Main;component/Resources/Icons/Array.png") as ImageSource;

	private static readonly ImageSource? _constantImage = new ImageSourceConverter()
		.ConvertFromString("pack://application:,,,/TombLib.Scripting.Tomb1Main;component/Resources/Icons/Constant.png") as ImageSource;

	private static readonly ImageSource? _propertyImage = new ImageSourceConverter()
		.ConvertFromString("pack://application:,,,/TombLib.Scripting.Tomb1Main;component/Resources/Icons/Property.png") as ImageSource;

	public TRXCompletionData(string text, CompletionType type = CompletionType.Generic, string? description = null)
	{
		Text = text;
		Description = description;

		Image = type switch
		{
			CompletionType.Array => _arrayImage,
			CompletionType.Property => _propertyImage,
			CompletionType.Constant => _constantImage,
			_ => null
		};
	}

	public ImageSource? Image { get; }
	public string Text { get; }
	public object Content => Text;
	public object? Description { get; }
	public double Priority => 0;

	public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
		=> textArea.Document.Replace(completionSegment, Text);
}
