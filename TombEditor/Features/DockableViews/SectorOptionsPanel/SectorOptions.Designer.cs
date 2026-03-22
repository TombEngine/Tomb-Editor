namespace TombEditor.Features.DockableViews.SectorOptionsPanel
{
    partial class SectorOptions
    {
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        private void InitializeComponent()
        {
            _elementHost = new System.Windows.Forms.Integration.ElementHost();
            _sectorOptionsView = new SectorOptionsView();
            SuspendLayout();
            // 
            // _elementHost
            // 
            _elementHost.Dock = System.Windows.Forms.DockStyle.Fill;
            _elementHost.Name = "_elementHost";
            _elementHost.Child = _sectorOptionsView;
            // 
            // SectorOptions
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            Controls.Add(_elementHost);
            DockText = "Sector Options";
            MinimumSize = new System.Drawing.Size(284, 278);
            Name = "SectorOptions";
            SerializationKey = "SectorOptions";
            Size = new System.Drawing.Size(284, 278);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost _elementHost;
        private SectorOptionsView _sectorOptionsView;
    }
}
