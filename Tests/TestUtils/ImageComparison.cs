using System.IO;
using Codeuctivity;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OpenTemple.Tests.TestUtils;

public static class ImageComparison
{
    public static void AssertImagesEqual<TPixel>(Image<TPixel> actualImage,
        string referenceName) where TPixel : unmanaged, IPixel<TPixel>
    {
        var imageBasename = TestContext.CurrentContext.Test.FullName.Replace("OpenTemple.Tests.", "") 
                            + Path.GetFileNameWithoutExtension(referenceName);
        var actualPath = imageBasename + "_actual.png";
        actualImage.Save(actualPath);

        var expectedPath = TestData.GetPath(referenceName);

        if (!File.Exists(expectedPath))
        {
            File.Copy(actualPath, expectedPath);
            throw new NUnitException("Reference file didn't exist: " + expectedPath
                                                                     + " copied actual to its location");
        }

        using var expectedImage = Image.Load(expectedPath);
        using var expectedImage32 = expectedImage.CloneAs<Rgba32>();
        using var actualImage32 = actualImage.CloneAs<Rgba32>();
        
        var imageDiff = ImageSharpCompare.CalcDiff(actualImage32, expectedImage32);
        if (imageDiff.PixelErrorCount > 0)
        {
            // Re-save the expected image so it is next to the actual and difference
            File.Delete(imageBasename + "_expected.png");
            File.Copy(expectedPath, imageBasename + "_expected.png");

            // Calculate the difference as an image
            var visualDifference = imageBasename + "_difference.png";
            using (var maskImage = ImageSharpCompare.CalcDiffMaskImage(actualImage32, expectedImage32))
                maskImage.SaveAsPng(visualDifference);

            TestContext.AddTestAttachment(actualPath, "Actual image");
            TestContext.AddTestAttachment(imageBasename + "_expected.png", "Expected image");
            TestContext.AddTestAttachment(visualDifference, "Visual difference");
            Assert.AreEqual(0, imageDiff.PixelErrorCount, "Images are different, see "
                                                          + actualPath + " and " + visualDifference);
        }
    }
}