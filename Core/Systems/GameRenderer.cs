using System;
using System.Drawing;
using JetBrains.Annotations;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.Anim;
using OpenTemple.Core.Systems.FogOfWar;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.MapSector;
using OpenTemple.Core.Systems.Pathfinding;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;

namespace OpenTemple.Core.Systems
{
    public class GameRenderer : IDisposable
    {
        private readonly RenderingDevice _device;

        private readonly MapObjectRenderer _mapObjectRenderer;
        private readonly ParticleSystemsRenderer _particleSysRenderer;
        private readonly GMeshRenderer _gMeshRenderer;
        private readonly FogOfWarRenderer _fogOfWarRenderer;
        private readonly SectorDebugRenderer _sectorDebugRenderer;
        private readonly SectorVisibilityRenderer _sectorVisibilityRenderer;

        private ResourceRef<RenderTargetTexture> _sceneColor;

        private ResourceRef<RenderTargetDepthStencil> _sceneDepth;

        public bool RenderSectorDebugInfo { get; set; }

        public bool RenderSectorVisibility { get; set; }

        public bool DebugPathFinding { get; set; }

        public MapObjectRenderer GetMapObjectRenderer() => _mapObjectRenderer;

        public MapFogDebugRenderer MapFogDebugRenderer { get; }

        private readonly IGameViewport _viewport;

        // A texture containing the last rendered scene
        [CanBeNull] public ITexture SceneTexture => _sceneColor.Resource;

        // The pixel size of the render texture to create
        private Size _renderSize;

        private MultiSampleSettings _multiSampleSettings;

        // Indicates that resources need to be recreated
        private bool _resourcesDirty = true;

        public GameRenderer(RenderingDevice renderingDevice, IGameViewport viewport)
        {
            _device = renderingDevice;
            _viewport = viewport;

            _mapObjectRenderer = new MapObjectRenderer(renderingDevice, Tig.MdfFactory, GameSystems.AAS.Renderer);
            _sectorDebugRenderer = new SectorDebugRenderer();
            _sectorVisibilityRenderer = new SectorVisibilityRenderer();

            MapFogDebugRenderer = new MapFogDebugRenderer(GameSystems.MapFogging, renderingDevice);

            _particleSysRenderer = new ParticleSystemsRenderer(
                renderingDevice,
                Tig.ShapeRenderer2d,
                GameSystems.AAS.ModelFactory,
                GameSystems.AAS.Renderer,
                GameSystems.ParticleSys
            );
        }

        public Size RenderSize
        {
            get => _renderSize;
            set
            {
                if (value != _renderSize)
                {
                    _renderSize = value;
                    _resourcesDirty = true;
                }
            }
        }

        public MultiSampleSettings MultiSampleSettings
        {
            get => _multiSampleSettings;
            set
            {
                if (value != _multiSampleSettings)
                {
                    _multiSampleSettings = value;
                    _resourcesDirty = true;
                }
            }
        }

        public void Render()
        {
            if (_resourcesDirty && !CreateGpuResources())
            {
                return;
            }

            _device.PushRenderTarget(_sceneColor, _sceneDepth);

            // Clear the backbuffer
            _device.BeginDraw();

            var viewportSize = new Rectangle();
            viewportSize.Y = -256;
            viewportSize.Width = _viewport.Camera.ViewportSize.Width + 512;
            viewportSize.X = -256;
            viewportSize.Height = _viewport.Camera.ViewportSize.Height + 512;

            if (GameSystems.Location.GetVisibleTileRect(_viewport, viewportSize, out var tiles))
            {
                RenderWorld(_viewport, ref tiles);
            }

            _device.EndDraw();

            // Reset the render target
            _device.PopRenderTarget();
        }

        private void RenderWorld(IGameViewport viewport, ref TileRect tileRect)
        {
            if (!GameSystems.Map.IsMapOpen())
            {
                return;
            }

            // We need to set the canvas size because renderers like the terrain rendering system will make use
            // of a UI projection that has to match the scaled viewport in size. The viewport is in a 2D-coordinate
            // system transformed by the "x,y translation" used by the vanilla engine to apply scrolling, and we're
            // implementing "zoom" by essentially making the size of the viewport in this 2d-coordinate system
            // smaller.
            var previousUiSize = _device.UiCanvasSize;
            _device.UiCanvasSize = _viewport.Camera.ViewportSize;

            GameSystems.Terrain.Render(_viewport.Camera);

            _device.UiCanvasSize = previousUiSize;

            GameSystems.MapFogging.PerformFogChecks();

            GameSystems.Clipping.Render(viewport.Camera);

            _mapObjectRenderer.RenderMapObjects(viewport, tileRect.x1, tileRect.x2,
                tileRect.y1, tileRect.y2);

            // TODO mGmeshRenderer.Render();

            GameSystems.Vfx.Render(viewport);

            _particleSysRenderer.Render(viewport);

            GameSystems.MapFogging.Renderer.Render(viewport);

            _mapObjectRenderer.RenderOccludedMapObjects(viewport, tileRect.x1, tileRect.x2,
                tileRect.y1, tileRect.y2);

            using (var uiPerfGroup = _device.CreatePerfGroup("World UI"))
            {
                if (RenderSectorDebugInfo)
                {
                    _sectorDebugRenderer.Render(viewport, tileRect);
                }

                if (RenderSectorVisibility)
                {
                    _sectorVisibilityRenderer.Render(viewport, tileRect);
                }

                MapFogDebugRenderer.Render(viewport);

                if (DebugPathFinding)
                {
                    RenderDebugPathfinding(viewport);
                }

                GameUiBridge.RenderTurnBasedUI(viewport);
                GameSystems.TextBubble.Render(viewport);
                GameSystems.TextFloater.Render(viewport);
            }

            UiSystems.RadialMenu
                .Render(); // TODO: Radial Menu should not become a "game" render aspect, but rather a UI render aspect

            AnimGoalsDebugRenderer.RenderAllAnimGoals(
                viewport,
                tileRect.x1, tileRect.x2,
                tileRect.y1, tileRect.y2);
        }

        private void RenderDebugPathfinding(IGameViewport viewport)
        {
            var leader = GameSystems.Party.GetLeader();
            var mousePos = Tig.Mouse.GetPos();
            var worldPos = viewport.ScreenToTile(mousePos.X, mousePos.Y);

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
                Tig.ShapeRenderer3d.DrawFilledCircle(viewport, center, locXY.INCH_PER_FEET, red, red);
                Tig.ShapeRenderer3d.DrawFilledCircle(viewport, center, locXY.INCH_PER_FEET, red, red, true);
                return;
            }

            GameSystems.PathXRender.RenderPathPreview(viewport, pqr, true);
        }

        public void TakeScreenshot(string filename, int width, int height, int quality = 90)
        {
            _device.TakeScaledScreenshot(_sceneColor.Resource, filename,
                width, height, quality);
        }

        private bool CreateGpuResources()
        {
            _sceneColor.Dispose();
            _sceneDepth.Dispose();

            if (_renderSize.IsEmpty)
            {
                return false;
            }

            // Create the buffers for the scaled game view
            _sceneColor = _device.CreateRenderTargetTexture(
                BufferFormat.A8R8G8B8, _renderSize.Width, _renderSize.Height, _multiSampleSettings,
                "WorldView"
            );
            _sceneDepth = _device.CreateRenderTargetDepthStencil(
                _renderSize.Width, _renderSize.Height, _multiSampleSettings
            );
            _resourcesDirty = false;
            return true;
        }

        public void Dispose()
        {
            _sceneColor.Dispose();
            _sceneDepth.Dispose();

            _sectorDebugRenderer.Dispose();
            _particleSysRenderer.Dispose();
        }
    }
}