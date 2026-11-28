#nullable enable

using System;
using System.Windows.Controls;
using TombLib.Wad;
using TombLib.WPF.CustomControls;
using WadTool.Features.Dialogs.AnimEditor;

namespace WadTool.Features.DockableViews.AnimationEditor;

/// <summary>Dockable timeline with the transport buttons of the Animation Editor.</summary>
public partial class TimelinePanel : UserControl
{
    public TimelinePanel()
    {
        InitializeComponent();

        timeline.ValueChanged += OnValueChanged;
        timeline.SelectionChanged += OnSelectionChanged;
        timeline.AnimCommandDoubleClick += OnAnimCommandDoubleClick;
    }

    /// <summary>The track bar hosted by the panel.</summary>
    public AnimationTrackBar Track => timeline;

    private AnimationEditorWindowViewModel? ViewModel => DataContext as AnimationEditorWindowViewModel;

    private void OnValueChanged(object? sender, EventArgs e) => ViewModel?.OnTimelineValueChanged();

    private void OnSelectionChanged(object? sender, EventArgs e) => ViewModel?.OnTimelineSelectionChanged();

    private void OnAnimCommandDoubleClick(object? sender, WadAnimCommand command) => ViewModel?.EditAnimCommands(command);
}
