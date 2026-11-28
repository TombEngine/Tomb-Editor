using System;
using System.Windows;
using TombLib.Input;

namespace TombLib.WPF.Input;

/// <summary>Adapts WPF clipboard and native drag data to editor transfer contracts.</summary>
public sealed class WpfDataTransfer : IDataTransfer
{
    private readonly IDataObject _data;

    /// <summary>Creates an empty data transfer object.</summary>
    public WpfDataTransfer() : this(new DataObject()) { }

    /// <summary>Wraps existing WPF data.</summary>
    public WpfDataTransfer(IDataObject data) => _data = data;

    /// <summary>Gets the WPF data object for clipboard and drag operations.</summary>
    public IDataObject Data => _data;

    public object? GetData(string format, bool autoConvert) => _data.GetData(format, autoConvert);
    public object? GetData(string format) => _data.GetData(format);
    public object? GetData(Type format) => _data.GetData(format);
    public bool GetDataPresent(string format, bool autoConvert) => _data.GetDataPresent(format, autoConvert);
    public bool GetDataPresent(string format) => _data.GetDataPresent(format);
    public bool GetDataPresent(Type format) => _data.GetDataPresent(format);
    public string[] GetFormats(bool autoConvert) => _data.GetFormats(autoConvert);
    public string[] GetFormats() => _data.GetFormats();
    public void SetData(string format, bool autoConvert, object data) => _data.SetData(format, data, autoConvert);
    public void SetData(string format, object data) => _data.SetData(format, data);
    public void SetData(Type format, object data) => _data.SetData(format, data);
    public void SetData(object data) => _data.SetData(data);
    public bool ContainsFileDropList() => _data.GetDataPresent(DataFormats.FileDrop);
    public string[] GetFileDropList() => _data.GetData(DataFormats.FileDrop) as string[] ?? [];
}
