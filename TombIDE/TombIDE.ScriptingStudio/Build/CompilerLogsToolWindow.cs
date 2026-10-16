using System.Windows;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.Shared;

namespace TombIDE.ScriptingStudio.Build;

public sealed class CompilerLogsToolWindow : StudioDockPane
{
	private readonly CompilerLogsView _view;
	private readonly CompilerLogsViewModel _viewModel;

	public CompilerLogsToolWindow()
		: base(Strings.Default.CompilerLogs, "CompilerLogs", StudioDockPaneLocation.Bottom, new Size(420, 220))
	{
		_viewModel = new CompilerLogsViewModel();
		_view = new CompilerLogsView
		{
			DataContext = _viewModel
		};
	}

	public override UIElement Content => _view;

	public void UpdateLogs(string text)
		=> _viewModel.UpdateLogs(text);
}
