using Moq;
using System.ComponentModel;
using TombLib.Forms.Services;
using TombLib.Forms.ViewModels;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF.Services.Abstract;

namespace TombLib.Test.ViewModels;

[TestClass]
public class MaterialEditorWindowViewModelTests
{
    [TestMethod]
    public void CancelDoesNotPersistPendingChanges()
    {
        var storage = CreateStorage();
        var viewModel = CreateViewModel(storage.Object);
        viewModel.MaterialName = "Changed";
        viewModel.CancelCommand.Execute(null);

        Assert.AreEqual(false, viewModel.DialogResult);
        storage.Verify(service => service.Save(It.IsAny<string>(), It.IsAny<MaterialData>()), Times.Never);
    }

    [TestMethod]
    public void FailedSaveKeepsDialogOpenAndDocumentDirty()
    {
        var storage = CreateStorage();
        storage.Setup(service => service.Save(It.IsAny<string>(), It.IsAny<MaterialData>())).Throws(new IOException("Locked sidecar"));
        var viewModel = CreateViewModel(storage.Object);
        viewModel.MaterialName = "Changed";
        viewModel.ConfirmCommand.Execute(null);

        Assert.IsNull(viewModel.DialogResult);
        Assert.IsTrue(viewModel.IsDirty);
        Assert.IsFalse(viewModel.MaterialChanged);
    }

    [TestMethod]
    public void SavingBeforeTextureSwitchRemainsReportedAfterCancel()
    {
        var storage = CreateStorage();
        var viewModel = CreateViewModel(storage.Object);
        viewModel.MaterialName = "Saved material";
        viewModel.SelectedTexture = viewModel.Textures[1];
        viewModel.MaterialName = "Unsaved material";
        viewModel.CancelCommand.Execute(null);

        Assert.IsTrue(viewModel.MaterialChanged);
        Assert.AreEqual(false, viewModel.DialogResult);
        storage.Verify(service => service.Save(It.IsAny<string>(), It.Is<MaterialData>(document => document.Name == "Saved material")), Times.Once);
        storage.Verify(service => service.Save(It.IsAny<string>(), It.Is<MaterialData>(document => document.Name == "Unsaved material")), Times.Never);
    }

    [TestMethod]
    public void UntouchedConfirmationDoesNotCreateSidecar()
    {
        var storage = CreateStorage();
        var viewModel = CreateViewModel(storage.Object);
        viewModel.ConfirmCommand.Execute(null);

        Assert.AreEqual(true, viewModel.DialogResult);
        Assert.IsFalse(viewModel.MaterialChanged);
        storage.Verify(service => service.Save(It.IsAny<string>(), It.IsAny<MaterialData>()), Times.Never);
    }

    [TestMethod]
    public void CancelledTextureSwitchKeepsPendingChanges()
    {
        var storage = CreateStorage();
        var viewModel = CreateViewModel(storage.Object, new TestMessages { Choice = 2 });
        var original = viewModel.SelectedTexture;
        viewModel.MaterialName = "Pending material";

        viewModel.SelectedTexture = viewModel.Textures[1];

        Assert.AreSame(original, viewModel.SelectedTexture);
        Assert.AreEqual("Pending material", viewModel.MaterialName);
        Assert.IsTrue(viewModel.IsDirty);
        storage.Verify(service => service.Save(It.IsAny<string>(), It.IsAny<MaterialData>()), Times.Never);
    }

    [TestMethod]
    public void DiscardedTextureSwitchDoesNotPersistPendingChanges()
    {
        var storage = CreateStorage();
        var viewModel = CreateViewModel(storage.Object, new TestMessages { Choice = 1 });
        viewModel.MaterialName = "Pending material";

        viewModel.SelectedTexture = viewModel.Textures[1];

        Assert.AreSame(viewModel.Textures[1], viewModel.SelectedTexture);
        Assert.IsFalse(viewModel.IsDirty);
        storage.Verify(service => service.Save(It.IsAny<string>(), It.IsAny<MaterialData>()), Times.Never);
    }

    [TestMethod]
    public void FailedLoadCannotOverwriteTheUnreadableSidecar()
    {
        var storage = CreateStorage();
        storage.Setup(service => service.Load(It.IsAny<Texture>(), "first.png"))
            .Returns(new MaterialData { ColorMap = "first.png", XmlMaterialFileName = "first.xml" });
        storage.Setup(service => service.Load(It.IsAny<Texture>(), "second.png"))
            .Throws(new InvalidDataException("Unreadable sidecar"));
        var viewModel = CreateViewModel(storage.Object);

        viewModel.SelectedTexture = viewModel.Textures[1];
        viewModel.MaterialName = "Accidental replacement";
        viewModel.ConfirmCommand.Execute(null);

        Assert.IsNull(viewModel.MaterialFileName);
        Assert.IsFalse(viewModel.CanEdit);
        Assert.IsFalse(viewModel.IsDirty);
        storage.Verify(service => service.Save("second.png", It.IsAny<MaterialData>()), Times.Never);
        viewModel.SelectedTexture = viewModel.Textures[0];
        Assert.IsTrue(viewModel.CanEdit);
    }
    private static Mock<IMaterialEditorStorage> CreateStorage()
    {
        var storage = new Mock<IMaterialEditorStorage>();
        storage.Setup(service => service.Load(It.IsAny<Texture>(), It.IsAny<string>()))
            .Returns((Texture texture, string path) => new MaterialData { ColorMap = path });
        storage.Setup(service => service.Save(It.IsAny<string>(), It.IsAny<MaterialData>())).Returns("material.xml");
        return storage;
    }

    private static MaterialEditorWindowViewModel CreateViewModel(IMaterialEditorStorage storage, IMessageService? messages = null)
    {
        var localization = new Mock<ILocalizationService>();
        localization.Setup(service => service.WithKeysFor(It.IsAny<INotifyPropertyChanged>())).Returns(localization.Object);
        localization.Setup(service => service[It.IsAny<string>()]).Returns((string key) => key);
        localization.Setup(service => service.Format(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Material error");
        return new MaterialEditorWindowViewModel([new TestTexture("first.png"), new TestTexture("second.png")],
            storage: storage, messages: messages ?? new TestMessages(), localization: localization.Object);
    }

    private sealed class TestTexture : Texture
    {
        public TestTexture(string path) => AbsolutePath = path;
        public override Texture Clone() => new TestTexture(AbsolutePath);
    }

    private sealed class TestMessages : IMessageService
    {
        public int Choice { get; init; }

        public void ShowInformation(string message, string caption = "Information") { }
        public void ShowError(string message, string caption = "Error") { }
        public bool ShowConfirmation(string message, string caption = "Are you sure?", bool? defaultValue = null, bool isRisky = false) => true;

        public T ShowConfirmation<T>(string message, string caption, T yesValue, T noValue, T? cancelValue = null,
            T? defaultValue = null, bool isRisky = false) where T : struct => Choice == 0 ? yesValue : Choice == 1 ? noValue : cancelValue ?? throw new InvalidOperationException("Cancel is unavailable.");
    }
}
