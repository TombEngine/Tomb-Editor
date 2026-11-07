#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using TombLib;
using TombLib.Services;
using TombLib.Utils;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.Operation;

/// <summary>Lifecycle of the running operation; the View mirrors it onto the taskbar button.</summary>
public enum OperationState
{
    Running,
    Succeeded,
    Cancelled,
    Failed
}

public partial class OperationDialogWindowViewModel : ObservableObject, IModalDialogViewModel, IProgressReporter
{
    public sealed class LogEntry
    {
        public string Text { get; init; } = string.Empty;
        // Full-width row background; the text itself is always white (see the view), so warnings/errors
        // read as solid coloured rows rather than the old partial-width "striped" highlights.
        public Brush Background { get; init; } = Brushes.Transparent;
    }

    private static Brush Frozen(byte r, byte g, byte b)
    {
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }

    // Dark, white-text-readable equivalents of the legacy previous log colours.
    private static readonly Brush DefaultLogBrush = Frozen(0x2D, 0x2D, 0x30);
    private static readonly Brush WarningRowBrush = Frozen(0x8F, 0x7A, 0x1E);
    private static readonly Brush ErrorRowBrush = Frozen(0x8B, 0x3A, 0x3A);
    private static readonly Brush SuccessLogBrush = Frozen(0x35, 0x6B, 0x35);
    private static readonly Brush FailureLogBrush = Frozen(0x6E, 0x3A, 0x3A);

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly Action<IProgressReporter, CancellationToken> _operation;
    private readonly bool _autoCloseWhenDone;
    private readonly CancellationTokenSource _cts = new();
    private readonly Dispatcher _dispatcher;
    private readonly ILocalizationService _localizationService;

    private Task? _task;
    private IntPtr _hwnd = IntPtr.Zero;

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private bool _isProgressVisible = true;
    [ObservableProperty] private int _progress;
    [ObservableProperty] private OperationState _state = OperationState.Running;
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(OkCommand))] private bool _isOkEnabled;
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(CancelCommand))] private bool _isCancelEnabled = true;
    [ObservableProperty] private Brush _logBackground = DefaultLogBrush;

    public ObservableCollection<LogEntry> LogEntries { get; } = new();

    public OperationDialogWindowViewModel(
        string operationName,
        bool autoCloseWhenDone,
        bool noProgressBar,
        Action<IProgressReporter, CancellationToken> operation,
        ILocalizationService? localizationService = null)
    {
        Title = operationName;
        _autoCloseWhenDone = autoCloseWhenDone;
        IsProgressVisible = !noProgressBar;
        _operation = operation;
        _localizationService = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);
        _dispatcher = Dispatcher.CurrentDispatcher;
    }

    /// <summary>Provided by the View on Loaded; owns sub-dialogs raised during the operation.</summary>
    public void SetWindowHandle(IntPtr hwnd) => _hwnd = hwnd;

    public async void Start()
    {
        try
        {
            IsCancelEnabled = true;
            IsOkEnabled = false;

            _task = Task.Run(() =>
            {
                _operation(this, _cts.Token);
                _cts.Token.ThrowIfCancellationRequested();
            });

            await _task;
            OnSuccess();
        }
        catch (Exception ex)
        {
            OnFailure(ex);
            _logger.Error(ex, "Operation failed: " + Title);

            string message = _localizationService.Format("ErrorMessage", ex.Message);
            if (ex.InnerException is not null)
                message += " : " + ex.InnerException.Message;

            AppendLine(message, ErrorRowBrush);
#if DEBUG
            throw;
#endif
        }
    }

    private void OnFailure(Exception ex)
    {
        // State first: the View stops mirroring Progress onto the taskbar once the operation
        // has ended, so the red error bar keeps its last value (like the legacy dialog).
        State = ex is not OperationCanceledException ? OperationState.Failed : OperationState.Cancelled;

        Progress = 0;
        IsCancelEnabled = true;
        IsOkEnabled = false;
        LogBackground = FailureLogBrush;
    }

    private void OnSuccess()
    {
        State = OperationState.Succeeded;

        Progress = 100;
        IsOkEnabled = true;
        IsCancelEnabled = false;
        LogBackground = SuccessLogBrush;

        if (_autoCloseWhenDone)
            DialogResult = true;
    }

    private void AddMessage(float? progress, string message, bool isWarning)
    {
        if (_cts.IsCancellationRequested)
            return;

        _dispatcher.BeginInvoke(() =>
        {
            if (progress.HasValue)
                Progress = (int)Math.Round(MathC.Clamp(progress.Value, 0, 100), 0);

            if (!string.IsNullOrEmpty(message))
                AppendLine(message, isWarning ? WarningRowBrush : Brushes.Transparent);
        });
    }

    private void AppendLine(string message, Brush background)
        => LogEntries.Add(new LogEntry { Text = message, Background = background });

    void IProgressReporter.ReportWarn(string message)
    {
        _logger.Warn(message);
        AddMessage(null, message, true);
    }

    void IProgressReporter.ReportInfo(string message)
    {
        _logger.Info(message);
        AddMessage(null, message, false);
    }

    void IProgressReporter.ReportProgress(float progress, string message)
    {
        _logger.Info(progress + " - " + message);
        AddMessage(progress, message, false);
    }

    void IDialogHandler.RaiseDialog(IDialogDescription description)
    {
        // GraphicalDialogHandler needs an IWindowOwner owner. Wrap the WPF HWND.
        var owner = _hwnd != IntPtr.Zero ? (IWindowOwner)new HwndOwner(_hwnd) : null!;
        GraphicalDialogHandler.HandleDialog(description, owner);
    }

    [RelayCommand(CanExecute = nameof(IsOkEnabled))]
    private void Ok() => DialogResult = true;

    [RelayCommand(CanExecute = nameof(IsCancelEnabled))]
    private void Cancel()
    {
        if (_task is not null && _task.Status >= TaskStatus.RanToCompletion)
        {
            // Reset first so assigning false raises a change even if a veto left it at false before.
            DialogResult = null;
            DialogResult = false;
            return;
        }
        EndThread();
    }

    private void EndThread()
    {
        _cts.Cancel();
        AppendLine(_localizationService["StoppingProcess"], ErrorRowBrush);
    }

    /// <summary>Called by the View when the user attempts to close while running.</summary>
    public bool TryRequestClose()
    {
        if (_task is null)
            return true;

        if (_task.Status <= TaskStatus.Running)
        {
            if (!_cts.IsCancellationRequested)
                EndThread();
            return false;
        }

        return true;
    }

    private sealed class HwndOwner : IWindowOwner
    {
        public HwndOwner(IntPtr h) { Handle = h; }
        public IntPtr Handle { get; }
    }
}
