using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Serialization;
using TombLib.Input;
using CoreResources = TombLib.ResourcesC.ResourcesC;

namespace TombEditor.Tests;

[TestClass]
public class InputAndCoreMigrationTests
{
    [DataTestMethod]
    [DataRow("Return Enter", 13)]
    [DataRow("Capital CapsLock", 20)]
    [DataRow("KanaMode HanguelMode HangulMode", 21)]
    [DataRow("HanjaMode KanjiMode", 25)]
    [DataRow("IMEAccept IMEAceept", 30)]
    [DataRow("Prior PageUp", 33)]
    [DataRow("Next PageDown", 34)]
    [DataRow("Snapshot PrintScreen", 44)]
    [DataRow("OemSemicolon Oem1", 186)]
    [DataRow("OemQuestion Oem2", 191)]
    [DataRow("Oemtilde Oem3", 192)]
    [DataRow("OemOpenBrackets Oem4", 219)]
    [DataRow("OemPipe Oem5", 220)]
    [DataRow("OemCloseBrackets Oem6", 221)]
    [DataRow("OemQuotes Oem7", 222)]
    [DataRow("OemBackslash Oem102", 226)]
    public void ExistingKeyAliasesDeserializeToTheirOriginalVirtualKey(string aliases, int virtualKey)
    {
        var serializer = new XmlSerializer(typeof(EditorKey));

        foreach (var alias in aliases.Split(' '))
        {
            using var reader = new StringReader($"<EditorKey>{alias}</EditorKey>");
            Assert.AreEqual((EditorKey)virtualKey, serializer.Deserialize(reader), alias);
        }
    }

    [TestMethod]
    public void ModifierFlagsKeepExistingConfigValuesAndXmlNames()
    {
        Assert.AreEqual(65535, (int)EditorKey.KeyCode);
        Assert.AreEqual(-65536, (int)EditorKey.Modifiers);
        Assert.AreEqual(65536, (int)EditorKey.Shift);
        Assert.AreEqual(131072, (int)EditorKey.Control);
        Assert.AreEqual(262144, (int)EditorKey.Alt);

        var serializer = new XmlSerializer(typeof(EditorKey));
        const EditorKey shortcut = EditorKey.F5 | EditorKey.Shift | EditorKey.Control;
        using var writer = new StringWriter();
        serializer.Serialize(writer, shortcut);

        const string legacyFlags = "MButton ShiftKey Capital CapsLock Space Home D0 D4 D P T NumPad0 NumPad4 F1 F5 Shift Control";
        StringAssert.Contains(writer.ToString(), $">{legacyFlags}</EditorKey>");
        using var reader = new StringReader(writer.ToString());
        Assert.AreEqual(shortcut, serializer.Deserialize(reader));
    }

    [TestMethod]
    public void EmbeddedFontAndPaletteKeepTheirOriginalContents()
    {
        Assert.AreEqual(14856, CoreResources.font.Length);
        Assert.AreEqual(1920, CoreResources.palette.Length);
        Assert.AreEqual("6AAE1E6FAC371BA5D5B2D242028FD58121A96F325D7A6DBCFB90CC6045843B0C", Convert.ToHexString(SHA256.HashData(CoreResources.font)));
        Assert.AreEqual("09F25A74F7168AD797B5124175645C8179F4D3D0E03A06F5ED133C99332D6171", Convert.ToHexString(SHA256.HashData(CoreResources.palette)));
    }

    [TestMethod]
    public void EmbeddedReferenceTexturesRemainUsableAfterTheirStreamsClose()
    {
        AssertTexture(CoreResources.Extra_Tr5_pc, -15987700);
        AssertTexture(CoreResources.Font_pc, 0);
        AssertTexture(CoreResources.pcsky_raw, -6316129);
    }

    private static void AssertTexture(Bitmap bitmap, int centerPixel)
    {
        Assert.AreEqual(256, bitmap.Width);
        Assert.AreEqual(256, bitmap.Height);
        Assert.AreEqual(centerPixel, bitmap.GetPixel(128, 128).ToArgb());
    }
}
