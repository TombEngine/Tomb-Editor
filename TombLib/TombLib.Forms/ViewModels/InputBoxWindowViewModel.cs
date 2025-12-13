#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System.Collections.Generic;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombLib.Forms.ViewModels;

public partial class InputBoxWindowViewModel : ObservableObject, IModalDialogViewModel
{
	[ObservableProperty] private string _title;
	[ObservableProperty] private string _label;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsValueNotEmpty))]
	[NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
	private string _value = string.Empty;

	[ObservableProperty] private bool? _dialogResult;

	public bool IsValueNotEmpty => !string.IsNullOrWhiteSpace(Value);

	private readonly IEnumerable<string> _invalidNames;

	private readonly IMessageService _messageService;
	private readonly ILocalizationService _localizationService;

	public InputBoxWindowViewModel(
		string? title = null,
		string? label = null,
		string? placeholder = null,
		IEnumerable<string>? invalidNames = null,
		IMessageService? messageService = null,
		ILocalizationService? localizationService = null)
	{
		// Services
		_messageService = ServiceLocator.ResolveService(messageService);
		_localizationService = ServiceLocator.ResolveService(localizationService)
			.WithKeysFor(this);

		// Properties
		Title = title ?? string.Empty;
		Label = label ?? _localizationService["EnterValue"];
		Value = placeholder ?? string.Empty;

		_invalidNames = invalidNames ?? [];
	}

	[RelayCommand(CanExecute = nameof(IsValueNotEmpty))]
	private void Confirm()
	{
		foreach (string invalidName in _invalidNames)
		{
			if (Value.Equals(invalidName))
			{
				_messageService.ShowError(_localizationService["InvalidNameMessage"]);
				return;
			}
		}

		DialogResult = true;
	}

	[RelayCommand]
	private void Cancel()
		=> DialogResult = false;
}
