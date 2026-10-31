using System.Runtime.ExceptionServices;
using System.Windows.Threading;

namespace TombLib.Test;

internal static class StaTest
{
    public static void Run(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() => {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            finally
            {
                if (!Dispatcher.CurrentDispatcher.HasShutdownStarted)
                    Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(10)), "The WPF test did not complete.");
        if (failure is not null)
            ExceptionDispatchInfo.Capture(failure).Throw();
    }
}
