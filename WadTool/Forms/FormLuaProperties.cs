using DarkUI.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.Forms.ViewModels;
using TombLib.Forms.Views;
using TombLib.LuaProperties;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormLuaProperties : DarkForm
    {
        private readonly WadToolClass _tool;
        private readonly Wad2 _wad;
        private readonly IWadObject _wadObject;
        private readonly IWadObjectId _objectId;

        // WPF hosting
        private readonly ElementHost _elementHost;
        private readonly LuaPropertyGridControl _wpfControl;
        private readonly LuaPropertyGridViewModel _viewModel;

        // Original container for cancel/restore
        private readonly LuaPropertyContainer _originalProperties;

        public FormLuaProperties(WadToolClass tool, Wad2 wad, IWadObjectId objectId, IWadObject wadObject)
        {
            _tool = tool;
            _wad = wad;
            _wadObject = wadObject;
            _objectId = objectId;

            InitializeForm();

            // Determine kind and type ID
            LuaPropertyObjectKind kind;
            uint typeId;
            string objectName;

            if (wadObject is WadMoveable moveable)
            {
                kind = LuaPropertyObjectKind.Moveable;
                typeId = ((WadMoveableId)objectId).TypeId;
                objectName = TombLib.Wad.Catalog.TrCatalog.GetMoveableName(wad.GameVersion, typeId);
            }
            else if (wadObject is WadStatic staticObj)
            {
                kind = LuaPropertyObjectKind.Static;
                typeId = ((WadStaticId)objectId).TypeId;
                objectName = TombLib.Wad.Catalog.TrCatalog.GetStaticName(wad.GameVersion, typeId);
            }
            else
            {
                // Unsupported object type
                return;
            }

            Text = $"Lua Properties - {objectName} (Slot {typeId})";

            // Save original state for undo on cancel
            _originalProperties = GetContainer().Clone();

            // Create WPF control + view model
            _viewModel = new LuaPropertyGridViewModel();
            _viewModel.Title = objectName;

            var definitions = LuaPropertyCatalog.GetDefinitions(kind, typeId);
            _viewModel.Load(definitions, GetContainer());

            _wpfControl = new LuaPropertyGridControl();
            _wpfControl.ViewModel = _viewModel;

            _elementHost = new ElementHost
            {
                Dock = DockStyle.Fill,
                Child = _wpfControl
            };

            panelContent.Controls.Add(_elementHost);

            if (definitions.Count == 0)
            {
                lblNoProperties.Visible = true;
                lblNoProperties.BringToFront();
            }
        }

        private LuaPropertyContainer GetContainer()
        {
            if (_wadObject is WadMoveable moveable)
                return moveable.LuaProperties;
            if (_wadObject is WadStatic staticObj)
                return staticObj.LuaProperties;
            return null;
        }

        private void InitializeForm()
        {
            SuspendLayout();

            // Form settings
            Name = "FormLuaProperties";
            Size = new Size(420, 520);
            MinimumSize = new Size(350, 300);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;

            // Bottom button panel
            panelButtons = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                Padding = new Padding(6, 6, 6, 6)
            };

            butCancel = new DarkUI.Controls.DarkButton
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Dock = DockStyle.Right,
                Width = 80,
                Padding = new Padding(0)
            };
            butCancel.Click += ButCancel_Click;

            butOK = new DarkUI.Controls.DarkButton
            {
                Text = "OK",
                Dock = DockStyle.Right,
                Width = 80,
                Padding = new Padding(0)
            };
            butOK.Click += ButOK_Click;

            butReset = new DarkUI.Controls.DarkButton
            {
                Text = "Reset All",
                Dock = DockStyle.Left,
                Width = 80,
                Padding = new Padding(0)
            };
            butReset.Click += ButReset_Click;

            panelButtons.Controls.Add(butCancel);
            panelButtons.Controls.Add(butOK);
            panelButtons.Controls.Add(butReset);

            // Content panel for the WPF control
            panelContent = new Panel
            {
                Dock = DockStyle.Fill
            };

            // Label for when no properties exist
            lblNoProperties = new DarkUI.Controls.DarkLabel
            {
                Text = "No Lua properties defined for this object type.\n\nAdd property definitions in:\nCatalogs/TEN Property Catalogs/",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false,
                AutoSize = false
            };
            panelContent.Controls.Add(lblNoProperties);

            Controls.Add(panelContent);
            Controls.Add(panelButtons);

            AcceptButton = butOK;
            CancelButton = butCancel;

            ResumeLayout(false);
        }

        private void ButOK_Click(object sender, EventArgs e)
        {
            // Values are already written to the container via the ViewModel's live-write.
            // Just mark the wad as changed and close.
            _tool.ToggleUnsavedChanges();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButCancel_Click(object sender, EventArgs e)
        {
            // Restore original properties
            var container = GetContainer();
            if (container != null)
            {
                container.Clear();
                foreach (var kvp in _originalProperties.GetAll())
                    container.SetValue(kvp.Key, kvp.Value);
            }

            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ButReset_Click(object sender, EventArgs e)
        {
            _viewModel.ResetAll();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _elementHost?.Dispose();
            base.Dispose(disposing);
        }

        // Controls
        private Panel panelButtons;
        private Panel panelContent;
        private DarkUI.Controls.DarkButton butOK;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butReset;
        private DarkUI.Controls.DarkLabel lblNoProperties;
    }
}
