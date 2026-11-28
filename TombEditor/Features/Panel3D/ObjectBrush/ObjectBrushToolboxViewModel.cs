#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using TombLib.LevelData;

namespace TombEditor.Features.Panel3D.ObjectBrush;

/// <summary>
/// ViewModel for the Object Brush Toolbox panel.
/// Manages brush configuration settings (radius, density, rotation, scale, etc.)
/// and determines which controls are enabled based on the current tool.
/// </summary>
public partial class ObjectBrushToolboxViewModel : ObservableObject
{
	private readonly Editor _editor;
	private bool _isLoadingSettings;
	private bool _isSavingSettings;

	public ObjectBrushToolboxViewModel()
	{
		_editor = Editor.Instance;
		_editor.EditorEventRaised += OnEditorEventRaised;
		LoadSettings();
	}

	public void Cleanup()
	{
		_editor.EditorEventRaised -= OnEditorEventRaised;
	}

	#region Settings Properties

	// Brush radius in sectors (displayed in UI). Stored in config as world units.
	[ObservableProperty] private double _radius = 0.5;
	[ObservableProperty] private double _density = 1.0;
	[ObservableProperty] private double _rotation;
	[ObservableProperty] private bool _isOrthogonal;
	[ObservableProperty] private bool _isRandomRotation;
	[ObservableProperty] private bool _isFollowMouseDirection;
	[ObservableProperty] private bool _isRandomScale;
	[ObservableProperty] private double _scaleMin = 0.8;
	[ObservableProperty] private double _scaleMax = 1.2;
	[ObservableProperty] private bool _isFitToGround;
	[ObservableProperty] private bool _isAlignToGrid;
	[ObservableProperty] private bool _isPlaceInAdjacentRooms;

	#endregion Settings Properties

	#region Enabled States

	[ObservableProperty] private bool _isRadiusEnabled = true;
	[ObservableProperty] private bool _isDensityEnabled;
	[ObservableProperty] private bool _isRotationEnabled;
	[ObservableProperty] private bool _isOrthogonalEnabled;
	[ObservableProperty] private bool _isRandomRotationEnabled;
	[ObservableProperty] private bool _isFollowMouseDirectionEnabled;
	[ObservableProperty] private bool _isRandomScaleEnabled;
	[ObservableProperty] private bool _isScaleMinEnabled;
	[ObservableProperty] private bool _isScaleMaxEnabled;
	[ObservableProperty] private bool _isFitToGroundEnabled;
	[ObservableProperty] private bool _isAlignToGridEnabled;
	[ObservableProperty] private bool _isAdjacentRoomsEnabled;

	#endregion Enabled States

	#region Property Change Handlers

	partial void OnRadiusChanged(double value) => SaveSettingsIfNotLoading();
	partial void OnDensityChanged(double value) => SaveSettingsIfNotLoading();
	partial void OnRotationChanged(double value) => SaveSettingsIfNotLoading();
	partial void OnIsOrthogonalChanged(bool value) => SaveSettingsIfNotLoading();
	partial void OnScaleMinChanged(double value) => SaveSettingsIfNotLoading();
	partial void OnScaleMaxChanged(double value) => SaveSettingsIfNotLoading();
	partial void OnIsFitToGroundChanged(bool value) => SaveSettingsIfNotLoading();
	partial void OnIsAlignToGridChanged(bool value) => SaveSettingsIfNotLoading();
	partial void OnIsPlaceInAdjacentRoomsChanged(bool value) => SaveSettingsIfNotLoading();

	partial void OnIsRandomRotationChanged(bool value)
	{
		SaveSettingsIfNotLoading();
		UpdateControlsForTool();
	}

	partial void OnIsFollowMouseDirectionChanged(bool value)
	{
		SaveSettingsIfNotLoading();
		UpdateControlsForTool();
	}

	partial void OnIsRandomScaleChanged(bool value)
	{
		SaveSettingsIfNotLoading();
		UpdateControlsForTool();
	}

	#endregion Property Change Handlers

	private void SaveSettingsIfNotLoading()
	{
		if (_isLoadingSettings)
			return;

		SaveSettings();
	}

	private void LoadSettings()
	{
		_isLoadingSettings = true;

		var config = _editor.Configuration;

		Radius = Math.Clamp(config.ObjectBrush_Radius / Level.SectorSizeUnit, Constants.MinRadius, Constants.MaxRadius);
		Density = Math.Clamp(config.ObjectBrush_Density, Constants.MinDensity, Constants.MaxDensity);
		Rotation = Math.Clamp(config.ObjectBrush_Rotation, 0.0, 360.0);
		IsOrthogonal = config.ObjectBrush_Orthogonal;
		IsRandomRotation = config.ObjectBrush_RandomizeRotation;
		IsFollowMouseDirection = config.ObjectBrush_FollowMouseDirection;
		IsRandomScale = config.ObjectBrush_RandomizeScale;
		ScaleMin = Math.Clamp(config.ObjectBrush_ScaleMin, 0.1, 10.0);
		ScaleMax = Math.Clamp(config.ObjectBrush_ScaleMax, 0.1, 10.0);
		IsFitToGround = config.ObjectBrush_FitToGround;
		IsAlignToGrid = config.ObjectBrush_AlignToGrid;
		IsPlaceInAdjacentRooms = config.ObjectBrush_PlaceInAdjacentRooms;

		_isLoadingSettings = false;

		UpdateControlsForTool();
	}

	private void SaveSettings()
	{
		_isSavingSettings = true;

		var config = _editor.Configuration;

		config.ObjectBrush_Radius = (float)Radius * Level.SectorSizeUnit;
		config.ObjectBrush_Density = (float)Density;
		config.ObjectBrush_Rotation = (float)Rotation;
		config.ObjectBrush_Orthogonal = IsOrthogonal;
		config.ObjectBrush_RandomizeRotation = IsRandomRotation;
		config.ObjectBrush_FollowMouseDirection = IsFollowMouseDirection;
		config.ObjectBrush_RandomizeScale = IsRandomScale;
		config.ObjectBrush_ScaleMin = (float)ScaleMin;
		config.ObjectBrush_ScaleMax = (float)ScaleMax;
		config.ObjectBrush_FitToGround = IsFitToGround;
		config.ObjectBrush_AlignToGrid = IsAlignToGrid;
		config.ObjectBrush_PlaceInAdjacentRooms = IsPlaceInAdjacentRooms;

		_editor.ObjectBrushSettingsChange();

		_isSavingSettings = false;
	}

	private void UpdateControlsForTool()
	{
		if (_editor.Mode != EditorMode.ObjectPlacement)
			return;

		var tool = _editor.Tool.Tool;
		bool isBrush = tool == EditorToolType.Brush;
		bool isPencil = tool == EditorToolType.Pencil;
		bool isLine = tool == EditorToolType.Line;
		bool isEraser = tool == EditorToolType.Eraser;
		bool isFill = tool == EditorToolType.Fill;
		bool isSelection = tool == EditorToolType.Selection;

		IsRadiusEnabled = !isFill && !isSelection;
		IsDensityEnabled = isBrush || isEraser || isFill;
		IsAdjacentRoomsEnabled = isBrush || isEraser || isPencil || isLine;

		bool allowRotation = isBrush || isPencil || isLine || isFill;

		IsOrthogonalEnabled = allowRotation;
		IsRandomRotationEnabled = allowRotation && !isLine;
		IsFollowMouseDirectionEnabled = isBrush || isPencil;
		IsRotationEnabled = isLine || (allowRotation && !IsRandomRotation && !IsFollowMouseDirection);

		bool allowScale = isBrush || isPencil || isLine || isFill;

		IsFitToGroundEnabled = allowScale;
		IsAlignToGridEnabled = isLine || isPencil;
		IsRandomScaleEnabled = allowScale;
		IsScaleMinEnabled = allowScale && IsRandomScale;
		IsScaleMaxEnabled = allowScale && IsRandomScale;
	}

	private void OnEditorEventRaised(IEditorEvent obj)
	{
		if (!_isSavingSettings &&
			obj is Editor.ConfigurationChangedEvent or
			Editor.ObjectBrushSettingsChangedEvent)
			LoadSettings();

		if (obj is Editor.ModeChangedEvent or
			Editor.ToolChangedEvent)
			UpdateControlsForTool();
	}
}
