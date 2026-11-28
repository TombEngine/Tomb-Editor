#nullable enable

using System;

namespace TombLib.Input;

/// <summary>
/// Provides access to clipboard and drag data without a window framework dependency.
/// </summary>
public interface IDataTransfer
{
    /// <summary>Checks whether a named data format is available.</summary>
    bool GetDataPresent(string format);
    /// <summary>Checks whether a data type is available.</summary>
    bool GetDataPresent(Type format);
    /// <summary>Checks a named format with optional format conversion.</summary>
    bool GetDataPresent(string format, bool autoConvert);
    /// <summary>Gets data in a named format.</summary>
    object? GetData(string format);
    /// <summary>Gets data in a named format with optional conversion.</summary>
    object? GetData(string format, bool autoConvert);
    /// <summary>Gets data matching a type.</summary>
    object? GetData(Type format);
    /// <summary>Gets the available format names.</summary>
    string[] GetFormats();
    /// <summary>Gets the available formats with optional converted formats.</summary>
    string[] GetFormats(bool autoConvert);
    /// <summary>Stores data in a named format.</summary>
    void SetData(string format, object data);
    /// <summary>Stores data and controls automatic format conversion.</summary>
    void SetData(string format, bool autoConvert, object data);
    /// <summary>Stores data under a type.</summary>
    void SetData(Type format, object data);
    /// <summary>Stores data using its runtime type.</summary>
    void SetData(object data);
    /// <summary>Checks whether the transfer includes file paths.</summary>
    bool ContainsFileDropList();
    /// <summary>Gets the transferred file paths.</summary>
    string[] GetFileDropList();
}
