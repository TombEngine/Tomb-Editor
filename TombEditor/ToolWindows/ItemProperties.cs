using DarkUI.Docking;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.Forms.ViewModels;
using TombLib.Forms.Views;
using TombLib.LevelData;
using TombLib.LuaProperties;
using TombLib.Wad;

namespace TombEditor.ToolWindows
{
    public partial class ItemProperties : DarkToolWindow
    {
        private readonly Editor _editor;

        // WPF hosting
        private readonly ElementHost _elementHost;
        private readonly LuaPropertyGridControl _wpfControl;
        private readonly LuaPropertyGridViewModel _viewModel;

        // Tracked selection
        private ObjectInstance _currentObject;

        public ItemProperties()
        {
            InitializeComponent();

            _editor = Editor.Instance;

            // Create WPF control + view model
            _viewModel = new LuaPropertyGridViewModel();
            _viewModel.PropertyValueChanged += OnPropertyValueChanged;

            _wpfControl = new LuaPropertyGridControl();
            _wpfControl.ViewModel = _viewModel;

            // ElementHost bridges WPF into the DarkToolWindow
            _elementHost = new ElementHost
            {
                Dock = DockStyle.Fill,
                Child = _wpfControl
            };
            this.Controls.Add(_elementHost);

            _editor.EditorEventRaised += EditorEventRaised;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _editor.EditorEventRaised -= EditorEventRaised;
                _viewModel.PropertyValueChanged -= OnPropertyValueChanged;
                _elementHost?.Dispose();
            }
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            // Respond to selection changes
            if (obj is Editor.SelectedObjectChangedEvent ||
                obj is Editor.SelectedRoomChangedEvent)
            {
                UpdatePropertyGrid();
            }

            // Respond to object property changes (e.g. if OCB or slot changed externally)
            if (obj is Editor.ObjectChangedEvent oce)
            {
                if (oce.Object == _currentObject)
                    UpdatePropertyGrid();
            }

            // Listen for wad/game version changes to update catalog
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

            // Only show for TombEngine levels
            if (!_editor.Level.IsTombEngine)
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
                var typeId = moveable.WadObjectId.TypeId;
                var definitions = LuaPropertyCatalog.GetDefinitions(LuaPropertyObjectKind.Moveable, typeId);

                // Get wad2 global defaults for this moveable type (if available).
                var wadMoveable = _editor.Level.Settings.WadTryGetMoveable(moveable.WadObjectId);
                var globalDefaults = wadMoveable?.LuaProperties;

                _viewModel.Title = $"Properties: {moveable.ItemType.ToString()}";
                _viewModel.Load(definitions, moveable.LuaProperties, globalDefaults);
                _viewModel.StatusMessage = "No properties defined for this moveable type.";
            }
            else if (selected is StaticInstance staticObj)
            {
                _currentObject = staticObj;
                var typeId = staticObj.WadObjectId.TypeId;
                var definitions = LuaPropertyCatalog.GetDefinitions(LuaPropertyObjectKind.Static, typeId);

                // Get wad2 global defaults for this static type (if available).
                var wadStatic = _editor.Level.Settings.WadTryGetStatic(staticObj.WadObjectId);
                var globalDefaults = wadStatic?.LuaProperties;

                _viewModel.Title = $"Properties: {staticObj.ItemType.ToString()}";
                _viewModel.Load(definitions, staticObj.LuaProperties, globalDefaults);
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

        /// <summary>
        /// When a property value changes in the WPF grid, notify the editor
        /// that the object has been modified so undo/save state is updated.
        /// </summary>
        private void OnPropertyValueChanged(object sender, EventArgs e)
        {
            if (_currentObject != null)
            {
                _editor.ObjectChange(_currentObject, ObjectChangeType.Change);
            }
        }
    }
}
