#nullable enable

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using WadTool.Features.DockableViews.AnimationEditor;

namespace WadTool.Tests;

[TestClass]
[DoNotParallelize]
public class AnimationEditorPanelTests
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
            throw new AssertFailedException(failure.ToString(), failure);
    }

    [TestMethod]
    public void PanelsLoadTheirXaml()
    {
        RunSta(() =>
        {
            _ = new Application();

            var styles = new ResourceDictionary { Source = new Uri("/WadTool;component/Features/DockableViews/AnimationEditor/PanelStyles.xaml", UriKind.Relative) };
            Assert.IsTrue(styles.Contains("Axis") && styles.Contains("Val"));

            foreach (var panel in new UserControl[] { new SkeletonPanel(), new TransformPanel(), new RootMotionPanel(), new BoundingBoxPanel() })
            {
                panel.Measure(new Size(280.0, 400.0));
                Assert.IsNotNull(panel.Content);
            }
        });
    }
}
