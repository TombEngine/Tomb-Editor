#nullable enable

using NLog;
using System;
using System.Runtime.InteropServices;

using ComDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;
using WinForms = System.Windows.Forms;

namespace TombLib.Controls.Wpf;

public partial class RenderingPanel
{
    private NativeDropTarget? _dropTarget;
    private bool _oleInitialized;

    private void RegisterDropTarget()
    {
        if (!AllowDrop)
            return;

        int initialized = OleInitialize(IntPtr.Zero);
        if (initialized < 0)
            Marshal.ThrowExceptionForHR(initialized);

        _oleInitialized = true;
        _dropTarget = new NativeDropTarget(this);
        int registered = RegisterDragDrop(_windowHandle, _dropTarget);
        if (registered < 0)
            Marshal.ThrowExceptionForHR(registered);
    }

    private void RevokeDropTarget()
    {
        if (_dropTarget is not null)
        {
            RevokeDragDrop(_windowHandle);
            _dropTarget = null;
        }

        if (_oleInitialized)
        {
            OleUninitialize();
            _oleInitialized = false;
        }
    }

    [ComVisible(true)]
    [Guid("00000122-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface INativeDropTarget
    {
        [PreserveSig]
        int DragEnter([MarshalAs(UnmanagedType.Interface)] ComDataObject data, uint keys, NativePoint point, ref uint effect);

        [PreserveSig]
        int DragOver(uint keys, NativePoint point, ref uint effect);

        [PreserveSig]
        int DragLeave();

        [PreserveSig]
        int Drop([MarshalAs(UnmanagedType.Interface)] ComDataObject data, uint keys, NativePoint point, ref uint effect);
    }

    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    private sealed class NativeDropTarget : INativeDropTarget
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly RenderingPanel _owner;
        private WinForms.IDataObject? _data;

        public NativeDropTarget(RenderingPanel owner) => _owner = owner;

        public int DragEnter(ComDataObject data, uint keys, NativePoint point, ref uint effect)
        {
            _data = WrapDragData(data);
            return Dispatch(_owner.OnDragEnter, keys, point, ref effect);
        }

        public int DragOver(uint keys, NativePoint point, ref uint effect) => Dispatch(_owner.OnDragOver, keys, point, ref effect);

        public int DragLeave()
        {
            try
            {
                _owner.OnDragLeave(EventArgs.Empty);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Unable to end viewport drag operation.");
            }

            _data = null;
            return 0;
        }

        public int Drop(ComDataObject data, uint keys, NativePoint point, ref uint effect)
        {
            _data = WrapDragData(data);
            int result = Dispatch(_owner.OnDragDrop, keys, point, ref effect);
            _data = null;
            return result;
        }

        private int Dispatch(Action<WinForms.DragEventArgs> handler, uint keys, NativePoint point, ref uint effect)
        {
            try
            {
                var args = new WinForms.DragEventArgs(_data, (int)keys, point.X, point.Y,
                    (WinForms.DragDropEffects)effect, WinForms.DragDropEffects.None);
                handler(args);
                effect = (uint)args.Effect;
                return 0;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Unable to process viewport drag operation.");
                effect = 0;
                return 0;
            }
        }
    }

    private static WinForms.IDataObject WrapDragData(ComDataObject data) => data switch
    {
        WinForms.IDataObject native => native,
        System.Windows.IDataObject managed => new ManagedDragData(managed),
        _ => new WinForms.DataObject(data)
    };

    private sealed class ManagedDragData : WinForms.IDataObject
    {
        private readonly System.Windows.IDataObject _data;

        public ManagedDragData(System.Windows.IDataObject data) => _data = data;

        public object? GetData(string format, bool autoConvert) => _data.GetData(format, autoConvert);
        public object? GetData(string format) => _data.GetData(format);
        public object? GetData(Type format) => _data.GetData(format);
        public bool GetDataPresent(string format, bool autoConvert) => _data.GetDataPresent(format, autoConvert);
        public bool GetDataPresent(string format) => _data.GetDataPresent(format);
        public bool GetDataPresent(Type format) => _data.GetDataPresent(format);
        public string[] GetFormats(bool autoConvert) => _data.GetFormats(autoConvert);
        public string[] GetFormats() => _data.GetFormats();
        public void SetData(string format, bool autoConvert, object? data) => _data.SetData(format, data, autoConvert);
        public void SetData(string format, object? data) => _data.SetData(format, data);
        public void SetData(Type format, object? data) => _data.SetData(format, data);
        public void SetData(object? data) => _data.SetData(data);
    }

    [DllImport("ole32.dll")]
    private static extern int OleInitialize(IntPtr reserved);

    [DllImport("ole32.dll")]
    private static extern void OleUninitialize();

    [DllImport("ole32.dll")]
    private static extern int RegisterDragDrop(IntPtr window, [MarshalAs(UnmanagedType.Interface)] INativeDropTarget target);

    [DllImport("ole32.dll")]
    private static extern int RevokeDragDrop(IntPtr window);
}
