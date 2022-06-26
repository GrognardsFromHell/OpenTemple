using System;
using System.IO;
using ImGuiNET;
using OpenTemple.Core.Config;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Anim;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.Raycast;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.DebugUi;

public class DebugUiSystem : IDebugUi, IDisposable
{
    private readonly ImGuiRenderer _renderer;

    private readonly RenderingDevice _device;

    private bool _renderDebugOverlay;

    private bool _renderObjectTree;

    private bool _renderRaycastStats;

    private ImFontPtr _smallFont;

    private ImFontPtr _normalFont;

    private ImFontPtr _boldFont;

    public DebugUiSystem(IFileSystem fs, DebugConfig config, IMainWindow mainWindow, RenderingDevice device)
    {
        _device = device;

        // This is used for IME only.
        var hwnd = IntPtr.Zero;
        if (mainWindow is MainWindow realMainWindow)
        {
            hwnd = realMainWindow.NativeHandle;
        }

        var context = device.Device.ImmediateContext;

        var guiContext = ImGui.CreateContext();
        ImGui.SetCurrentContext(guiContext);
        var style = ImGui.GetStyle();
        style.ScaleAllSizes(mainWindow.UiScale);

        ImGui.GetIO().Fonts.AddFontDefault();
        AddFonts(fs, config, mainWindow.UiScale);

        _renderer = new ImGuiRenderer();
        if (!_renderer.ImGui_ImplDX11_Init(hwnd, device.Device, context))
        {
            throw new Exception("Unable to initialize IMGui!");
        }

        mainWindow.SetWindowMsgFilter(HandleMessage);
    }

    public void PushSmallFont()
    {
        ImGui.PushFont(_smallFont);
    }

    public void PushBoldFont()
    {
        ImGui.PushFont(_boldFont);
    }

    private void AddFonts(IFileSystem fs, DebugConfig config, float uiScale)
    {
        _smallFont = LoadFont(fs, config.DebugUiFontRegular, 10 * uiScale);
        _normalFont = LoadFont(fs, config.DebugUiFontRegular, 12 * uiScale);
        _boldFont = LoadFont(fs, config.DebugUiFontBold, 12 * uiScale);
    }

    private unsafe ImFontPtr LoadFont(IFileSystem fs, string path, float pixelSize)
    {
        using var memoryOwner = fs.ReadFile(path);
        var memory = memoryOwner.Memory.Span;

        // ImGui takes ownership of the pointer and will free it using its own allocator
        // We need to pass it memory that comes from that allocator, otherwise we risk crashes,
        // even if we'd use C#'s allocator to make a copy of the font.
        var nativeMem = ImGuiNative.igMemAlloc((uint) memory.Length);
        var nativeSpan = new Span<byte>(nativeMem, memory.Length);
        memory.CopyTo(nativeSpan);
        return ImGui.GetIO().Fonts.AddFontFromMemoryTTF(new IntPtr(nativeMem), memory.Length, pixelSize);
    }

    public void Dispose()
    {
        _renderer.ImGui_ImplDX11_Shutdown();
    }

    public void NewFrame()
    {
        var size = _device.GetBackBufferSize();
        _renderer.ImGui_ImplDX11_NewFrame(size.Width, size.Height);
        ImGui.PushFont(_normalFont);
    }

    public void Render()
    {
        try
        {
            if (_renderDebugOverlay && GameViews.Primary != null)
            {
                DebugOverlay.Render(GameViews.Primary);
            }

            if (_renderObjectTree)
            {
                new DebugObjectGraph().Render();
            }

            if (_renderRaycastStats)
            {
                RaycastStats.Render();
            }

            RenderMainMenuBar(out var height);

            // Disable mouse scrolling when the mouse is on the debug UI
            if (GameViews.Primary is GameView gameView)
            {
                gameView.IsMouseScrollingEnabled = height <= 0;
            }

            ObjectEditors.Render();

            ActionsDebugUi.Render();

            Tig.Console.Render(height);
        }
        catch (Exception e)
        {
            if (!ErrorReporting.ReportException(e))
            {
                throw;
            }
        }

        ErrorReportingUi.Render();

        ImGui.Render();
        var drawData = ImGui.GetDrawData();
        _renderer.ImGui_ImplDX11_RenderDrawLists(drawData);
    }

    // Used to keep the main menu visible even when the mouse is out of range, used if the mouse is on a submenu
    private bool _forceMainMenu;

    private void RenderMainMenuBar(out int height)
    {
        height = 0;

        // Only render the main menu bar when the mouse is in the vicinity
        if (ImGui.GetIO().MousePos.Y > 30 && !_forceMainMenu && !Tig.Console.IsVisible)
        {
            return;
        }

        _forceMainMenu = false;

        if (ImGui.BeginMainMenuBar())
        {
            height = (int) ImGui.GetWindowHeight();

            _forceMainMenu = ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows);

            var anyMenuOpen = false;

            if (ImGui.MenuItem("Console"))
            {
                Tig.Console.IsVisible = !Tig.Console.IsVisible;
            }

            if (ImGui.BeginMenu("View"))
            {
                anyMenuOpen = true;

                var enabled = Globals.UiManager.Debug.DebugMenuVisible;
                if (ImGui.MenuItem("UI Debug Menu", null, ref enabled))
                {
                    Globals.UiManager.Debug.DebugMenuVisible = enabled;
                }

                ImGui.MenuItem("Debug Overlay", null, ref _renderDebugOverlay);

                ActionsDebugUi.RenderMenuOptions();

                ImGui.MenuItem("Game Object Tree", null, ref _renderObjectTree);
                ImGui.MenuItem("Raycast Stats", null, ref _renderRaycastStats);

                var showGoalsChecked = AnimGoalsDebugRenderer.Enabled;
                if (ImGui.MenuItem("Render Anim Goals", null, ref showGoalsChecked))
                {
                    AnimGoalsDebugRenderer.Enabled = showGoalsChecked;
                }

                var showNamesChecked = AnimGoalsDebugRenderer.ShowObjectNames;
                if (ImGui.MenuItem("Render Object Names", null, ref showNamesChecked))
                {
                    AnimGoalsDebugRenderer.ShowObjectNames = showNamesChecked;
                }

                var verbosePartyLogging = GameSystems.Anim.VerbosePartyLogging;
                if (ImGui.MenuItem("Verbose Anim Goal Logging (Party)", null, ref verbosePartyLogging))
                {
                    GameSystems.Anim.VerbosePartyLogging = verbosePartyLogging;
                }

                var gameRenderer = UiSystems.GameView.GameRenderer;

                var renderSectorDebug = gameRenderer.RenderSectorDebugInfo;
                if (ImGui.MenuItem("Sector Blocking Debug", null, ref renderSectorDebug))
                {
                    gameRenderer.RenderSectorDebugInfo = renderSectorDebug;
                }

                var renderSectorVisibility = gameRenderer.RenderSectorVisibility;
                if (ImGui.MenuItem("Sector Visibility", null, ref renderSectorVisibility))
                {
                    gameRenderer.RenderSectorVisibility = renderSectorVisibility;
                }

                var pathFindingDebug = gameRenderer.DebugPathFinding;
                if (ImGui.MenuItem("Debug Pathfinding", null, ref pathFindingDebug))
                {
                    gameRenderer.DebugPathFinding = pathFindingDebug;
                }

                if (ImGui.BeginMenu("Line of Sight"))
                {
                    var fogDebugRenderer = gameRenderer.MapFogDebugRenderer;
                    var index = 0;
                    foreach (var partyMember in GameSystems.Party.PartyMembers)
                    {
                        var displayName = GameSystems.MapObject.GetDisplayName(partyMember);
                        var selected = fogDebugRenderer.RenderFor == index;
                        if (ImGui.MenuItem(displayName, null, ref selected))
                        {
                            fogDebugRenderer.RenderFor = selected ? index : -1;
                        }

                        index++;
                    }

                    ImGui.EndMenu();
                }

                var particleSystems = Globals.Config.Debug.ParticleSystems;
                if (ImGui.MenuItem("Particle Systems", null, ref particleSystems))
                {
                    Globals.Config.Debug.ParticleSystems = particleSystems;
                }

                var clipping = GameSystems.Clipping.Debug;
                if (ImGui.MenuItem("Clipping Meshes", null, ref clipping))
                {
                    GameSystems.Clipping.Debug = clipping;
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Screenshot"))
            {
                anyMenuOpen = true;

                if (GameViews.Primary != null)
                {
                    if (ImGui.MenuItem("Game View"))
                    {
                        GameViews.Primary.TakeScreenshot("gameview.jpg");
                    }
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Scripts"))
            {
                anyMenuOpen = true;

                RenderScriptsFolder(Tig.Console.AvailableScripts);

                ImGui.EndMenu();
            }

            if (anyMenuOpen)
            {
                _forceMainMenu = true;
            }

            RenderCheatsMenu();

            if (GameViews.Primary != null)
            {
                var screenCenter = GameViews.Primary.CenteredOn;
                ImGui.Text($"X: {screenCenter.location.locx} Y: {screenCenter.location.locy}");
            }

            ImGui.End();
        }
    }

    private void RenderScriptsFolder(ScriptFolder scripts, string pathPrefix = "")
    {
        ImGui.PushID(scripts.Name);
        foreach (var subFolder in scripts.SubFolders)
        {
            if (ImGui.BeginMenu(subFolder.Name))
            {
                RenderScriptsFolder(subFolder, pathPrefix + subFolder.Name + "/");
                ImGui.EndMenu();
            }
        }

        foreach (var file in scripts.Files)
        {
            if (ImGui.MenuItem(file))
            {
                Tig.Console.RunScript(pathPrefix + file);
            }
        }

        ImGui.PopID();
    }

    private void RenderCheatsMenu()
    {
        if (ImGui.BeginMenu("Cheats"))
        {
            var savingThrowsAlwaysFail = GameSystems.D20.Combat.SavingThrowsAlwaysFail;
            if (ImGui.MenuItem("Saving Throws Always Fail", null, ref savingThrowsAlwaysFail))
            {
                GameSystems.D20.Combat.SavingThrowsAlwaysFail = savingThrowsAlwaysFail;
            }

            var savingThrowsAlwaysSucceed = GameSystems.D20.Combat.SavingThrowsAlwaysSucceed;
            if (ImGui.MenuItem("Saving Throws Always Succeed", null, ref savingThrowsAlwaysSucceed))
            {
                GameSystems.D20.Combat.SavingThrowsAlwaysSucceed = savingThrowsAlwaysSucceed;
            }

            if (ImGui.MenuItem("Levelup"))
            {
                GameSystems.Cheats.LevelupParty();
            }

            ImGui.EndMenu();

            _forceMainMenu = true;
        }
    }

    private bool HandleMessage(uint message, nuint wParam, nint lParam)
    {
        var io = ImGui.GetIO();
        switch (message)
        {
            case WM_LBUTTONDOWN:
                io.MouseDown[0] = true;
                return io.WantCaptureMouse;
            case WM_LBUTTONUP:
                io.MouseDown[0] = false;
                return io.WantCaptureMouse;
            case WM_RBUTTONDOWN:
                io.MouseDown[1] = true;
                return io.WantCaptureMouse;
            case WM_RBUTTONUP:
                io.MouseDown[1] = false;
                return io.WantCaptureMouse;
            case WM_MBUTTONDOWN:
                io.MouseDown[2] = true;
                return io.WantCaptureMouse;
            case WM_MBUTTONUP:
                io.MouseDown[2] = false;
                return io.WantCaptureMouse;
            case WM_MOUSEWHEEL:
                io.MouseWheel += ((short) (wParam >> 16)) > 0 ? +1.0f : -1.0f;
                return io.WantCaptureMouse;
            case WM_MOUSEMOVE:
                io.MousePos.X = (short) (lParam);
                io.MousePos.Y = (short) (lParam >> 16);
                return false; // Always update, never take it
            case WM_KEYDOWN:
                if (wParam < 256)
                    io.KeysDown[(int) wParam] = true;
                return io.WantCaptureKeyboard;
            case WM_KEYUP:
                if (wParam < 256)
                    io.KeysDown[(int) wParam] = false;
                return io.WantCaptureKeyboard;
            case WM_CHAR:
                // You can also use ToAscii()+GetKeyboardState() to retrieve characters.
                if (wParam > 0 && wParam < 0x10000)
                    io.AddInputCharacter((ushort) wParam);
                return io.WantCaptureKeyboard;
            default:
                return false;
        }
    }

    private const uint WM_CHAR = 0x0102;
    private const uint WM_KEYDOWN = 0x0100;
    private const uint WM_KEYUP = 0x0101;
    private const uint WM_LBUTTONDOWN = 0x0201;
    private const uint WM_LBUTTONUP = 0x0202;
    private const uint WM_MBUTTONDOWN = 0x0207;
    private const uint WM_MBUTTONUP = 0x0208;
    private const uint WM_MOUSEMOVE = 0x0200;
    private const uint WM_MOUSEWHEEL = 0x020A;
    private const uint WM_RBUTTONDOWN = 0x0204;
    private const uint WM_RBUTTONUP = 0x0205;
}