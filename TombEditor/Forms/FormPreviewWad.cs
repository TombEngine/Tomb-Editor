using DarkUI.Forms;
using System;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Controls;
using TombLib.Rendering;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombEditor.Forms
{
    public partial class FormPreviewWad : DarkForm
    {
        private Wad2 _wad;

        public FormPreviewWad(Wad2 wad, RenderingDevice device, Editor editor)
        {
            _wad = wad;
            InitializeComponent();

            panelItem.Editor = editor;
            panelItem.InitializeRendering(device, editor.Configuration.RenderingItem_Antialias);
            panelItem.AnimatePreview = editor.Configuration.RenderingItem_Animate;
            wadTree.Wad = wad;
            wadTree.MultiSelect = false;
            wadTree.SelectFirst();
        }

        private void wadTree_SelectedWadObjectIdsChanged(object sender, System.EventArgs e)
        {
            var selectedObjectId = wadTree.SelectedWadObjectIds.FirstOrDefault();
            var selectedObject = selectedObjectId == null ? null : _wad.TryGet(selectedObjectId);

            if (selectedObject != null && selectedObject is WadMoveable)
            {
                var item = selectedObject as WadMoveable;
                var skinId = new WadMoveableId(TrCatalog.GetMoveableSkin(panelItem.Editor.Level.Settings.GameVersion, item.Id.TypeId));
                var skin = panelItem.Editor.Level.Settings.WadTryGetMoveable(skinId);

                if (skin != null && skin != item)
                    panelItem.CurrentObject = item.ReplaceDummyMeshes(skin);
                else
                    panelItem.CurrentObject = item;
            }
            else
                panelItem.CurrentObject = selectedObject;

            panelItem.ResetCamera();
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);

            if (!Bounds.Contains(Cursor.Position))
                Close();
        }

        public class PanelRenderingItemPreview : PanelItemPreview
        {
            public Editor Editor { get; set; }

            protected override Vector4 ClearColor => Editor.Configuration.UI_ColorScheme.Color3DBackground;
            public override float FieldOfView => Editor.Configuration.RenderingItem_FieldOfView;
            public override float NavigationSpeedMouseWheelZoom => Editor.Configuration.RenderingItem_NavigationSpeedMouseWheelZoom;
            public override float NavigationSpeedMouseZoom => Editor.Configuration.RenderingItem_NavigationSpeedMouseZoom;
            public override float NavigationSpeedMouseTranslate => Editor.Configuration.RenderingItem_NavigationSpeedMouseTranslate;
            public override float NavigationSpeedMouseRotate => Editor.Configuration.RenderingItem_NavigationSpeedMouseRotate;
            public override bool ReadOnly => true;
        }
    }
}
