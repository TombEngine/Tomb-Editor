using System;
using System.Collections.Generic;
using TombLib.Scripting.TRX.Models;

namespace TombLib.Tests;

[TestClass]
public class TRXGameFlowSchemaModelTests
{
	[TestMethod]
	public void ModelProperties_CannotBeMutatedThroughServiceOwnedState()
	{
		var model = new TRXGameFlowSchemaModel(
			[new TRXGameFlowProperty("title", [TRXGameFlowPropertyType.String], "The title.")],
			TRXSchemaKeywords.Empty);

		// The runtime collection must not be a caller-mutable List or array.
		Assert.IsNotInstanceOfType(model.Properties, typeof(List<TRXGameFlowProperty>));
		Assert.IsNotInstanceOfType(model.Properties, typeof(TRXGameFlowProperty[]));
		Assert.ThrowsException<NotSupportedException>(() =>
			((IList<TRXGameFlowProperty>)model.Properties).Add(new TRXGameFlowProperty("other", [TRXGameFlowPropertyType.String], null)));
	}

	[TestMethod]
	public void SchemaKeywordsLists_CannotBeMutatedThroughServiceOwnedState()
	{
		var keywords = new TRXSchemaKeywords(["collections"], ["properties"], ["constants"]);

		// The runtime collections must not be caller-mutable Lists or arrays.
		Assert.IsNotInstanceOfType(keywords.Collections, typeof(List<string>));
		Assert.IsNotInstanceOfType(keywords.Collections, typeof(string[]));
		Assert.ThrowsException<NotSupportedException>(() => ((IList<string>)keywords.Collections).Add("x"));

		Assert.IsNotInstanceOfType(keywords.Properties, typeof(List<string>));
		Assert.IsNotInstanceOfType(keywords.Properties, typeof(string[]));
		Assert.ThrowsException<NotSupportedException>(() => ((IList<string>)keywords.Properties).Add("x"));

		Assert.IsNotInstanceOfType(keywords.Constants, typeof(List<string>));
		Assert.IsNotInstanceOfType(keywords.Constants, typeof(string[]));
		Assert.ThrowsException<NotSupportedException>(() => ((IList<string>)keywords.Constants).Add("x"));
	}

	[TestMethod]
	public void PropertyTypes_CannotBeMutatedThroughServiceOwnedState()
	{
		var property = new TRXGameFlowProperty("engine", [TRXGameFlowPropertyType.Integer], null);

		// The runtime collection must not be a caller-mutable List or array.
		Assert.IsNotInstanceOfType(property.Types, typeof(List<TRXGameFlowPropertyType>));
		Assert.IsNotInstanceOfType(property.Types, typeof(TRXGameFlowPropertyType[]));
		Assert.ThrowsException<NotSupportedException>(() => ((IList<TRXGameFlowPropertyType>)property.Types).Add(TRXGameFlowPropertyType.String));
	}

	[TestMethod]
	public void IsArray_OnlyTrueForExactlyTheArrayType()
	{
		Assert.IsTrue(new TRXGameFlowProperty("levels", [TRXGameFlowPropertyType.Array], null).IsArray);
		Assert.IsFalse(new TRXGameFlowProperty("name", [TRXGameFlowPropertyType.String], null).IsArray);
		Assert.IsFalse(new TRXGameFlowProperty("mixed", [TRXGameFlowPropertyType.Array, TRXGameFlowPropertyType.Null], null).IsArray);
		Assert.IsFalse(new TRXGameFlowProperty("untyped", [], null).IsArray);
	}

	[TestMethod]
	public void Constructor_ModelProperties_IgnoresLaterCallerMutation()
	{
		var source = new List<TRXGameFlowProperty>
		{
			new("title", [TRXGameFlowPropertyType.String], "The title.")
		};

		var model = new TRXGameFlowSchemaModel(source, TRXSchemaKeywords.Empty);
		source.Add(new TRXGameFlowProperty("added", [TRXGameFlowPropertyType.String], null));

		Assert.AreEqual(1, model.Properties.Count);
		Assert.AreEqual("title", model.Properties[0].Name);
	}

	[TestMethod]
	public void Constructor_SchemaKeywords_IgnoresLaterCallerMutation()
	{
		var collections = new List<string> { "levels" };
		var properties = new List<string> { "name" };
		var constants = new List<string> { "ENGINE_1" };

		var keywords = new TRXSchemaKeywords(collections, properties, constants);
		collections.Add("modified");
		properties.Add("modified");
		constants.Add("modified");

		Assert.AreEqual(1, keywords.Collections.Count);
		Assert.AreEqual("levels", keywords.Collections[0]);
		Assert.AreEqual(1, keywords.Properties.Count);
		Assert.AreEqual("name", keywords.Properties[0]);
		Assert.AreEqual(1, keywords.Constants.Count);
		Assert.AreEqual("ENGINE_1", keywords.Constants[0]);
	}

	[TestMethod]
	public void Constructor_PropertyTypes_IgnoresLaterCallerMutation()
	{
		var types = new List<TRXGameFlowPropertyType> { TRXGameFlowPropertyType.String };

		var property = new TRXGameFlowProperty("name", types, null);
		types.Add(TRXGameFlowPropertyType.Number);

		Assert.AreEqual(1, property.Types.Count);
		Assert.AreEqual(TRXGameFlowPropertyType.String, property.Types[0]);
	}
}