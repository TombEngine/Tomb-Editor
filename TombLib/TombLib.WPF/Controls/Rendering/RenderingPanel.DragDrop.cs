#nullable enable

using ComDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;
using NLog;
using System;
using System.Runtime.InteropServices;
using TombLib.Input;
using TombLib.WPF.Input;

namespace TombLib.WPF.Controls.Rendering;

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
        private IDataTransfer? _data;
        private TransferEffects _effect;

        public NativeDropTarget(RenderingPanel owner) => _owner = owner;

        public int DragEnter(ComDataObject data, uint keys, NativePoint point, ref uint effect)
        {
            Reset();
            return Dispatch(_owner.OnDragEnter, keys, point, ref effect, data);
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
            finally
            {
                Reset();
            }

            return 0;
        }

        public int Drop(ComDataObject data, uint keys, NativePoint point, ref uint effect)
        {
            try
            {
                if (_data is null || (_effect & (TransferEffects)effect) == TransferEffects.None)
                {
                    effect = 0;
                    return 0;
                }

                return Dispatch(_owner.OnDragDrop, keys, point, ref effect, data);
            }
            finally
            {
                Reset();
            }
        }

        private int Dispatch(Action<PointerDragEventArgs> handler, uint keys, NativePoint point, ref uint effect, ComDataObject? data = null)
        {
            try
            {
                if (data is not null)
                    _data = WrapDragData(data);

                if (_data is null)
                {
                    effect = 0;
                    return 0;
                }

                var allowedEffect = (TransferEffects)effect;
                var args = new PointerDragEventArgs(_data, (int)keys, point.X, point.Y,
                    allowedEffect, _effect & allowedEffect);
                handler(args);
                _effect = args.Effect & allowedEffect;
                effect = (uint)_effect;
                return 0;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Unable to process viewport drag operation.");
                Reset();
                effect = 0;
                return 0;
            }
        }

        private void Reset()
        {
            _data = null;
            _effect = TransferEffects.None;
        }
    }

    private static IDataTransfer WrapDragData(ComDataObject data)
        => new WpfDataTransfer(new System.Windows.DataObject(data));

    [DllImport("ole32.dll")]
    private static extern int OleInitialize(IntPtr reserved);

    [DllImport("ole32.dll")]
    private static extern void OleUninitialize();

    [DllImport("ole32.dll")]
    private static extern int RegisterDragDrop(IntPtr window, [MarshalAs(UnmanagedType.Interface)] INativeDropTarget target);

    [DllImport("ole32.dll")]
    private static extern int RevokeDragDrop(IntPtr window);
}
