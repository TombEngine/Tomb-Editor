using System;
using System.Windows.Controls;
using System.Windows.Input;
using TombLib.Scripting.UI.Presentation;

namespace TombLib.Scripting.UI.Presentation;

public partial class TextDiagnosticsView : UserControl
{
	private readonly Action<TextDiagnosticListItem>? _activateDiagnostic;

	/// <summary>
	/// Initializes a new instance of the <see cref="TextDiagnosticsView"/> class.
	/// </summary>
	/// <param name="viewModel">The diagnostics view model.</param>
	/// <param name="activateDiagnostic">The callback invoked when a diagnostic is activated.</param>
	public TextDiagnosticsView(TextDiagnosticsViewModel viewModel, Action<TextDiagnosticListItem>? activateDiagnostic)
	{
		InitializeComponent();
		DataContext = viewModel;
		_activateDiagnostic = activateDiagnostic;
	}

	private TextDiagnosticsViewModel ViewModel => (TextDiagnosticsViewModel)DataContext;

	private void DiagnosticsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		=> ActivateSelectedDiagnostic();

	private void DiagnosticsGrid_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key != Key.Enter)
			return;

		ActivateSelectedDiagnostic();
		e.Handled = true;
	}

	private void ActivateSelectedDiagnostic()
	{
		if (ViewModel.SelectedItem is not TextDiagnosticListItem selectedDiagnostic)
			return;

		_activateDiagnostic?.Invoke(selectedDiagnostic);
	}
}
