#nullable enable

using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace TombIDE.ScriptingStudio.FindAndReplace;

public partial class SearchResultsView : UserControl
{
	public SearchResultsView()
	{
		InitializeComponent();
	}

	public event EventHandler<SearchResultsItemViewModel>? ResultInvoked;

	private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (TreeView.SelectedItem is not SearchResultsItemViewModel item)
			return;

		ResultInvoked?.Invoke(this, item);
		e.Handled = true;
	}
}
