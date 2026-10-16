using System.Windows;
using Moq;
using TombIDE.ScriptingStudio.Shell;

namespace TombEditor.Tests.ScriptingStudio;

internal static class ScriptingStudioChromeTestFixture
{
	public static Mock<IMenuService> CreateMenuServiceMock()
	{
		var mock = new Mock<IMenuService>();
		mock.Setup(service => service.MenuView).Returns(Mock.Of<FrameworkElement>());
		return mock;
	}

	public static Mock<IToolBarService> CreateToolBarServiceMock()
	{
		var mock = new Mock<IToolBarService>();
		mock.Setup(service => service.ToolBarView).Returns(Mock.Of<FrameworkElement>());
		return mock;
	}

	public static Mock<IStatusBarService> CreateStatusBarServiceMock()
	{
		var mock = new Mock<IStatusBarService>();
		mock.Setup(service => service.StatusBarView).Returns(Mock.Of<FrameworkElement>());
		return mock;
	}
}
