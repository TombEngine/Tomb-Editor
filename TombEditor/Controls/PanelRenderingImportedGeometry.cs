using DarkUI.Config;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Controls;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.Controls
{
    public class PanelRenderingImportedGeometry : PanelItemPreview
    {
        private readonly Editor _editor;

        public PanelRenderingImportedGeometry()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                _editor = Editor.Instance;
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
            // Update field of view
            if (obj is Editor.ConfigurationChangedEvent)
            {
                Camera.FieldOfView = _editor.Configuration.RenderingItem_FieldOfView * (float)(Math.PI / 180);
                Invalidate();
            }

            // Update currently viewed item.
            if (obj is Editor.ChosenItemsChangedEvent itemsChanged)
            {
                if (itemsChanged.Current?.Any(o => o is ImportedGeometry) == true)
                {
                    ResetCamera();
                    Invalidate();
                    Update(); // Magic fix for room view leaking into item view
                }
            }

            if (obj is Editor.LoadedImportedGeometriesChangedEvent ||
                obj is Editor.EditorFocusedEvent)
                Invalidate();

            if (obj is Editor.LoadedWadsChangedEvent ||
                obj is Editor.LevelChangedEvent)
                GarbageCollect();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            LevelSettings settings = _editor?.Level?.Settings;
            if (settings == null)
                return;
            if (settings.ImportedGeometries.All(geo => geo.LoadException != null))
            {
                ImportedGeometry errorGeo = settings.ImportedGeometries.FirstOrDefault(geo => geo.LoadException != null);
                string notifyMessage;
                if (errorGeo == null)
                    notifyMessage = "Click here to load new imported geometry.";
                else
                {
                    string filePath = settings.MakeAbsolute(errorGeo.Info.Path);
                    string fileName = PathC.GetFileNameWithoutExtensionTry(filePath) ?? "";
                    if (PathC.IsFileNotFoundException(errorGeo.LoadException))
                        notifyMessage = "Geometry file '" + fileName + "' was not found!\n";
                    else
                        notifyMessage = "Unable to load geometry from file '" + fileName + "'.\n";
                    notifyMessage += "Click here to choose a replacement.\n\n";
                    notifyMessage += "Path: " + (filePath ?? "");
                }

                e.Graphics.Clear(Parent.BackColor);
                using (var b = new SolidBrush(Colors.DisabledText))
                    e.Graphics.DrawString(notifyMessage, Font, b, ClientRectangle,
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

                ControlPaint.DrawBorder(e.Graphics, ClientRectangle, Colors.GreySelection, ButtonBorderStyle.Solid);
            }
            else
                base.OnPaint(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            switch (e.Button)
            {
                case MouseButtons.Left:
                    LevelSettings settings = _editor?.Level?.Settings;
                    if (settings != null && settings.ImportedGeometries.All(geo => geo.LoadException != null))
                    {
                        ImportedGeometry geoToUpdate = settings.ImportedGeometries.FirstOrDefault(geo => geo.LoadException != null);
                        if (geoToUpdate != null)
                            EditorActions.ReloadResource(Parent, settings, geoToUpdate, true);
                        else
                            EditorActions.AddImportedGeometry(Parent);
                    }
                    else if (CurrentObject != null)
                        DoDragDrop(CurrentObject, DragDropEffects.Copy);
                    break;
            }
        }

        protected override Vector4 ClearColor => _editor.Configuration.UI_ColorScheme.Color3DBackground;
        public override float FieldOfView => _editor.Configuration.RenderingItem_FieldOfView;
        public override float NavigationSpeedMouseWheelZoom => _editor.Configuration.RenderingItem_NavigationSpeedMouseWheelZoom;
        public override float NavigationSpeedMouseZoom => _editor.Configuration.RenderingItem_NavigationSpeedMouseZoom;
        public override float NavigationSpeedMouseTranslate => _editor.Configuration.RenderingItem_NavigationSpeedMouseTranslate;
        public override float NavigationSpeedMouseRotate => _editor.Configuration.RenderingItem_NavigationSpeedMouseRotate;
        public override bool ReadOnly => true;
    }
}
