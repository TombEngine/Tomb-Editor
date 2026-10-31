#nullable enable

using System.Windows;

namespace TombLib.WPF.Features.AnimatedTextures
{
    /// <summary>Shows a themed text prompt bound to an input dialog view model.</summary>
    public partial class InputDialog : Window
    {
        public InputDialog()
        {
            InitializeComponent();
            this.HookModalAutoClose();
            Loaded += (_, _) => { input.Focus(); input.SelectAll(); };
        }
    }
}
