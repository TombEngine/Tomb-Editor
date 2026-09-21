using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace TombLib.Tests;

internal static class WPFTestHelper
{
	public static T GetPrivateField<T>(object instance, string fieldName)
	{
		FieldInfo field = FindInstanceField(instance.GetType(), fieldName)
			?? throw new InvalidOperationException($"Private field '{fieldName}' was not found.");

		return (T)(field.GetValue(instance)
			?? throw new InvalidOperationException($"Private field '{fieldName}' returned null."));
	}

	public static object? GetPrivateFieldValue(object instance, string fieldName)
	{
		FieldInfo field = FindInstanceField(instance.GetType(), fieldName)
			?? throw new InvalidOperationException($"Private field '{fieldName}' was not found.");

		return field.GetValue(instance);
	}

	public static void SetPrivateField(object instance, string fieldName, object? value)
	{
		FieldInfo field = FindInstanceField(instance.GetType(), fieldName)
			?? throw new InvalidOperationException($"Private field '{fieldName}' was not found.");

		field.SetValue(instance, value);
	}

	public static object? InvokeInstanceMethod(object instance, string methodName, Type[] parameterTypes, params object?[] arguments)
	{
		MethodInfo method = instance.GetType().GetMethod(
			methodName,
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
			binder: null,
			parameterTypes,
			modifiers: null)
			?? throw new InvalidOperationException($"Instance method '{methodName}' was not found.");

		return method.Invoke(instance, arguments);
	}

	public static object? InvokeStaticMethod(Type type, string methodName, Type[] parameterTypes, params object?[] arguments)
	{
		MethodInfo method = type.GetMethod(
			methodName,
			BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
			binder: null,
			parameterTypes,
			modifiers: null)
			?? throw new InvalidOperationException($"Static method '{methodName}' was not found.");

		return method.Invoke(null, arguments);
	}

	public static FieldInfo? FindInstanceField(Type type, string fieldName)
	{
		for (Type? currentType = type; currentType is not null; currentType = currentType.BaseType)
		{
			FieldInfo? field = currentType.GetField(
				fieldName,
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

			if (field is not null)
				return field;
		}

		return null;
	}

	public static Window ShowInHostWindow(FrameworkElement content)
	{
		var window = new Window
		{
			Content = content,
			Width = 800.0,
			Height = 600.0,
			ShowActivated = false,
			ShowInTaskbar = false,
			WindowStyle = WindowStyle.None
		};

		window.Show();
		PumpDispatcher(window.Dispatcher, DispatcherPriority.Background);
		return window;
	}

	public static void PumpDispatcher(Dispatcher dispatcher, DispatcherPriority priority)
	{
		dispatcher.Invoke(priority, new Action(() => { }));
	}

	public static void RunInSta(Action action)
	{
		Exception? capturedException = null;
		using var completed = new ManualResetEventSlim(false);

		var thread = new Thread(() =>
		{
			try
			{
				action();
			}
			catch (Exception exception)
			{
				capturedException = exception;
			}
			finally
			{
				completed.Set();
			}
		});

		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		completed.Wait();
		thread.Join();

		if (capturedException is not null)
			ExceptionDispatchInfo.Capture(capturedException).Throw();
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
}
