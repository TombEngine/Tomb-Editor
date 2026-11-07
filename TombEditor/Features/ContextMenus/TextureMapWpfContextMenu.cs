#nullable enable

using static TombEditor.Features.ContextMenus.WpfContextMenuHelper;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using TombLib.Services;
using TombLib.Utils;

namespace TombEditor.Features.ContextMenus;

internal static class TextureMapWpfContextMenu
{
    public static ContextMenu Show(Editor editor, IWindowOwner owner,
                                   Vector2 position, System.Drawing.Point screenPoint)
    {
        var menu = new ContextMenu();

        if (editor.SelectedTexture != TextureArea.None)
            menu.Items.Add(Item("Set as default texture", null,
                () =>
                {
                    editor.Level.Settings.DefaultTexture = editor.SelectedTexture;
                    (owner as UIElement)?.InvalidateVisual();
                }));

        if (editor.Level.Settings.DefaultTexture != TextureArea.None)
            menu.Items.Add(Item("Clear default texture", null,
                () =>
                {
                    editor.Level.Settings.DefaultTexture = TextureArea.None;
                    (owner as UIElement)?.InvalidateVisual();
                }));

        // No items at all means right-click is a no-op. Match the old
        // DarkContextMenu behaviour (which did nothing in that case).
        if (menu.Items.Count == 0)
            return menu;

        Open(menu, screenPoint);
        return menu;
    }
}
