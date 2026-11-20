#nullable enable

using DarkUI.WPF;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace WadTool.Tests;

/// <summary>Runs WPF test code on one shared STA thread that owns the application and the DarkUI theme.</summary>
internal static class StaTestHost
{
    private static readonly Lazy<Dispatcher> HostDispatcher = new(StartHost);

    public static void Run(Action action)
    {
        Exception? failure = null;

        HostDispatcher.Value.Invoke(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });

        if (failure is not null)
            throw new AssertFailedException(failure.ToString(), failure);
    }

    private static Dispatcher StartHost()
    {
        Dispatcher? dispatcher = null;
        using var ready = new ManualResetEventSlim();

        var thread = new Thread(() =>
        {
            var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/DarkUI.WPF;component/Generic.xaml") });
            ThemeManager.Apply(Theme.Dark);

            dispatcher = Dispatcher.CurrentDispatcher;
            ready.Set();
            Dispatcher.Run();
        })
        {
            IsBackground = true
        };

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        ready.Wait();

        return dispatcher!;
    }
}
