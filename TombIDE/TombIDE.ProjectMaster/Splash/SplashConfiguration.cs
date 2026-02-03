using System;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;
using TombLib.Utils;

namespace TombIDE.ProjectMaster.Splash;

[XmlRoot("Configuration")]
public sealed class SplashConfiguration
{
	public ACCENT WindowAccent { get; set; } = ACCENT.ENABLE_ACRYLICBLURBEHIND;

	public GradientFlow TopBar_GradientFlow { get; set; } = GradientFlow.TopToBottom;
	public string TopBar_GradientStartColor { get; set; } = "#404040";
	public int TopBar_GradientStartAlpha { get; set; } = 192;
	public string TopBar_GradientEndColor { get; set; } = "#202020";
	public int TopBar_GradientEndAlpha { get; set; } = 192;

	public GradientFlow BottomBar_GradientFlow { get; set; } = GradientFlow.TopToBottom;
	public string BottomBar_GradientStartColor { get; set; } = "#404040";
	public int BottomBar_GradientStartAlpha { get; set; } = 192;
	public string BottomBar_GradientEndColor { get; set; } = "#202020";
	public int BottomBar_GradientEndAlpha { get; set; } = 192;

	public string FontColor { get; set; } = ColorTranslator.ToHtml(Color.White);

	public int DisplayTimeMilliseconds { get; set; } = 1500;

	public SplashConfiguration Load(Stream stream)
	{
		try
		{
			return XmlUtils.ReadXmlFile<SplashConfiguration>(stream);
		}
		catch (Exception)
		{
			return new SplashConfiguration();
		}
	}

	public SplashConfiguration Load(string filePath)
	{
		try
		{
			return XmlUtils.ReadXmlFile<SplashConfiguration>(filePath);
		}
		catch (Exception)
		{
			return new SplashConfiguration();
		}
	}

	public void Save(Stream stream)
		=> XmlUtils.WriteXmlFile(stream, GetType(), this);

	public void Save(string path)
	{
		string? directoryName = Path.GetDirectoryName(path);

		if (directoryName is not null && !Directory.Exists(directoryName))
			Directory.CreateDirectory(directoryName);

		XmlUtils.WriteXmlFile(path, GetType(), this);
	}
}
