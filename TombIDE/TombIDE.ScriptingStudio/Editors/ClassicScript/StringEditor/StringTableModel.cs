#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace TombIDE.ScriptingStudio.Editors.ClassicScript.StringEditor;

public enum StringTableMode
{
	Normal,
	ExtraNG
}

public sealed partial class StringTableRow : ObservableObject
{
	[ObservableProperty]
	private int _id;

	[ObservableProperty]
	private string _hexValue = string.Empty;

	[ObservableProperty]
	private string _stringValue = string.Empty;
}

public sealed class StringTableSection
{
	public string SectionName { get; set; } = string.Empty;
	public StringTableMode Mode { get; set; }
	public ObservableCollection<StringTableRow> Rows { get; } = [];

	public bool IsExtraNG => Mode == StringTableMode.ExtraNG;
}
