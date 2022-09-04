using System;
using System.Drawing;
using OpenTemple.Core.Config;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Widgets;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Ui;

/// <summary>
/// Shows a view of the ingame world in the user interface. Handles rendering of the world and
/// interacting with object in the world.
/// </summary>
public class GameView : WidgetContainer, IGameViewport
{
    private const float MinZoom = 0.5f;
    private const float MaxZoom = 2.0f;

    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private readonly GameViewScrollingController _scrollingController;

    private readonly RenderingDevice _device;

    private readonly GameRenderer _gameRenderer;

    public bool IsMouseScrollingEnabled
    {
        get => _scrollingController.IsMouseScrolling;
        set => _scrollingController.IsMouseScrolling = value;
    }

    public WorldCamera Camera { get; } = new();

    [Obsolete]
    public GameRenderer GameRenderer => _gameRenderer;

    private event Action _onResize;

    private float _renderScale;

    private float _zoom = 1f;

    public float Zoom => _zoom;

    Size IGameViewport.Size => Size;

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

        Globals.ConfigManager.OnConfigChanged += OnConfigChange;

        // The order of these does matter
        UiSystems.RadialMenu.AddEventListeners(this);
        UiSystems.TurnBased.AddEventListeners(this);
        UiSystems.InGameSelect.AddEventListeners(this);
        UiSystems.InGame.AddEventListeners(this);

        Globals.UiManager.AddWindow(this);
        Globals.UiManager.SendToBack(this);
    }

    public override void HandleHotkeyAction(HotkeyActionMessage msg)
    {
        base.HandleHotkeyAction(msg);

        if (!msg.IsHandled)
        {
            // Delegate handling hotkeys to the InGame ui
            if (UiSystems.InGame.HandleHotkeyAction(this, msg.Hotkey))
            {
                msg.SetHandled();
            }
        }
    }

    private void OnConfigChange()
    {
        ReloadConfig(Globals.Config.Rendering);
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
        if (!Visible || !IsInTree)
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
            if (!ErrorReporting.ReportException(e))
            {
                throw;
            }
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

        UiSystems.TurnBased.Render(this);
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

    protected override void HandleMouseMove(MouseEvent e)
    {
        var mousePos = new PointF(e.X, e.Y);
        _scrollingController.MouseMoved(GetRelativeMousePos(mousePos));
    }
    
    public override bool HandleMessage(Message msg)
    {
        UiSystems.InGame.HandleMessage(this, msg);

        return base.HandleMessage(msg);
    }

    protected override void HandleMouseDown(MouseEvent e)
    {
        if (e.Button == MouseButton.MIDDLE)
        {
            var mousePos = new PointF(e.X, e.Y);
            if (_scrollingController.MiddleMouseDown(GetRelativeMousePos(mousePos)))
            {
                e.StopImmediatePropagation();
            }
        }
    }

    protected override void HandleMouseUp(MouseEvent e)
    {
        if (e.Button == MouseButton.MIDDLE)
        {
            _scrollingController.MiddleMouseUp();
        }
    }

    protected override void HandleMouseWheel(WheelEvent e)
    {
        _zoom = Math.Clamp(_zoom + Math.Sign(e.DeltaY) * 0.1f, MinZoom, MaxZoom);
        UpdateCamera();
    }

    private void UpdateCamera()
    {
        var size = new Size(
            (int)(Width / _zoom),
            (int)(Height / _zoom)
        );

        if (size != Camera.ViewportSize)
        {
            // Using IGameViewport.CenteredOn will be wrong here if the zoom just changed
            var currentCenter = Camera.ScreenToTile(Camera.GetViewportWidth() / 2, Camera.GetViewportHeight() / 2);

            Camera.ViewportSize = size;

            // See also @ 0x10028db0
            var restoreCenter = currentCenter.ToInches3D();
            Camera.CenterOn(restoreCenter.X, restoreCenter.Y, restoreCenter.Z);

            // Ensure the primary translation is updated, otherwise the next scroll event will undo what we just did
            if (GameViews.Primary == this)
            {
                var dt = Camera.Get2dTranslation();
                GameSystems.Location.LocationTranslationX = (int) dt.X;
                GameSystems.Location.LocationTranslationY = (int) dt.Y;
                // This clamps the translation to the scroll limit
                GameSystems.Scroll.ScrollBy(this, 0, 0);
            }

            _onResize?.Invoke();
        }
    }

    public override void OnUpdateTime(TimePoint now)
    {
        base.OnUpdateTime(now);
        _scrollingController.UpdateTime(now);
    }

    private PointF GetRelativeMousePos(PointF mousePos)
    {
        var contentArea = GetContentArea();
        mousePos.X -= contentArea.X;
        mousePos.Y -= contentArea.Y;
        return mousePos;
    }
}