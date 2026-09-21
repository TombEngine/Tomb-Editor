using System.Data;

namespace TombLib.Scripting.ClassicScript.ReferenceTables;

/// <summary>
/// Loads the JSON reference tables used by the Reference Browser.
/// The <see cref="GetTable"/> entry point is retained while TombIDE consumers
/// (e.g. <c>ClassicScriptReferenceBrowserDataProvider</c>) depend on it.
/// </summary>
public sealed class ClassicScriptReferenceTableService
{
	private readonly ClassicScriptReferenceTableLoader _loader = new();

	/// <summary>
	/// Loads the reference table for the given resource name.
	/// </summary>
	/// <param name="tableName">The reference table resource name (without extension).</param>
	public DataTable GetTable(string tableName)
		=> _loader.Load(ClassicScriptResourcePaths.GetResourcePath(tableName + ".json"));
}
