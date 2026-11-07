using System;
using System.Drawing;
using DarkUI.Extensions;

namespace DarkUI.Config
{
    public static class Colors
    {
        // Default color values. Will be always used if brightness is unmodified.

        public static Color GreyBackground { get; private set; } = Color.FromArgb(60, 63, 65);
        public static Color HeaderBackground { get; private set; } = Color.FromArgb(57, 60, 62);
        public static Color BlueBackground { get; private set; } = Color.FromArgb(66, 77, 95);
        public static Color DarkBlueBackground { get; private set; } = Color.FromArgb(52, 57, 66);
        public static Color DarkBackground { get; private set; } = Color.FromArgb(43, 43, 43);
        public static Color MediumBackground { get; private set; } = Color.FromArgb(49, 51, 53);
        public static Color LightBackground { get; private set; } = Color.FromArgb(69, 73, 74);
        public static Color LighterBackground { get; private set; } = Color.FromArgb(95, 101, 102);
        public static Color LightestBackground { get; private set; } = Color.FromArgb(178, 178, 178);
        public static Color LightBorder { get; private set; } = Color.FromArgb(81, 81, 81);
        public static Color DarkBorder { get; private set; } = Color.FromArgb(51, 51, 51);
        public static Color LightText { get; private set; } = Color.FromArgb(220, 220, 220);
        public static Color DisabledText { get; private set; } = Color.FromArgb(153, 153, 153);
        public static Color BlueHighlight { get; private set; } = Color.FromArgb(104, 151, 187);
        public static Color BlueSelection { get; private set; } = Color.FromArgb(75, 110, 175);
        public static Color GreyHighlight { get; private set; } = Color.FromArgb(122, 128, 132);
        public static Color GreySelection { get; private set; } = Color.FromArgb(92, 92, 92);
        public static Color DarkGreySelection { get; private set; } = Color.FromArgb(82, 82, 82);
        public static Color DarkBlueBorder { get; private set; } = Color.FromArgb(51, 61, 78);
        public static Color LightBlueBorder { get; private set; } = Color.FromArgb(86, 97, 114);
        public static Color ActiveControl { get; private set; } = Color.FromArgb(159, 178, 196);

        // Button/toolbar control highlight color is specified independently to allow
        // high-contrast color schemes.

        public static Color HighlightBase { get; set; } = Color.FromArgb(104, 151, 187);
        public static Color HighlightFill => HighlightBase.Multiply(0.66f, 0.45f, 0.56f);

        // DarkBase is base colour on which all other colours are recalculated if brightness
        // is changed.

        private static Color DarkBase { get; } = Color.FromArgb(60, 63, 65);

        // MaxBrightness should be 1.0f at all times!
        // MinBrightness can go down up to 0.1, but almost nothing will be visible...
        // So let's clamp it to 0.5 at least.

        public const float MaxBrightness = 1.0f;
        public const float MinBrightness = 0.5f;

        public static bool HasBrightnessChanged { get; private set; }

        // AlphaBrightness is a helper value which is used in filling overlay rectangles
        // over image controls (e.g. buttons with pictures, arrows in comboboxes etc.).
        public static float AlphaBrightness => MaxBrightness - Brightness;

        // Fonts should use different brightness formula to provide enough contrast.
        public static float FontBrightness => Brightness + (MaxBrightness - Brightness) * 0.35f;

        // Every time brightness is changed, all UI colours are automatically recalculated against DarkBase.

        public static float Brightness
        {
            get { return _brightness; }
            set
            {
                _brightness = Math.Min(Math.Max(value, MinBrightness), MaxBrightness);
                HasBrightnessChanged = _brightness != 1.0f;

                GreyBackground  	= DarkBase.Multiply(_brightness * 1.000f, _brightness * 1.000f, _brightness * 1.000f);
				HeaderBackground  	= DarkBase.Multiply(_brightness * 0.950f, _brightness * 0.952f, _brightness * 0.954f);
				BlueBackground  	= DarkBase.Multiply(_brightness * 1.100f, _brightness * 1.222f, _brightness * 1.462f);
				DarkBlueBackground 	= DarkBase.Multiply(_brightness * 0.867f, _brightness * 0.905f, _brightness * 1.015f);
				DarkBackground  	= DarkBase.Multiply(_brightness * 0.717f, _brightness * 0.683f, _brightness * 0.662f);
				MediumBackground  	= DarkBase.Multiply(_brightness * 0.817f, _brightness * 0.810f, _brightness * 0.815f);
				LightBackground  	= DarkBase.Multiply(_brightness * 1.150f, _brightness * 1.159f, _brightness * 1.138f);
				LighterBackground   = DarkBase.Multiply(_brightness * 1.583f, _brightness * 1.603f, _brightness * 1.569f);
				LightestBackground  = DarkBase.Multiply(_brightness * 2.967f, _brightness * 2.825f, _brightness * 2.738f);
				LightBorder  		= DarkBase.Multiply(_brightness * 1.350f, _brightness * 1.286f, _brightness * 1.246f);
				DarkBorder  		= DarkBase.Multiply(_brightness * 0.850f, _brightness * 0.810f, _brightness * 0.785f);
				BlueHighlight  	 	= DarkBase.Multiply(_brightness * 1.733f, _brightness * 2.397f, _brightness * 2.877f);
				BlueSelection  	 	= DarkBase.Multiply(_brightness * 1.250f, _brightness * 1.746f, _brightness * 2.692f);
				GreyHighlight  	 	= DarkBase.Multiply(_brightness * 2.033f, _brightness * 2.032f, _brightness * 2.031f);
				GreySelection  	 	= DarkBase.Multiply(_brightness * 1.533f, _brightness * 1.460f, _brightness * 1.415f);
				DarkGreySelection   = DarkBase.Multiply(_brightness * 1.367f, _brightness * 1.302f, _brightness * 1.262f);
				DarkBlueBorder  	= DarkBase.Multiply(_brightness * 0.850f, _brightness * 0.968f, _brightness * 1.200f);
				LightBlueBorder  	= DarkBase.Multiply(_brightness * 1.433f, _brightness * 1.540f, _brightness * 1.754f);
				ActiveControl  	 	= DarkBase.Multiply(_brightness * 2.650f, _brightness * 2.825f, _brightness * 3.015f);

                // Text is multiplied by lesser value to preserve contrast
                LightText           = DarkBase.Multiply(FontBrightness * 3.667f, FontBrightness * 3.492f, FontBrightness * 3.385f);
                DisabledText        = DarkBase.Multiply(FontBrightness * 2.550f, FontBrightness * 2.429f, FontBrightness * 2.354f);
            }
        }
        private static float _brightness = 1.0f; // Set it here by default, so designer will correctly show controls which use brightness natively.
    }
}
