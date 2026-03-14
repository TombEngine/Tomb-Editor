namespace TombEditor.ToolWindows
{
    partial class Palette
    {
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        private void InitializeComponent()
        {
            _elementHost = new System.Windows.Forms.Integration.ElementHost();
            _paletteView = new WPF.ToolWindows.Palette();
            SuspendLayout();
            // 
            // _elementHost
            // 
            _elementHost.Dock = System.Windows.Forms.DockStyle.Fill;
            _elementHost.Name = "_elementHost";
            _elementHost.Child = _paletteView;
            // 
            // Palette
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(_elementHost);
            DefaultDockArea = DarkUI.Docking.DarkDockArea.Bottom;
            DockText = "Palette";
            MinimumSize = new System.Drawing.Size(100, 100);
            Name = "Palette";
            SerializationKey = "Palette";
            Size = new System.Drawing.Size(645, 141);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost _elementHost;
        private WPF.ToolWindows.Palette _paletteView;
    }
}
