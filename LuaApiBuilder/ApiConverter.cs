using LuaApiBuilder.Objects;
using System.Text;
using System.Text.RegularExpressions;

namespace LuaApiBuilder;

public sealed class ApiConverter
{
	private Version _engineVersion = null!;
	private ApiResult _api = null!;

	public void Convert(Version engineVersion, string inputPath, string outputDirectory)
	{
		_engineVersion = engineVersion;

		// Parse the XML file
		_api = ApiXmlParser.ParseApi(inputPath);

		// Ensure output directory exists
		Directory.CreateDirectory(outputDirectory);

		// Generate module files
		foreach (var moduleName in _api.AllModules)
			GenerateModuleFile(moduleName, outputDirectory);

		// Generate the main TEN module file
		GenerateMainTenFile(outputDirectory);
	}

	private void WriteFileHeader(StringBuilder builder)
	{
		builder.AppendLine($"-- TEN {_engineVersion} API");
		builder.AppendLine("-- This file is auto-generated and for IntelliSense purposes only!");
		builder.AppendLine("-- Any changes made here will not be reflected in the engine.");
		builder.AppendLine();
	}

	// TEMPORARY - Fix LDoc XML export to include these globals!
	private static void WriteUndocumentedGlobals(StringBuilder builder)
	{
		// Write variable declaration for `Lara` global which is of type `LaraObject`
		string description = "---Global variable for Lara object.";

		builder.AppendLine(description);
		builder.AppendLine("Lara = LaraObject()");
		builder.AppendLine("Objects.Lara = Lara");
		builder.AppendLine("TEN.Objects.Lara = Lara");
		builder.AppendLine();

		// Write variable declaration for `GameVars` table
		description = "---A table with game data which will be saved and loaded. " +
			"This is for information not specific to any level, but which concerns your whole levelset or game, that you want to store in saved games.";

		builder.AppendLine(description);
		builder.AppendLine("GameVars = {}");
		builder.AppendLine("Logic.GameVars = GameVars");
		builder.AppendLine("TEN.Logic.GameVars = GameVars");
		builder.AppendLine();

		// Write variable declaration for `LevelVars` table
		description = "---A table with level-specific data which will be saved and loaded. " +
			"This is for level-specific information that you want to store in saved games.";

		builder.AppendLine(description);
		builder.AppendLine("LevelVars = {}");
		builder.AppendLine("Logic.LevelVars = LevelVars");
		builder.AppendLine("TEN.Logic.LevelVars = LevelVars");
		builder.AppendLine();

		// Write variable declaration for `LevelFuncs` table
		description = "---A nested table system for level-specific functions.";

		builder.AppendLine(description);
		builder.AppendLine("LevelFuncs = {}");
		builder.AppendLine("Logic.LevelFuncs = LevelFuncs");
		builder.AppendLine("TEN.Logic.LevelFuncs = LevelFuncs");
		builder.AppendLine();

		// Add functions for LevelFuncs

		builder.AppendLine("---Will be called when a level is entered by completing a previous level or by selecting it in the menu. Will not be called when loaded from a saved game.");
		builder.AppendLine("LevelFuncs.OnStart = function() end");

		builder.AppendLine("---Will be called when a saved game is loaded, just after data is loaded.");
		builder.AppendLine("LevelFuncs.OnLoad = function() end");

		builder.AppendLine("---Will be called during the game's update loop, and provides the delta time (a float representing game time since last call) via its argument.");
		builder.AppendLine("---@param dt number # Delta time since last call.");
		builder.AppendLine("LevelFuncs.OnLoop = function(dt) end");

		builder.AppendLine("---Will be called when the player saves the game, just before data is saved.");
		builder.AppendLine("LevelFuncs.OnSave = function() end");

		builder.AppendLine("---Will be called when leaving a level. This includes finishing it, exiting to the menu, or loading a save in a different level.");
		builder.AppendLine("LevelFuncs.OnEnd = function() end");

		builder.AppendLine("---Will be called when using an item from inventory.");
		builder.AppendLine("LevelFuncs.OnUseItem = function() end");

		builder.AppendLine("---Will be called when any of the Freeze modes are activated.");
		builder.AppendLine("LevelFuncs.OnFreeze = function() end");
	}

	private void GenerateModuleFile(string moduleName, string outputDirectory)
	{
		bool isCoreModule = moduleName.Equals("Core", StringComparison.OrdinalIgnoreCase);
		string actualModuleName = isCoreModule ? "TEN" : moduleName;

		var builder = new StringBuilder();

		WriteFileHeader(builder);

		builder.AppendLine("---@meta");
		builder.AppendLine();

		if (!isCoreModule)
		{
			// Module comment
			builder.AppendLine($"---{moduleName} module for Tomb Engine");
			builder.AppendLine($"---@class {moduleName}");
			builder.AppendLine($"{moduleName} = {{}}");
			builder.AppendLine();
		}

		// Generate enums first
		if (_api.ModuleEnums.ContainsKey(moduleName))
		{
			foreach (var apiEnum in _api.ModuleEnums[moduleName])
			{
				GenerateEnum(builder, apiEnum, actualModuleName);
				builder.AppendLine();
			}
		}

		// Generate classes
		if (_api.ModuleClasses.ContainsKey(moduleName))
		{
			foreach (var apiClass in _api.ModuleClasses[moduleName])
			{
				GenerateClass(builder, apiClass, actualModuleName);
				builder.AppendLine();
			}
		}

		// Generate module functions
		if (_api.ModuleTypeFunctions.ContainsKey(moduleName))
		{
			foreach (var function in _api.ModuleTypeFunctions[moduleName])
			{
				GenerateFunction(builder, function, actualModuleName);
				builder.AppendLine();
			}
		}

		if (!isCoreModule)
			builder.AppendLine($"return {moduleName}");

		// Write to file
		var fileName = Path.Combine(outputDirectory, $"{moduleName}.lua");
		File.WriteAllText(fileName, builder.ToString());
	}

	private void GenerateMainTenFile(string outputDirectory)
	{
		var builder = new StringBuilder();

		WriteFileHeader(builder);

		builder.AppendLine("---@meta");
		builder.AppendLine();
		builder.AppendLine("---Tomb Engine Lua API");
		builder.AppendLine("---@class TEN");
		builder.AppendLine("TEN = {}");
		builder.AppendLine();

		foreach (var moduleName in _api.AllModules.OrderBy(x => x))
		{
			if (moduleName.Equals("Core", StringComparison.OrdinalIgnoreCase))
				continue; // Skip Core module as it is not a Lua module

			builder.AppendLine($"---@type {moduleName}");
			builder.AppendLine($"TEN.{moduleName} = require(\"{moduleName}\")");
		}

		builder.AppendLine();

		// Generate global aliases for each module to support both TEN.Module and Module syntax
		builder.AppendLine("-- Global aliases for direct module access (supports both TEN.Module and Module syntax)");

		foreach (var moduleName in _api.AllModules.OrderBy(x => x))
		{
			if (moduleName.Equals("Core", StringComparison.OrdinalIgnoreCase))
				continue; // Skip Core module as it is not a Lua module

			builder.AppendLine($"---@type {moduleName}");
			builder.AppendLine($"{moduleName} = TEN.{moduleName}");
		}

		builder.AppendLine();

		WriteUndocumentedGlobals(builder);

		builder.AppendLine();
		builder.AppendLine("return TEN");

		var fileName = Path.Combine(outputDirectory, "TEN.lua");
		File.WriteAllText(fileName, builder.ToString());
	}

	private static void GenerateEnum(StringBuilder builder, ApiEnum apiEnum, string moduleName)
	{
		TryWriteDescription(builder, apiEnum.Summary);
		TryWriteDescription(builder, apiEnum.Description);

		var enumName = GetLastPart(apiEnum.Name);

		// Generate the direct enum name first
		builder.AppendLine($"---@enum {enumName}");
		builder.AppendLine($"{enumName} = {{");

		foreach (var value in apiEnum.Values)
		{
			TryWriteDescription(builder, value.Summary, 1);
			builder.AppendLine($"\t{value.Name} = \"{value.Name}\",");
		}

		builder.AppendLine("}");
		builder.AppendLine();

		// Generate the module-prefixed alias
		builder.AppendLine($"---Module-prefixed alias for {enumName}");
		builder.AppendLine($"{moduleName}.{enumName} = {enumName}");
	}

	private void GenerateClass(StringBuilder builder, ApiClass apiClass, string moduleName)
	{
		TryWriteDescription(builder, apiClass.Summary);
		TryWriteDescription(builder, apiClass.Description);

		var className = GetLastPart(apiClass.Name);

		if (!string.IsNullOrWhiteSpace(apiClass.Inherits))
			builder.AppendLine($"---@class {className} : {MapType(apiClass.Inherits)}");
		else
			builder.AppendLine($"---@class {className}");

		// Generate fields
		foreach (var field in apiClass.Fields)
			builder.AppendLine($"---@field {field.Name} {MapType(field.Type)} # {CleanDescription(field.Summary)}");

		// Generate the direct class name first
		builder.AppendLine($"{className} = {{}}");
		builder.AppendLine();

		// Generate the module-prefixed alias
		builder.AppendLine($"---Module-prefixed alias for {className}");
		builder.AppendLine($"{moduleName}.{className} = {className}");
		builder.AppendLine();

		// Generate methods (including constructors) for both direct class and module-prefixed version
		foreach (var method in apiClass.Methods)
		{
			var isConstructor = IsConstructor(method.Name, className);

			// Generate method for direct class name
			GenerateMethodForClass(builder, method, className, className, isConstructor, false);
			builder.AppendLine();

			if (isConstructor)
			{
				// Generate constructor for module-prefixed version
				GenerateMethodForClass(builder, method, moduleName, className, isConstructor, true);
				builder.AppendLine();
			}
		}

		if (!apiClass.HasConstructor)
			return; // Don't generate constructors

		// Check if class has any constructors defined
		var constructorMethods = apiClass.Methods.Where(m => IsConstructor(m.Name, className)).ToList();

		// If no constructors are defined, generate empty constructors
		if (constructorMethods.Count == 0)
		{
			string returnLine = $"---@return {className} # A new {className} object.";

			// Generate empty constructor for direct class name
			builder.AppendLine($"---Constructor for {className}");
			builder.AppendLine(returnLine);
			builder.AppendLine($"function {className}.new() end");
			builder.AppendLine();

			// Generate empty constructor for module-prefixed version
			builder.AppendLine($"---Constructor for {moduleName}.{className}");
			builder.AppendLine(returnLine);
			builder.AppendLine($"function {moduleName}.{className}.new() end");
			builder.AppendLine();

			// Generate constructor alias for direct class name
			builder.AppendLine($"---Constructor for {className} (alias for {className}.new)");
			builder.AppendLine(returnLine);
			builder.AppendLine($"function {className}() end");
			builder.AppendLine();

			// Generate constructor alias for module-prefixed version
			builder.AppendLine($"---Constructor for {moduleName}.{className} (alias for {moduleName}.{className}.new)");
			builder.AppendLine(returnLine);
			builder.AppendLine($"function {moduleName}.{className}() end");
			builder.AppendLine();
		}
		else // Generate direct function call constructor aliases for existing constructors
		{
			// Generate direct constructor for direct class name
			builder.AppendLine($"---Constructor function for {className} (alias for {className}.new)");

			foreach (var constructor in constructorMethods)
			{
				// Generate parameter annotations for the direct constructor
				foreach (var param in constructor.Parameters)
					GenerateParameterAnnotation(builder, param);

				// Generate return annotations for the direct constructor
				foreach (var ret in constructor.Returns)
				{
					var returnType = MapType(ret.Type);
					var description = CleanDescription(ret.Description);

					builder.AppendLine($"---@return {returnType} # {description}");
				}

				// Generate direct constructor function signature for direct class
				var paramNames = GetParameterNamesForSignature(constructor.Parameters);

				builder.AppendLine($"function {className}({paramNames}) end");
				builder.AppendLine();
			}

			// Generate direct constructor for module-prefixed version
			builder.AppendLine($"---Constructor function for {moduleName}.{className} (alias for {moduleName}.{className}.new)");

			foreach (var constructor in constructorMethods)
			{
				// Generate parameter annotations for the direct constructor
				foreach (var param in constructor.Parameters)
					GenerateParameterAnnotation(builder, param);

				// Generate return annotations for the direct constructor
				foreach (var ret in constructor.Returns)
				{
					var returnType = MapType(ret.Type);
					var description = CleanDescription(ret.Description);

					builder.AppendLine($"---@return {returnType} # {description}");
				}

				// Generate direct constructor function signature for module-prefixed version
				var paramNames = GetParameterNamesForSignature(constructor.Parameters);

				builder.AppendLine($"function {moduleName}.{className}({paramNames}) end");
				builder.AppendLine();
			}
		}
	}

	private void GenerateFunction(StringBuilder builder, ApiFunction function, string moduleName)
	{
		bool isConstructor = IsConstructor(function.Name, moduleName);
		bool isInstanceMethod = !string.IsNullOrEmpty(function.Caller);
		char separator = isInstanceMethod ? ':' : '.';

		TryWriteDescription(builder, function.Summary);
		TryWriteDescription(builder, function.Description);

		// Generate parameter annotations
		foreach (var param in function.Parameters)
			GenerateParameterAnnotation(builder, param);

		// Generate return annotations
		foreach (var ret in function.Returns)
		{
			var returnType = MapType(ret.Type);
			var description = CleanDescription(ret.Description);

			builder.AppendLine($"---@return {returnType} # {description}");
		}

		// Generate function signature
		var paramNames = GetParameterNamesForSignature(function.Parameters);

		if (isConstructor)
			builder.AppendLine($"function {moduleName}{separator}new({paramNames}) end");
		else
			builder.AppendLine($"function {moduleName}{separator}{function.Name}({paramNames}) end");
	}

	private void GenerateMethodForClass(StringBuilder builder, ApiMethod method, string moduleName, string className, bool isConstructor, bool useModulePrefix)
	{
		TryWriteDescription(builder, method.Summary);
		TryWriteDescription(builder, method.Description);

		// Generate parameter annotations
		foreach (var param in method.Parameters)
			GenerateParameterAnnotation(builder, param);

		// Generate return annotations
		foreach (var ret in method.Returns)
		{
			var returnType = MapType(ret.Type);
			var description = CleanDescription(ret.Description);

			builder.AppendLine($"---@return {returnType} # {description}");
		}

		// Generate method signature
		var paramNames = GetParameterNamesForSignature(method.Parameters);

		if (isConstructor)
		{
			if (useModulePrefix)
				builder.AppendLine($"function {moduleName}.{className}.new({paramNames}) end");
			else
				builder.AppendLine($"function {className}.new({paramNames}) end");
		}
		else
		{
			var methodName = method.Name.Replace($"{className}:", string.Empty);

			if (useModulePrefix)
				builder.AppendLine($"function {moduleName}.{className}:{methodName}({paramNames}) end");
			else
				builder.AppendLine($"function {className}:{methodName}({paramNames}) end");
		}
	}

	/// <summary>
	/// Generates a parameter annotation for Lua Language Server, handling optional parameters and default values.
	/// </summary>
	/// <param name="builder">The string builder to append to.</param>
	/// <param name="param">The parameter to generate annotation for.</param>
	private void GenerateParameterAnnotation(StringBuilder builder, ApiParameter param)
	{
		var paramType = MapType(param.Type);
		var description = CleanDescription(param.Description);
		var paramName = EscapeLuaReservedKeyword(param.Name);

		// Handle optional parameters
		if (param.Optional)
		{
			// Make the type optional by appending '?'
			if (!paramType.EndsWith("?"))
				paramType += "?";

			// Add default value information to description if available
			if (!string.IsNullOrWhiteSpace(param.DefaultValue))
			{
				if (!string.IsNullOrWhiteSpace(description))
					description += $" (default: `{param.DefaultValue}`)";
				else
					description = $"Default: `{param.DefaultValue}`";
			}
			else
			{
				// If no default value is specified but parameter is optional, indicate it's optional
				if (!string.IsNullOrWhiteSpace(description))
					description += " (optional)";
				else
					description = "Optional parameter";
			}
		}

		builder.AppendLine($"---@param {paramName} {paramType} # {description}");
	}

	/// <summary>
	/// Gets parameter names for function signature, considering optional parameters.
	/// </summary>
	/// <param name="parameters">The list of parameters.</param>
	/// <returns>A comma-separated string of parameter names for the function signature.</returns>
	private static string GetParameterNamesForSignature(IEnumerable<ApiParameter> parameters)
	{
		var paramNames = new List<string>();

		foreach (var param in parameters)
		{
			var paramName = EscapeLuaReservedKeyword(param.Name);
			paramNames.Add(paramName);
		}

		return string.Join(", ", paramNames);
	}

	private static void TryWriteDescription(StringBuilder builder, string? description, int indentation = 0)
	{
		if (string.IsNullOrWhiteSpace(description))
			return;

		var lines = FormatDescription(description);
		var indent = new string('\t', indentation);

		foreach (var line in lines)
			builder.AppendLine($"{indent}---{line}");
	}

	private static bool IsConstructor(string functionName, string typeName)
		=> functionName.Equals(typeName, StringComparison.OrdinalIgnoreCase)
		|| functionName.Equals(GetLastPart(typeName), StringComparison.OrdinalIgnoreCase);

	private static string GetLastPart(string name) => name.Contains('.')
		? name.Split('.')[^1]
		: name;

	private string MapType(string xmlType)
	{
		if (string.IsNullOrEmpty(xmlType))
			return "any";

		// Handle optional types
		if (xmlType.StartsWith("?"))
			return MapType(xmlType[1..]) + "?";

		// Remove XML reference markers
		xmlType = Regex.Replace(xmlType, @"@\{([^}]+)\}", "$1");

		// Handle union types (e.g., "string|number", "Flow.Level|Objects.Moveable")
		if (xmlType.Contains('|'))
		{
			var unionParts = xmlType.Split('|')
				.Select(part => MapType(part.Trim()))
				.Where(part => !string.IsNullOrEmpty(part));

			return string.Join("|", unionParts);
		}

		// Handle module-prefixed types (e.g., Flow.Level, Objects.Moveable)
		if (xmlType.Contains('.'))
		{
			var parts = xmlType.Split('.');

			if (parts.Length == 2)
			{
				var moduleName = parts[0];
				var typeName = parts[1];

				if (_api.AllModules.Contains(moduleName))
					return $"{typeName}";

				// For other module-prefixed types (like Objects.Moveable), keep as-is
				return xmlType;
			}
		}

		// Check if this is a class that belongs to a specific module
		foreach (var moduleName in _api.AllModules)
		{
			if (!_api.ModuleClasses.ContainsKey(moduleName))
				continue;

			foreach (var apiClass in _api.ModuleClasses[moduleName])
			{
				var className = GetLastPart(apiClass.Name);

				if (className.Equals(xmlType, StringComparison.OrdinalIgnoreCase))
					return $"{className}";
			}
		}

		// Map basic types
		return xmlType.ToLowerInvariant() switch
		{
			"int" or "short" => "integer",
			"float" or "number" => "number",
			"bool" or "boolean" => "boolean",
			"string" => "string",
			"table" => "table",
			"function" or "levelfunc" => "function",
			"variable" or "any" => "any",
			_ => xmlType
		};
	}

	private static string CleanDescription(string description)
	{
		if (string.IsNullOrEmpty(description))
			return string.Empty;

		// Remove XML reference markers
		description = Regex.Replace(description, @"@\{([^}]+)\}", "$1");

		// Clean up excessive whitespace and newlines
		description = Regex.Replace(description, @"\s+", " ").Trim();

		return description;
	}

	private static IReadOnlyList<string> FormatDescription(string description)
	{
		if (string.IsNullOrEmpty(description))
			return new List<string>();

		description = CleanDescription(description);
		var lines = new List<string>();

		// Split into sentences or reasonable chunks
		var sentences = description.Split(new[] { ". ", ".\n", ".\r\n" }, StringSplitOptions.RemoveEmptyEntries);

		foreach (var sentence in sentences)
		{
			var trimmed = sentence.Trim();

			if (!string.IsNullOrEmpty(trimmed))
			{
				// Ensure sentence ends with period if it doesn't already
				if (!trimmed.EndsWith(".") && !trimmed.EndsWith("!") && !trimmed.EndsWith("?"))
					trimmed += ".";

				lines.Add(trimmed);
			}
		}

		return lines.Count > 0 ? lines : new List<string> { description };
	}

	private static string EscapeLuaReservedKeyword(string name)
	{
		// List of Lua reserved keywords that need to be escaped
		var reservedKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"and", "break", "do", "else", "elseif", "end", "false", "for",
			"function", "if", "in", "local", "nil", "not", "or", "repeat",
			"return", "then", "true", "until", "while", "goto"
		};

		return reservedKeywords.Contains(name) ? $"{name}_" : name;
	}
}
