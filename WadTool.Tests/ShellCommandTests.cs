using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using TombLib.Wad;
using WadTool;
using WadTool.Features.Main;

namespace WadTool.Tests;

[TestClass]
public class ShellCommandTests
{
    [TestMethod]
    public void EmptyDestinationDisablesSavingAndCreationButAllowsNewWad()
    {
        using var scope = new EditorScope();
        int invoked = 0;
        using var viewModel = CreateViewModel(scope.Tool, () => invoked++);

        Assert.IsFalse(viewModel.TryExecute("SaveMenu_Click"));
        Assert.IsFalse(viewModel.TryExecute("SaveAsMenu_Click"));
        Assert.IsFalse(viewModel.Commands["NewMoveableMenu_Click"].CanExecute(null));
        Assert.IsTrue(viewModel.TryExecute("NewWad2Menu_Click"));
        Assert.AreEqual(1, invoked);
    }

    [TestMethod]
    public void LoadedCleanDestinationAllowsSaveAsAndCreation()
    {
        using var scope = new EditorScope();
        scope.Tool.DestinationWad = new Wad2 { FileName = "loaded.wad2" };
        scope.Tool.ToggleUnsavedChanges(false);
        using var viewModel = CreateViewModel(scope.Tool, () => { });

        Assert.IsFalse(viewModel.CanSave);
        Assert.IsTrue(viewModel.Commands["SaveAsMenu_Click"].CanExecute(null));
        Assert.IsTrue(viewModel.Commands["NewMoveableMenu_Click"].CanExecute(null));
    }

    [TestMethod]
    public void DirtyStateNotifiesAndEnablesSavingUntilSaveCompletes()
    {
        using var scope = new EditorScope();
        scope.Tool.DestinationWad = new Wad2 { FileName = "loaded.wad2" };
        int invoked = 0;
        int availabilityChanges = 0;
        using var viewModel = CreateViewModel(scope.Tool, () => invoked++);
        viewModel.Commands["SaveMenu_Click"].CanExecuteChanged += (_, _) => availabilityChanges++;

        scope.Tool.ToggleUnsavedChanges(true);
        Assert.IsTrue(viewModel.TryExecute("SaveMenu_Click"));
        scope.Tool.ToggleUnsavedChanges(false);

        Assert.IsFalse(viewModel.CanSave);
        Assert.AreEqual(1, invoked);
        Assert.AreEqual(2, availabilityChanges);
    }

    [TestMethod]
    public void CopyAndDeleteRequireTheirOwnSelectionAndADestination()
    {
        using var scope = new EditorScope();
        scope.Tool.DestinationWad = new Wad2();
        using var viewModel = CreateViewModel(scope.Tool, () => { });
        Assert.IsFalse(viewModel.Commands["DeleteObjectButton_Click"].CanExecute(null));
        Assert.IsFalse(viewModel.Commands["AddObjectButton_Click"].CanExecute(null));

        viewModel.HasDestinationSelection = true;
        Assert.IsTrue(viewModel.Commands["DeleteObjectButton_Click"].CanExecute(null));
        Assert.IsFalse(viewModel.Commands["AddObjectButton_Click"].CanExecute(null));
        viewModel.HasSourceSelection = true;
        Assert.IsTrue(viewModel.Commands["AddObjectButton_Click"].CanExecute(null));
    }

    [TestMethod]
    public void DisposedShellStopsReceivingAvailabilityNotifications()
    {
        using var scope = new EditorScope();
        var viewModel = CreateViewModel(scope.Tool, () => { });
        int changes = 0;
        viewModel.Commands["SaveMenu_Click"].CanExecuteChanged += (_, _) => changes++;
        viewModel.Dispose();

        scope.Tool.ToggleUnsavedChanges(true);

        Assert.AreEqual(0, changes);
    }

    private static MainWindowViewModel CreateViewModel(WadToolClass tool, Action action) => new(tool, new Dictionary<string, Action>
    {
        ["AddObjectButton_Click"] = action,
        ["DeleteObjectButton_Click"] = action,
        ["NewMoveableMenu_Click"] = action,
        ["NewWad2Menu_Click"] = action,
        ["SaveAsMenu_Click"] = action,
        ["SaveMenu_Click"] = action
    });

    private sealed class EditorScope : IDisposable
    {
        private readonly SynchronizationContext? _previousContext = SynchronizationContext.Current;

        public WadToolClass Tool { get; } = new(new Configuration());

        public EditorScope() => SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

        public void Dispose()
        {
            Tool.Dispose();
            SynchronizationContext.SetSynchronizationContext(_previousContext);
        }
    }
}
