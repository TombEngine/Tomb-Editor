using System;
using System.Windows.Forms;
using System.Windows.Input;
using TombLib.Utils;

namespace TombEditor
{
    // This is a temporary bridge to allow WPF to handle hotkeys in the editor, while still using WinForms for the main window.
    // It fixes issues such as Undo/Redo not working when the flyby timeline is focused.
    // Remove this once the editor is fully migrated to WPF.
    internal sealed class WpfEditorShortcutBridge : IDisposable
    {
        private readonly Form _form;
        private readonly Editor _editor;

        public WpfEditorShortcutBridge(Form form, Editor editor)
        {
            _form = form;
            _editor = editor;
            InputManager.Current.PostProcessInput += OnPostProcessInput;
        }

        public void Dispose()
        {
            InputManager.Current.PostProcessInput -= OnPostProcessInput;
        }

        private void OnPostProcessInput(object sender, ProcessInputEventArgs e)
        {
            if (e.StagingItem.Input is not System.Windows.Input.KeyEventArgs keyEventArgs ||
                !_form.ContainsFocus)
                return;

            bool isPreviewKeyDown = keyEventArgs.RoutedEvent == Keyboard.PreviewKeyDownEvent;

            if (!isPreviewKeyDown && keyEventArgs.RoutedEvent != Keyboard.KeyDownEvent)
                return;

            Keys keyData = GetKeyData(keyEventArgs);

            if (_editor.FlyMode && !_editor.Configuration.UI_Hotkeys["ToggleFlyMode"].Contains(keyData))
                return;

            if (_editor.CameraPreviewMode != CameraPreviewType.None)
            {
                if (keyData == Keys.Escape)
                {
                    _editor.ToggleCameraPreview(false);
                    keyEventArgs.Handled = true;
                    return;
                }

                if (!_editor.Configuration.UI_Hotkeys["PreviewCamera"].Contains(keyData))
                {
                    keyEventArgs.Handled = true;
                    return;
                }
            }

            bool isUnhandledPreviewKeyDown = isPreviewKeyDown && !keyEventArgs.Handled;
            bool isHandledKeyDown = !isPreviewKeyDown && keyEventArgs.Handled;
            bool isDirectionalCameraKey = WinFormsUtils.DirectionalCameraKeys.Contains(keyData);
            bool focusedControlSupportsInput = WinFormsUtils.CurrentControlSupportsInput(_form, keyData);

            if (isUnhandledPreviewKeyDown ||
                isHandledKeyDown ||
                isDirectionalCameraKey ||
                focusedControlSupportsInput)
                return;

            bool commandExecuted = CommandHandler.ExecuteHotkey(new CommandArgs
            {
                Editor = _editor,
                KeyData = keyData,
                Window = _form
            });

            if (commandExecuted)
                keyEventArgs.Handled = true;
        }

        private static Keys GetKeyData(System.Windows.Input.KeyEventArgs keyEventArgs)
        {
            Key key = keyEventArgs.Key == Key.System ? keyEventArgs.SystemKey : keyEventArgs.Key;
            Keys keyData = (Keys)KeyInterop.VirtualKeyFromKey(key);
            ModifierKeys modifiers = keyEventArgs.KeyboardDevice.Modifiers;

            if (modifiers.HasFlag(ModifierKeys.Control))
                keyData |= Keys.Control;

            if (modifiers.HasFlag(ModifierKeys.Shift))
                keyData |= Keys.Shift;

            if (modifiers.HasFlag(ModifierKeys.Alt))
                keyData |= Keys.Alt;

            return keyData;
        }
    }
}
