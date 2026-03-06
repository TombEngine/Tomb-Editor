using System.Windows.Forms;
using TombLib.Wad;

namespace WadTool.Controls.ContextMenus
{
    class MoveableContextMenu : BaseContextMenu
    {
        public MoveableContextMenu(WadToolClass tool, WadMoveableId moveableId)
            : base(tool)
        {
            Items.Add(new ToolStripMenuItem("Edit skeleton", Properties.Resources.edit_16, (o, e) =>
            {
                //using (var form = new FormSkeletonEditor(tool,WadToolClass.)
                //     _tool.SelectedObjectEdited();
            }));            
        }
    }
}
