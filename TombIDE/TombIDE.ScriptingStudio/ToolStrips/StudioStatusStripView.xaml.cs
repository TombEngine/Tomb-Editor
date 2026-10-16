#nullable enable

using System.Windows.Controls;

namespace TombIDE.ScriptingStudio.ToolStrips;

public partial class StudioStatusStripView : UserControl
{
	public StudioStatusStripView()
	{
		InitializeComponent();
	}

	public void ReloadContributionSettings()
		=> SyntaxPreview.ReloadSettings();
}
