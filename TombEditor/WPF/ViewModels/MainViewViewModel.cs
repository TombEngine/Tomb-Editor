using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace TombEditor.WPF.ViewModels;

public partial class MainViewViewModel : ObservableObject
{
	private readonly Editor _editor;

	public ICommand ChangeModeCommand { get; }

	public MainViewViewModel(Editor editor)
	{
		_editor = editor;
	}
}
