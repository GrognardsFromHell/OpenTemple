using System;
using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.Location;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;

namespace OpenTemple.Core.Systems.FogOfWar
{
    public class MapFogDebugRenderer : IDisposable
    {
        private readonly MapFoggingSystem _system;

        private readonly RenderingDevice _device;

        private ResourceRef<DynamicTexture> _texture;

        private PackedLinearColorA[] _colorBuffer;

        public int RenderFor { get; set; } = -1;

        public MapFogDebugRenderer(MapFoggingSystem system, RenderingDevice device)
        {
            _system = system;
            _device = device;
        }

        /// <summary>
        /// Renders the Line Of Sight buffer for a given party member.
        /// </summary>
        public void RenderLineOfSight(IGameViewport viewport, int partyIndex)
        {
            var buffer = _system.GetLineOfSightBuffer(partyIndex, out var size, out var originTile);

            if (!_texture.IsValid || _texture.Resource.GetSize() != size)
            {
                _colorBuffer = new PackedLinearColorA[size.Width * size.Height];

                _texture.Dispose();
                _texture = _device.CreateDynamicTexture(BufferFormat.A8R8G8B8, size.Width, size.Height);
            }

            // Update color buffer
            for (int i = 0; i < buffer.Length; i++)
            {
                _colorBuffer[i] = GetColorFromLosFlags(buffer[i]);
            }

            var rawColorBuffer = MemoryMarshal.Cast<PackedLinearColorA, byte>(_colorBuffer);
            _texture.Resource.UpdateRaw(rawColorBuffer, size.Width * 4);

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

            Tig.ShapeRenderer3d.DrawQuad(viewport, corners, new PackedLinearColorA(255, 255, 255, 127), _texture.Resource);
        }

        private PackedLinearColorA GetColorFromLosFlags(byte flags)
        {
            var color = new PackedLinearColorA(0, 0, 0, 0);
            /*if ((flags & LineOfSightBuffer.BLOCKING) != 0)
            {
                color.R = 255;
                color.A = 255;
            }

            if ((flags & LineOfSightBuffer.UNK) != 0)
            {
                color.G = 255;
                color.A = 255;
            }

            if ((flags & LineOfSightBuffer.UNK1) != 0)
            {
                color.B = 255;
                color.A = 255;
            }*/

            if ((flags & LineOfSightBuffer.ARCHWAY) != 0)
            {
                color.G = 255;
                color.A = 255;
            }

            return color;
        }

        public void Render(IGameViewport viewport)
        {
            if (RenderFor >= 0 && RenderFor < GameSystems.Party.PartySize)
            {
                RenderLineOfSight(viewport, RenderFor);
            }
        }

        public void Dispose()
        {
            _texture.Dispose();
        }
    }
}