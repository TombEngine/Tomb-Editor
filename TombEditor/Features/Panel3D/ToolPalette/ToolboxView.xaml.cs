using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace TombEditor.Features.Panel3D.ToolPalette;

public partial class ToolboxView : UserControl
{
	private DispatcherTimer _contextMenuTimer;

	// Parent WinForms control reference for context menu hosting.
	private System.Windows.Forms.Control _winFormsHost;

	// Reference to the ViewModel for cleanup purposes.
	private ToolboxViewModel _viewModel;

	// Fires when the preferred height of the visible content changes.
	public event Action<int> PreferredHeightChanged;

	// Fires when the preferred width of the visible content changes.
	public event Action<int> PreferredWidthChanged;

	public ToolboxView()
	{
		InitializeComponent();
		Loaded += OnLoaded;

		if (!DesignerProperties.GetIsInDesignMode(this))
		{
			_viewModel = new ToolboxViewModel();
			DataContext = _viewModel;

			_viewModel.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == nameof(ToolboxViewModel.CurrentMode))
					RequestHeightUpdate();
			};

			Unloaded += (_, _) => _viewModel.Cleanup();
		}

		IsVisibleChanged += (_, _) => RequestHeightUpdate();

		_contextMenuTimer = new()
		{
			Interval = TimeSpan.FromMilliseconds(300)
		};

		_contextMenuTimer.Tick += OnContextMenuTimerTick;
	}

	// Sets the parent WinForms control for context menu hosting.
	public void SetWinFormsHost(System.Windows.Forms.Control host)
	{
		_winFormsHost = host;
	}

	/// <summary>
	/// Cleans up the ViewModel, unsubscribing from Editor events.
	/// Safe to call multiple times.
	/// </summary>
	public void Cleanup()
	{
		_contextMenuTimer?.Stop();
		_viewModel?.Cleanup();
	}

	public Orientation PanelOrientation
	{
		get => toolPanel.Orientation;
		set
		{
			if (toolPanel.Orientation == value)
				return;

			toolPanel.Orientation = value;
			UpdateSeparatorOrientation();
			RequestWidthUpdate();
		}
	}

	private void UpdateSeparatorOrientation()
	{
		bool vertical = toolPanel.Orientation == Orientation.Vertical;

		foreach (var child in LogicalTreeHelper.GetChildren(toolPanel))
		{
			if (child is Border border && border.Style == (Style)FindResource("ToolSeparator"))
			{
				if (vertical)
				{
					border.Width = double.NaN;
					border.Height = 1;
				}
				else
				{
					border.Width = 1;
					border.Height = double.NaN;
				}
			}
		}
	}

	#region Layout Measurement

	// Schedules a deferred preferred height recalculation.
	public void RequestHeightUpdate()
	{
		Dispatcher.BeginInvoke(new Action(NotifyPreferredHeightChanged), DispatcherPriority.Render);
	}

	// Schedules a deferred preferred width recalculation (vertical orientation only).
	public void RequestWidthUpdate()
	{
		if (toolPanel.Orientation == Orientation.Vertical)
			Dispatcher.BeginInvoke(new Action(NotifyPreferredWidthChanged), DispatcherPriority.Render);
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		RequestWidthUpdate();
	}

	private void NotifyPreferredWidthChanged()
	{
		if (Visibility != Visibility.Visible)
		{
			PreferredWidthChanged?.Invoke(0);
			return;
		}

		Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

		double dpiScale = GetDpiScale();
		int width = Math.Max(1, (int)Math.Ceiling(DesiredSize.Width * dpiScale));

		PreferredWidthChanged?.Invoke(width);
	}

	private void NotifyPreferredHeightChanged()
	{
		if (Visibility != Visibility.Visible)
		{
			PreferredHeightChanged?.Invoke(0);
			return;
		}

		double availableWidth = ActualWidth > 0 ? ActualWidth : 9999;
		Measure(new Size(availableWidth, double.PositiveInfinity));

		double dpiScale = GetDpiScale();
		int height = Math.Max(1, (int)Math.Ceiling(DesiredSize.Height * dpiScale));

		PreferredHeightChanged?.Invoke(height);
	}

	private double GetDpiScale()
	{
		var source = PresentationSource.FromVisual(this);
		return source?.CompositionTarget?.TransformToDevice.M22 ?? 1.0;
	}

	#endregion Layout Measurement

	#region Grid Paint

	private void OnGridPaintMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.RightButton == MouseButtonState.Pressed)
			ShowGridPaintContextMenu();
		else
			_contextMenuTimer.Start();
	}

	private void OnGridPaintMouseUp(object sender, MouseButtonEventArgs e)
	{
		_contextMenuTimer.Stop();
	}

	private void OnContextMenuTimerTick(object sender, EventArgs e)
	{
		_contextMenuTimer.Stop();
		ShowGridPaintContextMenu();
	}

	private void ShowGridPaintContextMenu()
	{
		var editor = Editor.Instance;
		var owner = _winFormsHost as System.Windows.Forms.IWin32Window;
		var menu = new Controls.ContextMenus.GridPaintContextMenu(editor, owner);
		menu.Show(System.Windows.Forms.Cursor.Position);
	}

	#endregion Grid Paint
}
