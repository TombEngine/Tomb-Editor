using DarkUI.Docking;

namespace TombEditor.ToolWindows
{
    public partial class Palette : DarkToolWindow
    {
        public Palette()
        {
            InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _paletteView?.Cleanup();
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }
    }
}
