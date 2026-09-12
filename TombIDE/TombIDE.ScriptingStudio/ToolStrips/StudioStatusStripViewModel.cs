#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Presentation;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.ToolStrips;

public sealed partial class StudioStatusStripViewModel : ObservableObject
{
	private readonly TextEditorStatusPresentationService _statusPresentationService = new();

	private readonly TextEditorStatusText _statusText = new(
		Shared.Strings.Default.Row,
		Shared.Strings.Default.Line,
		Shared.Strings.Default.Column,
		Shared.Strings.Default.Selected,
		Shared.Strings.Default.Zoom,
		Shared.Strings.Default.ResetZoom);

	private DocumentMode _documentMode;
	private IEditorControl? _editorControl;
	private StudioStatusStripSegment[] _segmentContributions = [];

	[ObservableProperty]
	private bool _showCaretPosition;

	[ObservableProperty]
	private bool _showSelectionLength;

	[ObservableProperty]
	private bool _showZoom;

	[ObservableProperty]
	private bool _showSyntaxPreviewPanel;

	[ObservableProperty]
	private string _rowLabelText = string.Empty;

	[ObservableProperty]
	private string _columnLabelText = string.Empty;

	[ObservableProperty]
	private string _selectionLabelText = string.Empty;

	[ObservableProperty]
	private string _zoomLabelText = string.Empty;

	[ObservableProperty]
	private string _resetZoomToolTipText = string.Empty;

	[ObservableProperty]
	private bool _canResetZoom;

	[ObservableProperty]
	private string _previewText = string.Empty;

	[ObservableProperty]
	private int _currentArgumentIndex = -1;

	public void SetDocumentMode(DocumentMode documentMode)
	{
		_documentMode = documentMode;
		UpdateStatus();
	}

	public void SetEditorControl(IEditorControl? editorControl)
	{
		if (_editorControl is not null)
			_editorControl.StatusChanged -= EditorControl_StatusChanged;

		_editorControl = editorControl;

		if (_editorControl is not null)
			_editorControl.StatusChanged += EditorControl_StatusChanged;

		UpdateStatus();
	}

	public void SetSegmentContributions(StudioStatusStripSegment[]? segmentContributions)
	{
		_segmentContributions = segmentContributions ?? [];
		UpdateStatus();
	}

	[RelayCommand]
	private void ResetZoom()
	{
		if (_editorControl is null)
			return;

		_editorControl.Zoom = 100;
		UpdateStatus();
	}

	private void EditorControl_StatusChanged(object? sender, EventArgs e)
		=> UpdateStatus();

	private void UpdateStatus()
	{
		bool showSyntaxPreviewWhenEmpty = _editorControl is null && HasSegment(StudioStatusStripSegment.SyntaxPreview);
		TextEditorStatusPresentation presentation = _statusPresentationService.Create(_editorControl, showSyntaxPreviewWhenEmpty, _statusText);

		ShowCaretPosition = presentation.HasEditor && HasSegment(StudioStatusStripSegment.CaretPosition);
		ShowSelectionLength = presentation.HasEditor && HasSegment(StudioStatusStripSegment.SelectionLength);
		ShowZoom = presentation.HasEditor && HasSegment(StudioStatusStripSegment.Zoom);
		ShowSyntaxPreviewPanel = presentation.HasEditor && HasSegment(StudioStatusStripSegment.SyntaxPreview) && presentation.ShowSyntaxPreview;

		RowLabelText = presentation.RowLabelText;
		ColumnLabelText = presentation.ColumnLabelText;
		SelectionLabelText = presentation.SelectionLabelText;
		ZoomLabelText = presentation.ZoomLabelText;
		CanResetZoom = presentation.CanResetZoom;
		ResetZoomToolTipText = presentation.ResetZoomToolTipText;
		PreviewText = presentation.SyntaxPreview?.Label ?? string.Empty;
		CurrentArgumentIndex = presentation.SyntaxPreview?.ActiveParameterIndex ?? -1;

		ResetZoomCommand.NotifyCanExecuteChanged();
	}

	private bool HasSegment(StudioStatusStripSegment segment)
		=> _segmentContributions.Contains(segment);
}
