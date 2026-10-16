#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;

namespace TombIDE.ScriptingStudio.ClassicScript;

public sealed partial class ReferenceBrowserCategoryViewModel : ObservableObject
{
	internal ReferenceBrowserCategoryViewModel(ReferenceItemType itemType, string title)
	{
		ItemType = itemType;
		Title = title;
	}

	internal ReferenceItemType ItemType { get; }

	public string Title { get; }

	[ObservableProperty]
	private bool _isSelected;
}
