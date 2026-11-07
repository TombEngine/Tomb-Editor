#nullable enable

using Keys = TombLib.Input.EditorKey;
using System;
using System.Linq;
using System.Windows.Interop;
using TombLib.WPF;
using TombLib.WPF.Input;

namespace TombEditor
{
    public partial class MainWindow
    {
        private void InstallHotkeyFilter() => ComponentDispatcher.ThreadFilterMessage += OnThreadFilterMessage;

        private void RemoveHotkeyFilter() => ComponentDispatcher.ThreadFilterMessage -= OnThreadFilterMessage;

        private void OnThreadFilterMessage(ref MSG message, ref bool handled)
        {
            if (handled || message.message is not (0x0100 or 0x0104))
                return;

            Keys keyData = (Keys)(int)message.wParam | KeyboardState.ModifierKeys;
            handled = HandleHotkey(keyData);
        }

        /// <summary>
        /// WPF-shell port of <c>FormMain.ProcessCmdKey</c>. Returns <see langword="true"/> to swallow
        /// the key (so it is not processed further), mirroring the legacy return values.
        /// </summary>
        private bool HandleHotkey(Keys keyData)
        {
            // Only act when this window is the active one; modal dialogs and other windows handle
            // their own keys (the legacy ProcessCmdKey was scoped to FormMain in the same way).
            if (!IsActive)
                return false;

            // Disable all hotkeys in fly mode except ToggleFlyMode; the Alt key is swallowed so the menu bar does not activate.
            if (_editor.FlyMode && !_editor.Configuration.UI_Hotkeys["ToggleFlyMode"].Contains(keyData))
                return keyData.HasFlag(Keys.Alt);

            if (_editor.CameraPreviewMode != CameraPreviewType.None)
            {
                if (keyData == Keys.Escape)
                {
                    _editor.ToggleCameraPreview(false);
                    return true;
                }

                // Disable all hotkeys in camera preview mode except PreviewCamera
                if (!_editor.Configuration.UI_Hotkeys["PreviewCamera"].Contains(keyData))
                    return true;
            }

            // Don't process reserved camera keys
            if (KeyboardInput.DirectionalCameraKeys.Contains(keyData))
                return false;

            // Don't process one-key and shift hotkeys if we're focused on a control which allows text input
            if (FocusedControlSupportsTextInput(keyData))
                return false;

            CommandHandler.ExecuteHotkey(new CommandArgs
            {
                Editor = _editor,
                KeyData = keyData,
                Window = WPFUtils.GetWindowOwner()
            });

            // Don't open menus with the alt key
            if (keyData.HasFlag(Keys.Alt))
                return true;

            return false;
        }

        /// <summary>
        /// WPF-aware equivalent of <c>KeyboardInput.IsEditingText</c>: returns
        /// <see langword="true"/> when the focused element is a text-entry control that should
        /// receive the key as input instead of having it trigger a hotkey.
        /// </summary>
        private static bool FocusedControlSupportsTextInput(Keys keyData)
        {
            bool textRelevantKey =
                keyData.HasFlag(Keys.Control | Keys.A) ||
                keyData.HasFlag(Keys.Control | Keys.X) ||
                keyData.HasFlag(Keys.Control | Keys.C) ||
                keyData.HasFlag(Keys.Control | Keys.V) ||
                (!keyData.HasFlag(Keys.Control) && !keyData.HasFlag(Keys.Alt));

            if (!textRelevantKey)
                return false;

            // WPF-focused text controls
            var wpfFocused = System.Windows.Input.Keyboard.FocusedElement;
            if (wpfFocused is System.Windows.Controls.Primitives.TextBoxBase ||
                wpfFocused is System.Windows.Controls.PasswordBox ||
                wpfFocused is System.Windows.Controls.ComboBox ||
                wpfFocused is System.Windows.Controls.ListBox ||
                wpfFocused is System.Windows.Controls.ListBoxItem ||
                wpfFocused is System.Windows.Controls.DataGrid ||
                wpfFocused is System.Windows.Controls.DataGridCell)
                return true;

            return false;
        }

    }
}
