#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace TombIDE.ScriptingStudio.ClassicScript;

/// <summary>
/// Represents a single tab in the reference info viewer.
/// </summary>
public partial class ReferenceInfoTab : ObservableObject
{
	public string Keyword { get; }
	public string Description { get; }

	public ReferenceInfoTab(string keyword, string description)
	{
		Keyword = keyword;
		Description = description;
	}
}

/// <summary>
/// ViewModel for the ClassicScript Reference Info dialog.
/// Displays reference information in a tabbed viewer.
/// </summary>
public partial class ReferenceInfoViewModel : ObservableObject
{
	private readonly Func<bool> _getAlwaysOnTop;
	private readonly Func<bool> _getCloseTabsOnClose;
	private readonly Action<bool> _setAlwaysOnTop;
	private readonly Action<bool> _setCloseTabsOnClose;

	[ObservableProperty]
	private bool _alwaysOnTop;

	[ObservableProperty]
	private bool _closeTabsOnClose;

	[ObservableProperty]
	private string _title = "Reference Info";

	public ObservableCollection<ReferenceInfoTab> Tabs { get; } = [];

	public bool WasAlreadyOpened { get; set; }

	public ReferenceInfoViewModel(
		Func<bool> getAlwaysOnTop,
		Action<bool> setAlwaysOnTop,
		Func<bool> getCloseTabsOnClose,
		Action<bool> setCloseTabsOnClose)
	{
		ArgumentNullException.ThrowIfNull(getAlwaysOnTop);
		ArgumentNullException.ThrowIfNull(setAlwaysOnTop);
		ArgumentNullException.ThrowIfNull(getCloseTabsOnClose);
		ArgumentNullException.ThrowIfNull(setCloseTabsOnClose);

		_getAlwaysOnTop = getAlwaysOnTop;
		_setAlwaysOnTop = setAlwaysOnTop;
		_getCloseTabsOnClose = getCloseTabsOnClose;
		_setCloseTabsOnClose = setCloseTabsOnClose;

		ApplySettings();
	}

	/// <summary>
	/// Shows the given reference info, adding a new tab or selecting an existing one.
	/// </summary>
	public void ShowReferenceInfo(string keyword, string description)
	{
		ApplySettings();

		ReferenceInfoTab? existingTab = Tabs.FirstOrDefault(
			t => t.Keyword.Equals(keyword, StringComparison.OrdinalIgnoreCase));

		if (existingTab is not null)
		{
			SelectedTab = existingTab;
			return;
		}

		var newTab = new ReferenceInfoTab(keyword.ToUpperInvariant(), description);
		Tabs.Add(newTab);
		SelectedTab = newTab;
	}

	/// <summary>
	/// Reloads settings from the backing delegates.
	/// </summary>
	public void ApplySettings()
	{
		AlwaysOnTop = _getAlwaysOnTop();
		CloseTabsOnClose = _getCloseTabsOnClose();
	}

	/// <summary>
	/// Removes all tabs if the close-tabs setting is enabled.
	/// Called when the dialog is hidden.
	/// </summary>
	public void OnHidden()
	{
		if (CloseTabsOnClose)
			Tabs.Clear();

		WasAlreadyOpened = false;
	}

	// -- Bound properties --

	[ObservableProperty]
	private ReferenceInfoTab? _selectedTab;

	partial void OnAlwaysOnTopChanged(bool value)
	{
		_setAlwaysOnTop(value);
	}

	partial void OnCloseTabsOnCloseChanged(bool value)
	{
		_setCloseTabsOnClose(value);
	}

	partial void OnSelectedTabChanged(ReferenceInfoTab? value)
	{
		if (value is not null)
			Title = "Information about " + value.Keyword;
		else
			Title = "Reference Info";
	}

	// -- Commands --

	[RelayCommand]
	private void CloseTab(ReferenceInfoTab? tab)
	{
		if (tab is null)
			return;

		Tabs.Remove(tab);

		if (Tabs.Count == 0)
			RequestHide?.Invoke();
	}

	/// <summary>
	/// Raised when all tabs have been closed and the dialog should hide.
	/// </summary>
	public event Action? RequestHide;
}
