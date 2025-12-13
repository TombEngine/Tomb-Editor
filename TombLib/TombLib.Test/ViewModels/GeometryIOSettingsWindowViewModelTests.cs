using Microsoft.Extensions.DependencyInjection;
using Moq;
using MvvmDialogs;
using System.ComponentModel;
using TombLib.Forms.ViewModels;
using TombLib.GeometryIO;
using TombLib.Services.Abstract;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombLib.Test.ViewModels;

[TestClass]
public class GeometryIOSettingsWindowViewModelTests
{
	private const string TestCustomPresetPath = @"C:\TestPresets.xml";

	private Mock<ICustomGeometrySettingsPresetIOService> _mockPresetIOService = null!;
	private Mock<IDialogService> _mockDialogService = null!;
	private Mock<IMessageService> _mockMessageService = null!;
	private Mock<ILocalizationService> _mockLocalizationService = null!;

	private List<IOGeometrySettingsPreset> _builtInPresets = null!;
	private IOGeometryInternalSettings _exportSettings = null!;
	private IOGeometryInternalSettings _importSettings = null!;

	[TestInitialize]
	public void TestInitialize()
	{
		_mockPresetIOService = new Mock<ICustomGeometrySettingsPresetIOService>();
		_mockDialogService = new Mock<IDialogService>();
		_mockMessageService = new Mock<IMessageService>();
		_mockLocalizationService = new Mock<ILocalizationService>();

		// Setup default behaviour for preset service
		_mockPresetIOService.Setup(x => x.LoadPresets(It.IsAny<string>()))
			.Returns([]);

		_mockPresetIOService.Setup(x => x.SavePresets(It.IsAny<string>(), It.IsAny<IEnumerable<IOGeometrySettingsPreset>>()))
			.Returns(true);

		// Setup localization service
		_mockLocalizationService.Setup(x => x.WithKeysFor(It.IsAny<INotifyPropertyChanged>()))
			.Returns(_mockLocalizationService.Object);

		_mockLocalizationService.Setup(x => x[It.IsAny<string>()])
			.Returns<string>(key => key);

		_mockLocalizationService.Setup(x => x.Format(It.IsAny<string>(), It.IsAny<object[]>()))
			.Returns<string, object[]>(string.Format);

		// Setup ServiceLocator with mocked services for InputBoxWindowViewModel to resolve dependencies
		var services = new ServiceCollection();

		services.AddSingleton(_ => _mockMessageService.Object);
		services.AddTransient(_ => _mockLocalizationService.Object);

		ServiceLocator.Configure(services.BuildServiceProvider());

		// Create test presets
		_builtInPresets =
		[
			new("Default", new IOGeometrySettings { Scale = 1.0f }),
			new("2x Scale", new IOGeometrySettings { Scale = 2.0f }),
			new("Inverted Y", new IOGeometrySettings { FlipY = true })
		];

		// Create test internal settings
		_exportSettings = new IOGeometryInternalSettings
		{
			Export = true,
			ExportRoom = false,
			ProcessGeometry = true,
			ProcessUntexturedGeometry = false,
			ProcessAnimations = false
		};

		_importSettings = new IOGeometryInternalSettings
		{
			Export = false,
			ExportRoom = false,
			ProcessGeometry = true,
			ProcessUntexturedGeometry = true,
			ProcessAnimations = false
		};
	}

	#region Helper Methods

	private GeometryIOSettingsWindowViewModel CreateViewModel(
		List<IOGeometrySettingsPreset>? presets = null,
		IOGeometryInternalSettings? internalSettings = null)
	{
		presets ??= _builtInPresets;

		return new GeometryIOSettingsWindowViewModel(
			presets,
			internalSettings,
			TestCustomPresetPath,
			_mockPresetIOService.Object,
			_mockDialogService.Object,
			_mockMessageService.Object,
			_mockLocalizationService.Object
		);
	}

	#endregion Helper Methods

	#region Constructor Tests

	[TestMethod]
	public void Constructor_WithDefaultParameters_InitializesCorrectly()
	{
		// Act
		var viewModel = CreateViewModel();

		// Assert
		Assert.IsNotNull(viewModel);
		Assert.AreEqual(_builtInPresets.Count, viewModel.AvailablePresets.Count);
		Assert.IsNotNull(viewModel.SelectedPreset);
		Assert.AreEqual(1.0f, viewModel.Scale);
		Assert.IsTrue(viewModel.IsValid);
	}

	[TestMethod]
	public void Constructor_WithExportSettings_SetsCorrectWindowTitleAndCapabilities()
	{
		// Act
		var viewModel = CreateViewModel(internalSettings: _exportSettings);

		// Assert
		Assert.AreEqual("TitleExport", viewModel.WindowTitle);
		Assert.IsTrue(viewModel.CanPackTextures);
	}

	[TestMethod]
	public void Constructor_WithImportSettings_SetsCorrectWindowTitleAndCapabilities()
	{
		// Act
		var viewModel = CreateViewModel(internalSettings: _importSettings);

		// Assert
		Assert.AreEqual("TitleImport", viewModel.WindowTitle);
		Assert.IsTrue(viewModel.CanProcessTextures);
		Assert.IsTrue(viewModel.CanSortByName);
	}

	[TestMethod]
	public void Constructor_WithCustomPresets_LoadsAndCombinesPresets()
	{
		// Arrange
		var customPresets = new List<IOGeometrySettingsPreset>
		{
			new("Custom1", new IOGeometrySettings(), true),
			new("Custom2", new IOGeometrySettings(), true)
		};

		_mockPresetIOService.Setup(x => x.LoadPresets(TestCustomPresetPath))
			.Returns(customPresets);

		// Act
		var viewModel = CreateViewModel();

		// Assert
		Assert.AreEqual(_builtInPresets.Count + customPresets.Count, viewModel.AvailablePresets.Count);
	}

	#endregion Constructor Tests

	#region Property Tests

	[TestMethod]
	public void Scale_WhenSetToZero_IsValidReturnsFalse()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act
		viewModel.Scale = 0.0f;

		// Assert
		Assert.IsFalse(viewModel.IsValid);
	}

	[TestMethod]
	public void Scale_WhenSetToNegative_IsValidReturnsFalse()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act
		viewModel.Scale = -1.0f;

		// Assert
		Assert.IsFalse(viewModel.IsValid);
	}

	[TestMethod]
	public void Scale_WhenSetToPositive_IsValidReturnsTrue()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act
		viewModel.Scale = 2.5f;

		// Assert
		Assert.IsTrue(viewModel.IsValid);
		Assert.AreEqual(2.5f, viewModel.Scale);
	}

	[TestMethod]
	public void CanInvertFaces_WhenProcessAnimationsIsTrue_ReturnsFalse()
	{
		// Arrange
		var animationSettings = new IOGeometryInternalSettings
		{
			ProcessAnimations = true
		};

		var viewModel = CreateViewModel(internalSettings: animationSettings);

		// Assert
		Assert.IsFalse(viewModel.CanInvertFaces);
	}

	[TestMethod]
	public void CanPadPackedTextures_DependsOnPackTextures()
	{
		// Arrange
		var viewModel = CreateViewModel(internalSettings: _exportSettings);

		// Act & Assert
		viewModel.PackTextures = true;
		Assert.IsTrue(viewModel.CanPadPackedTextures);

		viewModel.PackTextures = false;
		Assert.IsFalse(viewModel.CanPadPackedTextures);
	}

	#endregion Property Tests

	#region Preset Management Tests

	[TestMethod]
	public void SelectPreset_WithValidNameFilter_SelectsCorrectPreset()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act
		viewModel.SelectPreset("2x");

		// Assert
		Assert.AreEqual("2x Scale", viewModel.SelectedPreset.Name);
		Assert.AreEqual(2.0f, viewModel.Scale);
	}

	[TestMethod]
	public void SelectPreset_WithInvalidNameFilter_DoesNotChangeSelection()
	{
		// Arrange
		var viewModel = CreateViewModel();
		var originalPreset = viewModel.SelectedPreset;

		// Act
		viewModel.SelectPreset("NonExistent");

		// Assert
		Assert.AreEqual(originalPreset, viewModel.SelectedPreset);
	}

	[TestMethod]
	public void SelectedPreset_WhenChanged_UpdatesAllSettings()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act
		viewModel.SelectedPreset = _builtInPresets.First(p => p.Name == "Inverted Y");

		// Assert
		Assert.IsTrue(viewModel.InvertYAxis);
		Assert.IsFalse(viewModel.InvertXAxis);
		Assert.IsTrue(viewModel.InvertZAxis);
	}

	[TestMethod]
	public void MatchingPreset_WhenSettingsMatchExistingPreset_ReturnsCorrectPreset()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act
		viewModel.Scale = 2.0f;

		// Assert
		Assert.AreEqual("2x Scale", viewModel.MatchingPreset.Name);
	}

	[TestMethod]
	public void MatchingPreset_WhenSettingsDoNotMatch_ReturnsUnsavedPreset()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act
		viewModel.Scale = 3.5f; // Not matching any preset

		// Assert
		Assert.AreEqual("CustomPresetName", viewModel.MatchingPreset.Name);
		Assert.IsTrue(viewModel.AvailablePresets.Any(p => p.Name == "CustomPresetName"));
	}

	[TestMethod]
	public void IsSelectedPresetCustom_WithBuiltInPreset_ReturnsFalse()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act
		viewModel.SelectedPreset = _builtInPresets[0];

		// Assert
		Assert.IsFalse(viewModel.IsSelectedPresetCustom);
	}

	[TestMethod]
	public void IsSelectedPresetCustom_WithCustomPreset_ReturnsTrue()
	{
		// Arrange
		var customPreset = new IOGeometrySettingsPreset("Custom", new IOGeometrySettings(), true);
		var presets = _builtInPresets.Concat([customPreset]).ToList();
		var viewModel = CreateViewModel(presets);

		// Act
		viewModel.SelectedPreset = customPreset;

		// Assert
		Assert.IsTrue(viewModel.IsSelectedPresetCustom);
	}

	#endregion Preset Management Tests

	#region GetCurrentSettings Tests

	[TestMethod]
	public void GetCurrentSettings_ReturnsCorrectSettings()
	{
		// Arrange
		var viewModel = CreateViewModel(internalSettings: _exportSettings);

		// Set some values
		viewModel.SwapXYAxes = true;
		viewModel.InvertXAxis = true;
		viewModel.Scale = 2.5f;
		viewModel.PackTextures = true;

		// Act
		var settings = viewModel.GetCurrentSettings();

		// Assert
		Assert.IsTrue(settings.Export);
		Assert.IsTrue(settings.ProcessGeometry);
		Assert.IsTrue(settings.SwapXY);
		Assert.IsTrue(settings.FlipX);
		Assert.AreEqual(2.5f, settings.Scale);
		Assert.IsTrue(settings.PackTextures);
	}

	#endregion GetCurrentSettings Tests

	#region Command Tests

	[TestMethod]
	public void ConfirmCommand_WhenIsValidTrue_SetsDialogResultToTrue()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act
		viewModel.ConfirmCommand.Execute(null);

		// Assert
		Assert.IsTrue(viewModel.DialogResult);
	}

	[TestMethod]
	public void ConfirmCommand_WhenIsValidFalse_CannotExecute()
	{
		// Arrange
		var viewModel = CreateViewModel();
		viewModel.Scale = 0; // Invalid scale

		// Act & Assert
		Assert.IsFalse(viewModel.ConfirmCommand.CanExecute(null));
	}

	[TestMethod]
	public void CancelCommand_SetsDialogResultToFalse()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act
		viewModel.CancelCommand.Execute(null);

		// Assert
		Assert.IsFalse(viewModel.DialogResult);
	}

	[TestMethod]
	public void SavePresetCommand_WhenUserEntersValidName_CreatesNewPreset()
	{
		// Arrange
		var viewModel = CreateViewModel();
		var originalCount = viewModel.AvailablePresets.Count;

		_mockDialogService.Setup(x => x.ShowDialog(viewModel, It.IsAny<InputBoxWindowViewModel>()))
			.Callback<object, object>((_, vm) =>
				// Simulate user entering a valid name
				(vm as InputBoxWindowViewModel)!.Value = "New Preset")
			.Returns(true);

		// Act
		viewModel.SavePresetCommand.Execute(null);

		// Assert
		_mockDialogService.Verify(x => x.ShowDialog(viewModel, It.IsAny<InputBoxWindowViewModel>()), Times.Once);
		Assert.AreEqual(originalCount + 1, viewModel.AvailablePresets.Count);
		Assert.IsTrue(viewModel.AvailablePresets.Any(p => p.Name == "New Preset" && p.IsCustom));
		_mockPresetIOService.Verify(x => x.SavePresets(TestCustomPresetPath, It.IsAny<IEnumerable<IOGeometrySettingsPreset>>()), Times.Once);
	}

	[TestMethod]
	public void SavePresetCommand_WhenUserCancels_DoesNotCreatePreset()
	{
		// Arrange
		var viewModel = CreateViewModel();

		_mockDialogService.Setup(x => x.ShowDialog(viewModel, It.IsAny<InputBoxWindowViewModel>()))
			.Returns(false);

		var originalCount = viewModel.AvailablePresets.Count;

		// Act
		viewModel.SavePresetCommand.Execute(null);

		// Assert
		Assert.AreEqual(originalCount, viewModel.AvailablePresets.Count);
		_mockPresetIOService.Verify(x => x.SavePresets(It.IsAny<string>(), It.IsAny<IEnumerable<IOGeometrySettingsPreset>>()), Times.Never);
	}

	[TestMethod]
	public void DeletePresetCommand_WithCustomPreset_DeletesPreset()
	{
		// Arrange
		var customPreset = new IOGeometrySettingsPreset("Custom", new IOGeometrySettings(), true);
		var presets = _builtInPresets.Concat([customPreset]).ToList();
		var viewModel = CreateViewModel(presets);

		viewModel.SelectedPreset = customPreset;

		_mockMessageService.Setup(x => x.ShowConfirmation(
			It.IsAny<string>(),
			It.IsAny<string>(),
			It.IsAny<bool?>(),
			It.IsAny<bool>()))
			.Returns(true);

		// Act
		viewModel.DeletePresetCommand.Execute(null);

		// Assert
		Assert.IsFalse(viewModel.AvailablePresets.Contains(customPreset));
		_mockPresetIOService.Verify(x => x.SavePresets(TestCustomPresetPath, It.IsAny<IEnumerable<IOGeometrySettingsPreset>>()), Times.Once);
	}

	[TestMethod]
	public void DeletePresetCommand_WithBuiltInPreset_CannotExecute()
	{
		// Arrange
		var viewModel = CreateViewModel();
		viewModel.SelectedPreset = _builtInPresets[0];

		// Act & Assert
		Assert.IsFalse(viewModel.DeletePresetCommand.CanExecute(null));
	}

	#endregion Command Tests

	#region Property Change Notification Tests

	[TestMethod]
	public void PropertyChanges_UpdateMatchingPreset()
	{
		// Arrange
		var viewModel = CreateViewModel();
		var originalMatchingPreset = viewModel.MatchingPreset;

		// Act
		viewModel.SwapXYAxes = true; // This should change the matching preset

		// Assert
		Assert.AreNotEqual(originalMatchingPreset, viewModel.MatchingPreset);
	}

	[TestMethod]
	public void PackTextures_PropertyChange_NotifiesCanPadPackedTextures()
	{
		// Arrange
		var viewModel = CreateViewModel(internalSettings: _exportSettings);

		// Act
		viewModel.PackTextures = true;

		// Assert
		Assert.IsTrue(viewModel.CanPadPackedTextures);
	}

	#endregion Property Change Notification Tests

	#region Edge Case Tests

	[TestMethod]
	public void UnsavedPreset_WhenDeselected_IsRemovedFromCollection()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Force creation of unsaved preset
		viewModel.Scale = 3.5f; // Non-matching value
		var unsavedPreset = viewModel.MatchingPreset;
		Assert.AreEqual("CustomPresetName", unsavedPreset.Name);

		// Act
		viewModel.SelectedPreset = _builtInPresets[0]; // Select a different preset

		// Assert
		Assert.IsFalse(viewModel.AvailablePresets.Any(p => p.Name == "CustomPresetName"));
	}

	[TestMethod]
	public void RecursiveUpdate_Prevention_WhenApplyingPreset()
	{
		// Arrange
		var viewModel = CreateViewModel();
		int propertyChangeCount = 0;

		viewModel.PropertyChanged += (sender, e) =>
		{
			if (e.PropertyName == nameof(viewModel.MatchingPreset))
				propertyChangeCount++;
		};

		// Act - This should not cause recursive updates
		viewModel.SelectedPreset = _builtInPresets.First(p => p.Name == "2x Scale");

		// Assert - Only one change notification should have occurred
		Assert.IsTrue(propertyChangeCount == 1);
	}

	#endregion Edge Case Tests
}
