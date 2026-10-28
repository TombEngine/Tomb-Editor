#nullable enable

using System;
using System.ComponentModel;
using System.Numerics;
using TombLib.Controls.Wpf;

namespace WadTool.Controls;

public class PanelRenderingMainPreview : PanelItemPreview
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Configuration Configuration { get; set; } = new();

    protected override Vector4 ClearColor => Configuration.RenderingItem_BackgroundColor;
    public override float FieldOfView => Configuration.RenderingItem_FieldOfView;
    public override float NavigationSpeedMouseRotate => Configuration.RenderingItem_NavigationSpeedMouseRotate;
    public override float NavigationSpeedMouseTranslate => Configuration.RenderingItem_NavigationSpeedMouseTranslate;
    public override float NavigationSpeedMouseWheelZoom => Configuration.RenderingItem_NavigationSpeedMouseWheelZoom;
    public override float NavigationSpeedMouseZoom => Configuration.RenderingItem_NavigationSpeedMouseZoom;
    public override bool ReadOnly => false;

    protected override void OnMouseEnter(EventArgs e)
    {
        // Keep the object tree focused while moving across the preview.
    }
}
