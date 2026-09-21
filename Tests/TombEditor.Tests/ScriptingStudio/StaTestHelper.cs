using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Runtime.ExceptionServices;
using System.Threading;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Tests.ScriptingStudio;

internal static class StaTestHelper
{
    private static readonly TimeSpan StaThreadTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Runs the test body on a dedicated STA thread without installing a synchronization context.
    /// Blocking waits are safe for these tests because production UI dispatch is not being exercised.
    /// </summary>
    public static void RunInSta(Action action, TimeSpan? timeout = null)
    {
        ExceptionDispatchInfo? capturedException = null;

        var thread = new Thread(() =>
        {
            try
            {
                ConfigureTestServiceLocator();
                action();
            }
            catch (Exception exception)
            {
                capturedException = ExceptionDispatchInfo.Capture(exception);
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        JoinStaThread(thread, timeout ?? StaThreadTimeout);

        capturedException?.Throw();
    }

    public static WeakReference RunInSta(Func<WeakReference> function)
    {
        ArgumentNullException.ThrowIfNull(function);

        ExceptionDispatchInfo? capturedException = null;
        WeakReference? result = null;

        var thread = new Thread(() =>
        {
            try
            {
                ConfigureTestServiceLocator();
                result = function();
            }
            catch (Exception exception)
            {
                capturedException = ExceptionDispatchInfo.Capture(exception);
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        JoinStaThread(thread, StaThreadTimeout);

        capturedException?.Throw();
        return result ?? throw new InvalidOperationException("The STA function did not return a workbench reference.");
    }

    private static void JoinStaThread(Thread thread, TimeSpan timeout)
    {
        if (thread.Join(timeout))
            return;

        throw new TimeoutException(
            $"The STA test thread did not finish within {timeout.TotalSeconds:0} seconds. "
            + "The test may be blocked on an incomplete task or dispatcher operation.");
    }

    /// <summary>
    /// Configures the global <see cref="ServiceLocator"/> with a minimal set
    /// of services required for unit testing ScriptingStudio components.
    /// Call once per test assembly or before tests that resolve services.
    /// </summary>
    public static void ConfigureTestServiceLocator()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILocalizationService>(new TestLocalizationService());
        services.AddSingleton<IMessageService>(new Mock<IMessageService>().Object);
        ServiceLocator.Configure(services.BuildServiceProvider());
    }

    public static void AssertCollected(WeakReference reference, string description, int maxAttempts = 8)
    {
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxAttempts);

        for (int attempt = 0; attempt < maxAttempts && reference.IsAlive; attempt++)
        {
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            Thread.Yield();
        }

        Assert.IsFalse(reference.IsAlive, $"The {description} remained reachable after {maxAttempts} forced collection attempts.");
    }

    private sealed class TestLocalizationService : ILocalizationService
    {
        public string? NamespaceName => null;

        public string? ComponentName => null;

        public string this[string key] => key;

        public string Format(string key, params object[] args)
            => string.Format(this[key], args);

        public ILocalizationService WithKeysFor(System.ComponentModel.INotifyPropertyChanged viewModel) => this;
    }
}
