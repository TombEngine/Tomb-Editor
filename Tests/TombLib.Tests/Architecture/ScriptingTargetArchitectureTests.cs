using System.Reflection;
using Nickelony.IDEKit.Core.Text;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.Lua;
using TombLib.Scripting.TRX;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Tests;

/// <summary>
/// Expresses the target dependency graph frozen by Phase 8 of the provider-libraries remedy
/// plan: the neutral core is the sink, the framework adapter composes the neutral contracts,
/// and each language provider composes on top of the adapter without referencing its siblings
/// or the host application.
/// </summary>
[TestClass]
public class ScriptingTargetArchitectureTests
{
	private static readonly string[] LanguageProviderAssemblyNames =
	[
		"TombLib.Scripting.ClassicScript",
		"TombLib.Scripting.GameFlowScript",
		"TombLib.Scripting.TRX",
		"TombLib.Scripting.Lua"
	];

	private static readonly string[] HostApplicationPrefixes = ["TombIDE"];

	[TestMethod]
	public void LanguageProviders_DoNotReferenceEachOther()
	{
		foreach (Assembly provider in GetLanguageProviderAssemblies())
		{
			string[] siblingReferences = GetReferencedAssemblyNames(provider)
				.Where(static name => LanguageProviderAssemblyNames.Contains(name))
				.ToArray();

			Assert.AreEqual(0, siblingReferences.Length, $"{provider.GetName().Name} must not reference a sibling language provider: {string.Join(", ", siblingReferences)}");
		}
	}

	[TestMethod]
	public void LanguageProviders_ReferenceTheFrameworkAdapter()
	{
		foreach (Assembly provider in GetLanguageProviderAssemblies())
		{
			Assert.IsTrue(ReferencesAssembly(provider, "TombLib.Scripting.UI"), $"{provider.GetName().Name} must reference the TombLib.Scripting.UI framework adapter.");
		}
	}

	[TestMethod]
	public void ScriptingProjects_DoNotReferenceTheHostApplication()
	{
		Assembly[] scriptingAssemblies =
		[
			typeof(TextRange).Assembly,
			typeof(TextEditorBase).Assembly,
			typeof(ClassicScriptLanguageServices).Assembly,
			typeof(GameFlowLanguageServices).Assembly,
			typeof(TRXLanguageServices).Assembly,
			typeof(LuaEditor).Assembly
		];

		foreach (Assembly assembly in scriptingAssemblies)
		{
			string[] hostReferences = GetReferencedAssemblyNames(assembly)
				.Where(static name => HostApplicationPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal)))
				.ToArray();

			Assert.AreEqual(0, hostReferences.Length, $"{assembly.GetName().Name} must not reference the host application: {string.Join(", ", hostReferences)}");
		}
	}

	[TestMethod]
	public void LanguageProviders_DoNotCompileTheLinkedGlobalPathsFile()
	{
		// GlobalPaths.cs defines the internal DefaultPaths type. The provider projects must not
		// reintroduce the linked compile item, so none of their assemblies may contain it.
		foreach (Assembly provider in GetLanguageProviderAssemblies())
		{
			Assert.IsNull(provider.GetType("DefaultPaths"), $"{provider.GetName().Name} must not compile the linked GlobalPaths.cs file.");
		}
	}

	private static Assembly[] GetLanguageProviderAssemblies() =>
	[
		typeof(ClassicScriptLanguageServices).Assembly,
		typeof(GameFlowLanguageServices).Assembly,
		typeof(TRXLanguageServices).Assembly,
		typeof(LuaEditor).Assembly
	];

	private static string[] GetReferencedAssemblyNames(Assembly assembly)
	{
		return assembly.GetReferencedAssemblies()
			.Select(static reference => reference.Name)
			.OfType<string>()
			.ToArray();
	}

	private static bool ReferencesAssembly(Assembly assembly, string assemblyName)
		=> GetReferencedAssemblyNames(assembly).Contains(assemblyName, StringComparer.Ordinal);
}
