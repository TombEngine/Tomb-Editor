using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;

namespace TombEditor.WPF.Views;

public abstract class WindowEx : Window
{
	private readonly NativeWindow _win32Parent = new();
	public IWin32Window Win32Window => _win32Parent;

	protected WindowEx()
		=> _win32Parent.AssignHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
}

public abstract class ViewWindow<T> : WindowEx where T : class, INotifyPropertyChanged
{
	public T? ViewModel
	{
		get => DataContext as T;
		set => DataContext = value;
	}
}
