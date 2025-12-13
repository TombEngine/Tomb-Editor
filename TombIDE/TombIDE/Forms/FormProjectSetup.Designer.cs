namespace TombIDE
{
	partial class FormProjectSetup
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

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			button_BrowseLevels = new DarkUI.Controls.DarkButton();
			button_BrowseScript = new DarkUI.Controls.DarkButton();
			button_Create = new DarkUI.Controls.DarkButton();
			panel_LevelsRadioChoice = new System.Windows.Forms.Panel();
			radio_Levels_01 = new DarkUI.Controls.DarkRadioButton();
			radio_Levels_02 = new DarkUI.Controls.DarkRadioButton();
			panel_ScriptRadioChoice = new System.Windows.Forms.Panel();
			radio_Script_01 = new DarkUI.Controls.DarkRadioButton();
			radio_Script_02 = new DarkUI.Controls.DarkRadioButton();
			textBox_LevelsPath = new DarkUI.Controls.DarkTextBox();
			textBox_ScriptPath = new DarkUI.Controls.DarkTextBox();
			progressBar = new DarkUI.Controls.DarkProgressBar();
			tabPage_Extra = new System.Windows.Forms.TabPage();
			tableLayoutPanel_Main02 = new System.Windows.Forms.TableLayoutPanel();
			tableLayoutPanel_Buttons02 = new System.Windows.Forms.TableLayoutPanel();
			button_Back = new DarkUI.Controls.DarkButton();
			tableLayoutPanel_Content02 = new System.Windows.Forms.TableLayoutPanel();
			tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			label_SubTitle02 = new DarkUI.Controls.DarkLabel();
			label_Title02 = new DarkUI.Controls.DarkLabel();
			tabPage_Basic = new System.Windows.Forms.TabPage();
			tableLayoutPanel_Main01 = new System.Windows.Forms.TableLayoutPanel();
			tableLayoutPanel_Content01 = new System.Windows.Forms.TableLayoutPanel();
			button_BrowseProject = new DarkUI.Controls.DarkButton();
			button_Help = new DarkUI.Controls.DarkButton();
			textBox_ProjectPath = new DarkUI.Controls.DarkTextBox();
			comboBox_EngineType = new DarkUI.Controls.DarkComboBox();
			label_01 = new DarkUI.Controls.DarkLabel();
			label_02 = new DarkUI.Controls.DarkLabel();
			textBox_ProjectName = new DarkUI.Controls.DarkTextBox();
			label_03 = new DarkUI.Controls.DarkLabel();
			checkBox_IncludeFLEP = new DarkUI.Controls.DarkCheckBox();
			tableLayoutPanel_Title = new System.Windows.Forms.TableLayoutPanel();
			label_SubTitle01 = new DarkUI.Controls.DarkLabel();
			label_Title01 = new DarkUI.Controls.DarkLabel();
			tableLayoutPanel_Buttons01 = new System.Windows.Forms.TableLayoutPanel();
			button_Next = new DarkUI.Controls.DarkButton();
			button_Cancel = new DarkUI.Controls.DarkButton();
			tablessTabControl = new TombLib.Controls.DarkTabbedContainer();
			panel_LevelsRadioChoice.SuspendLayout();
			panel_ScriptRadioChoice.SuspendLayout();
			tabPage_Extra.SuspendLayout();
			tableLayoutPanel_Main02.SuspendLayout();
			tableLayoutPanel_Buttons02.SuspendLayout();
			tableLayoutPanel_Content02.SuspendLayout();
			tableLayoutPanel3.SuspendLayout();
			tabPage_Basic.SuspendLayout();
			tableLayoutPanel_Main01.SuspendLayout();
			tableLayoutPanel_Content01.SuspendLayout();
			tableLayoutPanel_Title.SuspendLayout();
			tableLayoutPanel_Buttons01.SuspendLayout();
			tablessTabControl.SuspendLayout();
			SuspendLayout();
			// 
			// button_BrowseLevels
			// 
			button_BrowseLevels.Checked = false;
			button_BrowseLevels.Dock = System.Windows.Forms.DockStyle.Fill;
			button_BrowseLevels.Enabled = false;
			button_BrowseLevels.Location = new System.Drawing.Point(425, 190);
			button_BrowseLevels.Margin = new System.Windows.Forms.Padding(5, 0, 0, 5);
			button_BrowseLevels.Name = "button_BrowseLevels";
			button_BrowseLevels.Size = new System.Drawing.Size(95, 25);
			button_BrowseLevels.TabIndex = 14;
			button_BrowseLevels.Text = "Browse...";
			button_BrowseLevels.Click += button_BrowseLevels_Click;
			// 
			// button_BrowseScript
			// 
			button_BrowseScript.Checked = false;
			button_BrowseScript.Dock = System.Windows.Forms.DockStyle.Fill;
			button_BrowseScript.Enabled = false;
			button_BrowseScript.Location = new System.Drawing.Point(425, 80);
			button_BrowseScript.Margin = new System.Windows.Forms.Padding(5, 0, 0, 5);
			button_BrowseScript.Name = "button_BrowseScript";
			button_BrowseScript.Size = new System.Drawing.Size(95, 25);
			button_BrowseScript.TabIndex = 10;
			button_BrowseScript.Text = "Browse...";
			button_BrowseScript.Click += button_BrowseScript_Click;
			// 
			// button_Create
			// 
			button_Create.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			button_Create.Checked = false;
			button_Create.DialogResult = System.Windows.Forms.DialogResult.OK;
			button_Create.Location = new System.Drawing.Point(121, 350);
			button_Create.Margin = new System.Windows.Forms.Padding(0);
			button_Create.Name = "button_Create";
			button_Create.Size = new System.Drawing.Size(75, 25);
			button_Create.TabIndex = 1;
			button_Create.Text = "Create";
			button_Create.Click += button_Create_Click;
			// 
			// panel_LevelsRadioChoice
			// 
			panel_LevelsRadioChoice.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			panel_LevelsRadioChoice.Controls.Add(radio_Levels_01);
			panel_LevelsRadioChoice.Controls.Add(radio_Levels_02);
			panel_LevelsRadioChoice.Location = new System.Drawing.Point(0, 140);
			panel_LevelsRadioChoice.Margin = new System.Windows.Forms.Padding(0);
			panel_LevelsRadioChoice.Name = "panel_LevelsRadioChoice";
			panel_LevelsRadioChoice.Size = new System.Drawing.Size(420, 50);
			panel_LevelsRadioChoice.TabIndex = 15;
			// 
			// radio_Levels_01
			// 
			radio_Levels_01.Checked = true;
			radio_Levels_01.Location = new System.Drawing.Point(3, 3);
			radio_Levels_01.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			radio_Levels_01.Name = "radio_Levels_01";
			radio_Levels_01.Size = new System.Drawing.Size(348, 20);
			radio_Levels_01.TabIndex = 11;
			radio_Levels_01.TabStop = true;
			radio_Levels_01.Text = "Store newly created Levels inside the project folder (Default)";
			radio_Levels_01.CheckedChanged += radio_Levels_01_CheckedChanged;
			// 
			// radio_Levels_02
			// 
			radio_Levels_02.Location = new System.Drawing.Point(3, 27);
			radio_Levels_02.Name = "radio_Levels_02";
			radio_Levels_02.Size = new System.Drawing.Size(348, 20);
			radio_Levels_02.TabIndex = 12;
			radio_Levels_02.Text = "Use a different \"Levels\" location";
			radio_Levels_02.CheckedChanged += radio_Level_02_CheckedChanged;
			// 
			// panel_ScriptRadioChoice
			// 
			panel_ScriptRadioChoice.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			panel_ScriptRadioChoice.Controls.Add(radio_Script_01);
			panel_ScriptRadioChoice.Controls.Add(radio_Script_02);
			panel_ScriptRadioChoice.Location = new System.Drawing.Point(0, 30);
			panel_ScriptRadioChoice.Margin = new System.Windows.Forms.Padding(0);
			panel_ScriptRadioChoice.Name = "panel_ScriptRadioChoice";
			panel_ScriptRadioChoice.Size = new System.Drawing.Size(420, 50);
			panel_ScriptRadioChoice.TabIndex = 16;
			// 
			// radio_Script_01
			// 
			radio_Script_01.Checked = true;
			radio_Script_01.Location = new System.Drawing.Point(3, 3);
			radio_Script_01.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			radio_Script_01.Name = "radio_Script_01";
			radio_Script_01.Size = new System.Drawing.Size(348, 20);
			radio_Script_01.TabIndex = 7;
			radio_Script_01.TabStop = true;
			radio_Script_01.Text = "Create a \"Script\" folder inside the project folder (Default)";
			radio_Script_01.CheckedChanged += radio_Script_01_CheckedChanged;
			// 
			// radio_Script_02
			// 
			radio_Script_02.Location = new System.Drawing.Point(3, 27);
			radio_Script_02.Name = "radio_Script_02";
			radio_Script_02.Size = new System.Drawing.Size(348, 20);
			radio_Script_02.TabIndex = 8;
			radio_Script_02.Text = "Use a different \"Script\" location";
			radio_Script_02.CheckedChanged += radio_Script_02_CheckedChanged;
			// 
			// textBox_LevelsPath
			// 
			textBox_LevelsPath.Dock = System.Windows.Forms.DockStyle.Fill;
			textBox_LevelsPath.Enabled = false;
			textBox_LevelsPath.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			textBox_LevelsPath.Location = new System.Drawing.Point(0, 190);
			textBox_LevelsPath.Margin = new System.Windows.Forms.Padding(0);
			textBox_LevelsPath.Name = "textBox_LevelsPath";
			textBox_LevelsPath.Size = new System.Drawing.Size(420, 25);
			textBox_LevelsPath.TabIndex = 13;
			// 
			// textBox_ScriptPath
			// 
			textBox_ScriptPath.Dock = System.Windows.Forms.DockStyle.Fill;
			textBox_ScriptPath.Enabled = false;
			textBox_ScriptPath.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			textBox_ScriptPath.Location = new System.Drawing.Point(0, 80);
			textBox_ScriptPath.Margin = new System.Windows.Forms.Padding(0);
			textBox_ScriptPath.Name = "textBox_ScriptPath";
			textBox_ScriptPath.Size = new System.Drawing.Size(420, 25);
			textBox_ScriptPath.TabIndex = 9;
			// 
			// progressBar
			// 
			progressBar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tableLayoutPanel_Content02.SetColumnSpan(progressBar, 2);
			progressBar.Location = new System.Drawing.Point(0, 350);
			progressBar.Margin = new System.Windows.Forms.Padding(0);
			progressBar.Name = "progressBar";
			progressBar.Size = new System.Drawing.Size(520, 25);
			progressBar.TabIndex = 2;
			// 
			// tabPage_Extra
			// 
			tabPage_Extra.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabPage_Extra.Controls.Add(tableLayoutPanel_Main02);
			tabPage_Extra.Location = new System.Drawing.Point(4, 22);
			tabPage_Extra.Name = "tabPage_Extra";
			tabPage_Extra.Size = new System.Drawing.Size(776, 485);
			tabPage_Extra.TabIndex = 1;
			tabPage_Extra.Text = "Extra Options";
			// 
			// tableLayoutPanel_Main02
			// 
			tableLayoutPanel_Main02.ColumnCount = 2;
			tableLayoutPanel_Main02.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 520F));
			tableLayoutPanel_Main02.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel_Main02.Controls.Add(tableLayoutPanel_Buttons02, 1, 1);
			tableLayoutPanel_Main02.Controls.Add(tableLayoutPanel_Content02, 0, 1);
			tableLayoutPanel_Main02.Controls.Add(tableLayoutPanel3, 0, 0);
			tableLayoutPanel_Main02.Dock = System.Windows.Forms.DockStyle.Fill;
			tableLayoutPanel_Main02.Location = new System.Drawing.Point(0, 0);
			tableLayoutPanel_Main02.Margin = new System.Windows.Forms.Padding(0);
			tableLayoutPanel_Main02.Name = "tableLayoutPanel_Main02";
			tableLayoutPanel_Main02.Padding = new System.Windows.Forms.Padding(30);
			tableLayoutPanel_Main02.RowCount = 2;
			tableLayoutPanel_Main02.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			tableLayoutPanel_Main02.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel_Main02.Size = new System.Drawing.Size(776, 485);
			tableLayoutPanel_Main02.TabIndex = 19;
			// 
			// tableLayoutPanel_Buttons02
			// 
			tableLayoutPanel_Buttons02.ColumnCount = 2;
			tableLayoutPanel_Buttons02.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel_Buttons02.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			tableLayoutPanel_Buttons02.Controls.Add(button_Back, 0, 0);
			tableLayoutPanel_Buttons02.Controls.Add(button_Create, 1, 0);
			tableLayoutPanel_Buttons02.Dock = System.Windows.Forms.DockStyle.Fill;
			tableLayoutPanel_Buttons02.Location = new System.Drawing.Point(550, 80);
			tableLayoutPanel_Buttons02.Margin = new System.Windows.Forms.Padding(0);
			tableLayoutPanel_Buttons02.Name = "tableLayoutPanel_Buttons02";
			tableLayoutPanel_Buttons02.RowCount = 1;
			tableLayoutPanel_Buttons02.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel_Buttons02.Size = new System.Drawing.Size(196, 375);
			tableLayoutPanel_Buttons02.TabIndex = 4;
			// 
			// button_Back
			// 
			button_Back.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			button_Back.Checked = false;
			button_Back.Location = new System.Drawing.Point(41, 350);
			button_Back.Margin = new System.Windows.Forms.Padding(0);
			button_Back.Name = "button_Back";
			button_Back.Size = new System.Drawing.Size(75, 25);
			button_Back.TabIndex = 11;
			button_Back.Text = "Back";
			button_Back.Click += button_Back_Click;
			// 
			// tableLayoutPanel_Content02
			// 
			tableLayoutPanel_Content02.ColumnCount = 2;
			tableLayoutPanel_Content02.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel_Content02.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			tableLayoutPanel_Content02.Controls.Add(panel_LevelsRadioChoice, 0, 2);
			tableLayoutPanel_Content02.Controls.Add(progressBar, 0, 4);
			tableLayoutPanel_Content02.Controls.Add(panel_ScriptRadioChoice, 0, 0);
			tableLayoutPanel_Content02.Controls.Add(button_BrowseLevels, 1, 3);
			tableLayoutPanel_Content02.Controls.Add(textBox_LevelsPath, 0, 3);
			tableLayoutPanel_Content02.Controls.Add(textBox_ScriptPath, 0, 1);
			tableLayoutPanel_Content02.Controls.Add(button_BrowseScript, 1, 1);
			tableLayoutPanel_Content02.Dock = System.Windows.Forms.DockStyle.Fill;
			tableLayoutPanel_Content02.Location = new System.Drawing.Point(30, 80);
			tableLayoutPanel_Content02.Margin = new System.Windows.Forms.Padding(0);
			tableLayoutPanel_Content02.Name = "tableLayoutPanel_Content02";
			tableLayoutPanel_Content02.RowCount = 5;
			tableLayoutPanel_Content02.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			tableLayoutPanel_Content02.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			tableLayoutPanel_Content02.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			tableLayoutPanel_Content02.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			tableLayoutPanel_Content02.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel_Content02.Size = new System.Drawing.Size(520, 375);
			tableLayoutPanel_Content02.TabIndex = 0;
			// 
			// tableLayoutPanel3
			// 
			tableLayoutPanel3.ColumnCount = 1;
			tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel3.Controls.Add(label_SubTitle02, 0, 1);
			tableLayoutPanel3.Controls.Add(label_Title02, 0, 0);
			tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			tableLayoutPanel3.Location = new System.Drawing.Point(30, 30);
			tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
			tableLayoutPanel3.Name = "tableLayoutPanel3";
			tableLayoutPanel3.RowCount = 2;
			tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel3.Size = new System.Drawing.Size(520, 50);
			tableLayoutPanel3.TabIndex = 2;
			// 
			// label_SubTitle02
			// 
			label_SubTitle02.Dock = System.Windows.Forms.DockStyle.Fill;
			label_SubTitle02.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label_SubTitle02.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			label_SubTitle02.Location = new System.Drawing.Point(2, 30);
			label_SubTitle02.Margin = new System.Windows.Forms.Padding(2, 0, 0, 0);
			label_SubTitle02.Name = "label_SubTitle02";
			label_SubTitle02.Size = new System.Drawing.Size(518, 20);
			label_SubTitle02.TabIndex = 2;
			label_SubTitle02.Text = "Extra Options";
			label_SubTitle02.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label_Title02
			// 
			label_Title02.Dock = System.Windows.Forms.DockStyle.Fill;
			label_Title02.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label_Title02.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			label_Title02.Location = new System.Drawing.Point(0, 0);
			label_Title02.Margin = new System.Windows.Forms.Padding(0);
			label_Title02.Name = "label_Title02";
			label_Title02.Size = new System.Drawing.Size(520, 30);
			label_Title02.TabIndex = 1;
			label_Title02.Text = "Create a new project";
			// 
			// tabPage_Basic
			// 
			tabPage_Basic.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabPage_Basic.Controls.Add(tableLayoutPanel_Main01);
			tabPage_Basic.Location = new System.Drawing.Point(4, 22);
			tabPage_Basic.Name = "tabPage_Basic";
			tabPage_Basic.Size = new System.Drawing.Size(776, 485);
			tabPage_Basic.TabIndex = 0;
			tabPage_Basic.Text = "Basic Info";
			// 
			// tableLayoutPanel_Main01
			// 
			tableLayoutPanel_Main01.ColumnCount = 2;
			tableLayoutPanel_Main01.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 520F));
			tableLayoutPanel_Main01.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel_Main01.Controls.Add(tableLayoutPanel_Content01, 0, 1);
			tableLayoutPanel_Main01.Controls.Add(tableLayoutPanel_Title, 0, 0);
			tableLayoutPanel_Main01.Controls.Add(tableLayoutPanel_Buttons01, 1, 1);
			tableLayoutPanel_Main01.Dock = System.Windows.Forms.DockStyle.Fill;
			tableLayoutPanel_Main01.Location = new System.Drawing.Point(0, 0);
			tableLayoutPanel_Main01.Margin = new System.Windows.Forms.Padding(0);
			tableLayoutPanel_Main01.Name = "tableLayoutPanel_Main01";
			tableLayoutPanel_Main01.Padding = new System.Windows.Forms.Padding(30);
			tableLayoutPanel_Main01.RowCount = 2;
			tableLayoutPanel_Main01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			tableLayoutPanel_Main01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel_Main01.Size = new System.Drawing.Size(776, 485);
			tableLayoutPanel_Main01.TabIndex = 18;
			// 
			// tableLayoutPanel_Content01
			// 
			tableLayoutPanel_Content01.ColumnCount = 2;
			tableLayoutPanel_Content01.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel_Content01.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			tableLayoutPanel_Content01.Controls.Add(button_BrowseProject, 1, 3);
			tableLayoutPanel_Content01.Controls.Add(button_Help, 1, 5);
			tableLayoutPanel_Content01.Controls.Add(textBox_ProjectPath, 0, 3);
			tableLayoutPanel_Content01.Controls.Add(comboBox_EngineType, 0, 5);
			tableLayoutPanel_Content01.Controls.Add(label_01, 0, 0);
			tableLayoutPanel_Content01.Controls.Add(label_02, 0, 4);
			tableLayoutPanel_Content01.Controls.Add(textBox_ProjectName, 0, 1);
			tableLayoutPanel_Content01.Controls.Add(label_03, 0, 2);
			tableLayoutPanel_Content01.Controls.Add(checkBox_IncludeFLEP, 0, 6);
			tableLayoutPanel_Content01.Dock = System.Windows.Forms.DockStyle.Fill;
			tableLayoutPanel_Content01.Location = new System.Drawing.Point(30, 80);
			tableLayoutPanel_Content01.Margin = new System.Windows.Forms.Padding(0);
			tableLayoutPanel_Content01.Name = "tableLayoutPanel_Content01";
			tableLayoutPanel_Content01.RowCount = 7;
			tableLayoutPanel_Content01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			tableLayoutPanel_Content01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			tableLayoutPanel_Content01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			tableLayoutPanel_Content01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			tableLayoutPanel_Content01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			tableLayoutPanel_Content01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			tableLayoutPanel_Content01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel_Content01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			tableLayoutPanel_Content01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			tableLayoutPanel_Content01.Size = new System.Drawing.Size(520, 375);
			tableLayoutPanel_Content01.TabIndex = 0;
			// 
			// button_BrowseProject
			// 
			button_BrowseProject.Checked = false;
			button_BrowseProject.Dock = System.Windows.Forms.DockStyle.Fill;
			button_BrowseProject.Location = new System.Drawing.Point(425, 130);
			button_BrowseProject.Margin = new System.Windows.Forms.Padding(5, 0, 0, 5);
			button_BrowseProject.Name = "button_BrowseProject";
			button_BrowseProject.Size = new System.Drawing.Size(95, 25);
			button_BrowseProject.TabIndex = 6;
			button_BrowseProject.Text = "Browse...";
			button_BrowseProject.Click += button_BrowseProject_Click;
			// 
			// button_Help
			// 
			button_Help.Checked = false;
			button_Help.Dock = System.Windows.Forms.DockStyle.Fill;
			button_Help.Location = new System.Drawing.Point(425, 210);
			button_Help.Margin = new System.Windows.Forms.Padding(5, 0, 0, 4);
			button_Help.Name = "button_Help";
			button_Help.Size = new System.Drawing.Size(95, 26);
			button_Help.TabIndex = 17;
			button_Help.Text = "Need Help?";
			button_Help.Click += button_Help_Click;
			// 
			// textBox_ProjectPath
			// 
			textBox_ProjectPath.Dock = System.Windows.Forms.DockStyle.Fill;
			textBox_ProjectPath.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			textBox_ProjectPath.Location = new System.Drawing.Point(0, 130);
			textBox_ProjectPath.Margin = new System.Windows.Forms.Padding(0);
			textBox_ProjectPath.Name = "textBox_ProjectPath";
			textBox_ProjectPath.Size = new System.Drawing.Size(420, 25);
			textBox_ProjectPath.TabIndex = 5;
			// 
			// comboBox_EngineType
			// 
			comboBox_EngineType.Dock = System.Windows.Forms.DockStyle.Fill;
			comboBox_EngineType.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			comboBox_EngineType.FormattingEnabled = true;
			comboBox_EngineType.Items.AddRange(new object[] { "- Select -", "Tomb Raider 1 (TR1X)", "Tomb Raider 2 (TR2X)", "Tomb Raider 2 (TR2Main)", "Tomb Raider 3 (tomb3)", "Tomb Raider 4 (Original TRLE)", "Tomb Raider Next-Generation", "Tomb Engine" });
			comboBox_EngineType.Location = new System.Drawing.Point(0, 210);
			comboBox_EngineType.Margin = new System.Windows.Forms.Padding(0);
			comboBox_EngineType.Name = "comboBox_EngineType";
			comboBox_EngineType.Size = new System.Drawing.Size(420, 26);
			comboBox_EngineType.TabIndex = 3;
			comboBox_EngineType.SelectedIndexChanged += comboBox_EngineType_SelectedIndexChanged;
			// 
			// label_01
			// 
			label_01.Dock = System.Windows.Forms.DockStyle.Fill;
			label_01.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			label_01.Location = new System.Drawing.Point(0, 0);
			label_01.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
			label_01.Name = "label_01";
			label_01.Size = new System.Drawing.Size(420, 44);
			label_01.TabIndex = 0;
			label_01.Text = "Project name:";
			label_01.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// label_02
			// 
			label_02.Dock = System.Windows.Forms.DockStyle.Fill;
			label_02.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			label_02.Location = new System.Drawing.Point(0, 160);
			label_02.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
			label_02.Name = "label_02";
			label_02.Size = new System.Drawing.Size(420, 44);
			label_02.TabIndex = 2;
			label_02.Text = "Engine type:";
			label_02.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// textBox_ProjectName
			// 
			tableLayoutPanel_Content01.SetColumnSpan(textBox_ProjectName, 2);
			textBox_ProjectName.Dock = System.Windows.Forms.DockStyle.Fill;
			textBox_ProjectName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			textBox_ProjectName.Location = new System.Drawing.Point(0, 50);
			textBox_ProjectName.Margin = new System.Windows.Forms.Padding(0);
			textBox_ProjectName.Name = "textBox_ProjectName";
			textBox_ProjectName.Size = new System.Drawing.Size(520, 29);
			textBox_ProjectName.TabIndex = 1;
			// 
			// label_03
			// 
			label_03.Dock = System.Windows.Forms.DockStyle.Fill;
			label_03.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			label_03.Location = new System.Drawing.Point(0, 80);
			label_03.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
			label_03.Name = "label_03";
			label_03.Size = new System.Drawing.Size(420, 44);
			label_03.TabIndex = 4;
			label_03.Text = "Project location: (Where should the project be installed?)";
			label_03.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// checkBox_IncludeFLEP
			// 
			checkBox_IncludeFLEP.AutoSize = true;
			checkBox_IncludeFLEP.Checked = true;
			checkBox_IncludeFLEP.CheckState = System.Windows.Forms.CheckState.Checked;
			checkBox_IncludeFLEP.Location = new System.Drawing.Point(0, 250);
			checkBox_IncludeFLEP.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
			checkBox_IncludeFLEP.Name = "checkBox_IncludeFLEP";
			checkBox_IncludeFLEP.Size = new System.Drawing.Size(165, 17);
			checkBox_IncludeFLEP.TabIndex = 18;
			checkBox_IncludeFLEP.Text = "Include FLEP tools & patches";
			checkBox_IncludeFLEP.Visible = false;
			// 
			// tableLayoutPanel_Title
			// 
			tableLayoutPanel_Title.ColumnCount = 1;
			tableLayoutPanel_Title.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel_Title.Controls.Add(label_SubTitle01, 0, 1);
			tableLayoutPanel_Title.Controls.Add(label_Title01, 0, 0);
			tableLayoutPanel_Title.Dock = System.Windows.Forms.DockStyle.Fill;
			tableLayoutPanel_Title.Location = new System.Drawing.Point(30, 30);
			tableLayoutPanel_Title.Margin = new System.Windows.Forms.Padding(0);
			tableLayoutPanel_Title.Name = "tableLayoutPanel_Title";
			tableLayoutPanel_Title.RowCount = 2;
			tableLayoutPanel_Title.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			tableLayoutPanel_Title.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel_Title.Size = new System.Drawing.Size(520, 50);
			tableLayoutPanel_Title.TabIndex = 2;
			// 
			// label_SubTitle01
			// 
			label_SubTitle01.Dock = System.Windows.Forms.DockStyle.Fill;
			label_SubTitle01.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label_SubTitle01.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			label_SubTitle01.Location = new System.Drawing.Point(2, 30);
			label_SubTitle01.Margin = new System.Windows.Forms.Padding(2, 0, 0, 0);
			label_SubTitle01.Name = "label_SubTitle01";
			label_SubTitle01.Size = new System.Drawing.Size(518, 20);
			label_SubTitle01.TabIndex = 2;
			label_SubTitle01.Text = "General Information";
			label_SubTitle01.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label_Title01
			// 
			label_Title01.Dock = System.Windows.Forms.DockStyle.Fill;
			label_Title01.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label_Title01.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			label_Title01.Location = new System.Drawing.Point(0, 0);
			label_Title01.Margin = new System.Windows.Forms.Padding(0);
			label_Title01.Name = "label_Title01";
			label_Title01.Size = new System.Drawing.Size(520, 30);
			label_Title01.TabIndex = 1;
			label_Title01.Text = "Create a new project";
			// 
			// tableLayoutPanel_Buttons01
			// 
			tableLayoutPanel_Buttons01.ColumnCount = 2;
			tableLayoutPanel_Buttons01.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel_Buttons01.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			tableLayoutPanel_Buttons01.Controls.Add(button_Next, 0, 0);
			tableLayoutPanel_Buttons01.Controls.Add(button_Cancel, 0, 0);
			tableLayoutPanel_Buttons01.Dock = System.Windows.Forms.DockStyle.Fill;
			tableLayoutPanel_Buttons01.Location = new System.Drawing.Point(550, 80);
			tableLayoutPanel_Buttons01.Margin = new System.Windows.Forms.Padding(0);
			tableLayoutPanel_Buttons01.Name = "tableLayoutPanel_Buttons01";
			tableLayoutPanel_Buttons01.RowCount = 1;
			tableLayoutPanel_Buttons01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel_Buttons01.Size = new System.Drawing.Size(196, 375);
			tableLayoutPanel_Buttons01.TabIndex = 3;
			// 
			// button_Next
			// 
			button_Next.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			button_Next.Checked = false;
			button_Next.Location = new System.Drawing.Point(121, 350);
			button_Next.Margin = new System.Windows.Forms.Padding(0);
			button_Next.Name = "button_Next";
			button_Next.Size = new System.Drawing.Size(75, 25);
			button_Next.TabIndex = 13;
			button_Next.Text = "Next";
			button_Next.Click += button_Next_Click;
			// 
			// button_Cancel
			// 
			button_Cancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			button_Cancel.Checked = false;
			button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			button_Cancel.Location = new System.Drawing.Point(41, 350);
			button_Cancel.Margin = new System.Windows.Forms.Padding(0);
			button_Cancel.Name = "button_Cancel";
			button_Cancel.Size = new System.Drawing.Size(75, 25);
			button_Cancel.TabIndex = 12;
			button_Cancel.Text = "Cancel";
			// 
			// tablessTabControl
			// 
			tablessTabControl.Controls.Add(tabPage_Basic);
			tablessTabControl.Controls.Add(tabPage_Extra);
			tablessTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			tablessTabControl.Location = new System.Drawing.Point(0, 0);
			tablessTabControl.Name = "tablessTabControl";
			tablessTabControl.SelectedIndex = 0;
			tablessTabControl.Size = new System.Drawing.Size(784, 511);
			tablessTabControl.TabIndex = 3;
			// 
			// FormProjectSetup
			// 
			AcceptButton = button_Create;
			AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			ClientSize = new System.Drawing.Size(784, 511);
			Controls.Add(tablessTabControl);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			MaximizeBox = false;
			Name = "FormProjectSetup";
			ShowIcon = false;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "Create a new project...";
			panel_LevelsRadioChoice.ResumeLayout(false);
			panel_ScriptRadioChoice.ResumeLayout(false);
			tabPage_Extra.ResumeLayout(false);
			tableLayoutPanel_Main02.ResumeLayout(false);
			tableLayoutPanel_Buttons02.ResumeLayout(false);
			tableLayoutPanel_Content02.ResumeLayout(false);
			tableLayoutPanel_Content02.PerformLayout();
			tableLayoutPanel3.ResumeLayout(false);
			tabPage_Basic.ResumeLayout(false);
			tableLayoutPanel_Main01.ResumeLayout(false);
			tableLayoutPanel_Content01.ResumeLayout(false);
			tableLayoutPanel_Content01.PerformLayout();
			tableLayoutPanel_Title.ResumeLayout(false);
			tableLayoutPanel_Buttons01.ResumeLayout(false);
			tablessTabControl.ResumeLayout(false);
			ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_BrowseLevels;
		private DarkUI.Controls.DarkButton button_BrowseScript;
		private DarkUI.Controls.DarkButton button_Create;
		private DarkUI.Controls.DarkProgressBar progressBar;
		private DarkUI.Controls.DarkRadioButton radio_Levels_01;
		private DarkUI.Controls.DarkRadioButton radio_Levels_02;
		private DarkUI.Controls.DarkRadioButton radio_Script_01;
		private DarkUI.Controls.DarkRadioButton radio_Script_02;
		private DarkUI.Controls.DarkTextBox textBox_LevelsPath;
		private DarkUI.Controls.DarkTextBox textBox_ScriptPath;
		private System.Windows.Forms.Panel panel_LevelsRadioChoice;
		private System.Windows.Forms.Panel panel_ScriptRadioChoice;
		private System.Windows.Forms.TabPage tabPage_Extra;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Main02;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Buttons02;
		private DarkUI.Controls.DarkButton button_Back;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Content02;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		private DarkUI.Controls.DarkLabel label_SubTitle02;
		private DarkUI.Controls.DarkLabel label_Title02;
		private System.Windows.Forms.TabPage tabPage_Basic;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Main01;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Content01;
		private DarkUI.Controls.DarkButton button_BrowseProject;
		private DarkUI.Controls.DarkButton button_Help;
		private DarkUI.Controls.DarkTextBox textBox_ProjectPath;
		private DarkUI.Controls.DarkComboBox comboBox_EngineType;
		private DarkUI.Controls.DarkLabel label_01;
		private DarkUI.Controls.DarkLabel label_02;
		private DarkUI.Controls.DarkTextBox textBox_ProjectName;
		private DarkUI.Controls.DarkLabel label_03;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Title;
		private DarkUI.Controls.DarkLabel label_SubTitle01;
		private DarkUI.Controls.DarkLabel label_Title01;
		private TombLib.Controls.DarkTabbedContainer tablessTabControl;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Buttons01;
		private DarkUI.Controls.DarkButton button_Next;
		private DarkUI.Controls.DarkButton button_Cancel;
		private DarkUI.Controls.DarkCheckBox checkBox_IncludeFLEP;
	}
}