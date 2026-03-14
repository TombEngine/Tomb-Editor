using DarkUI.Docking;

namespace TombEditor.ToolWindows
{
    public partial class SectorOptions : DarkToolWindow
    {
        public SectorOptions()
        {
            InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _sectorOptionsView?.Cleanup();
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }
    }
}
