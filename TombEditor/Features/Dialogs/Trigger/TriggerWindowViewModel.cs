#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using System;
using System.Windows;
using TombLib;
using TombLib.LevelData;
using TombLib.NG;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;
using TombLib.WPF.ViewModels;

namespace TombEditor.Features.Dialogs.Trigger;

public partial class TriggerWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly TriggerInstance _trigger;
    private readonly Level _level;

    private readonly IDialogService _dialogService;
    private readonly IMessageService _messageService;
    private readonly ILocalizationService _localizationService;

    // Snapshot of the six parameter controls, pushed in by the view whenever they change.
    private TriggerType _currentTriggerType = TriggerType.Trigger;
    private TriggerTargetType _currentTargetType = TriggerTargetType.Object;
    private ITriggerParameter? _plugin;
    private ITriggerParameter? _target;
    private ITriggerParameter? _timer;
    private ITriggerParameter? _extra;

    private string? _scriptWithComments;

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private string _title = string.Empty;

    [ObservableProperty] private bool _isNG;

    [ObservableProperty] private bool _bit1;
    [ObservableProperty] private bool _bit2;
    [ObservableProperty] private bool _bit3;
    [ObservableProperty] private bool _bit4;
    [ObservableProperty] private bool _bit5;

    [ObservableProperty] private bool _oneShot;
    [ObservableProperty] private bool _oneShotEnabled = true;

    [ObservableProperty] private bool _rawMode;

    [ObservableProperty] private string _scriptText = string.Empty;
    [ObservableProperty] private bool _scriptEnabled;
    [ObservableProperty] private bool _copyAsAnimcommandEnabled;

    /// <summary>
    /// Raised when a trigger was successfully imported from a script command
    /// and the view should redisplay it in the parameter controls.
    /// </summary>
    public event Action<TriggerInstance>? TriggerImported;

    public TriggerWindowViewModel(
        TriggerInstance trigger,
        Level level,
        IDialogService? dialogService = null,
        IMessageService? messageService = null,
        ILocalizationService? localizationService = null)
    {
        // Services
        _dialogService = ServiceLocator.ResolveService(dialogService);
        _messageService = ServiceLocator.ResolveService(messageService);
        _localizationService = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        // Properties and Fields
        _trigger = trigger;
        _level = level;

        IsNG = level.IsNG;
        Title = level.IsTombEngine ? _localizationService["ClassicTitle"] : _localizationService["Title"];
    }

    #region State pushed in by the view

    /// <summary>
    /// Called by the view after the parameter controls were (re)initialized,
    /// so the script export can be recomputed from the current values.
    /// </summary>
    public void SetCurrentParameters(TriggerType triggerType, TriggerTargetType targetType,
        ITriggerParameter? plugin, ITriggerParameter? target, ITriggerParameter? timer, ITriggerParameter? extra)
    {
        StoreParameters(triggerType, targetType, plugin, target, timer, extra);
        UpdateScriptExport();
    }

    /// <summary>
    /// Called by the view after it recomputed the parameter ranges. Besides refreshing
    /// the script export, this derives the one-shot availability and, when the extra
    /// parameter represents button flags, unpacks it into the code bits.
    /// </summary>
    public void SetCurrentParametersAfterRangeUpdate(TriggerType triggerType, TriggerTargetType targetType,
        ITriggerParameter? plugin, ITriggerParameter? target, ITriggerParameter? timer, ITriggerParameter? extra,
        bool extraIsButtons)
    {
        StoreParameters(triggerType, targetType, plugin, target, timer, extra);

        bool isEvent = targetType is TriggerTargetType.VolumeEvent or TriggerTargetType.GlobalEvent;
        OneShotEnabled = !isEvent;

        if (extraIsButtons)
        {
            ushort selectedExtraKey = extra is TriggerParameterUshort extraParam
                ? extraParam.Key
                : (ushort)0;

            Bit1 = (selectedExtraKey & 1) != 0;
            Bit2 = (selectedExtraKey & 2) != 0;
            Bit3 = (selectedExtraKey & 4) != 0;
            Bit4 = (selectedExtraKey & 8) != 0;
            Bit5 = (selectedExtraKey & 16) != 0;
            OneShot = (selectedExtraKey & 32) != 0;
        }

        UpdateScriptExport();
    }

    /// <summary>
    /// Loads the code bits and the one-shot flag from <paramref name="trigger"/>.
    /// </summary>
    public void LoadTrigger(TriggerInstance trigger)
    {
        Bit1 = (trigger.CodeBits & (1 << 0)) != 0;
        Bit2 = (trigger.CodeBits & (1 << 1)) != 0;
        Bit3 = (trigger.CodeBits & (1 << 2)) != 0;
        Bit4 = (trigger.CodeBits & (1 << 3)) != 0;
        Bit5 = (trigger.CodeBits & (1 << 4)) != 0;
        OneShot = trigger.OneShot;
    }

    /// <summary>
    /// Allocates new script IDs for the current parameters if they are missing (TRNG only).
    /// </summary>
    public void AllocateNewScriptIds()
    {
        if (_level.Settings.GameVersion != TRVersion.Game.TRNG)
            return;

        if (_target is IHasScriptID target && !target.ScriptId.HasValue)
            target.AllocateNewScriptId();

        if (_timer is IHasScriptID timer && !timer.ScriptId.HasValue)
            timer.AllocateNewScriptId();

        if (_extra is IHasScriptID extra && !extra.ScriptId.HasValue)
            extra.AllocateNewScriptId();
    }

    private void StoreParameters(TriggerType triggerType, TriggerTargetType targetType,
        ITriggerParameter? plugin, ITriggerParameter? target, ITriggerParameter? timer, ITriggerParameter? extra)
    {
        _currentTriggerType = triggerType;
        _currentTargetType = targetType;
        _plugin = plugin;
        _target = target;
        _timer = timer;
        _extra = extra;
    }

    #endregion State pushed in by the view

    #region Trigger assembly

    private byte CodeBits
    {
        get
        {
            byte b = 0;

            if (Bit1)
                b |= 1 << 0;

            if (Bit2)
                b |= 1 << 1;

            if (Bit3)
                b |= 1 << 2;

            if (Bit4)
                b |= 1 << 3;

            if (Bit5)
                b |= 1 << 4;

            return b;
        }
    }

    private TriggerInstance MakeTestTrigger() => new(new RectangleInt2())
    {
        TriggerType = _currentTriggerType,
        TargetType = _currentTargetType,
        Plugin = _plugin,
        Target = _target,
        Timer = _timer,
        Extra = _extra,
        CodeBits = CodeBits,
        OneShot = OneShot
    };

    private void UpdateScriptExport()
    {
        if (!_level.IsNG)
        {
            ScriptEnabled = false;
            CopyAsAnimcommandEnabled = false;
            return;
        }

        try
        {
            ScriptText = NgParameterInfo.ExportToScriptTrigger(_level, MakeTestTrigger(), null);
            _scriptWithComments = NgParameterInfo.ExportToScriptTrigger(_level, MakeTestTrigger(), null, true);
            ScriptEnabled = true;
            CopyAsAnimcommandEnabled = _currentTargetType is not TriggerTargetType.ParameterNg and not TriggerTargetType.ActionNg;
        }
        catch (NgParameterInfo.ExceptionScriptNotSupported)
        {
            ScriptText = _localizationService["ScriptNotSupported"];
            _scriptWithComments = null;
            ScriptEnabled = false;
            CopyAsAnimcommandEnabled = false;
        }
        catch (NgParameterInfo.ExceptionScriptIdMissing)
        {
            ScriptText = _localizationService["ScriptClickToGenerate"];
            _scriptWithComments = null;
            ScriptEnabled = false;
            CopyAsAnimcommandEnabled = false;
        }
        catch (Exception exc)
        {
            ScriptText = _localizationService["ScriptCheckAllFields"];
            _scriptWithComments = null;
            ScriptEnabled = false;
            CopyAsAnimcommandEnabled = false;
            _logger.Debug(exc, "\"ExportToScriptTrigger\" failed.");
        }
    }

    #endregion Trigger assembly

    #region Relay Commands

    [RelayCommand]
    private void Confirm()
    {
        if (!NgParameterInfo.TriggerIsValid(_level.Settings, MakeTestTrigger()))
        {
            bool proceed = _messageService.ShowConfirmation(
                _localizationService["TriggerInvalidMessage"],
                _localizationService["TriggerInvalidTitle"],
                defaultValue: true,
                isRisky: true);

            if (!proceed)
                return;
        }

        _trigger.TriggerType = _currentTriggerType;
        _trigger.TargetType = _currentTargetType;
        _trigger.Plugin = _plugin;
        _trigger.Target = _target;
        _trigger.Timer = _timer;
        _trigger.Extra = _extra;
        _trigger.CodeBits = CodeBits;
        _trigger.OneShot = OneShot;

        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel() => DialogResult = false;

    [RelayCommand]
    private void CopyToClipboard()
    {
        if (ScriptText.Length > 0)
            Clipboard.SetText(ScriptText);
    }

    [RelayCommand]
    private void CopyWithComments()
    {
        if (!string.IsNullOrEmpty(_scriptWithComments))
            Clipboard.SetText(_scriptWithComments);
    }

    [RelayCommand]
    private void CopyAsAnimcommand()
    {
        var frameVm = new InputBoxWindowViewModel(
            title: _localizationService["AnimcommandFrameTitle"],
            label: _localizationService["AnimcommandFrameLabel"],
            initialValue: "-1");

        bool confirmed = _dialogService.ShowDialog(this, frameVm) == true;

        if (!confirmed || !int.TryParse(frameVm.Value, out int frame) || frame < -1 || frame > 254)
        {
            if (frameVm.DialogResult == true)
                _messageService.ShowError(_localizationService["AnimcommandFrameInvalidMessage"]);
            return;
        }

        string result = NgParameterInfo.ExportToScriptTrigger(_level, MakeTestTrigger(), frame);

        if (string.IsNullOrEmpty(result))
            return;

        var outVm = new InputBoxWindowViewModel(
            title: _localizationService["AnimcommandExportTitle"],
            label: _localizationService["AnimcommandExportLabel"],
            initialValue: result);

        _dialogService.ShowDialog(this, outVm);
    }

    [RelayCommand]
    private void SearchTrigger()
    {
        var inputVm = new InputBoxWindowViewModel(
            title: _localizationService["ImportTriggerTitle"],
            label: _localizationService["ImportTriggerLabel"]);

        if (_dialogService.ShowDialog(this, inputVm) != true)
            return;

        var imported = NgParameterInfo.ImportFromScriptTrigger(_level, inputVm.Value);

        if (imported is null)
        {
            _messageService.ShowError(_localizationService["ImportTriggerInvalidMessage"]);
            return;
        }

        TriggerImported?.Invoke(imported);
    }

    #endregion Relay Commands
}
