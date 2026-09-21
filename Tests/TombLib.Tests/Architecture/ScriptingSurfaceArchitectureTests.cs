using System.Reflection;
using Nickelony.IDEKit.Core.Text;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.Lua;
using TombLib.Scripting.TRX;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Tests;

[TestClass]
public class ScriptingSurfaceArchitectureTests
{
	private static readonly string[] ScriptingAssemblyNames =
	[
		"TombLib.Scripting",
		"TombLib.Scripting.UI",
		"TombLib.Scripting.ClassicScript",
		"TombLib.Scripting.GameFlowScript",
		"TombLib.Scripting.TRX",
		"TombLib.Scripting.Lua"
	];

	private static readonly string[] FeatureContractNames =
	[
		"ITextCompletionProvider",
		"ITextHoverProvider",
		"ITextDefinitionProvider",
		"ITextSignatureHelpProvider",
		"ITextDiagnosticsProvider",
		"ITextDocumentFormatter"
	];

	[TestMethod]
	public void FeatureImplementationClasses_AreSealed()
	{
		string[] offending = GetScriptingAssemblies()
			.SelectMany(static assembly => assembly.GetExportedTypes())
			.Where(static type => type.IsClass && !type.IsAbstract && !type.IsSealed && ImplementsAnyContract(type))
			.Select(static type => type.FullName ?? type.Name)
			.OrderBy(static name => name, StringComparer.Ordinal)
			.ToArray();

		Assert.AreEqual(0, offending.Length, "Concrete feature implementation classes must be sealed: " + string.Join(", ", offending));
	}

	[TestMethod]
	public void PublicApi_ContainsNoForbiddenLegacyIdentifiers()
	{
		string[] offending = GetScriptingAssemblies()
			.SelectMany(GetPublicApiIdentifiers)
			.Where(static identifier => ContainsForbiddenLegacyIdentifier(identifier))
			.Distinct(StringComparer.Ordinal)
			.OrderBy(static identifier => identifier, StringComparer.Ordinal)
			.ToArray();

		Assert.AreEqual(0, offending.Length, "Public API contains forbidden legacy identifiers: " + string.Join(", ", offending));
	}

	private static bool ImplementsAnyContract(Type type)
	{
		foreach (Type interfaceType in type.GetInterfaces())
		{
			if (FeatureContractNames.Contains(interfaceType.Name, StringComparer.Ordinal))
				return true;
		}

		return false;
	}

	private static IEnumerable<string> GetPublicApiIdentifiers(Assembly assembly)
	{
		foreach (Type type in assembly.GetExportedTypes())
		{
			yield return type.Name;

			foreach (MemberInfo member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
			{
				yield return member.Name;

				if (member is MethodBase method)
				{
					foreach (ParameterInfo parameter in method.GetParameters())
						yield return parameter.Name ?? string.Empty;
				}
			}
		}
	}

	private static bool ContainsForbiddenLegacyIdentifier(string identifier)
	{
		if (identifier.Contains("Autocomplete", StringComparison.OrdinalIgnoreCase))
			return true;

		if (identifier.Contains("Intellisense", StringComparison.Ordinal))
			return true;

		if (identifier.Contains("Gameflow", StringComparison.Ordinal))
			return true;

		if (identifier.Contains("gameflow", StringComparison.Ordinal))
			return true;

		return false;
	}

	private static IEnumerable<Assembly> GetScriptingAssemblies()
	{
		Assembly[] assemblies =
		[
			typeof(TextRange).Assembly,
			typeof(TextEditorBase).Assembly,
			typeof(ClassicScriptEditor).Assembly,
			typeof(GameFlowEditor).Assembly,
			typeof(TRXEditor).Assembly,
			typeof(LuaEditor).Assembly
		];

		foreach (Assembly assembly in assemblies)
		{
			if (ScriptingAssemblyNames.Contains(assembly.GetName().Name, StringComparer.Ordinal))
				yield return assembly;
		}
	}
}
