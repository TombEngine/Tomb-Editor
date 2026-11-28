#nullable enable

using System.Windows;
using TombLib.Input;

namespace TombLib.WPF.Input;

/// <summary>Conversions from WPF drag events to editor drag contracts.</summary>
public static class DragEventArgsExtensions
{
    /// <summary>Copies the WPF drag data and effects into platform independent drag arguments.</summary>
    public static PointerDragEventArgs ToPointerDragEventArgs(this DragEventArgs e)
    {
        var data = new WpfDataTransfer();
        foreach (var format in e.Data.GetFormats())
        {
            var value = e.Data.GetData(format);
            if (value is not null)
                data.SetData(format, value);
        }

        return new PointerDragEventArgs(data, (int)e.KeyStates, 0, 0, (TransferEffects)e.AllowedEffects, (TransferEffects)e.Effects);
    }
}
