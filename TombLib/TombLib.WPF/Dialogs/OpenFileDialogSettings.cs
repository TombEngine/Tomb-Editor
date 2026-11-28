namespace TombLib.WPF.Dialogs;

/// <summary>Stores file picker options and the accepted selection.</summary>
public sealed class OpenFileDialogSettings
{
    /// <summary>Gets or sets the dialog title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the native file type filter.</summary>
    public string Filter { get; set; } = string.Empty;

    /// <summary>Gets or sets the initial directory.</summary>
    public string InitialDirectory { get; set; } = string.Empty;

    /// <summary>Gets or sets the initial or accepted file name.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Gets the accepted file names when multiple selection is enabled.</summary>
    public string[] FileNames { get; internal set; } = [];

    /// <summary>Gets or sets whether selected files must already exist.</summary>
    public bool CheckFileExists { get; set; } = true;

    /// <summary>Gets or sets whether multiple files may be selected.</summary>
    public bool Multiselect { get; set; }
}
