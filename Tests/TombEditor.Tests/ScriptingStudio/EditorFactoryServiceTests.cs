using Moq;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombLib.Scripting.UI.Editors;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class EditorFactoryServiceTests
{
    [TestMethod]
    public void CreateEditor_UsesRegistrationOrderForOverlappingFilePredicates()
    {
        var factory = new EditorFactoryService();
        var firstEditor = CreateEditor();
        var secondEditor = CreateEditor();
        factory.Register(CreateRegistration(
            DocumentMode.ClassicScript,
            _ => true,
            _ => false,
            firstEditor));
        factory.Register(CreateRegistration(
            DocumentMode.Lua,
            _ => true,
            _ => false,
            secondEditor));

        IEditorControl createdEditor = factory.CreateEditor("script.txt", EditorType.Default, new Version(1, 0));

        Assert.AreSame(firstEditor, createdEditor);
        Assert.AreEqual(DocumentMode.ClassicScript, factory.GetDocumentRegistration(createdEditor)!.DocumentMode);
    }

    [TestMethod]
    public void CreateEditor_PrefersDefaultRegistrationBeforeRegistrationOrder()
    {
        var factory = new EditorFactoryService();
        var generalEditor = CreateEditor();
        var defaultEditor = CreateEditor();
        factory.Register(CreateRegistration(
            DocumentMode.PlainText,
            _ => true,
            _ => false,
            generalEditor));
        factory.Register(CreateRegistration(
            DocumentMode.Lua,
            _ => true,
            _ => true,
            defaultEditor));

        IEditorControl createdEditor = factory.CreateEditor("script.txt", EditorType.Default, new Version(1, 0));

        Assert.AreSame(defaultEditor, createdEditor);
        Assert.AreEqual(DocumentMode.Lua, factory.GetDocumentRegistration(createdEditor)!.DocumentMode);
    }

    [TestMethod]
    public void CreateEditor_ExplicitEditorTypeUsesMatchingRegistrationBeforeDefault()
    {
        var factory = new EditorFactoryService();
        var defaultEditor = CreateEditor();
        var stringsEditor = CreateEditor(EditorType.Strings);
        factory.Register(CreateRegistration(
            DocumentMode.Lua,
            _ => true,
            _ => true,
            defaultEditor));
        factory.Register(CreateRegistration(
            DocumentMode.Strings,
            _ => true,
            _ => false,
            stringsEditor,
            EditorType.Strings));

        IEditorControl createdEditor = factory.CreateEditor("script.txt", EditorType.Strings, new Version(1, 0));

        Assert.AreSame(stringsEditor, createdEditor);
        Assert.AreEqual(DocumentMode.Strings, factory.GetDocumentRegistration(createdEditor)!.DocumentMode);
    }

    [TestMethod]
    public void GetSourceViewEditorType_UsesTextRegistrationAndFallsBackToDefault()
    {
        var factory = new EditorFactoryService();
        factory.Register(CreateRegistration(
            DocumentMode.Lua,
            _ => true,
            _ => true,
            CreateEditor()));

        Assert.AreEqual(EditorType.Text, factory.GetSourceViewEditorType("script.lua"));

        var emptyFactory = new EditorFactoryService();
        Assert.AreEqual(EditorType.Text, emptyFactory.GetSourceViewEditorType("unknown.bin"));
    }

    [TestMethod]
    public void CreateEditor_UsesPlainTextFactoryForUnsupportedFile()
    {
        var factory = new EditorFactoryService();
        var plainTextEditor = CreateEditor();
        factory.SetPlainTextEditorFactory(_ => plainTextEditor, DocumentMode.PlainText, ScriptingDocumentContributions.None);

        IEditorControl createdEditor = factory.CreateEditor("unknown.bin", EditorType.Default, new Version(1, 0));

        Assert.AreSame(plainTextEditor, createdEditor);
        Assert.AreEqual(DocumentMode.PlainText, factory.GetDocumentRegistration(createdEditor)!.DocumentMode);
    }

    [TestMethod]
    public void GetDocumentRegistration_DoesNotInferRegistrationFromEditorType()
    {
        var factory = new EditorFactoryService();
        factory.Register(CreateRegistration(DocumentMode.ClassicScript, _ => true, _ => true, CreateEditor()));
        IEditorControl unassociatedEditor = CreateEditor();

        Assert.IsNull(factory.GetDocumentRegistration(unassociatedEditor));
        Assert.AreSame(ScriptingDocumentContributions.None, factory.GetDocumentContributions(unassociatedEditor));
    }

    [TestMethod]
    public void RegistrationAssociation_DoesNotKeepCreatedEditorAlive()
    {
        WeakReference editorReference = CreateEditorReferenceWithoutExternalOwner();

        StaTestHelper.AssertCollected(editorReference, "created editor");
    }

    [TestMethod]
    public void Registration_RequiresExplicitContributions()
    {
        var registration = new ScriptingDocumentRegistration(
            EditorType.Text,
            DocumentMode.Lua,
            _ => true,
            _ => true,
            _ => CreateEditor(),
            new(ScriptingSettingsPageKind.Lua, ScriptingDocumentConfigurationKind.Lua));

        Assert.AreEqual(ScriptingSettingsPageKind.Lua, registration.Contributions.SettingsPageKind);
        Assert.AreEqual(ScriptingDocumentConfigurationKind.Lua, registration.Contributions.ConfigurationKind);
        Assert.IsFalse(registration.IsFallback);
    }

    [TestMethod]
    public void Registration_PlainTextFallbackHasExplicitIdentity()
    {
        var registration = new ScriptingDocumentRegistration(
            EditorType.Text,
            DocumentMode.PlainText,
            _ => false,
            _ => false,
            _ => CreateEditor(),
            ScriptingDocumentContributions.None,
            isFallback: true);

        Assert.AreEqual(DocumentMode.PlainText, registration.DocumentMode);
        Assert.AreEqual(ScriptingDocumentConfigurationKind.None, registration.Contributions.ConfigurationKind);
        Assert.IsTrue(registration.IsFallback);
    }

    [TestMethod]
    public void CreateEditor_ProvidesContributionsForResolvedRegistration()
    {
        var factory = new EditorFactoryService();
        IEditorControl luaEditor = CreateEditor();
        ScriptingDocumentRegistration registration = CreateRegistration(
            DocumentMode.Lua,
            _ => true,
            _ => true,
            luaEditor,
            contributions: new(ScriptingSettingsPageKind.Lua, ScriptingDocumentConfigurationKind.Lua));
        factory.Register(registration);

        IEditorControl createdEditor = factory.CreateEditor("script.lua", EditorType.Default, new Version(1, 0));

        Assert.AreSame(registration, factory.GetDocumentRegistration(createdEditor));
        Assert.AreEqual(ScriptingSettingsPageKind.Lua, factory.GetDocumentContributions(createdEditor).SettingsPageKind);
        Assert.AreEqual(ScriptingDocumentConfigurationKind.Lua, factory.GetDocumentContributions(createdEditor).ConfigurationKind);
    }

    [TestMethod]
    public void CreateEditor_DefaultPlainTextFallbackProvidesExplicitIdentity()
    {
        var factory = new EditorFactoryService();
        IEditorControl plainTextEditor = CreateEditor();
        factory.SetPlainTextEditorFactory(_ => plainTextEditor, DocumentMode.PlainText, ScriptingDocumentContributions.None);

        IEditorControl createdEditor = factory.CreateEditor("unknown.bin", EditorType.Default, new Version(1, 0));

        Assert.AreSame(plainTextEditor, createdEditor);
        Assert.AreEqual(DocumentMode.PlainText, factory.GetDocumentRegistration(createdEditor)!.DocumentMode);
        Assert.AreEqual(ScriptingDocumentConfigurationKind.None, factory.GetDocumentContributions(createdEditor).ConfigurationKind);
    }

    private static ScriptingDocumentRegistration CreateRegistration(
        DocumentMode documentMode,
        Func<string, bool> supportsFile,
        Func<string, bool> isDefaultForFile,
        IEditorControl editor,
        EditorType editorType = EditorType.Text,
        ScriptingDocumentContributions? contributions = null)
        => new(editorType, documentMode, supportsFile, isDefaultForFile, _ => editor, contributions ?? ScriptingDocumentContributions.None);

    private static IEditorControl CreateEditor(EditorType editorType = EditorType.Text)
    {
        var mock = new Mock<IEditorControl>();
        mock.SetupGet(editor => editor.EditorType).Returns(editorType);
        return mock.Object;
    }

    private static WeakReference CreateEditorReferenceWithoutExternalOwner()
    {
        var factory = new EditorFactoryService();
        factory.Register(new ScriptingDocumentRegistration(
            EditorType.Text,
            DocumentMode.ClassicScript,
            _ => true,
            _ => true,
            _ => CreateEditor(),
            ScriptingDocumentContributions.None));

        IEditorControl editor = factory.CreateEditor("script.txt", EditorType.Default, new Version(1, 0));
        return new WeakReference(editor);
    }
}