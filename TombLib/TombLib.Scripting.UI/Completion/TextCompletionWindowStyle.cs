using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.UI.Completion;

internal static class TextCompletionWindowStyle
{
	private static readonly Style ItemContainerStyle = CreateItemContainerStyle();

	public static void Apply(CompletionWindow window)
	{
		if (window is null)
			return;

		window.FontFamily = window.TextArea.FontFamily;
		window.FontSize = window.TextArea.FontSize;
		window.CompletionList.HorizontalContentAlignment = HorizontalAlignment.Stretch;
		window.CompletionList.ListBox.HorizontalContentAlignment = HorizontalAlignment.Stretch;
		window.CompletionList.ListBox.BorderThickness = new Thickness(0.0);
		window.CompletionList.ListBox.Focusable = false;
		window.CompletionList.ListBox.IsTabStop = false;
		window.CompletionList.ListBox.FontFamily = window.TextArea.FontFamily;
		window.CompletionList.ListBox.FontSize = window.TextArea.FontSize;
		window.CompletionList.ListBox.ItemTemplate = CreateItemTemplate(TextEditorColorPalette.CompletionDetailForeground);
		window.CompletionList.ListBox.ItemContainerStyle = ItemContainerStyle;
	}

	private static DataTemplate CreateItemTemplate(Brush detailBrush)
	{
		var template = new DataTemplate(typeof(CompletionData));

		var panel = new FrameworkElementFactory(typeof(DockPanel));
		panel.SetValue(DockPanel.LastChildFillProperty, true);

		var icon = new FrameworkElementFactory(typeof(Image));
		icon.SetValue(DockPanel.DockProperty, Dock.Left);
		icon.SetValue(FrameworkElement.WidthProperty, 16.0);
		icon.SetValue(FrameworkElement.HeightProperty, 16.0);
		icon.SetValue(FrameworkElement.MarginProperty, new Thickness(0.0, 0.0, 8.0, 0.0));
		icon.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
		icon.SetBinding(Image.SourceProperty, new Binding(nameof(CompletionData.Image)));

		var detail = new FrameworkElementFactory(typeof(TextBlock));
		detail.SetValue(DockPanel.DockProperty, Dock.Right);
		detail.SetValue(TextBlock.ForegroundProperty, detailBrush);
		detail.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
		detail.SetValue(FrameworkElement.MarginProperty, new Thickness(12.0, 0.0, 0.0, 0.0));
		detail.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
		detail.SetBinding(TextBlock.TextProperty, new Binding(nameof(CompletionData.DisplayDetail)));
		detail.SetBinding(UIElement.VisibilityProperty, new Binding(nameof(CompletionData.DetailVisibility)));

		var label = new FrameworkElementFactory(typeof(TextBlock));
		label.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
		label.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
		label.SetBinding(TextBlock.TextProperty, new Binding(nameof(CompletionData.DisplayText)));

		panel.AppendChild(icon);
		panel.AppendChild(detail);
		panel.AppendChild(label);

		template.VisualTree = panel;
		return template;
	}

	private static Style CreateItemContainerStyle()
	{
		var style = new Style(typeof(ListBoxItem));
		style.Setters.Add(new Setter(UIElement.FocusableProperty, false));
		style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8.0, 3.0, 8.0, 3.0)));
		style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
		style.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));
		return style;
	}
}
