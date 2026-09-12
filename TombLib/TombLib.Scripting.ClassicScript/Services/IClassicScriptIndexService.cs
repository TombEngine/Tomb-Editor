using Nickelony.IDEKit.Core.Text;

namespace TombLib.Scripting.ClassicScript.Services;

/// <summary>
/// Provides free-index discovery for ClassicScript commands that use sequential numbering
/// (TriggerGroup, GlobalTrigger, Organizer, Plugin, etc.).
/// </summary>
public interface IClassicScriptIndexService
{
	/// <summary>
	/// Gets the next free index for the command at the specified offset.
	/// The command key is resolved from the source text at the given offset.
	/// Returns -1 if the command does not support index discovery.
	/// </summary>
	int GetNextFreeIndex(ITextSnapshot source, int offset);

	/// <summary>
	/// Gets the next free index for the specified command key within the section
	/// that contains the given offset.
	/// Returns -1 if the command key is null, empty, or does not support index discovery.
	/// </summary>
	int GetNextFreeIndex(ITextSnapshot source, int offset, string? commandKey);
}
