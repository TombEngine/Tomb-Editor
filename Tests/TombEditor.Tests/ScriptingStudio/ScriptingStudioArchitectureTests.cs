using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.Settings;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared;
using TombIDE.Shared.Messaging.Scripting;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;
using TombLib.Scripting.UI.Editors;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class ScriptingStudioArchitectureTests
{
    [TestMethod]
    [DataRow(TRVersion.Game.TR4)]
    [DataRow(TRVersion.Game.TR2)]
    [DataRow(TRVersion.Game.TR1)]
    [DataRow(TRVersion.Game.TombEngine)]
    public void ProfileCatalog_HasUniquePrimaryIdentitiesAndExplicitFallback(TRVersion.Game gameVersion)
    {
        using var scriptDirectory = new TemporaryScriptDirectory(gameVersion);
        ScriptingWorkspaceProfile profile = CreateProfile(gameVersion, supportsLua: true, scriptDirectory.Path);

        ScriptingDocumentRegistration[] primaryRegistrations = profile.DocumentRegistrations
            .Where(registration => !registration.IsFallback)
            .ToArray();

        Assert.AreEqual(
            primaryRegistrations.Length,
            primaryRegistrations.Select(registration => registration.DocumentMode).Distinct().Count(),
            "Each primary registration must have one stable document identity.");
        Assert.IsTrue(
            profile.DocumentRegistrations.Any(registration => registration.IsFallback)
                || profile.Kind is ScriptingWorkspaceKind.TRX or ScriptingWorkspaceKind.Lua,
            "A workspace without a fallback registration must explicitly use the neutral fallback policy.");

        foreach (ScriptingDocumentRegistration registration in profile.DocumentRegistrations)
        {
            Assert.IsTrue(registration.SupportedDocumentModes.Count > 0);
            Assert.IsTrue(
                registration.IsFallback
                    ? registration.SupportedDocumentModes.Contains(DocumentMode.PlainText)
                    : registration.SupportedDocumentModes.Contains(registration.DocumentMode));
        }
    }

    [TestMethod]
    public void RegistrationCatalog_ProvidesDocumentContributionsWithoutGlobalLookup()
    {
        using var scriptDirectory = new TemporaryScriptDirectory(TRVersion.Game.TR1);
        ScriptingWorkspaceProfile profile = CreateProfile(TRVersion.Game.TR1, supportsLua: true, scriptDirectory.Path);

        foreach (ScriptingDocumentRegistration registration in profile.DocumentRegistrations)
        {
            Assert.IsNotNull(registration.Contributions);
        }

        ScriptingDocumentRegistration trx = profile.DocumentRegistrations.Single(registration => registration.DocumentMode == DocumentMode.TRX);
        ScriptingDocumentRegistration lua = profile.DocumentRegistrations.Single(registration => registration.DocumentMode == DocumentMode.Lua);

        Assert.AreEqual(ScriptingDocumentConfigurationKind.TRX, trx.Contributions.ConfigurationKind);
        Assert.AreEqual(ScriptingDocumentConfigurationKind.Lua, lua.Contributions.ConfigurationKind);
        Assert.IsNotNull(trx.Contributions.CommandSurfaceProvider);
        Assert.IsNotNull(lua.Contributions.CommandSurfaceProvider);
    }

    [TestMethod]
    public void EditorControlContract_DoesNotOwnDiagnosticsState()
    {
        Assert.IsFalse(typeof(IEditorControl).GetProperties().Any(property =>
            property.Name.Contains("Diagnostic", StringComparison.Ordinal)));
        Assert.IsFalse(typeof(IEditorControl).GetEvents().Any(@event =>
            @event.Name.Contains("Diagnostic", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void DocumentContributions_DoesNotExposeDocumentModeLookup()
    {
        Assert.IsFalse(typeof(ScriptingDocumentContributions)
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Any(method => method.GetParameters().Any(parameter => parameter.ParameterType == typeof(DocumentMode))));
    }

    [TestMethod]
    public void PaneCatalog_KeepsOneStablePaneInstancePerCommand()
    {
        var firstPane = new TestPane("SharedPane");
        var secondPane = new TestPane("IgnoredPane");
        var providers = new IStudioPaneContributionProvider[]
        {
            new StaticPaneProvider([new StudioPaneContribution(UICommand.LuaDiagnostics, firstPane.SerializationKey, () => firstPane)]),
            new StaticPaneProvider([new StudioPaneContribution(UICommand.LuaDiagnostics, secondPane.SerializationKey, () => secondPane)])
        };

        var catalog = new PaneCatalog(providers);
        try
        {
            Assert.AreSame(firstPane, catalog.GetPane<TestPane>(UICommand.LuaDiagnostics));
            Assert.AreEqual(1, catalog.Panes.Count);
        }
        finally
        {
            catalog.Dispose();
        }

        Assert.AreEqual(1, firstPane.DisposeCount);
        Assert.AreEqual(0, secondPane.DisposeCount);
    }

    private static ScriptingWorkspaceProfile CreateProfile(TRVersion.Game gameVersion, bool supportsLua, string scriptDirectoryPath)
        => ScriptingWorkspaceProfileTestFactory.CreateSelectorProfile(gameVersion, supportsLua, scriptDirectoryPath);

    private sealed class StaticPaneProvider(IReadOnlyList<StudioPaneContribution> contributions) : IStudioPaneContributionProvider
    {
        public IReadOnlyList<StudioPaneContribution> GetPaneContributions() => contributions;
    }

    private sealed class TestPane(string serializationKey) : StudioDockPane(
        "Test",
        serializationKey,
        StudioDockPaneLocation.Bottom,
        new System.Windows.Size(100, 100))
    {
        public int DisposeCount { get; private set; }

        public override System.Windows.UIElement Content => new System.Windows.Controls.Border();

        public override void Dispose()
        {
            DisposeCount++;
        }
    }

    private sealed class TemporaryScriptDirectory : IDisposable
    {
        public TemporaryScriptDirectory(TRVersion.Game gameVersion)
        {
            Path = System.IO.Directory.CreateTempSubdirectory("TombEditor-Architecture-").FullName;
            string fileName = gameVersion == TRVersion.Game.TombEngine
                ? "Gameflow.lua"
                : gameVersion is TRVersion.Game.TR1 or TRVersion.Game.TR2X or TRVersion.Game.TR3X
                ? "gameflow.json5"
                : "Script.txt";
            System.IO.File.WriteAllText(System.IO.Path.Combine(Path, fileName), string.Empty);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (System.IO.Directory.Exists(Path))
                System.IO.Directory.Delete(Path, recursive: true);
        }
    }
}
