using System;
using System.Diagnostics;
using System.Numerics;

namespace SpicyTemple.Core.Systems.FogOfWar
{
    public static class TriangleRasterizer
    {
        [TempleDllLocation(0x10031a20)]
        public static void Rasterize(int bufferWidth, int bufferHeight, Span<byte> buffer,
            ReadOnlySpan<Vector2> vertices, byte flags)
        {
            // If any point of the triangle lies outside the buffer, don't do anything
            for (int i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];
                if (vertex.X < 0.0f || vertex.X >= bufferWidth
                                    || vertex.Y < 0.0f || vertex.Y >= bufferHeight)
                {
                    return;
                }
            }

            SortVerticesByY(vertices, out var v0, out var v1, out var v2);

            // Calculate the middle-point on the longer edge that'll be the new starting point for the bottom triangle
            var bottomTriRightX = v0.X + (v1.Y - v0.Y) * (v2.X - v0.X) / (v2.Y - v0.Y);
            if (bottomTriRightX < v1.X)
            {
                var tmp = v1.X;
                v1.X = bottomTriRightX;
                bottomTriRightX = tmp;
            }

            // Rasterize the upper half of the triangle
            if (v0.Y <= v1.Y)
            {
                var slopeLeft = (v1.X - v0.X) / (v1.Y - v0.Y);
                var slopeRight = (bottomTriRightX - v0.X) / (v1.Y - v0.Y);
                RasterizerTriangleHalf(bufferWidth, buffer, flags, v0.Y, v1.Y, v0.X, v0.X, slopeLeft, slopeRight);
            }

            // Rasterize the bottom half of the triangle
            if (v1.Y <= v2.Y)
            {
                var slopeLeft = (v2.X - v1.X) / (v2.Y - v1.Y);
                var slopeRight = (v2.X - bottomTriRightX) / (v2.Y - v1.Y);

                RasterizerTriangleHalf(bufferWidth, buffer, flags, v1.Y, v2.Y, v1.X, bottomTriRightX, slopeLeft,
                    slopeRight);
            }
        }

        private static void RasterizerTriangleHalf(int bufferStride, Span<byte> buffer, byte flags,
            float yMin, float yMax,
            float leftX, float rightX,
            float slopeLeft, float slopeRight)
        {
            for (int y = (int) yMin; y <= (int) yMax; y++)
            {
                if (leftX <= rightX)
                {
                    var idx = (int) (y * bufferStride + leftX);
                    var stripeWidth = (int) (rightX - leftX + 1);
                    buffer.Slice(idx, stripeWidth).Fill(flags);
                }

                leftX += slopeLeft;
                rightX += slopeRight;
            }
        }

        private static void SortVerticesByY(ReadOnlySpan<Vector2> vertices, out Vector2 v0, out Vector2 v1,
            out Vector2 v2)
        {
            Debug.Assert(vertices.Length == 3);

            // This code sorts the vertices by their Y component
            if (vertices[0].Y <= vertices[2].Y)
            {
                if (vertices[0].Y > vertices[1].Y)
                {
                    v1 = vertices[0];
                    v0 = vertices[1];
                    v2 = vertices[2];
                }
                else
                {
                    v0 = vertices[0];
                    if (vertices[1].Y <= vertices[2].Y)
                    {
                        v1 = vertices[1];
                        v2 = vertices[2];
                    }
                    else
                    {
                        v1 = vertices[2];
                        v2 = vertices[1];
                    }
                }
            }
            else if (vertices[1].Y < vertices[2].Y)
            {
                v0 = vertices[1];
                v1 = vertices[2];
                v2 = vertices[0];
            }
            else
            {
                v0 = vertices[2];

                if (vertices[0].Y <= vertices[1].Y)
                {
                    v1 = vertices[0];
                    v2 = vertices[1];
                }
                else
                {
                    v1 = vertices[1];
                    v2 = vertices[0];
                }
            }
        }
    }
}