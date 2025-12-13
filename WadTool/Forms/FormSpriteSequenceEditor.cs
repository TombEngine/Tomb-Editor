using DarkUI.Controls;
using DarkUI.Forms;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormSpriteSequenceEditor : DarkForm
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public WadSpriteSequence SpriteSequence { get; }
        public Wad2 Wad { get; }

        private WadToolClass _tool;
        private string _currentPath;
        private readonly Cache<WadTexture, Bitmap> _imageCache = new Cache<WadTexture, Bitmap>(1024, sprite => sprite.Image.ToBitmap());

        private bool _lockAlignment = false;

        public FormSpriteSequenceEditor(WadToolClass tool, Wad2 wad, WadSpriteSequence spriteSequence)
        {
            InitializeComponent();

            SpriteSequence = spriteSequence;
            Wad = wad;

            _tool = tool;
            _currentPath = Wad.FileName;

            // Load data
            Text = "Sprite sequence '" + spriteSequence.Id.ToString(Wad.GameVersion) + "'";
            dataGridView.DataSource = new BindingList<WadSprite>(new List<WadSprite>(spriteSequence.Sprites));
            dataGridViewControls.CreateNewRow = newObject;
            dataGridViewControls.DataGridView = dataGridView;
            dataGridViewControls.Enabled = true;

            // Refresh initially
            if (dataGridView.Rows.Count > 0)
                dataGridView.Rows[0].Selected = true;
            SelectSprite();

            cmbVerAdj.SelectedIndex = 2;
            cmbHorAdj.SelectedIndex = 1;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                _imageCache.Dispose();
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private object newObject()
        {
            using (var fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = ImageC.FileExtensions.GetFilter();
                fileDialog.Multiselect = true;
                if (!string.IsNullOrWhiteSpace(_currentPath))
                    try
                    {
                        fileDialog.InitialDirectory = Path.GetDirectoryName(_currentPath);
                        fileDialog.FileName = Path.GetFileName(_currentPath);
                    }
                    catch { }
                fileDialog.Title = "Select sprite files that you want to see imported.";

                DialogResult dialogResult = fileDialog.ShowDialog(this);
                _currentPath = fileDialog.FileName;
                if (dialogResult != DialogResult.OK)
                    return null;

                // Load sprites
                List<WadSprite> sprites = new List<WadSprite>();
                foreach (string fileName in fileDialog.FileNames)
                {
                Retry:
                    ;
                    try
                    {
                        var newSprite = new WadSprite { Texture = new WadTexture(ImageC.FromFile(fileName)) };
                        newSprite.RecalculateAlignment();
                        newSprite.Texture.Image.SetColorDataForTransparentPixels(new ColorC(0, 0, 0));
                        sprites.Add(newSprite);
                    }
                    catch (Exception exc)
                    {
                        logger.Error(exc, "Unable to open file '" + fileName + "'.");
                        switch (DarkMessageBox.Show(this, "Unable to load sprite from file '" + fileName + "'. " + exc, "Unable to load sprite.",
                            fileDialog.FileNames.Length == 1 ? MessageBoxButtons.RetryCancel : MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error,
                            fileDialog.FileNames.Length == 1 ? MessageBoxDefaultButton.Button2 : MessageBoxDefaultButton.Button1))
                        {
                            case DialogResult.Ignore:
                                continue;
                            case DialogResult.Retry:
                                goto Retry;
                        }
                        return null;
                    }
                }
                return sprites;
            }
        }

        private void SelectSprite()
        {
            if (dataGridView.SelectedRows.Count > 0 && dataGridView.SelectedRows[0].Index >= 0)
            {
                var sprite = (WadSprite)dataGridView.SelectedRows[0].DataBoundItem;
                picSprite.Image = _imageCache[sprite.Texture];

                _lockAlignment = true;
                nudL.Value = sprite.Alignment.X0;
                nudR.Value = sprite.Alignment.X1;
                nudT.Value = sprite.Alignment.Y0;
                nudB.Value = sprite.Alignment.Y1;
                _lockAlignment = false;
            }
        }

        private void UpdateAlignment()
        {
            if (!_lockAlignment && dataGridView.SelectedRows.Count > 0 && dataGridView.SelectedRows[0].Index >= 0)
            {
                var sprite = (WadSprite)dataGridView.SelectedRows[0].DataBoundItem;
                sprite.Alignment = new TombLib.RectangleInt2((int)nudL.Value, (int)nudT.Value, (int)nudR.Value, (int)nudB.Value);
                dataGridView.EditableRowCollection[dataGridView.SelectedRows[0].Index] = sprite;
            }
        }

        private void btExport_Click(object sender, EventArgs e)
        {
            using (var fileDialog = new SaveFileDialog())
            {
                fileDialog.Filter = ImageC.SaveFileFileExtensions.GetFilter(true);
                if (!string.IsNullOrWhiteSpace(_currentPath))
                    try
                    {
                        fileDialog.InitialDirectory = Path.GetDirectoryName(_currentPath);
                        fileDialog.FileName = "Untitled";
                    }
                    catch { }
                fileDialog.Title = "Choose a sprite file name.";
                fileDialog.AddExtension = true;

                DialogResult dialogResult = fileDialog.ShowDialog(this);
                _currentPath = fileDialog.FileName;
                if (dialogResult != DialogResult.OK)
                    return;

                // Save sprites
                try
                {
                    foreach (DataGridViewRow row in dataGridView.SelectedRows)
                    {
                        string fileName = fileDialog.FileName;
                        if (dataGridView.SelectedRows.Count > 1)
                            fileName = Path.Combine(Path.GetDirectoryName(fileName),
                                Path.GetFileNameWithoutExtension(fileName) + row.Index.ToString("0000") + Path.GetExtension(fileName));
                        ((WadSprite)row.DataBoundItem).Texture.Image.SaveToFile(fileName);
                    }
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Unable to save file '" + fileDialog.FileName + "'.");
                    DarkMessageBox.Show(this, "Unable to save sprite. " + exc, "Saving sprite failed.", MessageBoxIcon.Error);
                }
            }
        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            var selected = dataGridView.SelectedRows.Count > 0;
            btExport.Enabled  = selected;
            butRecalc.Enabled = selected;
            cmbHorAdj.Enabled = selected;
            cmbVerAdj.Enabled = selected;
            nudScale.Enabled  = selected;
            nudL.Enabled      = selected;
            nudT.Enabled      = selected;
            nudR.Enabled      = selected;
            nudB.Enabled      = selected;

            if (dataGridView.SelectedRows.Count > 0)
                SelectSprite();
        }

        private void dataGridView_Click(object sender, EventArgs e)
        {
            // Update big image view
            if (dataGridView.SelectedRows.Count <= 0)
                return;
        }

        private void dataGridView_CellFormattingSafe(object sender, DarkDataGridViewSafeCellFormattingEventArgs e)
        {
            if (!(e.Row.DataBoundItem is WadSprite))
                return;
            WadSprite item = (WadSprite)e.Row.DataBoundItem;

            if (e.Column.Name == SizeColumn.Name)
                e.Value = item.Texture.Image.Size.ToString();
            else if (e.Column.Name == IdColumn.Name)
                e.Value = e.RowIndex.ToString();
            else if (e.Column.Name == PreviewColumn.Name)
                e.Value = _imageCache[item.Texture];
            e.FormattingApplied = true;
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            // Update data
            SpriteSequence.Sprites.Clear();
            SpriteSequence.Sprites.AddRange((IEnumerable<WadSprite>)dataGridView.DataSource);

            _tool.ToggleUnsavedChanges();

            // Close
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButReplaceSprite_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0)
                return;
            var row = dataGridView.SelectedRows[0];

            using (var fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = ImageC.FileExtensions.GetFilter();
                fileDialog.Multiselect = false;
                if (!string.IsNullOrWhiteSpace(_currentPath))
                    try
                    {
                        fileDialog.InitialDirectory = Path.GetDirectoryName(_currentPath);
                        fileDialog.FileName = Path.GetFileName(_currentPath);
                    }
                    catch { }
                fileDialog.Title = "Select sprite file that you want to see imported.";

                DialogResult dialogResult = fileDialog.ShowDialog(this);
                _currentPath = fileDialog.FileName;
                if (dialogResult != DialogResult.OK)
                    return;

                // Load sprites
                WadSprite sprite;
                try
                {
                    sprite = new WadSprite { Texture = new WadTexture(ImageC.FromFile(fileDialog.FileName)) };
                    sprite.RecalculateAlignment();
                    sprite.Texture.Image.SetColorDataForTransparentPixels(new ColorC(0, 0, 0));
                    dataGridView.EditableRowCollection[row.Index] = sprite;
                    dataGridView.Refresh();
                    dataGridView.Invalidate();
                    SelectSprite();
                    nudScale.Value = 1;
                    cmbHorAdj.SelectedIndex = 1;
                    cmbVerAdj.SelectedIndex = 2;
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Unable to open file '" + fileDialog.FileName + "'.");
                    DarkMessageBox.Show(this, "Unable to load sprite from file '" + fileDialog.FileName + "'. " + exc, "Unable to load sprite",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void RecalculateAlignment()
        {
            if (dataGridView.SelectedRows.Count > 0 && dataGridView.SelectedRows[0].Index >= 0)
            {
                var sprite = (WadSprite)dataGridView.SelectedRows[0].DataBoundItem;
                sprite.RecalculateAlignment((WadSprite.HorizontalAlignment)cmbHorAdj.SelectedIndex,
                                            (WadSprite.VerticalAlignment)cmbVerAdj.SelectedIndex,
                                            (float)nudScale.Value);
                dataGridView.EditableRowCollection[dataGridView.SelectedRows[0].Index] = sprite;
                SelectSprite();
            }
        }

        private void nudAlignment_ValueChanged(object sender, EventArgs e) => UpdateAlignment();
        private void butRecalc_Click(object sender, EventArgs e) => RecalculateAlignment();
    }
}
