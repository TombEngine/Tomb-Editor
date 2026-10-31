#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool
{
    /// <summary>
    /// WPF rewrite of the previous <c>AnimCommandEditor</c> user control: edits a single
    /// <see cref="WadAnimCommand"/> in place (type combo + per-type parameter panel) and raises
    /// <see cref="AnimCommandChanged"/> on every user edit, exactly like the legacy control.
    /// </summary>
    public partial class AnimCommandEditor : UserControl
    {
        private bool _currentlyDoingCommandSelection = false;
        private AnimationEditor _editor;
        private List<string> _soundItems = new();

        public WadAnimCommand Command
        {
            get { return _command; }
            set
            {
                _command = value;
                UpdateUI(_command);
            }
        }
        private WadAnimCommand _command;

        public class AnimCommandEventArgs : EventArgs { public WadAnimCommand Command { get; set; } }
        public event EventHandler<AnimCommandEventArgs> AnimCommandChanged;
        private void InvokeChanged() { if (!_currentlyDoingCommandSelection) AnimCommandChanged?.Invoke(this, new AnimCommandEventArgs() { Command = _command }); }

        public AnimCommandEditor() { InitializeComponent(); }

        public void Initialize(AnimationEditor editor, bool disableFrameControls = false)
        {
            _editor = editor;

            comboCommandType.Items.Clear();

            foreach (WadAnimCommandType type in Enum.GetValues(typeof(WadAnimCommandType)))
            {
                if (editor.Wad.GameVersion != TRVersion.Game.TombEngine && type >= WadAnimCommandType.DisableInterpolation)
                    continue;

                comboCommandType.Items.Add(type.ToString().SplitCamelcase());
            }

            tbPlaySoundFrame.IsEnabled = !disableFrameControls;
            tbFlipEffectFrame.IsEnabled = !disableFrameControls;

            butPlaySound.IsEnabled = (_editor.Tool.ReferenceLevel is not null &&
                _editor.Tool.ReferenceLevel.Settings.GlobalSoundMap.Count > 0);

            comboFlipeffectConditions.IsEnabled = _editor.Tool.DestinationWad.GameVersion != TRVersion.Game.TombEngine;

            ReloadSounds();

            // Populate sound conditions. For non-TEN engines, only first 3 conditions are supported.

            comboPlaySoundConditions.Items.Clear();

            var soundConditions = Enum.GetValues(typeof(WadSoundEnvironmentType)).Cast<object>().ToArray();
            if (editor.Wad.GameVersion != TRVersion.Game.TombEngine)
                Array.Resize(ref soundConditions, 3);

            foreach (object condition in soundConditions)
                comboPlaySoundConditions.Items.Add(condition);

            // Populate flipeffect conditions.

            comboFlipeffectConditions.Items.Clear();
            foreach (string condition in Enum.GetValues(typeof(WadFootstepFlipeffectCondition)).Cast<object>().Select(o => o.ToString().SplitCamelcase()))
                comboFlipeffectConditions.Items.Add(condition);
        }

        public void UpdateUI(WadAnimCommand cmd)
        {
            if (_currentlyDoingCommandSelection)
                return;

            if (cmd is null)
            {
                comboCommandType.IsEnabled = false;
                ShowPanel(null);
                return;
            }

            comboCommandType.IsEnabled = true;

            try
            {
                _currentlyDoingCommandSelection = true;

                comboCommandType.SelectedIndex = comboCommandType.Items.Count < (int)cmd.Type ? -1 : (int)(cmd.Type) - 1;

                switch (cmd.Type)
                {
                    case WadAnimCommandType.SetPosition:
                        ShowPanel(panelSetPosition);

                        tbPosX.Value = cmd.Parameter1;
                        tbPosY.Value = cmd.Parameter2;
                        tbPosZ.Value = cmd.Parameter3;
                        break;

                    case WadAnimCommandType.SetJumpDistance:
                        ShowPanel(panelSetJumpVelocity);

                        tbHorizontal.Value = cmd.Parameter1;
                        tbVertical.Value = cmd.Parameter2;
                        break;

                    case WadAnimCommandType.EmptyHands:
                    case WadAnimCommandType.KillEntity:
                        ShowPanel(null);
                        break;

                    case WadAnimCommandType.DisableInterpolation:
                        ShowPanel(panelDisableInterpolation);

                        tbFrameDisableInterpolation.Value = cmd.Parameter1;
                        break;

                    case WadAnimCommandType.PlaySound:
                        ShowPanel(panelPlaySound);

                        tbPlaySoundFrame.Value = cmd.Parameter1;
                        nudSoundId.Value = cmd.Parameter2;
                        SyncSoundComboToId(cmd.Parameter2);

                        comboPlaySoundConditions.SelectedItem = (WadSoundEnvironmentType)cmd.Parameter3;
                        break;

                    case WadAnimCommandType.FlipEffect:
                        ShowPanel(panelFlipEffect);

                        tbFlipEffectFrame.Value = cmd.Parameter1;
                        tbFlipEffect.Value = cmd.Parameter2;

                        comboFlipeffectConditions.SelectedIndex = cmd.Parameter3;
                        break;
                }
            }
            finally
            {
                _currentlyDoingCommandSelection = false;
            }
        }

        private void ShowPanel(FrameworkElement panel)
        {
            panelSetPosition.Visibility = panel == panelSetPosition ? Visibility.Visible : Visibility.Collapsed;
            panelSetJumpVelocity.Visibility = panel == panelSetJumpVelocity ? Visibility.Visible : Visibility.Collapsed;
            panelFlipEffect.Visibility = panel == panelFlipEffect ? Visibility.Visible : Visibility.Collapsed;
            panelPlaySound.Visibility = panel == panelPlaySound ? Visibility.Visible : Visibility.Collapsed;
            panelDisableInterpolation.Visibility = panel == panelDisableInterpolation ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ReloadSounds()
        {
            _soundItems = WadSounds.GetFormattedList(_editor.Tool.ReferenceLevel, _editor.Wad.GameVersion).ToList();
            _soundItems.Add("Custom sound ID");

            comboSound.ItemsSource = _soundItems;
            comboSound.SelectedIndex = 0;
        }

        // While a search filter is active the view indices don't match the sound list, so both
        // sync directions go through the unfiltered _soundItems list instead of SelectedIndex.
        private void SyncSoundComboToId(int soundId)
        {
            if (_soundItems.Count == 0 || comboSound.Items.Filter is not null)
                return;

            comboSound.SelectedItem = _soundItems[Math.Min(soundId, _soundItems.Count - 1)];
        }

        private void comboCommandType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_command is null || _currentlyDoingCommandSelection)
                return;

            WadAnimCommandType newType = (WadAnimCommandType)(comboCommandType.SelectedIndex + 1);
            _command.Type = newType;
            UpdateUI(_command);
            InvokeChanged();
        }

        private void tbPosX_ValueChanged(object sender, EventArgs e)
        {
            if (_command is null || _command.Type != WadAnimCommandType.SetPosition)
                return;
            _command.Parameter1 = (short)tbPosX.Value;
            InvokeChanged();
        }

        private void tbPosY_ValueChanged(object sender, EventArgs e)
        {
            if (_command is null || _command.Type != WadAnimCommandType.SetPosition)
                return;
            _command.Parameter2 = (short)tbPosY.Value;
            InvokeChanged();
        }

        private void tbPosZ_ValueChanged(object sender, EventArgs e)
        {
            if (_command is null || _command.Type != WadAnimCommandType.SetPosition)
                return;
            _command.Parameter3 = (short)tbPosZ.Value;
            InvokeChanged();
        }

        private void tbHorizontal_ValueChanged(object sender, EventArgs e)
        {
            if (_command is null || _command.Type != WadAnimCommandType.SetJumpDistance)
                return;
            _command.Parameter1 = (short)tbHorizontal.Value;
            InvokeChanged();
        }

        private void tbVertical_ValueChanged(object sender, EventArgs e)
        {
            if (_command is null || _command.Type != WadAnimCommandType.SetJumpDistance)
                return;
            _command.Parameter2 = (short)tbVertical.Value;
            InvokeChanged();
        }

        private void tbFlipEffectFrame_ValueChanged(object sender, EventArgs e)
        {
            if (_command is null || _command.Type != WadAnimCommandType.FlipEffect)
                return;
            _command.Parameter1 = (short)tbFlipEffectFrame.Value;
            InvokeChanged();
        }

        private void tbFlipEffect_ValueChanged(object sender, EventArgs e)
        {
            if (_command is null || _command.Type != WadAnimCommandType.FlipEffect)
                return;
            _command.Parameter2 = (short)tbFlipEffect.Value;
            InvokeChanged();
        }

        private void tbPlaySoundFrame_ValueChanged(object sender, EventArgs e)
        {
            if (_command is null || _command.Type != WadAnimCommandType.PlaySound)
                return;
            _command.Parameter1 = (short)tbPlaySoundFrame.Value;
            InvokeChanged();
        }

        private void comboPlaySoundConditions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_command is null || _command.Type != WadAnimCommandType.PlaySound || comboPlaySoundConditions.SelectedItem is null)
                return;
            _command.Parameter3 = (short)((WadSoundEnvironmentType)comboPlaySoundConditions.SelectedItem);
            InvokeChanged();
        }

        private void comboSound_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_command is null || _command.Type != WadAnimCommandType.PlaySound)
                return;

            if (comboSound.SelectedItem is not string selectedSound)
                return;

            int index = _soundItems.IndexOf(selectedSound);
            if (index >= 0 && index < _soundItems.Count - 1)
                nudSoundId.Value = index;
        }

        private void comboFlipeffectConditions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_command is null || _command.Type != WadAnimCommandType.FlipEffect || comboFlipeffectConditions.SelectedIndex < 0)
                return;
            _command.Parameter3 = (short)((WadFootstepFlipeffectCondition)comboFlipeffectConditions.SelectedIndex);
            InvokeChanged();
        }

        private void butPlaySound_Click(object sender, RoutedEventArgs e)
        {
            if (_editor.Tool.ReferenceLevel is null ||
                _editor.Tool.ReferenceLevel.Settings.GlobalSoundMap.Count == 0)
                return;

            var soundInfo = _editor.Tool.ReferenceLevel.Settings.GlobalSoundMap.FirstOrDefault(soundInfo_ => soundInfo_.Id == (int)nudSoundId.Value);
            if (soundInfo is not null)
                try { WadSoundPlayer.PlaySoundInfo(_editor.Tool.ReferenceLevel, soundInfo); }
                catch (Exception) { } // FIXME: do something!
        }

        private void nudSoundId_ValueChanged(object sender, EventArgs e)
        {
            if (_command is null || _command.Type != WadAnimCommandType.PlaySound)
                return;

            _command.Parameter2 = (short)nudSoundId.Value;
            SyncSoundComboToId((int)nudSoundId.Value);

            InvokeChanged();
        }

        private void tbFrameDisableInterpolation_ValueChanged(object sender, EventArgs e)
        {
            if (_command is null || _command.Type != WadAnimCommandType.DisableInterpolation)
                return;
            _command.Parameter1 = (short)tbFrameDisableInterpolation.Value;
            InvokeChanged();
        }
    }
}
