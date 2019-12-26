using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using ImGuiNET;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Scripting;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Anim;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.Raycast;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.DebugUI
{
    public class DebugUiSystem : IDisposable
    {
        private readonly ImGuiRenderer _renderer;

        private readonly WorldCamera _camera;

        private bool _renderObjectTree;

        private bool _renderRaycastStats;

        private ImFontPtr _smallFont;

        private ImFontPtr _normalFont;

        public DebugUiSystem(IMainWindow mainWindow, RenderingDevice device, WorldCamera camera)
        {
            _camera = camera;
            var hwnd = mainWindow.NativeHandle;
            var d3dDevice = device.mD3d11Device;
            var context = device.mContext;

            var guiContext = ImGui.CreateContext();
            ImGui.SetCurrentContext(guiContext);

            ImGui.GetIO().Fonts.AddFontDefault();
            AddRobotoFonts();

            _renderer = new ImGuiRenderer();
            if (!_renderer.ImGui_ImplDX11_Init(hwnd, d3dDevice, context))
            {
                throw new Exception("Unable to initialize IMGui!");
            }

            mainWindow.SetWindowMsgFilter(HandleMessage);
        }

        public void PushSmallFont()
        {
            ImGui.PushFont(_smallFont);
        }

        private void AddRobotoFonts()
        {
            var fontPath = Path.Join(
                Path.GetDirectoryName(typeof(DebugUiSystem).Assembly.Location),
                "DebugUI/Roboto-Medium.ttf"
            );
            _smallFont = ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPath, 10);
            _normalFont = ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPath, 12);
        }

        public void Dispose()
        {
            _renderer.ImGui_ImplDX11_Shutdown();
        }

        public void NewFrame()
        {
            _renderer.ImGui_ImplDX11_NewFrame((int) _camera.GetScreenWidth(), (int) _camera.GetScreenHeight());
            ImGui.PushFont(_normalFont);
        }

        public void Render()
        {
            try
            {
                if (_renderObjectTree)
                {
                    new DebugObjectGraph().Render();
                }

                if (_renderRaycastStats)
                {
                    RaycastStats.Render();
                }

                RenderMainMenuBar(out var height);

                ObjectEditors.Render();

                ActionsDebugUi.Render();

                Tig.Console.Render(height);
            }
            catch (Exception e)
            {
                ErrorReporting.ReportException(e);
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

                var screenSize = Tig.RenderingDevice.GetCamera().ScreenSize;
                GameSystems.Location.ScreenToLoc(screenSize.Width / 2, screenSize.Height / 2, out var loc);

                _forceMainMenu = ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows);

                if (ImGui.MenuItem("Console"))
                {
                    Tig.Console.IsVisible = !Tig.Console.IsVisible;
                }

                if (ImGui.BeginMenu("View"))
                {
                    var enabled = Globals.UiManager.Debug.DebugMenuVisible;
                    if (ImGui.MenuItem("UI Debug Menu", null, ref enabled))
                    {
                        Globals.UiManager.Debug.DebugMenuVisible = enabled;
                    }

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

                    var renderSectorDebug = Globals.GameLoop.GameRenderer.RenderSectorDebugInfo;
                    if (ImGui.MenuItem("Sector Blocking Debug", null, ref renderSectorDebug))
                    {
                        Globals.GameLoop.GameRenderer.RenderSectorDebugInfo = renderSectorDebug;
                    }

                    var renderSectorVisibility = Globals.GameLoop.GameRenderer.RenderSectorVisibility;
                    if (ImGui.MenuItem("Sector Visibility", null, ref renderSectorVisibility))
                    {
                        Globals.GameLoop.GameRenderer.RenderSectorVisibility = renderSectorVisibility;
                    }

                    if (ImGui.BeginMenu("Line of Sight"))
                    {
                        var fogDebugRenderer = Globals.GameLoop.GameRenderer.MapFogDebugRenderer;
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

                    var particleSystems = Globals.Config.DebugPartSys;
                    if (ImGui.MenuItem("Particle Systems", null, ref particleSystems))
                    {
                        Globals.Config.DebugPartSys = particleSystems;
                    }

                    var clipping = GameSystems.Clipping.Debug;
                    if (ImGui.MenuItem("Clipping Meshes", null, ref clipping))
                    {
                        GameSystems.Clipping.Debug = clipping;
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Scripts"))
                {
                    foreach (var availableScript in Tig.Console.AvailableScripts)
                    {
                        if (ImGui.MenuItem(availableScript))
                        {
                            Tig.Console.RunScript(availableScript);
                        }
                    }

                    ImGui.EndMenu();
                }

                RenderCheatsMenu();

                ImGui.Text($"X: {loc.locx} Y: {loc.locy}");
                ImGui.End();
            }
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

                ImGui.EndMenu();
            }
        }

        private bool HandleMessage(uint message, ulong wParam, long lParam)
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
}