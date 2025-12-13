using DarkUI.WPF.Extensions;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DarkUI.WPF.Converters;

public class IndentToMarginConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is not TreeViewItem item)
			return new Thickness(0);

		double leftMargin = Defaults.TreeView_Indentation.Value * item.GetDepth();
		return new Thickness(leftMargin, 0, 0, 0);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}
