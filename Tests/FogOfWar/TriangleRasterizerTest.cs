using System;
using System.IO;
using System.Numerics;
using Codeuctivity;
using NUnit.Framework;
using OpenTemple.Core.Systems.FogOfWar;
using OpenTemple.Tests.TestUtils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenTemple.Tests.FogOfWar
{
    public class TriangleRasterizerTest
    {
        [Test]
        public void TestRasterization()
        {
            const int dimension = 192;
            var buffer = new byte[dimension * dimension];

            Span<Vector2> vertices = stackalloc Vector2[3]
            {
                new Vector2(5.5f, 10.2f),
                new Vector2(5.5f + 16, 10.2f + 5.0f),
                new Vector2(5.5f - 5, 10.2f + 5.0f)
            };

            TriangleRasterizer.Rasterize(dimension, dimension, buffer, vertices, 0xFF);

            using var image = Image.LoadPixelData<L8>(buffer, dimension, dimension);
            ImageComparison.AssertImagesEqual(image, "FogOfWar/tesselated_expected.png");

        }
    }
}