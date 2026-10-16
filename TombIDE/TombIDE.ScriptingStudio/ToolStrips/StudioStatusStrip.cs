#nullable enable

using System.ComponentModel;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.ToolStrips;

public sealed class StudioStatusStrip
{
	private readonly StudioStatusStripViewModel _viewModel;

	private DocumentMode _documentMode;
	private IEditorControl? _editorControl;
	private StudioStatusStripSegment[] _segmentContributions = [];

	public StudioStatusStrip()
	{
		_viewModel = new StudioStatusStripViewModel();
		View = new StudioStatusStripView
		{
			DataContext = _viewModel
		};
	}

	public StudioStatusStripView View { get; }

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public DocumentMode DocumentMode
	{
		get => _documentMode;
		set
		{
			_documentMode = value;
			_viewModel.SetDocumentMode(value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public IEditorControl? EditorControl
	{
		get => _editorControl;
		set
		{
			_editorControl = value;
			_viewModel.SetEditorControl(value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public StudioStatusStripSegment[] SegmentContributions
	{
		get => _segmentContributions;
		set
		{
			_segmentContributions = value ?? [];
			_viewModel.SetSegmentContributions(_segmentContributions);
		}
	}

	public void ReloadContributionSettings()
		=> View.ReloadContributionSettings();
}
