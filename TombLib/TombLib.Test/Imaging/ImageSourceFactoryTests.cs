using TombLib.Utils;
using TombLib.WPF.Imaging;

namespace TombLib.Test.Imaging;

[TestClass]
public class ImageSourceFactoryTests
{
    [TestMethod]
    public void PreviewPreservesColorChannelsAndTransparency()
    {
        var image = ImageC.CreateNew(2, 1);
        image.SetPixel(0, 0, 10, 20, 30, 40);
        image.SetPixel(1, 0, 50, 60, 70, 80);
        var bitmap = ImageSourceFactory.FromImage(image);

        Assert.IsNotNull(bitmap);
        var pixels = new byte[8];
        bitmap.CopyPixels(pixels, 8, 0);

        CollectionAssert.AreEqual(new byte[] { 30, 20, 10, 40, 70, 60, 50, 80 }, pixels);
        Assert.IsTrue(bitmap.IsFrozen);
    }

    [TestMethod]
    public void PreviewRemainsUnchangedWhenEditorImageIsModified()
    {
        var image = ImageC.CreateNew(1, 1);
        image.SetPixel(0, 0, 10, 20, 30, 40);
        var bitmap = ImageSourceFactory.FromImage(image);
        image.SetPixel(0, 0, 100, 110, 120, 130);

        Assert.IsNotNull(bitmap);
        var pixels = new byte[4];
        bitmap.CopyPixels(pixels, 4, 0);

        CollectionAssert.AreEqual(new byte[] { 30, 20, 10, 40 }, pixels);
    }

    [TestMethod]
    public void EmptyImageHasNoPreview() => Assert.IsNull(ImageSourceFactory.FromImage(ImageC.CreateNew(0, 0)));
}
