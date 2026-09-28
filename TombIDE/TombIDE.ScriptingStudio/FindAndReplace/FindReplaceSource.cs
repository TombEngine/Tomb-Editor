#nullable enable

using System.Collections;
using System.Collections.Generic;
using System;

namespace TombIDE.ScriptingStudio.FindAndReplace;

/// <summary>
/// A named collection of find-and-replace matches for one document.
/// </summary>
/// <remarks>
/// <para>
/// A search builds the source with <see cref="Add"/>, <see cref="AddRange"/>, and <see cref="Clear"/>;
/// consumers that only read results use the <see cref="IReadOnlyList{T}"/> view the type implements.
/// </para>
/// <para>
/// The intended model is a single-threaded producer that builds the list and consumers that read it
/// through the <see cref="IReadOnlyList{T}"/> view; the type is not thread-safe.
/// <see cref="AddRange"/> adds items one by one, so an enumerator that throws mid-sequence leaves
/// the source populated with the items added before the failure; the call is not atomic.
/// </para>
/// </remarks>
public sealed class FindReplaceSource : IReadOnlyList<FindReplaceItem>
{
	private readonly List<FindReplaceItem> _items = [];
	private string _name;

	/// <summary>
	/// Gets or sets the display name of the document the matches belong to.
	/// </summary>
	/// <exception cref="ArgumentNullException">The assigned value is <see langword="null"/>.</exception>
	public string Name
	{
		get => _name;
		set
		{
			ArgumentNullException.ThrowIfNull(value);
			_name = value;
		}
	}

	/// <summary>
	/// Creates an empty find-and-replace source without a name.
	/// </summary>
	public FindReplaceSource()
		=> _name = string.Empty;

	/// <summary>
	/// Creates a find-and-replace source for the named document.
	/// </summary>
	/// <param name="name">The display name of the document.</param>
	/// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
	public FindReplaceSource(string name)
	{
		ArgumentNullException.ThrowIfNull(name);
		_name = name;
	}

	/// <summary>
	/// Gets the number of matches in the source.
	/// </summary>
	public int Count => _items.Count;

	/// <summary>
	/// Gets the match at the specified index.
	/// </summary>
	/// <param name="index">The zero-based index of the match.</param>
	/// <returns>The match at the specified index.</returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative or not less than <see cref="Count"/>.</exception>
	public FindReplaceItem this[int index] => _items[index];

	/// <summary>
	/// Adds a match to the end of the source.
	/// </summary>
	/// <param name="item">The match to add.</param>
	/// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null"/>.</exception>
	public void Add(FindReplaceItem item)
	{
		ArgumentNullException.ThrowIfNull(item);
		_items.Add(item);
	}

	/// <summary>
	/// Adds the supplied matches to the end of the source.
	/// </summary>
	/// <param name="items">The matches to add.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="items"/> or one of its entries is <see langword="null"/>.
	/// </exception>
	public void AddRange(IEnumerable<FindReplaceItem> items)
	{
		ArgumentNullException.ThrowIfNull(items);

		foreach (FindReplaceItem item in items)
			Add(item);
	}

	/// <summary>
	/// Removes all matches from the source.
	/// </summary>
	public void Clear()
		=> _items.Clear();

	/// <summary>
	/// Returns an enumerator over the matches in order.
	/// </summary>
	/// <returns>The enumerator over the matches.</returns>
	public IEnumerator<FindReplaceItem> GetEnumerator()
		=> _items.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator()
		=> GetEnumerator();
}
