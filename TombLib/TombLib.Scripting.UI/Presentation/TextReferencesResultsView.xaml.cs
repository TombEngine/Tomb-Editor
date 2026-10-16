using System;
using System.Windows.Controls;
using System.Windows.Input;
using TombLib.Scripting.UI.Presentation;

namespace TombLib.Scripting.UI.Presentation;

public partial class TextReferencesResultsView : UserControl
{
	private readonly Action<TextReferenceListItem>? _activateReference;

	/// <summary>
	/// Initializes a new instance of the <see cref="TextReferencesResultsView"/> class.
	/// </summary>
	/// <param name="viewModel">The references results view model.</param>
	/// <param name="activateReference">The callback invoked when a reference is activated.</param>
	public TextReferencesResultsView(TextReferencesResultsViewModel viewModel, Action<TextReferenceListItem>? activateReference)
	{
		InitializeComponent();
		DataContext = viewModel;
		_activateReference = activateReference;
	}

	private void ReferencesTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		=> ActivateSelectedReference();

	private void ReferencesTree_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key != Key.Enter)
			return;

		ActivateSelectedReference();
		e.Handled = true;
	}

	private void ActivateSelectedReference()
	{
		if (ReferencesTree.SelectedItem is not TextReferenceListItem selectedReference)
			return;

		_activateReference?.Invoke(selectedReference);
	}
}
