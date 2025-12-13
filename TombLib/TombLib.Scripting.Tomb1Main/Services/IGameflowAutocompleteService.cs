#nullable enable

using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Collections.Generic;

namespace TombLib.Scripting.Tomb1Main.Services;

public interface IGameflowAutocompleteService
{
	List<ICompletionData> GetAutocompleteData();
}
