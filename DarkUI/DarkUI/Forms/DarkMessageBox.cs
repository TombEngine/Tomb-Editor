using System;
using System.Windows.Forms;

namespace DarkUI.Forms
{
    public static class DarkMessageBox
    {
        /// <summary>
        /// Optional redirection used by WPF-based shells: when set, every Show overload routes
        /// here — (owner, message, caption, buttons, icon, defaultButton) → result — instead of
        /// opening the WinForms dialog, so message boxes match the host UI framework. Shells that
        /// never set it (e.g. the legacy --winforms one) keep the WinForms dialog.
        /// </summary>
        public static Func<IWin32Window, string, string, MessageBoxButtons, MessageBoxIcon, MessageBoxDefaultButton, DialogResult> ShowOverride { get; set; }

        private static DialogResult ShowCore(IWin32Window owner, string message, string caption,
            MessageBoxButtons? buttons, MessageBoxIcon? icon, MessageBoxDefaultButton? defaultButton, bool centerScreen)
        {
            if (ShowOverride != null)
                return ShowOverride(owner, message, caption ?? string.Empty,
                    buttons ?? MessageBoxButtons.OK, icon ?? MessageBoxIcon.None, defaultButton ?? MessageBoxDefaultButton.Button1);

            using (var form = new DarkDialogMessageBox() { Message = message })
            {
                if (caption != null)
                    form.Text = caption;
                if (buttons.HasValue)
                    form.DialogButtons = buttons.Value;
                if (icon.HasValue)
                    form.Icon = icon.Value;
                if (defaultButton.HasValue)
                    form.DefaultButton = defaultButton.Value;
                if (centerScreen)
                    form.StartPosition = FormStartPosition.CenterScreen;

                return owner != null ? form.ShowDialog(owner) : form.ShowDialog();
            }
        }

        [Obsolete("It is recommended to specify the \"owner\" of the message box. This way the message box can be opened on top of the currently active window.")]
        public static DialogResult Show(string message)
            => ShowCore(null, message, null, null, null, null, false);

        public static DialogResult Show(IWin32Window owner, string message)
            => ShowCore(owner, message, null, null, null, null, false);

        [Obsolete("It is recommended to specify the \"owner\" of the message box. This way the message box can be opened on top of the currently active window.")]
        public static DialogResult Show(string message, string caption)
            => ShowCore(null, message, caption, null, null, null, true);

        public static DialogResult Show(IWin32Window owner, string message, string caption)
            => ShowCore(owner, message, caption, null, null, null, false);

        [Obsolete("It is recommended to specify the \"owner\" of the message box. This way the message box can be opened on top of the currently active window.")]
        public static DialogResult Show(string message, string caption, MessageBoxIcon icon)
            => ShowCore(null, message, caption, null, icon, null, true);

        public static DialogResult Show(IWin32Window owner, string message, string caption, MessageBoxIcon icon)
            => ShowCore(owner, message, caption, null, icon, null, false);

        [Obsolete("It is recommended to specify the \"owner\" of the message box. This way the message box can be opened on top of the currently active window.")]
        public static DialogResult Show(string message, string caption, MessageBoxButtons buttons)
            => ShowCore(null, message, caption, buttons, null, null, true);

        public static DialogResult Show(IWin32Window owner, string message, string caption, MessageBoxButtons buttons)
            => ShowCore(owner, message, caption, buttons, null, null, false);

        [Obsolete("It is recommended to specify the \"owner\" of the message box. This way the message box can be opened on top of the currently active window.")]
        public static DialogResult Show(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
            => ShowCore(null, message, caption, buttons, icon, null, true);

        public static DialogResult Show(IWin32Window owner, string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
            => ShowCore(owner, message, caption, buttons, icon, null, false);

        [Obsolete("It is recommended to specify the \"owner\" of the message box. This way the message box can be opened on top of the currently active window.")]
        public static DialogResult Show(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
            => ShowCore(null, message, caption, buttons, icon, defaultButton, true);

        public static DialogResult Show(IWin32Window owner, string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
            => ShowCore(owner, message, caption, buttons, icon, defaultButton, false);
    }
}
