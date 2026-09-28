#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Nickelony.LanguageServer.Abstractions;
using Nickelony.LanguageServer.Lua;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Composition;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Messaging;
using TombIDE.ScriptingStudio.Navigation;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Workbench;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombLib.Scripting.Lua;
using TombLib.Scripting.UI.Editors;
using TombIDE.Shared.Messaging.Scripting;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class ScriptingPhaseCCompositionTests
{
    private const string MissingExecutableMessage =
        "The bundled Lua language server could not be found. Lua IntelliSense is unavailable.";
    private const string ProviderMissingExecutableMessage =
        "The Lua language server executable is unavailable, so Lua IntelliSense is disabled until the provider is recreated with a valid executable path.";

    [TestMethod]
    public void LuaProviderResolution_DoesNotReportThroughMessageService()
    {
        StaTestHelper.RunInSta(() =>
        {
            var messageService = new Mock<IMessageService>();
            var project = new Mock<IGameProject>();
            project.SetupGet(value => value.GameVersion).Returns(TRVersion.Game.TR4);
            project.Setup(value => value.GetCurrentEngineVersion()).Returns(new Version(4, 8));

            var projectContext = new Mock<IScriptingProjectContext>();
            projectContext.SetupGet(value => value.Project).Returns(project.Object);
            projectContext.SetupGet(value => value.ScriptRootDirectoryPath).Returns(@"C:\Scripts");

            var services = new ServiceCollection();
            services.AddScriptingStudioHostComposition();
            services.AddScoped<IScriptingProjectContext>(_ => projectContext.Object);
            services.AddSingleton<IMessageService>(messageService.Object);

            using ServiceProvider serviceProvider = services.BuildServiceProvider();
            using IServiceScope scope = serviceProvider.CreateScope();
            _ = scope.ServiceProvider.GetRequiredService<ILuaLanguageServerIntelliSenseProvider>();

            messageService.Verify(
                service => service.ShowError(MissingExecutableMessage, "Lua IntelliSense"),
                Times.Never);
        });
    }

    [TestMethod]
    public void NonLuaProfile_DoesNotComposeLuaHostServices()
    {
        DirectoryInfo scriptDirectory = Directory.CreateTempSubdirectory("TombEditor-PhaseC-");
        try
        {
            File.WriteAllText(Path.Combine(scriptDirectory.FullName, "Script.txt"), string.Empty);
            ScriptingWorkspaceProfile profile = ScriptingWorkspaceProfileTestFactory.CreateSelectorProfile(
                TRVersion.Game.TR4,
                supportsLua: false,
                scriptDirectory.FullName);
            var serviceProvider = new Mock<IServiceProvider>();

            LuaHostServices? luaHostServices =
                ScriptingStudioServiceCollectionExtensions.CreateLuaHostServices(serviceProvider.Object, profile);

            Assert.IsNull(luaHostServices);
            serviceProvider.Verify(provider => provider.GetService(It.IsAny<Type>()), Times.Never);
        }
        finally
        {
            scriptDirectory.Delete(recursive: true);
        }
    }

    [TestMethod]
    public void MissingExecutableStartupFailure_UsesHostReportingBoundary()
    {
        StaTestHelper.RunInSta(() =>
        {
            using var builder = new WorkbenchServiceTestBuilder();
            builder.Build();

            builder.Messenger.Send(new LuaStartupFailedMessage(
                new LanguageServerStartupFailure(MissingExecutableMessage, isPersistent: true)));

            builder.MessageService.Verify(
                service => service.ShowError(MissingExecutableMessage, "Lua IntelliSense"),
                Times.Once);
            builder.MessageService.Verify(
                service => service.ShowInformation(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        });
    }

    [TestMethod]
    public void DisposingWorkbench_CancelsReferenceRequestBeforeLateCompletion()
    {
        StaTestHelper.RunInSta(() =>
        {
            using var builder = new WorkbenchServiceTestBuilder();
            var editor = new LuaEditor(new Version(1, 0))
            {
                FilePath = @"C:\Scripts\active.lua",
                Text = "local value = 1"
            };
            ScriptingDocumentRegistration registration = builder.CreateRegistration(
                DocumentMode.Lua,
                editor,
                ScriptingDocumentConfigurationKind.Lua);
            builder.AddEditor(editor, registration);
            builder.Build();
            builder.Activate(editor, registration);

            Task request = StartReferenceSearch(builder.Workbench, editor);
            Assert.IsTrue(builder.ReferenceCancellationToken.HasValue);
            CancellationToken cancellationToken = builder.ReferenceCancellationToken!.Value;

            builder.DisposeWorkbench();
            Assert.IsTrue(cancellationToken.IsCancellationRequested);

            builder.ReferenceCompletion.SetResult([]);
            request.GetAwaiter().GetResult();

            builder.MessageService.Verify(
                service => service.ShowError(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        });
    }

    [TestMethod]
    public void MissingExecutableProvider_RaisesPersistentStartupFailure()
    {
        StaTestHelper.RunInSta(() =>
        {
            using var provider = new LuaLanguageServerIntelliSenseProvider(
                [@"C:\Scripts"],
                serverExecutablePath: null,
                options: LuaLanguageServerOptions.Default,
                logger: null);
            var startupFailure = new TaskCompletionSource<LanguageServerStartupFailure>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            provider.StartupFailed += (_, eventArgs) => startupFailure.SetResult(eventArgs.Failure);

            provider.OpenDocument(@"C:\Scripts\main.lua", "return 1");

            Assert.IsTrue(startupFailure.Task.Wait(TimeSpan.FromSeconds(5)));
            Assert.AreEqual(ProviderMissingExecutableMessage, startupFailure.Task.Result.Message);
            Assert.IsTrue(startupFailure.Task.Result.IsPersistent);
        });
    }

    private static Task StartReferenceSearch(WorkbenchService workbench, LuaEditor editor)
    {
        MethodInfo? method = typeof(WorkbenchService).GetMethod(
            "FindLuaReferencesAsync",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(method);
        return (Task)method.Invoke(workbench, [editor])!;
    }
}
