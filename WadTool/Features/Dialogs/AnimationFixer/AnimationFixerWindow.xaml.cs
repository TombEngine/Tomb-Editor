#nullable enable

using System.Windows;
using TombLib.WPF;

namespace WadTool.Features.Dialogs.AnimationFixer;

public partial class AnimationFixerWindow : Window
{
    public AnimationFixerWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();

        // Reuses the legacy Window_FormAnimationFixer_* config slots so existing user configs keep applying.
        Loaded += (_, _) =>
        {
            if (DataContext is AnimationFixerWindowViewModel viewModel)
                WindowConfiguration.ConfigureWindow(this, viewModel.Configuration, key: "FormAnimationFixer");
        };
    }
}
