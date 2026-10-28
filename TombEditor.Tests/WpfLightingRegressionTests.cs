using System.Numerics;
using TombEditor.Features.Dialogs.Transform;
using TombLib.LevelData;
using TombLib.WPF.Services;

namespace TombEditor.Tests;

[TestClass]
[DoNotParallelize]
public class WpfLightingRegressionTests
{
    [TestMethod]
    public void LightPropertyUndoAndRedoRestoreAllEditableProperties()
    {
        using var editor = CreateEditor();
        var light = AddLight(editor);
        var original = (LightInstance)light.Clone();
        var undo = new ChangeObjectPropertyUndoInstance(editor.UndoManager, light);

        light.Color = new Vector3(0.25f, 0.5f, 0.75f);
        light.Type = LightType.Spot;
        light.Quality = LightQuality.High;
        light.Intensity = 0.75f;
        light.InnerRange = 2.0f;
        light.OuterRange = 8.0f;
        light.InnerAngle = 15.0f;
        light.OuterAngle = 45.0f;
        light.RotationX = 30.0f;
        light.RotationY = 90.0f;
        light.Enabled = false;
        light.IsObstructedByRoomGeometry = false;
        light.IsDynamicallyUsed = false;
        light.IsStaticallyUsed = false;
        light.IsUsedForImportedGeometry = false;
        light.CastDynamicShadows = true;

        var changed = (LightInstance)light.Clone();
        var redo = undo.RedoInstance();
        undo.UndoAction();

        AssertLightPropertiesEqual(original, light);
        Assert.IsTrue(light.Room.PendingRelight);

        redo.UndoAction();

        AssertLightPropertiesEqual(changed, light);
    }

    [TestMethod]
    public void DynamicLightChangeCanRecordUndoWithoutInvalidatingLighting()
    {
        using var editor = CreateEditor();
        var light = AddLight(editor);
        editor.SelectedObject = light;

        EditorActions.UpdateLight<bool>((current, value) => current.IsDynamicallyUsed == value,
            (current, value) => current.IsDynamicallyUsed = value, _ => false, pushUndo: true, updateLighting: false);

        Assert.IsFalse(light.IsDynamicallyUsed);
        Assert.IsFalse(light.Room.PendingRelight);
        Assert.IsTrue(editor.UndoManager.UndoPossible);

        editor.UndoManager.Undo();

        Assert.IsTrue(light.IsDynamicallyUsed);
    }

    [TestMethod]
    public void ConfirmingDeferredLightTransformInvalidatesLighting()
    {
        using var editor = CreateEditor();
        var light = AddLight(editor);
        var viewModel = CreateTransform(editor, light);

        viewModel.TranslationX += 128;

        Assert.AreEqual(128.0f, light.Position.X);
        Assert.IsFalse(light.Room.PendingRelight);

        viewModel.ConfirmCommand.Execute(null);
        viewModel.OnClosing();

        Assert.AreEqual(128.0f, light.Position.X);
        Assert.IsTrue(light.Room.PendingRelight);
        Assert.AreEqual(true, viewModel.DialogResult);
    }

    [TestMethod]
    public void ClosingWithoutConfirmationRestoresTransform()
    {
        using var editor = CreateEditor();
        var light = AddLight(editor);
        light.Position = new Vector3(128.0f, 256.0f, 512.0f);
        light.RotationX = 15.0f;
        light.RotationY = 30.0f;
        var viewModel = CreateTransform(editor, light);

        viewModel.TranslationX += 128;
        viewModel.TranslationY += 64;
        viewModel.RotationX = 45;
        viewModel.RotationY = 90;
        viewModel.OnClosing();

        Assert.AreEqual(new Vector3(128.0f, 256.0f, 512.0f), light.Position);
        Assert.AreEqual(15.0f, light.RotationX);
        Assert.AreEqual(30.0f, light.RotationY);
        Assert.AreNotEqual(true, viewModel.DialogResult);
    }

    [TestMethod]
    public void CancelAndSubsequentClosingRestoreAndRelightOnlyOnce()
    {
        using var editor = CreateEditor();
        var light = AddLight(editor);
        editor.Mode = EditorMode.Lighting;
        var viewModel = CreateTransform(editor, light);
        viewModel.TranslationX += 128;
        int lightingEvents = 0;
        editor.EditorEventRaised += eventObject =>
        {
            if (eventObject is Editor.RoomLightingChangedEvent)
                lightingEvents++;
        };

        viewModel.CancelCommand.Execute(null);
        viewModel.OnClosing();

        Assert.AreEqual(Vector3.Zero, light.Position);
        Assert.AreEqual(1, lightingEvents);
        Assert.IsFalse(light.Room.PendingRelight);
    }

    [TestMethod]
    public void ConfirmWithoutChangesDoesNotCreateUndoOrDirtyLevel()
    {
        using var editor = CreateEditor();
        var light = AddLight(editor);
        var viewModel = CreateTransform(editor, light);
        editor.HasUnsavedChanges = false;

        viewModel.ConfirmCommand.Execute(null);
        viewModel.OnClosing();

        Assert.IsFalse(editor.UndoManager.UndoPossible);
        Assert.IsFalse(editor.HasUnsavedChanges);
        Assert.IsFalse(light.Room.PendingRelight);
    }

    [TestMethod]
    public void ClosingAfterLeavingLightingModeInvalidatesRestoredPreview()
    {
        using var editor = CreateEditor();
        var light = AddLight(editor);
        editor.Mode = EditorMode.Lighting;
        var viewModel = CreateTransform(editor, light);
        viewModel.TranslationX += 128;
        Assert.IsFalse(light.Room.PendingRelight);

        editor.Mode = EditorMode.Geometry;
        viewModel.OnClosing();

        Assert.AreEqual(Vector3.Zero, light.Position);
        Assert.IsTrue(light.Room.PendingRelight);
    }

    [TestMethod]
    public void ReturningToOriginalTransformAfterImmediatePreviewStillInvalidatesOnConfirm()
    {
        using var editor = CreateEditor();
        var light = AddLight(editor);
        editor.Mode = EditorMode.Lighting;
        var viewModel = CreateTransform(editor, light);
        decimal originalX = viewModel.TranslationX;
        viewModel.TranslationX += 128;

        editor.Mode = EditorMode.Geometry;
        viewModel.TranslationX = originalX;
        viewModel.ConfirmCommand.Execute(null);

        Assert.AreEqual(Vector3.Zero, light.Position);
        Assert.IsTrue(light.Room.PendingRelight);
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void ClosingWithoutChangesDoesNotDirtyLevel(bool useCancel)
    {
        using var editor = CreateEditor();
        var light = AddLight(editor);
        var viewModel = CreateTransform(editor, light);
        editor.HasUnsavedChanges = false;

        if (useCancel)
            viewModel.CancelCommand.Execute(null);

        viewModel.OnClosing();

        Assert.IsFalse(editor.HasUnsavedChanges);
        Assert.IsFalse(editor.UndoManager.UndoPossible);
        Assert.IsFalse(light.Room.PendingRelight);
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void ReturningToOriginalTransformAfterImmediatePreviewStillInvalidatesOnClose(bool useCancel)
    {
        using var editor = CreateEditor();
        var light = AddLight(editor);
        editor.Mode = EditorMode.Lighting;
        var viewModel = CreateTransform(editor, light);
        decimal originalX = viewModel.TranslationX;
        viewModel.TranslationX += 128;

        editor.Mode = EditorMode.Geometry;
        viewModel.TranslationX = originalX;

        if (useCancel)
            viewModel.CancelCommand.Execute(null);

        viewModel.OnClosing();

        Assert.AreEqual(Vector3.Zero, light.Position);
        Assert.IsTrue(light.Room.PendingRelight);
    }

    private static Editor CreateEditor() =>
        new(new ImmediateSynchronizationContext(), new Configuration(), Level.CreateSimpleLevel());

    private static LightInstance AddLight(Editor editor)
    {
        var room = editor.Level.Rooms[0];
        var light = new LightInstance(LightType.Point);
        room.AddObject(editor.Level, light);
        room.RebuildLighting(highQualityLighting: false);
        return light;
    }

    private static TransformWindowViewModel CreateTransform(Editor editor, LightInstance light) =>
        new(light, editor, new LocalizationService());

    private static void AssertLightPropertiesEqual(LightInstance expected, LightInstance actual)
    {
        Assert.AreEqual(expected.Color, actual.Color);
        Assert.AreEqual(expected.Type, actual.Type);
        Assert.AreEqual(expected.Quality, actual.Quality);
        Assert.AreEqual(expected.Intensity, actual.Intensity);
        Assert.AreEqual(expected.InnerRange, actual.InnerRange);
        Assert.AreEqual(expected.OuterRange, actual.OuterRange);
        Assert.AreEqual(expected.InnerAngle, actual.InnerAngle);
        Assert.AreEqual(expected.OuterAngle, actual.OuterAngle);
        Assert.AreEqual(expected.RotationX, actual.RotationX);
        Assert.AreEqual(expected.RotationY, actual.RotationY);
        Assert.AreEqual(expected.Enabled, actual.Enabled);
        Assert.AreEqual(expected.IsObstructedByRoomGeometry, actual.IsObstructedByRoomGeometry);
        Assert.AreEqual(expected.IsDynamicallyUsed, actual.IsDynamicallyUsed);
        Assert.AreEqual(expected.IsStaticallyUsed, actual.IsStaticallyUsed);
        Assert.AreEqual(expected.IsUsedForImportedGeometry, actual.IsUsedForImportedGeometry);
        Assert.AreEqual(expected.CastDynamicShadows, actual.CastDynamicShadows);
    }

    private sealed class ImmediateSynchronizationContext : SynchronizationContext
    {
        public override void Send(SendOrPostCallback callback, object? state) => callback(state);
    }
}
