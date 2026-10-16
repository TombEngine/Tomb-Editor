using System;
using System.Collections.Generic;

namespace TombLib.Scripting.UI.Presentation;

/// <summary>
/// Groups the references to a symbol that live in the same file.
/// </summary>
public sealed class TextReferenceGroup
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TextReferenceGroup"/> class.
	/// </summary>
	/// <param name="filePath">The full path of the file.</param>
	/// <param name="displayPath">The display path of the file.</param>
	/// <param name="items">The reference items in the file.</param>
	public TextReferenceGroup(string filePath, string displayPath, IReadOnlyList<TextReferenceListItem> items)
	{
		FilePath = filePath;
		DisplayPath = displayPath;
		Items = Array.AsReadOnly([.. items ?? []]);
	}

	/// <summary>
	/// Gets the full path of the file.
	/// </summary>
	public string FilePath { get; }

	/// <summary>
	/// Gets the display path of the file.
	/// </summary>
	public string DisplayPath { get; }

	/// <summary>
	/// Gets the reference items in the file.
	/// </summary>
	public IReadOnlyList<TextReferenceListItem> Items { get; }

	/// <summary>
	/// Gets the number of reference items in the group.
	/// </summary>
	public int Count => Items.Count;

	/// <summary>
	/// Gets the header text that combines the display path with the item count.
	/// </summary>
	public string Header => $"{DisplayPath} ({Count})";
}
