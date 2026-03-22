namespace TombEditor.Features.DockableViews.RoomOptionsPanel
{
    partial class RoomOptions
    {
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        private void InitializeComponent()
        {
            _elementHost = new System.Windows.Forms.Integration.ElementHost();
            _roomOptionsView = new RoomOptionsView();
            SuspendLayout();
            // 
            // _elementHost
            // 
            _elementHost.Dock = System.Windows.Forms.DockStyle.Fill;
            _elementHost.Name = "_elementHost";
            _elementHost.Child = _roomOptionsView;
            // 
            // RoomOptions
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(_elementHost);
            DockText = "Room Options";
            MinimumSize = new System.Drawing.Size(284, 236);
            Name = "RoomOptions";
            SerializationKey = "RoomOptions";
            Size = new System.Drawing.Size(284, 236);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost _elementHost;
        private RoomOptionsView _roomOptionsView;
    }
}
