#nullable enable

using System;
using System.Windows.Threading;

namespace TombLib.WPF.Threading;

/// <summary>Processes queued UI requests while synchronous batch operations wait for workers.</summary>
public static class DispatcherEvents
{
    /// <summary>Processes one dispatcher turn without requiring a second UI framework's message loop.</summary>
    public static void ProcessPending()
    {
        var dispatcher = Dispatcher.CurrentDispatcher;
        if (dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
            return;

        var frame = new DispatcherFrame();
        dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }
}
