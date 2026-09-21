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
	public void GetIcon_EverySharedKind_ReturnsImage()
	{
		WPFTestHelper.RunInSta(() =>
		{
			LuaThemeBrushSet brushSet = CreateBrushSet("IconCoverageTestTheme");

			// Identifier strings keep this test independent of the library revision it compiles against;
			// every shared kind (including kinds without a dedicated glyph) must produce an icon.
			string[] kindIdentifiers =
			[
				"Generic", "Text", "Property", "Array", "Section", "Directive", "Constant", "Keyword",
				"Method", "Function", "Constructor", "Event", "Operator", "Variable", "Value",
				"Reference", "Field", "Class", "Interface", "Enum", "EnumMember", "Struct", "TypeParameter",
				"Parameter", "Namespace", "Module", "Unit", "File", "Folder", "Snippet", "Color"
			];

			foreach (string identifier in kindIdentifiers)
			{
				ImageSource icon = LuaCompletionIconFactory.GetIcon(TextCompletionItemKind.FromIdentifier(identifier), brushSet);

				Assert.IsNotNull(icon, $"Kind '{identifier}' must produce an icon.");
				Assert.IsTrue(icon.IsFrozen, $"Kind '{identifier}' must produce a frozen icon.");
			}
		});
	}

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
