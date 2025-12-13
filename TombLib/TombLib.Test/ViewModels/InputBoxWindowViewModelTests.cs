using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.ComponentModel;
using TombLib.Forms.ViewModels;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombLib.Test.ViewModels;

[TestClass]
public class InputBoxWindowViewModelTests
{
	private Mock<IMessageService> _mockMessageService = null!;
	private Mock<ILocalizationService> _mockLocalizationService = null!;

	private List<string> _invalidNames = null!;

	[TestInitialize]
	public void TestInitialize()
	{
		_mockMessageService = new Mock<IMessageService>();
		_mockLocalizationService = new Mock<ILocalizationService>();

		// Setup localization service
		_mockLocalizationService.Setup(x => x.WithKeysFor(It.IsAny<INotifyPropertyChanged>()))
			.Returns(_mockLocalizationService.Object);

		_mockLocalizationService.Setup(x => x[It.IsAny<string>()])
			.Returns<string>(key => key);

		// Setup ServiceLocator with mocked services
		var services = new ServiceCollection();

		services.AddSingleton(_ => _mockMessageService.Object);
		services.AddSingleton(_ => _mockLocalizationService.Object);

		ServiceLocator.Configure(services.BuildServiceProvider());

		// Create test invalid names
		_invalidNames = ["InvalidName1", "InvalidName2", "ReservedName"];
	}

	#region Helper Methods

	private InputBoxWindowViewModel CreateViewModel(
		string? title = null,
		string? label = null,
		string? placeholder = null,
		IEnumerable<string>? invalidNames = null)
	{
		return new InputBoxWindowViewModel(
			title,
			label,
			placeholder,
			invalidNames,
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
		Assert.AreEqual(string.Empty, viewModel.Title);
		Assert.AreEqual("EnterValue", viewModel.Label);
		Assert.AreEqual(string.Empty, viewModel.Value);
		Assert.IsFalse(viewModel.IsValueNotEmpty);
		Assert.IsNull(viewModel.DialogResult);
	}

	[TestMethod]
	public void Constructor_WithTitle_SetsTitle()
	{
		// Arrange
		const string title = "Test Title";

		// Act
		var viewModel = CreateViewModel(title: title);

		// Assert
		Assert.AreEqual(title, viewModel.Title);
	}

	[TestMethod]
	public void Constructor_WithLabel_SetsLabel()
	{
		// Arrange
		const string label = "Test Label";

		// Act
		var viewModel = CreateViewModel(label: label);

		// Assert
		Assert.AreEqual(label, viewModel.Label);
	}

	[TestMethod]
	public void Constructor_WithPlaceholder_SetsValue()
	{
		// Arrange
		const string placeholder = "Test Placeholder";

		// Act
		var viewModel = CreateViewModel(placeholder: placeholder);

		// Assert
		Assert.AreEqual(placeholder, viewModel.Value);
		Assert.IsTrue(viewModel.IsValueNotEmpty);
	}

	[TestMethod]
	public void Constructor_CallsLocalizationServiceWithKeysFor()
	{
		// Act
		var viewModel = CreateViewModel();

		// Assert
		_mockLocalizationService.Verify(x => x.WithKeysFor(It.IsAny<InputBoxWindowViewModel>()), Times.Once);
	}

	[TestMethod]
	public void Constructor_WithNullLabel_UsesLocalizedDefault()
	{
		// Act
		var viewModel = CreateViewModel();

		// Assert
		Assert.AreEqual("EnterValue", viewModel.Label);
		_mockLocalizationService.Verify(x => x["EnterValue"], Times.Once);
	}

	#endregion Constructor Tests

	#region Property Tests

	[TestMethod]
	public void Value_WhenSetToEmptyString_IsValueNotEmptyReturnsFalse()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act
		viewModel.Value = string.Empty;

		// Assert
		Assert.IsFalse(viewModel.IsValueNotEmpty);
	}

	[TestMethod]
	public void Value_WhenSetToWhitespace_IsValueNotEmptyReturnsFalse()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act
		viewModel.Value = "   ";

		// Assert
		Assert.IsFalse(viewModel.IsValueNotEmpty);
	}

	[TestMethod]
	public void Value_WhenSetToNull_IsValueNotEmptyReturnsFalse()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act
		viewModel.Value = null!;

		// Assert
		Assert.IsFalse(viewModel.IsValueNotEmpty);
	}

	[TestMethod]
	public void Value_WhenSetToValidString_IsValueNotEmptyReturnsTrue()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act
		viewModel.Value = "Valid Value";

		// Assert
		Assert.IsTrue(viewModel.IsValueNotEmpty);
		Assert.AreEqual("Valid Value", viewModel.Value);
	}

	[TestMethod]
	public void Title_CanBeSetAndRetrieved()
	{
		// Arrange
		var viewModel = CreateViewModel();
		const string newTitle = "New Title";

		// Act
		viewModel.Title = newTitle;

		// Assert
		Assert.AreEqual(newTitle, viewModel.Title);
	}

	[TestMethod]
	public void Label_CanBeSetAndRetrieved()
	{
		// Arrange
		var viewModel = CreateViewModel();
		const string newLabel = "New Label";

		// Act
		viewModel.Label = newLabel;

		// Assert
		Assert.AreEqual(newLabel, viewModel.Label);
	}

	#endregion Property Tests

	#region Command Tests

	[TestMethod]
	public void ConfirmCommand_WhenValueIsEmpty_CannotExecute()
	{
		// Arrange
		var viewModel = CreateViewModel();
		viewModel.Value = string.Empty;

		// Act & Assert
		Assert.IsFalse(viewModel.ConfirmCommand.CanExecute(null));
	}

	[TestMethod]
	public void ConfirmCommand_WhenValueIsWhitespace_CannotExecute()
	{
		// Arrange
		var viewModel = CreateViewModel();
		viewModel.Value = "   ";

		// Act & Assert
		Assert.IsFalse(viewModel.ConfirmCommand.CanExecute(null));
	}

	[TestMethod]
	public void ConfirmCommand_WhenValueIsValid_CanExecute()
	{
		// Arrange
		var viewModel = CreateViewModel();
		viewModel.Value = "Valid Value";

		// Act & Assert
		Assert.IsTrue(viewModel.ConfirmCommand.CanExecute(null));
	}

	[TestMethod]
	public void ConfirmCommand_WithValidValue_SetsDialogResultToTrue()
	{
		// Arrange
		var viewModel = CreateViewModel();
		viewModel.Value = "Valid Value";

		// Act
		viewModel.ConfirmCommand.Execute(null);

		// Assert
		Assert.IsTrue(viewModel.DialogResult);
	}

	[TestMethod]
	public void ConfirmCommand_WithInvalidName_ShowsErrorAndDoesNotSetDialogResult()
	{
		// Arrange
		var viewModel = CreateViewModel(invalidNames: _invalidNames);
		viewModel.Value = "InvalidName1";

		// Act
		viewModel.ConfirmCommand.Execute(null);

		// Assert
		_mockMessageService.Verify(x => x.ShowError(It.Is<string>(s => s == "InvalidNameMessage"), It.Is<string>(s => s == "Error")), Times.Once);
		Assert.IsNull(viewModel.DialogResult);
	}

	[TestMethod]
	public void ConfirmCommand_WithMultipleInvalidNames_ChecksAllAndShowsError()
	{
		// Arrange
		var viewModel = CreateViewModel(invalidNames: _invalidNames);
		viewModel.Value = "InvalidName2";

		// Act
		viewModel.ConfirmCommand.Execute(null);

		// Assert
		_mockMessageService.Verify(x => x.ShowError(It.Is<string>(s => s == "InvalidNameMessage"), It.Is<string>(s => s == "Error")), Times.Once);
		Assert.IsNull(viewModel.DialogResult);
	}

	[TestMethod]
	public void ConfirmCommand_WithCaseSensitiveInvalidName_ShowsError()
	{
		// Arrange
		var viewModel = CreateViewModel(invalidNames: _invalidNames);
		viewModel.Value = "InvalidName1"; // Exact match

		// Act
		viewModel.ConfirmCommand.Execute(null);

		// Assert
		_mockMessageService.Verify(x => x.ShowError(It.Is<string>(s => s == "InvalidNameMessage"), It.Is<string>(s => s == "Error")), Times.Once);
		Assert.IsNull(viewModel.DialogResult);
	}

	[TestMethod]
	public void ConfirmCommand_WithDifferentCaseValidName_SetsDialogResultToTrue()
	{
		// Arrange
		var viewModel = CreateViewModel(invalidNames: _invalidNames);
		viewModel.Value = "invalidname1"; // Different case

		// Act
		viewModel.ConfirmCommand.Execute(null);

		// Assert
		Assert.IsTrue(viewModel.DialogResult);
		_mockMessageService.Verify(x => x.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
	}

	[TestMethod]
	public void ConfirmCommand_WithNoInvalidNames_AlwaysSetsDialogResultToTrue()
	{
		// Arrange
		var viewModel = CreateViewModel(); // No invalid names
		viewModel.Value = "Any Value";

		// Act
		viewModel.ConfirmCommand.Execute(null);

		// Assert
		Assert.IsTrue(viewModel.DialogResult);
		_mockMessageService.Verify(x => x.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
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
	public void CancelCommand_CanAlwaysExecute()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act & Assert
		Assert.IsTrue(viewModel.CancelCommand.CanExecute(null));

		// Even with empty value
		viewModel.Value = string.Empty;
		Assert.IsTrue(viewModel.CancelCommand.CanExecute(null));
	}

	#endregion Command Tests

	#region Property Change Notification Tests

	[TestMethod]
	public void Value_PropertyChange_NotifiesIsValueNotEmpty()
	{
		// Arrange
		var viewModel = CreateViewModel();
		var propertyChangedEvents = new List<string>();

		viewModel.PropertyChanged += (sender, e) =>
		{
			if (e.PropertyName != null)
				propertyChangedEvents.Add(e.PropertyName);
		};

		// Act
		viewModel.Value = "Test Value";

		// Assert
		Assert.IsTrue(propertyChangedEvents.Contains(nameof(viewModel.Value)));
		Assert.IsTrue(propertyChangedEvents.Contains(nameof(viewModel.IsValueNotEmpty)));
	}

	[TestMethod]
	public void Value_PropertyChange_NotifiesConfirmCommandCanExecuteChanged()
	{
		// Arrange
		var viewModel = CreateViewModel();
		var canExecuteChangedRaised = false;

		viewModel.ConfirmCommand.CanExecuteChanged += (sender, e) => canExecuteChangedRaised = true;

		// Act
		viewModel.Value = "Test Value";

		// Assert
		Assert.IsTrue(canExecuteChangedRaised);
	}

	#endregion Property Change Notification Tests

	#region Edge Case Tests

	[TestMethod]
	public void InvalidNames_WithNullCollection_DoesNotThrow()
	{
		// Arrange
		var viewModel = CreateViewModel(invalidNames: null);
		viewModel.Value = "Any Value";

		// Act
		Exception? exception = null;

		try
		{
			viewModel.ConfirmCommand.Execute(null);
		}
		catch (Exception ex)
		{
			exception = ex;
		}

		// Assert
		Assert.IsNull(exception);
		Assert.IsTrue(viewModel.DialogResult);
	}

	[TestMethod]
	public void InvalidNames_WithEmptyCollection_DoesNotThrow()
	{
		// Arrange
		var viewModel = CreateViewModel(invalidNames: []);
		viewModel.Value = "Any Value";

		// Act
		Exception? exception = null;

		try
		{
			viewModel.ConfirmCommand.Execute(null);
		}
		catch (Exception ex)
		{
			exception = ex;
		}

		// Assert
		Assert.IsNull(exception);
		Assert.IsTrue(viewModel.DialogResult);
	}

	[TestMethod]
	public void Value_SetToSameValue_DoesNotCauseUnnecessaryNotifications()
	{
		// Arrange
		var viewModel = CreateViewModel();
		viewModel.Value = "Initial Value";

		int propertyChangedCount = 0;

		viewModel.PropertyChanged += (sender, e) =>
		{
			if (e.PropertyName == nameof(viewModel.Value))
				propertyChangedCount++;
		};

		// Act
		viewModel.Value = "Initial Value"; // Same value

		// Assert - Should not raise PropertyChanged for same value
		Assert.AreEqual(0, propertyChangedCount);
	}

	[TestMethod]
	public void IsValueNotEmpty_WithVariousWhitespaceInputs_ReturnsFalse()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Test various whitespace scenarios
		var whitespaceValues = new[] { "", " ", "\t", "\n", "\r\n", "   ", "\t\n  " };

		foreach (var whitespace in whitespaceValues)
		{
			// Act
			viewModel.Value = whitespace;

			// Assert
			Assert.IsFalse(viewModel.IsValueNotEmpty, $"Failed for whitespace: '{whitespace}'");
		}
	}

	[TestMethod]
	public void ConfirmCommand_WithTrimmedValidValue_SetsDialogResultToTrue()
	{
		// Arrange
		var viewModel = CreateViewModel(invalidNames: _invalidNames);
		viewModel.Value = "  Valid Value  "; // Value with leading/trailing spaces

		// Act
		viewModel.ConfirmCommand.Execute(null);

		// Assert
		Assert.IsTrue(viewModel.DialogResult);
	}

	#endregion Edge Case Tests

	#region Service Integration Tests

	[TestMethod]
	public void Constructor_WithNullServices_ResolvesFromServiceLocator()
	{
		// Act & Assert - Should not throw
		var viewModel = new InputBoxWindowViewModel();

		Assert.IsNotNull(viewModel);
	}

	[TestMethod]
	public void LocalizationService_UsedForDefaultLabel()
	{
		// Act
		var viewModel = CreateViewModel();

		// Assert
		_mockLocalizationService.Verify(x => x["EnterValue"], Times.Once);
	}

	[TestMethod]
	public void LocalizationService_UsedForErrorMessage()
	{
		// Arrange
		var viewModel = CreateViewModel(invalidNames: _invalidNames);
		viewModel.Value = "InvalidName1";

		// Act
		viewModel.ConfirmCommand.Execute(null);

		// Assert
		_mockLocalizationService.Verify(x => x["InvalidNameMessage"], Times.Once);
	}

	#endregion Service Integration Tests
}
