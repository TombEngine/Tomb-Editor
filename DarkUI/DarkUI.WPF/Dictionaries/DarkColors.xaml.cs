using DarkUI.WPF.Config;
using System.Windows;

namespace DarkUI.WPF.Dictionaries;

public partial class DarkColors : ResourceDictionary
{
    public DarkColors()
    {
        InitializeComponent();

        if (!Colors.HasBrightnessChanged)
            return;

        ApplyDarkUiColors();
    }

    private void ApplyDarkUiColors()
    {
        // Keep the XAML palette as the fallback and overwrite it only when DarkUI brightness has changed.
        SetColor("Color_Text", Colors.LightText);
        SetColor("Color_Background", Colors.GreyBackground);
        SetColor("Color_Background_Alternative", Colors.HeaderBackground);
        SetColor("Color_Background_Control", Colors.LightBackground);
        SetColor("Color_Background_Defaulted", Colors.DarkBlueBackground);
        SetColor("Color_Background_Disabled", Colors.DarkGreySelection);
        SetColor("Color_Background_High", Colors.LighterBackground);
        SetColor("Color_Background_Low", Colors.MediumBackground);

        SetColor("Color_Border", Colors.GreySelection);
        SetColor("Color_Border_High", Colors.LightestBackground);
        SetColor("Color_Border_Low", Colors.MediumBackground);

        SetColor("Color_Highlight", Colors.BlueHighlight);
        SetColor("Color_Selection", Colors.BlueSelection);
        SetColor("Color_Selection_LostFocus", Colors.GreySelection);
        SetColor("Color_WindowBorder", Colors.BlueBackground);

        this["Opacity_Icon"] = (double)Colors.Brightness;
    }

    private void SetColor(string key, System.Drawing.Color color)
    {
        this[key] = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
    }
}
