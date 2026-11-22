#nullable enable

using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WadTool.Features.Dialogs.AnimEditor;

namespace WadTool.Features.DockableViews.AnimationEditor;

/// <summary>Dockable animation list with filter, add and delete buttons.</summary>
public partial class AnimationListPanel : UserControl
{
    public AnimationListPanel()
    {
        InitializeComponent();

        animationList.SelectionChanged += OnSelectionChanged;
    }

    private AnimationEditorWindowViewModel? ViewModel => DataContext as AnimationEditorWindowViewModel;

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel?.SetSelectedAnimations(animationList.SelectedItems.Cast<AnimListItem>());

        // Added or pasted animations are selected from code and must be brought into view.
        if (e.AddedItems.Count > 0)
        {
            var added = e.AddedItems[e.AddedItems.Count - 1];
            Dispatcher.BeginInvoke(() => animationList.ScrollIntoView(added), DispatcherPriority.Loaded);
        }
    }

    private void OnAnimationListClick(object sender, MouseButtonEventArgs e) => ViewModel?.ResetChainedPlaybackStart();
}
