using DarkUI.WPF.CustomControls;
using System.Windows;
using System.Windows.Forms;
using TombEditor.WPF.ViewModels;
using TombEditor.WPF.Views;

namespace TombEditor.WPF.Forms;

public partial class OptionsWindow : ViewWindow<OptionsViewModel>
{
	public OptionsWindow(Editor editor)
	{
		DataContext = new OptionsViewModel(editor.Configuration);
		InitializeComponent();
	}

	private void ColorSchemePreset_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
	{ }

	private void ColorPickerButton_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not ColorPickerButton button)
			return;

		var oldColor = button.SelectedColor;

		using var colorDialog = new ColorDialog
		{
			Color = System.Drawing.Color.FromArgb(oldColor.A, oldColor.R, oldColor.G, oldColor.B),
			FullOpen = true
		};

		DialogResult result = colorDialog.ShowDialog();

		if (result == System.Windows.Forms.DialogResult.OK)
		{
			var pickedColor = colorDialog.Color;
			var newUIColor = System.Windows.Media.Color.FromArgb(pickedColor.A, pickedColor.R, pickedColor.G, pickedColor.B);

			button.SelectedColor = newUIColor;
			ColorSchemePresetSelection.SelectedIndex = -1; // Make Preset Selection show nothing, because we just changed a color
		}
	}
}
