#nullable enable

using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Collections.Generic;

namespace TombIDE.ScriptingStudio.FindAndReplace;

/// <summary>
/// Published when a "Find All" operation completes.
/// Consumed by <see cref="SearchResultsToolWindow"/> via <see cref="WorkbenchService"/>.
/// </summary>
public sealed class FindAllPerformedMessage : ValueChangedMessage<IReadOnlyList<FindReplaceSource>>
{
	public FindAllPerformedMessage(IReadOnlyList<FindReplaceSource> sources)
		: base(sources)
	{
	}
}
