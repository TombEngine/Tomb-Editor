using DarkUI.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace TombLib.Controls
{
    public partial class DarkDataGridViewControls : UserControl
    {
        private DarkDataGridView _dataGridView;

        public bool AlwaysInsertAtZero { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DarkDataGridView DataGridView
        {
            get { return _dataGridView; }
            set
            {
                if (_dataGridView != null)
                    _dataGridView.SelectionChanged -= dataGridView_SelectionChanged;

                _dataGridView = value;
                UpdateEnable();

                if (value != null)
                    value.SelectionChanged += dataGridView_SelectionChanged;
            }
        }

        // Must return a suitable object if data binding is used, otherwise a DataGridViewRow object must be returned. A IEnumerable of multiple rows can be returned alternatively.
        public Func<object> CreateNewRow;
        public Func<bool> DeleteRowCheckIfCancel = () => false;

        public DarkDataGridViewControls()
        {
            InitializeComponent();
        }

        private void butNew_Click(object sender, EventArgs e)
        {
            // Create new row(s)
            object newRowsOrSingleRow = CreateNewRow();
            if (newRowsOrSingleRow == null)
                return;

            // Select new row(s)
            foreach (var row in DataGridView.SelectedRows.Cast<DataGridViewRow>().ToList())
                row.Selected = false;
            int startIndex = DataGridView.EditableRowCollection.Count;
            foreach (object newRow in (newRowsOrSingleRow as IEnumerable) ?? new[] { newRowsOrSingleRow })
            {
                int newIndex = 0;

                if (AlwaysInsertAtZero)
                    DataGridView.EditableRowCollection.Insert(0, newRow);
                else
                    newIndex = DataGridView.EditableRowCollection.Add(newRow);

                DataGridView.ClearSelection();
                DataGridView.Rows[newIndex].Selected = true;
            }
            if (startIndex > 0 && DataGridView.EditableRowCollection.Count > startIndex)
                DataGridView.FirstDisplayedScrollingRowIndex = startIndex;
        }

        private void butDelete_Click(object sender, EventArgs e)
        {
            if (DataGridView.SelectedRows.Count <= 0 || DataGridView.EditableRowCollection.Count < 1)
                return;

            if (DeleteRowCheckIfCancel())
                return;

            // Get sorted selection
            List<int> selectedRowIndices = DataGridView.SelectedRows.Cast<DataGridViewRow>().Select(row => row.Index).ToList();
            selectedRowIndices.Sort();
            selectedRowIndices.Reverse();

            DataGridView.CurrentCell = null;
            DataGridView.ClearSelection();

            // Remove rows
            var rows = DataGridView.EditableRowCollection;
            foreach (var selectedRowIndex in selectedRowIndices)
                if (selectedRowIndex >= 0 && selectedRowIndex < rows.Count)
                    rows.RemoveAt(selectedRowIndex);
        }

        private void butUp_Click(object sender, EventArgs e)
        {
            if (DataGridView.SelectedRows.Count <= 0 || DataGridView.EditableRowCollection.Count < 1)
                return;

            // Get sorted selection
            List<int> selectedRowIndices = DataGridView.SelectedRows.Cast<DataGridViewRow>().Select(row => row.Index).ToList();
            selectedRowIndices.Sort();

            // Move items
            var rows = DataGridView.EditableRowCollection;
            int lastItemIndex = 0;
            foreach (int selectedRowIndex in selectedRowIndices)
            {
                if (selectedRowIndex == lastItemIndex++)
                    continue;

                var row = rows[selectedRowIndex - 1];
                rows.RemoveAt(selectedRowIndex - 1);
                rows.Insert(selectedRowIndex, row);
            }
        }

        private void butDown_Click(object sender, EventArgs e)
        {
            if (DataGridView.SelectedRows.Count <= 0 || DataGridView.EditableRowCollection.Count < 1)
                return;

            // Get sorted selection
            List<int> selectedRowIndices = DataGridView.SelectedRows.Cast<DataGridViewRow>()
                .Select(row => row.Index).ToList();
            selectedRowIndices.Sort();
            selectedRowIndices.Reverse();

            // Move items
            var rows = DataGridView.EditableRowCollection;
            int lastItemIndex = rows.Count - 1;
            foreach (int selectedRowIndex in selectedRowIndices)
            {
                if (selectedRowIndex == lastItemIndex--)
                    continue;

                var row = rows[selectedRowIndex + 1];
                rows.RemoveAt(selectedRowIndex + 1);
                rows.Insert(selectedRowIndex, row);
            }
        }

        private void UpdateEnable()
        {
            butNew.Enabled = DataGridView != null;
            butUp.Enabled = DataGridView != null && DataGridView.SelectedRows.Count > 0;
            butDown.Enabled = DataGridView != null && DataGridView.SelectedRows.Count > 0;
            butDelete.Enabled = DataGridView != null && DataGridView.SelectedRows.Count > 0;
        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            UpdateEnable();
        }

        private bool _allowUserMove = true;
        [DefaultValue(true)]
        public bool AllowUserMove
        {
            get { return _allowUserMove; }
            set { butDown.Visible = butUp.Visible = _allowUserMove = value; }
        }

        private bool _allowUserDelete = true;
        [DefaultValue(true)]
        public bool AllowUserDelete
        {
            get { return _allowUserDelete; }
            set { butDelete.Visible = _allowUserDelete = value; }
        }

        private bool _allowUserNew = true;
        [DefaultValue(true)]
        public bool AllowUserNew
        {
            get { return _allowUserNew; }
            set { butNew.Visible = _allowUserNew = value; }
        }
    }
}
