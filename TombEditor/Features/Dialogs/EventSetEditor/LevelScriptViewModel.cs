#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;
using TombLib.LevelData;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;
using TombLib.WPF;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    /// <summary>
    /// Level-script mode of an event: pick one of the level script functions (with a filter and a
    /// notice/browse link when the script is missing or empty), the function argument and the call count.
    /// </summary>
    public partial class LevelScriptViewModel : ObservableObject
    {
        private readonly Editor _editor;
        private readonly ILocalizationService _localizationService;
        private Event? _event;
        private bool _lockEvent;

        public ObservableCollection<string> Functions { get; } = [];

        /// <summary>Function list narrowed by <see cref="SearchText"/>.</summary>
        public ICollectionView FilteredFunctions { get; }

        [ObservableProperty] private string _searchText = string.Empty;
        [ObservableProperty] private string _notifyText = string.Empty;
        [ObservableProperty] private bool _canBrowse;
        [ObservableProperty] private string? _selectedFunction;
        [ObservableProperty] private string _missingFunctionText = string.Empty;

        public bool HasFunctions => Functions.Count > 0;
        public bool HasNotify => NotifyText.Length > 0;
        public bool HasMissingFunction => MissingFunctionText.Length > 0;
        public bool HasEvent => _event is not null;

        public LevelScriptViewModel(Editor editor, ILocalizationService? localizationService = null)
        {
            _editor = editor;
            _localizationService = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

            FilteredFunctions = CollectionViewSource.GetDefaultView(Functions);
            FilteredFunctions.Filter = item => item is string name &&
                (SearchText.Length == 0 || name.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase));

            Reload();
        }

        /// <summary>Gets or sets the function argument (Lua syntax) of the edited event.</summary>
        public string Argument
        {
            get => _event?.Argument ?? string.Empty;
            set
            {
                if (_event is null || _event.Argument == value)
                    return;

                _event.Argument = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Gets or sets how many times the event may fire (0 = unlimited).</summary>
        public int CallCounter
        {
            get => _event?.CallCounter ?? 0;
            set
            {
                if (_event is null || _event.CallCounter == value)
                    return;

                _event.CallCounter = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Binds the view-model to another event (or none).</summary>
        public void SetEvent(Event? evt)
        {
            _event = evt;
            RefreshFromEvent();
        }

        /// <summary>Re-reads values that may have been changed through the other editor mode.</summary>
        public void RefreshFromEvent()
        {
            _lockEvent = true;
            SelectedFunction = string.IsNullOrEmpty(_event?.Function) ? null : _event.Function;
            _lockEvent = false;

            UpdateMissingFunction();
            OnPropertyChanged(nameof(HasEvent));
            OnPropertyChanged(nameof(Argument));
            OnPropertyChanged(nameof(CallCounter));
        }

        /// <summary>Reloads the function list from the level script file and updates the notice shown instead of an empty list.</summary>
        public void Reload()
        {
            var settings = _editor.Level.Settings;
            var functions = new List<string>();
            var notify = string.Empty;
            var canBrowse = false;

            if (string.IsNullOrWhiteSpace(settings.TenLuaScriptFile))
            {
                notify = _localizationService["ScriptNotSpecified"];
                canBrowse = true;
            }
            else
            {
                var path = settings.MakeAbsolute(settings.TenLuaScriptFile);

                if (!File.Exists(path))
                {
                    notify = _localizationService.Format("ScriptNotFound", Path.GetFileName(path));
                    canBrowse = true;
                }
                else
                {
                    functions = ScriptingUtils.GetAllFunctionNames(path);
                    if (functions.Count == 0)
                        notify = _localizationService["ScriptNoFunctions"];
                }
            }

            Functions.Clear();
            foreach (var function in functions)
                Functions.Add(function);

            NotifyText = notify;
            CanBrowse = canBrowse;
            OnPropertyChanged(nameof(HasFunctions));
            RefreshFromEvent();
        }

        partial void OnSearchTextChanged(string value) => FilteredFunctions.Refresh();

        partial void OnNotifyTextChanged(string value) => OnPropertyChanged(nameof(HasNotify));

        partial void OnMissingFunctionTextChanged(string value) => OnPropertyChanged(nameof(HasMissingFunction));

        partial void OnSelectedFunctionChanged(string? value)
        {
            if (_event is null || _lockEvent)
                return;

            _event.Function = value ?? string.Empty;
            MissingFunctionText = string.Empty;
        }

        private void UpdateMissingFunction()
        {
            var function = _event?.Function;
            if (string.IsNullOrEmpty(function) || !HasFunctions || Functions.Contains(function))
            {
                MissingFunctionText = string.Empty;
                return;
            }

            MissingFunctionText = _localizationService.Format("FunctionNotFound", function);
        }

        [RelayCommand]
        private void Unassign()
        {
            SelectedFunction = null;
            if (_event is not null)
                _event.Function = string.Empty;

            MissingFunctionText = string.Empty;
        }

        [RelayCommand]
        private void Browse()
        {
            var settings = _editor.Level.Settings;
            var result = LevelFileDialog.BrowseFile(WPFUtils.GetWindowOwner(), settings, settings.TenLuaScriptFile, _localizationService["BrowseTitle"],
                [new FileFormat("LUA script file", "lua")], VariableType.LevelDirectory, false);

            if (result is null)
                return;

            settings.TenLuaScriptFile = result;
            _editor.LoadedScriptsChange();
        }
    }
}
