using DarkUI.Docking;

namespace TombEditor.ToolWindows
{
    public partial class RoomOptions : DarkToolWindow
    {
        public RoomOptions()
        {
            InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _roomOptionsView?.Cleanup();
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }
    }
}
