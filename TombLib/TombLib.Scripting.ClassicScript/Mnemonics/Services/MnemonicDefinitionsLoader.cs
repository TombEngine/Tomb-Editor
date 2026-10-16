using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using TombLib.Scripting.ClassicScript.Mnemonics.Models;
using TombLib.Scripting.ClassicScript.ReferenceTables;

namespace TombLib.Scripting.ClassicScript.Mnemonics.Services;

/// <summary>
/// Loads mnemonic definitions from the standard constants resource and the plugin scripts.
/// </summary>
public sealed class MnemonicDefinitionsLoader
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private readonly ClassicScriptReferenceTableLoader _referenceTableLoader = new();

	/// <summary>
	/// Loads the mnemonic definitions from the given paths.
	/// </summary>
	/// <param name="mnemonicConstantsJsonPath">The path of the mnemonic constants resource.</param>
	/// <param name="pluginScriptsDirectoryPath">The directory that contains the plugin scripts.</param>
	/// <returns>The loaded mnemonic definitions.</returns>
	public MnemonicDefinitions Load(string mnemonicConstantsJsonPath, string pluginScriptsDirectoryPath) => new(
		LoadStandardConstants(mnemonicConstantsJsonPath),
		LoadPluginMnemonics(pluginScriptsDirectoryPath));

	private IReadOnlyList<MnemonicConstantDefinition> LoadStandardConstants(string mnemonicConstantsJsonPath)
	{
		try
		{
			DataTable table = _referenceTableLoader.Load(mnemonicConstantsJsonPath);

			var definitions = new List<MnemonicConstantDefinition>();

			foreach (DataRow row in table.Rows)
			{
				var definition = new MnemonicConstantDefinition(
					CellText(row["decimal"]),
					CellText(row["hex"]),
					CellText(row["flag"]));

				if (!string.IsNullOrWhiteSpace(definition.FlagName))
					definitions.Add(definition);
			}

			return definitions;
		}
		catch (Exception exception)
		{
			Log.Warn(exception, "Failed to load standard mnemonics from '{Path}'; using an empty list.", mnemonicConstantsJsonPath);
			return [];
		}
	}

	private static string CellText(object? value)
		=> value is null || value == DBNull.Value ? string.Empty : value.ToString() ?? string.Empty;

	private static IReadOnlyList<PluginMnemonicDefinition> LoadPluginMnemonics(string pluginScriptsDirectoryPath)
	{
		var pluginMnemonics = new List<PluginMnemonicDefinition>();

		try
		{
			foreach (string file in Directory.GetFiles(pluginScriptsDirectoryPath, "plugin_*.script", SearchOption.TopDirectoryOnly))
			{
				string[] lines = File.ReadAllLines(file);

				foreach (string line in lines)
				{
					if (line.StartsWith("<start_constants>", StringComparison.OrdinalIgnoreCase))
						continue;

					if (line.StartsWith("<end>", StringComparison.OrdinalIgnoreCase))
						break;

					if (string.IsNullOrWhiteSpace(line))
						continue;

					int decimalStart = line.IndexOf(':');

					if (decimalStart == -1)
						continue;

					int descriptionStart = line.IndexOf(';');
					string decimalString;
					string description = string.Empty;

					if (descriptionStart != -1)
					{
						string[] descriptionLines = line.Substring(descriptionStart + 1).Trim().Split('>');
						description = string.Join(Environment.NewLine, descriptionLines.Select(descriptionLine => descriptionLine.TrimEnd()));

						int decimalLength = descriptionStart - decimalStart - 1;
						decimalString = decimalLength > 0 ? line.Substring(decimalStart + 1, decimalLength).Trim() : string.Empty;
					}
					else
					{
						decimalString = line.Substring(decimalStart + 1).Trim();
					}

					try
					{
						short decimalValue;

						if (!short.TryParse(decimalString, out decimalValue))
							decimalValue = Convert.ToInt16(decimalString.Replace("$", string.Empty), 16);

						pluginMnemonics.Add(new PluginMnemonicDefinition(
							line.Substring(0, decimalStart).Trim(),
							description,
							decimalValue,
							FormatHexValue(decimalValue)));
					}
					catch (Exception exception)
					{
						Log.Warn(exception, "Failed to parse a mnemonic line in '{File}'.", file);
					}
				}
			}

			return pluginMnemonics;
		}
		catch (Exception exception)
		{
			Log.Warn(exception, "Failed to load plugin mnemonics from '{Directory}'; using an empty list.", pluginScriptsDirectoryPath);
			return [];
		}
	}

	private static string FormatHexValue(short decimalValue)
	{
		string hexValue = decimalValue.ToString("X");
		int zerosToAdd = 4 - hexValue.Length;

		return $"${new string('0', zerosToAdd)}{hexValue}";
	}
}
