using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DarkUI.Controls;


namespace TombLib.Controls
{
    public class DarkAutocompleteTextBox : DarkTextBox
    {
        private DarkListBox _listBox;
        private bool _isAdded;
        private String _formerValue = string.Empty;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<String> AutocompleteWords { get; set; } = new List<string>();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<String> SelectedWords
        {
            get
            {
                String[] result = Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return new List<String>(result);
            }
        }

        public DarkAutocompleteTextBox()
        {
            _listBox = new DarkListBox();
            _listBox.MouseDown += listbox_MouseDown;
            _listBox.MouseMove += listbox_MouseMove;

            KeyDown += this_KeyDown;
            KeyUp += this_KeyUp;
            Leave += delegate { HideListBox(); };

            HideListBox();
        }

        private void ShowListBox()
        {
            if (!_isAdded)
            {
                FindForm().Controls.Add(_listBox);
                _isAdded = true;
            }

            var textPosition = GetPositionFromCharIndex(SelectionStart > 0 ? SelectionStart - 1 : 0);
            var position = FindForm().PointToClient(this.Parent.PointToScreen(this.Location));

            _listBox.Left = position.X + textPosition.X;
            _listBox.Top = position.Y + Height + textPosition.Y;
            _listBox.Visible = true;
            _listBox.BringToFront();
        }

        private void HideListBox()
        {
            _listBox.Visible = false;
        }

        private void AddWordFromList()
        {
            InsertWord((String)_listBox.SelectedItem);
            HideListBox();
            _formerValue = Text;
        }

        private void listbox_MouseDown(object sender, MouseEventArgs e)
        {
            _listBox.SelectedIndex = _listBox.IndexFromPoint(e.Location);
            AddWordFromList();
            Focus();
        }

        private void listbox_MouseMove(object sender, MouseEventArgs e)
        {
            _listBox.SelectedIndex = _listBox.IndexFromPoint(e.Location);
        }

        private void this_KeyUp(object sender, KeyEventArgs e)
        {
            UpdateListBox();
        }

        private void this_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (_listBox.Visible)
                        AddWordFromList();
                    break;
                case Keys.Down:
                    if ((_listBox.Visible) && (_listBox.SelectedIndex < _listBox.Items.Count - 1))
                        _listBox.SelectedIndex++;
                    break;
                case Keys.Up:
                    if ((_listBox.Visible) && (_listBox.SelectedIndex > 0))
                        _listBox.SelectedIndex--;
                    break;
            }
        }

        private void UpdateListBox()
        {
            if (Text == _formerValue) return;
            _formerValue = Text;
            String word = GetWord();

            if (AutocompleteWords != null && AutocompleteWords.Count > 0 && word.Length > 0)
            {
                var matches = AutocompleteWords.FindAll(x => (x.StartsWith(word, StringComparison.OrdinalIgnoreCase) && !SelectedWords.Contains(x)));
                if (matches.Count > 0)
                {
                    _listBox.Items.Clear();
                    matches.ForEach(x => _listBox.Items.Add(x));

                    _listBox.SelectedIndex = 0;

                    ShowListBox();
                    Focus();

                    using (System.Drawing.Graphics graphics = _listBox.CreateGraphics())
                    {
                        _listBox.Size = Size.Empty; // Reset size

                        for (int i = 0; i < _listBox.Items.Count; i++)
                        {
                            _listBox.Height += _listBox.GetItemHeight(i);
                            int itemWidth = (int)graphics.MeasureString(((String)_listBox.Items[i]) + "_", _listBox.Font).Width;
                            _listBox.Width = (_listBox.Width < itemWidth) ? itemWidth : _listBox.Width;
                        }
                    }
                }
                else
                    HideListBox();
            }
            else
                HideListBox();
        }

        private String GetWord()
        {
            String text = Text;
            int pos = SelectionStart;

            int posStart = text.LastIndexOf(' ', (pos < 1) ? 0 : pos - 1);
            posStart = (posStart == -1) ? 0 : posStart + 1;
            int posEnd = text.IndexOf(' ', pos);
            posEnd = (posEnd == -1) ? text.Length : posEnd;

            int length = ((posEnd - posStart) < 0) ? 0 : posEnd - posStart;

            return text.Substring(posStart, length);
        }

        private void InsertWord(String newTag)
        {
            String text = Text;
            int pos = SelectionStart;

            int posStart = text.LastIndexOf(' ', (pos < 1) ? 0 : pos - 1);
            posStart = (posStart == -1) ? 0 : posStart + 1;
            int posEnd = text.IndexOf(' ', pos);

            String firstPart = text.Substring(0, posStart) + newTag;
            String updatedText = firstPart + ((posEnd == -1) ? "" : text.Substring(posEnd, text.Length - posEnd));

            Text = updatedText;
            SelectionStart = firstPart.Length;
        }
    }
}
