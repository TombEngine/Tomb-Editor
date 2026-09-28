#nullable enable

using System.Windows.Controls;

namespace TombIDE.ScriptingStudio.ClassicScript;

public partial class ReferenceBrowserView : UserControl
{
	public ReferenceBrowserView()
		=> InitializeComponent();

	private ReferenceBrowserViewModel? ViewModel => DataContext as ReferenceBrowserViewModel;

	private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
	{
		if (e.PropertyName == "_RowString")
		{
			e.Cancel = true;
			return;
		}

		e.Column.Width = new System.Windows.Controls.DataGridLength(1.0, System.Windows.Controls.DataGridLengthUnitType.Star);

		if (ViewModel is not null)
			e.Column.Header = ViewModel.GetColumnHeader(e.PropertyName);
	}
}
