using System.Drawing.Imaging;
using TombLib.Icons;

namespace TombLib.Test;

[TestClass]
public class IconLoadingTests
{
    [TestMethod]
    public void VectorLoadingPreservesSquareViewportAndPadding()
    {
        var image = IconSources.Load("Misc/minus_math");

        Assert.AreEqual(16.0, image.Width);
        Assert.AreEqual(16.0, image.Height);
        Assert.IsTrue(image.IsFrozen);
        Assert.AreSame(image, IconSources.Load("Misc/minus_math"));
    }
    [TestMethod]
    public void EveryEmbeddedVectorLoadsWithMatchingBitmapBuckets()
    {
        const string prefix = "TombLib.Icons.Svg.";
        var resources = typeof(IconSources).Assembly.GetManifestResourceNames().Where(name => name.StartsWith(prefix, StringComparison.Ordinal));
        int count = 0;

        foreach (var resource in resources)
        {
            var id = IconId.Parse(resource[prefix.Length..^4].Replace('.', '/'));
            var image = IconSources.Load(id);

            Assert.IsTrue(image.IsFrozen, id.Path);
            Assert.AreEqual(16.0, image.Width, 0.001, id.Path);
            Assert.AreEqual(16.0, image.Height, 0.001, id.Path);

            for (int bucket = 1; bucket <= 4; bucket++)
            {
                var bitmap = IconBitmaps.LoadBucket(id, bucket);
                Assert.AreEqual(16 * bucket, bitmap.Width, id.Path);
                Assert.AreEqual(16 * bucket, bitmap.Height, id.Path);
            }

            count++;
        }

        Assert.IsTrue(count > 0);
    }

    [DataTestMethod]
    [DataRow("Actions/Play", "Actions", "Play")]
    [DataRow("Actions/Playback/Play", "Actions/Playback", "Play")]
    [DataRow(@"Actions\Playback\Play", "Actions/Playback", "Play")]
    public void ParsePreservesCategorySubdirectoriesAndCase(string path, string category, string name)
    {
        var id = IconId.Parse(path);

        Assert.AreEqual(category, id.Category);
        Assert.AreEqual(name, id.Name);
        Assert.AreEqual($"{category}/{name}", id.Path);
        Assert.AreEqual(id.Path, id.ToString());
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow("Play")]
    [DataRow("/Actions/Play")]
    [DataRow("Actions/Play/")]
    [DataRow("Actions//Play")]
    [DataRow("Actions/../Play")]
    [DataRow("Actions/./Play")]
    [DataRow("Actions/ /Play")]
    [DataRow("C:/Actions/Play")]
    [DataRow("Actions/Pl?ay")]
    public void ParseRejectsInvalidRelativePaths(string path)
    {
        Assert.ThrowsException<ArgumentException>(() => IconId.Parse(path));
    }

    [TestMethod]
    public void LoadersRejectDefaultIdentifiersBeforeResourceLookup()
    {
        Assert.ThrowsException<ArgumentException>(() => IconBitmaps.Load(default(IconId)));
        Assert.ThrowsException<ArgumentException>(() => IconSources.Load(default(IconId)));
    }

    [TestMethod]
    public void NestedMissingResourcesUseManifestSeparators()
    {
        var id = IconId.Parse("Missing/Nested/Icon");
        var bitmapError = Assert.ThrowsException<FileNotFoundException>(() => IconBitmaps.Load(id));
        var vectorError = Assert.ThrowsException<FileNotFoundException>(() => IconSources.Load(id));

        StringAssert.Contains(bitmapError.Message, "TombLib.Icons.Png._1x.Missing.Nested.Icon.png");
        StringAssert.Contains(vectorError.Message, "TombLib.Icons.Svg.Missing.Nested.Icon.svg");
    }

    [TestMethod]
    public void BitmapRemainsReadableAndSaveableAfterResourceStreamCloses()
    {
        var bitmap = IconBitmaps.Load("Misc/action_stop");
        var pixel = bitmap.GetPixel(0, 0);
        using var output = new MemoryStream();
        bitmap.Save(output, ImageFormat.Png);
        output.Position = 0;
        using var copy = new System.Drawing.Bitmap(output);

        Assert.AreEqual(bitmap.Size, copy.Size);
        Assert.AreEqual(pixel, copy.GetPixel(0, 0));
        Assert.AreEqual(IconId.Parse("Misc/action_stop"), bitmap.Tag);
        Assert.AreSame(bitmap, IconBitmaps.Load(@"Misc\action_stop"));
    }

    [DataTestMethod]
    [DataRow(1, 16)]
    [DataRow(2, 32)]
    [DataRow(3, 48)]
    [DataRow(4, 64)]
    public void BitmapBucketsKeepTheirExpectedDimensions(int bucket, int size)
    {
        var bitmap = IconBitmaps.LoadBucket(IconId.Parse("Misc/action_stop"), bucket);

        Assert.AreEqual(size, bitmap.Width);
        Assert.AreEqual(size, bitmap.Height);
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(5)]
    public void BitmapRejectsInvalidBuckets(int bucket)
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => IconBitmaps.LoadBucket(IconId.Parse("Misc/action_stop"), bucket));
    }
}
