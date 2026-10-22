using DarkUI.Controls;
using System;
using System.Windows.Forms;
using TombLib.Scripting.Objects;

namespace TombLib.Scripting.Controls
{
	public class ExtendedDarkDataGridView : DarkDataGridView
	{
		#region Fields

		private DataGridViewTextBoxEditingControl _editingControl;

		protected DataGridUndoItem CachedUndoItem;

		#endregion Fields

		#region Construction

		public ExtendedDarkDataGridView()
		{
			CellBeginEdit += OnCellBeginEdit;
			CellEndEdit += OnCellEndEdit;
			CellMouseDown += OnCellMouseDown;
			EditingControlShowing += OnEditingControlShowing;
		}

		#endregion Construction

		#region Override methods

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (IsCurrentCellInEditMode)
			{
				if (keyData == (Keys.Control | Keys.C))
				{
					_editingControl.Copy();
					keyData = Keys.None;
				}
				else if (keyData == Keys.Delete)
				{
					keyData = Keys.None;
				}
			}
			else
			{
				if (keyData == (Keys.Control | Keys.X))
				{
					CutCellText();
					keyData = Keys.None;
				}
				else if (keyData == (Keys.Control | Keys.C))
				{
					CopyCellText();
					keyData = Keys.None;
				}
				else if (keyData == (Keys.Control | Keys.V))
				{
					PasteCellText();
					keyData = Keys.None;
				}
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		#endregion Override methods

		#region Public methods

		public void AutoAdjustRowHeight(int rowIndex, bool useEditingControl = false)
		{
			DataGridViewRow row = Rows[rowIndex];

			int maxLineCount = 0;

			if (useEditingControl)
				maxLineCount = _editingControl.Text.Split('\n').Length;
			else
				foreach (DataGridViewCell cell in row.Cells)
					maxLineCount = Math.Max(maxLineCount, cell.Value?.ToString().Split('\n').Length ?? 1);

			int fontHeight = row.DefaultCellStyle.Font.Height;
			int additionalPadding = row.DefaultCellStyle.Font.Height / 2;

			row.Height = fontHeight * maxLineCount + additionalPadding;
		}

		public void CutCellText()
		{
			if (SelectedCells.Count > 0)
			{
				object cachedValue = SelectedCells[0].Value;

				Clipboard.SetText(SelectedCells[0].Value.ToString());

				if (!SelectedCells[0].OwningColumn.ReadOnly)
				{
					SelectedCells[0].Value = string.Empty;

					OnCellContentChanged(new CellContentChangedEventArgs(
						SelectedCells[0].ColumnIndex, SelectedCells[0].RowIndex,
						cachedValue, SelectedCells[0].Value));
				}
			}
		}

		public void CopyCellText()
		{
			if (SelectedCells.Count > 0)
				Clipboard.SetText(SelectedCells[0].Value.ToString());
		}

		public void PasteCellText()
		{
			if (SelectedCells.Count > 0 && Clipboard.ContainsText() && !SelectedCells[0].OwningColumn.ReadOnly)
			{
				object cachedValue = SelectedCells[0].Value;

				SelectedCells[0].Value = Clipboard.GetText();

				OnCellContentChanged(new CellContentChangedEventArgs(
					SelectedCells[0].ColumnIndex, SelectedCells[0].RowIndex,
					cachedValue, SelectedCells[0].Value));
			}
		}

		#endregion Public methods

		#region Events

		public event CellValueChangedEventHandler CellContentChanged;
		protected virtual void OnCellContentChanged(CellContentChangedEventArgs e)
			=> CellContentChanged?.Invoke(this, e);

		private void OnEditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
		{
			if (e.Control is DataGridViewTextBoxEditingControl editingControl)
			{
				_editingControl = editingControl;
				_editingControl.TextChanged += OnEditingControlTextChanged;
			}
		}

		private void OnEditingControlTextChanged(object sender, EventArgs e)
			=> AutoAdjustRowHeight(_editingControl.EditingControlRowIndex, true);

		private void OnCellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
		{
			object cellValue = this[e.ColumnIndex, e.RowIndex].Value;
			CachedUndoItem = new DataGridUndoItem(this, e.ColumnIndex, e.RowIndex, cellValue);
		}

		private void OnCellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			DataGridViewCell tableCell = this[e.ColumnIndex, e.RowIndex];

			if (tableCell.Value != CachedUndoItem.Value)
				OnCellContentChanged(new CellContentChangedEventArgs(
					CachedUndoItem.ColumnIndex, CachedUndoItem.RowIndex,
					CachedUndoItem.Value, tableCell.Value));
		}

		private void OnCellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right && e.RowIndex != -1 && e.ColumnIndex != -1)
				CurrentCell = Rows[e.RowIndex].Cells[e.ColumnIndex];
		}

		#endregion Events
	}
}
