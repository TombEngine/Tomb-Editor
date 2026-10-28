#nullable enable

using System;
using System.Linq;
using System.Windows.Controls;
using TombLib.Forms;
using TombLib.Forms.ViewModels;
using TombLib.LevelData;
using TombLib.LuaProperties;
using TombLib.Wad.Catalog;

namespace TombEditor.Features.DockableViews.ItemPropertiesPanel;

/// <summary>
/// WPF counterpart of the WinForms <c>ToolWindows.ItemProperties</c> tool window.
/// Both shells share <c>LuaPropertyGridControl</c>/<c>LuaPropertyGridViewModel</c>;
/// this view only adds the editor-selection tracking around them.
/// </summary>
public partial class ItemPropertiesView : UserControl
{
    private readonly Editor _editor;
    private readonly LuaPropertyGridViewModel _viewModel;

    private ObjectInstance? _currentObject;

    public ItemPropertiesView()
    {
        InitializeComponent();

        _editor = Editor.Instance;

        _viewModel = new LuaPropertyGridViewModel();
        _viewModel.PropertyValueChanged += OnPropertyValueChanged;
        propertyGrid.ViewModel = _viewModel;

        _editor.EditorEventRaised += EditorEventRaised;

        UpdatePropertyGrid();
    }

    public void Cleanup()
    {
        _editor.EditorEventRaised -= EditorEventRaised;
        _viewModel.PropertyValueChanged -= OnPropertyValueChanged;
    }

    private void EditorEventRaised(IEditorEvent obj)
    {
        if (obj is Editor.SelectedObjectChangedEvent ||
            obj is Editor.SelectedRoomChangedEvent)
        {
            UpdatePropertyGrid();
        }

        // Object property changes from elsewhere (e.g. OCB or slot changed externally).
        if (obj is Editor.ObjectChangedEvent objEvent && objEvent.Object == _currentObject)
            UpdatePropertyGrid();

        if (obj is Editor.LoadedWadsChangedEvent ||
            obj is Editor.GameVersionChangedEvent ||
            obj is Editor.LevelChangedEvent ||
            obj is Editor.InitEvent)
        {
            UpdatePropertyGrid();
        }
    }

    private void UpdatePropertyGrid()
    {
        var selected = _editor.SelectedObject;

        if (!_editor.Level.IsTombEngine && !_editor.Level.IsTRX)
        {
            _viewModel.Clear();
            _viewModel.Title = "Item Properties";
            _viewModel.StatusMessage = "Not supported for this engine target.";
            _currentObject = null;
            return;
        }

        if (selected is MoveableInstance moveable)
        {
            _currentObject = moveable;
            var typeId = _editor.Level.IsTombEngine
                ? moveable.WadObjectId.TypeId
                : TrCatalog.GetSubstituteID(_editor.Level.Settings.GameVersion, moveable.WadObjectId.TypeId);
            var definitions = LuaPropertyCatalog.GetDefinitions(ObjectKind.Moveable, typeId, _editor.Level.Settings.GameVersion);
            var globalDefaults = _editor.Level.Settings.WadTryGetMoveable(moveable.WadObjectId)?.LuaProperties;

            _viewModel.Title = $"Properties: {moveable.ItemType}";
            _viewModel.Load(definitions, moveable.LuaProperties, globalDefaults, moveable.Ocb != 0);
            _viewModel.StatusMessage = "No properties defined for this moveable type.";

            // Warn if legacy OCB is set on a moveable that has properties replacing OCB functionality.
            if (moveable.Ocb != 0 && definitions.Any(d => d.ReplacesOCB))
            {
                string defName = definitions.Count(d => d.ReplacesOCB) > 1
                    ? "some properties"
                    : "\"" + definitions.First(d => d.ReplacesOCB).DisplayName + "\" property";

                _editor.SendMessage("A legacy OCB field overrides " + defName + " for this moveable." + "\n" +
                    "Reset OCB to 0 and use properties instead to solve conflict.", PopupType.Warning);
            }
        }
        else if (selected is StaticInstance staticObj)
        {
            _currentObject = staticObj;
            var definitions = LuaPropertyCatalog.GetDefinitions(ObjectKind.Static, staticObj.WadObjectId.TypeId, _editor.Level.Settings.GameVersion);
            var globalDefaults = _editor.Level.Settings.WadTryGetStatic(staticObj.WadObjectId)?.LuaProperties;

            _viewModel.Title = $"Properties: {staticObj.ItemType}";
            _viewModel.Load(definitions, staticObj.LuaProperties, globalDefaults, staticObj.Ocb != 0);
            _viewModel.StatusMessage = "No properties defined for this static mesh slot.";
        }
        else
        {
            _currentObject = null;
            _viewModel.Clear();
            _viewModel.Title = "Item Properties";
            _viewModel.StatusMessage = "Select a valid object to edit properties.";
        }
    }

    private void OnPropertyValueChanged(object? sender, EventArgs e)
    {
        if (_currentObject is not null)
            _editor.ObjectChange(_currentObject, ObjectChangeType.Change);
    }
}
