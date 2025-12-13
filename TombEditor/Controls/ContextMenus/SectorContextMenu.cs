using System.Windows.Forms;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.Controls.ContextMenus
{
    class SectorContextMenu : BaseContextMenu
    {
        public SectorContextMenu(Editor editor, IWin32Window owner, Room targetRoom, VectorInt2 targetSector)
            : base(editor, owner)
        {
			Items.Add(new ToolStripMenuItem("Paste object", Properties.Resources.general_clipboard_16, (o, e) =>
            {
                EditorActions.PasteObject(targetSector, targetRoom);
            }) { Enabled = Clipboard.ContainsData(typeof(ObjectClipboardData).FullName) });

            Items.Add(new ToolStripMenuItem("Select objects", null, (o, e) =>
            {
                EditorActions.SelectObjectsInArea(owner, editor.SelectedSectors);
            }));

            Items.Add(new ToolStripSeparator());

            Items.Add(new ToolStripMenuItem("Move Lara", null, (o, e) =>
            {
                EditorActions.MoveLara(owner, targetRoom, targetSector);
            }));

			Items.Add(new ToolStripMenuItem("Move Object", Properties.Resources.general_target_16, (o, e) => 
            {
				var obj = editor.SelectedObject as PositionBasedObjectInstance;
				EditorActions.MoveObject(obj, targetRoom, targetSector);
			}) { Enabled = _editor.SelectedObject is PositionBasedObjectInstance && !(_editor.SelectedObject is ObjectGroup) });
			Items.Add(new ToolStripSeparator());

            Items.Add(new ToolStripMenuItem("Add camera", Properties.Resources.objects_Camera_16, (o, e) =>
            {
                EditorActions.PlaceObject(targetRoom, targetSector, new CameraInstance());
            }));

            if (editor.Level.Settings.GameVersion.Native() <= TRVersion.Game.TR2)
            Items.Add(new ToolStripMenuItem("Add room sprite", Properties.Resources.objects_Sprite_16, (o, e) =>
            {
                EditorActions.PlaceObject(targetRoom, targetSector, new SpriteInstance());
            }));

            Items.Add(new ToolStripMenuItem("Add flyby camera", Properties.Resources.objects_movie_projector_16, (o, e) =>
            {
                EditorActions.PlaceObject(targetRoom, targetSector, new FlybyCameraInstance(editor.SelectedObject));
            }));

            Items.Add(new ToolStripMenuItem("Add sink", Properties.Resources.objects_tornado_16, (o, e) =>
            {
                EditorActions.PlaceObject(targetRoom, targetSector, new SinkInstance());
            }));

            Items.Add(new ToolStripMenuItem("Add sound source", Properties.Resources.objects_speaker_16, (o, e) =>
            {
                EditorActions.PlaceObject(targetRoom, targetSector, new SoundSourceInstance());
            }));

            Items.Add(new ToolStripMenuItem("Add ghost block", Properties.Resources.objects_geometry_override_16, (o, e) =>
            {
                EditorActions.PlaceObject(targetRoom, targetSector, new GhostBlockInstance());
            }));

            Items.Add(new ToolStripMenuItem("Add imported geometry", Properties.Resources.objects_custom_geometry, (o, e) =>
            {
                EditorActions.PlaceObject(targetRoom, targetSector, new ImportedGeometryInstance());
            }));

            Items.Add(new ToolStripMenuItem("Add memo", Properties.Resources.objects_Memo_16, (o, e) =>
            {
                EditorActions.PlaceObject(targetRoom, targetSector, new MemoInstance());
            }));

            if (_editor.Level.IsTombEngine)
            {
                Items.Add(new ToolStripMenuItem("Add box volume", Properties.Resources.objects_volume_box_16, (o, e) =>
                {
                    EditorActions.PlaceObject(targetRoom, targetSector, new BoxVolumeInstance());
                }));
                Items.Add(new ToolStripMenuItem("Add sphere volume", Properties.Resources.objects_volume_sphere_16, (o, e) =>
                {
                    EditorActions.PlaceObject(targetRoom, targetSector, new SphereVolumeInstance());
                }));
            }
        }
    }
}
