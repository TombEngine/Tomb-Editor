using DarkUI.WPF.CustomControls;
using System.Windows;
using System.Windows.Controls;
using TombLib.WPF.Input;

namespace TombLib.Test;

[TestClass]
public class KeyboardInputTests
{
    [TestMethod]
    public void EditableControlsOwnTheirTypingAndUndoShortcuts()
    {
        StaTest.Run(() => {
            DependencyObject[] controls = [new TextBox(), new RichTextBox(), new PasswordBox(), new NumericUpDown(), new ComboBox { IsEditable = true }];
            foreach (var control in controls)
                Assert.IsTrue(KeyboardInput.IsEditingText(control), control.GetType().Name);
        });
    }

    [TestMethod]
    public void ViewportsAndReadOnlySelectionControlsAllowEditorShortcuts()
    {
        StaTest.Run(() => {
            Assert.IsFalse(KeyboardInput.IsEditingText(null));
            Assert.IsFalse(KeyboardInput.IsEditingText(new Border()));
            Assert.IsFalse(KeyboardInput.IsEditingText(new ComboBox { IsEditable = false }));
        });
    }
}
