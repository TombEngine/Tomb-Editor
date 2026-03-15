using System;
using System.Windows.Forms;

namespace TombEditor.Controls
{
    public partial class ToolBox : UserControl
    {
        public ToolStripLayoutStyle LayoutStyle
        {
            get => _layoutStyle;
            set
            {
                if (value == _layoutStyle)
                    return;

                _layoutStyle = value;

                if (value == ToolStripLayoutStyle.Flow)
                    _toolBoxView.PanelOrientation = System.Windows.Controls.Orientation.Horizontal;
                else
                    _toolBoxView.PanelOrientation = System.Windows.Controls.Orientation.Vertical;

                _toolBoxView.RequestHeightUpdate();
            }
        }

        private ToolStripLayoutStyle _layoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
        private int _lastMeasuredWidth = -1;

        public ToolBox()
        {
            InitializeComponent();
            _toolBoxView.SetWinFormsHost(_elementHost);
            _toolBoxView.PreferredHeightChanged += OnPreferredHeightChanged;
            _toolBoxView.PreferredWidthChanged += OnPreferredWidthChanged;
        }

        private void OnPreferredHeightChanged(int preferredHeight)
        {
            if (Height != preferredHeight)
                Height = preferredHeight;
        }

        private void OnPreferredWidthChanged(int preferredWidth)
        {
            if (preferredWidth > 0 && Width != preferredWidth)
                Width = preferredWidth;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (Width != _lastMeasuredWidth)
            {
                _lastMeasuredWidth = Width;
                _toolBoxView?.RequestHeightUpdate();
            }
        }
    }
}
