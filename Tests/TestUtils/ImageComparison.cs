using Codeuctivity;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenTemple.Tests.TestUtils
{
    public static class ImageComparison
    {
        public static void AssertImagesEqual<TPixel>(Image<TPixel> image,
            string referenceName) where TPixel : unmanaged, IPixel<TPixel>
        {
            var imageBasename = TestContext.CurrentContext.Test.FullName;
            var imageName = imageBasename + ".png";
            image.Save(imageName);

            var expectedPath = TestData.GetPath(referenceName);
            var imageDiff = ImageSharpCompare.CalcDiff(imageName, expectedPath);
            if (imageDiff.PixelErrorCount > 0)
            {
                // Calculate the difference as an image
                var visualDifference = imageBasename + "_difference.png";
                using (var maskImage = ImageSharpCompare.CalcDiffMaskImage(imageName, expectedPath))
                    maskImage.SaveAsPng(visualDifference);

                TestContext.AddTestAttachment(imageName, "Actual image");
                TestContext.AddTestAttachment(expectedPath, "Expected image");
                TestContext.AddTestAttachment(visualDifference, "Visual difference");
                Assert.AreEqual(0, imageDiff.PixelErrorCount, "Images are different, see "
                                                              + imageName + " and " + visualDifference);
            }
        }
    }
}