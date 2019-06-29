using System;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SpicyTemple.Core.Systems.FogOfWar;
using Xunit;

namespace SpicyTemple.Tests
{
    public class TriangleRasterizerTest
    {
        [Fact]
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

            using var image = Image.LoadPixelData<Gray8>(buffer, dimension, dimension);
            image.Save("tesselated.png");
        }
    }
}