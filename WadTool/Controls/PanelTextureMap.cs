using DarkUI.Config;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media.TextFormatting;
using TombLib.Controls;
using TombLib.Utils;

namespace WadTool.Controls
{
    public class PanelTextureMap : TextureMapBase
    {
        private WadToolClass _tool;

        protected override float TileSelectionSize => 32.0f;
        protected override bool ResetAttributesOnNewSelection => false;
        protected override bool MouseWheelMovesTheTextureInsteadOfZooming => _tool?.Configuration.MeshEditor_MouseWheelMovesTheTextureInsteadOfZooming ?? false;
        protected override float NavigationSpeedKeyMove => 100.0f;
        protected override float NavigationSpeedKeyZoom => 0.15f;
        protected override float NavigationSpeedMouseZoom => (_tool?.Configuration.RenderingItem_NavigationSpeedMouseZoom ?? 1.0f) * 0.00225f;
        protected override float NavigationSpeedMouseWheelZoom => (_tool?.Configuration.RenderingItem_NavigationSpeedMouseWheelZoom ?? 1.0f) * 0.00025f;
        protected override float NavigationMaxZoom => 2000.0f;
        protected override float NavigationMinZoom => 0.5f;
        protected override bool DrawSelectionDirectionIndicators => true;

        public PanelTextureMap() : base() { }

        public void Initialize(WadToolClass tool)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                _tool = tool;
                _tool.EditorEventRaised += EditorEventRaised;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _tool != null)
                _tool.EditorEventRaised -= EditorEventRaised;
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            //if (obj is WadToolClass.MeshEditorTextureChangedEvent)
            //    Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var externalTextureMissing = !string.IsNullOrEmpty(VisibleTexture.AbsolutePath) && !File.Exists(VisibleTexture.AbsolutePath);

            if (!externalTextureMissing)
                return;

            var rect = new Rectangle(ClientRectangle.X, ClientRectangle.Y, 
                                     ClientRectangle.Width - _scrollSize, ClientRectangle.Height - Font.Height - _scrollSize);

            var message = "External texture not found:\n" + VisibleTexture.AbsolutePath;
            var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };
            var stringSize = e.Graphics.MeasureString(message, Font, rect.Size, format);

            var textRect = new RectangleF(rect.X + (rect.Width - stringSize.Width) / 2, rect.Y + rect.Height - stringSize.Height,
                                          stringSize.Width, stringSize.Height);
            textRect.Inflate(6, 6);

            using (var brush = new SolidBrush(ForeColor.MixWith(Color.DarkRed, 0.55)))
                e.Graphics.FillRectangle(brush, textRect);

            using (var brush = new SolidBrush(Colors.LightestBackground))
                e.Graphics.DrawString("External texture not found:\n" + VisibleTexture.AbsolutePath, Font, brush, rect, format);
        }
    }
}
