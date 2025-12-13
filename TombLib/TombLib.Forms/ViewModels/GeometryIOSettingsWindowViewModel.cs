#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TombLib.GeometryIO;
using TombLib.Services.Abstract;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombLib.Forms.ViewModels;

public partial class GeometryIOSettingsWindowViewModel : ObservableObject, IModalDialogViewModel
{
	private const string GeometryImportPresetsConfigFileName = "GeometryImportPresets.xml";
	private const string GeometryExportPresetsConfigFileName = "GeometryExportPresets.xml";
	private const string AnimationPresetsConfigFileName = "AnimationPresets.xml";

	#region Properties and Fields

	[ObservableProperty] private bool? _dialogResult;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(WindowTitle))]
	[NotifyPropertyChangedFor(nameof(CanInvertFaces))]
	[NotifyPropertyChangedFor(nameof(CanProcessTextures))]
	[NotifyPropertyChangedFor(nameof(CanProcessGeometry))]
	[NotifyPropertyChangedFor(nameof(CanSortByName))]
	[NotifyPropertyChangedFor(nameof(CanPackTextures))]
	[NotifyPropertyChangedFor(nameof(CanPadPackedTextures))]
	[NotifyPropertyChangedFor(nameof(CanKeepTexturesExternal))]
	private IOGeometryInternalSettings _internalSettings;

	public string WindowTitle => InternalSettings.Export
		? _localizationService["TitleExport"]
		: _localizationService["TitleImport"];

	public bool CanInvertFaces => InternalSettings.ProcessGeometry;
	public bool CanProcessTextures => InternalSettings.ProcessGeometry && !InternalSettings.Export;
	public bool CanProcessGeometry => InternalSettings.ProcessGeometry;
	public bool CanSortByName => !InternalSettings.Export;
	public bool CanPackTextures => InternalSettings.Export && !InternalSettings.ExportRoom;
	public bool CanPadPackedTextures => CanPackTextures && PackTextures;
	public bool CanKeepTexturesExternal => !InternalSettings.Export && InternalSettings.ImportWadMesh;

	/* Axis Properties */

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _swapXYAxes;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _swapXZAxes;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _swapYZAxes;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _invertXAxis;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _invertYAxis;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _invertZAxis;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _invertFaces;

	/* Size Properties */

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsValid))]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private float _scale = 1.0f;

	/* Texture Mapping Properties */

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _invertVCoordinate;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _uvMapped;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _wrapUV;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _premultiplyUV;

	/* Misc. Properties */

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _vertexColorLight;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	[NotifyPropertyChangedFor(nameof(CanPadPackedTextures))]
	private bool _packTextures;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _padPackedTextures;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _sortByName;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _keepTexturesExternal;

	/* Preset Properties */

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsSelectedPresetCustom))]
	[NotifyPropertyChangedFor(nameof(IsSelectedPresetUnsaved))]
	[NotifyCanExecuteChangedFor(nameof(SavePresetCommand))]
	[NotifyCanExecuteChangedFor(nameof(DeletePresetCommand))]
	private IOGeometrySettingsPreset _selectedPreset;

	/// <summary>
	/// Indicates whether the selected preset is a custom user-defined preset.
	/// </summary>
	public bool IsSelectedPresetCustom => SelectedPreset?.IsCustom is true;

	/// <summary>
	/// Indicates whether the selected preset is the unsaved preset.
	/// </summary>
	public bool IsSelectedPresetUnsaved => SelectedPreset?.Name == _unsavedPresetName;

	/// <summary>
	/// Collection of available presets. This includes built-in and user-defined presets.
	/// </summary>
	public ObservableCollection<IOGeometrySettingsPreset> AvailablePresets { get; } = [];

	/// <summary>
	/// Indicates whether the current settings are valid.
	/// </summary>
	public bool IsValid => Scale > 0;

	/// <summary>
	/// Property to find matching preset based on current settings.
	/// </summary>
	public IOGeometrySettingsPreset MatchingPreset
	{
		get
		{
			IOGeometrySettingsPreset? matchingPreset = FindMatchingPreset();

			if (matchingPreset is null)
			{
				if (_unsavedPreset is null)
				{
					_unsavedPreset = new IOGeometrySettingsPreset(_unsavedPresetName, new IOGeometrySettings());
					AvailablePresets.Insert(0, _unsavedPreset);
				}

				return _unsavedPreset;
			}

			return matchingPreset;
		}
	}

	/// <summary>
	/// Custom preset for when settings don't match any preset.
	/// </summary>
	private IOGeometrySettingsPreset? _unsavedPreset;

	/// <summary>
	/// Flag to prevent recursive updates when applying a preset.
	/// </summary>
	private bool _applyingSelectedPreset;

	private readonly string _unsavedPresetName;
	private readonly string _customPresetConfigFilePath;

	/* Services */

	private readonly ICustomGeometrySettingsPresetIOService _customPresetIOService;
	private readonly IDialogService _dialogService;
	private readonly IMessageService _messageService;
	private readonly ILocalizationService _localizationService;

	#endregion Properties and Fields

	public GeometryIOSettingsWindowViewModel(
		IReadOnlyList<IOGeometrySettingsPreset> builtInPresets,
		IOGeometryInternalSettings? internalSettings = null,
		string? customPresetConfigFilePath = null,
		ICustomGeometrySettingsPresetIOService? customPresetIOService = null,
		IDialogService? dialogService = null,
		IMessageService? messageService = null,
		ILocalizationService? localizationService = null)
	{
		// Services
		_customPresetIOService = ServiceLocator.ResolveService(customPresetIOService);
		_dialogService = ServiceLocator.ResolveService(dialogService);
		_messageService = ServiceLocator.ResolveService(messageService);
		_localizationService = ServiceLocator.ResolveService(localizationService)
			.WithKeysFor(this);

		// Properties and Fields
		InternalSettings = internalSettings ?? new();

		string targetConfigFileName = internalSettings switch
		{
			{ IsGeometryExportSettings: true } => GeometryExportPresetsConfigFileName,
			{ IsAnimationSettings: true } => AnimationPresetsConfigFileName,
			_ => GeometryImportPresetsConfigFileName
		};

		_customPresetConfigFilePath = customPresetConfigFilePath
			?? Path.Combine(DefaultPaths.GeometryIOConfigsDirectory, targetConfigFileName);

		_unsavedPresetName = _localizationService["CustomPresetName"];

		var customPresets = _customPresetIOService.LoadPresets(_customPresetConfigFilePath);
		var allPresets = builtInPresets.Concat(customPresets).ToHashSet();

		AvailablePresets = new(allPresets);
		SelectedPreset = AvailablePresets.First();
	}

	#region Public Methods

	/// <summary>
	/// Selects a preset whose name contains the specified filter string.
	/// <para>If multiple presets match, the first one found is selected.</para>
	/// <para>If no presets match, no changes are made.</para>
	/// </summary>
	/// <param name="nameFilter">The substring to search for in preset names.</param>
	public void SelectPreset(string nameFilter)
	{
		if (string.IsNullOrWhiteSpace(nameFilter))
			return;

		IOGeometrySettingsPreset? presetToSelect = AvailablePresets
			.FirstOrDefault(p => p.Name.Contains(nameFilter));

		if (presetToSelect is not null)
			SelectedPreset = presetToSelect;
	}

	/// <summary>
	/// Gets the current geometry settings based on provided internal settings and user selections.
	/// </summary>
	public IOGeometrySettings GetCurrentSettings() => new()
	{
		// Internal settings
		Export = InternalSettings.Export,
		ExportRoom = InternalSettings.ExportRoom,
		ImportWadMesh = InternalSettings.ImportWadMesh,
		ProcessGeometry = InternalSettings.ProcessGeometry,
		ProcessUntexturedGeometry = InternalSettings.ProcessUntexturedGeometry,
		ProcessAnimations = InternalSettings.ProcessAnimations,

		SwapXY = SwapXYAxes,
		SwapXZ = SwapXZAxes,
		SwapYZ = SwapYZAxes,
		FlipX = InvertXAxis,
		FlipY = InvertYAxis,
		FlipZ = InvertZAxis,
		InvertFaces = InvertFaces,
		Scale = Scale,
		FlipUV_V = InvertVCoordinate,
		MappedUV = UvMapped,
		WrapUV = WrapUV,
		PremultiplyUV = PremultiplyUV,
		UseVertexColor = VertexColorLight,
		PackTextures = PackTextures,
		PadPackedTextures = PadPackedTextures,
		SortByName = SortByName,
		KeepTexturesExternal = KeepTexturesExternal
	};

	#endregion Public Methods

	#region Partial Methods for Property Changes

	partial void OnSwapXYAxesChanged(bool value) => UpdateSelectedPreset();
	partial void OnSwapXZAxesChanged(bool value) => UpdateSelectedPreset();
	partial void OnSwapYZAxesChanged(bool value) => UpdateSelectedPreset();
	partial void OnInvertXAxisChanged(bool value) => UpdateSelectedPreset();
	partial void OnInvertYAxisChanged(bool value) => UpdateSelectedPreset();
	partial void OnInvertZAxisChanged(bool value) => UpdateSelectedPreset();
	partial void OnInvertFacesChanged(bool value) => UpdateSelectedPreset();
	partial void OnScaleChanged(float value) => UpdateSelectedPreset();
	partial void OnInvertVCoordinateChanged(bool value) => UpdateSelectedPreset();
	partial void OnUvMappedChanged(bool value) => UpdateSelectedPreset();
	partial void OnWrapUVChanged(bool value) => UpdateSelectedPreset();
	partial void OnPremultiplyUVChanged(bool value) => UpdateSelectedPreset();
	partial void OnVertexColorLightChanged(bool value) => UpdateSelectedPreset();
	partial void OnPackTexturesChanged(bool value) => UpdateSelectedPreset();
	partial void OnPadPackedTexturesChanged(bool value) => UpdateSelectedPreset();
	partial void OnSortByNameChanged(bool value) => UpdateSelectedPreset();
	partial void OnKeepTexturesExternalChanged(bool value) => UpdateSelectedPreset();

	partial void OnSelectedPresetChanging(IOGeometrySettingsPreset? oldValue, IOGeometrySettingsPreset newValue)
	{
		if (oldValue?.Name == _unsavedPresetName)
			RemoveUnsavedPreset();
	}

	partial void OnSelectedPresetChanged(IOGeometrySettingsPreset value)
	{
		if (value is null || value.Name == _unsavedPresetName)
			return;

		_applyingSelectedPreset = true; // Prevent recursive updates

		// Update all settings based on the selected preset
		SwapXYAxes = value.Settings.SwapXY;
		SwapXZAxes = value.Settings.SwapXZ;
		SwapYZAxes = value.Settings.SwapYZ;
		InvertXAxis = value.Settings.FlipX;
		InvertYAxis = value.Settings.FlipY;
		InvertZAxis = value.Settings.FlipZ;
		InvertFaces = value.Settings.InvertFaces;
		Scale = value.Settings.Scale;
		InvertVCoordinate = value.Settings.FlipUV_V;
		UvMapped = value.Settings.MappedUV;
		WrapUV = value.Settings.WrapUV;
		PremultiplyUV = value.Settings.PremultiplyUV;
		VertexColorLight = value.Settings.UseVertexColor;
		PackTextures = value.Settings.PackTextures;
		PadPackedTextures = value.Settings.PadPackedTextures;
		SortByName = value.Settings.SortByName;
		KeepTexturesExternal = value.Settings.KeepTexturesExternal;

		_applyingSelectedPreset = false;
	}

	#endregion Partial Methods for Property Changes

	#region Relay Commands

	[RelayCommand(CanExecute = nameof(IsValid))]
	private void Confirm()
	{
		DialogResult = true;
		_dialogService.Close(this);
	}

	[RelayCommand]
	private void Cancel()
	{
		DialogResult = false;
		_dialogService.Close(this);
	}

	[RelayCommand(CanExecute = nameof(IsValid))]
	private void SavePreset()
	{
		var inputBox = new InputBoxWindowViewModel(
			title: _localizationService["SavePresetTitle"],
			label: _localizationService["EnterPresetName"],
			invalidNames: [_unsavedPresetName]
		);

		bool? result = _dialogService.ShowDialog(this, inputBox);

		if (result is not true)
			return;

		string presetName = inputBox.Value.Trim();

		if (string.IsNullOrWhiteSpace(presetName))
			return;

		bool presetAlreadyExists = AvailablePresets.Any(p => p.Name == presetName);

		if (presetAlreadyExists)
		{
			_messageService.ShowError(_localizationService["PresetNameAlreadyExists"]);
			return;
		}

		// Create a new preset and add it to the list
		var newPreset = new IOGeometrySettingsPreset(presetName, GetCurrentSettings(), isCustom: true);

		// Add the new preset and select it
		AvailablePresets.Add(newPreset);
		SelectedPreset = newPreset;

		SaveCustomPresets();
	}

	[RelayCommand(CanExecute = nameof(IsSelectedPresetCustom))]
	private void DeletePreset()
	{
		if (SelectedPreset is null)
			return;

		bool deleteConfirmed = _messageService.ShowConfirmation(
			_localizationService.Format("DeletePresetMessage", SelectedPreset.Name),
			_localizationService["DeletePresetTitle"],
			defaultValue: false,
			isRisky: true
		);

		if (deleteConfirmed)
		{
			AvailablePresets.Remove(SelectedPreset);
			SelectedPreset = AvailablePresets.First();

			SaveCustomPresets();
		}
	}

	#endregion Relay Commands

	#region Private Methods

	/// <summary>
	/// Finds a preset that matches the current settings.
	/// </summary>
	private IOGeometrySettingsPreset? FindMatchingPreset()
	{
		foreach (IOGeometrySettingsPreset preset in AvailablePresets)
		{
			if (preset.Name == _unsavedPresetName)
				continue; // Skip the unsaved preset

			bool isMatch = preset.Settings.SwapXY == SwapXYAxes
				&& preset.Settings.SwapXZ == SwapXZAxes
				&& preset.Settings.SwapYZ == SwapYZAxes
				&& preset.Settings.FlipX == InvertXAxis
				&& preset.Settings.FlipY == InvertYAxis
				&& preset.Settings.FlipZ == InvertZAxis
				&& preset.Settings.InvertFaces == InvertFaces
				&& preset.Settings.Scale == Scale
				&& preset.Settings.FlipUV_V == InvertVCoordinate
				&& preset.Settings.MappedUV == UvMapped
				&& preset.Settings.WrapUV == WrapUV
				&& preset.Settings.PremultiplyUV == PremultiplyUV
				&& preset.Settings.UseVertexColor == VertexColorLight
				&& preset.Settings.PackTextures == PackTextures
				&& preset.Settings.PadPackedTextures == PadPackedTextures
				&& preset.Settings.SortByName == SortByName
				&& preset.Settings.KeepTexturesExternal == KeepTexturesExternal;

			if (isMatch)
				return preset;
		}

		return null;
	}

	/// <summary>
	/// Updates the selected preset based on current settings.
	/// </summary>
	private void UpdateSelectedPreset()
	{
		if (_applyingSelectedPreset) // Prevent recursive updates
			return;

		IOGeometrySettingsPreset matchingPreset = MatchingPreset;

		if (SelectedPreset != matchingPreset)
			SelectedPreset = matchingPreset;
	}

	/// <summary>
	/// Removes the unsaved preset if it exists.
	/// </summary>
	private void RemoveUnsavedPreset()
	{
		if (_unsavedPreset is not null)
		{
			AvailablePresets.Remove(_unsavedPreset);
			_unsavedPreset = null;
		}
	}

	/// <summary>
	/// Saves custom presets to the configuration file.
	/// </summary>
	private void SaveCustomPresets()
	{
		_customPresetIOService.SavePresets(
			_customPresetConfigFilePath,
			AvailablePresets.Where(p => p.IsCustom)
		);
	}

	#endregion Private Methods
}
