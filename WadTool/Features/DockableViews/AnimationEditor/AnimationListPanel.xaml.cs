#nullable enable

using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
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
        => ViewModel?.SetSelectedAnimations(animationList.SelectedItems.Cast<AnimListItem>());

    private void OnAnimationListClick(object sender, MouseButtonEventArgs e) => ViewModel?.ResetChainedPlaybackStart();
}
