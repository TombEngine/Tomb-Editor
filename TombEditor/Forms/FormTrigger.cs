using System;
using System.Linq;
using System.Windows.Forms;
using DarkUI.Forms;
using NLog;
using TombLib;
using TombLib.Controls;
using TombLib.LevelData;
using TombLib.NG;
using TombLib.Utils;
using TombLib.Forms;

namespace TombEditor.Forms
{
    public partial class FormTrigger : DarkForm
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Level _level;
        private readonly TriggerInstance _trigger;

        private bool _dialogIsUpdating = false;

        public FormTrigger(TriggerInstance trigger, Level level, Action<ObjectInstance> selectObject,
                           Action<Room> selectRoom)
        {
            InitializeComponent();

            _level = level;
            _trigger = trigger;

            // Assign script IDs if necessary
            AllocateNewScriptIds();

            // Setup events
            foreach (var control in tableLayoutPanel.Controls.OfType<TriggerParameterControl>())
            {
                control.ViewObject += selectObject;
                control.ViewRoom += selectRoom;
                control.Level = level;
            }

            this.SetActualSize();
            this.LockHeight();
            this.butOK.Select();

            if (_level.IsTombEngine)
                Text = "Classic trigger editor";
            else
                Text = "Trigger editor";

            if (!_level.IsNG)
            {
                labelPlugin.Visible = paramPlugin.Visible = false;
                tableLayoutPanel.SetColumnSpan(paramTriggerType, 7);
            }

            // Set window property handlers
            Configuration.ConfigureWindow(this, Editor.Instance.Configuration);

            Initialize(_trigger);
        }

        public void Initialize(TriggerInstance trigger)
        {
            // Update the dialog
            UpdateDialog();

            // Set values
            cbBit1.Checked = (trigger.CodeBits & (1 << 0)) != 0;
            cbBit2.Checked = (trigger.CodeBits & (1 << 1)) != 0;
            cbBit3.Checked = (trigger.CodeBits & (1 << 2)) != 0;
            cbBit4.Checked = (trigger.CodeBits & (1 << 3)) != 0;
            cbBit5.Checked = (trigger.CodeBits & (1 << 4)) != 0;
            cbOneShot.Checked = trigger.OneShot;

            paramTriggerType.Parameter = new TriggerParameterUshort((ushort)trigger.TriggerType);
            paramTargetType.Parameter = new TriggerParameterUshort((ushort)trigger.TargetType);
            paramPlugin.Parameter = trigger.Plugin;

            // HACK: Change order of population based on target type.
            if (trigger.TriggerType == TriggerType.ConditionNg)
            {
                paramTimer.Parameter = trigger.Timer;
                paramTarget.Parameter = trigger.Target;
            }
            else
            {
                paramTarget.Parameter = trigger.Target;
                paramTimer.Parameter = trigger.Timer;
            }

            paramExtra.Parameter = trigger.Extra;
        }

        public void UpdateDialog()
        {
            // This is needed to prevent recursive UpdateDialog call.
            if (_dialogIsUpdating)
                return;

            _dialogIsUpdating = true;

            paramTriggerType.ParameterRange = NgParameterInfo.GetTriggerTypeRange(_level.Settings).ToParameterRange();
            paramTargetType.ParameterRange = NgParameterInfo.GetTargetTypeRange(_level.Settings, TriggerType).ToParameterRange();

            bool isEvent = TargetType == TriggerTargetType.VolumeEvent || TargetType == TriggerTargetType.GlobalEvent;
            bool isConditionNg = isEvent || TriggerType == TriggerType.ConditionNg || TargetType == TriggerTargetType.ActionNg;

            // HACK: Change order of population based on target type.

            if (_level.IsNG)
                paramPlugin.ParameterRange = NgParameterInfo.GetPluginRange(_level.Settings);

            if (isConditionNg)
            {
                paramTimer.ParameterRange = NgParameterInfo.GetTimerRange(_level.Settings, TriggerType, TargetType, paramTarget.Parameter, paramPlugin.Parameter);
                paramTarget.ParameterRange = NgParameterInfo.GetTargetRange(_level.Settings, TriggerType, TargetType, paramTimer.Parameter, paramPlugin.Parameter);
            }
            else
            {
                paramTarget.ParameterRange = NgParameterInfo.GetTargetRange(_level.Settings, TriggerType, TargetType, paramTimer.Parameter, paramPlugin.Parameter);
                paramTimer.ParameterRange = NgParameterInfo.GetTimerRange(_level.Settings, TriggerType, TargetType, paramTarget.Parameter, paramPlugin.Parameter);
            }

            paramExtra.ParameterRange = NgParameterInfo.GetExtraRange(
                _level.Settings, TriggerType, TargetType, paramTarget.Parameter, paramTimer.Parameter, paramPlugin.Parameter,
                out bool isButtons);

            _dialogIsUpdating = false;
            cbOneShot.Enabled = !isEvent;

            if (isButtons)
            {
                ushort selectedExtraKey = paramExtra.Parameter is TriggerParameterUshort extraParam
                    ? extraParam.Key
                    : (ushort)0;

                // Select all 6 check boxes in binary order
                cbBit1.Checked = (selectedExtraKey & 1) != 0;
                cbBit2.Checked = (selectedExtraKey & 2) != 0;
                cbBit3.Checked = (selectedExtraKey & 4) != 0;
                cbBit4.Checked = (selectedExtraKey & 8) != 0;
                cbBit5.Checked = (selectedExtraKey & 16) != 0;
                cbOneShot.Checked = (selectedExtraKey & 32) != 0;
            }

            UpdateExportToTrigger();
        }

        private TriggerType TriggerType => paramTriggerType.Parameter is TriggerParameterUshort ?
                (TriggerType)((TriggerParameterUshort)paramTriggerType.Parameter).Key : TriggerType.Trigger;

        private TriggerTargetType TargetType => paramTargetType.Parameter is TriggerParameterUshort ?
                (TriggerTargetType)((TriggerParameterUshort)paramTargetType.Parameter).Key : TriggerTargetType.Object;

        private byte CodeBits
        {
            get
            {
                byte codeBits = 0;
                codeBits |= (byte)(cbBit1.Checked ? 1 << 0 : 0);
                codeBits |= (byte)(cbBit2.Checked ? 1 << 1 : 0);
                codeBits |= (byte)(cbBit3.Checked ? 1 << 2 : 0);
                codeBits |= (byte)(cbBit4.Checked ? 1 << 3 : 0);
                codeBits |= (byte)(cbBit5.Checked ? 1 << 4 : 0);
                return codeBits;
            }
        }

        private TriggerInstance TestTrigger => new TriggerInstance(new RectangleInt2())
            {
                TriggerType = TriggerType,
                TargetType = TargetType,
                Plugin = paramPlugin.Parameter,
                Target = paramTarget.Parameter,
                Timer = paramTimer.Parameter,
                Extra = paramExtra.Parameter,
                CodeBits = CodeBits,
                OneShot = cbOneShot.Checked
            };

        private void butOK_Click(object sender, EventArgs e)
        {
            // Test if everything is 'ok'
            if (!NgParameterInfo.TriggerIsValid(_level.Settings, TestTrigger))
                if (DarkMessageBox.Show(this, "The currently selected trigger data is not valid for the engine.",
                    "Trigger invalid", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != DialogResult.OK)
                    return;

            // Store some values like we have not NG triggers
            _trigger.TriggerType = TriggerType;
            _trigger.TargetType = TargetType;
            _trigger.Plugin = paramPlugin.Parameter;
            _trigger.Target = paramTarget.Parameter;
            _trigger.Timer = paramTimer.Parameter;
            _trigger.Extra = paramExtra.Parameter;
            _trigger.CodeBits = CodeBits;
            _trigger.OneShot = cbOneShot.Checked;

            // Close
            DialogResult = DialogResult.OK;
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private const string _noScriptIdStr = "<NoScriptID>";

        private void OnParameterChanged(object sender, EventArgs e)
        {
            UpdateDialog();
        }

        private void UpdateExportToTrigger()
        {
            scriptExportPanel.Visible = _level.IsNG;
            if (!_level.IsNG)
                return;

            try
            {
                tbScript.Text = NgParameterInfo.ExportToScriptTrigger(_level, TestTrigger, null);
                tbScript.Tag = NgParameterInfo.ExportToScriptTrigger(_level, TestTrigger, null, true);
                tbScript.Enabled = true;
                butCopyToClipboard.Enabled = true;
                butCopyWithComments.Enabled = true;

                // Disable animcommand export for action/condition triggers because only god and Paolone knows how it is encoded.
                butCopyAsAnimcommand.Enabled = !(TargetType == TriggerTargetType.ParameterNg || TargetType == TriggerTargetType.ActionNg);
            }
            catch (NgParameterInfo.ExceptionScriptNotSupported)
            {
                tbScript.Text = "Not supported";
                tbScript.Tag = null;
                tbScript.Enabled = false;
                butCopyToClipboard.Enabled = false;
                butCopyWithComments.Enabled = false;
                butCopyAsAnimcommand.Enabled = false;
            }
            catch (NgParameterInfo.ExceptionScriptIdMissing)
            {
                tbScript.Text = "Click to generate";
                tbScript.Tag = null;
                tbScript.Enabled = false;
                butCopyToClipboard.Enabled = false;
                butCopyWithComments.Enabled = false;
                butCopyAsAnimcommand.Enabled = false;
            }
            catch (Exception exc)
            {
                tbScript.Text = "Check all fields";
                tbScript.Tag = null;
                tbScript.Enabled = false;
                butCopyToClipboard.Enabled = false;
                butCopyWithComments.Enabled = false;
                butCopyAsAnimcommand.Enabled = false;
                logger.Debug(exc, "\"ExportToScriptTrigger\" failed.");
            }
        }

        private void AllocateNewScriptIds()
        {
            if (!_level.Settings.GameVersion.IsNG())
                return;

            if (paramTarget.Parameter is IHasScriptID && !(paramTarget.Parameter as IHasScriptID).ScriptId.HasValue)
                (paramTarget.Parameter as IHasScriptID).AllocateNewScriptId();

            if (paramTimer.Parameter is IHasScriptID && !(paramTimer.Parameter as IHasScriptID).ScriptId.HasValue)
                (paramTimer.Parameter as IHasScriptID)?.AllocateNewScriptId();

            if (paramExtra.Parameter is IHasScriptID && !(paramExtra.Parameter as IHasScriptID).ScriptId.HasValue)
                (paramExtra.Parameter as IHasScriptID)?.AllocateNewScriptId();
        }

        private void butCopyToClipboard_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(tbScript.Text))
                Clipboard.SetText(tbScript.Text);
        }

        private void butCopyWithComments_Click(object sender, EventArgs e)
        {
            var commentString = tbScript.Tag as string;
            if (!string.IsNullOrEmpty(commentString))
                Clipboard.SetText(commentString);
        }

        private void butCopyAsAnimcommand_Click(object sender, EventArgs e)
        {
            using (var form = new FormInputBox("Specify animcommand frame number",
                "Enter value from -1 (any frame) to 254:", "-1"))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    int frameNumber = 0;
                    if (!int.TryParse(form.Result, out frameNumber) || frameNumber < -1 || frameNumber > 254)
                    {
                        DarkMessageBox.Show(this, "Frame number is invalid. Maximum is 254.", "Error", MessageBoxIcon.Error);
                        return;
                    }

                    var result = NgParameterInfo.ExportToScriptTrigger(_level, TestTrigger, frameNumber);
                    if (!string.IsNullOrEmpty(result))
                    {
                        using (var form2 = new FormInputBox("Export as SetPosition animcommand",
                            "Put these values into X, Y and Z fields in WadTool:", result))
                            form2.ShowDialog(this);
                    }
                }
            }
        }

        private void cbRawMode_CheckedChanged(object sender, EventArgs e)
        {
            foreach (var control in tableLayoutPanel.Controls.OfType<TriggerParameterControl>())
                control.RawMode = cbRawMode.Checked;
        }

        private void butSearchTrigger_Click(object sender, EventArgs e)
        {
            using (var form = new FormInputBox("Import trigger from script", "Enter a script command:"))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    var result = NgParameterInfo.ImportFromScriptTrigger(_level, form.Result);
                    if (result == null)
                    {
                        DarkMessageBox.Show(this, "Script entry is invalid. Trigger can't be imported.", "Error", MessageBoxIcon.Error);
                        return;
                    }
                    else
                        Initialize(result);
                }
            }
        }
    }
}
