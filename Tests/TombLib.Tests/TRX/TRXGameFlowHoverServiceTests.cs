using Nickelony.IDEKit.IntelliSense.Hover;
using TombLib.Scripting.TRX.Hover;
using TombLib.Scripting.TRX.Models;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Tests;

[TestClass]
public class TRXGameFlowHoverServiceTests
{
	private sealed class StubSchemaService(TRXGameFlowSchemaModel? model) : ITRXGameFlowSchemaService
	{
		public TRXSchemaLoadState LoadState => model is null ? TRXSchemaLoadState.InvalidSchema : TRXSchemaLoadState.Loaded;

		public TRXGameFlowSchemaModel? Model => model;

		public TRXSchemaKeywords Keywords => model?.Keywords ?? TRXSchemaKeywords.Empty;
	}

	[TestMethod]
	public void GetHoverInfo_NullModel_ReturnsNull()
	{
		var service = new TRXGameFlowHoverService(new StubSchemaService(null));

		var result = service.GetHoverInfo(new TextHoverRequest("\"name\":", 2));

		Assert.IsNull(result);
	}

	[TestMethod]
	public void GetHoverInfo_KnownProperty_ReturnsMarkdownWithDescription()
	{
		var model = new TRXGameFlowSchemaModel(
			[new TRXGameFlowProperty("name", [TRXGameFlowPropertyType.String], "Human-readable display name.")],
			TRXSchemaKeywords.Empty);
		var service = new TRXGameFlowHoverService(new StubSchemaService(model));

		var result = service.GetHoverInfo(new TextHoverRequest("\"name\":", 2));

		Assert.IsNotNull(result);
		Assert.AreEqual(TextHoverContentKind.Markdown, result.ContentKind);
		Assert.AreEqual("name", result.SymbolName);
		Assert.IsTrue(result.Content.Contains("Human-readable display name."));
	}

	[TestMethod]
	public void GetHoverInfo_ArrayProperty_ShowsArrayType()
	{
		var model = new TRXGameFlowSchemaModel(
			[new TRXGameFlowProperty("levels", [TRXGameFlowPropertyType.Array], "Array of level definitions.")],
			TRXSchemaKeywords.Empty);
		var service = new TRXGameFlowHoverService(new StubSchemaService(model));

		var result = service.GetHoverInfo(new TextHoverRequest("\"levels\":", 2));

		Assert.IsNotNull(result);
		Assert.IsTrue(result.Content.Contains("Type: `Array`"));
		Assert.IsTrue(result.Content.Contains("Array of level definitions."));
	}

	[TestMethod]
	public void GetHoverInfo_UnknownWord_ReturnsNull()
	{
		var model = new TRXGameFlowSchemaModel(
			[new TRXGameFlowProperty("name", [TRXGameFlowPropertyType.String], null)],
			TRXSchemaKeywords.Empty);
		var service = new TRXGameFlowHoverService(new StubSchemaService(model));

		var result = service.GetHoverInfo(new TextHoverRequest("\"unknown\":", 2));

		Assert.IsNull(result);
	}

	[TestMethod]
	public void GetHoverInfo_ValueEqualToKnownProperty_ReturnsNull()
	{
		var model = new TRXGameFlowSchemaModel(
			[new TRXGameFlowProperty("secret", [TRXGameFlowPropertyType.String], "A hidden property.")],
			TRXSchemaKeywords.Empty);
		var service = new TRXGameFlowHoverService(new StubSchemaService(model));

		// Hovering the value position of "secret" (equal to a known property name) yields no
		// hover information because only property-name positions are hoverable.
		var result = service.GetHoverInfo(new TextHoverRequest("\"name\": \"secret\"", 11));

		Assert.IsNull(result);
	}

	[TestMethod]
	public void GetHoverInfo_ColonAfterPropertyName_ReturnsNull()
	{
		var model = new TRXGameFlowSchemaModel(
			[new TRXGameFlowProperty("name", [TRXGameFlowPropertyType.String], null)],
			TRXSchemaKeywords.Empty);
		var service = new TRXGameFlowHoverService(new StubSchemaService(model));

		// Hovering the colon immediately after the property name is outside the property range.
		var result = service.GetHoverInfo(new TextHoverRequest("\"name\":", 6));

		Assert.IsNull(result);
	}
}