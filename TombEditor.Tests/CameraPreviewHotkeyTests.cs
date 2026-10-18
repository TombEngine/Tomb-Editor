using System.Windows.Forms;

namespace TombEditor.Tests;

[TestClass]
public class CameraPreviewHotkeyTests
{
    // Pinned to a layout so the default hotkeys do not depend on the machine running the tests.
    private static HotkeySets CreateDefaultHotkeys() => new(KeyboardLayout.Qwerty);

    [DataTestMethod]
    [DataRow("PreviewCamera")]
    [DataRow("Undo")]
    [DataRow("Redo")]
    [DataRow("SaveLevel")]
    [DataRow("SaveLevelAs")]
    public void IsHotkeyAllowedInCameraPreview_AllowsEveryHotkeyOfAllowedCommands(string command)
    {
        var hotkeys = CreateDefaultHotkeys();

        Assert.IsTrue(hotkeys[command].Count > 0, $"'{command}' has no default hotkey, so this test would pass vacuously.");

        foreach (var hotkey in hotkeys[command])
            Assert.IsTrue(CommandHandler.IsHotkeyAllowedInCameraPreview(hotkeys, hotkey.Keys), $"{hotkey} ({command}) should stay active in camera preview.");
    }

    [DataTestMethod]
    [DataRow("Delete")]
    [DataRow("Cut")]
    [DataRow("Copy")]
    [DataRow("Paste")]
    [DataRow("HighlightSplit1")]
    [DataRow("ToggleFlyMode")]
    public void IsHotkeyAllowedInCameraPreview_BlocksHotkeysOfOtherCommands(string command)
    {
        var hotkeys = CreateDefaultHotkeys();

        Assert.IsTrue(hotkeys[command].Count > 0, $"'{command}' has no default hotkey, so this test would pass vacuously.");

        foreach (var hotkey in hotkeys[command])
            Assert.IsFalse(CommandHandler.IsHotkeyAllowedInCameraPreview(hotkeys, hotkey.Keys), $"{hotkey} ({command}) should be disabled in camera preview.");
    }

    [TestMethod]
    public void IsHotkeyAllowedInCameraPreview_BlocksKeysWithoutAnyCommand()
    {
        var hotkeys = CreateDefaultHotkeys();

        Assert.IsFalse(CommandHandler.IsHotkeyAllowedInCameraPreview(hotkeys, Keys.None));
        Assert.IsFalse(CommandHandler.IsHotkeyAllowedInCameraPreview(hotkeys, Keys.Control | Keys.Shift | Keys.Alt | Keys.F12));
    }

    [TestMethod]
    public void IsHotkeyAllowedInCameraPreview_FollowsRemappedHotkeys()
    {
        var hotkeys = CreateDefaultHotkeys();
        var defaultUndo = hotkeys["Undo"].First().Keys;
        var remappedUndo = Keys.Control | Keys.U;

        hotkeys["Undo"] = [remappedUndo];

        Assert.IsTrue(CommandHandler.IsHotkeyAllowedInCameraPreview(hotkeys, remappedUndo));
        Assert.IsFalse(CommandHandler.IsHotkeyAllowedInCameraPreview(hotkeys, defaultUndo));
    }
}
