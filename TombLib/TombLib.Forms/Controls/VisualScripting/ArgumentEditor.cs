using DarkUI.Config;
using DarkUI.Controls;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombLib.Controls.VisualScripting
{
    public partial class ArgumentEditor : UserControl
    {
        private class ComboBoxItem
        {
            public ComboBoxItem(string displayText, string value)
            {
                DisplayText = displayText;
                Value = value;
            }

            public ComboBoxItem(PositionAndScriptBasedObjectInstance item)
            {
                DisplayText = item.ToShortString();
                Value = TextExtensions.Quote(item.LuaName == null ? string.Empty : item.LuaName);
            }

            public string DisplayText;
            public string Value;
            public override string ToString() => DisplayText;
        }

        public ArgumentType ArgumentType => _argumentType;
        private ArgumentType _argumentType = ArgumentType.Numerical;

        public event EventHandler ValueChanged;
        private void OnValueChanged()
            => ValueChanged?.Invoke(this, EventArgs.Empty);

        public event EventHandler LocatedItemFound;
        private void OnLocatedItemFound(IHasLuaName item)
            => LocatedItemFound?.Invoke(item, EventArgs.Empty);

        public event EventHandler SoundtrackPlayed;
        private void OnSoundtrackPlayed(string filename)
            => SoundtrackPlayed?.Invoke(filename, EventArgs.Empty);

        public event EventHandler SoundEffectPlayed;
        private void OnSoundEffectPlayed(string sound)
            => SoundEffectPlayed?.Invoke(sound, EventArgs.Empty);

        public new string Text
        {
            get { return _text; }
            set { UnboxValue(value); }
        }
        private string _text = string.Empty;

        public ArgumentEditor()
        {
            InitializeComponent();

            // HACK: Fix textbox UI height.
            tbString.AutoSize = false;

            // HACK: Force increased size for reluctant controls.
            nudNumerical.Font = tableVector3.Font = new Font(Font.Name, Font.Size + 1.0f);
        }

        public void ShowGroup()
        {
            var groups = Controls.OfType<DarkPanel>().Where(c => c.Name.StartsWith("group")).ToList();
            groups.ForEach(g => g.Visible = false);

            DarkPanel targetPanel;

            switch (_argumentType)
            {
                case ArgumentType.Boolean: targetPanel = groupBool; break;
                case ArgumentType.Numerical: targetPanel = groupNumerical; break;
                case ArgumentType.String: targetPanel = groupString; break;
                case ArgumentType.Color: targetPanel = groupColor; break;
                case ArgumentType.Vector2: targetPanel = groupVector2; break;
                case ArgumentType.Vector3: targetPanel = groupVector3; break;
                case ArgumentType.Time: targetPanel = groupTime; break;
                default: targetPanel = groupList; break;
            }

            targetPanel.Visible = true;
            targetPanel.BringToFront();

            foreach (Control control in targetPanel.Controls)
                control.Height = targetPanel.Height;

            tbString.AutoSize = false;
            tbString.Height = nudNumerical.Height;
        }

        public void SetArgumentType(ArgumentLayout layout, NodeEditor editor)
        {
            _argumentType = layout.Type;

            Visible = false;
            SuspendLayout();

            ShowGroup();

            if (_argumentType >= ArgumentType.LuaScript)
            {

                switch (_argumentType)
                {
                    case ArgumentType.Sinks:
                    case ArgumentType.Statics:
                    case ArgumentType.Moveables:
                    case ArgumentType.Volumes:
                    case ArgumentType.Cameras:
                    case ArgumentType.FlybyCameras:
                    case ArgumentType.SoundTracks:
                    case ArgumentType.SoundEffects:
                        panelAction.Size = new Size(cbList.Height, cbList.Height);
                        panelAction.Visible = true;

                        if (_argumentType == ArgumentType.SoundTracks ||
                            _argumentType == ArgumentType.SoundEffects)
                        {
                            butAction.Image = Properties.Resources.actions_play_16;
                        }
                        else
                        {
                            butAction.Image = Properties.Resources.general_target_16;
                        }

                        break;

                    default:
                        panelAction.Visible = false;
                        break;
                }

                cbList.Items.Clear();
            }

            if (_argumentType == ArgumentType.String)
                panelMultiline.Visible = !layout.CustomEnumeration.Contains("NoMultiline");

            switch (_argumentType)
            {
                case ArgumentType.LuaScript:
                    foreach (var item in editor.CachedLuaFunctions)
                        cbList.Items.Add(new ComboBoxItem(item, "LevelFuncs." + item));
                    break;
                case ArgumentType.VolumeEventSets:
                    foreach (var item in editor.CachedVolumeEventSets)
                        cbList.Items.Add(new ComboBoxItem(item, TextExtensions.Quote(item)));
                    break;
                case ArgumentType.GlobalEventSets:
                    foreach (var item in editor.CachedGlobalEventSets)
                        cbList.Items.Add(new ComboBoxItem(item, TextExtensions.Quote(item)));
                    break;
                case ArgumentType.Sinks:
                    foreach (var item in editor.CachedSinks)
                        cbList.Items.Add(new ComboBoxItem(item));
                    break;
                case ArgumentType.Statics:
                    foreach (var item in editor.CachedStatics)
                        cbList.Items.Add(new ComboBoxItem(item));
                    break;
                case ArgumentType.Moveables:
                    cbList.Items.Add(new ComboBoxItem("[ Activator ]", LuaSyntax.ActivatorNamePrefix));
                    foreach (var item in editor.CachedMoveables.Where(s => layout.CustomEnumeration.Count == 0 ||
                                                                           layout.CustomEnumeration.Any(e => s
                                                                            .WadObjectId.ShortName(TRVersion.Game.TombEngine)
                                                                            .IndexOf(e, StringComparison.InvariantCultureIgnoreCase) != -1)))
                        cbList.Items.Add(new ComboBoxItem(item));
                    break;
                case ArgumentType.Volumes:
                    foreach (var item in editor.CachedVolumes)
                        cbList.Items.Add(new ComboBoxItem(item));
                    break;
                case ArgumentType.Cameras:
                    foreach (var item in editor.CachedCameras)
                        cbList.Items.Add(new ComboBoxItem(item));
                    break;
                case ArgumentType.FlybyCameras:
                    foreach (var item in editor.CachedFlybys)
                        cbList.Items.Add(new ComboBoxItem(item));
                    break;
                case ArgumentType.Rooms:
                    foreach (var item in editor.CachedRooms)
                        cbList.Items.Add(new ComboBoxItem(cbList.Items.Count.ToString() + ": " +
                            item.ToString(), TextExtensions.Quote(item.Name)));
                    break;
                case ArgumentType.SoundEffects:
                    foreach (var item in editor.CachedSoundInfos)
                        cbList.Items.Add(new ComboBoxItem(item.ToString(), item.Id.ToString()));
                    break;
                case ArgumentType.SoundTracks:
                    foreach (var item in editor.CachedSoundTracks)
                        cbList.Items.Add(new ComboBoxItem(Path.GetFileNameWithoutExtension(item), TextExtensions.Quote(item)));
                    break;
                case ArgumentType.Videos:
                    foreach (var item in editor.CachedVideos)
                        cbList.Items.Add(new ComboBoxItem(Path.GetFileNameWithoutExtension(item), TextExtensions.Quote(item)));
                    break;
                case ArgumentType.CompareOperator:
                    foreach (var item in Enum.GetValues(typeof(ConditionType)))
                        cbList.Items.Add(new ComboBoxItem(item.ToString().SplitCamelcase(), cbList.Items.Count.ToString()));
                    break;
                case ArgumentType.VolumeEvents:
                    foreach (var item in Event.VolumeEventTypes)
                        cbList.Items.Add(new ComboBoxItem(item.ToString().SplitCamelcase(), cbList.Items.Count.ToString()));
                    break;
                case ArgumentType.GlobalEvents:
                    foreach (var item in Event.GlobalEventTypes)
                        cbList.Items.Add(new ComboBoxItem(item.ToString().SplitCamelcase(), cbList.Items.Count.ToString()));
                    break;
                case ArgumentType.SpriteSlots:
                    foreach (var item in editor.CachedSpriteSlots.Where(s => layout.CustomEnumeration.Count == 0 ||
                                                                             layout.CustomEnumeration.Any(e => s
                                                                              .IndexOf(e, StringComparison.InvariantCultureIgnoreCase) != -1)))
                        cbList.Items.Add(new ComboBoxItem(item, LuaSyntax.ObjectIDPrefix + LuaSyntax.Splitter + item));
                    break;
                case ArgumentType.WadSlots:
                    foreach (var item in editor.CachedWadSlots.Where(s => layout.CustomEnumeration.Count == 0 ||
                                                                          layout.CustomEnumeration.Any(e => s
                                                                           .IndexOf(e, StringComparison.InvariantCultureIgnoreCase) != -1)))
                        cbList.Items.Add(new ComboBoxItem(item, LuaSyntax.ObjectIDPrefix + LuaSyntax.Splitter + item));
                    break;
                case ArgumentType.Enumeration:
                    foreach (var item in layout.CustomEnumeration)
                        cbList.Items.Add(new ComboBoxItem(item, layout.CustomEnumeration.IndexOf(item).ToString()));
                    break;
                case ArgumentType.Boolean:
                    cbBool.Text = layout.Description;
                    break;
                case ArgumentType.Numerical:
                case ArgumentType.Vector2:
                case ArgumentType.Vector3:
                    if (layout.CustomEnumeration.Count >= 2)
                    {
                        float min = -1000000.0f;
                        float max = 1000000.0f;
                        float step1 = 1.0f;
                        float step2 = 5.0f;
                        int decimals = 0;

                        float.TryParse(layout.CustomEnumeration[0], out min);
                        float.TryParse(layout.CustomEnumeration[1], out max);

                        if (layout.CustomEnumeration.Count >= 3)
                            int.TryParse(layout.CustomEnumeration[2], out decimals);
                        if (layout.CustomEnumeration.Count >= 4)
                            float.TryParse(layout.CustomEnumeration[3], out step1);
                        if (layout.CustomEnumeration.Count >= 5)
                            float.TryParse(layout.CustomEnumeration[4], out step2);

                        nudVector3X.Minimum =
                        nudVector3Y.Minimum =
                        nudVector3Z.Minimum =
                        nudNumerical.Minimum = (decimal)min;

                        nudVector3X.Maximum =
                        nudVector3Y.Maximum =
                        nudVector3Z.Maximum =
                        nudNumerical.Maximum = (decimal)max;

                        nudVector3X.Increment =
                        nudVector3Y.Increment =
                        nudVector3Z.Increment =
                        nudNumerical.Increment = (decimal)step1;

                        nudVector3X.IncrementAlternate =
                        nudVector3Y.IncrementAlternate =
                        nudVector3Z.IncrementAlternate =
                        nudNumerical.IncrementAlternate = (decimal)step2;

                        nudVector3X.DecimalPlaces =
                        nudVector3Y.DecimalPlaces =
                        nudVector3Z.DecimalPlaces =
                        nudNumerical.DecimalPlaces = decimals;
                    }
                    break;
                default:
                    break;
            }

            ResumeLayout();
            Visible = true;

        }

        public void SetToolTip(ToolTip toolTip, string caption)
        {
            var groups = Controls.OfType<DarkPanel>().Where(c => c.Name.StartsWith("group")).ToList();

            foreach (var group in groups)
                foreach (Control control in WinFormsUtils.AllSubControls(group))
                {
                    toolTip.SetToolTip(control, caption);

                    // HACK: there is no easy way of assigning tooltips to user controls...

                    if (control is DarkSearchableComboBox)
                        foreach (Control cnt in (control as DarkSearchableComboBox).Controls)
                            toolTip.SetToolTip(cnt, caption);
                }
        }

        public IHasLuaName LocateItem(NodeEditor editor)
        {
            if (cbList.Items.Count == 0 || cbList.SelectedIndex == -1)
                return null;

            switch (_argumentType)
            {
                case ArgumentType.Sinks:
                    return editor.CachedSinks.FirstOrDefault(i => i.LuaName == TextExtensions.Unquote((cbList.SelectedItem as ComboBoxItem).Value));
                case ArgumentType.Statics:
                    return editor.CachedStatics.FirstOrDefault(i => i.LuaName == TextExtensions.Unquote((cbList.SelectedItem as ComboBoxItem).Value));
                case ArgumentType.Moveables:
                    return editor.CachedMoveables.FirstOrDefault(i => i.LuaName == TextExtensions.Unquote((cbList.SelectedItem as ComboBoxItem).Value));
                case ArgumentType.Volumes:
                    return editor.CachedVolumes.FirstOrDefault(i => i.LuaName == TextExtensions.Unquote((cbList.SelectedItem as ComboBoxItem).Value));
                case ArgumentType.Cameras:
                    return editor.CachedCameras.FirstOrDefault(i => i.LuaName == TextExtensions.Unquote((cbList.SelectedItem as ComboBoxItem).Value));
                case ArgumentType.FlybyCameras:
                    return editor.CachedFlybys.FirstOrDefault(i => i.LuaName == TextExtensions.Unquote((cbList.SelectedItem as ComboBoxItem).Value));
                default:
                    return null;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using (var b = new SolidBrush(Colors.LightBackground))
                e.Graphics.FillRectangle(b, ClientRectangle);

            using (var p = new Pen(Colors.GreySelection, 1))
                e.Graphics.DrawRectangle(p, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            // HACK: Force background color for reluctant controls.

            base.OnBackColorChanged(e);
            //tabBoolean.BackColor = tableVector3.BackColor = BackColor;
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            // Absorb event
        }

        protected override void OnResize(EventArgs e)
        {
            // Absorb event
        }

        private void UnboxValue(string source)
        {
            switch (_argumentType)
            {
                case ArgumentType.Boolean:
                    {
                        bool result;
                        if (float.TryParse(source, out float parsedFloat))
                            result = parsedFloat == 0.0f ? false : true;
                        else if (!bool.TryParse(source, out result))
                            result = false;

                        cbBool.Checked = result;

                        BoxBoolValue();
                        break;
                    }
                case ArgumentType.Numerical:
                    {
                        float result;
                        if (bool.TryParse(source, out bool parsedBool))
                            result = parsedBool ? 1.0f : 0.0f;
                        else if (!(float.TryParse(source, out result)))
                            result = 0.0f;

                        try { nudNumerical.Value = (decimal)Math.Round(result, nudNumerical.DecimalPlaces); }
                        catch { nudNumerical.Value = (decimal)result < nudNumerical.Minimum ? nudNumerical.Minimum : nudNumerical.Maximum; }

                        BoxNumericalValue();
                        break;
                    }
                case ArgumentType.Vector2:
                    {
                        if (source.StartsWith(LuaSyntax.Vec2TypePrefix + LuaSyntax.BracketOpen) && source.EndsWith(LuaSyntax.BracketClose))
                            source = source.Substring(LuaSyntax.Vec2TypePrefix.Length + 1, source.Length - LuaSyntax.Vec2TypePrefix.Length - 2);

                        var floats = UnboxVectorValue(source);

                        for (int i = 0; i < 2; i++)
                        {
                            var currentFloat = 0.0f;
                            if (floats.Length > i)
                                currentFloat = floats[i];

                            try
                            {
                                switch (i)
                                {
                                    case 0: nudVector2X.Value = (decimal)currentFloat; break;
                                    case 1: nudVector2Y.Value = (decimal)currentFloat; break;
                                }
                            }
                            catch
                            {
                                switch (i)
                                {
                                    case 0: nudVector2X.Value = (decimal)currentFloat < nudVector2X.Minimum ? nudVector2X.Minimum : nudVector2X.Maximum; break;
                                    case 1: nudVector2Y.Value = (decimal)currentFloat < nudVector2Y.Minimum ? nudVector2Y.Minimum : nudVector2Y.Maximum; break;
                                }
                            }
                        }

                        BoxVector2Value();
                        break;
                    }
                case ArgumentType.Vector3:
                    {
                        if (source.StartsWith(LuaSyntax.Vec3TypePrefix + LuaSyntax.BracketOpen) && source.EndsWith(LuaSyntax.BracketClose))
                            source = source.Substring(LuaSyntax.Vec3TypePrefix.Length + 1, source.Length - LuaSyntax.Vec3TypePrefix.Length - 2);

                        var floats = UnboxVectorValue(source);

                        for (int i = 0; i < 3; i++)
                        {
                            var currentFloat = 0.0f;
                            if (floats.Length > i)
                                currentFloat = floats[i];

                            try
                            {
                                switch (i)
                                {
                                    case 0: nudVector3X.Value = (decimal)currentFloat; break;
                                    case 1: nudVector3Y.Value = (decimal)currentFloat; break;
                                    case 2: nudVector3Z.Value = (decimal)currentFloat; break;
                                }
                            }
                            catch
                            {
                                switch (i)
                                {
                                    case 0: nudVector3X.Value = (decimal)currentFloat < nudVector3X.Minimum ? nudVector3X.Minimum : nudVector3X.Maximum; break;
                                    case 1: nudVector3Y.Value = (decimal)currentFloat < nudVector3Y.Minimum ? nudVector3Y.Minimum : nudVector3Y.Maximum; break;
                                    case 2: nudVector3Z.Value = (decimal)currentFloat < nudVector3Z.Minimum ? nudVector3Z.Minimum : nudVector3Z.Maximum; break;
                                }
                            }
                        }

                        BoxVector3Value();
                        break;
                    }
                case ArgumentType.Color:
                    {
                        if (source.StartsWith(LuaSyntax.ColorTypePrefix + LuaSyntax.BracketOpen) && source.EndsWith(LuaSyntax.BracketClose))
                            source = source.Substring(LuaSyntax.ColorTypePrefix.Length + 1, source.Length - LuaSyntax.ColorTypePrefix.Length - 2);

                        var floats = UnboxVectorValue(source);
                        var bytes = new byte[3] { 0, 0, 0 };

                        for (int i = 0; i < 3; i++)
                            if (floats.Length > i)
                                bytes[i] = (byte)MathC.Clamp(floats[i], 0, 255);

                        panelColor.BackColor = Color.FromArgb(255, bytes[0], bytes[1], bytes[2]);

                        BoxColorValue();
                        break;
                    }
                case ArgumentType.Time:
                    {
                        if (source.StartsWith(LuaSyntax.TimeTypePrefix + LuaSyntax.BracketOpen + LuaSyntax.TableOpen) && source.EndsWith(LuaSyntax.TableClose + LuaSyntax.BracketClose))
                            source = source.Substring(LuaSyntax.TimeTypePrefix.Length + 2, source.Length - LuaSyntax.Vec3TypePrefix.Length - 4);

                        var floats = UnboxVectorValue(source);

                        for (int i = 0; i < 5; i++)
                        {
                            var currentFloat = 0.0f;
                            if (floats.Length > i)
                                currentFloat = floats[i];

                            try
                            {
                                switch (i)
                                {
                                    case 0: nudTimeHours.Value = (decimal)currentFloat; break;
                                    case 1: nudTimeMinutes.Value = (decimal)currentFloat; break;
                                    case 2: nudTimeSeconds.Value = (decimal)currentFloat; break;
                                    case 3: nudTimeCents.Value = (decimal)currentFloat; break;
                                }
                            }
                            catch
                            {
                                switch (i)
                                {
                                    case 0: nudTimeHours.Value = (decimal)currentFloat < nudTimeHours.Minimum ? nudTimeHours.Minimum : nudTimeHours.Maximum; break;
                                    case 1: nudTimeMinutes.Value = (decimal)currentFloat < nudTimeMinutes.Minimum ? nudTimeMinutes.Minimum : nudTimeMinutes.Maximum; break;
                                    case 2: nudTimeSeconds.Value = (decimal)currentFloat < nudTimeSeconds.Minimum ? nudTimeSeconds.Minimum : nudTimeSeconds.Maximum; break;
                                    case 3: nudTimeCents.Value = (decimal)currentFloat < nudTimeCents.Minimum ? nudTimeCents.Minimum : nudTimeCents.Maximum; break;
                                }
                            }
                        }

                        BoxTimeValue();
                        break;
                    }
                case ArgumentType.String:
                    {
                        tbString.Text = TextExtensions.UnescapeQuotes(TextExtensions.Unquote(source));
                        BoxStringValue();
                        break;
                    }
                default: // Lists
                    {
                        float potentialIndex;
                        if (bool.TryParse(source, out bool potentialValue))
                            potentialIndex = potentialValue ? 1 : 0;
                        else if (!float.TryParse(source, out potentialIndex))
                            potentialIndex = -1;

                        var item = cbList.Items.Cast<ComboBoxItem>().FirstOrDefault(i => i.Value == source);
                        if (item != null)
                            cbList.SelectedItem = item;
                        else if (cbList.Items.Count > potentialIndex && potentialIndex >= 0)
                            cbList.SelectedIndex = (int)potentialIndex;
                        else
                            cbList.SelectedIndex = cbList.Items.Count > 0 ? 0 : -1;

                        BoxListValue();
                        break;
                    }
            }
        }

        private float[] UnboxVectorValue(string source)
        {
            return source.Split(new string[] { LuaSyntax.Separator }, StringSplitOptions.None).Select(x =>
            {
                float result;
                if (float.TryParse(x.Trim(), out result))
                    return result;
                else
                    return 0.0f;
            }).ToArray();
        }

        private void BoxBoolValue()
        {
            _text = cbBool.Checked.ToString().ToLower();
            OnValueChanged();
        }

        private void BoxVector2Value()
        {
            var x = ((float)nudVector2X.Value).ToString();
            var y = ((float)nudVector2Y.Value).ToString();
            _text = LuaSyntax.Vec2TypePrefix + LuaSyntax.BracketOpen +
                    x + LuaSyntax.Separator +
                    y + LuaSyntax.BracketClose;
            OnValueChanged();
        }

        private void BoxVector3Value()
        {
            var x = ((float)nudVector3X.Value).ToString();
            var y = ((float)nudVector3Y.Value).ToString();
            var z = ((float)nudVector3Z.Value).ToString();
            _text = LuaSyntax.Vec3TypePrefix + LuaSyntax.BracketOpen +
                    x + LuaSyntax.Separator +
                    y + LuaSyntax.Separator +
                    z + LuaSyntax.BracketClose;
            OnValueChanged();
        }

        private void BoxTimeValue()
        {
            var h = ((int)nudTimeHours.Value).ToString();
            var m = ((int)nudTimeMinutes.Value).ToString();
            var s = ((int)nudTimeSeconds.Value).ToString();
            var c = ((int)nudTimeCents.Value).ToString();
            _text = LuaSyntax.TimeTypePrefix + LuaSyntax.BracketOpen + LuaSyntax.TableOpen +
                    h + LuaSyntax.Separator +
                    m + LuaSyntax.Separator +
                    s + LuaSyntax.Separator +
                    c + LuaSyntax.TableClose + LuaSyntax.BracketClose;
            OnValueChanged();
        }

        private void BoxColorValue()
        {
            _text = LuaSyntax.ColorTypePrefix + LuaSyntax.BracketOpen +
                    panelColor.BackColor.R.ToString() + LuaSyntax.Separator +
                    panelColor.BackColor.G.ToString() + LuaSyntax.Separator +
                    panelColor.BackColor.B.ToString() + LuaSyntax.BracketClose;
            OnValueChanged();
        }

        private void BoxNumericalValue()
        {
            _text = ((float)nudNumerical.Value).ToString();
            OnValueChanged();
        }

        private void BoxStringValue()
        {
            _text = TextExtensions.Quote(TextExtensions.EscapeQuotes(tbString.Text));
            OnValueChanged();
        }

        private void BoxListValue()
        {
            _text = (cbList.SelectedItem as ComboBoxItem)?.Value ?? string.Empty;
            OnValueChanged();
        }

        private bool IsLuaNameBased()
        {
            switch (_argumentType)
            {
                case ArgumentType.Moveables:
                case ArgumentType.Cameras:
                case ArgumentType.FlybyCameras:
                case ArgumentType.Sinks:
                case ArgumentType.Statics:
                case ArgumentType.Volumes:
                    return true;

                default:
                    return false;
            }
        }

        private bool IsWadSlotBased()
        {
            return (_argumentType == ArgumentType.WadSlots);
        }

        private bool IsPositionBased()
        {
            return (_argumentType == ArgumentType.Vector3);
        }

        private bool IsColorBased()
        {
            return (_argumentType == ArgumentType.Color);
        }

        private void rb_CheckedChanged(object sender, EventArgs e) => BoxBoolValue();
        private void nudNumerical_ValueChanged(object sender, EventArgs e) => BoxNumericalValue();
        private void nudTime_ValueChanged(object sender, EventArgs e) => BoxTimeValue();
        private void nudVector2_ValueChanged(object sender, EventArgs e) => BoxVector2Value();
        private void nudVector3_ValueChanged(object sender, EventArgs e) => BoxVector3Value();
        private void tbString_TextChanged(object sender, EventArgs e) => BoxStringValue();
        private void panelColor_BackColorChanged(object sender, EventArgs e) => BoxColorValue();
        private void cbList_SelectedIndexChanged(object sender, EventArgs e) => BoxListValue();

        private void panelColor_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            using (var colorDialog = new RealtimeColorDialog(Control.MousePosition.X,
                       Control.MousePosition.Y, c => { panelColor.BackColor = c; }))
            {
                var oldColor = panelColor.BackColor;
                colorDialog.Color = oldColor;
                colorDialog.FullOpen = true;
                if (colorDialog.ShowDialog(this) != DialogResult.OK)
                {
                    panelColor.BackColor = oldColor;
                    BoxColorValue();
                    return;
                }

                if (oldColor != colorDialog.Color)
                {
                    panelColor.BackColor = colorDialog.Color;
                    BoxColorValue();
                }
            }
        }

        private void butAction_Click(object sender, EventArgs e)
        {
            if (cbList.Items.Count == 0 || cbList.SelectedIndex == -1)
                return;

            switch (_argumentType)
            {
                case ArgumentType.SoundEffects:
                    OnSoundEffectPlayed(cbList.SelectedItem.ToString());
                    break;

                case ArgumentType.SoundTracks:
                    OnSoundtrackPlayed(TextExtensions.Unquote((cbList.SelectedItem as ComboBoxItem).Value));
                    break;

                default:
                    var item = LocateItem((Parent as VisibleNodeBase)?.Editor);
                    if (item != null)
                        OnLocatedItemFound(item);
                    break;
            }
        }

        private void draggableControl_DragEnter(object sender, DragEventArgs e)
        {
            if (IsLuaNameBased() && (e.Data.GetData(e.Data.GetFormats()[0]) as IHasLuaName) != null)
                e.Effect = DragDropEffects.Copy;

            if (IsWadSlotBased() && (e.Data.GetData(e.Data.GetFormats()[0]) as WadMoveable) != null)
                e.Effect = DragDropEffects.Copy;

            if (IsPositionBased() && (e.Data.GetData(e.Data.GetFormats()[0]) as PositionBasedObjectInstance) != null)
                e.Effect = DragDropEffects.Copy;

            if (IsColorBased() && (e.Data.GetData(e.Data.GetFormats()[0]) as IColorable) != null)
                e.Effect = DragDropEffects.Copy;
        }

        private void cbList_DragDrop(object sender, DragEventArgs e)
        {
            if (!IsLuaNameBased() && !IsWadSlotBased())
                return;

            if (IsLuaNameBased() && (e.Data.GetData(e.Data.GetFormats()[0]) as IHasLuaName) != null)
            {
                var item = e.Data.GetData(e.Data.GetFormats()[0]) as PositionAndScriptBasedObjectInstance;

                if (string.IsNullOrEmpty(item.LuaName))
                    return;

                var list = cbList.Items.OfType<ComboBoxItem>();
                var index = list.IndexOf(i => i.Value == TextExtensions.Quote(item.LuaName));

                if (index == -1 && item is MoveableInstance)
                {
                    var name = TrCatalog.GetMoveableName(item.Room.Level.Settings.GameVersion, (item as MoveableInstance).WadObjectId.TypeId);
                    index = list.IndexOf(i => i.DisplayText == name);
                }

                if (index != -1)
                    cbList.SelectedIndex = index;

                return;
            }

            if (IsWadSlotBased() && (e.Data.GetData(e.Data.GetFormats()[0]) as WadMoveable) != null)
            {
                var item = e.Data.GetData(e.Data.GetFormats()[0]) as WadMoveable;

                var list = cbList.Items.OfType<ComboBoxItem>();
                var name = LuaSyntax.ObjectIDPrefix + LuaSyntax.Splitter + TrCatalog.GetMoveableName(TRVersion.Game.TombEngine, item.Id.TypeId);
                var index = list.IndexOf(i => i.Value == name);

                if (index != -1)
                    cbList.SelectedIndex = index;

                return;
            }
        }

        private void vector3Control_DragDrop(object sender, DragEventArgs e)
        {
            if ((e.Data.GetData(e.Data.GetFormats()[0]) as PositionBasedObjectInstance) == null)
                return;

            var item = e.Data.GetData(e.Data.GetFormats()[0]) as PositionBasedObjectInstance;

            nudVector3X.Value = (decimal)item.WorldPosition.X;
            nudVector3Y.Value = (decimal)-item.WorldPosition.Y;
            nudVector3Z.Value = (decimal)item.WorldPosition.Z;
        }

        private void panelColor_DragDrop(object sender, DragEventArgs e)
        {
            if ((e.Data.GetData(e.Data.GetFormats()[0]) as IColorable) == null)
                return;

            var item = e.Data.GetData(e.Data.GetFormats()[0]) as IColorable;
            panelColor.BackColor = (item.Color * 0.5f).ToWinFormsColor();
        }

        private void butMultiline_Click(object sender, EventArgs e)
        {
            var multiline = new FormMultilineText(Cursor.Position, tbString);
            multiline.Show(this.FindForm());
        }
    }
}
