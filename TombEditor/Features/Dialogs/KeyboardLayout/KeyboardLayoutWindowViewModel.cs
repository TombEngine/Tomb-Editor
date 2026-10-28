#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Keys = TombLib.Input.EditorKey;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Input;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.KeyboardLayout;

public partial class KeyboardLayoutWindowViewModel : ObservableObject, IModalDialogViewModel
{
    public partial class HotkeyRow : ObservableObject
    {
        public CommandObj Command { get; init; } = null!;
        public string FriendlyName => Command.FriendlyName;
        public string TypeName => Command.Type.ToString();
        [ObservableProperty] private string _hotkeysText = string.Empty;
        [ObservableProperty] private bool _isConflicting;
    }

    private readonly Editor _editor;
    private readonly IMessageService _messageService;
    private readonly ILocalizationService _localizationService;

    private HotkeySets _currConfig;
    private HotkeyRow? _listeningTarget;
    private bool _listeningClearAfterwards;
    private Keys _listeningKeys = Keys.None;

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private HotkeyRow? _selectedRow;
    [ObservableProperty] private bool _isListening;
    [ObservableProperty] private string _listeningText = string.Empty;
    [ObservableProperty] private string _conflictsText = string.Empty;
    [ObservableProperty] private bool _hasConflicts;

    public ObservableCollection<HotkeyRow> Rows { get; } = new();

    public KeyboardLayoutWindowViewModel(
        Editor editor,
        IMessageService? messageService = null,
        ILocalizationService? localizationService = null)
    {
        _editor = editor;
        _messageService = ServiceLocator.ResolveService(messageService);
        _localizationService = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        _currConfig = _editor.Configuration.UI_Hotkeys.Clone();

        foreach (var cmd in CommandHandler.Commands)
            Rows.Add(new HotkeyRow { Command = cmd });

        RefreshAllRows();
    }

    private void RefreshAllRows()
    {
        foreach (var row in Rows)
            row.HotkeysText = string.Join(", ", _currConfig[row.Command].Select(h => h.ToString()));

        // Conflict detection mirrors the previous FormKeyboardLayout: any two commands
        // sharing at least one hotkey are flagged.
        var conflictPairs = new List<(string, string)>();
        var entries = _currConfig.ToList();
        for (int i = 0; i < entries.Count; i++)
            for (int j = i + 1; j < entries.Count; j++)
                if (entries[i].Value.Intersect(entries[j].Value).Any())
                    conflictPairs.Add((entries[i].Key, entries[j].Key));

        var conflictingNames = new HashSet<string>(conflictPairs.SelectMany(p => new[] { p.Item1, p.Item2 }), StringComparer.OrdinalIgnoreCase);
        foreach (var row in Rows)
            row.IsConflicting = conflictingNames.Contains(row.Command.Name);

        HasConflicts = conflictPairs.Count > 0;
        if (HasConflicts)
        {
            var first = conflictPairs[0];
            ConflictsText = _localizationService.Format("PossibleConflicts", first.Item1, first.Item2);
        }
    }

    [RelayCommand]
    private void StartListening(HotkeyRow? row)
    {
        if (row is null || IsListening)
            return;

        _listeningTarget = row;
        _listeningClearAfterwards = false;
        _listeningKeys = Keys.None;
        ListeningText = _localizationService["~TombEditor.KeyboardLayout.CancelPush"];
        IsListening = true;
    }

    // Legacy double-click-on-row behaviour: replace all hotkeys of the command with the new one.
    [RelayCommand]
    private void StartListeningReplace(HotkeyRow? row)
    {
        if (row is null || IsListening)
            return;

        _listeningTarget = row;
        _listeningClearAfterwards = true;
        _listeningKeys = Keys.None;
        ListeningText = _localizationService["~TombEditor.KeyboardLayout.CancelPush"];
        IsListening = true;
    }

    [RelayCommand]
    private void StopListening()
    {
        IsListening = false;
        _listeningTarget = null;
        _listeningKeys = Keys.None;
    }

    /// <summary>Called by the View on key-down while listening; aggregates modifiers and the key.</summary>
    public void OnListenKeyDown(Keys keyData)
    {
        if (!IsListening)
            return;

        _listeningKeys |= keyData & ~Keys.KeyCode;
        switch (keyData & Keys.KeyCode)
        {
            case Keys.ControlKey: _listeningKeys |= Keys.Control; break;
            case Keys.ShiftKey: _listeningKeys |= Keys.Shift; break;
            case Keys.Menu: _listeningKeys |= Keys.Alt; break;
            default: _listeningKeys = (_listeningKeys & ~Keys.KeyCode) | (keyData & Keys.KeyCode); break;
        }

        ListeningText = ((Hotkey)_listeningKeys).ToString();
    }

    /// <summary>Called by the View on key-up to commit if a non-modifier was pressed.</summary>
    public void OnListenKeyUp()
    {
        if (!IsListening || _listeningTarget is null)
            return;
        if ((_listeningKeys & Keys.KeyCode) == Keys.None)
            return;

        if (KeyboardInput.DirectionalCameraKeys.Contains(_listeningKeys))
        {
            _messageService.ShowError(_localizationService["ReservedCameraKey"]);
            StopListening();
            return;
        }

        if (_listeningClearAfterwards)
            _currConfig[_listeningTarget.Command.Name]?.Clear();
        _currConfig[_listeningTarget.Command.Name].Add(_listeningKeys);

        RefreshAllRows();
        StopListening();
    }

    [RelayCommand]
    private void DeleteHotkeys(HotkeyRow? row)
    {
        if (row is null)
            return;

        _currConfig[row.Command].Clear();
        RefreshAllRows();
    }

    [RelayCommand]
    private void Search()
    {
        if (string.IsNullOrEmpty(SearchText))
            return;

        int startIndex = SelectedRow is null ? 0 : Rows.IndexOf(SelectedRow) + 1;
        if (startIndex >= Rows.Count) startIndex = 0;

        for (int pass = 0; pass < 2; pass++)
        {
            int from = pass == 0 ? startIndex : 0;
            int to = pass == 0 ? Rows.Count : startIndex;
            for (int i = from; i < to; i++)
            {
                if (Rows[i].FriendlyName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0
                    || Rows[i].Command.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    SelectedRow = Rows[i];
                    return;
                }
            }
        }
    }

    [RelayCommand]
    private void RestoreDefaults()
    {
        if (!_messageService.ShowConfirmation(_localizationService["RestoreDefaultsConfirm"]))
            return;

        _currConfig = new HotkeySets();
        RefreshAllRows();
    }

    [RelayCommand]
    private void Ok()
    {
        _editor.Configuration.UI_Hotkeys = _currConfig;
        _editor.ConfigurationChange(true);
        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel() => DialogResult = false;
}
