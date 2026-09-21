using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using TombLib.Scripting.ClassicScript.Compilers;
using TombLib.Scripting.ClassicScript.Mnemonics.Models;
using TombLib.Scripting.ClassicScript.Mnemonics.Services;

namespace TombLib.Scripting.ClassicScript.Mnemonics;

/// <summary>
/// Serves the ClassicScript mnemonic catalog as an immutable snapshot.
/// The snapshot is shared process-wide through a static field so that every catalog instance
/// observes reloads (for example after plugin deployment). The reference is published with
/// volatile semantics and a version counter: a reload replaces the snapshot reference and
/// increments the version under the lock, making the new snapshot visible to readers on other
/// threads without torn reads.
/// </summary>
public sealed class ClassicScriptMnemonicCatalogService
{
	private static readonly object SyncRoot = new();
	private static readonly MnemonicDefinitionsLoader Loader = new();
	private static volatile ClassicScriptMnemonicCatalogSnapshot s_snapshot = LoadSnapshot(ClassicScriptCompilerPaths.Default.InternalNGCDirectory);
	private static int s_snapshotVersion;
	private Regex? _cachedMnemonicRegex;
	private int _cachedSnapshotVersion = -1;

	/// <summary>
	/// Gets the current catalog snapshot version. Callers that cache derived data (for example
	/// highlighting rule sets) can invalidate that cache when the version changes.
	/// </summary>
	internal static int CurrentSnapshotVersion => Volatile.Read(ref s_snapshotVersion);

	/// <summary>
	/// Gets all known mnemonic flags.
	/// </summary>
	public IReadOnlyList<string> GetAllFlags() => s_snapshot.AllFlags;

	/// <summary>
	/// Builds a word-boundary regex that matches any known mnemonic flag, with every flag name
	/// regex-escaped and the pattern compiled. The compiled pattern is cached and rebuilt only
	/// when the catalog snapshot changes.
	/// </summary>
	internal Regex GetMnemonicRegex()
	{
		int snapshotVersion = CurrentSnapshotVersion;

		if (_cachedMnemonicRegex is null || _cachedSnapshotVersion != snapshotVersion)
		{
			_cachedMnemonicRegex = new Regex(@"\b(" + string.Join("|", s_snapshot.AllFlags.Select(Regex.Escape)) + @")\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			_cachedSnapshotVersion = snapshotVersion;
		}

		return _cachedMnemonicRegex;
	}

	/// <summary>
	/// Returns whether the given name is a known mnemonic flag.
	/// </summary>
	public bool ContainsFlag(string? flag)
		=> !string.IsNullOrWhiteSpace(flag) && s_snapshot.EntriesByFlag.ContainsKey(flag);

	/// <summary>
	/// Attempts to resolve a flag to its decimal value.
	/// </summary>
	public bool TryGetDecimalValue(string flag, out int decimalValue)
	{
		decimalValue = 0;

		if (!s_snapshot.EntriesByFlag.TryGetValue(flag, out ClassicScriptMnemonicEntry entry))
			return false;

		return int.TryParse(entry.DecimalValue, out decimalValue);
	}

	/// <summary>
	/// Attempts to resolve a value to its flag name, optionally constraining the match to flags
	/// that start with the given prefix.
	/// </summary>
	public bool TryResolveFlagByValue(string value, bool isHexValue, string? prefix, out string flagName)
	{
		flagName = string.Empty;

		if (string.IsNullOrWhiteSpace(value))
			return false;

		foreach (ClassicScriptMnemonicEntry entry in s_snapshot.Entries)
		{
			string candidateValue = isHexValue ? entry.HexValue : entry.DecimalValue;

			if (!candidateValue.Equals(value, StringComparison.OrdinalIgnoreCase))
				continue;

			if (!string.IsNullOrWhiteSpace(prefix)
				&& !entry.FlagName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			flagName = entry.FlagName;
			return true;
		}

		return false;
	}

	/// <summary>
	/// Attempts to get the description for a flag.
	/// </summary>
	public bool TryGetDescription(string flag, out string description)
	{
		description = string.Empty;

		if (!s_snapshot.EntriesByFlag.TryGetValue(flag, out ClassicScriptMnemonicEntry entry))
			return false;

		description = entry.Description;
		return !string.IsNullOrWhiteSpace(description);
	}

	internal bool TryGetEntry(string flag, out ClassicScriptMnemonicEntry entry)
		=> s_snapshot.EntriesByFlag.TryGetValue(flag, out entry);

	/// <summary>
	/// Attempts to extract the plugin syntax for a plugin-defined flag.
	/// </summary>
	public bool TryGetPluginSyntax(string key, out string syntax)
	{
		syntax = string.Empty;

		if (!s_snapshot.EntriesByFlag.TryGetValue(key, out ClassicScriptMnemonicEntry entry) || !entry.IsPlugin)
			return false;

		string? extractedSyntax = ExtractSyntax(entry.Description);

		if (string.IsNullOrWhiteSpace(extractedSyntax))
			return false;

		syntax = extractedSyntax;
		return true;
	}

	/// <summary>
	/// Creates a copy of the mnemonic data table used for display.
	/// </summary>
	public DataTable CreateMnemonicTable()
		=> s_snapshot.DataTable.Copy();

	/// <summary>
	/// Reloads the catalog from the default NG-C directory.
	/// </summary>
	public void Reload()
		=> Reload(ClassicScriptCompilerPaths.Default.InternalNGCDirectory);

	/// <summary>
	/// Reloads the catalog from the given plugin scripts directory, publishing the new snapshot.
	/// </summary>
	public void Reload(string pluginScriptsDirectoryPath)
	{
		lock (SyncRoot)
		{
			s_snapshot = LoadSnapshot(pluginScriptsDirectoryPath);
			Volatile.Write(ref s_snapshotVersion, s_snapshotVersion + 1);
		}
	}

	private static ClassicScriptMnemonicCatalogSnapshot LoadSnapshot(string pluginScriptsDirectoryPath)
	{
		MnemonicDefinitions definitions = Loader.Load(ClassicScriptResourcePaths.GetMnemonicConstantsPath(), pluginScriptsDirectoryPath);
		var entries = new List<ClassicScriptMnemonicEntry>(definitions.StandardConstants.Count + definitions.PluginConstants.Count);

		foreach (MnemonicConstantDefinition definition in definitions.StandardConstants)
			entries.Add(new ClassicScriptMnemonicEntry(definition.DecimalValue, definition.HexValue, definition.FlagName, string.Empty, false));

		foreach (PluginMnemonicDefinition definition in definitions.PluginConstants)
			entries.Add(new ClassicScriptMnemonicEntry(definition.DecimalValue.ToString(), definition.HexValue, definition.FlagName, definition.Description, true));

		var allFlags = entries.Select(entry => entry.FlagName).ToList();
		var entriesByFlag = new Dictionary<string, ClassicScriptMnemonicEntry>(StringComparer.OrdinalIgnoreCase);

		foreach (ClassicScriptMnemonicEntry entry in entries)
			entriesByFlag[entry.FlagName] = entry;

		return new ClassicScriptMnemonicCatalogSnapshot(entries, allFlags, entriesByFlag, BuildDataTable(entries));
	}

	private static DataTable BuildDataTable(IReadOnlyList<ClassicScriptMnemonicEntry> entries)
	{
		var dataTable = new DataTable();

		dataTable.Columns.Add("decimal", typeof(string));
		dataTable.Columns.Add("hex", typeof(string));
		dataTable.Columns.Add("flag", typeof(string));

		foreach (ClassicScriptMnemonicEntry entry in entries)
			dataTable.Rows.Add(entry.DecimalValue, entry.HexValue, entry.FlagName);

		return dataTable;
	}

	private static string? ExtractSyntax(string description)
	{
		if (string.IsNullOrWhiteSpace(description))
			return null;

		string[] parts = Regex.Split(description, "syntax:", RegexOptions.IgnoreCase);

		if (parts.Length <= 1)
			return null;

		return parts[1].Replace("\r", string.Empty).Split('\n')[0].Trim();
	}
}

internal sealed class ClassicScriptMnemonicCatalogSnapshot(
	IReadOnlyList<ClassicScriptMnemonicEntry> entries,
	IReadOnlyList<string> allFlags,
	IReadOnlyDictionary<string, ClassicScriptMnemonicEntry> entriesByFlag,
	DataTable dataTable)
{
	public IReadOnlyList<ClassicScriptMnemonicEntry> Entries { get; } = entries;

	public IReadOnlyList<string> AllFlags { get; } = allFlags;

	public IReadOnlyDictionary<string, ClassicScriptMnemonicEntry> EntriesByFlag { get; } = entriesByFlag;

	public DataTable DataTable { get; } = dataTable;
}

internal readonly record struct ClassicScriptMnemonicEntry(
	string DecimalValue,
	string HexValue,
	string FlagName,
	string Description,
	bool IsPlugin);
