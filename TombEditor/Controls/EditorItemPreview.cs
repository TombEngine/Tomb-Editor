#nullable enable

using System;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Controls.Wpf;

namespace TombEditor.Controls;

/// <summary>Renders browser and WAD previews using the editor's current settings.</summary>
public sealed class EditorItemPreview : PanelItemPreview
{
    private readonly Editor _editor;
    private readonly bool _allowDragging;

    public EditorItemPreview(Editor editor, bool allowDragging = true)
    {
        _editor = editor;
        _allowDragging = allowDragging;
        _editor.EditorEventRaised += OnEditorEvent;
    }

    private void OnEditorEvent(IEditorEvent editorEvent)
    {
        if (editorEvent is Editor.ConfigurationChangedEvent)
            Camera.FieldOfView = _editor.Configuration.RenderingItem_FieldOfView * (float)(Math.PI / 180);

        if (editorEvent is Editor.ChosenItemsChangedEvent)
            ResetCamera();

        if (editorEvent is Editor.LoadedWadsChangedEvent or Editor.LoadedImportedGeometriesChangedEvent or Editor.LevelChangedEvent)
            GarbageCollect();

        if (editorEvent is Editor.ConfigurationChangedEvent or Editor.ChosenItemsChangedEvent or Editor.LoadedWadsChangedEvent
            or Editor.LoadedImportedGeometriesChangedEvent or Editor.LevelChangedEvent or Editor.EditorFocusedEvent)
        {
            Invalidate();
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (_allowDragging && e.Button == MouseButtons.Left && CurrentObject is not null)
            DoDragDrop(CurrentObject, DragDropEffects.Copy);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _editor.EditorEventRaised -= OnEditorEvent;

        base.Dispose(disposing);
    }

    protected override Vector4 ClearColor => _editor.Configuration.UI_ColorScheme.Color3DBackground;
    public override float FieldOfView => _editor.Configuration.RenderingItem_FieldOfView;
    public override float NavigationSpeedMouseWheelZoom => _editor.Configuration.RenderingItem_NavigationSpeedMouseWheelZoom;
    public override float NavigationSpeedMouseZoom => _editor.Configuration.RenderingItem_NavigationSpeedMouseZoom;
    public override float NavigationSpeedMouseTranslate => _editor.Configuration.RenderingItem_NavigationSpeedMouseTranslate;
    public override float NavigationSpeedMouseRotate => _editor.Configuration.RenderingItem_NavigationSpeedMouseRotate;
    public override bool ReadOnly => true;
}
