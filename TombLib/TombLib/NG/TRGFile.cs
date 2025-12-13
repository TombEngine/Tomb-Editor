using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using TombLib.LevelData;

namespace TombLib.NG;

public enum TriggerField
{
	FlipEffect = 9,
	ActionNG = 11,
	ConditionNg = 12
}

public enum ListPrefix
{
	ObjectToTrigger,
	Timer,
	Extra,
	Buttons = Extra
}

public sealed class TRGFile
{
    private static readonly Regex KeyValueRegex = new(@"^\s*(\d+)\s*:\s*(.*)"); // Matches "123:Some text"

    public string[] Lines { get; }

    public TRGFile(string trgFilePath)
        => Lines = File.ReadAllLines(trgFilePath);

    public NgParameterRange GetTriggerFieldSection(TriggerField field)
    {
        string internalNumberPrefix = field switch
        {
            TriggerField.FlipEffect => "F",
            TriggerField.ActionNG => "A",
            TriggerField.ConditionNg => "C",
            _ => string.Empty
        };

        var results = new SortedDictionary<ushort, TriggerParameterUshort>();

        // Get start line index of TRG section
        int sectionStartLineIndex = field switch
        {
            TriggerField.ActionNG => Array.FindIndex(Lines, line => line.StartsWith("<START_TRIGGERWHAT_11", StringComparison.OrdinalIgnoreCase)),
            TriggerField.ConditionNg => Array.FindIndex(Lines, line => line.StartsWith("<START_TRIGGERTYPE_12", StringComparison.OrdinalIgnoreCase)),
            _ => Array.FindIndex(Lines, line => line.StartsWith("<START_TRIGGERWHAT_9", StringComparison.OrdinalIgnoreCase)), // Flip Effect
        };

        if (sectionStartLineIndex == -1)
            return new NgParameterRange(results);

        // Get end line index of TRG section
        int sectionEndLineIndex = Array.FindIndex(Lines, sectionStartLineIndex, line => line.StartsWith("<END", StringComparison.OrdinalIgnoreCase));

        if (sectionEndLineIndex == -1) // If not found, set to end of file
            sectionEndLineIndex = Lines.Length;

        for (int i = sectionStartLineIndex + 1; i < sectionEndLineIndex; i++)
        {
            string line = Lines[i];
            Match match = KeyValueRegex.Match(line);

            if (!match.Success)
                continue; // Not a key-value pair

            if (!ushort.TryParse(match.Groups[1].Value, out ushort internalNumber))
                continue; // Invalid internal number

            // Remove comments and extra spaces
            string name = match.Groups[2].Value
                .Split("#END_DOC#")[0]
                .Split("#START_DOC#")[0]
                .Split("#REMARK#")[0]
                .Trim();

            if (string.IsNullOrEmpty(name))
                continue; // Empty name

            if (!string.IsNullOrEmpty(internalNumberPrefix))
                name = $"{name} ({internalNumberPrefix}{internalNumber})";

            results[internalNumber] = new TriggerParameterUshort(internalNumber, name);
        }

        return new NgParameterRange(results, NgParameterKind.PluginEnumeration);
    }

	public NgParameterRange GetParameterRange(TriggerField field, int internalNumber, ListPrefix prefix)
		=> GetParameterRange(field, internalNumber, prefix, out _);

	public NgParameterRange GetParameterRange(TriggerField field, int internalNumber, ListPrefix prefix, out bool isButtons)
		=> new(GetRangeKeyValues(field, internalNumber, prefix, out NgParameterKind kind, out isButtons), kind);

	private IDictionary<ushort, TriggerParameterUshort> GetRangeKeyValues(
		TriggerField field, int internalNumber, ListPrefix prefix,
		out NgParameterKind kind,
		out bool isButtons)
	{
		kind = NgParameterKind.PluginEnumeration;
		isButtons = false;

		// Get the trigger field name
		string triggerField = field switch
		{
			TriggerField.ActionNG => "ACTION",
			TriggerField.ConditionNg => "CONDITION",
			_ => "EFFECT"
		};

		// Get the start string of the section
		string startsWithString = prefix switch
		{
			ListPrefix.ObjectToTrigger => $"<START_{triggerField}_{internalNumber}_O",
			ListPrefix.Timer => $"<START_{triggerField}_{internalNumber}_T",
			_ => $"<START_{triggerField}_{internalNumber}_E"
		};

		// Find the start line index of the section
		int sectionStartLineIndex = Array.FindIndex(Lines, line => line.StartsWith(startsWithString, StringComparison.OrdinalIgnoreCase));

		if (sectionStartLineIndex == -1) // If not found, check if field is Extra and _B prefix is used
		{
			if (prefix is ListPrefix.Extra)
			{
				startsWithString = $"<START_{triggerField}_{internalNumber}_B";
				sectionStartLineIndex = Array.FindIndex(Lines, line => line.StartsWith(startsWithString, StringComparison.OrdinalIgnoreCase));

				if (sectionStartLineIndex == -1)
					return new Dictionary<ushort, TriggerParameterUshort>();

				isButtons = true;
			}
			else
				return new Dictionary<ushort, TriggerParameterUshort>();
		}

		// Find the end line index of the section
		int sectionEndLineIndex = Array.FindIndex(Lines, sectionStartLineIndex, line => line.StartsWith("<END", StringComparison.OrdinalIgnoreCase));

		if (sectionEndLineIndex == -1) // If not found, set to end of file
			sectionEndLineIndex = Lines.Length;

		return GetKeyValuesFromLines(
			Lines[sectionStartLineIndex..sectionEndLineIndex],
			parseLists: true, // Nested lists are allowed
			out kind);
	}

	private IDictionary<ushort, TriggerParameterUshort> GetKeyValuesFromLines(string[] lines, bool parseLists, out NgParameterKind kind)
	{
		kind = NgParameterKind.PluginEnumeration;
		var results = new SortedDictionary<ushort, TriggerParameterUshort>();

		foreach (string line in lines)
		{
			Match match = KeyValueRegex.Match(line);

			if (!match.Success) // If not a key-value pair, check if it's a list reference
			{
				if (line.StartsWith('#') && parseLists)
				{
					// Get the referenced list of key-values
					foreach (KeyValuePair<ushort, TriggerParameterUshort> item in GetList(line, out kind))
						results.Add(item.Key, item.Value);
				}

				continue;
			}

			if (!ushort.TryParse(match.Groups[1].Value, out ushort internalNumber))
				continue; // Invalid internal number

			string name = match.Groups[2].Value.Trim();

			if (string.IsNullOrEmpty(name))
				continue; // Empty name

			results[internalNumber] = new TriggerParameterUshort(internalNumber, name);
		}

		return results;
	}

	private IDictionary<ushort, TriggerParameterUshort> GetList(string line, out NgParameterKind kind)
	{
		kind = NgParameterKind.PluginEnumeration;
		var results = new SortedDictionary<ushort, TriggerParameterUshort>();

		if (line.StartsWith("#REPEAT#")) // Repeated strings
		{
			string[] tokens = line["#REPEAT#".Length..].Split('#');

			if (tokens.Length is not 3 and not 4) // Invalid format
				return results;

			string radix = tokens[0].Replace("\"", "");

			if (!int.TryParse(tokens[1], out int start) ||
				!int.TryParse(tokens[2], out int end))
				return results;

			int key = start;

			if (tokens.Length == 4 && !int.TryParse(tokens[3], out key)) // #REPEAT#Move up of clicks #1#4#0
				return results;

			for (int i = start; i <= end; i++)
			{
				results.Add(unchecked((ushort)checked((short)i)), new TriggerParameterUshort((ushort)key, radix + i));
				key++;
			}

			return results;
		}
		else if (line.StartsWith("#SAME_OF#=")) // Same as another list
		{
			string value = line["#SAME_OF#=".Length..].Trim();
			string[] tokens = value.Split('_');

			if (tokens.Length != 3)
				return results; // Invalid format

			if (!int.TryParse(tokens[1], out int internalNumber))
				return results; // Invalid internal number

			TriggerField field = tokens[0] switch
			{
				"ACTION" => TriggerField.ActionNG,
				"CONDITION" => TriggerField.ConditionNg,
				_ => TriggerField.FlipEffect
			};

			ListPrefix prefix = tokens[2] switch
			{
				"O" => ListPrefix.ObjectToTrigger,
				"T" => ListPrefix.Timer,
				_ => ListPrefix.Extra
			};

			return GetRangeKeyValues(field, internalNumber, prefix, out kind, out _);
		}

		kind = line switch
		{
			"#ROOMS_255#" => NgParameterKind.Rooms255,
			"#SOUND_EFFECT_A#" => NgParameterKind.SoundEffectsA,
			"#SOUND_EFFECT_B#" => NgParameterKind.SoundEffectsB,
			"#SFX_1024#" => NgParameterKind.Sfx1024,
			"#NG_STRING_LIST_255#" => NgParameterKind.NgStringsList255,
			"#NG_STRING_LIST_ALL#" => NgParameterKind.NgStringsAll,
			"#PSX_STRING_LIST#" => NgParameterKind.PsxStringsList,
			"#PC_STRING_LIST#" => NgParameterKind.PcStringsList,
			"#STRING_LIST_255#" => NgParameterKind.StringsList255,
			"#MOVEABLES#" => NgParameterKind.MoveablesInLevel,
			"#SINK_LIST#" => NgParameterKind.SinksInLevel,
			"#STATIC_LIST#" => NgParameterKind.StaticsInLevel,
			"#FLYBY_LIST#" => NgParameterKind.FlybyCamerasInLevel,
			"#CAMERA_EFFECTS#" => NgParameterKind.CamerasInLevel,
			"#WAD-SLOTS#" or "#LARA_ANIM_SLOT#" => NgParameterKind.WadSlots,
			"#STATIC_SLOTS#" => NgParameterKind.StaticsSlots,
			"#LARA_POS_OCB#" => NgParameterKind.LaraStartPosOcb,
			_ => NgParameterKind.PluginEnumeration
		};

		// Read the list from a file
		string listName = line.Replace("#", "");
		string executablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		string filePath = Path.Combine(executablePath, "Catalogs\\Engines\\TRNG\\" + listName + ".txt");

		if (!File.Exists(filePath))
			return results; // File not found

		string[] lines = File.ReadAllLines(filePath);
		return GetKeyValuesFromLines(lines, parseLists: false, out _); // These files don't have nested lists
	}
}
