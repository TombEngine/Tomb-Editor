#nullable enable

using System;
using System.Windows;
using TombLib.Wad;
using TombLib.WPF;

namespace WadTool.Features.Dialogs.AnimCommandsEditor;

/// <summary>
/// WPF port of the legacy <c>FormAnimCommandsEditor</c>. Selecting a row pushes its command into
/// the <see cref="AnimCommandEditor"/> control; edits made in the editor are pushed back into the
/// view model.
/// </summary>
public partial class AnimCommandsEditorWindow : Window
{
    private readonly AnimCommandEditor _commandEditor;
    private AnimCommandsEditorWindowViewModel? _vm;

    public AnimCommandsEditorWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();

        _commandEditor = commandEditor;
        _commandEditor.AnimCommandChanged += OnCommandEditorChanged;

        DataContextChanged += OnDataContextChanged;
        Loaded += OnLoaded;
        Closed += OnClosed;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_vm is not null)
            _vm.SelectedCommandChanged -= OnSelectedCommandChanged;

        _vm = e.NewValue as AnimCommandsEditorWindowViewModel;
        if (_vm is null)
            return;

        _commandEditor.Initialize(_vm.Editor);
        _vm.SelectedCommandChanged += OnSelectedCommandChanged;

        // Push the initially selected command into the hosted editor.
        OnSelectedCommandChanged(_vm.SelectedRow?.Command);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_vm is not null)
            WindowConfiguration.ConfigureWindow(this, _vm.Tool.Configuration, key: "FormAnimCommandsEditor");
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _commandEditor.AnimCommandChanged -= OnCommandEditorChanged;

        if (_vm is not null)
        {
            _vm.SelectedCommandChanged -= OnSelectedCommandChanged;
            _vm.RevertIfNotConfirmed();
            _vm.Detach();
        }

        // The hosted editor can preview sounds (PlaySound anim command); stop any sample still
        // playing, like the legacy OnFormClosing did.
        WadSoundPlayer.StopSample();
    }

    private void OnSelectedCommandChanged(WadAnimCommand? command) => _commandEditor.Command = command;

    private void OnCommandEditorChanged(object? sender, AnimCommandEditor.AnimCommandEventArgs e)
    {
        if (e.Command is not null)
            _vm?.OnCommandEditedFromControl(e.Command);
    }
}
