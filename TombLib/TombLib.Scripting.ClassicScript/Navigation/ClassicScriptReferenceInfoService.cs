using System;
using TombLib.Scripting.ClassicScript.Descriptions;
using TombLib.Scripting.ClassicScript.Mnemonics;

namespace TombLib.Scripting.ClassicScript.Navigation;

/// <summary>
/// Resolves the description and metadata for a ClassicScript reference.
/// </summary>
public sealed class ClassicScriptReferenceInfoService
{
	private readonly ClassicScriptDescriptionCatalogService _descriptionCatalogService = new();
	private readonly ClassicScriptMnemonicCatalogService _mnemonicCatalogService = new();

	/// <summary>
	/// Gets the reference information for the given keyword and reference type.
	/// </summary>
	/// <param name="keyword">The keyword to look up.</param>
	/// <param name="type">The reference type of the keyword.</param>
	/// <returns>The reference information.</returns>
	public ClassicScriptReferenceInfo GetReferenceInfo(string keyword, ReferenceType type)
	{
		string description = GetReferenceDescription(keyword.TrimEnd('='), type);
		string? missingDescriptionMessage = string.IsNullOrEmpty(description)
			? CreateMissingDescriptionMessage(keyword)
			: null;

		return new ClassicScriptReferenceInfo(keyword, description, missingDescriptionMessage);
	}

	private static string CreateMissingDescriptionMessage(string keyword)
	{
		if (keyword.StartsWith('$'))
			return "Couldn't identify the hexadecimal value for the given context.";

		if (int.TryParse(keyword, out _))
			return "Couldn't identify the decimal value for the given context.";

		return "No description found for the " + keyword.ToUpperInvariant() + " flag.";
	}

	private string GetReferenceDescription(string keyword, ReferenceType type)
	{
		string description = type switch
		{
			ReferenceType.MnemonicConstant => _descriptionCatalogService.GetMnemonicConstantDescription(keyword),
			ReferenceType.OldCommand => _descriptionCatalogService.GetOldCommandDescription(keyword),
			ReferenceType.NewCommand => _descriptionCatalogService.GetNewCommandDescription(keyword),
			ReferenceType.OCB => _descriptionCatalogService.GetOcbDescription(keyword),
			_ => string.Empty
		};

		if (string.IsNullOrEmpty(description) && type == ReferenceType.MnemonicConstant)
			return TryGetPluginDescription(keyword);

		return description;
	}

	private string TryGetPluginDescription(string keyword)
	{
		return _mnemonicCatalogService.TryGetDescription(keyword, out string description)
			? description
			: string.Empty;
	}
}

/// <summary>
/// Describes the resolved information for a ClassicScript reference.
/// </summary>
/// <param name="Keyword">The referenced keyword.</param>
/// <param name="Description">The resolved description of the keyword.</param>
/// <param name="MissingDescriptionMessage">The message explaining a missing description, when applicable.</param>
public sealed record ClassicScriptReferenceInfo(string Keyword, string Description, string? MissingDescriptionMessage);
