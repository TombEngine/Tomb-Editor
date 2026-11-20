#nullable enable

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;
using System.Windows.Controls;
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
}
