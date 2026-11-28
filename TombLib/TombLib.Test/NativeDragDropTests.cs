using ComDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using System.Windows.Interop;
using TombLib.Input;
using TombLib.WPF.Controls.Rendering;

namespace TombLib.Test;

[TestClass]
public class NativeDragDropTests
{
    [TestMethod]
    public void AcceptedManagedPayloadSurvivesEnterOverAndDrop()
    {
        StaTest.Run(() => {
            using var panel = new DropProbePanel { AllowDrop = true };
            using var source = CreateSource(panel);
            var drag = new DragDriver(panel);
            var payload = new object();
            var data = new DataObject("EditorObject", payload);
            const TransferEffects allowed = TransferEffects.Copy | TransferEffects.Move;

            Assert.AreEqual(TransferEffects.Copy, drag.Enter(data, allowed));
            Assert.AreEqual(TransferEffects.Copy, drag.Over(allowed));
            Assert.AreEqual(TransferEffects.Copy, drag.Over(allowed));
            Assert.AreEqual(TransferEffects.Copy, drag.Drop(data, allowed));
            Assert.AreEqual(TransferEffects.Copy, panel.DroppedEffect);
            Assert.AreSame(payload, panel.DroppedObject);
            Assert.AreEqual(1, panel.DropCount);
            Assert.AreEqual(TransferEffects.None, drag.Over(allowed));
            Assert.AreEqual(TransferEffects.None, drag.Drop(data, allowed));
            Assert.AreEqual(1, panel.DropCount);
        });
    }

    [TestMethod]
    public void UnsupportedDataAndCompletedDragsCannotReuseAcceptance()
    {
        StaTest.Run(() => {
            using var panel = new DropProbePanel { AllowDrop = true };
            using var source = CreateSource(panel);
            var drag = new DragDriver(panel);
            var accepted = new DataObject("EditorObject", new object());
            var rejected = new DataObject("Unsupported", new object());

            Assert.AreEqual(TransferEffects.Copy, drag.Enter(accepted));
            drag.Leave();
            Assert.AreEqual(TransferEffects.None, drag.Over());
            Assert.AreEqual(TransferEffects.None, drag.Drop(accepted));
            Assert.AreEqual(TransferEffects.None, drag.Enter(rejected));
            Assert.AreEqual(TransferEffects.None, drag.Over());
            Assert.AreEqual(TransferEffects.None, drag.Drop(rejected));
            Assert.AreEqual(0, panel.DropCount);

            Assert.AreEqual(TransferEffects.Copy, drag.Enter(accepted));
            Assert.AreEqual(TransferEffects.Copy, drag.Drop(accepted));
            Assert.AreEqual(TransferEffects.None, drag.Drop(rejected));
            Assert.AreEqual(1, panel.DropCount);
        });
    }

    [TestMethod]
    public void TargetSelectionIsRestrictedToSourceAllowedEffects()
    {
        StaTest.Run(() => {
            using var panel = new DropProbePanel { AllowDrop = true, AcceptedEffect = TransferEffects.Copy | TransferEffects.Move };
            using var source = CreateSource(panel);
            var drag = new DragDriver(panel);
            var data = new DataObject("EditorObject", new object());

            Assert.AreEqual(TransferEffects.Move, drag.Enter(data, TransferEffects.Move));
            Assert.AreEqual(TransferEffects.Move, drag.Over(TransferEffects.Move));
            Assert.AreEqual(TransferEffects.None, drag.Drop(data, TransferEffects.Copy));
            Assert.AreEqual(0, panel.DropCount);

            panel.AcceptedEffect = TransferEffects.Link;
            Assert.AreEqual(TransferEffects.None, drag.Enter(data));
            Assert.AreEqual(TransferEffects.None, drag.Over());
            Assert.AreEqual(TransferEffects.None, drag.Drop(data));
            Assert.AreEqual(0, panel.DropCount);
        });
    }

    [DataTestMethod]
    [DataRow("DragEnter")]
    [DataRow("DragOver")]
    [DataRow("Drop")]
    [DataRow("DragLeave")]
    public void CallbackFailuresDiscardStateAndAllowANewDrag(string stage)
    {
        StaTest.Run(() => {
            using var panel = new DropProbePanel { AllowDrop = true, FaultStage = stage };
            using var source = CreateSource(panel);
            var drag = new DragDriver(panel);
            var data = new DataObject("EditorObject", new object());
            var entered = drag.Enter(data);

            if (stage == "DragEnter")
                Assert.AreEqual(TransferEffects.None, entered);
            else if (stage == "DragOver")
                Assert.AreEqual(TransferEffects.None, drag.Over());
            else if (stage == "Drop")
                Assert.AreEqual(TransferEffects.None, drag.Drop(data));
            else
                drag.Leave();

            panel.FaultStage = null;
            int previousDrops = panel.DropCount;
            Assert.AreEqual(TransferEffects.None, drag.Over());
            Assert.AreEqual(TransferEffects.None, drag.Drop(data));
            Assert.AreEqual(previousDrops, panel.DropCount);
            Assert.AreEqual(TransferEffects.Copy, drag.Enter(data));
            Assert.AreEqual(TransferEffects.Copy, drag.Drop(data));
            Assert.AreEqual(previousDrops + 1, panel.DropCount);
        });
    }

    [TestMethod]
    public void ManagedFileListsSurviveTheNativeDragLifecycle()
    {
        StaTest.Run(() => {
            string[] files = [@"C:\Projects\Example.prj2", @"C:\Projects\Second.prj2"];
            var data = new DataObject(DataFormats.FileDrop, files);
            VerifyFileDrop(data, files);
        });
    }

    [TestMethod]
    public void NativeComFileListsSurviveTheNativeDragLifecycle()
    {
        StaTest.Run(() => {
            string[] files = [@"C:\Projects\Example.prj2"];
            var data = CreateNativeFileData(files);

            try
            {
                Assert.IsTrue(Marshal.IsComObject(data));
                VerifyFileDrop(data, files);
            }
            finally
            {
                Marshal.FinalReleaseComObject(data);
            }
        });
    }

    private static void VerifyFileDrop(ComDataObject data, string[] expectedFiles)
    {
        using var panel = new DropProbePanel { AllowDrop = true };
        using var source = CreateSource(panel);
        var drag = new DragDriver(panel);

        Assert.AreEqual(TransferEffects.Copy, drag.Enter(data));
        Assert.AreEqual(TransferEffects.Copy, drag.Over());
        Assert.AreEqual(TransferEffects.Copy, drag.Drop(data));
        CollectionAssert.AreEqual(expectedFiles, panel.DroppedFiles);
    }

    private static ComDataObject CreateNativeFileData(string[] files)
    {
        var interfaceId = typeof(ComDataObject).GUID;
        Marshal.ThrowExceptionForHR(SHCreateDataObject(IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero, ref interfaceId, out var data));
        var format = new FORMATETC { cfFormat = 15, dwAspect = DVASPECT.DVASPECT_CONTENT, lindex = -1, tymed = TYMED.TYMED_HGLOBAL };
        var managed = (ComDataObject)new DataObject(DataFormats.FileDrop, files);
        managed.GetData(ref format, out var medium);

        try
        {
            data.SetData(ref format, ref medium, true);
            return data;
        }
        catch
        {
            ReleaseStgMedium(ref medium);
            Marshal.FinalReleaseComObject(data);
            throw;
        }
    }

    private static HwndSource CreateSource(RenderingPanel panel)
    {
        var source = new HwndSource(new HwndSourceParameters("Native drag lifecycle regression test")
        {
            PositionX = -30000,
            PositionY = -30000,
            Width = 160,
            Height = 120,
            WindowStyle = unchecked((int)0x80000000)
        });
        source.RootVisual = panel;
        return source;
    }

    private sealed class DragDriver
    {
        private readonly object _target;
        private readonly object _point;

        public DragDriver(RenderingPanel panel)
        {
            var field = typeof(RenderingPanel).GetField("_dropTarget", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("The viewport has no native drop target.");
            _target = field.GetValue(panel) ?? throw new InvalidOperationException("The native target was not registered.");
            var enter = GetMethod("DragEnter");
            _point = Activator.CreateInstance(enter.GetParameters()[2].ParameterType)
                ?? throw new InvalidOperationException("The native point could not be created.");
        }

        public TransferEffects Enter(ComDataObject data, TransferEffects allowed = TransferEffects.Copy)
            => Invoke("DragEnter", data, 0u, _point, (uint)allowed);

        public TransferEffects Over(TransferEffects allowed = TransferEffects.Copy) => Invoke("DragOver", 0u, _point, (uint)allowed);

        public TransferEffects Drop(ComDataObject data, TransferEffects allowed = TransferEffects.Copy)
            => Invoke("Drop", data, 0u, _point, (uint)allowed);

        public void Leave() => Assert.AreEqual(0, GetMethod("DragLeave").Invoke(_target, null));

        private TransferEffects Invoke(string methodName, params object?[] arguments)
        {
            Assert.AreEqual(0, GetMethod(methodName).Invoke(_target, arguments));
            return (TransferEffects)(uint)(arguments[^1] ?? throw new InvalidOperationException("The drag effect was not returned."));
        }

        private MethodInfo GetMethod(string name) => _target.GetType().GetMethod(name)
            ?? throw new InvalidOperationException($"Missing OLE callback {name}.");
    }

    private sealed class DropProbePanel : RenderingPanel
    {
        public TransferEffects AcceptedEffect { get; set; } = TransferEffects.Copy;
        public TransferEffects DroppedEffect { get; private set; }
        public object? DroppedObject { get; private set; }
        public string[] DroppedFiles { get; private set; } = [];
        public int DropCount { get; private set; }
        public string? FaultStage { get; set; }

        protected override void OnDragEnter(PointerDragEventArgs e)
        {
            FailIfRequested("DragEnter");
            bool supported = e.Data?.GetDataPresent("EditorObject") is true || e.Data?.ContainsFileDropList() is true;
            e.Effect = supported ? AcceptedEffect : TransferEffects.None;
        }

        protected override void OnDragOver(PointerDragEventArgs e) => FailIfRequested("DragOver");

        protected override void OnDragDrop(PointerDragEventArgs e)
        {
            DropCount++;
            FailIfRequested("Drop");
            DroppedEffect = e.Effect;
            DroppedObject = e.Data?.GetData("EditorObject");
            DroppedFiles = e.Data?.GetFileDropList() ?? [];
        }

        protected override void OnDragLeave(EventArgs e) => FailIfRequested("DragLeave");

        private void FailIfRequested(string stage)
        {
            if (FaultStage == stage)
                throw new InvalidOperationException($"Injected {stage} failure.");
        }
    }

    [DllImport("shell32.dll")]
    private static extern int SHCreateDataObject(IntPtr folder, uint count, IntPtr items, IntPtr inner,
        ref Guid interfaceId, [MarshalAs(UnmanagedType.Interface)] out ComDataObject data);

    [DllImport("ole32.dll")]
    private static extern void ReleaseStgMedium(ref STGMEDIUM medium);
}
