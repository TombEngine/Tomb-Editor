#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using TombLib.Wad;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.QuickItemGroup;

public partial class QuickItemGroupWindowViewModel : ObservableObject, IModalDialogViewModel
{
    public sealed record QuickItem(IWadObjectId Id, string DisplayText);

    [ObservableProperty] private bool? _dialogResult;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private QuickItem? _selectedItem;

    public IReadOnlyList<QuickItem> Items { get; }

    public IWadObjectId? SelectedValue => SelectedItem?.Id;

    public QuickItemGroupWindowViewModel(
        Editor? editor = null,
        ILocalizationService? localizationService = null)
    {
        editor ??= Editor.Instance;
        _ = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        var settings = editor.Level.Settings;
        var version = settings.GameVersion;

        IEnumerable<IWadObjectId> ids = settings.WadGetAllMoveables().Keys
            .Cast<IWadObjectId>()
            .Concat(settings.WadGetAllStatics().Keys.Cast<IWadObjectId>());

        Items = ids
            .Select(id => new QuickItem(id, id.ToString(version)))
            .ToList();

        SelectedItem = Items.FirstOrDefault();
    }

    private bool CanConfirm() => SelectedItem is not null;

    [RelayCommand(CanExecute = nameof(CanConfirm))]
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
