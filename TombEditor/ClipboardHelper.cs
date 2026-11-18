#nullable enable

using System.Windows;

namespace TombEditor;

/// <summary>Clipboard queries that tolerate the clipboard being locked by another program.</summary>
internal static class ClipboardHelper
{
    /// <summary>
    /// Returns whether the clipboard holds data the editor can paste. A busy clipboard counts as "nothing to paste"
    /// instead of throwing (reading it can fail while another program is using it).
    /// </summary>
    public static bool CanPaste(bool roomMode, bool includeSectors)
    {
        try
        {
            if (roomMode)
                return Clipboard.ContainsData(typeof(RoomClipboardData).FullName);

            return Clipboard.ContainsData(typeof(ObjectClipboardData).FullName)
                || includeSectors && Clipboard.ContainsData(typeof(SectorsClipboardData).FullName);
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
            return false;
        }
    }
}