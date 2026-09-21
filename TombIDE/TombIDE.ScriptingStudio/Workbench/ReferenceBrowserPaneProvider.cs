#nullable enable

using System;
using System.Collections.Generic;
using TombIDE.ScriptingStudio.ClassicScript;
using TombIDE.ScriptingStudio.Navigation;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombLib.Scripting.ClassicScript.Navigation;

namespace TombIDE.ScriptingStudio.Workbench;

internal sealed class ReferenceBrowserPaneProvider : IStudioPaneContributionProvider, IDisposable
{
	private readonly ScriptingWorkspaceProfile _profile;
	private readonly ReferenceBrowserViewModel _viewModel;
	private readonly ReferenceInfoViewModel _referenceInfoViewModel;
	private readonly ReferenceInfoView _referenceInfoView;
	private readonly ClassicScriptReferenceInfoService _referenceInfoService;
	private bool _disposed;

	public ReferenceBrowserPaneProvider(
		ScriptingWorkspaceProfile profile,
		ReferenceBrowserViewModel viewModel,
		ReferenceInfoViewModel referenceInfoViewModel)
	{
		ArgumentNullException.ThrowIfNull(profile);
		ArgumentNullException.ThrowIfNull(viewModel);
		ArgumentNullException.ThrowIfNull(referenceInfoViewModel);

		_profile = profile;
		_viewModel = viewModel;
		_referenceInfoViewModel = referenceInfoViewModel;
		_referenceInfoService = new ClassicScriptReferenceInfoService();
		_referenceInfoView = new ReferenceInfoView(referenceInfoViewModel);
	}

	public IReadOnlyList<StudioPaneContribution> GetPaneContributions()
	{
		if (_profile.Kind != ScriptingWorkspaceKind.ClassicScript || !_profile.SupportsView(UICommand.ReferenceBrowser))
			return [];

		var pane = new ReferenceBrowserToolWindow(_viewModel);
		pane.ReferenceDefinitionRequested += ReferenceBrowser_ReferenceDefinitionRequested;
		return [new StudioPaneContribution(UICommand.ReferenceBrowser, pane.SerializationKey, () => pane)];
	}

	public void DisposeReferenceInfo()
		=> Dispose();

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;
		_referenceInfoView.ClosePermanently();
	}

	private void ReferenceBrowser_ReferenceDefinitionRequested(object? sender, ReferenceDefinitionEventArgs e)
	{
		ClassicScriptReferenceInfo referenceInfo = _referenceInfoService.GetReferenceInfo(e.Keyword, e.Type);
		_referenceInfoView.Show(referenceInfo.Keyword, referenceInfo.Description);
	}
}
