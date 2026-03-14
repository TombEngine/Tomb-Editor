using System.ComponentModel;
using System.Windows.Controls;

namespace TombEditor.WPF.Views;

public abstract class ViewUserControl<T> : UserControl where T : class, INotifyPropertyChanged
{
	public T? ViewModel
	{
		get => DataContext as T;
		set => DataContext = value;
	}
}
