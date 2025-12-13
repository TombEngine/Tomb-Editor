using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace DarkUI.WPF.CustomControls;

[TemplatePart(Name = "PART_ToggleButton", Type = typeof(ToggleButton))]
[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
public class DropDownButton : ContentControl
{
	public static readonly DependencyProperty IsOpenProperty;
	public static readonly DependencyProperty DropDownPositionProperty;
	public static readonly DependencyProperty MaxDropDownHeightProperty;
	public static readonly DependencyProperty DropDownContentProperty;

	public bool IsOpen
	{
		get => (bool)GetValue(IsOpenProperty);
		set => SetValue(IsOpenProperty, value);
	}

	public PlacementMode DropDownPosition
	{
		get => (PlacementMode)GetValue(DropDownPositionProperty);
		set => SetValue(DropDownPositionProperty, value);
	}

	public double MaxDropDownHeight
	{
		get => (double)GetValue(MaxDropDownHeightProperty);
		set => SetValue(MaxDropDownHeightProperty, value);
	}

	public object? DropDownContent
	{
		get => GetValue(DropDownContentProperty);
		set => SetValue(DropDownContentProperty, value);
	}

	public ToggleButton? ToggleButton { get; set; }
	public Popup? Popup { get; set; }

	static DropDownButton()
	{
		IsOpenProperty = DependencyProperty.Register(
			nameof(IsOpen),
			typeof(bool),
			typeof(DropDownButton),
			new PropertyMetadata(false));

		DropDownPositionProperty = DependencyProperty.Register(
			nameof(DropDownPosition),
			typeof(PlacementMode),
			typeof(DropDownButton),
			new PropertyMetadata(PlacementMode.Bottom));

		MaxDropDownHeightProperty = DependencyProperty.Register(
			nameof(MaxDropDownHeight),
			typeof(double),
			typeof(DropDownButton),
			new PropertyMetadata(SystemParameters.PrimaryScreenHeight / 2.0));

		DropDownContentProperty = DependencyProperty.Register(
			nameof(DropDownContent),
			typeof(object),
			typeof(DropDownButton),
			new PropertyMetadata(null));
	}

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		ToggleButton = GetTemplateChild("PART_ToggleButton") as ToggleButton;
		Popup = GetTemplateChild("PART_Popup") as Popup;
	}
}
