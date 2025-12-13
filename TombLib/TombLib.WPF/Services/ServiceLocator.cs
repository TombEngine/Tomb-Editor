using Microsoft.Extensions.DependencyInjection;
using System;

namespace TombLib.WPF.Services;

/// <summary>
/// Provides a global service locator for dependency injection.
/// </summary>
public static class ServiceLocator
{
	private static readonly object _lock = new();

	/// <summary>
	/// Gets the current service provider instance.
	/// </summary>
	public static IServiceProvider? Current { get; private set; }

	/// <summary>
	/// Configures the service provider with a custom instance.
	/// </summary>
	/// <param name="serviceProvider">The service provider to use.</param>
	public static void Configure(IServiceProvider serviceProvider)
	{
		lock (_lock)
			Current = serviceProvider;
	}

	/// <summary>
	/// Gets a service of the specified type, or <see langword="null" /> if not found.
	/// </summary>
	/// <typeparam name="T">The type of service to get.</typeparam>
	/// <returns>The service instance or <see langword="null" />.</returns>
	public static T? GetService<T>() where T : class
		=> Current?.GetService<T>();

	/// <summary>
	/// Gets a service of the specified type, or throws an exception if not found.
	/// </summary>
	/// <typeparam name="T">The type of service to get.</typeparam>
	/// <param name="injectedService">An optional pre-injected service instance to use instead of resolving from the provider.</param>
	/// <returns>The service instance.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the service is not registered.</exception>
	public static T ResolveService<T>(T? injectedService = null) where T : class
		=> injectedService
		?? GetService<T>()
		?? throw new InvalidOperationException($"Service of type {typeof(T).FullName} is not registered.");
}
