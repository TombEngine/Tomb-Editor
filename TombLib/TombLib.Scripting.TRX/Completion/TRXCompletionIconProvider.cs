using Nickelony.IDEKit.IntelliSense.Completion;
using System.Windows.Media;

namespace TombLib.Scripting.TRX.Completion;

internal static class TRXCompletionIconProvider
{
	private static readonly ImageSource? s_arrayImage = new ImageSourceConverter()
		.ConvertFromString("pack://application:,,,/TombLib.Scripting.TRX;component/Resources/Icons/Array.png") as ImageSource;

	private static readonly ImageSource? s_constantImage = new ImageSourceConverter()
		.ConvertFromString("pack://application:,,,/TombLib.Scripting.TRX;component/Resources/Icons/Constant.png") as ImageSource;

	private static readonly ImageSource? s_propertyImage = new ImageSourceConverter()
		.ConvertFromString("pack://application:,,,/TombLib.Scripting.TRX;component/Resources/Icons/Property.png") as ImageSource;

	public static ImageSource? GetImage(TextCompletionItem item) => item.Kind.Identifier switch
	{
		"Array" => s_arrayImage,
		"Property" => s_propertyImage,
		"Constant" => s_constantImage,
		_ => null,
	};
}
