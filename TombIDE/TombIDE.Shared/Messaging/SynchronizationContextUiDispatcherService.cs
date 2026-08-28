#nullable enable

using System;
using System.Threading;

namespace TombIDE.Shared.Messaging;

/// <summary>
/// UI dispatcher backed by a captured <see cref="SynchronizationContext"/>.
/// </summary>
public sealed class SynchronizationContextUiDispatcherService : IUiDispatcherService
{
	private static readonly SendOrPostCallback InvokeCallback = static state => ((Action)state!).Invoke();

	private readonly SynchronizationContext _synchronizationContext;

	/// <summary>
	/// Initializes a new dispatcher over the supplied synchronization context.
	/// </summary>
	/// <param name="synchronizationContext">The UI synchronization context.</param>
	public SynchronizationContextUiDispatcherService(SynchronizationContext synchronizationContext)
	{
		_synchronizationContext = synchronizationContext ?? throw new ArgumentNullException(nameof(synchronizationContext));
	}

	/// <summary>
	/// Creates a dispatcher from the current synchronization context.
	/// </summary>
	public static SynchronizationContextUiDispatcherService FromCurrentContext()
	{
		var synchronizationContext = SynchronizationContext.Current;

		if (synchronizationContext is null)
			throw new InvalidOperationException("A UI synchronization context is required to publish shared scripting messages.");

		return new SynchronizationContextUiDispatcherService(synchronizationContext);
	}

	/// <inheritdoc/>
	public bool CheckAccess()
		=> ReferenceEquals(SynchronizationContext.Current, _synchronizationContext);

	/// <inheritdoc/>
	public void Invoke(Action action)
	{
		ArgumentNullException.ThrowIfNull(action);

		if (CheckAccess())
		{
			action();
			return;
		}

		_synchronizationContext.Send(InvokeCallback, action);
	}
}
