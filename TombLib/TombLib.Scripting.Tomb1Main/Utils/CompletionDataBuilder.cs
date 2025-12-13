#nullable enable

using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Collections.Generic;
using TombLib.Scripting.Tomb1Main.Enums;
using TombLib.Scripting.Tomb1Main.Objects;

namespace TombLib.Scripting.Tomb1Main.Utils;

/// <summary>
/// Helper class to build completion data while avoiding duplicates.
/// </summary>
public sealed class CompletionDataBuilder
{
	private readonly List<ICompletionData> _data = new();
	private readonly HashSet<string> _addedTexts = new();

	public bool TryAdd(string text, CompletionType type = CompletionType.Generic, string? description = null)
	{
		if (!_addedTexts.Add(text))
			return false;

		_data.Add(new TRXCompletionData(text, type, description));
		return true;
	}

	public List<ICompletionData> Build() => _data;
}
