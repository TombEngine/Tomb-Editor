#nullable enable

using System;
using System.Windows;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.Utils;
using TombLib.WPF;
using WadTool.Controls;

using PopUpInfo = TombLib.WPF.PopUpInfo;

namespace WadTool.Features.Dialogs.StaticEditor;

/// <summary>Native WPF StaticEditor view with a Direct3D viewport and MVVM editing tools.</summary>
public partial class StaticEditorWindow : Window
{
    private readonly PanelRenderingStaticEditor _panel;
    private readonly PopUpInfo _popup = new();
    private StaticEditorWindowViewModel? _vm;

    public StaticEditorWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();

        _panel = new PanelRenderingStaticEditor();
        _panel.MouseDoubleClick += OnPanelMouseDoubleClick;
        _panel.PositionChanged += OnPanelPositionChanged;
        renderingHost.Child = _panel;

        DataContextChanged += OnDataContextChanged;
        Closed += OnClosed;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_vm is not null)
            _vm.MessageRaised -= OnMessageRaised;

        _vm = e.NewValue as StaticEditorWindowViewModel;
        if (_vm is null)
            return;

        _vm.MessageRaised += OnMessageRaised;
        _vm.AttachPanel(_panel);
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _panel.Dispose();
        _popup.Dispose();

        _panel.MouseDoubleClick -= OnPanelMouseDoubleClick;
        _panel.PositionChanged -= OnPanelPositionChanged;

        if (_vm is not null)
        {
            _vm.MessageRaised -= OnMessageRaised;
            _vm.Detach();
        }
    }

    private void OnPanelMouseDoubleClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && _vm?.EditMeshCommand.CanExecute(null) == true)
            _vm.EditMeshCommand.Execute(null);
    }

    private void OnPanelPositionChanged() => _vm?.RefreshPositionFromPanel();

    private void OnMessageRaised(string message, PopupType type)
        => PopUpInfo.Show(_popup, null, _panel, message, type);
}
