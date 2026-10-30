#nullable enable

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Utils;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    public partial class EventSetEditorWindow : Window
    {
        private EventSetEditorWindowViewModel? Vm => DataContext as EventSetEditorWindowViewModel;

        // Drag-drop state for the event-set tree.
        private Point _dragStart;
        private SetTreeNode? _dragCandidate;

        public EventSetEditorWindow()
        {
            InitializeComponent();
            // This window is shown modeless, so the hook falls back to Close() when the
            // view-model sets DialogResult (OK/Cancel buttons, level switch).
            this.HookModalAutoClose();
            Loaded += OnLoaded;
            Closed += OnClosed;
        }

        private readonly PopUpInfo _popup = new();

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var configuration = Editor.Instance.Configuration;
            WindowConfiguration.ConfigureWindow(this, configuration, "FormEventSetEditor");

            // Same list width as the previous splitter distance (restored after the window size).
            listColumn.Width = new GridLength(Math.Max(configuration.Window_FormEventSetEditor_SplitterDistance, listColumn.MinWidth));

            if (Vm is { } vm)
                vm.WarningRequested += ShowWarning;
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            if (Vm is { } vm)
                vm.WarningRequested -= ShowWarning;

            _popup.Dispose();
            Vm?.OnWindowClosed();
        }

        private void ShowWarning(string message)
            => _popup.Show(editorPanel, PopupAlignment.BottomRight, message, string.Empty, PopupType.Warning);

        private void Splitter_DragCompleted(object sender, DragCompletedEventArgs e)
            => Editor.Instance.Configuration.Window_FormEventSetEditor_SplitterDistance = (int)Math.Round(listColumn.ActualWidth);

        private void SetsTree_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is not (Key.Delete or Key.Back) || Vm is not { } vm)
                return;

            if (vm.DeleteSetCommand.CanExecute(null))
                vm.DeleteSetCommand.Execute(null);

            e.Handled = true;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            searchPopup.IsOpen = true;
            searchBox.Focus();
            searchBox.SelectAll();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
            => Vm?.FindSet(searchBox.Text, false);

        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    Vm?.FindSet(searchBox.Text, true);
                    e.Handled = true;
                    break;

                case Key.Escape:
                    searchPopup.IsOpen = false;
                    e.Handled = true;
                    break;
            }
        }

        private void SetsTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (Vm is { } vm)
                vm.SelectedNode = e.NewValue as SetTreeNode;
        }

        private void SetsTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Double-click renames folders (mirrors the previous editor). The expansion toggle
            // the double-click also caused is undone so the rename does not visually jump.
            if (NodeFromPoint(e.OriginalSource) is { IsFolder: true } node && Vm is { } vm)
            {
                node.IsExpanded = !node.IsExpanded;
                vm.RenameFolder(node);
            }
        }

        private void SetsTree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStart = e.GetPosition(setsTree);
            _dragCandidate = NodeFromPoint(e.OriginalSource);
        }

        private void SetsTree_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _dragCandidate is null)
                return;

            var delta = e.GetPosition(setsTree) - _dragStart;
            if (Math.Abs(delta.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(delta.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;

            var node = _dragCandidate;
            _dragCandidate = null;
            DragDrop.DoDragDrop(setsTree, new DataObject(typeof(SetTreeNode), node), DragDropEffects.Move);
        }

        private void SetsTree_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;

            if (e.Data.GetData(typeof(SetTreeNode)) is not SetTreeNode dragged)
                return;

            // Only folders (or the empty area = root) accept drops.
            var target = NodeFromPoint(e.OriginalSource);
            if (target is null || (target.IsFolder && target != dragged && !target.IsDescendantOf(dragged)))
                e.Effects = DragDropEffects.Move;
        }

        private void SetsTree_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(SetTreeNode)) is not SetTreeNode dragged || Vm is not { } vm)
                return;

            var target = NodeFromPoint(e.OriginalSource);
            if (target is not null && !target.IsFolder)
                return;

            vm.MoveNode(dragged, target);
            e.Handled = true;
        }

        private static SetTreeNode? NodeFromPoint(object originalSource)
        {
            for (var element = originalSource as DependencyObject; element is not null; element = VisualTreeHelper.GetParent(element))
            {
                if (element is TreeViewItem item)
                    return item.DataContext as SetTreeNode;
            }
            return null;
        }
    }
}
