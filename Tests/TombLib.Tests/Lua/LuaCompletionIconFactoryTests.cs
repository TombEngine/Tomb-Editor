using Nickelony.IDEKit.IntelliSense.Completion;
using System.Windows.Media;
using TombLib.Scripting.Lua.Completion;
using TombLib.Scripting.Lua.Resources;
using TombLib.Scripting.Lua.Themes;

namespace TombLib.Tests;

[TestClass]
public class LuaCompletionIconFactoryTests
{
	private static LuaThemeBrushSet CreateBrushSet(string themeName)
		=> LuaEditorColorPalette.Create(new LuaTheme { Name = themeName });

	[TestMethod]
	public void GetIcon_SameThemeAndKind_ReturnsCachedInstance()
	{
		WPFTestHelper.RunInSta(() =>
		{
			LuaThemeBrushSet brushSet = CreateBrushSet("IconCacheTestThemeA");

			ImageSource first = LuaCompletionIconFactory.GetIcon(TextCompletionItemKind.Method, brushSet);
			ImageSource second = LuaCompletionIconFactory.GetIcon(TextCompletionItemKind.Method, brushSet);

			Assert.AreSame(first, second);
		});
	}

	[TestMethod]
	public void GetIcon_DifferentKinds_AreCachedSeparately()
	{
		WPFTestHelper.RunInSta(() =>
		{
			LuaThemeBrushSet brushSet = CreateBrushSet("IconCacheTestThemeA");

			ImageSource methodIcon = LuaCompletionIconFactory.GetIcon(TextCompletionItemKind.Method, brushSet);
			ImageSource keywordIcon = LuaCompletionIconFactory.GetIcon(TextCompletionItemKind.Keyword, brushSet);

			Assert.AreNotSame(methodIcon, keywordIcon);
		});
	}

	[TestMethod]
	public void GetIcon_ThemeChange_InvalidatesCachedIcons()
	{
		WPFTestHelper.RunInSta(() =>
		{
			LuaThemeBrushSet themeA = CreateBrushSet("IconCacheTestThemeA");
			LuaThemeBrushSet themeB = CreateBrushSet("IconCacheTestThemeB");

			ImageSource first = LuaCompletionIconFactory.GetIcon(TextCompletionItemKind.Method, themeA);
			ImageSource second = LuaCompletionIconFactory.GetIcon(TextCompletionItemKind.Method, themeB);

			Assert.AreNotSame(first, second);
		});
	}
}
