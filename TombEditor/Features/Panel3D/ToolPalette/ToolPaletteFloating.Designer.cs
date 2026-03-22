namespace TombEditor.Features.Panel3D.ToolPalette
{
    partial class ToolPaletteFloating
    {
        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolBox = new Toolbox();
            this.SuspendLayout();
            // 
            // toolBox
            // 
            this.toolBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.toolBox.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.toolBox.Location = new System.Drawing.Point(0, 16);
            this.toolBox.Name = "toolBox";
            this.toolBox.Size = new System.Drawing.Size(26, 431);
            this.toolBox.TabIndex = 0;
            this.toolBox.SizeChanged += new System.EventHandler(this.toolBox_SizeChanged);
            // 
            // ToolPaletteFloating
            // 
            this.AutoAnchor = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.toolBox);
            this.GripSize = 10;
            this.Name = "ToolPaletteFloating";
            this.Size = new System.Drawing.Size(28, 447);
            this.ResumeLayout(false);

        }

        #endregion

        private Toolbox toolBox;
    }
}
