#nullable enable

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace TombIDE.ScriptingStudio.Settings;

public partial class ScriptingSettingsWindow : Window
{
	public ScriptingSettingsWindow()
	{
		InitializeComponent();
		DataContextChanged += ScriptingSettingsWindow_DataContextChanged;
		Loaded += ScriptingSettingsWindow_Loaded;
	}

	private void ScriptingSettingsWindow_Loaded(object sender, RoutedEventArgs e)
		=> (DataContext as ScriptingSettingsWindowViewModel)?.RefreshSelectedPagePreview();

	private void ScriptingSettingsWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (e.OldValue is ScriptingSettingsWindowViewModel oldViewModel)
			oldViewModel.PropertyChanged -= ViewModel_PropertyChanged;

		if (e.NewValue is ScriptingSettingsWindowViewModel newViewModel)
			newViewModel.PropertyChanged += ViewModel_PropertyChanged;
	}

	private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName != nameof(ScriptingSettingsWindowViewModel.DialogResult))
			return;

		if (DataContext is not ScriptingSettingsWindowViewModel viewModel || !viewModel.DialogResult.HasValue)
			return;

		DialogResult = viewModel.DialogResult;
	}

	private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!ReferenceEquals(sender, e.OriginalSource))
			return;

		(DataContext as ScriptingSettingsWindowViewModel)?.RefreshSelectedPagePreview();
	}

	private void ResetCurrentPageButton_Click(object sender, RoutedEventArgs e)
	{
		if (DataContext is not ScriptingSettingsWindowViewModel viewModel || !viewModel.ResetCurrentPageCommand.CanExecute(null))
			return;

		viewModel.ResetCurrentPageCommand.Execute(null);
	}

	private void CancelButton_Click(object sender, RoutedEventArgs e)
	{
		if (DataContext is not ScriptingSettingsWindowViewModel viewModel || !viewModel.CancelCommand.CanExecute(null))
			return;

		viewModel.CancelCommand.Execute(null);
	}

	private void SaveButton_Click(object sender, RoutedEventArgs e)
	{
		if (DataContext is not ScriptingSettingsWindowViewModel viewModel || !viewModel.SaveCommand.CanExecute(null))
			return;

		viewModel.SaveCommand.Execute(null);
	}
}
