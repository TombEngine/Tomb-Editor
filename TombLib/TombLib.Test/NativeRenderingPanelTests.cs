using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using TombLib.Controls.Wpf;

using DrawingPoint = System.Drawing.Point;
using WinForms = System.Windows.Forms;
using WpfSize = System.Windows.Size;

namespace TombLib.Test;

[TestClass]
public class NativeRenderingPanelTests
{
    [TestMethod]
    public void SurfaceCreatesNativeChildAndPreservesScreenCoordinates()
    {
        StaTest.Run(() => {
            using var source = CreateSource();
            using var panel = new RenderingPanel();
            source.RootVisual = panel;
            panel.Measure(new WpfSize(160.0, 120.0));
            panel.Arrange(new Rect(0.0, 0.0, 160.0, 120.0));

            Assert.AreNotEqual(IntPtr.Zero, panel.Handle);
            Assert.AreNotEqual(0, GetClassLong(panel.Handle, -26) & 8, "The native window class must synthesize double-click messages.");
            Assert.AreEqual(source.Handle, GetParent(panel.Handle));
            Assert.IsFalse(typeof(WinForms.Control).IsAssignableFrom(panel.GetType()));
            var point = new DrawingPoint(37, 49);
            Assert.AreEqual(point, panel.PointToClient(panel.PointToScreen(point)));
            Assert.AreEqual(new Rectangle(DrawingPoint.Empty, panel.ClientSize), panel.ClientRectangle);
        });
    }

    [TestMethod]
    public void NativeMouseMessagesPreserveButtonCoordinatesAndDoubleClicks()
    {
        StaTest.Run(() => {
            using var source = CreateSource();
            using var panel = new RenderingPanel();
            source.RootVisual = panel;
            WinForms.MouseEventArgs? pressed = null;
            WinForms.MouseEventArgs? released = null;
            WinForms.MouseEventArgs? doubleClicked = null;
            panel.MouseDown += (_, args) => pressed = args;
            panel.MouseUp += (_, args) => released = args;
            panel.MouseDoubleClick += (_, args) => doubleClicked = args;
            var location = new IntPtr((49 << 16) | 37);

            SendMessage(panel.Handle, 0x0204, new IntPtr(2), location);
            SendMessage(panel.Handle, 0x0205, IntPtr.Zero, location);
            SendMessage(panel.Handle, 0x0206, new IntPtr(2), location);

            Assert.IsNotNull(pressed);
            Assert.IsNotNull(released);
            Assert.IsNotNull(doubleClicked);
            Assert.AreEqual(WinForms.MouseButtons.Right, pressed.Button);
            Assert.AreEqual(new DrawingPoint(37, 49), pressed.Location);
            Assert.AreEqual(WinForms.MouseButtons.Right, released.Button);
            Assert.AreEqual(2, doubleClicked.Clicks);
        });
    }

    [TestMethod]
    public void SurfaceRegistersNativeDropTargetAndDestroysItsWindow()
    {
        StaTest.Run(() => {
            using var source = CreateSource();
            var panel = new RenderingPanel { AllowDrop = true };
            source.RootVisual = panel;
            var handle = panel.Handle;
            Assert.AreEqual(0, RevokeDragDrop(handle));

            panel.Dispose();

            Assert.IsFalse(IsWindow(handle));
        });
    }

    [TestMethod]
    public void NativeDropPreservesManagedEditorObjectsWithoutSerialization()
    {
        StaTest.Run(() => {
            using var source = CreateSource();
            using var panel = new DropProbePanel { AllowDrop = true };
            source.RootVisual = panel;
            var payload = new object();
            var data = new System.Windows.DataObject("EditorObject", payload);
            var field = typeof(RenderingPanel).GetField("_dropTarget", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("The viewport has no native drop target.");
            var target = field.GetValue(panel) ?? throw new InvalidOperationException("The native target was not registered.");
            var enter = target.GetType().GetMethod("DragEnter") ?? throw new InvalidOperationException("Missing OLE drag callback.");
            var point = Activator.CreateInstance(enter.GetParameters()[2].ParameterType);

            enter.Invoke(target, new object?[] { data, 0u, point, 1u });

            Assert.AreSame(payload, panel.ReceivedObject);
        });
    }

    private sealed class DropProbePanel : RenderingPanel
    {
        public object? ReceivedObject { get; private set; }

        protected override void OnDragEnter(WinForms.DragEventArgs e)
        {
            ReceivedObject = e.Data?.GetData("EditorObject");
            e.Effect = WinForms.DragDropEffects.Copy;
        }
    }

    [TestMethod]
    public void RenderingFailureStopsNativeDrawingAndRaisesOneRecoveryNotification()
    {
        StaTest.Run(() => {
            using var source = CreateSource();
            using var panel = new FailureProbePanel();
            source.RootVisual = panel;
            var failure = new InvalidOperationException("Device lost during drawing.");
            int notifications = 0;
            panel.RenderingFailed += (_, _) => notifications++;

            panel.Fail(failure);
            panel.Fail(new InvalidOperationException("Repeated frame failure."));

            Assert.AreSame(failure, panel.RenderingError);
            Assert.AreEqual(1, notifications);
            Assert.IsFalse(IsWindowVisible(panel.Handle));
        });
    }

    private sealed class FailureProbePanel : RenderingPanel
    {
        public void Fail(Exception exception) => ReportRenderingError(exception);
    }

    [DllImport("user32.dll", EntryPoint = "GetClassLongW")]
    private static extern int GetClassLong(IntPtr window, int index);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr window);

    private static HwndSource CreateSource() => new(new HwndSourceParameters("Native rendering surface regression test")
    {
        PositionX = -10000,
        PositionY = -10000,
        Width = 160,
        Height = 120,
        WindowStyle = unchecked((int)0x80000000)
    });

    [DllImport("ole32.dll")]
    private static extern int RevokeDragDrop(IntPtr window);

    [DllImport("user32.dll")]
    private static extern IntPtr GetParent(IntPtr window);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindow(IntPtr window);
}
