using System.Windows;
using TombLib.Wad;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.ReplaceObject;

public partial class ReplaceObjectWindow : Window
{
	public ReplaceObjectWindow()
	{
		InitializeComponent();
		this.HookModalAutoClose();
		Loaded += (_, _) => WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormReplaceObject");
		Closed += (_, _) =>
		{
			if (DataContext is ReplaceObjectWindowViewModel vm)
				vm.Dispose();
		};
	}

	private ReplaceObjectWindowViewModel? Vm => DataContext as ReplaceObjectWindowViewModel;

	private void OnDragEnter(object sender, DragEventArgs e)
	{
		e.Effects = TryGetWadObject(e) is not null ? DragDropEffects.Copy : DragDropEffects.None;
		e.Handled = true;
	}

	private void OnDrop(object sender, DragEventArgs e)
	{
		var item = TryGetWadObject(e);
		if (item is null || Vm is null)
			return;

		var dropPos = e.GetPosition(this);
		var sourceTop = tbSource.TransformToAncestor(this).Transform(new Point(0, 0)).Y;
		var destTop = tbDest.TransformToAncestor(this).Transform(new Point(0, 0)).Y;
		var midpoint = (sourceTop + destTop) / 2;

		Vm.ToggleWadObject(item, dropPos.Y >= midpoint
			? ReplaceObjectWindowViewModel.ObjectSelectionType.Destination
			: ReplaceObjectWindowViewModel.ObjectSelectionType.Source);

		e.Handled = true;
	}

	private static IWadObject? TryGetWadObject(DragEventArgs e)
	{
		var formats = e.Data.GetFormats();
		if (formats.Length == 0)
			return null;
		return e.Data.GetData(formats[0]) as IWadObject;
	}
}
