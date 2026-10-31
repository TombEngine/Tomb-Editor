#nullable enable

using System;
using System.ComponentModel;
using System.Windows;
using TombLib.LevelData;
using TombLib.NG;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.Trigger;

public partial class TriggerWindow : Window
{
    private readonly Level _level;
    private readonly TriggerInstance _trigger;

    private bool _dialogIsUpdating;
    private TriggerWindowViewModel? _vm;

    public TriggerWindow(TriggerInstance trigger, Level level,
        Action<ObjectInstance> selectObject, Action<Room> selectRoom)
    {
        InitializeComponent();
        this.HookModalAutoClose();

        _level = level;
        _trigger = trigger;

        foreach (var ctl in new[] { paramTriggerType, paramPlugin, paramTargetType, paramTarget, paramTimer, paramExtra })
        {
            ctl.Level = level;
            ctl.ViewObject += selectObject;
            ctl.ViewRoom += selectRoom;
            ctl.ParameterChanged += (_, _) => UpdateDialog();
        }

        Loaded += OnLoaded;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormTrigger");
        Initialize(_trigger);

        // The previous form locked the height; only the width can be changed.
        Dispatcher.BeginInvoke(new Action(() => MinHeight = MaxHeight = ActualHeight), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_vm is not null)
        {
            _vm.PropertyChanged -= OnVmPropertyChanged;
            _vm.TriggerImported -= Initialize;
        }

        _vm = e.NewValue as TriggerWindowViewModel;
        if (_vm is null)
            return;

        _vm.PropertyChanged += OnVmPropertyChanged;
        _vm.TriggerImported += Initialize;
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_dialogIsUpdating)
            return;

        if (e.PropertyName == nameof(TriggerWindowViewModel.RawMode))
        {
            paramTriggerType.RawMode = _vm!.RawMode;
            paramPlugin.RawMode      = _vm.RawMode;
            paramTargetType.RawMode  = _vm.RawMode;
            paramTarget.RawMode      = _vm.RawMode;
            paramTimer.RawMode       = _vm.RawMode;
            paramExtra.RawMode       = _vm.RawMode;
        }
    }

    private void Initialize(TriggerInstance trigger)
    {
        _vm?.AllocateNewScriptIds();
        UpdateDialog();

        // Mirror the legacy FormTrigger ctor: the assignments below must NOT be suppressed —
        // each one fires ParameterChanged → UpdateDialog, which re-derives the dependent
        // parameter ranges from the progressively assigned values (the population order below
        // relies on that cascade). UpdateDialog guards itself against recursion.
        _vm?.LoadTrigger(trigger);

        paramTriggerType.Parameter = new TriggerParameterUshort((ushort)trigger.TriggerType);
        paramTargetType.Parameter = new TriggerParameterUshort((ushort)trigger.TargetType);
        paramPlugin.Parameter = trigger.Plugin;

        if (trigger.TriggerType == TriggerType.ConditionNg)
        {
            paramTimer.Parameter = trigger.Timer;
            paramTarget.Parameter = trigger.Target;
        }
        else
        {
            paramTarget.Parameter = trigger.Target;
            paramTimer.Parameter = trigger.Timer;
        }

        paramExtra.Parameter = trigger.Extra;

        _vm?.SetCurrentParameters(CurrentTriggerType, CurrentTargetType,
            paramPlugin.Parameter, paramTarget.Parameter, paramTimer.Parameter, paramExtra.Parameter);
    }

    private void UpdateDialog()
    {
        if (_dialogIsUpdating)
            return;

        _dialogIsUpdating = true;
        bool isButtons = false;
        try
        {
            paramTriggerType.ParameterRange = NgParameterInfo.GetTriggerTypeRange(_level.Settings).ToParameterRange();
            paramTargetType.ParameterRange = NgParameterInfo.GetTargetTypeRange(_level.Settings, CurrentTriggerType).ToParameterRange();

            bool isEvent = CurrentTargetType is TriggerTargetType.VolumeEvent or TriggerTargetType.GlobalEvent;
            bool isConditionNg = isEvent || CurrentTriggerType == TriggerType.ConditionNg || CurrentTargetType == TriggerTargetType.ActionNg;

            if (_level.IsNG)
                paramPlugin.ParameterRange = NgParameterInfo.GetPluginRange(_level.Settings);

            if (isConditionNg)
            {
                paramTimer.ParameterRange = NgParameterInfo.GetTimerRange(_level.Settings, CurrentTriggerType, CurrentTargetType, paramTarget.Parameter, paramPlugin.Parameter);
                paramTarget.ParameterRange = NgParameterInfo.GetTargetRange(_level.Settings, CurrentTriggerType, CurrentTargetType, paramTimer.Parameter, paramPlugin.Parameter);
            }
            else
            {
                paramTarget.ParameterRange = NgParameterInfo.GetTargetRange(_level.Settings, CurrentTriggerType, CurrentTargetType, paramTimer.Parameter, paramPlugin.Parameter);
                paramTimer.ParameterRange = NgParameterInfo.GetTimerRange(_level.Settings, CurrentTriggerType, CurrentTargetType, paramTarget.Parameter, paramPlugin.Parameter);
            }

            paramExtra.ParameterRange = NgParameterInfo.GetExtraRange(
                _level.Settings, CurrentTriggerType, CurrentTargetType, paramTarget.Parameter, paramTimer.Parameter, paramPlugin.Parameter,
                out isButtons);
        }
        finally { _dialogIsUpdating = false; }

        _vm?.SetCurrentParametersAfterRangeUpdate(CurrentTriggerType, CurrentTargetType,
            paramPlugin.Parameter, paramTarget.Parameter, paramTimer.Parameter, paramExtra.Parameter,
            isButtons);
    }

    private TriggerType CurrentTriggerType => paramTriggerType.Parameter is TriggerParameterUshort u
        ? (TriggerType)u.Key
        : TriggerType.Trigger;

    private TriggerTargetType CurrentTargetType => paramTargetType.Parameter is TriggerParameterUshort u
        ? (TriggerTargetType)u.Key
        : TriggerTargetType.Object;
}
