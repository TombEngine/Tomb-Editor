#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Nickelony.LanguageServer.Abstractions;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.Messaging;
using TombIDE.ScriptingStudio.Navigation;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Workbench;
using TombIDE.Shared;
using TombLib.Forms.ViewModels;
using TombLib.Scripting.Lua;
using TombLib.Scripting.UI.Presentation;
using TombLib.Scripting.UI.Editors;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class ScriptingPhase0AsyncFailureTests
{
	[TestMethod]
	public void ActiveReferenceProviderFailure_LeavesReferencesPaneInTerminalState()
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
			builder.ReferenceCompletion.SetException(new InvalidOperationException("reference failed"));
			request.GetAwaiter().GetResult();

			TextReferencesResultsViewModel references = GetReferencesViewModel(builder.PaneCatalog.GetPane<TextReferencesResultsToolWindow>(UICommand.LuaReferencesResults)!);
			Assert.AreNotEqual(
				Strings.Default.LuaReferencesLoading,
				references.StatusText,
				"A failed reference request must leave the pane in a terminal state.");
		});
	}

	[TestMethod]
	public void PendingReferenceSearch_ShellUiRefresh_KeepsReferencesPaneLoading()
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
			TextReferencesResultsViewModel references = GetReferencesViewModel(
				builder.PaneCatalog.GetPane<TextReferencesResultsToolWindow>(UICommand.LuaReferencesResults)!);
			Assert.AreEqual(Strings.Default.LuaReferencesLoading, references.StatusText);

			((IMessenger)builder.Messenger).Send(new ShellUiRefreshMessage());

			Assert.AreEqual(
				Strings.Default.LuaReferencesLoading,
				references.StatusText,
				"An unrelated shell refresh must not overwrite an in-flight reference request.");

			builder.ReferenceCompletion.SetResult([
				new TextReferenceLocation(editor.FilePath, 1, 1, 1, 5)
			]);
			request.GetAwaiter().GetResult();
			Assert.AreNotEqual(Strings.Default.LuaReferencesLoading, references.StatusText);
		});
	}

	[TestMethod]
	public void PendingReferenceSearch_ApplyEditorSettings_KeepsReferencesPaneLoading()
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
			TextReferencesResultsViewModel references = GetReferencesViewModel(
				builder.PaneCatalog.GetPane<TextReferencesResultsToolWindow>(UICommand.LuaReferencesResults)!);
			Assert.AreEqual(Strings.Default.LuaReferencesLoading, references.StatusText);

			builder.Workbench.ApplyEditorSettings();

			Assert.AreEqual(
				Strings.Default.LuaReferencesLoading,
				references.StatusText,
				"Applying editor settings must not overwrite an in-flight reference request.");

			builder.ReferenceCompletion.SetResult([
				new TextReferenceLocation(editor.FilePath, 1, 1, 1, 5)
			]);
			request.GetAwaiter().GetResult();
			Assert.AreNotEqual(Strings.Default.LuaReferencesLoading, references.StatusText);
		});
	}

	[TestMethod]
	public void SameEditorEdit_RejectsPendingReferenceResultFromOldSnapshot()
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
			editor.Text = "local value = 2";
			builder.ReferenceCompletion.SetResult([
				new TextReferenceLocation(editor.FilePath, 1, 1, 1, 5)
			]);
			request.GetAwaiter().GetResult();

			TextReferencesResultsViewModel references = GetReferencesViewModel(
				builder.PaneCatalog.GetPane<TextReferencesResultsToolWindow>(UICommand.LuaReferencesResults)!);
			Assert.AreEqual(0, references.Groups.Count, "A result for the old document snapshot must not update the active pane.");
		});
	}

	[TestMethod]
	public void RenameDialogFailure_IsReportedOnce()
	{
		StaTestHelper.RunInSta(() =>
		{
			using var builder = new WorkbenchServiceTestBuilder();
			var editor = new LuaEditor(new Version(1, 0))
			{
				FilePath = @"C:\Scripts\active.lua",
				Text = "local value = 1",
				CaretOffset = 6
			};
			ScriptingDocumentRegistration registration = builder.CreateRegistration(
				DocumentMode.Lua,
				editor,
				ScriptingDocumentConfigurationKind.Lua);
			builder.AddEditor(editor, registration);
			builder.EditProvider.SetupGet(provider => provider.SupportsRename).Returns(true);
			builder.DialogService
				.Setup(service => service.ShowDialog(It.IsAny<InputBoxWindowViewModel>(), It.IsAny<InputBoxWindowViewModel>()))
				.Throws(new InvalidOperationException("dialog failed"));
			builder.Build();
			builder.Activate(editor, registration);

			Task request = StartRename(builder.Workbench, editor);
			request.GetAwaiter().GetResult();
			builder.MessageService.Verify(
				service => service.ShowError("dialog failed", "Rename Symbol"),
				Times.Once);
		});
	}

	[TestMethod]
	public void ReferenceLoadingPaneFailure_IsReportedOnce()
	{
		StaTestHelper.RunInSta(() =>
		{
			var dockHost = new Mock<IAvalonDockHost>();
			dockHost
				.Setup(host => host.ShowPane(It.IsAny<StudioDockPane?>()))
				.Throws(new InvalidOperationException("loading pane failed"));
			using var builder = new WorkbenchServiceTestBuilder();
			builder.WithDockHost(dockHost);
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
			request.GetAwaiter().GetResult();
			builder.MessageService.Verify(
				service => service.ShowError("loading pane failed", "Lua References"),
				Times.Once);
		});
	}

	[TestMethod]
	public void ReferenceContextFailure_IsReportedOnce()
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
			builder.DocumentController
				.SetupGet(controller => controller.CurrentDocumentContext)
				.Throws(new InvalidOperationException("context failed"));

			Task request = StartReferenceSearch(builder.Workbench, editor);
			request.GetAwaiter().GetResult();
			builder.MessageService.Verify(
				service => service.ShowError("context failed", "Lua References"),
				Times.Once);
		});
	}

	[TestMethod]
	public void SupersededReferenceProviderFailure_DoesNotReportStaleRequest()
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
			builder.SetNoDocument();
			builder.ReferenceCompletion.SetException(new InvalidOperationException("stale reference failed"));
			request.GetAwaiter().GetResult();

			builder.MessageService.Verify(
				service => service.ShowError("stale reference failed", "Lua References"),
				Times.Never);
			TextReferencesResultsViewModel references = GetReferencesViewModel(builder.PaneCatalog.GetPane<TextReferencesResultsToolWindow>(UICommand.LuaReferencesResults)!);
			Assert.AreEqual(Strings.Default.LuaReferencesNoDocument, references.StatusText);
		});
	}

	[TestMethod]
	public void RenameProviderFailure_IsObservedAndReportedOnce()
	{
		StaTestHelper.RunInSta(() =>
		{
			using var builder = new WorkbenchServiceTestBuilder();
			var editor = new LuaEditor(new Version(1, 0))
			{
				FilePath = @"C:\Scripts\active.lua",
				Text = "local value = 1",
				CaretOffset = 6
			};
			ScriptingDocumentRegistration registration = builder.CreateRegistration(
				DocumentMode.Lua,
				editor,
				ScriptingDocumentConfigurationKind.Lua);
			builder.AddEditor(editor, registration);
			var providerInvoked = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
			builder.EditProvider.SetupGet(provider => provider.SupportsRename).Returns(true);
			builder.EditProvider
				.Setup(provider => provider.RenameSymbolAsync(It.IsAny<TextRenameRequest>(), It.IsAny<CancellationToken>()))
				.Callback<TextRenameRequest, CancellationToken>((_, _) => providerInvoked.SetResult(null))
				.Returns(Task.FromException<TextWorkspaceEdit?>(new InvalidOperationException("rename failed")));
			builder.DialogService
				.Setup(service => service.ShowDialog(It.IsAny<InputBoxWindowViewModel>(), It.IsAny<InputBoxWindowViewModel>()))
				.Returns(true);

			builder.Build();
			builder.Activate(editor, registration);

			Assert.IsTrue(builder.Workbench.TryExecuteCommand(UICommand.RenameSymbol));
			Assert.IsTrue(providerInvoked.Task.Wait(TimeSpan.FromSeconds(5)));

			builder.MessageService.Verify(
				service => service.ShowError("rename failed", It.IsAny<string>()),
				Times.Once);
			Assert.AreEqual("local value = 1", editor.Text);
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

	private static Task StartRename(WorkbenchService workbench, LuaEditor editor)
	{
		FieldInfo? componentsField = typeof(WorkbenchService).GetField(
			"_components",
			BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.IsNotNull(componentsField);
		var components = (WorkbenchComponents)componentsField.GetValue(workbench)!;
		MethodInfo? method = typeof(WorkbenchCodeNavigationCoordinator).GetMethod(
			"RenameLuaSymbolAsync",
			BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.IsNotNull(method);
		return (Task)method.Invoke(components.CodeNavigationCoordinator, [editor])!;
	}

	private static TextReferencesResultsViewModel GetReferencesViewModel(TextReferencesResultsToolWindow pane)
		=> (TextReferencesResultsViewModel)((TextReferencesResultsView)pane.Content).DataContext;
}
