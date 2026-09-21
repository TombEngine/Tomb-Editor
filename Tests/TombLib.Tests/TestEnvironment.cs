using System.Runtime.CompilerServices;

namespace TombLib.Tests;

/// <summary>
/// Process-wide test environment adjustments for the WPF test host.
/// </summary>
internal static class TestEnvironment
{
	/// <summary>
	/// Disables the ItemsControl automation bridge before any test runs. UI Automation peers create
	/// ref-counted native handles (<c>MS.Internal.Automation.ElementProxy</c>) that keep the hosted
	/// editor's visual tree reachable after disposal in full-suite runs, which breaks the
	/// forced-GC reachability assertions (verified with a live heap dump in the Phase 8 consumer
	/// pass). These tests exercise editor behavior, not UI Automation, so the bridge stays off.
	/// </summary>
	[ModuleInitializer]
	internal static void Initialize()
	{
		AppContext.SetSwitch("Switch.System.Windows.Automation.Peers.ItemAutomationPeerKeepsItsItemAlive", false);
		AppContext.SetSwitch("Switch.System.Windows.Controls.ItemsControlDoesNotSupportAutomation", true);
	}
}
