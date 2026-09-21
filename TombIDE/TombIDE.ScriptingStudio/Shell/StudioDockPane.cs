#nullable enable

using System;
using System.Windows;

namespace TombIDE.ScriptingStudio.Shell;

public enum StudioDockPaneLocation
{
	Left,
	Right,
	Bottom
}

public abstract class StudioDockPane : IDisposable
{
	protected StudioDockPane(string title, string serializationKey, StudioDockPaneLocation defaultLocation, Size defaultSize)
	{
		Title = title ?? throw new ArgumentNullException(nameof(title));
		SerializationKey = serializationKey ?? throw new ArgumentNullException(nameof(serializationKey));
		DefaultLocation = defaultLocation;
		DefaultSize = defaultSize;
	}

	public string Title { get; protected set; }

	public string SerializationKey { get; }

	public StudioDockPaneLocation DefaultLocation { get; }

	public Size DefaultSize { get; }

	public abstract UIElement Content { get; }

	public virtual void Dispose()
	{ }
}
