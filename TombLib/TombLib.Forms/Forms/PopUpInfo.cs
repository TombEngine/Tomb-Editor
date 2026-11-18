#nullable enable

using DarkUI.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;
using TombLib.Utils;

namespace TombLib.Forms
{
    // Retains the toast implementation for legacy desktop consumers.
    public partial class PopUpInfo : DarkForm
    {
        private const float _shiftCoeff = 0.25f;   // Intro animation push coefficient
        private const float _finalOpacity = 0.85f; // Non-animated opacity
        private const float _avgReadSpeed = 2.5f;  // Average reading speed

        private readonly Color _warningColor = Color.FromArgb(255, 192, 128);
        private readonly Color _errorColor = Color.FromArgb(255, 128, 128);
        private readonly Color _infoColor = Color.FromArgb(192, 192, 255);

        private readonly Timer _animTimer = new Timer() { Interval = 10 };
        private float _animProgress = 0.0f;
        private float _animTimeout = 1000.0f;

        private PopupAlignment _alignment;
        private int _padding;
        private Control? _parent;
        private Point _parentPosition;

        public PopUpInfo()
        {
            InitializeComponent();
            _animTimer.Tick += UpdateTimer;
        }

        // Generic function to call popup from any window
        public static void Show(PopUpInfo popup, IWin32Window? owner, Control parent, string message, PopupType type)
        {
            if (owner is not null)
            {
                var form = owner.Handle == IntPtr.Zero ? null : Control.FromHandle(owner.Handle) as Form;
                if (form is null || ActiveForm != form)
                    return;
            }

            switch (type)
            {
                case PopupType.None:
                    popup.ShowSimple(parent, message);
                    break;
                case PopupType.Info:
                    popup.ShowInfo(parent, message);
                    break;
                case PopupType.Warning:
                    popup.ShowWarning(parent, message);
                    break;
                case PopupType.Error:
                    popup.ShowError(parent, message);
                    break;
            }
    }

        // Common message helpers
        public void ShowInfo(Control parent, string message, string title = "Information")
        {
            Show(parent, PopupAlignment.BottomRight, message, title, PopupType.Info);
        }
        public void ShowWarning(Control parent, string message, string title = "Warning")
        {
            Show(parent, PopupAlignment.BottomRight, message, title, PopupType.Warning);
        }
        public void ShowError(Control parent, string message, string title = "Error")
        {
            Show(parent, PopupAlignment.BottomRight, message, title, PopupType.Error);
        }
        public void ShowSimple(Control parent, string message)
        {
            Show(parent, PopupAlignment.BottomRight, message);
        }

        public void Show(string message, PopupType type)
        {
            switch (type)
            {
                default:
                case PopupType.Info:
                    ShowInfo(this, message);
                    break;
                case PopupType.Warning:
                    ShowWarning(this, message);
                    break;
                case PopupType.Error:
                    ShowError(this, message);
                    break;
            }
        }

        public void Show(Control parent, PopupAlignment alignment, string message, string title = "", PopupType type = PopupType.Info, int timeout = 0, int padding = 10)
        {
            // No message means kill current pop-up
            if(message == "")
            {
                Hide();
                return;
            }

            _parent = parent;
            _alignment = alignment;
            _padding = padding;

            // Reset sizes
            Size = MinimumSize;
            MaximumSize = new Size((int)(parent.Size.Width * 0.7f), parent.Size.Height / 4);

            // Hide header if not specified
            bool titleIsVisible = (type == PopupType.None || title != string.Empty);
            if (titleIsVisible)
            {
                panelTitle.Visible = true;
                lblTitle.Text = title;

                // Setup header type
                switch (type)
                {
                    default:
                    case PopupType.Info:
                        lblTitle.ForeColor = _infoColor;
                        break;
                    case PopupType.Warning:
                        lblTitle.ForeColor = _warningColor;
                        break;
                    case PopupType.Error:
                        lblTitle.ForeColor = _errorColor;
                        break;
                }
            }
            else
                panelTitle.Visible = false;

            // Measure text dimensions
            SizeF size = TextRenderer.MeasureText(message,
                                                  lblMessage.Font,
                                                  new Size(MaximumSize.Width - panelMessage.Padding.Horizontal,
                                                           MaximumSize.Height - panelMessage.Padding.Vertical - (titleIsVisible ? panelTitle.Height : 0)),
                                                  TextFormatFlags.WordBreak);
            lblMessage.Text = message;
            Width = (int)Math.Ceiling(size.Width) + panelMessage.Padding.Horizontal + 1;
            Height = (int)Math.Ceiling(size.Height) + (titleIsVisible ? panelTitle.Height : 0) + panelMessage.Padding.Vertical + 1;

            // Setup message timeout
            if(timeout > 0)
                _animTimeout = timeout * 10.0f;
            else
            {
                // Default timeout is based on average reading speed.
                int wordCount = 0, index = 0;
                while (index < message.Length)
                {
                    while (index < message.Length && !char.IsWhiteSpace(message[index]))
                        index++;
                    wordCount++;
                    while (index < message.Length && char.IsWhiteSpace(message[index]))
                        index++;
                }
                _animTimeout = wordCount * _avgReadSpeed;
            }

            // Start intro animation
            _animProgress = 0.0f;
            _animTimer.Start();

            // Clear opacity to prevent flicker
            Opacity = 0.0f;

            if(!Visible)
                Show(parent);
        }

        private void UpdateTimer(object? sender, EventArgs e)
        {
            if (_parent is null || _parent.Disposing || _parent.IsDisposed)
            {
                _animTimer.Stop();
                _animTimer.Tick -= UpdateTimer;
                Close();
                return;
            }

            _animProgress += 0.1f;
            _animProgress = (float)Math.Round(_animProgress, 1, MidpointRounding.AwayFromZero);

            var callbackControlLocation = _parent.PointToScreen(Point.Empty);
            bool updateLocation = (_parentPosition != callbackControlLocation) ? true : false;
            int currentShift = 0;

            if (_animProgress <= 1.0f)
            {
                // Smoothly descend pop-up window from parent control using sine function
                var shiftDistance = Size.Height * _shiftCoeff;
                currentShift = (int)(shiftDistance - (shiftDistance * Math.Sin(_animProgress * Math.PI / 2)));

                updateLocation = true;  // Force location updating, as we are animating

                Opacity = _animProgress * _finalOpacity;
            }
            else
            {
                var outroTime = _animTimeout - _animProgress;

                if (outroTime < 1.0f)
                {
                    if (outroTime < 0.0f)
                        outroTime = 0.0f;
                    Opacity = outroTime * _finalOpacity;
                    if (outroTime == 0.0f)
                    {
                        _animProgress = 0.0f;
                        _animTimer.Stop();
                        Hide();
                    }
                }
            }

            if (updateLocation)
            {
                _parentPosition = callbackControlLocation;

                switch (_alignment)
                {
                    default:
                    case PopupAlignment.BottomLeft:
                        Location = new Point(callbackControlLocation.X + _padding, callbackControlLocation.Y + _parent.Size.Height - Size.Height - _padding - currentShift);
                        break;
                    case PopupAlignment.BottomRight:
                        Location = new Point(callbackControlLocation.X + _parent.Size.Width - Size.Width - _padding, callbackControlLocation.Y + _parent.Size.Height - Size.Height - _padding - currentShift);
                        break;
                    case PopupAlignment.TopLeft:
                        Location = new Point(callbackControlLocation.X + _padding, callbackControlLocation.Y + _padding + currentShift);
                        break;
                    case PopupAlignment.TopRight:
                        Location = new Point(callbackControlLocation.X + _parent.Size.Width - Size.Width - _padding, callbackControlLocation.Y + _padding + currentShift);
                        break;
                    case PopupAlignment.Center:
                        Location = new Point(callbackControlLocation.X + _parent.Size.Width / 2 - Size.Width / 2, callbackControlLocation.Y + _parent.Size.Height / 2 - Size.Height / 2 + currentShift);
                        break;
                }
            }
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80000 | 0x20; // Set the form click-through
                return cp;
            }
        }
    }
}
