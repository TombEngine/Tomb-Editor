using System;
using System.Collections.Generic;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.Shell;

public sealed class StudioPaneContribution
{
	public StudioPaneContribution(UICommand command, string serializationKey, Func<StudioDockPane> createContent)
	{
		Command = command;
		SerializationKey = serializationKey ?? throw new ArgumentNullException(nameof(serializationKey));
		CreateContent = createContent ?? throw new ArgumentNullException(nameof(createContent));
	}

	public UICommand Command { get; }

	public string SerializationKey { get; }

	public Func<StudioDockPane> CreateContent { get; }
}

/// <summary>
/// Provides pane contributions that the shell can register in the dock layout.
/// </summary>
public interface IStudioPaneContributionProvider
{
	/// <summary>
	/// Gets the list of pane contributions available for registration.
	/// </summary>
	IReadOnlyList<StudioPaneContribution> GetPaneContributions();
}

internal sealed class StaticStudioPaneContributionProvider : IStudioPaneContributionProvider
{
	private readonly IReadOnlyList<StudioPaneContribution> _contributions;

	public StaticStudioPaneContributionProvider(IReadOnlyList<StudioPaneContribution> contributions)
		=> _contributions = contributions ?? [];

	public IReadOnlyList<StudioPaneContribution> GetPaneContributions() => _contributions;
}
