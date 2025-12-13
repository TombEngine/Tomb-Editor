using System.ComponentModel;
using System.Windows;

namespace DarkUI.WPF;

public static class Defaults
{
	public static Theme CurrentTheme = Theme.Dark;

	public static event PropertyChangedEventHandler ShouldIconsInvertPropertyChanged;

	private static void OnShouldIconsInvertPropertyChanged(string propertyName)
		=> ShouldIconsInvertPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));

	private static bool _shouldIconsInvert = false;
	public static bool ShouldIconsInvert
	{
		get => _shouldIconsInvert;
		set
		{
			_shouldIconsInvert = value;
			OnShouldIconsInvertPropertyChanged(nameof(ShouldIconsInvert));
		}
	}

	#region Constants

	public const bool SnapsToDevicePixels = true;
	public const bool UseLayoutRounding = true;

	public const double SmallFontSize = 10;
	public const double DefaultFontSize = 11;
	public const double LargeFontSize = 13;
	public const double H1FontSize = 20;
	public const double H2FontSize = 16;

	public const double BorderThicknessWidth = 1;
	public const double TinyThicknessWidth = 2;
	public const double SmallThicknessWidth = 4;
	public const double MediumThicknessWidth = 8;
	public const double LargeThicknessWidth = 16;
	public const double ExtraLargeThicknessWidth = 24;
	public const double JumboThicknessWidth = 32;

	public const double StandardButtonWidth = 75;

	#endregion Constants

	#region General thicknesses

	public static readonly Thickness BorderThickness = new(BorderThicknessWidth);
	public static readonly Thickness TinyThickness = new(TinyThicknessWidth);
	public static readonly Thickness SmallThickness = new(SmallThicknessWidth);
	public static readonly Thickness MediumThickness = new(MediumThicknessWidth);
	public static readonly Thickness LargeThickness = new(LargeThicknessWidth);
	public static readonly Thickness ExtraLargeThickness = new(ExtraLargeThicknessWidth);
	public static readonly Thickness JumboThickness = new(JumboThicknessWidth);

	public static readonly Thickness InvertedBorderThickness = new(-BorderThicknessWidth);

	#endregion General thicknesses

	#region InputControl // Button, CheckBox, ColorPickerButton, ComboBox, NumericUpDown, ProgressBar, RadioButton, SearchableComboBox, TextBox, ToggleButton

	public static readonly double InputControl_Width = 75;
	public static readonly double InputControl_Height = 24;
	public static readonly double InputControl_DefaultPaddingWidth = 3;
	public static readonly double InputControl_LargePaddingWidth = 6;

	public static readonly Thickness InputControl_DefaultPadding = new(InputControl_DefaultPaddingWidth);

	#endregion InputControl // Button, CheckBox, ColorPickerButton, ComboBox, NumericUpDown, ProgressBar, RadioButton, SearchableComboBox, TextBox, ToggleButton

	#region CheckBox

	public static readonly double CheckBox_BoxWidth = 13;
	public static readonly double CheckBox_BoxHeight = 13;
	public static readonly double CheckBox_CheckMarkStrokeWidth = 2;

	public static readonly Thickness CheckBox_CheckMarkMargin = new(0.5);
	public static readonly Thickness CheckBox_IndeterminateMarkMargin = new(2);

	#endregion CheckBox

	#region ComboBox

	public static readonly double ComboBox_ArrowBox_Width = 17;
	public static readonly double ComboBox_Popup_MaxHeight = 300;
	public static readonly double ComboBox_Popup_NoItemsHeight = 40;

	public static readonly Thickness ComboBox_ArrowBox_Padding = new(3);

	#endregion ComboBox

	#region DataGrid

	public static readonly double DataGrid_ColumnHeader_GripperWidth = 8;
	public static readonly double DataGrid_ColumnHeader_SortingArrowWidth = 10;

	#endregion DataGrid

	#region DropShadowEffect

	public static readonly double DropShadow_Depth = 3;
	public static readonly double DropShadow_Opacity = 0.3;

	#endregion DropShadowEffect

	#region GroupBox

	public static readonly double GroupBox_BorderSpacingWidth = 6;
	public static readonly GridLength GroupBox_BorderSpacing = new(GroupBox_BorderSpacingWidth);

	#endregion GroupBox

	#region Image

	public static readonly double Image_MaxIconWidth = 16;
	public static readonly double Image_MaxIconHeight = 16;

	#endregion Image

	#region Menu / MenuItem

	public static readonly double MenuItem_Height = 24;
	public static readonly double MenuItem_CheckBoxWidth = 20;
	public static readonly double MenuItem_CheckBoxHeight = 20;

	public static readonly double SubMenuPopup_HorizontalOffset = -4;
	public static readonly double SubMenuPopup_ScrollButton_MinWidth = 10;
	public static readonly double SubMenuPopup_ScrollButton_MinHeight = 10;

	public static readonly GridLength SubMenuItem_ArrowArea_Width = new(12);
	public static readonly GridLength SubMenuItem_IconArea_Width = new(24);

	public static readonly Thickness SubMenuItem_Arrow_Margin = new(4);

	#endregion Menu / MenuItem

	#region NumericUpDown

	public static readonly double NumericUpDown_SpinnerWidth = 17;
	public static readonly Thickness NumericUpDown_RepeatButtonPadding = new(3);

	#endregion NumericUpDown

	#region Path

	public static readonly double Path_MaxIconWidth = 12;
	public static readonly double Path_MaxIconHeight = 12;

	#endregion Path

	#region RadioButton

	public static readonly double RadioButton_BoxWidth = 13;
	public static readonly double RadioButton_BoxHeight = 13;

	public static readonly Thickness RadioButton_CheckMarkMargin = new(2);

	#endregion RadioButton

	public static readonly double ShadowMarginThicknessWidth = 6;
	public static readonly Thickness Thickness_ShadowMargin = new(0, 0, ShadowMarginThicknessWidth, ShadowMarginThicknessWidth);

	public static readonly GridLength TreeView_Indentation = new(19);
	public static readonly Thickness TreeView_ExpanderIconNormalMargin = new(1.5);
	public static readonly Thickness TreeView_ExpanderIconCheckedMargin = new(2.75);

	public static readonly double ScrollBar_Width = 17;
	public static readonly double ScrollBar_Height = 17;

	public static readonly double TitleBar_Height = 30;
	public static readonly double TitleBar_ChromeHeight = TitleBar_Height - 5; // 5 is a system constant (I think)
	public static readonly double TitleBar_PathIconStrokeWidth = 1;
	public static readonly double TitleBar_ButtonWidth = 45;
	public static readonly double TitleBar_FontSize = 12;
	public static readonly double TitleBar_IconWidth = 64;
	public static readonly double TitleBar_IconVerticalOffset = -12;

	public static readonly double TabControl_SelectedTabMarginOffset = -2;

	public static readonly double ToolBar_GripperSize = 6;
	public static readonly double ToolBar_GripperMarginWidth = 3;
	public static readonly Thickness ToolBar_GripperMargin = new(ToolBar_GripperMarginWidth);
	public static readonly Rect ToolBar_GripperPatternViewbox = new(0, 0, 4, 4);
	public static readonly Rect ToolBar_GripperPatternViewport = new(0, 0, 4, 4);
	public static readonly double ToolBar_OverflowButtonSize = 14;
	public static readonly double ToolBar_MaxOverflowPopupWidth = 200;
	public static readonly double ToolBar_DropDownButtonArrowWidth = 6;
}
