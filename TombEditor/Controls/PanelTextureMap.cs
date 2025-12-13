using DarkUI.Config;
using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using TombEditor.Controls.ContextMenus;
using TombLib.Controls;
using TombLib.LevelData;
using TombLib.Utils;
using RectangleF = System.Drawing.RectangleF;

namespace TombEditor.Controls
{
    public class PanelTextureMap : TextureMapBase
    {
        protected readonly Editor _editor;

        public new LevelTexture VisibleTexture
        {
            get { return base.VisibleTexture as LevelTexture; }
            set { base.VisibleTexture = value; }
        }

        protected override float TileSelectionSize => _editor.Configuration.TextureMap_TileSelectionSize;
        protected override bool ResetAttributesOnNewSelection => _editor.Configuration.TextureMap_ResetAttributesOnNewSelection;
        protected override bool MouseWheelMovesTheTextureInsteadOfZooming => _editor.Configuration.TextureMap_MouseWheelMovesTheTextureInsteadOfZooming;
        protected override float NavigationSpeedKeyMove => _editor.Configuration.TextureMap_NavigationSpeedKeyMove;
        protected override float NavigationSpeedKeyZoom => _editor.Configuration.TextureMap_NavigationSpeedKeyZoom;
        protected override float NavigationSpeedMouseZoom => _editor.Configuration.TextureMap_NavigationSpeedMouseZoom;
        protected override float NavigationSpeedMouseWheelZoom => _editor.Configuration.TextureMap_NavigationSpeedMouseWheelZoom;
        protected override float NavigationMaxZoom => _editor.Configuration.TextureMap_NavigationMaxZoom;
        protected override float NavigationMinZoom => _editor.Configuration.TextureMap_NavigationMinZoom;
        protected override bool DrawSelectionDirectionIndicators => _editor.Configuration.TextureMap_DrawSelectionDirectionIndicators;

        public PanelTextureMap() : base()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                _editor = Editor.Instance;

                if (_editor != null)
                    _editor.EditorEventRaised += EditorEventRaised;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.ConfigurationChangedEvent)
                Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!(VisibleTexture?.IsAvailable ?? false))
            {
                if (VisibleTexture != null)
                    EditorActions.ReloadResource(Parent, _editor.Level.Settings, VisibleTexture);
                else
                    EditorActions.AddTexture(Parent);
                return;
            }

            base.OnMouseDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (VisibleTexture == null || !VisibleTexture.IsAvailable)
            {
                string notifyMessage;

                if (string.IsNullOrEmpty(VisibleTexture?.Path))
                    notifyMessage = "Click here to load new texture file.";
                else
                {
                    string fileName = PathC.GetFileNameWithoutExtensionTry(VisibleTexture?.Path) ?? "";
                    if (PathC.IsFileNotFoundException(VisibleTexture?.LoadException))
                        notifyMessage = "Texture file '" + fileName + "' was not found!\n";
                    else
                        notifyMessage = "Unable to load texture from file '" + fileName + "'.\n";
                    notifyMessage += "Click here to choose a replacement.\n\n";
                    notifyMessage += "Path: " + (_editor.Level.Settings.MakeAbsolute(VisibleTexture?.Path) ?? "");
                }

                RectangleF textArea = ClientRectangle;
                textArea.Size -= new SizeF(_scrollSizeTotal, _scrollSizeTotal);

                using (var b = new SolidBrush(Colors.DisabledText))
                    e.Graphics.DrawString(notifyMessage, Font, b, textArea,
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }

            base.OnPaint(e);
        }
    }
}
