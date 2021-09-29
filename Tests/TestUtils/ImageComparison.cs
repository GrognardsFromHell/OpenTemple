using System.IO;
using Codeuctivity;
using NUnit.Framework;
using NUnit.Framework.Internal;
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
            var imageName = imageBasename + "_actual.png";
            image.Save(imageName);

            var expectedPath = TestData.GetPath(referenceName);

            if (!File.Exists(expectedPath))
            {
                File.Copy(imageName, expectedPath);
                throw new NUnitException("Reference file didn't exist: " + expectedPath
                                                                         + " copied actual to its location");
            }

            var imageDiff = ImageSharpCompare.CalcDiff(imageName, expectedPath);
            if (imageDiff.PixelErrorCount > 0)
            {
                // Re-save the expected image so it is next to the actual and difference
                File.Delete(imageBasename + "_expected.png");
                File.Copy(expectedPath, imageBasename + "_expected.png");

                // Calculate the difference as an image
                var visualDifference = imageBasename + "_difference.png";
                using (var maskImage = ImageSharpCompare.CalcDiffMaskImage(imageName, expectedPath))
                    maskImage.SaveAsPng(visualDifference);

                TestContext.AddTestAttachment(imageName, "Actual image");
                TestContext.AddTestAttachment(imageBasename + "_expected.png", "Expected image");
                TestContext.AddTestAttachment(visualDifference, "Visual difference");
                Assert.AreEqual(0, imageDiff.PixelErrorCount, "Images are different, see "
                                                              + imageName + " and " + visualDifference);
            }
        }
    }
}