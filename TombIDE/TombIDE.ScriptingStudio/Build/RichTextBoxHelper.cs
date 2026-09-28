#nullable enable

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace TombIDE.ScriptingStudio.Build;

/// <summary>
/// Provides an attached property that enables binding to <see cref="RichTextBox.Document"/>
/// and automatically scrolls to the end when the document changes.
/// </summary>
public static class RichTextBoxHelper
{
	public static readonly DependencyProperty BindableDocumentProperty =
		DependencyProperty.RegisterAttached(
			"BindableDocument",
			typeof(FlowDocument),
			typeof(RichTextBoxHelper),
			new PropertyMetadata(null, OnBindableDocumentChanged));

	public static FlowDocument GetBindableDocument(RichTextBox richTextBox)
		=> (FlowDocument)richTextBox.GetValue(BindableDocumentProperty);

	public static void SetBindableDocument(RichTextBox richTextBox, FlowDocument value)
		=> richTextBox.SetValue(BindableDocumentProperty, value);

	private static void OnBindableDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not RichTextBox richTextBox || e.NewValue is not FlowDocument document)
			return;

		richTextBox.Document = document;
		richTextBox.ScrollToEnd();
	}
}
