using System.Xml.Linq;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF.Services;

namespace TombLib.Test;

[TestClass]
public class MaterialEditorStorageTests
{
    [TestMethod]
    public void FailedReplacementPreservesExistingSidecarAndRemovesTemporaryFile()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"MaterialEditor-{Guid.NewGuid():N}")).FullName;
        var texturePath = Path.Combine(directory, "texture.png");
        var sidecarPath = Path.ChangeExtension(texturePath, ".xml");
        try
        {
            File.WriteAllText(sidecarPath, "Original sidecar");
            using (var fileLock = new FileStream(sidecarPath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                var material = new MaterialData { Type = 77, Name = "Replacement", ColorMap = texturePath };
                bool replacementFailed = false;
                try
                {
                    new MaterialEditorStorage().Save(texturePath, material);
                }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                {
                    replacementFailed = true;
                }

                Assert.IsTrue(replacementFailed);
            }

            Assert.AreEqual("Original sidecar", File.ReadAllText(sidecarPath));
            Assert.AreEqual(0, Directory.GetFiles(directory, ".material-*.xml").Length);
        }
        finally
        {
            foreach (var path in Directory.GetFiles(directory))
                File.Delete(path);

            Directory.Delete(directory);
        }
    }

    [TestMethod]
    public void DefaultMaterialRemovesRedundantSidecar()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"MaterialEditor-{Guid.NewGuid():N}")).FullName;
        var texturePath = Path.Combine(directory, "texture.png");
        var sidecarPath = Path.ChangeExtension(texturePath, ".xml");
        try
        {
            File.WriteAllText(sidecarPath, "Previous sidecar");
            var material = new MaterialData { ColorMap = texturePath };
            material.Normalize();

            Assert.IsNull(new MaterialEditorStorage().Save(texturePath, material));
            Assert.IsFalse(File.Exists(sidecarPath));
        }
        finally
        {
            foreach (var path in Directory.GetFiles(directory))
                File.Delete(path);

            Directory.Delete(directory);
        }
    }
    [TestMethod]
    public void SavePreservesMissingMapReferencesWithoutMutatingTheDocument()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"MaterialEditor-{Guid.NewGuid():N}")).FullName;
        var texturePath = Path.Combine(directory, "texture.png");
        var mapPath = Path.Combine(directory, "maps", "missing.png");
        var material = new MaterialData
        {
            Name = "Custom material", ColorMap = texturePath, NormalMap = mapPath,
            AlphaMaskMap = mapPath, AdditionalColorMap = mapPath
        };
        try
        {
            var sidecarPath = new MaterialEditorStorage().Save(texturePath, material)
                ?? throw new AssertFailedException("The custom material sidecar was removed.");
            var document = XDocument.Load(sidecarPath);
            var expectedMapPath = Path.Combine("maps", "missing.png");

            Assert.AreEqual(expectedMapPath, document.Root?.Element(nameof(MaterialData.NormalMap))?.Value);
            Assert.AreEqual(expectedMapPath, document.Root?.Element(nameof(MaterialData.AlphaMaskMap))?.Value);
            Assert.AreEqual(expectedMapPath, document.Root?.Element(nameof(MaterialData.AdditionalColorMap))?.Value);
            Assert.AreEqual(mapPath, material.NormalMap);
            Assert.AreEqual(mapPath, material.AlphaMaskMap);
            Assert.AreEqual(mapPath, material.AdditionalColorMap);
            Assert.AreEqual(texturePath, material.ColorMap);
        }
        finally
        {
            foreach (var path in Directory.GetFiles(directory))
                File.Delete(path);

            Directory.Delete(directory);
        }
    }

    [TestMethod]
    public void LoadResolvesAlphaAndAdditionalMapsRelativeToTheSidecar()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"MaterialEditor-{Guid.NewGuid():N}")).FullName;
        var texturePath = Path.Combine(directory, "texture.png");
        var mapPath = Path.Combine("maps", "missing.png");
        try
        {
            var material = new MaterialData
            {
                ColorMap = "texture.png", NormalMap = mapPath, AlphaMaskMap = mapPath, AdditionalColorMap = mapPath
            };
            XmlUtils.WriteXmlFile(Path.ChangeExtension(texturePath, ".xml"), material);

            var loaded = new MaterialEditorStorage().Load(new TestTexture(texturePath), texturePath);

            Assert.AreEqual(Path.Combine(directory, mapPath), loaded.NormalMap);
            Assert.AreEqual(Path.Combine(directory, mapPath), loaded.AlphaMaskMap);
            Assert.AreEqual(Path.Combine(directory, mapPath), loaded.AdditionalColorMap);
        }
        finally
        {
            foreach (var path in Directory.GetFiles(directory))
                File.Delete(path);

            Directory.Delete(directory);
        }
    }

    private sealed class TestTexture : Texture
    {
        public TestTexture(string path) => AbsolutePath = path;
        public override Texture Clone() => new TestTexture(AbsolutePath);
    }
}
