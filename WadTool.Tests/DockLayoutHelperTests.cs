#nullable enable

using AvalonDock;
using AvalonDock.Layout;
using DarkUI.WPF.AvalonDock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Controls;

namespace WadTool.Tests;

[TestClass]
[DoNotParallelize]
public class DockLayoutHelperTests
{
    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (failure is not null)
            throw new AssertFailedException(failure.Message, failure);
    }

    private static DockingManager CreateManager()
    {
        var left = new LayoutAnchorablePane();
        left.Children.Add(new LayoutAnchorable { ContentId = "first", Title = "First", Content = new TextBlock() });
        left.Children.Add(new LayoutAnchorable { ContentId = "second", Title = "Second", Content = new TextBlock() });

        var panel = new LayoutPanel();
        panel.Children.Add(left);

        var manager = new DockingManager { Layout = new LayoutRoot { RootPanel = panel } };
        return manager;
    }

    [TestMethod]
    public void HiddenPanelStateSurvivesSerializeAndLoad()
    {
        RunSta(() =>
        {
            var manager = CreateManager();
            var helper = new DockLayoutHelper(manager);
            helper.CaptureDefault();

            helper.SetShown("second", false);
            Assert.IsFalse(helper.IsShown("second"));

            var saved = helper.Serialize();

            var restored = CreateManager();
            var restoredHelper = new DockLayoutHelper(restored);

            Assert.IsTrue(restoredHelper.Load(saved));
            Assert.IsFalse(restoredHelper.IsShown("second"));
            Assert.IsTrue(restoredHelper.IsShown("first"));
        });
    }

    [TestMethod]
    public void ResetReturnsToCapturedDefault()
    {
        RunSta(() =>
        {
            var manager = CreateManager();
            var helper = new DockLayoutHelper(manager);
            helper.CaptureDefault();

            helper.SetShown("first", false);
            helper.Reset();

            Assert.IsTrue(helper.IsShown("first"));
            Assert.IsTrue(helper.IsShown("second"));
        });
    }

    [TestMethod]
    public void ShowingAHiddenPanelDocksItAgain()
    {
        RunSta(() =>
        {
            var manager = CreateManager();
            var helper = new DockLayoutHelper(manager);

            helper.SetShown("first", false);
            helper.SetShown("first", true);

            Assert.IsTrue(helper.IsShown("first"));
            Assert.AreEqual(2, manager.Layout.Descendents().OfType<LayoutAnchorable>().Count(anchorable => !anchorable.IsHidden));
        });
    }

    [TestMethod]
    public void UnusableLayoutIsRejectedWithoutChangingPanels()
    {
        RunSta(() =>
        {
            var manager = CreateManager();
            var helper = new DockLayoutHelper(manager);

            Assert.IsFalse(helper.Load(string.Empty));
            Assert.IsFalse(helper.Load("<not a layout"));
            Assert.IsTrue(helper.IsShown("first"));
        });
    }
}
