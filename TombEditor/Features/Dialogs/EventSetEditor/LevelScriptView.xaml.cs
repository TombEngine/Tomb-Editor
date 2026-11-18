#nullable enable

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TombLib.WPF.Input;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    /// <summary>Level-script mode panel of the event editor; all logic lives in <see cref="LevelScriptViewModel"/>.</summary>
    public partial class LevelScriptView : UserControl
    {
        public LevelScriptView()
        {
            InitializeComponent();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // Delete and Backspace unassign the function, unless the filter or argument box is being typed in.
            if (e.Key is not (Key.Delete or Key.Back) || Keyboard.Modifiers != ModifierKeys.None)
                return;

            if (DataContext is not LevelScriptViewModel viewModel || KeyboardInput.IsEditingText(e.OriginalSource as DependencyObject))
                return;

            viewModel.Unassign();
            e.Handled = true;
        }
    }
}
