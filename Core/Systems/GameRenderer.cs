using System;
using System.Drawing;
using SpicyTemple.Core.AAS;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.Anim;
using SpicyTemple.Core.Systems.FogOfWar;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.MapSector;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;
using SpicyTemple.Core.Ui;

namespace SpicyTemple.Core.Systems
{
    public struct SectorList
    {
        public SectorLoc Sector;
        public locXY CornerTile; // tile coords
        public Size Extent; // relative to the above tile
    }

    internal struct RenderWorldInfo
    {
        public Rectangle Viewport;
        public TileRect Tiles;
        public SectorList[] Sectors;
    }

    public class GameRenderer : IDisposable
    {
        private readonly IAnimatedModel _model;
        private TimePoint _lastUpdate = TimePoint.Now;

        private readonly AasRenderer _aasRenderer;

        private readonly RenderingDevice mRenderingDevice;
        private readonly MapObjectRenderer mMapObjectRenderer;
        private readonly ParticleSystemsRenderer _particleSysRenderer;
        private readonly GMeshRenderer mGmeshRenderer;
        private readonly FogOfWarRenderer mFogOfWarRenderer;
        private readonly SectorDebugRenderer _sectorDebugRenderer;
        private readonly SectorVisibilityRenderer _sectorVisibilityRenderer;

        public bool RenderSectorDebugInfo { get; set; }

        public bool RenderSectorVisibility { get; set; }

        public ParticleSystemsRenderer GetParticleSysRenderer() => _particleSysRenderer;

        public MapObjectRenderer GetMapObjectRenderer() => mMapObjectRenderer;

        public MapFogDebugRenderer MapFogDebugRenderer { get; }

        private readonly GameView _gameView;

        private int _drawEnableCount = 1;

        public GameRenderer(RenderingDevice renderingDevice, GameView gameView)
        {
            mRenderingDevice = renderingDevice;
            _gameView = gameView;
            _aasRenderer = GameSystems.AAS.Renderer;

            mMapObjectRenderer = new MapObjectRenderer(renderingDevice, Tig.MdfFactory, _aasRenderer);
            _sectorDebugRenderer = new SectorDebugRenderer();
            _sectorVisibilityRenderer = new SectorVisibilityRenderer();

            MapFogDebugRenderer = new MapFogDebugRenderer(GameSystems.MapFogging, renderingDevice);

            _particleSysRenderer = new ParticleSystemsRenderer(
                renderingDevice,
                Tig.ShapeRenderer2d,
                GameSystems.AAS.ModelFactory,
                _aasRenderer,
                GameSystems.ParticleSys
            );
        }

        [TempleDllLocation(0x100027E0)]
        public void EnableDrawing()
        {
            _drawEnableCount++;
        }

        [TempleDllLocation(0x100027C0)]
        public void DisableDrawing()
        {
            _drawEnableCount--;
        }

        [TempleDllLocation(0x100027D0)]
        public void DisableDrawingForce()
        {
            _drawEnableCount = 0;
        }

        public void Render()
        {
            using var perfGroup = mRenderingDevice.CreatePerfGroup("Game Renderer");

            if (_drawEnableCount <= 0)
            {
                return;
            }

            var viewportSize = new Rectangle();
            viewportSize.Y = -256;
            viewportSize.Width = _gameView.Width + 512;
            viewportSize.X = -256;
            viewportSize.Height = _gameView.Height + 512;

            if (GameSystems.Location.GetVisibleTileRect(viewportSize, out var tiles))
            {
                RenderWorld(ref tiles);
            }
        }

        private void RenderWorld(ref TileRect tileRect)
        {
            if (mRenderingDevice.BeginFrame())
            {
                GameSystems.Terrain.Render();

                GameSystems.MapFogging.PerformFogChecks();

                GameSystems.Clipping.Render();

                mMapObjectRenderer.RenderMapObjects(
                    tileRect.x1, tileRect.x2,
                    tileRect.y1, tileRect.y2);

                // TODO mGmeshRenderer.Render();

                GameSystems.Vfx.Render();

                _particleSysRenderer.Render();

                GameSystems.MapFogging.Renderer.Render();

                mMapObjectRenderer.RenderOccludedMapObjects(
                    tileRect.x1, tileRect.x2,
                    tileRect.y1, tileRect.y2);

                using (var uiPerfGroup = mRenderingDevice.CreatePerfGroup("World UI"))
                {
                    if (RenderSectorDebugInfo)
                    {
                        _sectorDebugRenderer.Render(tileRect);
                    }

                    if (RenderSectorVisibility)
                    {
                        _sectorVisibilityRenderer.Render(tileRect);
                    }

                    MapFogDebugRenderer.Render();

                    GameUiBridge.RenderTurnBasedUI();
                    GameSystems.TextBubble.Render();
                    GameSystems.TextFloater.Render();

                    AnimGoalsDebugRenderer.RenderAllAnimGoals(
                        tileRect.x1, tileRect.x2,
                        tileRect.y1, tileRect.y2);
                }

                mRenderingDevice.Present();
            }
        }

        public void Dispose()
        {
            _sectorDebugRenderer.Dispose();
            _particleSysRenderer.Dispose();
        }
    }
}