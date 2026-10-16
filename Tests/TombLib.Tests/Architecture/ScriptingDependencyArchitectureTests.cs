using System.Reflection;
using Nickelony.IDEKit.Core.Text;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.Lua;
using TombLib.Scripting.TRX;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Tests;

[TestClass]
public class ScriptingDependencyArchitectureTests
{
	private static readonly string[] ForbiddenNeutralCoreReferences =
	[
		"AvalonEdit",
		"DarkUI",
		"DarkUI.WPF",
		"PresentationFramework",
		"WindowsBase",
		"System.Windows.Forms",
		"TombLib",
		"TombLib.Forms",
		"TombLib.WPF"
	];

	[TestMethod]
	public void NeutralCore_DoesNotReferenceUiOrHostAssemblies()
	{
		string[] offending = GetReferencedAssemblyNames(typeof(TextRange).Assembly)
			.Where(static name => ForbiddenNeutralCoreReferences.Contains(name))
			.ToArray();

		Assert.AreEqual(0, offending.Length, "TombLib.Scripting must not reference UI or host assemblies: " + string.Join(", ", offending));
	}

	[TestMethod]
	public void NeutralCore_DoesNotReferenceSiblingScriptingProjects()
	{
		string[] siblingReferences = GetReferencedAssemblyNames(typeof(TextRange).Assembly)
			.Where(static name => name.StartsWith("TombLib.Scripting", StringComparison.Ordinal))
			.ToArray();

		Assert.AreEqual(0, siblingReferences.Length, "TombLib.Scripting must not reference sibling scripting projects: " + string.Join(", ", siblingReferences));
	}

	[TestMethod]
	public void NeutralCore_RemainsDependencyFree()
	{
		Assert.IsFalse(
			ReferencesAssembly(typeof(TextRange).Assembly, "Nickelony.LanguageServer.Abstractions"),
			"Nickelony.IDEKit.Core must remain dependency-free.");
	}

	[TestMethod]
	public void EditorIntegrationProjects_ReferenceTheNeutralCore()
	{
		Assert.IsTrue(ReferencesAssembly(typeof(TextEditorBase).Assembly, "Nickelony.IDEKit.Core"), "TombLib.Scripting.UI must reference Nickelony.IDEKit.Core.");
		Assert.IsTrue(ReferencesAssembly(typeof(ClassicScriptEditor).Assembly, "Nickelony.IDEKit.Core"), "TombLib.Scripting.ClassicScript must reference Nickelony.IDEKit.Core.");
		Assert.IsTrue(ReferencesAssembly(typeof(GameFlowEditor).Assembly, "Nickelony.IDEKit.Core"), "TombLib.Scripting.GameFlowScript must reference Nickelony.IDEKit.Core.");
		Assert.IsTrue(ReferencesAssembly(typeof(TRXEditor).Assembly, "Nickelony.IDEKit.Core"), "TombLib.Scripting.TRX must reference Nickelony.IDEKit.Core.");
		Assert.IsTrue(ReferencesAssembly(typeof(LuaEditor).Assembly, "Nickelony.IDEKit.Core"), "TombLib.Scripting.Lua must reference Nickelony.IDEKit.Core.");
	}

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
