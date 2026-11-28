#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.SelectRoomByTags;

public partial class SelectRoomByTagsWindowViewModel : ObservableObject, IModalDialogViewModel
{

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private string _tagSearchText = string.Empty;
    [ObservableProperty] private bool _findAllTags;

    public IReadOnlyList<string> AutocompleteTags { get; }

    public SelectRoomByTagsWindowViewModel(
        Editor? editor = null,
        ILocalizationService? localizationService = null)
    {
        editor ??= Editor.Instance;
        _ = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        AutocompleteTags = editor.Level.Rooms
            .Where(r => r is not null && r.ExistsInLevel)
            .SelectMany(r => r.Properties.Tags)
            .Distinct()
            .OrderBy(t => t)
            .ToList();
    }

    [RelayCommand]
    private void Confirm()
    {
        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
    }
}
