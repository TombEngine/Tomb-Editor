using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using TombLib.Rendering;
using TombLib.Forms;
using TombLib.Utils;
using static TombLib.LevelData.TRVersion;
using TombLib.LevelData;

namespace TombEditor.Forms
{
    public partial class FormOptions : FormOptionsBase
    {
        private readonly Editor _editor;
        private bool _lockColorScheme;

        public FormOptions(Editor editor) : base(editor.Configuration, new List<string>() { "UI_ColorScheme" })
        {
            _editor = editor;
            _editor.EditorEventRaised += EditorEventRaised;

            InitializeComponent();
            InitializeDialog();
            this.SetActualSize(630, 570);
            this.LockWidth();
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.ConfigurationChangedEvent)
                ReadConfigIntoControls(this);
        }

        protected override void InitializeDialog()
        {
            // Filter out non-TrueType fonts by catching an exception on font creation.
            foreach (var font in FontFamily.Families)
            {
                try
                {
                    var testFont = new FontFamily(font.Name);
                    cmbRendering3DFont.Items.Add(font.Name);
                }
                catch
                {
                    // Absorb exception for non-TrueType fonts.
                }
            }

            // Populate versions
            cmbGameVersion.Items.AddRange(AllVersions.Cast<object>().ToArray());

            // Populate events
            cmbVolumeEvent.Items.AddRange(Event.VolumeEventTypes.Select(e => e.ToString().SplitCamelcase()).ToArray());
            cmbGlobalEvent.Items.AddRange(Event.GlobalEventTypes.Select(e => e.ToString().SplitCamelcase()).ToArray());

            // Populate color scheme presets
            typeof(ColorScheme)
                .GetFields()
                .Where(f => f.FieldType == typeof(ColorScheme))
                .ToList()
                .ForEach(item => cmbColorScheme.Items.Add(item.Name));

            // Reset color scheme combo if color was changed
            foreach (var panel in AllOptionControls(this).Where(c => c is DarkPanel))
                panel.BackColorChanged += (sender, e) =>
                {
                    if (!_lockColorScheme && panel.Parent.Text == "Color scheme")
                        cmbColorScheme.SelectedIndex = -1;
                };

            base.InitializeDialog();
        }

        protected override void WriteConfigFromControls()
        {
            base.WriteConfigFromControls();
            _editor.ConfigurationChange();
        }

        private void cmbColorScheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbColorScheme.SelectedIndex == -1) return;
            var config = _editor.Configuration;

            _lockColorScheme = true;
            foreach (var prop in typeof(ColorScheme).GetFields())
                if (prop.FieldType == typeof(ColorScheme) && prop.Name == cmbColorScheme.SelectedItem.ToString())
                {
                    foreach (var field in typeof(ColorScheme).GetFields().Where(f => f.FieldType == typeof(Vector4)))
                        foreach (var panel in AllOptionControls(groupColorScheme).OfType<DarkPanel>())
                            if (panel.Tag.ToString() == field.Name)
                                panel.BackColor = ((Vector4)field.GetValue(prop.GetValue(config.UI_ColorScheme))).ToWinFormsColor();
                    break;
                }
            _lockColorScheme = false;
        }
    }
}
