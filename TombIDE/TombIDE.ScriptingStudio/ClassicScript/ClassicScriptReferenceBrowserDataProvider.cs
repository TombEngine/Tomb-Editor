using System;
using System.Collections.Generic;
using System.Data;
using TombLib.Scripting.ClassicScript.Commands;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.ReferenceTables;

namespace TombIDE.ScriptingStudio.ClassicScript;

internal sealed class ClassicScriptReferenceBrowserDataProvider
{
	private readonly ClassicScriptMnemonicCatalogService _mnemonicCatalogService = new();
	private readonly ClassicScriptReferenceTableService _referenceTableService = new();
	private readonly ClassicScriptCommandCatalogService _commandCatalogService = new();

	public DataTable GetTable(ReferenceItemType itemType)
	{
		if (itemType == ReferenceItemType.MnemonicConstants)
			return _mnemonicCatalogService.CreateMnemonicTable();

		if (itemType == ReferenceItemType.OldCommandsList)
			return CreateCommandListTable(_commandCatalogService.OldCommands);

		if (itemType == ReferenceItemType.NewCommandsList)
			return CreateCommandListTable(_commandCatalogService.NewCommands);

		return _referenceTableService.GetTable(itemType.ToString());
	}

	// The legacy command list XML files stored names with a trailing '=' except for
	// directives (#DEFINE, ...) and the "Unknown" command; keep that display.
	private static DataTable CreateCommandListTable(IEnumerable<string> commandNames)
	{
		var table = new DataTable();
		table.Columns.Add("Name", typeof(string));

		foreach (string commandName in commandNames)
			table.Rows.Add(FormatCommandName(commandName));

		return table;
	}

	private static string FormatCommandName(string commandName)
	{
		return commandName.StartsWith('#') || commandName.Equals("Unknown", StringComparison.OrdinalIgnoreCase)
			? commandName
			: commandName + "=";
	}
}
