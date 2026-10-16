#nullable enable

using System;
using System.Windows;
using System.Windows.Threading;
using TombIDE.Shared.Docking;

namespace TombIDE.ScriptingStudio.Shell;

/// <summary>
/// Adapter that delegates <see cref="IAvalonDockHost"/> operations to
/// the concrete <see cref="StudioAvalonDockHostView"/>.
/// </summary>
internal sealed class AvalonDockHostAdapter : IAvalonDockHost
{
	private readonly StudioAvalonDockHostView _view;

	public AvalonDockHostAdapter(StudioAvalonDockHostView view)
	{
		ArgumentNullException.ThrowIfNull(view);
		_view = view;
	}

	public FrameworkElement View => _view;

	public Dispatcher Dispatcher => _view.Dispatcher;

	public string SaveLayout() => _view.SaveLayout();

	public void RestoreLayout(string? layoutXml, DockPanelState legacyLayout, Action? onLayoutRestored = null)
		=> _view.RestoreLayout(layoutXml, legacyLayout, onLayoutRestored);

	public void RestoreDefaultLayout(DockPanelState legacyLayout, Action? onLayoutRestored = null)
		=> _view.RestoreDefaultLayout(legacyLayout, onLayoutRestored);

	public void DetachDocumentController()
		=> _view.DetachDocumentController();

	public bool IsPaneVisible(StudioDockPane? pane)
		=> _view.IsPaneVisible(pane);

	public bool ShowPane(StudioDockPane? pane)
		=> _view.ShowPane(pane);

	public bool TogglePane(StudioDockPane? pane)
		=> _view.TogglePane(pane);

	public bool EnsurePane(StudioDockPane? pane)
		=> _view.EnsurePane(pane);
}
