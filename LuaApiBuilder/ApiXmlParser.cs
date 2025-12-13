using LuaApiBuilder.Interfaces;
using LuaApiBuilder.Objects;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace LuaApiBuilder;

public static class ApiXmlParser
{
	public static ApiResult ParseApi(string xmlFilePath)
	{
		var doc = XDocument.Load(xmlFilePath);
		var api = doc.Root!;

		var result = new ApiResult();

		// Parse functions
		foreach (var function in api.Element("functions")?.Elements("function") ?? Enumerable.Empty<XElement>())
		{
			var apiFunction = ParseFunction(function);
			var moduleName = apiFunction.Module;

			if (!result.ModuleTypeFunctions.ContainsKey(moduleName))
				result.ModuleTypeFunctions[moduleName] = new List<ApiFunction>();

			result.ModuleTypeFunctions[moduleName].Add(apiFunction);

			if (!result.AllModules.Contains(moduleName))
				result.AllModules.Add(moduleName);
		}

		// Parse classes
		foreach (var cls in api.Element("classes")?.Elements("class") ?? Enumerable.Empty<XElement>())
		{
			var apiClass = ParseClass(cls);
			var moduleName = GetModuleFromTypeName(apiClass.Name);

			if (!result.ModuleClasses.ContainsKey(moduleName))
				result.ModuleClasses[moduleName] = new List<ApiClass>();

			result.ModuleClasses[moduleName].Add(apiClass);

			if (!result.AllModules.Contains(moduleName))
				result.AllModules.Add(moduleName);
		}

		// Parse enums
		foreach (var enumElement in api.Element("enums")?.Elements("enum") ?? Enumerable.Empty<XElement>())
		{
			var apiEnum = ParseEnum(enumElement);
			var moduleName = GetModuleFromTypeName(apiEnum.Name);

			if (!result.ModuleEnums.ContainsKey(moduleName))
				result.ModuleEnums[moduleName] = new List<ApiEnum>();

			result.ModuleEnums[moduleName].Add(apiEnum);

			if (!result.AllModules.Contains(moduleName))
				result.AllModules.Add(moduleName);
		}

		return result;
	}

	private static ApiFunction ParseFunction(XElement function)
	{
		var apiFunction = new ApiFunction
		{
			Module = function.Element("module")?.Value ?? string.Empty,
			Name = function.Element("name")?.Value ?? string.Empty,
			Summary = function.Element("summary")?.Value ?? string.Empty,
			Description = function.Element("description")?.Value ?? string.Empty,
			Caller = function.Element("caller")?.Value
		};

		ParseParameters(function, apiFunction);
		ParseReturns(function, apiFunction);

		return apiFunction;
	}

	private static ApiClass ParseClass(XElement cls)
	{
		bool hasConstructor = true;

		if (bool.TryParse(cls.Element("ctor")?.Value, out var result))
			hasConstructor = result;

		var apiClass = new ApiClass
		{
			Name = cls.Element("name")?.Value ?? string.Empty,
			Type = cls.Element("type")?.Value ?? string.Empty,
			Summary = cls.Element("summary")?.Value ?? string.Empty,
			Description = cls.Element("description")?.Value ?? string.Empty,
			Inherits = cls.Element("inherits")?.Value ?? string.Empty,
			HasConstructor = hasConstructor
		};

		// Parse members
		var membersElement = cls.Element("members");

		if (membersElement is null)
			return apiClass; // No members to parse

		// Parse methods
		foreach (var method in membersElement.Elements("method"))
		{
			var apiMethod = new ApiMethod
			{
				Name = method.Element("name")?.Value ?? string.Empty,
				Summary = method.Element("summary")?.Value ?? string.Empty,
				Description = method.Element("description")?.Value ?? string.Empty
			};

			ParseParameters(method, apiMethod);
			ParseReturns(method, apiMethod);

			apiClass.Methods.Add(apiMethod);
		}

		// Parse fields
		foreach (var field in membersElement.Elements("field"))
		{
			var apiField = new ApiField
			{
				Name = field.Element("name")?.Value ?? string.Empty,
				Type = field.Element("type")?.Value ?? string.Empty,
				Summary = field.Element("summary")?.Value ?? string.Empty,
				Description = field.Element("description")?.Value ?? string.Empty
			};

			if (string.IsNullOrWhiteSpace(apiField.Type))
			{
				// If type is not specified, try to extract it from the summary
				apiField.Type = ExtractTypeFromSummary(apiField);
			}

			if (string.IsNullOrWhiteSpace(apiField.Type))
			{
				// If type is still not specified, try to extract it from the description
				apiField.Type = ExtractTypeFromDescription(apiField);
			}

			apiClass.Fields.Add(apiField);
		}

		return apiClass;
	}

	private static ApiEnum ParseEnum(XElement enumElement)
	{
		var apiEnum = new ApiEnum
		{
			Name = enumElement.Element("name")?.Value ?? string.Empty,
			Summary = enumElement.Element("summary")?.Value ?? string.Empty,
			Description = enumElement.Element("description")?.Value ?? string.Empty
		};

		// Parse values
		var valuesElement = enumElement.Element("values");

		if (valuesElement is null)
			return apiEnum; // No values to parse

		foreach (var value in valuesElement.Elements("value"))
		{
			apiEnum.Values.Add(new ApiEnumValue
			{
				Name = value.Element("name")?.Value ?? string.Empty,
				Summary = value.Element("summary")?.Value ?? string.Empty,
				Description = value.Element("description")?.Value ?? string.Empty
			});
		}

		return apiEnum;
	}

	private static void ParseParameters(XElement function, IApiFunction target)
	{
		var parametersElement = function.Element("parameters");

		if (parametersElement is null)
			return; // No parameters to parse

		foreach (var param in parametersElement.Elements("parameter"))
		{
			target.Parameters.Add(new ApiParameter
			{
				Name = param.Element("name")?.Value ?? string.Empty,
				Type = param.Element("type")?.Value ?? string.Empty,
				Description = param.Element("description")?.Value ?? string.Empty,
				Optional = bool.TryParse(param.Element("optional")?.Value, out var optional) && optional,
				DefaultValue = param.Element("defaultValue")?.Value ?? string.Empty
			});
		}
	}

	private static void ParseReturns(XElement function, IApiFunction target)
	{
		var returnsElement = function.Element("returns");

		if (returnsElement is null)
			return; // No return values to parse

		foreach (var ret in returnsElement.Elements("return"))
		{
			target.Returns.Add(new ApiReturn
			{
				Type = ret.Element("type")?.Value ?? string.Empty,
				Description = ret.Element("description")?.Value ?? string.Empty
			});
		}
	}

	private static string GetModuleFromTypeName(string typeName) => typeName.Contains('.')
		? typeName.Split('.')[0]
		: "Core";

	private static string ExtractTypeFromSummary(ISummarizedObject obj)
	{
		// Extract type from patterns like "(type) description"
		var match = Regex.Match(obj.Summary, @"^\(([^)]+)\)");
		var result = match.Success ? match.Groups[1].Value : "any";

		// Remove the type from the summary
		obj.Summary = Regex.Replace(obj.Summary, @"^\([^)]+\)\s*", string.Empty).Trim();

		return result;
	}

	private static string ExtractTypeFromDescription(IDescribedObject obj)
	{
		// Extract type from patterns like "(type) description"
		var match = Regex.Match(obj.Description, @"^\(([^)]+)\)");
		var result = match.Success ? match.Groups[1].Value : "any";

		// Remove the type from the description
		obj.Description = Regex.Replace(obj.Description, @"^\([^)]+\)\s*", string.Empty).Trim();

		return result;
	}
}
