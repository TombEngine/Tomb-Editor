using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.Scripting.ClassicScript.Commands;

namespace TombLib.Tests;

[TestClass]
public class ClassicScriptCommandCatalogServiceTests
{
	private readonly ClassicScriptCommandCatalogService _service = new();

	[TestMethod]
	public void IsOldCommand_ClassifiesOldCommands()
	{
		Assert.IsTrue(_service.IsOldCommand("Legend"));
		Assert.IsTrue(_service.IsOldCommand("AnimatingMIP"));
		Assert.IsTrue(_service.IsOldCommand("unknown")); // "Unknown" is a real TRNG command.
		Assert.IsTrue(_service.IsOldCommand("Cut"));     // Legacy array-only names are kept.
		Assert.IsTrue(_service.IsOldCommand("File"));
		Assert.IsTrue(_service.IsOldCommand("Layer2"));
	}

	[TestMethod]
	public void IsOldCommand_ClassifiesNewCommandsAsNotOld()
	{
		Assert.IsFalse(_service.IsOldCommand("AddEffect"));
		Assert.IsFalse(_service.IsOldCommand("Animation"));
		Assert.IsFalse(_service.IsOldCommand("FMV")); // Listed in both lists; new wins.
	}

	[TestMethod]
	public void IsNewCommand_ClassifiesKnownNewAndFallbackCommands()
	{
		Assert.IsTrue(_service.IsNewCommand("AddEffect"));
		Assert.IsTrue(_service.IsNewCommand("FMV"));
		Assert.IsTrue(_service.IsNewCommand("#DEFINE"));
		Assert.IsTrue(_service.IsNewCommand("#FIRST_ID"));
		Assert.IsTrue(_service.IsNewCommand("#INCLUDE"));
		Assert.IsTrue(_service.IsNewCommand("definitely_not_a_real_command")); // Absent from both lists -> new.
		Assert.IsFalse(_service.IsNewCommand("Legend"));
		Assert.IsFalse(_service.IsNewCommand("Unknown"));
	}

	[TestMethod]
	public void CommandCatalog_ContainsRequiredCommandsAndValidMetadata()
	{
		Assert.IsTrue(_service.NewCommands.Contains("#DEFINE", StringComparer.OrdinalIgnoreCase));
		Assert.IsTrue(_service.NewCommands.Contains("#FIRST_ID", StringComparer.OrdinalIgnoreCase));
		Assert.IsTrue(_service.NewCommands.Contains("#INCLUDE", StringComparer.OrdinalIgnoreCase));
		Assert.IsTrue(_service.NewCommands.Contains("FMV", StringComparer.OrdinalIgnoreCase));
		Assert.IsTrue(_service.OldCommands.Contains("Legend", StringComparer.OrdinalIgnoreCase));
		Assert.IsTrue(_service.OldCommands.Contains("AnimatingMIP", StringComparer.OrdinalIgnoreCase));

		AssertCatalogNamesAreUniqueAndNonempty(_service.NewCommands);
		AssertCatalogNamesAreUniqueAndNonempty(_service.OldCommands);

		ClassicScriptCommandsCatalog catalog = new ClassicScriptCommandsLoader().Load();
		Assert.IsTrue(catalog.Commands.Count > 0);
		Assert.IsTrue(catalog.Sections.Count > 0);
		Assert.IsTrue(catalog.Sections.All(section => !string.IsNullOrWhiteSpace(section)));
		Assert.AreEqual(
			catalog.Sections.Count,
			new HashSet<string>(catalog.Sections, StringComparer.OrdinalIgnoreCase).Count);
		Assert.IsTrue(catalog.Commands.All(command => !string.IsNullOrWhiteSpace(command.Name)));
		Assert.IsTrue(catalog.Commands.All(command => Enum.IsDefined(typeof(ClassicScriptCommandKind), command.Kind)));
		Assert.IsTrue(catalog.Commands.All(command => command.Syntaxes.All(syntax =>
			!string.IsNullOrWhiteSpace(syntax.Key) && !string.IsNullOrWhiteSpace(syntax.Text))));

		foreach (IGrouping<ClassicScriptCommandKind, ClassicScriptCommandEntry> commandsByKind in catalog.Commands.GroupBy(command => command.Kind))
		{
			Assert.AreEqual(
				commandsByKind.Count(),
				commandsByKind.Select(command => command.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
				$"Duplicate command name in {commandsByKind.Key} catalog.");
		}

	}

	[TestMethod]
	public void Sections_ContainRequiredSections()
	{
		string[] expected =
		[
			"Language", "Level", "Options", "PCExtensions", "PSXExtensions", "Title",
			"Strings", "PSXStrings", "PCStrings", "ExtraNG"
		];

		foreach (string section in expected)
			Assert.IsTrue(_service.Sections.Contains(section, StringComparer.OrdinalIgnoreCase), section);
	}

	[TestMethod]
	public void Lookups_AreCaseInsensitive()
	{
		Assert.IsTrue(_service.IsOldCommand("legend"));
		Assert.IsTrue(_service.IsOldCommand("LEGEND"));
		Assert.IsFalse(_service.IsOldCommand("addeffect"));
		Assert.IsTrue(_service.NewCommands.Contains("addeffect", StringComparer.OrdinalIgnoreCase));
	}

	private static void AssertCatalogNamesAreUniqueAndNonempty(IReadOnlyList<string> names)
	{
		Assert.IsTrue(names.All(name => !string.IsNullOrWhiteSpace(name)));
		Assert.AreEqual(names.Count, names.Distinct(StringComparer.OrdinalIgnoreCase).Count());
	}
}
