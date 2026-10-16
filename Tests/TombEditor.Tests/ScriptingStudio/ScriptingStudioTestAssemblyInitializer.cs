using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class ScriptingStudioTestAssemblyInitializer
{
    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext _)
    {
        StaTestHelper.ConfigureTestServiceLocator();
    }
}
