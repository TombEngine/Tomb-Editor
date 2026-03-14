using DarkUI.Docking;

namespace TombEditor.ToolWindows
{
    public partial class Lighting : DarkToolWindow
    {
        public Lighting()
        {
            InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _lightingView?.Cleanup();
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }
    }
}
