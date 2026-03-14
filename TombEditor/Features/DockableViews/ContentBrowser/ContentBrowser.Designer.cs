namespace TombEditor.Features.DockableViews.ContentBrowser
{
    partial class ContentBrowser
    {
        private System.ComponentModel.IContainer components = null;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.elementHost = new System.Windows.Forms.Integration.ElementHost();
            this.contentBrowserView = new ContentBrowserView();
            this.SuspendLayout();
            // 
            // elementHost
            // 
            this.elementHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.elementHost.Location = new System.Drawing.Point(0, 25);
            this.elementHost.Name = "elementHost";
            this.elementHost.Size = new System.Drawing.Size(350, 350);
            this.elementHost.TabIndex = 0;
            this.elementHost.Child = this.contentBrowserView;
            // 
            // ContentBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.elementHost);
            this.DockText = "Content Browser";
            this.MinimumSize = new System.Drawing.Size(250, 200);
            this.Name = "ContentBrowser";
            this.SerializationKey = "ContentBrowser";
            this.Size = new System.Drawing.Size(350, 400);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Integration.ElementHost elementHost;
        private ContentBrowserView contentBrowserView;
    }
}
