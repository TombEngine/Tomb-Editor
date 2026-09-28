using TombLib.Scripting.ClassicScript.Syntaxes;
using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.Scripting.ClassicScript.Commands;

namespace TombLib.Tests;

[TestClass]
public class ClassicScriptSyntaxCatalogServiceTests
{
	[TestMethod]
	public void GetCommandDefinition_UsesCaseInsensitiveLookupAndExposesParsedMetadata()
	{
		var service = new ClassicScriptSyntaxCatalogService();

		ClassicScriptSyntaxDefinition customize = service.GetCommandDefinition("customize")
			?? throw new AssertFailedException("Customize syntax definition was not loaded.");
		ClassicScriptSyntaxDefinition legend = service.GetCommandDefinition("Legend")
			?? throw new AssertFailedException("Legend syntax definition was not loaded.");

		Assert.AreEqual("Level", customize.ApplicableSection);
		Assert.AreEqual(2, customize.ArgumentCount);
		Assert.IsTrue(customize.HasArrayArguments);
		StringAssert.Contains(customize.SyntaxText, "Customize=");

		Assert.AreEqual("Level", legend.ApplicableSection);
		Assert.AreEqual(1, legend.ArgumentCount);
		Assert.IsFalse(legend.HasArrayArguments);
		StringAssert.Contains(legend.SyntaxText, "Legend=");
	}

	[TestMethod]
	public void GetCustomizeAndParameterSyntax_ReturnExpectedEntries()
	{
		var service = new ClassicScriptSyntaxCatalogService();

		string? customizeSyntax = service.GetCustomizeSyntax("cust_bar");
		string? parameterSyntax = service.GetParameterSyntax("param_rect");

		Assert.IsFalse(string.IsNullOrWhiteSpace(customizeSyntax));
		Assert.IsFalse(string.IsNullOrWhiteSpace(parameterSyntax));
		StringAssert.Contains(customizeSyntax, "CUST_BAR");
		StringAssert.Contains(parameterSyntax, "PARAM_RECT");
	}

	[TestMethod]
	public void GetCommandSyntax_ReturnsSyntaxForKnownCommand()
	{
		var service = new ClassicScriptSyntaxCatalogService();

		string? syntax = service.GetCommandSyntax("AddEffect");

		Assert.IsFalse(string.IsNullOrWhiteSpace(syntax));
		StringAssert.Contains(syntax, "AddEffect=");
		StringAssert.Contains(syntax, "(*Array*)");
	}

	[TestMethod]
	public void CommandAndSyntaxCatalogs_AreConsistent()
	{
		var commandCatalog = new ClassicScriptCommandsLoader().Load();
		var syntaxCatalog = new ClassicScriptSyntaxCatalogService();
		IReadOnlyList<ClassicScriptSyntaxDefinition> definitions = syntaxCatalog.GetCommandSyntaxDefinitions();
		HashSet<string> commandSyntaxKeys = commandCatalog.Commands
			.Where(command => command.Kind is ClassicScriptCommandKind.Old or ClassicScriptCommandKind.New)
			.SelectMany(command => command.Syntaxes)
			.Select(syntax => syntax.Key)
			.ToHashSet(StringComparer.OrdinalIgnoreCase);

		Assert.IsTrue(definitions.Count > 0);
		Assert.AreEqual(
			definitions.Count,
			definitions.Select(definition => definition.Key).Distinct(StringComparer.OrdinalIgnoreCase).Count());

		foreach (ClassicScriptSyntaxDefinition definition in definitions)
		{
			Assert.IsFalse(string.IsNullOrWhiteSpace(definition.Key));
			Assert.IsFalse(string.IsNullOrWhiteSpace(definition.SyntaxText));
			Assert.IsTrue(
				definition.ApplicableSection.Equals("Any", StringComparison.OrdinalIgnoreCase)
				|| commandCatalog.Sections.Contains(definition.ApplicableSection, StringComparer.OrdinalIgnoreCase),
				$"Unknown section '{definition.ApplicableSection}' for '{definition.Key}'.");
			Assert.IsTrue(commandSyntaxKeys.Contains(definition.Key), definition.Key);
			Assert.IsNotNull(syntaxCatalog.GetCommandDefinition(definition.Key));
		}
	}
}