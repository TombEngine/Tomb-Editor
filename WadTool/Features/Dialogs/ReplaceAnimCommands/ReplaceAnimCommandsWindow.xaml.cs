#nullable enable

using System.Windows;
using TombLib.Wad;
using TombLib.WPF;

namespace WadTool.Features.Dialogs.ReplaceAnimCommands;

/// <summary>
/// WPF port of the legacy <c>FormReplaceAnimCommands</c>, using two WPF
/// <see cref="AnimCommandEditor"/> controls.
/// </summary>
public partial class ReplaceAnimCommandsWindow : Window
{
    private readonly AnimCommandEditor _aceFind;
    private readonly AnimCommandEditor _aceReplace;

    public ReplaceAnimCommandsWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();

        _aceFind = findCommandEditor;
        _aceReplace = replaceCommandEditor;

        DataContextChanged += (_, _) => AttachViewModel();

        // Reuses the legacy Window_FormReplaceAnimCommands_* config slots so existing user
        // configs keep applying.
        Loaded += (_, _) =>
        {
            if (DataContext is ReplaceAnimCommandsWindowViewModel vm)
                WindowConfiguration.ConfigureWindow(this, vm.Editor.Tool.Configuration, key: "FormReplaceAnimCommands");
        };

        // The hosted editors can preview sounds (PlaySound anim command); stop any sample
        // still playing, like the legacy OnFormClosing did.
        Closed += (_, _) => WadSoundPlayer.StopSample();
    }

    private void AttachViewModel()
    {
        if (DataContext is not ReplaceAnimCommandsWindowViewModel vm)
            return;

        _aceFind.Initialize(vm.Editor, true);
        _aceReplace.Initialize(vm.Editor, true);

        // The hosted editors mutate these command instances in place, so the view model always
        // sees their current state without extra event wiring — the legacy form likewise read
        // aceFind.Command / aceReplace.Command directly when searching.
        _aceFind.Command = vm.FindAnimCommand;
        _aceReplace.Command = vm.ReplaceWithAnimCommand;
    }
}
