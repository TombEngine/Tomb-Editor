using DarkUI.Controls;
using System;

namespace TombEditor.Features.Panel3D.ToolPalette;

public partial class ToolPaletteFloating : DarkFloatingToolbox
{
	public ToolPaletteFloating()
	{
		InitializeComponent();
	}

	private void toolBox_SizeChanged(object sender, EventArgs e)
	{
		AutoSize = true;
	}
}
