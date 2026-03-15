using DarkUI.Forms;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using TombIDE.ProjectMaster.Splash;
using TombIDE.Shared;

namespace TombIDE.ProjectMaster.Forms;

public partial class FormAdjustFrame : DarkForm
{
	private readonly IDE _ide;
	private readonly string _configPath;
	private readonly SplashConfiguration _config;

	private readonly ColorDialog _colorDialog = new() { FullOpen = true };

	public FormAdjustFrame(IDE ide)
	{
		_ide = ide;
		_configPath = Path.Combine(_ide.Project.GetEngineRootDirectoryPath(), "splash.xml");
		_config = new SplashConfiguration().Load(_configPath);

		InitializeComponent();

		LoadConfiguration();
	}

	protected override void OnClosed(EventArgs e)
	{
		_config.Save(_configPath);
		base.OnClosed(e);
	}

	private void LoadConfiguration()
	{
		panel_Top_StartColor.BackColor = ColorTranslator.FromHtml(_config.TopBar_GradientStartColor);
		panel_Top_EndColor.BackColor = ColorTranslator.FromHtml(_config.TopBar_GradientEndColor);
		panel_Bottom_StartColor.BackColor = ColorTranslator.FromHtml(_config.BottomBar_GradientStartColor);
		panel_Bottom_EndColor.BackColor = ColorTranslator.FromHtml(_config.BottomBar_GradientEndColor);
		panel_FontColor.BackColor = ColorTranslator.FromHtml(_config.FontColor);

		comboBox_Top_GradientFlow.SelectedIndex = (int)_config.TopBar_GradientFlow;
		comboBox_Bottom_GradientFlow.SelectedIndex = (int)_config.BottomBar_GradientFlow;

		numUpDown_Top_StartAlpha.Value = _config.TopBar_GradientStartAlpha;
		numUpDown_Top_EndAlpha.Value = _config.TopBar_GradientEndAlpha;
		numUpDown_Bottom_StartAlpha.Value = _config.BottomBar_GradientStartAlpha;
		numUpDown_Bottom_EndAlpha.Value = _config.BottomBar_GradientEndAlpha;

		comboBox_WindowAccent.SelectedIndex = _config.WindowAccent switch
		{
			ACCENT.ENABLE_ACRYLICBLURBEHIND => 1,
			_ => 0
		};

		numUpDown_DisplayTime.Value = _config.DisplayTimeMilliseconds;

		label_Message.ForeColor = ColorTranslator.FromHtml(_config.FontColor);

		switch (_config.WindowAccent)
		{
			case ACCENT.ENABLE_ACRYLICBLURBEHIND:
				panel_Top.BackColor = Color.Transparent;
				panel_Bottom.BackColor = Color.Transparent;
				break;

			case ACCENT.DISABLED:
				panel_Top.BackColor = Color.Black;
				panel_Bottom.BackColor = Color.Black;
				break;
		}
	}

	private void panel_Top_Gradient_Paint(object sender, PaintEventArgs e)
	{
		Point start = GetGradientStartPoint(_config.TopBar_GradientFlow, panel_Top_Gradient.ClientRectangle.Size);
		Point end = GetGradientEndPoint(_config.TopBar_GradientFlow, panel_Top_Gradient.ClientRectangle.Size);

		Color startColor = ColorTranslator.FromHtml(_config.TopBar_GradientStartColor);
		Color endColor = ColorTranslator.FromHtml(_config.TopBar_GradientEndColor);
		startColor = Color.FromArgb(_config.TopBar_GradientStartAlpha, startColor.R, startColor.G, startColor.B);
		endColor = Color.FromArgb(_config.TopBar_GradientEndAlpha, endColor.R, endColor.G, endColor.B);

		var brush = new LinearGradientBrush(start, end, startColor, endColor);
		e.Graphics.FillRectangle(brush, 0, 0, panel_Top_Gradient.Width, panel_Top_Gradient.Height);
	}

	private void panel_Bottom_Gradient_Paint(object sender, PaintEventArgs e)
	{
		Point start = GetGradientStartPoint(_config.BottomBar_GradientFlow, panel_Bottom_Gradient.ClientRectangle.Size);
		Point end = GetGradientEndPoint(_config.BottomBar_GradientFlow, panel_Bottom_Gradient.ClientRectangle.Size);

		Color startColor = ColorTranslator.FromHtml(_config.BottomBar_GradientStartColor);
		Color endColor = ColorTranslator.FromHtml(_config.BottomBar_GradientEndColor);
		startColor = Color.FromArgb(_config.BottomBar_GradientStartAlpha, startColor.R, startColor.G, startColor.B);
		endColor = Color.FromArgb(_config.BottomBar_GradientEndAlpha, endColor.R, endColor.G, endColor.B);

		var brush = new LinearGradientBrush(start, end, startColor, endColor);
		e.Graphics.FillRectangle(brush, 0, 0, panel_Bottom_Gradient.Width, panel_Bottom_Gradient.Height);
	}

	private Point GetGradientStartPoint(GradientFlow flow, Size rectSize) => flow switch
	{
		GradientFlow.RightToLeft => new Point(rectSize.Width, 0),
		GradientFlow.BottomToTop => new Point(0, rectSize.Height),
		_ => Point.Empty,
	};

	private Point GetGradientEndPoint(GradientFlow flow, Size rectSize) => flow switch
	{
		GradientFlow.LeftToRight => new Point(rectSize.Width, 0),
		GradientFlow.TopToBottom => new Point(0, rectSize.Height),
		_ => Point.Empty,
	};

	private void panel_Top_StartColor_Click(object sender, EventArgs e)
	{
		DialogResult result = _colorDialog.ShowDialog(this);

		if (result is DialogResult.OK)
		{
			panel_Top_StartColor.BackColor = _colorDialog.Color;
			_config.TopBar_GradientStartColor = ColorTranslator.ToHtml(_colorDialog.Color);
		}

		panel_Top_Gradient.Invalidate();
	}

	private void panel_Top_EndColor_Click(object sender, EventArgs e)
	{
		DialogResult result = _colorDialog.ShowDialog(this);

		if (result is DialogResult.OK)
		{
			panel_Top_EndColor.BackColor = _colorDialog.Color;
			_config.TopBar_GradientEndColor = ColorTranslator.ToHtml(_colorDialog.Color);
		}

		panel_Top_Gradient.Invalidate();
	}

	private void panel_Bottom_StartColor_Click(object sender, EventArgs e)
	{
		DialogResult result = _colorDialog.ShowDialog(this);

		if (result is DialogResult.OK)
		{
			panel_Bottom_StartColor.BackColor = _colorDialog.Color;
			_config.BottomBar_GradientStartColor = ColorTranslator.ToHtml(_colorDialog.Color);
		}

		panel_Bottom_Gradient.Invalidate();
	}

	private void panel_Bottom_EndColor_Click(object sender, EventArgs e)
	{
		DialogResult result = _colorDialog.ShowDialog(this);

		if (result is DialogResult.OK)
		{
			panel_Bottom_EndColor.BackColor = _colorDialog.Color;
			_config.BottomBar_GradientEndColor = ColorTranslator.ToHtml(_colorDialog.Color);
		}

		panel_Bottom_Gradient.Invalidate();
	}

	private void comboBox_Top_GradientFlow_SelectedIndexChanged(object sender, EventArgs e)
	{
		_config.TopBar_GradientFlow = (GradientFlow)comboBox_Top_GradientFlow.SelectedIndex;
		panel_Top_Gradient.Invalidate();
	}

	private void comboBox_Bottom_GradientFlow_SelectedIndexChanged(object sender, EventArgs e)
	{
		_config.BottomBar_GradientFlow = (GradientFlow)comboBox_Bottom_GradientFlow.SelectedIndex;
		panel_Bottom_Gradient.Invalidate();
	}

	private void numUpDown_Top_StartAlpha_ValueChanged(object sender, EventArgs e)
	{
		_config.TopBar_GradientStartAlpha = (int)numUpDown_Top_StartAlpha.Value;
		panel_Top_Gradient.Invalidate();
	}

	private void numUpDown_Top_EndAlpha_ValueChanged(object sender, EventArgs e)
	{
		_config.TopBar_GradientEndAlpha = (int)numUpDown_Top_EndAlpha.Value;
		panel_Top_Gradient.Invalidate();
	}

	private void numUpDown_Bottom_StartAlpha_ValueChanged(object sender, EventArgs e)
	{
		_config.BottomBar_GradientStartAlpha = (int)numUpDown_Bottom_StartAlpha.Value;
		panel_Bottom_Gradient.Invalidate();
	}

	private void numUpDown_Bottom_EndAlpha_ValueChanged(object sender, EventArgs e)
	{
		_config.BottomBar_GradientEndAlpha = (int)numUpDown_Bottom_EndAlpha.Value;
		panel_Bottom_Gradient.Invalidate();
	}

	private void panel_FontColor_Click(object sender, EventArgs e)
	{
		DialogResult result = _colorDialog.ShowDialog(this);

		if (result is DialogResult.OK)
		{
			panel_FontColor.BackColor = _colorDialog.Color;
			label_Message.ForeColor = _colorDialog.Color;

			_config.FontColor = ColorTranslator.ToHtml(_colorDialog.Color);
		}
	}

	private void comboBox_WindowAccent_SelectedIndexChanged(object sender, EventArgs e)
	{
		_config.WindowAccent = comboBox_WindowAccent.SelectedIndex switch
		{
			1 => ACCENT.ENABLE_ACRYLICBLURBEHIND,
			_ => ACCENT.DISABLED
		};

		switch (_config.WindowAccent)
		{
			case ACCENT.ENABLE_ACRYLICBLURBEHIND:
				panel_Top.BackColor = Color.Transparent;
				panel_Bottom.BackColor = Color.Transparent;
				break;

			case ACCENT.DISABLED:
				panel_Top.BackColor = Color.Black;
				panel_Bottom.BackColor = Color.Black;
				break;
		}
	}

	private void numUpDown_DisplayTime_ValueChanged(object sender, EventArgs e)
	{
		_config.DisplayTimeMilliseconds = (int)numUpDown_DisplayTime.Value;
	}

	private void button_RestoreDefaults_Click(object sender, EventArgs e)
	{
		DialogResult result = DarkMessageBox.Show(this,
			"Are you sure you want to restore default values?", "Are you sure?",
			MessageBoxButtons.YesNo, MessageBoxIcon.Question);

		if (result != DialogResult.Yes)
			return;

		var defaultConfig = new SplashConfiguration();

		_config.TopBar_GradientFlow = defaultConfig.TopBar_GradientFlow;
		_config.TopBar_GradientStartColor = defaultConfig.TopBar_GradientStartColor;
		_config.TopBar_GradientStartAlpha = defaultConfig.TopBar_GradientStartAlpha;
		_config.TopBar_GradientEndColor = defaultConfig.TopBar_GradientEndColor;
		_config.TopBar_GradientEndAlpha = defaultConfig.TopBar_GradientEndAlpha;

		_config.BottomBar_GradientFlow = defaultConfig.BottomBar_GradientFlow;
		_config.BottomBar_GradientStartColor = defaultConfig.BottomBar_GradientStartColor;
		_config.BottomBar_GradientStartAlpha = defaultConfig.BottomBar_GradientStartAlpha;
		_config.BottomBar_GradientEndColor = defaultConfig.BottomBar_GradientEndColor;
		_config.BottomBar_GradientEndAlpha = defaultConfig.BottomBar_GradientEndAlpha;

		_config.WindowAccent = defaultConfig.WindowAccent;
		_config.FontColor = defaultConfig.FontColor;
		_config.DisplayTimeMilliseconds = defaultConfig.DisplayTimeMilliseconds;

		LoadConfiguration();

		panel_Top_Gradient.Invalidate();
		panel_Bottom_Gradient.Invalidate();
		label_Message.ForeColor = ColorTranslator.FromHtml(_config.FontColor);
	}

	private void label_LivePreview_Click(object sender, EventArgs e)
	{
		_config.Save(_configPath);

		string launcherExecutable = _ide.Project.GetLauncherFilePath();
		string? originalFileName = FileVersionInfo.GetVersionInfo(launcherExecutable).OriginalFilename;

		if (originalFileName?.Equals("launch.exe", StringComparison.OrdinalIgnoreCase) is not true)
		{
			DarkMessageBox.Show(this, "Project not supported.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}

		var startInfo = new ProcessStartInfo
		{
			FileName = launcherExecutable,
			Arguments = "-p",
			WorkingDirectory = Path.GetDirectoryName(launcherExecutable),
			UseShellExecute = true
		};

		Process.Start(startInfo);
	}
}
