using Nickelony.IDEKit.IntelliSense.Hover;
using System;
using System.Linq;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.GameFlowScript.Hover;
using TombLib.Scripting.GameFlowScript.Types;

namespace TombLib.Tests;

[TestClass]
public class GameFlowHoverProviderTests
{
	[TestMethod]
	public void PropertyHover_UsesTypedIdentifier()
	{
		string property = GameFlowDefinitionCatalog.Properties.First(name =>
			!string.IsNullOrWhiteSpace(name)
			&& !GameFlowDefinitionCatalog.Sections.Contains(name, StringComparer.OrdinalIgnoreCase)
			&& !GameFlowDefinitionCatalog.SpecialProperties.Contains(name, StringComparer.OrdinalIgnoreCase));

		AssertHoverIdentifier(property, ObjectType.Property);
	}

	[TestMethod]
	public void ConstantHover_UsesTypedIdentifier()
	{
		string constant = GameFlowDefinitionCatalog.Constants.First(name =>
			!string.IsNullOrWhiteSpace(name)
			&& !GameFlowDefinitionCatalog.Sections.Contains(name, StringComparer.OrdinalIgnoreCase)
			&& !GameFlowDefinitionCatalog.SpecialProperties.Contains(name, StringComparer.OrdinalIgnoreCase)
			&& !GameFlowDefinitionCatalog.Properties.Contains(name, StringComparer.OrdinalIgnoreCase));

		AssertHoverIdentifier(constant, ObjectType.Constant);
	}

	[TestMethod]
	public void SectionHover_StillUsesTypedSectionIdentifier()
	{
		string section = GameFlowDefinitionCatalog.Sections.First(name => !string.IsNullOrWhiteSpace(name));
		AssertHoverIdentifier(section, ObjectType.Section);
	}

	private static void AssertHoverIdentifier(string symbol, ObjectType expectedIdentifier)
	{
		var hoverProvider = new GameFlowHoverProvider();
		string text = $"LEVEL: {symbol}";
		int offset = text.IndexOf(symbol, StringComparison.Ordinal) + symbol.Length / 2;

		TextHoverInfo? hoverInfo = hoverProvider.GetHoverInfo(new TextHoverRequest(text, offset));

		Assert.IsNotNull(hoverInfo);
		Assert.AreEqual(new GameFlowObjectDiscriminator(expectedIdentifier), hoverInfo.Identifier);
	}
}
