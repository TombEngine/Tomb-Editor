#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using TombLib.Utils;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Panel3D.ToolPalette;

/// <summary>
/// ViewModel for the Toolbox panel.
/// Manages tool selection state, mode-based visibility, and brush/texture settings.
/// Icon assignment, DPI measurement, and WinForms interop remain in code-behind.
/// </summary>
public partial class ToolboxViewModel : ObservableObject
{
	private readonly Editor _editor;
	private readonly ILocalizationService _localizationService;

	private const string GridPaintIconBase = "/TombEditor;component/Resources/icons_toolbox/toolbox_GridPaint";

	public ToolboxViewModel(ILocalizationService? localizationService = null)
	{
		_editor = Editor.Instance;

		_localizationService = ServiceLocator.ResolveService(localizationService)
			.WithKeysFor(this);

		_editor.EditorEventRaised += OnEditorEventRaised;
		Refresh();
	}

	public void Cleanup()
	{
		_editor.EditorEventRaised -= OnEditorEventRaised;
	}

	#region Tool Checked States

	[ObservableProperty] private bool _isSelectionChecked;
	[ObservableProperty] private bool _isObjectSelectionChecked;
	[ObservableProperty] private bool _isBrushChecked;
	[ObservableProperty] private bool _isPencilChecked;
	[ObservableProperty] private bool _isLineChecked;
	[ObservableProperty] private bool _isObjectDeselectionChecked;
	[ObservableProperty] private bool _isFillChecked;
	[ObservableProperty] private bool _isGroupChecked;
	[ObservableProperty] private bool _isGridPaintChecked;
	[ObservableProperty] private bool _isShovelChecked;
	[ObservableProperty] private bool _isFlattenChecked;
	[ObservableProperty] private bool _isSmoothChecked;
	[ObservableProperty] private bool _isDragChecked;
	[ObservableProperty] private bool _isRampChecked;
	[ObservableProperty] private bool _isQuarterPipeChecked;
	[ObservableProperty] private bool _isHalfPipeChecked;
	[ObservableProperty] private bool _isBowlChecked;
	[ObservableProperty] private bool _isPyramidChecked;
	[ObservableProperty] private bool _isTerrainChecked;
	[ObservableProperty] private bool _isPortalDiggerChecked;
	[ObservableProperty] private bool _isObjectEraserChecked;

	// Object brush shapes
	[ObservableProperty] private bool _isBrushShapeCircleChecked;
	[ObservableProperty] private bool _isBrushShapeSquareChecked;

	// Special checked states (not derived from current tool type)
	[ObservableProperty] private bool _isUVFixerChecked;
	[ObservableProperty] private bool _isTextureEraserChecked;
	[ObservableProperty] private bool _isTextureInvisibleChecked;
	[ObservableProperty] private bool _isShowTexturesChecked;

	#endregion Tool Checked States

	#region Grid Size

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(GridPaintTooltip))]
	[NotifyPropertyChangedFor(nameof(GridPaintIconSource))]
	private PaintGridSize _gridSize = PaintGridSize.Grid2x2;

	public string GridPaintTooltip => GridSize switch
	{
		PaintGridSize.Grid2x2 => _localizationService.Format("GridPaint", "2x2"),
		PaintGridSize.Grid3x3 => _localizationService.Format("GridPaint", "3x3"),
		PaintGridSize.Grid4x4 => _localizationService.Format("GridPaint", "4x4"),
		_ => _localizationService["GridPaint"]
	};

	public string GridPaintIconSource => GridSize switch
	{
		PaintGridSize.Grid2x2 => $"{GridPaintIconBase}2x2-16.png",
		PaintGridSize.Grid3x3 => $"{GridPaintIconBase}3x3-16.png",
		PaintGridSize.Grid4x4 => $"{GridPaintIconBase}4x4-16.png",
		_ => $"{GridPaintIconBase}2x2-16.png"
	};

	#endregion Grid Size

	#region Mode Visibility

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsGeometryToolsVisible))]
	[NotifyPropertyChangedFor(nameof(IsFaceEditToolsVisible))]
	[NotifyPropertyChangedFor(nameof(IsObjectPlacementToolsVisible))]
	[NotifyPropertyChangedFor(nameof(IsFillVisible))]
	[NotifyPropertyChangedFor(nameof(IsSeparator2Visible))]
	[NotifyPropertyChangedFor(nameof(IsToolboxVisible))]
	private EditorMode _currentMode;

	public bool IsGeometryToolsVisible => CurrentMode == EditorMode.Geometry;

	public bool IsFaceEditToolsVisible => CurrentMode
		is EditorMode.FaceEdit
		or EditorMode.Lighting;

	public bool IsObjectPlacementToolsVisible => CurrentMode == EditorMode.ObjectPlacement;

	public bool IsFillVisible => IsFaceEditToolsVisible || IsObjectPlacementToolsVisible;

	public bool IsSeparator2Visible => !IsObjectPlacementToolsVisible;

	public bool IsToolboxVisible => CurrentMode
		is EditorMode.FaceEdit
		or EditorMode.Lighting
		or EditorMode.Geometry
		or EditorMode.ObjectPlacement;

	#endregion Mode Visibility

	#region Commands

	[RelayCommand]
	private void SwitchTool(string toolName)
	{
		if (Enum.TryParse<EditorToolType>(toolName, out var toolType))
			_editor.Tool = CreateEditorTool(toolType);
	}

	[RelayCommand]
	private void SwitchToGridPaint()
	{
		_editor.Tool = CreateEditorTool(EditorToolType.GridPaint);
	}

	[RelayCommand]
	private void SetBrushShapeCircle()
	{
		_editor.Configuration.ObjectBrush_Shape = ObjectBrushShape.Circle;
		_editor.ObjectBrushSettingsChange();
	}

	[RelayCommand]
	private void SetBrushShapeSquare()
	{
		_editor.Configuration.ObjectBrush_Shape = ObjectBrushShape.Square;
		_editor.ObjectBrushSettingsChange();
	}

	[RelayCommand]
	private void ToggleShowTextures()
	{
		_editor.Configuration.ObjectBrush_ShowTextures = !_editor.Configuration.ObjectBrush_ShowTextures;
		_editor.ObjectBrushSettingsChange();
	}

	[RelayCommand]
	private void SetTextureEraser()
	{
		if (IsTextureEraserChecked)
		{
			OnPropertyChanged(nameof(IsTextureEraserChecked));
			return;
		}

		_editor.SelectedTexture = TextureArea.None;
	}

	[RelayCommand]
	private void SetTextureInvisible()
	{
		if (IsTextureInvisibleChecked)
		{
			OnPropertyChanged(nameof(IsTextureInvisibleChecked));
			return;
		}

		_editor.SelectedTexture = TextureArea.Invisible;
	}

	[RelayCommand]
	private void ToggleUVFixer()
	{
		_editor.Tool = CreateEditorTool(uvFixer: !_editor.Tool.TextureUVFixer);
	}

	private EditorTool CreateEditorTool(EditorToolType? tool = null, bool? uvFixer = null)
	{
		var current = _editor.Tool;

		return new EditorTool
		{
			Tool = tool ?? current.Tool,
			TextureUVFixer = uvFixer ?? current.TextureUVFixer,
			GridSize = current.GridSize
		};
	}

	#endregion Commands

	#region State Updates

	private void Refresh()
	{
		UpdateToolCheckedState();
		UpdateBrushSettings();
		UpdateTextureState();
		UpdateModeVisibility();
	}

	private void UpdateToolCheckedState()
	{
		var tool = _editor.Tool;

		IsSelectionChecked = tool.Tool == EditorToolType.Selection;
		IsObjectSelectionChecked = tool.Tool == EditorToolType.ObjectSelection;
		IsObjectDeselectionChecked = tool.Tool == EditorToolType.ObjectDeselection;
		IsBrushChecked = tool.Tool == EditorToolType.Brush;
		IsPencilChecked = tool.Tool == EditorToolType.Pencil;
		IsLineChecked = tool.Tool == EditorToolType.Line;
		IsFillChecked = tool.Tool == EditorToolType.Fill;
		IsGroupChecked = tool.Tool == EditorToolType.Group;
		IsGridPaintChecked = tool.Tool == EditorToolType.GridPaint;
		IsShovelChecked = tool.Tool == EditorToolType.Shovel;
		IsFlattenChecked = tool.Tool == EditorToolType.Flatten;
		IsSmoothChecked = tool.Tool == EditorToolType.Smooth;
		IsDragChecked = tool.Tool == EditorToolType.Drag;
		IsRampChecked = tool.Tool == EditorToolType.Ramp;
		IsQuarterPipeChecked = tool.Tool == EditorToolType.QuarterPipe;
		IsHalfPipeChecked = tool.Tool == EditorToolType.HalfPipe;
		IsBowlChecked = tool.Tool == EditorToolType.Bowl;
		IsPyramidChecked = tool.Tool == EditorToolType.Pyramid;
		IsTerrainChecked = tool.Tool == EditorToolType.Terrain;
		IsPortalDiggerChecked = tool.Tool == EditorToolType.PortalDigger;
		IsObjectEraserChecked = tool.Tool == EditorToolType.Eraser;
		IsUVFixerChecked = tool.TextureUVFixer;
		GridSize = tool.GridSize;
	}

	private void UpdateBrushSettings()
	{
		IsBrushShapeCircleChecked = _editor.Configuration.ObjectBrush_Shape == ObjectBrushShape.Circle;
		IsBrushShapeSquareChecked = _editor.Configuration.ObjectBrush_Shape == ObjectBrushShape.Square;
		IsShowTexturesChecked = _editor.Configuration.ObjectBrush_ShowTextures;
	}

	private void UpdateTextureState()
	{
		IsTextureEraserChecked = _editor.SelectedTexture.Texture is null;
		IsTextureInvisibleChecked = _editor.SelectedTexture.Texture is TextureInvisible;
	}

	private void UpdateModeVisibility()
	{
		CurrentMode = _editor.Mode;
	}

	private void OnEditorEventRaised(IEditorEvent obj)
	{
		if (obj is Editor.ToolChangedEvent or Editor.InitEvent)
			UpdateToolCheckedState();

		if (obj is Editor.ObjectBrushSettingsChangedEvent or
			Editor.ConfigurationChangedEvent or
			Editor.InitEvent)
			UpdateBrushSettings();

		if (obj is Editor.SelectedTexturesChangedEvent or Editor.InitEvent)
			UpdateTextureState();

		if (obj is Editor.ModeChangedEvent or Editor.InitEvent)
			UpdateModeVisibility();
	}

	#endregion State Updates
}
