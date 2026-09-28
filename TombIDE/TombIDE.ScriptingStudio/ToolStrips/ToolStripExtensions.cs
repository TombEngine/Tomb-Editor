using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.ToolStrips;

internal static class ToolStripExtensions
{
	public static ToolStripItem FindItem(this ToolStrip toolStrip, UICommand command)
	{
		foreach (ToolStripItem item in toolStrip.GetAllItems())
		{
			if (item.Tag is UIElementArgs e && e.Command == command)
				return item;
		}

		return null;
	}

	public static IEnumerable<ToolStripItem> GetAllItems(this ToolStrip toolStrip)
		=> GetAllItems(toolStrip.Items);

	public static IEnumerable<ToolStripItem> GetAllItems(this ToolStripMenuItem menuItem)
		=> GetAllItems(menuItem.DropDownItems);

	public static IEnumerable<ToolStripItem> GetAllItems(this ToolStripItemCollection itemCollection)
	{
		foreach (ToolStripItem item in itemCollection)
		{
			yield return item;

			if (item is ToolStripMenuItem)
			{
				var stack = new Stack<ToolStripMenuItem>();
				stack.Push(item as ToolStripMenuItem);

				while (stack.Count > 0)
				{
					foreach (ToolStripItem dropDownItem in stack.Pop().DropDownItems)
					{
						yield return dropDownItem;

						if (dropDownItem is ToolStripMenuItem)
							stack.Push(dropDownItem as ToolStripMenuItem);
					}
				}
			}
		}
	}

	public static bool IsTargetItem(this ToolStripItem item, Enum modeEnum)
		=> item.Tag is UIElementArgs e && e.UIModeEnumType == modeEnum.GetType();

	public static IEnumerable<ToolStripItem> GetTargetItems(this ToolStripItemCollection items, Enum modeEnum)
		=> items.Cast<ToolStripItem>().ToList().FindAll(x => x.IsTargetItem(modeEnum));
}
