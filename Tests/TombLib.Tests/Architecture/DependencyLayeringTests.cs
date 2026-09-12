using System.Reflection;
using System.Runtime.Versioning;
using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.Lua;
using TombLib.Scripting.TRX;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Tests;

[TestClass]
public class DependencyLayeringTests
{
	private static readonly string[] HostAssemblyPrefixes = ["TombLib", "DarkUI", "TombIDE", "AvalonEdit", "ICSharpCode"];

	[TestMethod]
	public void NeutralCore_HasNoHostOrUiDependency()
	{
		Assembly neutralCore = typeof(ITextSnapshot).Assembly;

		string[] forbidden = neutralCore.GetReferencedAssemblies()
			.Select(reference => reference.Name)
			.Where(name => HostAssemblyPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
			.ToArray();

		Assert.AreEqual(0, forbidden.Length, "Neutral core references host assemblies: " + string.Join(", ", forbidden));
	}

	[TestMethod]
	public void NeutralCore_IsNotWindowsTargeted()
	{
		Assembly neutralCore = typeof(ITextSnapshot).Assembly;
		TargetFrameworkAttribute? framework = neutralCore.GetCustomAttribute<TargetFrameworkAttribute>();

		Assert.IsNotNull(framework, "Neutral core must carry a target framework attribute.");
		Assert.IsFalse(framework.FrameworkName.Contains("-windows", StringComparison.Ordinal), "Neutral core must not be Windows-targeted: " + framework.FrameworkName);
	}

	[TestMethod]
	public void IntelliSenseContracts_LiveInDependencyFreeAssembly()
	{
		Assembly intellisenseAssembly = typeof(ITextDiagnosticsProvider).Assembly;

		Assert.AreEqual("Nickelony.IDEKit.IntelliSense", intellisenseAssembly.GetName().Name, "IntelliSense contracts must live in the Nickelony.IDEKit.IntelliSense assembly.");

		string[] forbidden = intellisenseAssembly.GetReferencedAssemblies()
			.Select(reference => reference.Name)
			.Where(name => HostAssemblyPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
			.ToArray();

		Assert.AreEqual(0, forbidden.Length, "IntelliSense assembly references host assemblies: " + string.Join(", ", forbidden));
	}

	[TestMethod]
	public void UiAdapter_ReferencesNeutralCoreAndAvalonEdit()
	{
		Assembly uiAdapter = typeof(TextEditorBase).Assembly;

		Assert.IsNotNull(uiAdapter.GetReferencedAssemblies().FirstOrDefault(reference => reference.Name == "Nickelony.IDEKit.Core"), "UI adapter must reference Nickelony.IDEKit.Core.");
		Assert.IsNotNull(uiAdapter.GetReferencedAssemblies().FirstOrDefault(reference => reference.Name == "ICSharpCode.AvalonEdit"), "UI adapter must reference AvalonEdit.");
	}

	[TestMethod]
	public void LanguageProviders_ReferenceNeutralCore()
	{
		AssertProviderReferencesNeutralCore(typeof(ClassicScriptLanguageServices).Assembly);
		AssertProviderReferencesNeutralCore(typeof(GameFlowLanguageServices).Assembly);
		AssertProviderReferencesNeutralCore(typeof(TRXLanguageServices).Assembly);
		AssertProviderReferencesNeutralCore(typeof(LuaEditor).Assembly);
	}

	private static void AssertProviderReferencesNeutralCore(Assembly providerAssembly)
	{
		Assert.IsNotNull(
			providerAssembly.GetReferencedAssemblies().FirstOrDefault(reference => reference.Name == "Nickelony.IDEKit.Core"),
			$"{providerAssembly.GetName().Name} must reference the neutral Nickelony.IDEKit.Core assembly.");
	}
}
