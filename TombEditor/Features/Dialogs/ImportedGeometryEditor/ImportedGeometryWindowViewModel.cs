#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelSettingsData = TombLib.LevelData.LevelSettings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using TombEditor.Features.Dialogs.LevelSettings;
using TombLib.LevelData;
using TombLib.Services;
using TombLib.WPF;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;
using TombLib.WPF.ViewModels;
using WpfColor = System.Windows.Media.Color;

namespace TombEditor.Features.Dialogs.ImportedGeometryEditor
{
    /// <summary>
    /// WPF port of <c>FormImportedGeometry</c>: edits an <see cref="ImportedGeometryInstance"/> (assigned
    /// model, lighting model, color, flags) and lets the user manage the level's imported geometry list
    /// on a clone of the settings. On OK the caller applies <see cref="NewLevelSettings"/> and the
    /// modified instance, mirroring the previous dialog.
    /// </summary>
    public sealed record LightingModelOption(ImportedGeometryLightingModel Value, string Name);

    public partial class ImportedGeometryWindowViewModel : ObservableObject, IModalDialogViewModel, IDisposable
    {
        private readonly ImportedGeometryInstance _instance;
        private readonly Editor _editor;
        private readonly IMessageService _messageService;
        private readonly ImportedGeometryListEditor _geometryListEditor;
        private readonly ILocalizationService _localizationService;
        private readonly Vector3 _oldColor;
        private ImportedGeometry.UniqueIDType? _currentModel;

        public LevelSettingsData OldLevelSettings { get; }
        public LevelSettingsData NewLevelSettings { get; }

        public ObservableCollection<ImportedGeometryRow> Geometries { get; } = new();
        /// <summary>Gets the lighting models with the captions the previous dialog showed.</summary>
        public IReadOnlyList<LightingModelOption> LightingModels { get; } =
        [
            new(ImportedGeometryLightingModel.NoLighting, "No lights"),
            new(ImportedGeometryLightingModel.VertexColors, "Vertex colors"),
            new(ImportedGeometryLightingModel.CalculateFromLightsInRoom, "Calculate from room lights"),
            new(ImportedGeometryLightingModel.TintAsAmbient, "Specified tint only")
        ];

        [ObservableProperty] private bool? _dialogResult;
        [ObservableProperty] private ImportedGeometryRow? _selectedGeometry;
        [ObservableProperty] private string _assignedModelDisplay = string.Empty;
        [ObservableProperty] private ImportedGeometryLightingModel _lightingModel;
        [ObservableProperty] private WpfColor _color;
        [ObservableProperty] private bool _sharpEdges;
        [ObservableProperty] private bool _hidden;
        [ObservableProperty] private bool _useAlphaTest;

        public bool IsTombEngine => OldLevelSettings.GameVersion == TRVersion.Game.TombEngine;

        /// <summary>
        /// Raised when the user requests the material editor for the assigned model.
        /// The view handles this by showing the previous material editor.
        /// </summary>
        public event EventHandler<ImportedGeometry>? EditMaterialsRequested;

        public ImportedGeometryWindowViewModel(
            ImportedGeometryInstance instance,
            LevelSettingsData levelSettings,
            IDialogService? dialogService = null,
            ILocalizationService? localizationService = null,
            Editor? editor = null)
        {
            _instance = instance;
            _editor = editor ?? Editor.Instance;
            _messageService = ServiceLocator.ResolveService<IMessageService>();
            _localizationService = ServiceLocator.ResolveService(localizationService)
                .WithKeysFor(this);

            OldLevelSettings = levelSettings;
            NewLevelSettings = levelSettings.Clone();

            _currentModel = instance.Model?.UniqueID;
            _oldColor = instance.Color;

            _lightingModel = instance.LightingModel;
            _color = (instance.Color * 0.5f).ToWPFColor();
            _sharpEdges = instance.SharpEdges;
            _hidden = instance.Hidden;
            _useAlphaTest = IsTombEngine && instance.UseAlphaTestInsteadOfAlphaBlend;

            foreach (var geometry in NewLevelSettings.ImportedGeometries)
                Geometries.Add(new ImportedGeometryRow(NewLevelSettings, geometry));

            _geometryListEditor = new ImportedGeometryListEditor(NewLevelSettings, Geometries, _editor, ServiceLocator.ResolveService(dialogService), _messageService);

            if (_currentModel != null)
                SelectedGeometry = Geometries.FirstOrDefault(g => g.Object.UniqueID == _currentModel);

            UpdateAssignedModelDisplay();
            _editor.EditorEventRaised += OnEditorEvent;
        }

        /// <summary>Gets or sets every highlighted row of the geometry list (see DataGridSelection).</summary>
        [ObservableProperty]
        private IList? _selectedGeometries;

        /// <summary>Stops listening to editor events once the dialog is closed.</summary>
        public void Dispose() => _editor.EditorEventRaised -= OnEditorEvent;

        // Reloading a geometry from its row raises this event; the rows are refreshed like the previous grid did.
        private void OnEditorEvent(IEditorEvent editorEvent)
        {
            if (editorEvent is not Editor.LoadedImportedGeometriesChangedEvent)
                return;

            foreach (var row in Geometries.ToList())
                row.Refresh();
        }

        private void UpdateAssignedModelDisplay()
        {
            var model = NewLevelSettings.ImportedGeometryFromID(_currentModel);
            AssignedModelDisplay = model == null ? _localizationService["AssignedModelNone"] : model.Info.Name + "   (" + model.Info.Path + ")";
        }

        [RelayCommand]
        private void Assign()
        {
            if (SelectedGeometry == null)
                return;
            _currentModel = SelectedGeometry.Object.UniqueID;
            UpdateAssignedModelDisplay();
        }

        [RelayCommand]
        private void AddImportedGeometry() => _geometryListEditor.Add(this);

        [RelayCommand]
        private void DeleteImportedGeometry() => _geometryListEditor.Delete(SelectedGeometries);

        [RelayCommand]
        private void PickColor() => EditorActions.EditColor(Owner, _instance, pickedColor => Color = pickedColor.ToWPFColor());

        [RelayCommand]
        private void EditMaterials()
        {
            var model = NewLevelSettings.ImportedGeometryFromID(_currentModel);
            if (model == null)
            {
                _messageService.ShowError(_localizationService["AssignModelMessage"]);
                return;
            }

            if (model.Textures.Count == 0 || model.LoadException != null)
            {
                _messageService.ShowError(_localizationService["NoTexturesMessage"]);
                return;
            }

            EditMaterialsRequested?.Invoke(this, model);
        }

        [RelayCommand]
        private void Ok()
        {
            _instance.Model = OldLevelSettings.ImportedGeometryFromID(_currentModel) ?? NewLevelSettings.ImportedGeometryFromID(_currentModel);
            _instance.LightingModel = LightingModel;
            _instance.Color = new Vector3(Color.R / 255.0f, Color.G / 255.0f, Color.B / 255.0f) * 2.0f;
            _instance.SharpEdges = SharpEdges;
            _instance.Hidden = Hidden;
            _instance.UseAlphaTestInsteadOfAlphaBlend = UseAlphaTest;
            DialogResult = true;
        }

        [RelayCommand]
        private void Cancel()
        {
            _instance.Color = _oldColor;
            DialogResult = false;
        }

        private static IWindowOwner Owner => WPFUtils.GetWindowOwner();
    }
}
