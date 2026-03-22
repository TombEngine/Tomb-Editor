namespace TombEditor.Features.Panel3D.ToolPalette
{
    partial class Toolbox
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _toolBoxView?.Cleanup();
                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            _elementHost = new System.Windows.Forms.Integration.ElementHost();
            _toolBoxView = new ToolboxView();
            SuspendLayout();
            // 
            // _elementHost
            // 
            _elementHost.Dock = System.Windows.Forms.DockStyle.Fill;
            _elementHost.Name = "_elementHost";
            _elementHost.Child = _toolBoxView;
            // 
            // ToolBox
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            Controls.Add(_elementHost);
            Padding = new System.Windows.Forms.Padding(0);
            Margin = new System.Windows.Forms.Padding(0);
            Name = "ToolBox";
            Size = new System.Drawing.Size(35, 599);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost _elementHost;
        private ToolboxView _toolBoxView;
    }
}
