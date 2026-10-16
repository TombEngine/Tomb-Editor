#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.Shell;

/// <summary>
/// Owns the UICommand-to-StudioDockPane registry, disposal, and lookup.
/// Receives pane contributions from <see cref="IStudioPaneContributionProvider"/>
/// implementations resolved through DI.
/// Must not access AvalonDock directly; dock operations go through <see cref="IAvalonDockHost"/>.
/// </summary>
internal sealed class PaneCatalog : IDisposable
{
	private readonly Dictionary<UICommand, StudioDockPane> _panesByCommand = [];

	public PaneCatalog(IEnumerable<IStudioPaneContributionProvider> providers)
	{
		ArgumentNullException.ThrowIfNull(providers);

		foreach (IStudioPaneContributionProvider provider in providers)
		{
			foreach (StudioPaneContribution contribution in provider.GetPaneContributions())
			{
				if (_panesByCommand.ContainsKey(contribution.Command))
					continue;

				StudioDockPane pane = contribution.CreateContent();
				_panesByCommand[contribution.Command] = pane;
			}
		}
	}

	public IReadOnlyCollection<StudioDockPane> Panes => [.. _panesByCommand.Values.Distinct()];

	public T? GetPane<T>(UICommand command) where T : StudioDockPane
		=> _panesByCommand.TryGetValue(command, out StudioDockPane? pane) ? pane as T : null;

	public bool TryGetPane(UICommand command, out StudioDockPane? pane)
		=> _panesByCommand.TryGetValue(command, out pane);

	public void ShowPane(UICommand command, IAvalonDockHost dockHost)
	{
		ArgumentNullException.ThrowIfNull(dockHost);

		if (!_panesByCommand.TryGetValue(command, out StudioDockPane? pane))
			return;

		dockHost.ShowPane(pane);
	}

	public bool TryTogglePane(UICommand command, IAvalonDockHost dockHost, IPaneHostService paneHostService)
	{
		ArgumentNullException.ThrowIfNull(dockHost);
		ArgumentNullException.ThrowIfNull(paneHostService);

		if (!_panesByCommand.TryGetValue(command, out StudioDockPane? pane))
			return false;

		bool isVisible = dockHost.TogglePane(pane);
		paneHostService.SetPaneVisibility(command, isVisible);
		return true;
	}

	public void EnsurePanesRegistered(IAvalonDockHost dockHost)
	{
		ArgumentNullException.ThrowIfNull(dockHost);

		foreach (StudioDockPane pane in _panesByCommand.Values)
			dockHost.EnsurePane(pane);
	}

	public void UpdatePaneVisibilityChecks(IAvalonDockHost dockHost, IPaneHostService paneHostService)
	{
		ArgumentNullException.ThrowIfNull(dockHost);
		ArgumentNullException.ThrowIfNull(paneHostService);

		foreach ((UICommand command, StudioDockPane pane) in _panesByCommand)
		{
			bool isVisible = dockHost.IsPaneVisible(pane);
			paneHostService.SetPaneVisibility(command, isVisible);
		}
	}

	public void Dispose()
	{
		foreach (StudioDockPane pane in _panesByCommand.Values.Distinct())
			pane.Dispose();

		_panesByCommand.Clear();
	}
}
