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

            var v12 = v0.X + (v1.Y - v0.Y) * (v2.X - v0.X) / (v2.Y - v0.Y);
            var v56 = v12;
            if ( v12 < v1.X )
            {
              v56 = v1.X;
              v1.X = v12;
            }

            if ( v0.Y <= v1.Y )
            {
              var v15 = v0.X;
              var v16 = v0.X;
              var v17 = v1.Y;
              var v18 = v0.Y;
              var v42 = (v1.X - v0.X) / (v17 - v18);
              var v44 = (v56 - v0.X) / (v1.Y - v18);
              if ( v18 <= v17 )
              {
                var v19 = bufferWidth * v18;
                var v39 = v17 - v18 + 1;
                do
                {
                  if ( v15 <= v16 )
                  {
                      var leftX = (int)(v15 + v19);
                      var stripeWidth = (int)(v16 - v15 + 1);
                      buffer.Slice(leftX, stripeWidth).Fill(flags);
                  }
                  v15 += v42;
                  v19 += bufferWidth;
                  v16 += v44;
                  --v39;
                }
                while ( v39 > 0 );
              }
            }

            if ( v1.Y <= v2.Y )
            {
              var v27 = v1.X;
              var v28 = v56;
              var v43 = (v2.X - v1.X) / (v2.Y - v1.Y);
              var v41 = (v2.X - v56) / (v2.Y - v1.Y);
              if ( v1.Y <= v2.Y )
              {
                var v31 = bufferWidth * v1.Y;
                var v40 = v2.Y - v1.Y + 1;
                do
                {
                  var v32 = v27;
                  var v33 = v28;
                  if ( v32 <= v28 )
                  {
                      var leftX = (int)(v32 + v31);
                      var stripeWidth = (int)(v33 - v32 + 1);
                      buffer.Slice(leftX, stripeWidth).Fill(flags);
                  }
                  v27 = v27 + v43;
                  v31 += bufferWidth;
                  v28 = v28 + v41;
                  --v40;
                }
                while ( v40 > 0 );
              }
            }
        }

        private static void SortVerticesByY(ReadOnlySpan<Vector2> vertices, out Vector2 v0, out Vector2 v1, out Vector2 v2)
        {
            Debug.Assert(vertices.Length == 3);

            // This code sorts the vertices by their Y component
            if ( vertices[0].Y <= vertices[2].Y )
            {
                if ( vertices[0].Y > vertices[1].Y )
                {
                    v1 = vertices[0];
                    v0 = vertices[1];
                    v2 = vertices[2];
                }
                else
                {
                    v0 = vertices[0];
                    if ( vertices[1].Y <= vertices[2].Y )
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
            else if ( vertices[1].Y < vertices[2].Y )
            {
                v0 = vertices[1];
                v1 = vertices[2];
                v2 = vertices[0];
            }
            else
            {
                v0 = vertices[2];

                if ( vertices[0].Y <= vertices[1].Y )
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