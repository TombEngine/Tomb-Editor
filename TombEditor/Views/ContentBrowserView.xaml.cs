#nullable enable

using DarkUI.WPF.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TombEditor.ViewModels;

namespace TombEditor.Views;

public partial class ContentBrowserView : UserControl
{
	private Point _dragStartPoint;
	private bool _isDragging;
	private UIElement? _animatedElement;
	private bool _suppressSelectionChanged;

	// Rubber-band selection state.
	private bool _isRubberBanding;

	private Point _rubberBandOrigin;

	// Cached item bounds (built once at drag-start, avoids per-frame TransformToVisual).
	private List<(AssetItemViewModel Item, Rect Bounds)>? _rubberBandItemBounds;

	// Tracks which items are currently inside the rubber-band rect (enables incremental diff).
	private readonly HashSet<AssetItemViewModel> _rubberBandSelected = new();

	// Click-on-selected guard: prevents deselecting multi-selection on a plain click.
	private bool _clickedOnSelected;

	private AssetItemViewModel? _clickedItem;

	public ContentBrowserView()
	{
		InitializeComponent();
	}

	/// <summary>
	/// Handles selection changes in the ListBox.
	/// Updates the ViewModel's SelectedItems collection for multi-selection support.
	/// </summary>
	private void AssetListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (_suppressSelectionChanged)
			return;

		if (DataContext is ContentBrowserViewModel vm)
		{
			var selectedItems = AssetListBox.SelectedItems.OfType<AssetItemViewModel>().ToList();
			vm.UpdateSelectedItems(selectedItems);
		}
	}

	/// <summary>
	/// Handles double-click on an asset to trigger the Add Item action, or loads a WAD when empty area is double-clicked.
	/// Plays a subtle zoom+fade animation on the tile to confirm the action.
	/// Uses PreviewMouseDoubleClick (tunneling) to catch events even in empty space.
	/// Skipped when Ctrl is held (multi-selection mode).
	/// </summary>
	private void AssetListBox_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton != MouseButton.Left)
			return;

		// Don't trigger placement when Ctrl is held (user is building a multi-selection).
		if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
			return;

		if (DataContext is not ContentBrowserViewModel vm)
			return;

		var hitContainer = (e.OriginalSource as DependencyObject)?.FindVisualAncestorOrSelf<ListBoxItem>();

		if (hitContainer is not null && AssetListBox.SelectedItem is AssetItemViewModel)
		{
			// Double-click on an item: place it.
			PlayAddItemAnimation(hitContainer);
			vm.AddItemCommand.Execute(null);
			e.Handled = true;
		}
		else if (hitContainer is null)
		{
			// Double-click on empty area: load a new WAD file.
			vm.AddWadCommand.Execute(null);
			e.Handled = true;
		}
	}

	/// <summary>
	/// Plays a brief scale-up + fade-out animation on the given element
	/// to provide visual feedback that an item is being placed.
	/// The element stays grayed out until <see cref="RestoreLastAnimation"/> is called.
	/// </summary>
	private void PlayAddItemAnimation(UIElement element)
	{
		// Restore any previous animation before starting a new one.
		RestoreLastAnimation();

		_animatedElement = element;

		var duration = new Duration(TimeSpan.FromSeconds(0.25));

		// Ensure a render transform exists.
		if (element.RenderTransform is not ScaleTransform)
		{
			element.RenderTransform = new ScaleTransform(1, 1);
			element.RenderTransformOrigin = new Point(0.5, 0.5);
		}

		var scaleX = new DoubleAnimation(1.0, 1.08, duration) { EasingFunction = new QuadraticEase() };
		var scaleY = new DoubleAnimation(1.0, 1.08, duration) { EasingFunction = new QuadraticEase() };
		var fade = new DoubleAnimation(1.0, 0.4, duration) { EasingFunction = new QuadraticEase() };

		element.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
		element.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleY);
		element.BeginAnimation(OpacityProperty, fade);
	}

	/// <summary>
	/// Restores the last animated tile to its normal appearance by clearing
	/// all running/held animations and resetting opacity and transform.
	/// Called when EditorActionPlace finishes or is canceled.
	/// </summary>
	public void RestoreLastAnimation()
	{
		if (_animatedElement is null)
			return;

		// Remove held animations so local values take effect again.
		_animatedElement.BeginAnimation(OpacityProperty, null);

		if (_animatedElement.RenderTransform is ScaleTransform)
		{
			_animatedElement.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
			_animatedElement.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
		}

		_animatedElement.Opacity = 1.0;
		_animatedElement.RenderTransform = new ScaleTransform(1, 1);
		_animatedElement = null;
	}

	/// <summary>
	/// Records the mouse position when the left button is pressed for drag-drop detection.
	/// When Alt is held, performs a "Locate Item" operation without changing selection.
	/// When clicking on an already-selected item with multiple items selected and no modifiers,
	/// suppresses deselection of the other items.
	/// When clicking on empty space, initiates rubber-band selection.
	/// </summary>
	private void AssetListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		// Don't start drag when clicking on the scrollbar.
		if (IsOverScrollbar(e))
			return;

		_clickedOnSelected = false;
		_clickedItem = null;
		_isRubberBanding = false;
		SelectionRect.Visibility = Visibility.Collapsed;

		// Alt+click: locate item without selecting it.
		if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
		{
			if (DataContext is ContentBrowserViewModel vm && e.OriginalSource is DependencyObject source)
			{
				var container = source.FindVisualAncestorOrSelf<ListBoxItem>();

				if (container?.DataContext is AssetItemViewModel item)
				{
					vm.RequestLocateItem(item);
					e.Handled = true;
					return;
				}
			}
		}

		_dragStartPoint = e.GetPosition(null);
		_isDragging = false;

		var hitContainer = (e.OriginalSource as DependencyObject)?.FindVisualAncestorOrSelf<ListBoxItem>();

		if (hitContainer?.DataContext is AssetItemViewModel hitItem &&
			AssetListBox.SelectedItems.Contains(hitItem) &&
			AssetListBox.SelectedItems.Count > 1 &&
			!Keyboard.Modifiers.HasFlag(ModifierKeys.Control) &&
			!Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
		{
			// Prevent the ListBox from deselecting the multi-selection on mouse-down.
			// If the user just clicks (no drag), we deselect in PreviewMouseLeftButtonUp.
			_clickedOnSelected = true;
			_clickedItem = hitItem;
			e.Handled = true;
		}
		else if (hitContainer is null && !Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
		{
			// Click on empty space → start rubber-band selection.
			// Clear any stale visual flags from a previously-canceled drag.
			foreach (var item in _rubberBandSelected)
				item.IsRubberBandSelected = false;

			// Cache all item bounds now (one TransformToVisual call per item, not per frame).
			_rubberBandOrigin = e.GetPosition(RubberBandCanvas);
			_rubberBandSelected.Clear();
			_rubberBandItemBounds = new List<(AssetItemViewModel, Rect)>();

			foreach (var assetItem in AssetListBox.Items.Cast<AssetItemViewModel>())
			{
				var c = AssetListBox.ItemContainerGenerator.ContainerFromItem(assetItem) as ListBoxItem;

				if (c is null)
					continue;

				var origin = c.TransformToVisual(RubberBandCanvas).Transform(new Point(0, 0));
				_rubberBandItemBounds.Add((assetItem, new Rect(origin, new Size(c.ActualWidth, c.ActualHeight))));
			}

			_isRubberBanding = true;
			e.Handled = true;
		}
	}

	/// <summary>
	/// Finalizes the rubber-band selection when the left mouse button is released.
	/// </summary>
	private void AssetListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (_isRubberBanding)
		{
			bool wasClicked = SelectionRect.Visibility == Visibility.Collapsed;

			// Clear IsRubberBandSelected visual flags from all in-progress items.
			foreach (var item in _rubberBandSelected)
				item.IsRubberBandSelected = false;

			_isRubberBanding = false;
			SelectionRect.Visibility = Visibility.Collapsed;
			_rubberBandItemBounds = null;

			if (DataContext is ContentBrowserViewModel vm)
			{
				if (wasClicked)
				{
					// Empty-space click with no drag: deselect all.
					_rubberBandSelected.Clear();
					_suppressSelectionChanged = true;
					AssetListBox.UnselectAll();
					_suppressSelectionChanged = false;
					vm.UpdateSelectedItems(Array.Empty<AssetItemViewModel>());
				}
				else
				{
					// Commit the rubber-band set to the ListBox in one atomic batch.
					// suppressing SelectionChanged so UpdateSelectedItems is called exactly once.
					var selected = _rubberBandSelected.ToList();
					_rubberBandSelected.Clear();
					_suppressSelectionChanged = true;
					AssetListBox.UnselectAll();

					foreach (var item in selected)
						AssetListBox.SelectedItems.Add(item);

					_suppressSelectionChanged = false;
					vm.UpdateSelectedItems(selected);
				}
			}
			else
			{
				_rubberBandSelected.Clear();
			}

			e.Handled = true;
			return;
		}

		// Multi-selection click guard: user clicked on a selected item without dragging.
		// Now deselect the others and keep only the clicked item.
		if (_clickedOnSelected && _clickedItem is not null)
		{
			_clickedOnSelected = false;
			AssetListBox.SelectedItem = _clickedItem;
			_clickedItem = null;
		}
	}

	/// <summary>
	/// Cancels rubber-band selection when the mouse leaves the ListBox.
	/// </summary>
	private void AssetListBox_MouseLeave(object sender, MouseEventArgs e)
	{
		if (_isRubberBanding)
		{
			// Cancel rubber-band: clear visual flags and state.
			foreach (var item in _rubberBandSelected)
				item.IsRubberBandSelected = false;

			_isRubberBanding = false;
			SelectionRect.Visibility = Visibility.Collapsed;
			_rubberBandItemBounds = null;
			_rubberBandSelected.Clear();
		}
	}

	/// <summary>
	/// Initiates a drag-drop operation by delegating to the WinForms host.
	/// WPF's DragDrop.DoDragDrop uses COM OLE that doesn't interop cleanly
	/// with WinForms drop targets. Instead, we raise DragDropRequested on the
	/// ViewModel, and the WinForms ContentBrowser host calls Control.DoDragDrop.
	/// </summary>
	private void AssetListBox_PreviewMouseMove(object sender, MouseEventArgs e)
	{
		if (e.LeftButton != MouseButtonState.Pressed)
		{
			_isDragging = false;
			return;
		}

		// Update rubber-band rect and do an incremental selection diff against pre-cached bounds.
		if (_isRubberBanding)
		{
			var cur = e.GetPosition(RubberBandCanvas);
			var x = Math.Min(cur.X, _rubberBandOrigin.X);
			var y = Math.Min(cur.Y, _rubberBandOrigin.Y);
			var w = Math.Abs(cur.X - _rubberBandOrigin.X);
			var h = Math.Abs(cur.Y - _rubberBandOrigin.Y);

			Canvas.SetLeft(SelectionRect, x);
			Canvas.SetTop(SelectionRect, y);
			SelectionRect.Width = Math.Max(w, 0);
			SelectionRect.Height = Math.Max(h, 0);
			SelectionRect.Visibility = Visibility.Visible;

			if ((w > 1 || h > 1) && _rubberBandItemBounds is not null)
			{
				var rubberRect = new Rect(x, y, w, h);

				// Update IsRubberBandSelected on each item for live visual feedback.
				// Never modifies AssetListBox.SelectedItems during the drag - this avoids the SelectedItem binding side-effect and the ChosenItem/SyncSelectionFromEditor.
				// feedback loop. The real selection is committed atomically in PreviewMouseLeftButtonUp.

				foreach (var (item, bounds) in _rubberBandItemBounds)
				{
					bool intersects = rubberRect.IntersectsWith(bounds);
					bool wasSelected = _rubberBandSelected.Contains(item);

					if (intersects && !wasSelected)
					{
						_rubberBandSelected.Add(item);
						item.IsRubberBandSelected = true;
					}
					else if (!intersects && wasSelected)
					{
						_rubberBandSelected.Remove(item);
						item.IsRubberBandSelected = false;
					}
				}
			}

			e.Handled = true;
			return;
		}

		// Don't drag when interacting with the scrollbar.
		if (IsOverScrollbar(e))
			return;

		var currentPos = e.GetPosition(null);
		var diff = _dragStartPoint - currentPos;

		// Check if the movement exceeds the system drag threshold.
		if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
			Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
			return;

		if (_isDragging)
			return;

		// Find the asset item under the cursor.
		if (AssetListBox.SelectedItems.Count > 0 &&
			DataContext is ContentBrowserViewModel vm)
		{
			_isDragging = true;

			var selectedItems = AssetListBox.SelectedItems.Cast<AssetItemViewModel>().ToList();
			vm.RequestDragDrop(selectedItems); // Delegate drag-drop to WinForms host via ViewModel event.

			_isDragging = false;
		}
	}

	// Handles keyboard navigation in the asset grid. Arrow keys move selection, Home/End jump to first/last item, and Enter confirms.
	private void AssetListBox_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Escape)
		{
			// Escape clears the visual selection without touching ChosenItem/ChosenImportedGeometry.

			_suppressSelectionChanged = true;
			AssetListBox.UnselectAll();
			_suppressSelectionChanged = false;

			if (DataContext is ContentBrowserViewModel vm)
				vm.UpdateSelectedItems(Array.Empty<AssetItemViewModel>());

			e.Handled = true;
			return;
		}

		int itemCount = AssetListBox.Items.Count;

		if (itemCount == 0)
			return;

		int currentIndex = AssetListBox.SelectedItem is AssetItemViewModel sel
			? AssetListBox.Items.IndexOf(sel) : -1;

		int newIndex;

		switch (e.Key)
		{
			case Key.Right:
				newIndex = Math.Min(currentIndex + 1, itemCount - 1);
				e.Handled = true;
				break;

			case Key.Left:
				newIndex = Math.Max(currentIndex - 1, 0);
				e.Handled = true;
				break;

			case Key.Down:
				{
					int columns = EstimateColumnsInRow();
					newIndex = Math.Min(currentIndex + columns, itemCount - 1);
					e.Handled = true;
				}

				break;

			case Key.Up:
				{
					int columns = EstimateColumnsInRow();
					newIndex = Math.Max(currentIndex - columns, 0);
					e.Handled = true;
				}

				break;

			case Key.Home:
				newIndex = 0;
				e.Handled = true;
				break;

			case Key.End:
				newIndex = itemCount - 1;
				e.Handled = true;
				break;

			case Key.PageDown:
				{
					int columns = EstimateColumnsInRow();
					int pageItems = columns * 4;
					newIndex = Math.Min(currentIndex + pageItems, itemCount - 1);
					e.Handled = true;
				}

				break;

			case Key.PageUp:
				{
					int columns = EstimateColumnsInRow();
					int pageItems = columns * 4;
					newIndex = Math.Max(currentIndex - pageItems, 0);
					e.Handled = true;
				}

				break;

			default:
				return;
		}

		if (newIndex >= 0 && newIndex < itemCount && newIndex != currentIndex)
		{
			AssetListBox.SelectedItem = AssetListBox.Items[newIndex];
			AssetListBox.ScrollIntoView(AssetListBox.SelectedItem);
		}
	}

	/// <summary>
	/// Estimates how many tile columns fit in the current ListBox width.
	/// </summary>
	private int EstimateColumnsInRow()
	{
		double tileWidth = 92; // Default tile width + margin.

		if (DataContext is ContentBrowserViewModel vm)
			tileWidth = vm.TileWidth + 4; // 2px margin on each side.

		double availableWidth = AssetListBox.ActualWidth - 20; // Account for scrollbar.
		return Math.Max(1, (int)(availableWidth / tileWidth));
	}

	/// <summary>
	/// When Ctrl or Alt is held, intercepts mouse wheel to zoom (change tile size)
	/// instead of scrolling.
	/// </summary>
	private void AssetListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) || Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
		{
			if (DataContext is ContentBrowserViewModel vm)
			{
				const double step = 8;
				double newWidth = vm.TileWidth + (e.Delta > 0 ? step : -step);
				vm.TileWidth = Math.Clamp(newWidth, vm.MinTileWidth, vm.MaxTileWidth);
			}

			e.Handled = true;
		}
	}

	/// <summary>
	/// Returns true if the mouse event originated over a ScrollBar or its children
	/// (Thumb, RepeatButton, Track, etc.), so drag-drop should not be initiated.
	/// </summary>
	private static bool IsOverScrollbar(MouseEventArgs e)
	{
		return e.OriginalSource is DependencyObject source
			&& (source is ScrollBar || source.FindVisualAncestor<ScrollBar>() is not null);
	}

	/// <summary>
	/// Programmatically sets the ListBox selection to a single item,
	/// suppressing the SelectionChanged event to avoid feedback loops.
	/// </summary>
	public void SetSelectionSilently(AssetItemViewModel item)
	{
		_suppressSelectionChanged = true;

		AssetListBox.SelectedItems.Clear();

		if (item is not null)
			AssetListBox.SelectedItem = item;

		_suppressSelectionChanged = false;
	}

	/// <summary>
	/// Scrolls the ListBox to ensure the specified item is visible.
	/// </summary>
	public void ScrollToItem(AssetItemViewModel item)
	{
		if (item is null)
			return;

		// ScrollIntoView doesn't work reliably with WrapPanel, so use BringIntoView on the container directly.
		var container = AssetListBox.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
		container?.BringIntoView();
	}

	private void ContentBrowser_DragEnter(object sender, DragEventArgs e)
	{
		if (e.Data.GetDataPresent(DataFormats.FileDrop))
			e.Effects = DragDropEffects.Copy;
		else
			e.Effects = DragDropEffects.None;

		e.Handled = true;
	}

	private void ContentBrowser_Drop(object sender, DragEventArgs e)
	{
		if (e.Data.GetDataPresent(DataFormats.FileDrop))
		{
			var files = e.Data.GetData(DataFormats.FileDrop) as string[];

			if (files?.Length > 0 && DataContext is ContentBrowserViewModel vm)
				vm.HandleFileDrop(files);
		}

		e.Handled = true;
	}

	private void FavoriteStar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (sender is FrameworkElement element &&
			element.DataContext is AssetItemViewModel item &&
			DataContext is ContentBrowserViewModel vm)
		{
			vm.ToggleFavorite(item);
			e.Handled = true;
		}
	}

	// Event raised when the viewport scrolls; host uses this to render visible thumbnails.
	public event EventHandler? ViewportScrolled;

	private void AssetListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
	{
		ViewportScrolled?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Returns the list of AssetItemViewModels whose containers are currently visible in the viewport.
	/// </summary>
	public List<AssetItemViewModel> GetVisibleItems()
	{
		var result = new List<AssetItemViewModel>();
		var scrollViewer = FindVisualChild<ScrollViewer>(AssetListBox);

		if (scrollViewer is null)
			return result;

		var viewportRect = new Rect(0, 0, scrollViewer.ViewportWidth, scrollViewer.ViewportHeight);

		foreach (var item in AssetListBox.Items)
		{
			var container = AssetListBox.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;

			if (container is null)
				continue;

			var transform = container.TransformToAncestor(scrollViewer);
			var itemRect = transform.TransformBounds(new Rect(0, 0, container.ActualWidth, container.ActualHeight));

			if (viewportRect.IntersectsWith(itemRect) && item is AssetItemViewModel vm)
				result.Add(vm);
		}

		return result;
	}

	private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
	{
		int count = VisualTreeHelper.GetChildrenCount(parent);

		for (int i = 0; i < count; i++)
		{
			var child = VisualTreeHelper.GetChild(parent, i);

			if (child is T result)
				return result;

			var descendant = FindVisualChild<T>(child);

			if (descendant is not null)
				return descendant;
		}

		return null;
	}
}
