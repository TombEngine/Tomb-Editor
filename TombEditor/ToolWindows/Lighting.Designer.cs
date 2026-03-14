namespace TombEditor.ToolWindows
{
    partial class Lighting
    {
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        private void InitializeComponent()
        {
            _elementHost = new System.Windows.Forms.Integration.ElementHost();
            _lightingView = new WPF.ToolWindows.Lighting();
            SuspendLayout();
            // 
            // _elementHost
            // 
            _elementHost.Dock = System.Windows.Forms.DockStyle.Fill;
            _elementHost.Name = "_elementHost";
            _elementHost.Child = _lightingView;
            // 
            // Lighting
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(_elementHost);
            DefaultDockArea = DarkUI.Docking.DarkDockArea.Bottom;
            DockText = "Lighting";
            MinimumSize = new System.Drawing.Size(371, 141);
            Name = "Lighting";
            SerializationKey = "Lighting";
            Size = new System.Drawing.Size(371, 141);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost _elementHost;
        private WPF.ToolWindows.Lighting _lightingView;
    }
}
