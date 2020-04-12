using System;
using System.Drawing;
using JetBrains.Annotations;
using OpenTemple.Core.AAS;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.Anim;
using OpenTemple.Core.Systems.FogOfWar;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.MapSector;
using OpenTemple.Core.Systems.Pathfinding;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui;

namespace OpenTemple.Core.Systems
{
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

        public bool DebugPathFinding { get; set; }

        public ParticleSystemsRenderer GetParticleSysRenderer() => _particleSysRenderer;

        public MapObjectRenderer GetMapObjectRenderer() => mMapObjectRenderer;

        public MapFogDebugRenderer MapFogDebugRenderer { get; }

        private int _drawEnableCount = 1;

        public Size VisibleSize { get; set; }

        public GameRenderer(RenderingDevice renderingDevice, GameView gameView)
        {
            mRenderingDevice = renderingDevice;
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
            viewportSize.Width = VisibleSize.Width + 512;
            viewportSize.X = -256;
            viewportSize.Height = VisibleSize.Height + 512;

            if (GameSystems.Location.GetVisibleTileRect(viewportSize, out var tiles))
            {
                RenderWorld(ref tiles);
            }
        }

        private void RenderWorld(ref TileRect tileRect)
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

                if (DebugPathFinding)
                {
                    RenderDebugPathfinding();
                }

                GameUiBridge.RenderTurnBasedUI();
                GameSystems.TextBubble.Render();
                GameSystems.TextFloater.Render();

                AnimGoalsDebugRenderer.RenderAllAnimGoals(
                    tileRect.x1, tileRect.x2,
                    tileRect.y1, tileRect.y2);
            }
        }

        private void RenderDebugPathfinding()
        {
            var leader = GameSystems.Party.GetLeader();
            var mousePos = Tig.Mouse.GetPos();
            var worldPos = Tig.RenderingDevice.GetCamera().ScreenToTile(mousePos.X, mousePos.Y);

            var pq = new PathQuery();
            pq.from = leader.GetLocationFull();
            pq.critter = leader;
            pq.flags = PathQueryFlags.PQF_HAS_CRITTER;
            pq.to = worldPos;
            var reach = leader.GetReach();
            pq.tolRadius = reach * 12.0f - 8.0f;
            pq.distanceToTargetMin = reach;

            var pqr = new PathQueryResult();
            if (!GameSystems.PathX.FindPath(pq, out pqr))
            {
                var red = new PackedLinearColorA(255, 0, 0, 255);
                var center = worldPos.ToInches3D();
                Tig.ShapeRenderer3d.DrawFilledCircle(center, locXY.INCH_PER_FEET, red, red);
                Tig.ShapeRenderer3d.DrawFilledCircle(center, locXY.INCH_PER_FEET, red, red, true);
                return;
            }

            GameSystems.PathXRender.RenderPathPreview(pqr, true);
        }

        public void Dispose()
        {
            _sectorDebugRenderer.Dispose();
            _particleSysRenderer.Dispose();
        }
    }
}