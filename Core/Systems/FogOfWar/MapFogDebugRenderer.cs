using System;
using System.Numerics;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems.FogOfWar
{
    public class MapFogDebugRenderer : IDisposable
    {
        private readonly MapFoggingSystem _system;

        private ResourceRef<DynamicTexture> _texture;

        public int RenderFor { get; set; } = -1;

        public MapFogDebugRenderer(MapFoggingSystem system, RenderingDevice device)
        {
            _system = system;
        }

        /// <summary>
        /// Renders the Line Of Sight buffer for a given party member.
        /// </summary>
        public void RenderLineOfSight(int partyIndex)
        {
            var buffer = _system.GetLineOfSightBuffer(partyIndex, out var size, out var originTile);

            if (!_texture.IsValid || _texture.Resource.GetSize() != size)
            {
                _texture.Dispose();
                _texture = Tig.RenderingDevice.CreateDynamicTexture(BufferFormat.A8, size.Width, size.Height);
            }

            _texture.Resource.UpdateRaw(buffer, size.Width);

            var origin = new Vector4(originTile.ToInches3D(), 1);

            var widthVec = new Vector4(size.Width * locXY.INCH_PER_SUBTILE, 0, 0, 0);
            var heightVec = new Vector4(0, 0, size.Height * locXY.INCH_PER_SUBTILE, 0);

            Span<ShapeVertex3d> corners = stackalloc ShapeVertex3d[4]
            {
                new ShapeVertex3d
                {
                    pos = origin,
                    uv = new Vector2(0, 0)
                },
                new ShapeVertex3d
                {
                    pos = origin + widthVec,
                    uv = new Vector2(1, 0)
                },
                new ShapeVertex3d
                {
                    pos = origin + widthVec + heightVec,
                    uv = new Vector2(1, 1)
                },
                new ShapeVertex3d
                {
                    pos = origin + heightVec,
                    uv = new Vector2(0, 1)
                }
            };

            Tig.ShapeRenderer3d.DrawQuad(corners, new PackedLinearColorA(255, 255, 255, 127), _texture.Resource);
        }

        public void Render()
        {
            if (RenderFor < 0 || RenderFor >= GameSystems.Party.PartySize)
            {
                return;
            }

            RenderLineOfSight(RenderFor);
        }

        public void Dispose()
        {
            _texture.Dispose();
        }
    }
}