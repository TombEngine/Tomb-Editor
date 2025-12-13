#nullable enable

using ICSharpCode.AvalonEdit.Document;

namespace TombLib.Scripting.Tomb1Main.Services;

public interface IGameflowHoverService
{
	string? GetHoverInfo(TextDocument document, int offset);
}
