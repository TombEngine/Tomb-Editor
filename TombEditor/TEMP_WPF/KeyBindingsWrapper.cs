#nullable enable

using System.Collections.Generic;
using System.ComponentModel;

namespace TombEditor.WPF;

public class KeyBindingsWrapper : INotifyPropertyChanged
{
	public static KeyBindingsWrapper Instance { get; set; } = new KeyBindingsWrapper();

	private const string IndexerName = "Item";
	private const string IndexerArrayName = "Item[]";

	public SortedSet<Hotkey> this[string key] => Editor.Instance.Configuration.UI_Hotkeys[key];

	public event PropertyChangedEventHandler? PropertyChanged;

	public void Invalidate()
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
	}
}
