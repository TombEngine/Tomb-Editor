using DarkUI.Controls;

namespace WadTool.Controls.ContextMenus
{
    public abstract class BaseContextMenu : DarkContextMenu
    {
        protected WadToolClass _tool;

        public BaseContextMenu(WadToolClass tool)
        {
            _tool = tool;
        }
    }
}
