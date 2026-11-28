#nullable enable

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WadTool.Features.Dialogs.AnimEditor;
using WadTool.Features.DockableViews.AnimationEditor;

namespace WadTool.Tests;

[TestClass]
[DoNotParallelize]
public class AnimationEditorPanelTests
{
    [TestMethod]
    public void PanelsLoadTheirXaml()
    {
        StaTestHost.Run(() =>
        {
            var styles = new ResourceDictionary { Source = new Uri("/WadTool;component/Features/DockableViews/AnimationEditor/PanelStyles.xaml", UriKind.Relative) };
            Assert.IsTrue(styles.Contains("Axis") && styles.Contains("Val") && styles.Contains("Nud") && styles.Contains("TbBtn"));

            UserControl[] panels =
            [
                new SkeletonPanel(),
                new TransformPanel(),
                new RootMotionPanel(),
                new BoundingBoxPanel(),
                new AnimationListPanel(),
                new CurrentAnimationPanel(),
                new AnimationBlendingPanel(),
                new TimelinePanel()
            ];

            foreach (var panel in panels)
            {
                panel.Measure(new Size(280.0, 400.0));
                Assert.IsNotNull(panel.Content);
            }
        });
    }

    [TestMethod]
    public void WindowLoadsItsXaml()
    {
        StaTestHost.Run(() =>
        {
            var window = new AnimationEditorWindow();

            Assert.IsNotNull(window.FindName("timelinePanel"));
            Assert.IsNotNull(window.FindName("dockManager"));
        });
    }

    [TestMethod]
    public void EveryDialogWindowLoadsItsXaml()
    {
        var failures = new System.Collections.Generic.List<string>();

        foreach (var type in typeof(WadToolClass).Assembly.GetTypes().Where(type => type.Name.EndsWith("Window") && type.Namespace?.StartsWith("WadTool.Features.Dialogs") == true))
        {
            StaTestHost.Run(() =>
            {
                try
                {
                    Assert.IsInstanceOfType(Activator.CreateInstance(type), typeof(Window));
                }
                catch (Exception exception)
                {
                    failures.Add(type.Name + ": " + exception.GetBaseException().Message);
                }
            });
        }

        Assert.AreEqual(0, failures.Count, string.Join("; ", failures));
    }
}
