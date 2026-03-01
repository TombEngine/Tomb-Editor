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
    public partial class LuaProperties : DarkToolWindow
    {
        private readonly Editor _editor;

        // WPF hosting
        private readonly ElementHost _elementHost;
        private readonly LuaPropertyGridControl _wpfControl;
        private readonly LuaPropertyGridViewModel _viewModel;

        // Tracked selection
        private ObjectInstance _currentObject;

        public LuaProperties()
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
                obj is Editor.LevelChangedEvent)
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
                _viewModel.Title = "Lua Properties";
                _currentObject = null;
                return;
            }

            if (selected is MoveableInstance moveable)
            {
                _currentObject = moveable;
                var typeId = moveable.WadObjectId.TypeId;
                var definitions = LuaPropertyCatalog.GetDefinitions(LuaPropertyObjectKind.Moveable, typeId);

                _viewModel.Title = $"Properties: {moveable.ItemType.ToString()}";
                _viewModel.Load(definitions, moveable.LuaProperties);
            }
            else if (selected is StaticInstance staticObj)
            {
                _currentObject = staticObj;
                var typeId = staticObj.WadObjectId.TypeId;
                var definitions = LuaPropertyCatalog.GetDefinitions(LuaPropertyObjectKind.Static, typeId);

                _viewModel.Title = $"Properties: {staticObj.ItemType.ToString()}";
                _viewModel.Load(definitions, staticObj.LuaProperties);
            }
            else
            {
                _currentObject = null;
                _viewModel.Clear();
                _viewModel.Title = "Lua Properties";
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
