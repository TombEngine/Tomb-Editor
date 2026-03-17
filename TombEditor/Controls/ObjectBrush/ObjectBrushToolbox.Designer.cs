namespace TombEditor.Controls.ObjectBrush
{
    partial class ObjectBrushToolbox
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _toolboxView?.Cleanup();
                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            _elementHost = new System.Windows.Forms.Integration.ElementHost();
            _toolboxView = new Views.ObjectBrushToolboxView();
            SuspendLayout();
            // 
            // _elementHost
            // 
            _elementHost.Dock = System.Windows.Forms.DockStyle.Fill;
            _elementHost.Name = "_elementHost";
            _elementHost.Child = _toolboxView;
            // 
            // ObjectBrushToolbox
            // 
            BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            Controls.Add(_elementHost);
            Name = "ObjectBrushToolbox";
            Size = new System.Drawing.Size(404, 94);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost _elementHost;
        private Views.ObjectBrushToolboxView _toolboxView;
    }
}
