using System;
using System.Drawing;
using OpenTemple.Core.Config;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Widgets;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Ui
{
    public class GameView : WidgetContainer, IGameViewport
    {
        private const float MinZoom = 0.5f;
        private const float MaxZoom = 2.0f;

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private readonly GameViewScrollingController _scrollingController;

        private readonly RenderingDevice _device;

        private readonly GameRenderer _gameRenderer;

        public WorldCamera Camera { get; } = new();

        [Obsolete]
        public GameRenderer GameRenderer => _gameRenderer;

        private event Action _onResize;

        private float _renderScale;

        private float _zoom = 1f;

        public float Zoom => _zoom;

        public Size Size => GetSize();

        event Action IGameViewport.OnResize
        {
            add => _onResize += value;
            remove => _onResize -= value;
        }

        private readonly IMainWindow _mainWindow;

        private bool _isUpscaleLinearFiltering;

        public GameView(IMainWindow mainWindow, RenderingDevice device, RenderingConfig config) : base(Globals.UiManager
            .CanvasSize)
        {
            _device = device;
            _gameRenderer = new GameRenderer(_device, this);

            _mainWindow = mainWindow;

            ReloadConfig(config);

            GameViews.Add(this);

            SetSizeToParent(true);
            OnSizeChanged();

            _scrollingController = new GameViewScrollingController(this, this);
        }

        private void ReloadConfig(RenderingConfig config)
        {
            _isUpscaleLinearFiltering = config.IsUpscaleLinearFiltering;
            _renderScale = config.RenderScale;
            _gameRenderer.MultiSampleSettings = new MultiSampleSettings(
                config.IsAntiAliasing,
                config.MSAASamples,
                config.MSAAQuality
            );
            OnSizeChanged();
        }

        public void RenderScene()
        {
            if (!Visible)
            {
                return;
            }

            ApplyAutomaticSizing();

            if (!GameViews.IsDrawingEnabled)
            {
                return;
            }

            using var _ = _device.CreatePerfGroup("Updating GameView {0}", Name);

            try
            {
                _gameRenderer.Render();
            }
            catch (Exception e)
            {
                ErrorReporting.ReportException(e);
            }
        }

        public override void Render()
        {
            if (!Visible)
            {
                return;
            }

            ApplyAutomaticSizing();

            var sceneTexture = _gameRenderer.SceneTexture;
            if (sceneTexture == null)
            {
                return;
            }

            var samplerType = SamplerType2d.CLAMP;
            if (!_isUpscaleLinearFiltering)
            {
                samplerType = SamplerType2d.POINT;
            }

            Tig.ShapeRenderer2d.DrawRectangle(
                GetContentArea(),
                sceneTexture,
                PackedLinearColorA.White,
                samplerType
            );
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            GameViews.Remove(this);
        }

        protected override void OnSizeChanged()
        {
            // We're trying to render at native resolutions by default, hence we apply
            // the UI scale to determine the render target size here.
            _gameRenderer.RenderSize = new Size(
                (int) (Width * _mainWindow.UiScale * _renderScale),
                (int) (Height * _mainWindow.UiScale * _renderScale)
            );
            Logger.Debug("Rendering @ {0}x{1} ({2}%), MSAA: {3}",
                _gameRenderer.RenderSize.Width,
                _gameRenderer.RenderSize.Height,
                (int) (_renderScale * 100),
                _gameRenderer.MultiSampleSettings
            );

            UpdateCamera();
        }

        public void TakeScreenshot(string path, Size size = default)
        {
            throw new NotImplementedException();
        }

        public MapObjectRenderer GetMapObjectRenderer()
        {
            return _gameRenderer.GetMapObjectRenderer();
        }

        public override bool HandleMouseMessage(MessageMouseArgs msg)
        {
            if ((msg.flags & MouseEventFlag.ScrollWheelChange) != 0)
            {
                _zoom = Math.Clamp(_zoom + Math.Sign(msg.wheelDelta) * 0.1f, MinZoom, MaxZoom);
                UpdateCamera();
                return true;
            }
            else if ((msg.flags & MouseEventFlag.MiddleClick) != 0)
            {
                var mousePos = new Point(msg.X, msg.Y);
                if (_scrollingController.MiddleMouseDown(GetRelativeMousePos(mousePos)))
                {
                    return true;
                }
            }
            else if ((msg.flags & MouseEventFlag.MiddleReleased) != 0)
            {
                if (_scrollingController.MiddleMouseUp())
                {
                    return true;
                }
            }
            else if ((msg.flags & MouseEventFlag.PosChange) != 0)
            {
                var mousePos = new Point(msg.X, msg.Y);
                if (_scrollingController.MouseMoved(GetRelativeMousePos(mousePos)))
                {
                    return true;
                }
            }

            return base.HandleMouseMessage(msg);
        }

        private void UpdateCamera()
        {
            var size = new Size(
                (int)(Width / _zoom),
                (int)(Height / _zoom)
            );

            if (size != Camera.ViewportSize)
            {
                var currentCenter = ((IGameViewport) this).CenteredOn;

                Camera.ViewportSize = size;

                // See also @ 0x10028db0
                var restoreCenter = currentCenter.ToInches3D();
                Camera.CenterOn(restoreCenter.X, restoreCenter.Y, restoreCenter.Z);

                _onResize?.Invoke();
            }
        }

        public override void OnUpdateTime(TimePoint timeMs)
        {
            base.OnUpdateTime(timeMs);
            _scrollingController.UpdateTime(timeMs, GetRelativeMousePos(Tig.Mouse.GetPos()));
        }

        private Point GetRelativeMousePos(Point mousePos)
        {
            var contentArea = GetContentArea();
            mousePos.X -= contentArea.X;
            mousePos.Y -= contentArea.Y;
            return mousePos;
        }
    }
}