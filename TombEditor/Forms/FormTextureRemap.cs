using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib;
using TombLib.LevelData;
using TombLib.LevelData.SectorEnums;
using TombLib.Utils;

namespace TombEditor.Forms
{
    public partial class FormTextureRemap : DarkForm
    {
        private Editor _editor;

        public FormTextureRemap(Editor editor)
        {
            _editor = editor;
            InitializeComponent();
            sourceTextureMap.FormParent = destinationTextureMap.FormParent = this;

            // Set window property handlers
            Configuration.ConfigureWindow(this, _editor.Configuration);

            LevelTexture firstTexture = editor.Level.Settings.Textures.FirstOrDefault();
            comboSourceTexture.Items.AddRange(editor.Level.Settings.Textures.ToArray());
            comboSourceTexture.SelectedItem = firstTexture;
            comboDestinationTexture.Items.AddRange(editor.Level.Settings.Textures.ToArray());
            comboDestinationTexture.SelectedItem = firstTexture;
            if (firstTexture != null)
                sourceTextureMap.End = firstTexture.Image.Size;
        }

        private bool SourceContains(Vector2 texCoord)
        {
            return sourceTextureMap.Start.X <= texCoord.X && sourceTextureMap.Start.Y <= texCoord.Y &&
                   sourceTextureMap.End.X >= texCoord.X && sourceTextureMap.End.Y >= texCoord.Y;
        }

        private bool SourceEquals(Rectangle2 rect)
        {
            return sourceTextureMap.Start.X == rect.Start.X && sourceTextureMap.Start.Y == rect.Start.Y &&
                   sourceTextureMap.End.X == rect.End.X && sourceTextureMap.End.Y == rect.End.Y;
        }

        private AnimatedTextureFrame RemapTexture(AnimatedTextureFrame source, float scale)
        {
            var dummyTexture = new TextureArea() { TexCoord0 = source.TexCoord0, TexCoord1 = source.TexCoord1, TexCoord2 = source.TexCoord2, TexCoord3 = source.TexCoord3 };
            dummyTexture = RemapTexture(dummyTexture, scale);
            source.TexCoord0 = dummyTexture.TexCoord0;
            source.TexCoord1 = dummyTexture.TexCoord1;
            source.TexCoord2 = dummyTexture.TexCoord2;
            source.TexCoord3 = dummyTexture.TexCoord3;
            return source;
        }

        private TextureArea RemapTexture(TextureArea source, float scale)
        {
            var bounds = source.GetRect();

            // Limit scale if we already reached minimum size of 1 px by any side
            if (bounds.Width * scale < 1.0f || bounds.Height * scale < 1.0f) scale = 1.0f;

            // Scale all coords
            for (int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 0:
                        if (source.TexCoord0.X != bounds.X0) source.TexCoord0.X = bounds.X0 + (source.TexCoord0.X - bounds.X0) * scale;
                        if (source.TexCoord0.Y != bounds.Y0) source.TexCoord0.Y = bounds.Y0 + (source.TexCoord0.Y - bounds.Y0) * scale;
                        break;
                    case 1:
                        if (source.TexCoord1.X != bounds.X0) source.TexCoord1.X = bounds.X0 + (source.TexCoord1.X - bounds.X0) * scale;
                        if (source.TexCoord1.Y != bounds.Y0) source.TexCoord1.Y = bounds.Y0 + (source.TexCoord1.Y - bounds.Y0) * scale;
                        break;
                    case 2:
                        if (source.TexCoord2.X != bounds.X0) source.TexCoord2.X = bounds.X0 + (source.TexCoord2.X - bounds.X0) * scale;
                        if (source.TexCoord2.Y != bounds.Y0) source.TexCoord2.Y = bounds.Y0 + (source.TexCoord2.Y - bounds.Y0) * scale;
                        break;
                    case 3:
                        if (source.TexCoord3.X != bounds.X0) source.TexCoord3.X = bounds.X0 + (source.TexCoord3.X - bounds.X0) * scale;
                        if (source.TexCoord3.Y != bounds.Y0) source.TexCoord3.Y = bounds.Y0 + (source.TexCoord3.Y - bounds.Y0) * scale;
                        break;
                }
            }

            var distance = ((bounds.Start - sourceTextureMap.Start) * scale) - (bounds.Start - sourceTextureMap.Start);
            var shift = destinationTextureMap.Start - sourceTextureMap.Start;

            // Shift coords with respect to distance from enclosing region start
            source.TexCoord0 += distance + shift;
            source.TexCoord1 += distance + shift;
            source.TexCoord2 += distance + shift;
            source.TexCoord3 += distance + shift;

            return source;
        }

        private void UpdateScaling()
        {
            var destEnd = destinationTextureMap.Start + ((sourceTextureMap.End - sourceTextureMap.Start) * (float)scalingFactor.Value);
            if (destEnd.X > destinationTextureMap.VisibleTexture.Image.Size.X ||
                destEnd.Y > destinationTextureMap.VisibleTexture.Image.Size.Y)
            {
                var maxFactor = Math.Floor(Math.Min((destinationTextureMap.VisibleTexture.Image.Size.X - destinationTextureMap.Start.X) / sourceTextureMap.AreaSize.X,
                                                    (destinationTextureMap.VisibleTexture.Image.Size.Y - destinationTextureMap.Start.Y) / sourceTextureMap.AreaSize.Y));

                if (maxFactor < 1) maxFactor = 1; // Reset to default scaling if we go below zero during rounding

                scalingFactor.Value = (decimal)maxFactor;
                return; // Dest map will be invalidated in recursive call
            }
            else
            {
                destinationTextureMap.Scaling = (float)scalingFactor.Value;
                destinationTextureMap.Invalidate();
            }
        }

        private void cbUntextureCompletely_CheckedChanged(object sender, EventArgs e)
        {
            destinationPanel.Enabled = !cbUntextureCompletely.Checked;
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            // Gather some data
            Level level = _editor.Level;
            LevelTexture sourceTexture = comboSourceTexture.SelectedItem as LevelTexture;
            LevelTexture destinationTexture = comboDestinationTexture.SelectedItem as LevelTexture;
            bool untextureCompletely = cbUntextureCompletely.Checked;
            IEnumerable<Room> relevantRooms;
            if (cbRestrictToSelectedRooms.Checked)
                relevantRooms = _editor.SelectedRooms;
            else
                relevantRooms = _editor.Level.ExistingRooms;

            if (sourceTexture == null || destinationTexture == null)
            {
                DarkMessageBox.Show(this, "Source or destination texture may not be unset.", "Problem", MessageBoxIcon.Exclamation);
                return;
            }

            // Prepare undo
            var undoList = new List<UndoRedoInstance>();

            // Room textures
            int roomTextureCount = 0;
            foreach (Room room in relevantRooms)
                foreach (Sector sector in room.Sectors)
                    foreach (SectorFace face in sector.GetFaceTextures().Keys)
                    {
                        var currentTextureArea = sector.GetFaceTexture(face);
                        if (currentTextureArea.Texture == sourceTexture &&
                            SourceContains(currentTextureArea.TexCoord0) &&
                            SourceContains(currentTextureArea.TexCoord1) &&
                            SourceContains(currentTextureArea.TexCoord2) &&
                            SourceContains(currentTextureArea.TexCoord3))
                        {
                            // Add current room to undo if not already added
                            if (!undoList.Any(item => ((EditorUndoRedoInstance)item).Room == room))
                                undoList.Add(new GeometryUndoInstance(_editor.UndoManager, room));

                            // Replace texture
                            currentTextureArea = RemapTexture(currentTextureArea, destinationTextureMap.Scaling);
                            currentTextureArea.Texture = destinationTexture;

                            // Remove texture if untexture option is set
                            if (untextureCompletely)
                                currentTextureArea.Texture = null;

                            // Replace parent area if equal
                            if (SourceEquals(currentTextureArea.ParentArea))
                                currentTextureArea.ParentArea = new Rectangle2(destinationTextureMap.Start, destinationTextureMap.End);
                            else
                                currentTextureArea.ParentArea = Rectangle2.Zero;

                            sector.SetFaceTexture(face, currentTextureArea);
                            ++roomTextureCount;
                        }
                    }

            // Push undo
            if (undoList.Count > 0)
                _editor.UndoManager.Push(undoList);

            // Animated textures
            int animatedTextureCount = 0;
            if (cbRemapAnimTextures.Checked)
                foreach (AnimatedTextureSet set in level.Settings.AnimatedTextureSets)
                {
                    var framesToRemove = new List<AnimatedTextureFrame>();
                    foreach (AnimatedTextureFrame frame in set.Frames)
                    {
                        if (frame.Texture == sourceTexture &&
                            SourceContains(frame.TexCoord0) &&
                            SourceContains(frame.TexCoord1) &&
                            SourceContains(frame.TexCoord2) &&
                            SourceContains(frame.TexCoord3))
                        {
                            var newFrame = RemapTexture(frame, destinationTextureMap.Scaling);
                            frame.TexCoord0 = newFrame.TexCoord0;
                            frame.TexCoord1 = newFrame.TexCoord1;
                            frame.TexCoord2 = newFrame.TexCoord2;
                            frame.TexCoord3 = newFrame.TexCoord3;
                            frame.Texture = destinationTexture;
                            ++animatedTextureCount;
                            if (untextureCompletely)
                                framesToRemove.Add(frame);
                        }
                    }
                    set.Frames.RemoveAll(frame => framesToRemove.Contains(frame));
                }

            // Send out updates
            Parallel.ForEach(relevantRooms, room => room.Rebuild(_editor.ShouldRelight, _editor.Configuration.Rendering3D_HighQualityLightPreview));
            foreach (Room room in relevantRooms)
                _editor.RoomTextureChange(room);

            // Inform user
            var message = "Successfully remapped " + roomTextureCount + " textures in rooms";
            if (cbRemapAnimTextures.Checked == true)
                message += ", " + animatedTextureCount + " in texture animations.).\n";
            else
                message += ".";
            statusLabel.Text = message;
        }

        private void butCancel_Click(object sender, EventArgs e) => Close();

        private void comboSourceTexture_DropDown(object sender, EventArgs e)
        {
            // Make the combo box as wide as possible
            Point screenPointLeft = comboSourceTexture.PointToScreen(new Point(0, 0));
            Rectangle screenPointRight = Screen.GetBounds(comboSourceTexture.PointToScreen(new Point(0, comboSourceTexture.Width)));
            comboSourceTexture.DropDownWidth = screenPointRight.Right - screenPointLeft.X - 15; // Margin
        }

        private void comboDestinationTexture_DropDown(object sender, EventArgs e)
        {
            // Make the combo box as wide as possible
            Point screenPointLeft = comboDestinationTexture.PointToScreen(new Point(0, 0));
            Rectangle screenPointRight = Screen.GetBounds(comboDestinationTexture.PointToScreen(new Point(0, comboDestinationTexture.Width)));
            comboDestinationTexture.DropDownWidth = screenPointRight.Right - screenPointLeft.X - 15; // Margin
        }

        private void comboSourceTexture_SelectedValueChanged(object sender, EventArgs e)
        {
            if (sourceTextureMap.VisibleTexture != comboSourceTexture.SelectedItem)
            {
                sourceTextureMap.ResetVisibleTexture(comboSourceTexture.SelectedItem as LevelTexture);

                // Reset selection if out of bounds

                var dimRect = new Rectangle2(Vector2.Zero, sourceTextureMap.VisibleTexture.Image.Size);
                if (!dimRect.Contains(sourceTextureMap.Start) || !dimRect.Contains(sourceTextureMap.End))
                {
                    sourceTextureMap.Start = Vector2.Zero;
                    sourceTextureMap.End = sourceTextureMap.VisibleTexture.Image.Size;
                    destinationTextureMap.Invalidate();
                }
            }
        }

        private void comboDestinationTexture_SelectedValueChanged(object sender, EventArgs e)
        {
            if (destinationTextureMap.VisibleTexture != comboDestinationTexture.SelectedItem)
                destinationTextureMap.ResetVisibleTexture(comboDestinationTexture.SelectedItem as LevelTexture);
        }

        private void scalingFactor_ValueChanged(object sender, EventArgs e) => UpdateScaling();

        public class PanelTextureMapForRemap : Controls.PanelTextureMap
        {
            public FormTextureRemap FormParent { get; set; }
            public bool IsDestination { get; set; }

            public float Scaling { get; set; } = 1.0f;
            public Vector2 AreaSize => Vector2.Abs(End - Start) * Scaling;

            public Vector2 Start { get; set; }
            public Vector2 End
            {
                get { return _end; }
                set
                {
                    _end = value;

                    if (!IsDestination)
                    {
                        bool clamped = false;

                        var size = End - Start;
                        var destStart = FormParent.destinationTextureMap.Start;
                        var destEnd = destStart + size;

                        if (destEnd.X > VisibleTexture.Image.Size.X)
                        {
                            clamped = true;
                            destStart.X -= destEnd.X - VisibleTexture.Image.Size.X;
                        }

                        if (destEnd.Y > VisibleTexture.Image.Size.Y)
                        {
                            clamped = true;
                            destStart.Y -= destEnd.Y - VisibleTexture.Image.Size.Y;
                        }

                        if (clamped)
                            FormParent.destinationTextureMap.Start = destStart;
                    }
                }
            }
            private Vector2 _end;

            private void DrawArea(Vector2 start, Vector2 end, Graphics g)
            {
                PointF drawStart = ToVisualCoord(start);
                PointF drawEnd = ToVisualCoord(end);
                RectangleF drawArea = RectangleF.FromLTRB(drawStart.X, drawStart.Y, drawEnd.X, drawEnd.Y);

                using (var brush = new SolidBrush(Color.FromArgb(30, 220, 210, 20)))
                    g.FillRectangle(brush, drawArea);
                using (var pen = new Pen(Color.FromArgb(255, 220, 210, 20), 3))
                    g.DrawRectangle(pen, drawArea);
            }

            protected override void OnPaintSelection(PaintEventArgs e)
            {
                if (IsDestination)
                    DrawArea(FormParent.destinationTextureMap.Start, FormParent.destinationTextureMap.Start + FormParent.sourceTextureMap.AreaSize * FormParent.destinationTextureMap.Scaling, e.Graphics);
                else
                    DrawArea(FormParent.sourceTextureMap.Start, FormParent.sourceTextureMap.End, e.Graphics);
            }

            private Vector2 Quantize2(Vector2 texCoord, bool end)
            {
                var selectionPrecision = GetSelectionPrecision(true);

                if (selectionPrecision.Precision == 0.0f)
                    return texCoord;

                texCoord /= selectionPrecision.Precision;

                if (end)
                    texCoord = new Vector2((float)Math.Ceiling(texCoord.X), (float)Math.Ceiling(texCoord.Y));
                else
                    texCoord = new Vector2((float)Math.Floor(texCoord.X), (float)Math.Floor(texCoord.Y));

                texCoord *= selectionPrecision.Precision;
                return texCoord;
            }

            private void ProcessMouse(MouseEventArgs e)
            {
                if (!IsDestination && e.Button == MouseButtons.Left)
                {
                    FormParent.sourceTextureMap.End = Quantize2(FromVisualCoord(e.Location, true), true);
                    FormParent.UpdateScaling();

                    Invalidate();
                    FormParent.destinationTextureMap.Invalidate();
                }
                else if (IsDestination && e.Button == MouseButtons.Left)
                {
                    var newStart = Quantize2(FromVisualCoord(e.Location, false), false);
                    var size = (FormParent.sourceTextureMap.End - FormParent.sourceTextureMap.Start) * FormParent.destinationTextureMap.Scaling;

                    var finalStart = FormParent.destinationTextureMap.Start;

                    if (newStart.X >= 0 && newStart.X + size.X <= VisibleTexture.Image.Size.X)
                        finalStart.X = newStart.X;

                    if (newStart.Y >= 0 && newStart.Y + size.Y <= VisibleTexture.Image.Size.Y)
                        finalStart.Y = newStart.Y;

                    FormParent.destinationTextureMap.Start = finalStart;
                    FormParent.destinationTextureMap.End = finalStart + size;

                    Invalidate();
                }
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                if (!IsDestination)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        var newStart = Quantize2(FromVisualCoord(e.Location, false), false);
                        if (newStart.X >= 0 && newStart.Y >= 0 &&
                            newStart.X < VisibleTexture.Image.Size.X && newStart.Y < VisibleTexture.Image.Size.Y)
                        {
                            FormParent.sourceTextureMap.Start = newStart;
                            Invalidate();
                            FormParent.destinationTextureMap.Invalidate();
                        }
                    }
                }

                base.OnMouseDown(e);
                ProcessMouse(e);
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                ProcessMouse(e);
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);
                ProcessMouse(e);
            }
        }
    }
}
