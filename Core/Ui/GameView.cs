using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using OpenTemple.Core.Config;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Cursors;
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

    private event Action? _onResize;

    private float _renderScale;

    private float _zoom = 1f;

    public float Zoom => _zoom;

    Size IGameViewport.Size => Size;

    public bool IsInteractive => true;

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
        HitTesting = HitTestingMode.Area; // The entire area represents the in-game UI

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

        var samplerType = SamplerType2d.Clamp;
        if (!_isUpscaleLinearFiltering)
        {
            samplerType = SamplerType2d.Point;
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

    protected override void HandleMouseDown(MouseEvent e)
    {
        if (e.Button == MouseButton.Middle)
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
        if (e.Button == MouseButton.Middle)
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
            (int) (Width / _zoom),
            (int) (Height / _zoom)
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

    private static readonly Dictionary<ActionCursor, string> CursorPaths = new()
    {
        {ActionCursor.AttackOfOpportunity, CursorIds.AttackOfOpportunity},
        {ActionCursor.AttackOfOpportunityGrey, CursorIds.AttackOfOpportunityGrey},
        {ActionCursor.Sword, CursorIds.Sword},
        {ActionCursor.Arrow, CursorIds.Arrow},
        {ActionCursor.FeetGreen, CursorIds.MoveGreen},
        {ActionCursor.FeetYellow, CursorIds.MoveYellow},
        {ActionCursor.SlidePortraits, CursorIds.SlideHorizontal},
        {ActionCursor.Locked, CursorIds.Locked},
        {ActionCursor.HaveKey, CursorIds.UseKey},
        {ActionCursor.UseSkill, CursorIds.UseSkill},
        {ActionCursor.UsePotion, CursorIds.UsePotion},
        {ActionCursor.UseSpell, CursorIds.UseSpell},
        {ActionCursor.UseTeleportIcon, CursorIds.OpenHand},
        {ActionCursor.HotKeySelection, CursorIds.AssignHotkey},
        {ActionCursor.Talk, CursorIds.Talk},
        {ActionCursor.IdentifyCursor, CursorIds.Identify},
        {ActionCursor.IdentifyCursor2, CursorIds.Identify},
        {ActionCursor.ArrowInvalid, CursorIds.ArrowInvalid},
        {ActionCursor.SwordInvalid, CursorIds.SwordInvalid},
        {ActionCursor.ArrowInvalid2, CursorIds.ArrowInvalid},
        {ActionCursor.FeetRed, CursorIds.MoveRed},
        {ActionCursor.FeetRed2, CursorIds.MoveRed},
        {ActionCursor.InvalidSelection, CursorIds.InvalidSelection},
        {ActionCursor.Locked2, CursorIds.Locked},
        {ActionCursor.HaveKey2, CursorIds.UseKey},
        {ActionCursor.UseSkillInvalid, CursorIds.UseSkillInvalid},
        {ActionCursor.UsePotionInvalid, CursorIds.UsePotionInvalid},
        {ActionCursor.UseSpellInvalid, CursorIds.UseSpellInvalid},
        {ActionCursor.PlaceFlag, CursorIds.PlaceFlag},
        {ActionCursor.HotKeySelectionInvalid, CursorIds.AssignHotkeyInvalid},
        {ActionCursor.InvalidSelection2, CursorIds.InvalidSelection},
        {ActionCursor.InvalidSelection3, CursorIds.InvalidSelection},
        {ActionCursor.InvalidSelection4, CursorIds.InvalidSelection},
    };

    protected override void HandleGetCursor(GetCursorEvent e)
    {
        var actionCursor = GameSystems.D20.Actions.CurrentCursor;
        if (actionCursor != ActionCursor.Undefined)
        {
            if (CursorPaths.TryGetValue(actionCursor, out var cursorId))
            {
                e.Cursor = cursorId;
            }
            else
            {
                Logger.Error("Unknown D20 action cursor: {0}", actionCursor);
            }
        }
    }

    public override void AttachToTree(UiManager? manager)
    {
        // Uninstall / Install fallback key handlers to capture in-game keyboard shortcuts
        var oldRoot = UiManager?.Root;
        if (oldRoot != null)
        {
            oldRoot.OnKeyDown -= HandleGlobalKeyDown;
            oldRoot.OnKeyUp -= HandleGlobalKeyUp;
        }

        var newRoot = manager?.Root;
        if (newRoot != null)
        {
            newRoot.OnKeyDown += HandleGlobalKeyDown;
            newRoot.OnKeyUp += HandleGlobalKeyUp;
        }

        base.AttachToTree(manager);
    }

    private void HandleGlobalKeyDown(KeyboardEvent e) => DispatchKeyDown(e, true);

    private void HandleGlobalKeyUp(KeyboardEvent e) => DispatchKeyUp(e, true);
}