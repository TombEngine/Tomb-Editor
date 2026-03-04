using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.Forms.ViewModels;
using TombLib.Forms.Views;
using TombLib.LuaProperties;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace WadTool
{
    public partial class FormLuaProperties : DarkForm
    {
        private readonly WadToolClass _tool;
        private readonly Wad2 _wad;

        // Currently selected object.
        private IWadObjectId _currentObjectId;
        private IWadObject _currentWadObject;

        // WPF hosting.
        private readonly ElementHost _elementHost;
        private readonly LuaPropertyGridControl _wpfControl;
        private readonly LuaPropertyGridViewModel _viewModel;

        // Original containers for cancel/restore (key = objectId)
        private readonly Dictionary<IWadObjectId, LuaPropertyContainer> _originalProperties = new Dictionary<IWadObjectId, LuaPropertyContainer>();

        // Track which objects were actually modified
        private bool _anyChanges;

        public FormLuaProperties(WadToolClass tool, Wad2 wad, IWadObjectId initialObjectId = null)
        {
            _tool = tool;
            _wad = wad;

            InitializeComponent();
            PopulateObjectList();

            // Create WPF control + view model
            _viewModel = new LuaPropertyGridViewModel();
            _wpfControl = new LuaPropertyGridControl();
            _wpfControl.ViewModel = _viewModel;

            _elementHost = new ElementHost
            {
                Dock = DockStyle.Fill,
                Child = _wpfControl
            };

            panelContent.Controls.Add(_elementHost);

            // Track actual property modifications
            _viewModel.PropertyValueChanged += (s, ev) => _anyChanges = true;

            // Select the requested object, or the first one
            if (initialObjectId != null)
                SelectObjectInList(initialObjectId);
            else if (lstObjects.Items.Count > 0)
                lstObjects.SelectItem(0);
        }

        #region Object list management

        private void PopulateObjectList()
        {
            lstObjects.Items.Clear();
            var gameVersion = _wad.GameVersion;

            // Add moveables
            foreach (var kvp in _wad.Moveables)
            {
                string name = TrCatalog.GetMoveableName(gameVersion, kvp.Key.TypeId);
                var item = new DarkListItem(name)
                {
                    Tag = kvp.Key
                };
                lstObjects.Items.Add(item);
            }

            // Add statics
            foreach (var kvp in _wad.Statics)
            {
                string name = TrCatalog.GetStaticName(gameVersion, kvp.Key.TypeId);
                var item = new DarkListItem(name)
                {
                    Tag = kvp.Key
                };
                lstObjects.Items.Add(item);
            }
        }

        private void SelectObjectInList(IWadObjectId objectId)
        {
            for (int i = 0; i < lstObjects.Items.Count; i++)
            {
                if (lstObjects.Items[i].Tag is IWadObjectId id && id.Equals(objectId))
                {
                    lstObjects.SelectItem(i);
                    lstObjects.EnsureVisible();
                    return;
                }
            }

            // Fallback: select first
            if (lstObjects.Items.Count > 0)
                lstObjects.SelectItem(0);
        }

        private void lstObjects_SelectedIndicesChanged(object sender, EventArgs e)
        {
            if (lstObjects.SelectedIndices.Count == 0)
                return;

            var selected = lstObjects.Items[lstObjects.SelectedIndices[0]];
            if (selected.Tag is IWadObjectId objectId)
                LoadObject(objectId);
        }

        #endregion

        #region Property loading

        private void LoadObject(IWadObjectId objectId)
        {
            var wadObject = _wad.TryGet(objectId);
            if (wadObject == null)
                return;

            _currentObjectId = objectId;
            _currentWadObject = wadObject;

            // Snapshot original state for cancel/restore (only first time per object)
            if (!_originalProperties.ContainsKey(objectId))
            {
                var container = GetContainer(wadObject);
                _originalProperties[objectId] = container?.Clone() ?? new LuaPropertyContainer();
            }

            // Determine kind and type ID
            LuaPropertyObjectKind kind;
            uint typeId;
            string objectName;

            if (wadObject is WadMoveable)
            {
                kind = LuaPropertyObjectKind.Moveable;
                typeId = ((WadMoveableId)objectId).TypeId;
                objectName = TrCatalog.GetMoveableName(_wad.GameVersion, typeId);
            }
            else if (wadObject is WadStatic)
            {
                kind = LuaPropertyObjectKind.Static;
                typeId = ((WadStaticId)objectId).TypeId;
                objectName = TrCatalog.GetStaticName(_wad.GameVersion, typeId);
            }
            else
                return;

            _viewModel.Title = objectName;

            var definitions = LuaPropertyCatalog.GetDefinitions(kind, typeId);
            _viewModel.Load(definitions, GetContainer(wadObject));

            if (definitions.Count == 0)
                _viewModel.StatusMessage = "No item properties defined for this object type.";
        }

        private static LuaPropertyContainer GetContainer(IWadObject wadObject)
        {
            if (wadObject is WadMoveable moveable)
                return moveable.LuaProperties;
            if (wadObject is WadStatic staticObj)
                return staticObj.LuaProperties;
            return null;
        }

        #endregion

        #region Event handlers

        private void butOK_Click(object sender, EventArgs e)
        {
            // Values are already written to containers via the ViewModel's live-write.
            if (_anyChanges)
                _tool.ToggleUnsavedChanges();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            // Restore all original properties
            foreach (var kvp in _originalProperties)
            {
                var wadObject = _wad.TryGet(kvp.Key);
                if (wadObject == null)
                    continue;

                var container = GetContainer(wadObject);
                if (container != null)
                {
                    container.Clear();
                    foreach (var prop in kvp.Value.GetAll())
                        container.SetValue(prop.Key, prop.Value);
                }
            }

            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void butReset_Click(object sender, EventArgs e)
        {
            _viewModel.ResetAll();
        }

        #endregion
    }
}
