namespace TombIDE.ScriptingStudio.Settings
{
	partial class Tomb1MainSettingsControl
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.button_ImportScheme = new DarkUI.Controls.DarkButton();
			this.buttonContextMenu = new DarkUI.Controls.DarkContextMenu();
			this.menuItem_Bold = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Italic = new System.Windows.Forms.ToolStripMenuItem();
			this.checkBox_Autocomplete = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_HighlightCurrentLine = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_LineNumbers = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_VisibleSpaces = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_VisibleTabs = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_WordWrapping = new DarkUI.Controls.DarkCheckBox();
			this.colorButton_Background = new DarkUI.Controls.DarkButton();
			this.colorButton_Comments = new DarkUI.Controls.DarkButton();
			this.colorButton_Foreground = new DarkUI.Controls.DarkButton();
			this.colorButton_Collections = new DarkUI.Controls.DarkButton();
			this.colorButton_Constants = new DarkUI.Controls.DarkButton();
			this.colorButton_Values = new DarkUI.Controls.DarkButton();
			this.colorButton_Properties = new DarkUI.Controls.DarkButton();
			this.colorButton_Strings = new DarkUI.Controls.DarkButton();
			this.colorDialog = new System.Windows.Forms.ColorDialog();
			this.darkLabel1 = new DarkUI.Controls.DarkLabel();
			this.darkLabel10 = new DarkUI.Controls.DarkLabel();
			this.darkLabel11 = new DarkUI.Controls.DarkLabel();
			this.darkLabel12 = new DarkUI.Controls.DarkLabel();
			this.darkLabel2 = new DarkUI.Controls.DarkLabel();
			this.darkLabel3 = new DarkUI.Controls.DarkLabel();
			this.darkLabel4 = new DarkUI.Controls.DarkLabel();
			this.darkLabel5 = new DarkUI.Controls.DarkLabel();
			this.darkLabel6 = new DarkUI.Controls.DarkLabel();
			this.darkLabel7 = new DarkUI.Controls.DarkLabel();
			this.darkLabel8 = new DarkUI.Controls.DarkLabel();
			this.darkLabel9 = new DarkUI.Controls.DarkLabel();
			this.elementHost = new System.Windows.Forms.Integration.ElementHost();
			this.groupBox_Colors = new DarkUI.Controls.DarkGroupBox();
			this.button_SaveScheme = new DarkUI.Controls.DarkButton();
			this.button_DeleteScheme = new DarkUI.Controls.DarkButton();
			this.button_OpenSchemesFolder = new DarkUI.Controls.DarkButton();
			this.comboBox_ColorSchemes = new DarkUI.Controls.DarkComboBox();
			this.groupBox_Preview = new DarkUI.Controls.DarkGroupBox();
			this.numeric_FontSize = new DarkUI.Controls.DarkNumericUpDown();
			this.numeric_UndoStackSize = new DarkUI.Controls.DarkNumericUpDown();
			this.sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			this.checkBox_CloseBraces = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_CloseQuotes = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_CloseBrackets = new DarkUI.Controls.DarkCheckBox();
			this.comboBox_FontFamily = new DarkUI.Controls.DarkComboBox();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.checkBox_AutoAddCommas = new DarkUI.Controls.DarkCheckBox();
			this.buttonContextMenu.SuspendLayout();
			this.groupBox_Colors.SuspendLayout();
			this.groupBox_Preview.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numeric_FontSize)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numeric_UndoStackSize)).BeginInit();
			this.sectionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_ImportScheme
			// 
			this.button_ImportScheme.Checked = false;
			this.button_ImportScheme.Image = global::TombIDE.ScriptingStudio.Properties.Resources.Import_16;
			this.button_ImportScheme.Location = new System.Drawing.Point(483, 16);
			this.button_ImportScheme.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.button_ImportScheme.Name = "button_ImportScheme";
			this.button_ImportScheme.Size = new System.Drawing.Size(25, 25);
			this.button_ImportScheme.TabIndex = 21;
			this.toolTip.SetToolTip(this.button_ImportScheme, "Import Scheme...");
			this.button_ImportScheme.Click += new System.EventHandler(this.button_ImportScheme_Click);
			// 
			// buttonContextMenu
			// 
			this.buttonContextMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.buttonContextMenu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.buttonContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_Bold,
            this.menuItem_Italic});
			this.buttonContextMenu.Name = "buttonContextMenu";
			this.buttonContextMenu.Size = new System.Drawing.Size(100, 48);
			this.buttonContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.buttonContextMenu_Opening);
			// 
			// menuItem_Bold
			// 
			this.menuItem_Bold.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Bold.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Bold.Name = "menuItem_Bold";
			this.menuItem_Bold.Size = new System.Drawing.Size(99, 22);
			this.menuItem_Bold.Text = "Bold";
			this.menuItem_Bold.Click += new System.EventHandler(this.menuItem_Bold_Click);
			// 
			// menuItem_Italic
			// 
			this.menuItem_Italic.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Italic.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Italic.Name = "menuItem_Italic";
			this.menuItem_Italic.Size = new System.Drawing.Size(99, 22);
			this.menuItem_Italic.Text = "Italic";
			this.menuItem_Italic.Click += new System.EventHandler(this.menuItem_Italic_Click);
			// 
			// checkBox_Autocomplete
			// 
			this.checkBox_Autocomplete.AutoSize = true;
			this.checkBox_Autocomplete.Location = new System.Drawing.Point(6, 166);
			this.checkBox_Autocomplete.Margin = new System.Windows.Forms.Padding(6, 6, 3, 0);
			this.checkBox_Autocomplete.Name = "checkBox_Autocomplete";
			this.checkBox_Autocomplete.Size = new System.Drawing.Size(135, 17);
			this.checkBox_Autocomplete.TabIndex = 6;
			this.checkBox_Autocomplete.Text = "Enable autocomplete";
			// 
			// checkBox_HighlightCurrentLine
			// 
			this.checkBox_HighlightCurrentLine.AutoSize = true;
			this.checkBox_HighlightCurrentLine.Location = new System.Drawing.Point(6, 304);
			this.checkBox_HighlightCurrentLine.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
			this.checkBox_HighlightCurrentLine.Name = "checkBox_HighlightCurrentLine";
			this.checkBox_HighlightCurrentLine.Size = new System.Drawing.Size(137, 17);
			this.checkBox_HighlightCurrentLine.TabIndex = 11;
			this.checkBox_HighlightCurrentLine.Text = "Highlight current line";
			this.checkBox_HighlightCurrentLine.CheckedChanged += new System.EventHandler(this.VisiblePreviewSetting_Changed);
			// 
			// checkBox_LineNumbers
			// 
			this.checkBox_LineNumbers.AutoSize = true;
			this.checkBox_LineNumbers.Location = new System.Drawing.Point(6, 327);
			this.checkBox_LineNumbers.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
			this.checkBox_LineNumbers.Name = "checkBox_LineNumbers";
			this.checkBox_LineNumbers.Size = new System.Drawing.Size(125, 17);
			this.checkBox_LineNumbers.TabIndex = 12;
			this.checkBox_LineNumbers.Text = "Show line numbers";
			this.checkBox_LineNumbers.CheckedChanged += new System.EventHandler(this.VisiblePreviewSetting_Changed);
			// 
			// checkBox_VisibleSpaces
			// 
			this.checkBox_VisibleSpaces.AutoSize = true;
			this.checkBox_VisibleSpaces.Location = new System.Drawing.Point(6, 350);
			this.checkBox_VisibleSpaces.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
			this.checkBox_VisibleSpaces.Name = "checkBox_VisibleSpaces";
			this.checkBox_VisibleSpaces.Size = new System.Drawing.Size(127, 17);
			this.checkBox_VisibleSpaces.TabIndex = 14;
			this.checkBox_VisibleSpaces.Text = "Show visible spaces";
			this.checkBox_VisibleSpaces.CheckedChanged += new System.EventHandler(this.VisiblePreviewSetting_Changed);
			// 
			// checkBox_VisibleTabs
			// 
			this.checkBox_VisibleTabs.AutoSize = true;
			this.checkBox_VisibleTabs.Location = new System.Drawing.Point(6, 373);
			this.checkBox_VisibleTabs.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.checkBox_VisibleTabs.Name = "checkBox_VisibleTabs";
			this.checkBox_VisibleTabs.Size = new System.Drawing.Size(115, 17);
			this.checkBox_VisibleTabs.TabIndex = 15;
			this.checkBox_VisibleTabs.Text = "Show visible tabs";
			this.checkBox_VisibleTabs.CheckedChanged += new System.EventHandler(this.VisiblePreviewSetting_Changed);
			// 
			// checkBox_WordWrapping
			// 
			this.checkBox_WordWrapping.AutoSize = true;
			this.checkBox_WordWrapping.Location = new System.Drawing.Point(6, 189);
			this.checkBox_WordWrapping.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
			this.checkBox_WordWrapping.Name = "checkBox_WordWrapping";
			this.checkBox_WordWrapping.Size = new System.Drawing.Size(108, 17);
			this.checkBox_WordWrapping.TabIndex = 10;
			this.checkBox_WordWrapping.Text = "Word wrapping";
			this.checkBox_WordWrapping.CheckedChanged += new System.EventHandler(this.VisiblePreviewSetting_Changed);
			// 
			// colorButton_Background
			// 
			this.colorButton_Background.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
			this.colorButton_Background.BackColorUseGeneric = false;
			this.colorButton_Background.Checked = false;
			this.colorButton_Background.Location = new System.Drawing.Point(370, 59);
			this.colorButton_Background.Margin = new System.Windows.Forms.Padding(6, 0, 9, 3);
			this.colorButton_Background.Name = "colorButton_Background";
			this.colorButton_Background.Size = new System.Drawing.Size(169, 25);
			this.colorButton_Background.TabIndex = 14;
			this.colorButton_Background.Click += new System.EventHandler(this.button_Color_Click);
			// 
			// colorButton_Comments
			// 
			this.colorButton_Comments.BackColor = System.Drawing.Color.Green;
			this.colorButton_Comments.BackColorUseGeneric = false;
			this.colorButton_Comments.Checked = false;
			this.colorButton_Comments.ContextMenuStrip = this.buttonContextMenu;
			this.colorButton_Comments.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.colorButton_Comments.Location = new System.Drawing.Point(191, 141);
			this.colorButton_Comments.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.colorButton_Comments.Name = "colorButton_Comments";
			this.colorButton_Comments.Size = new System.Drawing.Size(170, 25);
			this.colorButton_Comments.TabIndex = 11;
			this.colorButton_Comments.UseForeColor = true;
			this.colorButton_Comments.Click += new System.EventHandler(this.button_Color_Click);
			// 
			// colorButton_Foreground
			// 
			this.colorButton_Foreground.BackColor = System.Drawing.Color.Gainsboro;
			this.colorButton_Foreground.BackColorUseGeneric = false;
			this.colorButton_Foreground.Checked = false;
			this.colorButton_Foreground.Location = new System.Drawing.Point(370, 100);
			this.colorButton_Foreground.Margin = new System.Windows.Forms.Padding(6, 0, 9, 3);
			this.colorButton_Foreground.Name = "colorButton_Foreground";
			this.colorButton_Foreground.Size = new System.Drawing.Size(169, 25);
			this.colorButton_Foreground.TabIndex = 16;
			this.colorButton_Foreground.Click += new System.EventHandler(this.button_Color_Click);
			// 
			// colorButton_Collections
			// 
			this.colorButton_Collections.BackColor = System.Drawing.Color.SpringGreen;
			this.colorButton_Collections.BackColorUseGeneric = false;
			this.colorButton_Collections.Checked = false;
			this.colorButton_Collections.ContextMenuStrip = this.buttonContextMenu;
			this.colorButton_Collections.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.colorButton_Collections.Location = new System.Drawing.Point(191, 100);
			this.colorButton_Collections.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.colorButton_Collections.Name = "colorButton_Collections";
			this.colorButton_Collections.Size = new System.Drawing.Size(170, 25);
			this.colorButton_Collections.TabIndex = 9;
			this.colorButton_Collections.UseForeColor = true;
			this.colorButton_Collections.Click += new System.EventHandler(this.button_Color_Click);
			// 
			// colorButton_Constants
			// 
			this.colorButton_Constants.BackColor = System.Drawing.Color.Orchid;
			this.colorButton_Constants.BackColorUseGeneric = false;
			this.colorButton_Constants.Checked = false;
			this.colorButton_Constants.ContextMenuStrip = this.buttonContextMenu;
			this.colorButton_Constants.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.colorButton_Constants.Location = new System.Drawing.Point(12, 141);
			this.colorButton_Constants.Margin = new System.Windows.Forms.Padding(9, 0, 3, 8);
			this.colorButton_Constants.Name = "colorButton_Constants";
			this.colorButton_Constants.Size = new System.Drawing.Size(170, 25);
			this.colorButton_Constants.TabIndex = 5;
			this.colorButton_Constants.UseForeColor = true;
			this.colorButton_Constants.Click += new System.EventHandler(this.button_Color_Click);
			// 
			// colorButton_Values
			// 
			this.colorButton_Values.BackColor = System.Drawing.Color.SteelBlue;
			this.colorButton_Values.BackColorUseGeneric = false;
			this.colorButton_Values.Checked = false;
			this.colorButton_Values.ContextMenuStrip = this.buttonContextMenu;
			this.colorButton_Values.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.colorButton_Values.Location = new System.Drawing.Point(12, 59);
			this.colorButton_Values.Margin = new System.Windows.Forms.Padding(9, 0, 3, 3);
			this.colorButton_Values.Name = "colorButton_Values";
			this.colorButton_Values.Size = new System.Drawing.Size(170, 25);
			this.colorButton_Values.TabIndex = 1;
			this.colorButton_Values.UseForeColor = true;
			this.colorButton_Values.Click += new System.EventHandler(this.button_Color_Click);
			// 
			// colorButton_Properties
			// 
			this.colorButton_Properties.BackColor = System.Drawing.Color.MediumAquamarine;
			this.colorButton_Properties.BackColorUseGeneric = false;
			this.colorButton_Properties.Checked = false;
			this.colorButton_Properties.ContextMenuStrip = this.buttonContextMenu;
			this.colorButton_Properties.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.colorButton_Properties.Location = new System.Drawing.Point(191, 59);
			this.colorButton_Properties.Margin = new System.Windows.Forms.Padding(6, 0, 3, 3);
			this.colorButton_Properties.Name = "colorButton_Properties";
			this.colorButton_Properties.Size = new System.Drawing.Size(170, 25);
			this.colorButton_Properties.TabIndex = 7;
			this.colorButton_Properties.UseForeColor = true;
			this.colorButton_Properties.Click += new System.EventHandler(this.button_Color_Click);
			// 
			// colorButton_Strings
			// 
			this.colorButton_Strings.BackColor = System.Drawing.Color.LightSalmon;
			this.colorButton_Strings.BackColorUseGeneric = false;
			this.colorButton_Strings.Checked = false;
			this.colorButton_Strings.ContextMenuStrip = this.buttonContextMenu;
			this.colorButton_Strings.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.colorButton_Strings.Location = new System.Drawing.Point(12, 100);
			this.colorButton_Strings.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.colorButton_Strings.Name = "colorButton_Strings";
			this.colorButton_Strings.Size = new System.Drawing.Size(170, 25);
			this.colorButton_Strings.TabIndex = 3;
			this.colorButton_Strings.UseForeColor = true;
			this.colorButton_Strings.Click += new System.EventHandler(this.button_Color_Click);
			// 
			// colorDialog
			// 
			this.colorDialog.AnyColor = true;
			this.colorDialog.FullOpen = true;
			// 
			// darkLabel1
			// 
			this.darkLabel1.AutoSize = true;
			this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel1.Location = new System.Drawing.Point(7, 34);
			this.darkLabel1.Margin = new System.Windows.Forms.Padding(6, 9, 3, 0);
			this.darkLabel1.Name = "darkLabel1";
			this.darkLabel1.Size = new System.Drawing.Size(56, 13);
			this.darkLabel1.TabIndex = 0;
			this.darkLabel1.Text = "Font size:";
			// 
			// darkLabel10
			// 
			this.darkLabel10.AutoSize = true;
			this.darkLabel10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel10.Location = new System.Drawing.Point(370, 46);
			this.darkLabel10.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.darkLabel10.Name = "darkLabel10";
			this.darkLabel10.Size = new System.Drawing.Size(72, 13);
			this.darkLabel10.TabIndex = 13;
			this.darkLabel10.Text = "Background:";
			// 
			// darkLabel11
			// 
			this.darkLabel11.AutoSize = true;
			this.darkLabel11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel11.Location = new System.Drawing.Point(370, 87);
			this.darkLabel11.Name = "darkLabel11";
			this.darkLabel11.Size = new System.Drawing.Size(98, 13);
			this.darkLabel11.TabIndex = 15;
			this.darkLabel11.Text = "Normal text color:";
			// 
			// darkLabel12
			// 
			this.darkLabel12.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.darkLabel12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel12.Location = new System.Drawing.Point(475, 17);
			this.darkLabel12.Name = "darkLabel12";
			this.darkLabel12.Size = new System.Drawing.Size(2, 23);
			this.darkLabel12.TabIndex = 20;
			// 
			// darkLabel2
			// 
			this.darkLabel2.AutoSize = true;
			this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel2.Location = new System.Drawing.Point(7, 76);
			this.darkLabel2.Margin = new System.Windows.Forms.Padding(6, 3, 3, 0);
			this.darkLabel2.Name = "darkLabel2";
			this.darkLabel2.Size = new System.Drawing.Size(67, 13);
			this.darkLabel2.TabIndex = 2;
			this.darkLabel2.Text = "Font family:";
			// 
			// darkLabel3
			// 
			this.darkLabel3.AutoSize = true;
			this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel3.Location = new System.Drawing.Point(7, 119);
			this.darkLabel3.Margin = new System.Windows.Forms.Padding(6, 3, 3, 0);
			this.darkLabel3.Name = "darkLabel3";
			this.darkLabel3.Size = new System.Drawing.Size(90, 13);
			this.darkLabel3.TabIndex = 4;
			this.darkLabel3.Text = "Undo stack size:";
			// 
			// darkLabel4
			// 
			this.darkLabel4.AutoSize = true;
			this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel4.Location = new System.Drawing.Point(12, 46);
			this.darkLabel4.Margin = new System.Windows.Forms.Padding(9, 3, 3, 0);
			this.darkLabel4.Name = "darkLabel4";
			this.darkLabel4.Size = new System.Drawing.Size(40, 13);
			this.darkLabel4.TabIndex = 0;
			this.darkLabel4.Text = "Values";
			// 
			// darkLabel5
			// 
			this.darkLabel5.AutoSize = true;
			this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel5.Location = new System.Drawing.Point(12, 87);
			this.darkLabel5.Name = "darkLabel5";
			this.darkLabel5.Size = new System.Drawing.Size(46, 13);
			this.darkLabel5.TabIndex = 2;
			this.darkLabel5.Text = "Strings:";
			// 
			// darkLabel6
			// 
			this.darkLabel6.AutoSize = true;
			this.darkLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel6.Location = new System.Drawing.Point(12, 128);
			this.darkLabel6.Name = "darkLabel6";
			this.darkLabel6.Size = new System.Drawing.Size(62, 13);
			this.darkLabel6.TabIndex = 4;
			this.darkLabel6.Text = "Constants:";
			// 
			// darkLabel7
			// 
			this.darkLabel7.AutoSize = true;
			this.darkLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel7.Location = new System.Drawing.Point(190, 46);
			this.darkLabel7.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.darkLabel7.Name = "darkLabel7";
			this.darkLabel7.Size = new System.Drawing.Size(62, 13);
			this.darkLabel7.TabIndex = 6;
			this.darkLabel7.Text = "Properties:";
			// 
			// darkLabel8
			// 
			this.darkLabel8.AutoSize = true;
			this.darkLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel8.Location = new System.Drawing.Point(190, 87);
			this.darkLabel8.Name = "darkLabel8";
			this.darkLabel8.Size = new System.Drawing.Size(67, 13);
			this.darkLabel8.TabIndex = 8;
			this.darkLabel8.Text = "Collections:";
			// 
			// darkLabel9
			// 
			this.darkLabel9.AutoSize = true;
			this.darkLabel9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel9.Location = new System.Drawing.Point(190, 128);
			this.darkLabel9.Name = "darkLabel9";
			this.darkLabel9.Size = new System.Drawing.Size(64, 13);
			this.darkLabel9.TabIndex = 10;
			this.darkLabel9.Text = "Comments:";
			// 
			// elementHost
			// 
			this.elementHost.Dock = System.Windows.Forms.DockStyle.Fill;
			this.elementHost.Location = new System.Drawing.Point(3, 18);
			this.elementHost.Name = "elementHost";
			this.elementHost.Size = new System.Drawing.Size(545, 170);
			this.elementHost.TabIndex = 0;
			this.elementHost.Child = null;
			// 
			// groupBox_Colors
			// 
			this.groupBox_Colors.Controls.Add(this.button_ImportScheme);
			this.groupBox_Colors.Controls.Add(this.darkLabel12);
			this.groupBox_Colors.Controls.Add(this.button_SaveScheme);
			this.groupBox_Colors.Controls.Add(this.button_DeleteScheme);
			this.groupBox_Colors.Controls.Add(this.button_OpenSchemesFolder);
			this.groupBox_Colors.Controls.Add(this.darkLabel11);
			this.groupBox_Colors.Controls.Add(this.colorButton_Foreground);
			this.groupBox_Colors.Controls.Add(this.darkLabel10);
			this.groupBox_Colors.Controls.Add(this.colorButton_Background);
			this.groupBox_Colors.Controls.Add(this.comboBox_ColorSchemes);
			this.groupBox_Colors.Controls.Add(this.darkLabel9);
			this.groupBox_Colors.Controls.Add(this.darkLabel8);
			this.groupBox_Colors.Controls.Add(this.darkLabel7);
			this.groupBox_Colors.Controls.Add(this.darkLabel6);
			this.groupBox_Colors.Controls.Add(this.darkLabel5);
			this.groupBox_Colors.Controls.Add(this.darkLabel4);
			this.groupBox_Colors.Controls.Add(this.colorButton_Properties);
			this.groupBox_Colors.Controls.Add(this.colorButton_Collections);
			this.groupBox_Colors.Controls.Add(this.colorButton_Values);
			this.groupBox_Colors.Controls.Add(this.colorButton_Strings);
			this.groupBox_Colors.Controls.Add(this.colorButton_Constants);
			this.groupBox_Colors.Controls.Add(this.colorButton_Comments);
			this.groupBox_Colors.Location = new System.Drawing.Point(162, 28);
			this.groupBox_Colors.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
			this.groupBox_Colors.Name = "groupBox_Colors";
			this.groupBox_Colors.Size = new System.Drawing.Size(551, 177);
			this.groupBox_Colors.TabIndex = 17;
			this.groupBox_Colors.TabStop = false;
			this.groupBox_Colors.Text = "Color schemes";
			// 
			// button_SaveScheme
			// 
			this.button_SaveScheme.Checked = false;
			this.button_SaveScheme.Image = global::TombIDE.ScriptingStudio.Properties.Resources.Save_16;
			this.button_SaveScheme.Location = new System.Drawing.Point(413, 16);
			this.button_SaveScheme.Name = "button_SaveScheme";
			this.button_SaveScheme.Size = new System.Drawing.Size(25, 25);
			this.button_SaveScheme.TabIndex = 19;
			this.toolTip.SetToolTip(this.button_SaveScheme, "Save Scheme As...");
			this.button_SaveScheme.Click += new System.EventHandler(this.button_SaveScheme_Click);
			// 
			// button_DeleteScheme
			// 
			this.button_DeleteScheme.Checked = false;
			this.button_DeleteScheme.Image = global::TombIDE.ScriptingStudio.Properties.Resources.Trash_16;
			this.button_DeleteScheme.Location = new System.Drawing.Point(444, 16);
			this.button_DeleteScheme.Name = "button_DeleteScheme";
			this.button_DeleteScheme.Size = new System.Drawing.Size(25, 25);
			this.button_DeleteScheme.TabIndex = 18;
			this.toolTip.SetToolTip(this.button_DeleteScheme, "Delete Scheme");
			this.button_DeleteScheme.Click += new System.EventHandler(this.button_DeleteScheme_Click);
			// 
			// button_OpenSchemesFolder
			// 
			this.button_OpenSchemesFolder.Checked = false;
			this.button_OpenSchemesFolder.Image = global::TombIDE.ScriptingStudio.Properties.Resources.ForwardArrow_16;
			this.button_OpenSchemesFolder.Location = new System.Drawing.Point(514, 16);
			this.button_OpenSchemesFolder.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.button_OpenSchemesFolder.Name = "button_OpenSchemesFolder";
			this.button_OpenSchemesFolder.Size = new System.Drawing.Size(25, 25);
			this.button_OpenSchemesFolder.TabIndex = 17;
			this.toolTip.SetToolTip(this.button_OpenSchemesFolder, "Open Schemes Folder");
			this.button_OpenSchemesFolder.Click += new System.EventHandler(this.button_OpenSchemesFolder_Click);
			// 
			// comboBox_ColorSchemes
			// 
			this.comboBox_ColorSchemes.FormattingEnabled = true;
			this.comboBox_ColorSchemes.Location = new System.Drawing.Point(12, 19);
			this.comboBox_ColorSchemes.Margin = new System.Windows.Forms.Padding(9, 3, 3, 3);
			this.comboBox_ColorSchemes.Name = "comboBox_ColorSchemes";
			this.comboBox_ColorSchemes.Size = new System.Drawing.Size(395, 23);
			this.comboBox_ColorSchemes.TabIndex = 12;
			this.comboBox_ColorSchemes.SelectedIndexChanged += new System.EventHandler(this.comboBox_ColorSchemes_SelectedIndexChanged);
			// 
			// groupBox_Preview
			// 
			this.groupBox_Preview.Controls.Add(this.elementHost);
			this.groupBox_Preview.Location = new System.Drawing.Point(162, 214);
			this.groupBox_Preview.Margin = new System.Windows.Forms.Padding(3, 6, 6, 6);
			this.groupBox_Preview.Name = "groupBox_Preview";
			this.groupBox_Preview.Size = new System.Drawing.Size(551, 191);
			this.groupBox_Preview.TabIndex = 2;
			this.groupBox_Preview.TabStop = false;
			this.groupBox_Preview.Text = "Preview";
			// 
			// numeric_FontSize
			// 
			this.numeric_FontSize.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
			this.numeric_FontSize.Location = new System.Drawing.Point(6, 50);
			this.numeric_FontSize.LoopValues = false;
			this.numeric_FontSize.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.numeric_FontSize.Maximum = new decimal(new int[] {
            32,
            0,
            0,
            0});
			this.numeric_FontSize.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
			this.numeric_FontSize.Name = "numeric_FontSize";
			this.numeric_FontSize.Size = new System.Drawing.Size(150, 22);
			this.numeric_FontSize.TabIndex = 1;
			this.numeric_FontSize.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
			this.numeric_FontSize.ValueChanged += new System.EventHandler(this.VisiblePreviewSetting_Changed);
			// 
			// numeric_UndoStackSize
			// 
			this.numeric_UndoStackSize.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
			this.numeric_UndoStackSize.Location = new System.Drawing.Point(6, 135);
			this.numeric_UndoStackSize.LoopValues = false;
			this.numeric_UndoStackSize.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.numeric_UndoStackSize.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
			this.numeric_UndoStackSize.Minimum = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.numeric_UndoStackSize.Name = "numeric_UndoStackSize";
			this.numeric_UndoStackSize.Size = new System.Drawing.Size(150, 22);
			this.numeric_UndoStackSize.TabIndex = 5;
			this.numeric_UndoStackSize.Value = new decimal(new int[] {
            256,
            0,
            0,
            0});
			// 
			// sectionPanel
			// 
			this.sectionPanel.Controls.Add(this.checkBox_AutoAddCommas);
			this.sectionPanel.Controls.Add(this.checkBox_CloseBraces);
			this.sectionPanel.Controls.Add(this.checkBox_CloseQuotes);
			this.sectionPanel.Controls.Add(this.checkBox_CloseBrackets);
			this.sectionPanel.Controls.Add(this.groupBox_Preview);
			this.sectionPanel.Controls.Add(this.checkBox_HighlightCurrentLine);
			this.sectionPanel.Controls.Add(this.checkBox_VisibleTabs);
			this.sectionPanel.Controls.Add(this.checkBox_VisibleSpaces);
			this.sectionPanel.Controls.Add(this.checkBox_LineNumbers);
			this.sectionPanel.Controls.Add(this.darkLabel3);
			this.sectionPanel.Controls.Add(this.darkLabel2);
			this.sectionPanel.Controls.Add(this.darkLabel1);
			this.sectionPanel.Controls.Add(this.numeric_UndoStackSize);
			this.sectionPanel.Controls.Add(this.checkBox_Autocomplete);
			this.sectionPanel.Controls.Add(this.checkBox_WordWrapping);
			this.sectionPanel.Controls.Add(this.groupBox_Colors);
			this.sectionPanel.Controls.Add(this.comboBox_FontFamily);
			this.sectionPanel.Controls.Add(this.numeric_FontSize);
			this.sectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sectionPanel.Location = new System.Drawing.Point(0, 0);
			this.sectionPanel.Name = "sectionPanel";
			this.sectionPanel.SectionHeader = "TR1X / TR2X Script";
			this.sectionPanel.Size = new System.Drawing.Size(720, 412);
			this.sectionPanel.TabIndex = 0;
			// 
			// checkBox_CloseBraces
			// 
			this.checkBox_CloseBraces.AutoSize = true;
			this.checkBox_CloseBraces.Location = new System.Drawing.Point(6, 258);
			this.checkBox_CloseBraces.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.checkBox_CloseBraces.Name = "checkBox_CloseBraces";
			this.checkBox_CloseBraces.Size = new System.Drawing.Size(128, 17);
			this.checkBox_CloseBraces.TabIndex = 20;
			this.checkBox_CloseBraces.Text = "Auto close braces { }";
			// 
			// checkBox_CloseQuotes
			// 
			this.checkBox_CloseQuotes.AutoSize = true;
			this.checkBox_CloseQuotes.Location = new System.Drawing.Point(6, 235);
			this.checkBox_CloseQuotes.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.checkBox_CloseQuotes.Name = "checkBox_CloseQuotes";
			this.checkBox_CloseQuotes.Size = new System.Drawing.Size(133, 17);
			this.checkBox_CloseQuotes.TabIndex = 19;
			this.checkBox_CloseQuotes.Text = "Auto close quotes \" \"";
			// 
			// checkBox_CloseBrackets
			// 
			this.checkBox_CloseBrackets.AutoSize = true;
			this.checkBox_CloseBrackets.Location = new System.Drawing.Point(6, 212);
			this.checkBox_CloseBrackets.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
			this.checkBox_CloseBrackets.Name = "checkBox_CloseBrackets";
			this.checkBox_CloseBrackets.Size = new System.Drawing.Size(138, 17);
			this.checkBox_CloseBrackets.TabIndex = 18;
			this.checkBox_CloseBrackets.Text = "Auto close brackets [ ]";
			// 
			// comboBox_FontFamily
			// 
			this.comboBox_FontFamily.FormattingEnabled = true;
			this.comboBox_FontFamily.Location = new System.Drawing.Point(6, 92);
			this.comboBox_FontFamily.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.comboBox_FontFamily.Name = "comboBox_FontFamily";
			this.comboBox_FontFamily.Size = new System.Drawing.Size(150, 23);
			this.comboBox_FontFamily.TabIndex = 3;
			this.comboBox_FontFamily.SelectedIndexChanged += new System.EventHandler(this.comboBox_FontFamily_SelectedIndexChanged);
			// 
			// checkBox_AutoAddCommas
			// 
			this.checkBox_AutoAddCommas.AutoSize = true;
			this.checkBox_AutoAddCommas.Location = new System.Drawing.Point(6, 281);
			this.checkBox_AutoAddCommas.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.checkBox_AutoAddCommas.Name = "checkBox_AutoAddCommas";
			this.checkBox_AutoAddCommas.Size = new System.Drawing.Size(130, 17);
			this.checkBox_AutoAddCommas.TabIndex = 21;
			this.checkBox_AutoAddCommas.Text = "Auto add commas \',\'";
			// 
			// Tomb1MainSettingsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(65)))), ((int)(((byte)(69)))));
			this.Controls.Add(this.sectionPanel);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MaximumSize = new System.Drawing.Size(720, 412);
			this.MinimumSize = new System.Drawing.Size(720, 412);
			this.Name = "Tomb1MainSettingsControl";
			this.Size = new System.Drawing.Size(720, 412);
			this.buttonContextMenu.ResumeLayout(false);
			this.groupBox_Colors.ResumeLayout(false);
			this.groupBox_Colors.PerformLayout();
			this.groupBox_Preview.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numeric_FontSize)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numeric_UndoStackSize)).EndInit();
			this.sectionPanel.ResumeLayout(false);
			this.sectionPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_DeleteScheme;
		private DarkUI.Controls.DarkButton button_ImportScheme;
		private DarkUI.Controls.DarkButton button_OpenSchemesFolder;
		private DarkUI.Controls.DarkButton button_SaveScheme;
		private DarkUI.Controls.DarkButton colorButton_Background;
		private DarkUI.Controls.DarkButton colorButton_Comments;
		private DarkUI.Controls.DarkButton colorButton_Foreground;
		private DarkUI.Controls.DarkButton colorButton_Collections;
		private DarkUI.Controls.DarkButton colorButton_Constants;
		private DarkUI.Controls.DarkButton colorButton_Values;
		private DarkUI.Controls.DarkButton colorButton_Properties;
		private DarkUI.Controls.DarkButton colorButton_Strings;
		private DarkUI.Controls.DarkCheckBox checkBox_Autocomplete;
		private DarkUI.Controls.DarkCheckBox checkBox_HighlightCurrentLine;
		private DarkUI.Controls.DarkCheckBox checkBox_LineNumbers;
		private DarkUI.Controls.DarkCheckBox checkBox_VisibleSpaces;
		private DarkUI.Controls.DarkCheckBox checkBox_VisibleTabs;
		private DarkUI.Controls.DarkCheckBox checkBox_WordWrapping;
		private DarkUI.Controls.DarkComboBox comboBox_ColorSchemes;
		private DarkUI.Controls.DarkComboBox comboBox_FontFamily;
		private DarkUI.Controls.DarkContextMenu buttonContextMenu;
		private DarkUI.Controls.DarkGroupBox groupBox_Colors;
		private DarkUI.Controls.DarkGroupBox groupBox_Preview;
		private DarkUI.Controls.DarkLabel darkLabel1;
		private DarkUI.Controls.DarkLabel darkLabel10;
		private DarkUI.Controls.DarkLabel darkLabel11;
		private DarkUI.Controls.DarkLabel darkLabel12;
		private DarkUI.Controls.DarkLabel darkLabel2;
		private DarkUI.Controls.DarkLabel darkLabel3;
		private DarkUI.Controls.DarkLabel darkLabel4;
		private DarkUI.Controls.DarkLabel darkLabel5;
		private DarkUI.Controls.DarkLabel darkLabel6;
		private DarkUI.Controls.DarkLabel darkLabel7;
		private DarkUI.Controls.DarkLabel darkLabel8;
		private DarkUI.Controls.DarkLabel darkLabel9;
		private DarkUI.Controls.DarkNumericUpDown numeric_FontSize;
		private DarkUI.Controls.DarkNumericUpDown numeric_UndoStackSize;
		private DarkUI.Controls.DarkSectionPanel sectionPanel;
		private System.Windows.Forms.ColorDialog colorDialog;
		private System.Windows.Forms.Integration.ElementHost elementHost;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Bold;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Italic;
		private System.Windows.Forms.ToolTip toolTip;
		private DarkUI.Controls.DarkCheckBox checkBox_CloseBraces;
		private DarkUI.Controls.DarkCheckBox checkBox_CloseQuotes;
		private DarkUI.Controls.DarkCheckBox checkBox_CloseBrackets;
		private DarkUI.Controls.DarkCheckBox checkBox_AutoAddCommas;
	}
}
