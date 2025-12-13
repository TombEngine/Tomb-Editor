using System;
using System.Numerics;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;
using TombLib.Utils;
using System.Drawing;

namespace TombEditor.Forms
{
    public partial class FormMoveable : DarkForm
    {
        private readonly MoveableInstance _movable;
        private Vector3 oldColor;
        private Editor _editor;
        public FormMoveable(MoveableInstance moveable)
        {
            _movable = moveable;
            InitializeComponent();
            _editor = Editor.Instance;

            // Set window property handlers
            Configuration.ConfigureWindow(this, _editor.Configuration);

            // Clear body flag in TEN is repurposed for reflection canceling.
            if (_editor.Level.IsTombEngine)
                cbClearBody.Text = "No reflection";
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            _movable.Color = oldColor;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void FormObject_Load(object sender, EventArgs e)
        {
            // Disable version-specific controls
            tbOCB.Enabled = _editor.Level.Settings.GameVersion.Native() > TRVersion.Game.TR3;

            oldColor = _movable.Color;
            cbBit1.Checked = (_movable.CodeBits & (1 << 0)) != 0;
            cbBit2.Checked = (_movable.CodeBits & (1 << 1)) != 0;
            cbBit3.Checked = (_movable.CodeBits & (1 << 2)) != 0;
            cbBit4.Checked = (_movable.CodeBits & (1 << 3)) != 0;
            cbBit5.Checked = (_movable.CodeBits & (1 << 4)) != 0;
            panelColor.BackColor = (_movable.Color * 0.5f).ToWinFormsColor();
            cbInvisible.Checked = _movable.Invisible;
            cbClearBody.Checked = _movable.ClearBody;
            tbOCB.Text = _movable.Ocb.ToString();

            // Disable mesh-specific controls
            var canBeColored = _movable.CanBeColored();
            Size = new System.Drawing.Size(Size.Width, canBeColored ? 264 : 236);
            lblColor.Visible = canBeColored;
            panelColor.Visible = canBeColored;
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            short ocb;
            if (!short.TryParse(tbOCB.Text, out ocb))
            {
                DarkMessageBox.Show(this, "The value of OCB field is not valid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _editor.UndoManager.PushObjectPropertyChanged(_movable);

            byte CodeBits = 0;
            CodeBits |= (byte)(cbBit1.Checked ? 1 << 0 : 0);
            CodeBits |= (byte)(cbBit2.Checked ? 1 << 1 : 0);
            CodeBits |= (byte)(cbBit3.Checked ? 1 << 2 : 0);
            CodeBits |= (byte)(cbBit4.Checked ? 1 << 3 : 0);
            CodeBits |= (byte)(cbBit5.Checked ? 1 << 4 : 0);
            _movable.CodeBits = CodeBits;

            _movable.Invisible = cbInvisible.Checked;
            _movable.ClearBody = cbClearBody.Checked;

            _movable.Ocb = ocb;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void tbOCB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '-' || tbOCB.SelectionStart != 0))
                e.Handled = true;
        }

        private void panelColor_Click(object sender, EventArgs e)
        {
            EditorActions.EditColor(this, _movable, (Vector3 newColor) =>
            {
                panelColor.BackColor = newColor.ToWinFormsColor();
            });
        }

        private void butResetTint_Click(object sender, EventArgs e)
        {
            panelColor.BackColor = Color.Gray;
            _movable.Color = panelColor.BackColor.ToFloat3Color() * 2.0f;
        }
    }
}
