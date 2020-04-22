using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using ImGuiNET;
using OpenTemple.Core.Location;
using OpenTemple.Core.Platform;
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

        private readonly IMainWindow _mainWindow;

        private bool _renderObjectTree;

        private bool _renderRaycastStats;

        private ImFontPtr _smallFont;

        private ImFontPtr _normalFont;

        public DebugUiSystem(IMainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            var hwnd = mainWindow.NativeHandle;

            var guiContext = ImGui.CreateContext();
            ImGui.SetCurrentContext(guiContext);

            var io = ImGui.GetIO();
            io.Fonts.AddFontDefault();
            AddRobotoFonts();

            _renderer = new ImGuiRenderer();
            if (!_renderer.ImGui_ImplDX11_Init(hwnd, mainWindow.D3D11Device))
            {
                throw new Exception("Unable to initialize IMGui!");
            }

            mainWindow.OnBeforeRendering += NewFrame;
            mainWindow.OnAfterRendering += Render;
            mainWindow.MouseEventFilter = FilterMouseEvent;
            mainWindow.WheelEventFilter = FilterWheelEvent;
            mainWindow.KeyEventFilter = FilterKeyEvent;
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
            Size screenSize = _mainWindow.RenderTargetSize;
            _renderer.ImGui_ImplDX11_NewFrame(screenSize.Width, screenSize.Height);
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

                Tig.Console?.Render(height);
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
                // return;
            }

            _forceMainMenu = false;

            if (ImGui.BeginMainMenuBar())
            {
                height = (int) ImGui.GetWindowHeight();

                locXY loc = default;
                if (GameSystems.Location != null)
                {
                    var screenSize = Tig.RenderingDevice.GetCamera().ScreenSize;
                    GameSystems.Location.ScreenToLoc(screenSize.Width / 2, screenSize.Height / 2, out loc);
                }

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

                    // TODO: ALL OF THIS IS GAME VIEW DEPENDENT
                    // Should be toggle-able on a game view per game view basis
                    // var renderSectorDebug = Globals.GameLoop.GameRenderer.RenderSectorDebugInfo;
                    // if (ImGui.MenuItem("Sector Blocking Debug", null, ref renderSectorDebug))
                    // {
                    //     Globals.GameLoop.GameRenderer.RenderSectorDebugInfo = renderSectorDebug;
                    // }
                    // 
                    // var renderSectorVisibility = Globals.GameLoop.GameRenderer.RenderSectorVisibility;
                    // if (ImGui.MenuItem("Sector Visibility", null, ref renderSectorVisibility))
                    // {
                    //     Globals.GameLoop.GameRenderer.RenderSectorVisibility = renderSectorVisibility;
                    // }
                    // 
                    // var pathFindingDebug = Globals.GameLoop.GameRenderer.DebugPathFinding;
                    // if (ImGui.MenuItem("Debug Pathfinding", null, ref pathFindingDebug))
                    // {
                    //     Globals.GameLoop.GameRenderer.DebugPathFinding = pathFindingDebug;
                    // }
                    // 
                    // if (ImGui.BeginMenu("Line of Sight"))
                    // {
                    //     var fogDebugRenderer = Globals.GameLoop.GameRenderer.MapFogDebugRenderer;
                    //     var index = 0;
                    //     foreach (var partyMember in GameSystems.Party.PartyMembers)
                    //     {
                    //         var displayName = GameSystems.MapObject.GetDisplayName(partyMember);
                    //         var selected = fogDebugRenderer.RenderFor == index;
                    //         if (ImGui.MenuItem(displayName, null, ref selected))
                    //         {
                    //             fogDebugRenderer.RenderFor = selected ? index : -1;
                    //         }
                    // 
                    //         index++;
                    //     }
                    // 
                    //     ImGui.EndMenu();
                    // }

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

                if (ImGui.BeginMenu("Screenshot"))
                {
                    anyMenuOpen = true;

                    if (ImGui.MenuItem("Game View"))
                    {
                        var screenSize = _mainWindow.RenderTargetSize;
                        throw new NotImplementedException();
                            // This should screenshot whatever the "main view" is, or
                            // provide sub-menus to allow screenshotting all views individually
// TODO                        Globals.GameLoop.TakeScreenshot(
// TODO                            "gameview.jpg",
// TODO                            screenSize.Width,
// TODO                            screenSize.Height
// TODO                        );
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Scripts"))
                {
                    anyMenuOpen = true;

                    foreach (var availableScript in Tig.Console.AvailableScripts)
                    {
                        if (ImGui.MenuItem(availableScript))
                        {
                            Tig.Console.RunScript(availableScript);
                        }
                    }

                    ImGui.EndMenu();
                }

                if (anyMenuOpen)
                {
                    _forceMainMenu = true;
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

                _forceMainMenu = true;
            }
        }

        private bool FilterMouseEvent(in NativeMouseEvent evt)
        {
            var io = ImGui.GetIO();

            var buttonIndex = evt.button switch
            {
                NativeMouseButton.LeftButton => 0,
                NativeMouseButton.RightButton => 1,
                NativeMouseButton.MidButton => 2,
                _ => -1
            };

            // Always update the mouse position for _all_ events
            io.MousePos.X = evt.windowX;
            io.MousePos.Y = evt.windowY;

            switch (evt.type)
            {
                case NativeMouseEventType.MouseButtonPress:
                    if (buttonIndex != -1)
                    {
                        io.MouseDown[buttonIndex] = true;
                    }

                    break;
                case NativeMouseEventType.MouseButtonRelease:
                    if (buttonIndex != -1)
                    {
                        io.MouseDown[buttonIndex] = false;
                    }

                    break;
                default:
                    return false;
            }

            return io.WantCaptureMouse;
        }

        private bool FilterWheelEvent(in NativeWheelEvent evt)
        {
            var io = ImGui.GetIO();
            if (evt.type == NativeWheelEventType.Wheel && evt.pixelDeltaY != 0)
            {
                io.MouseWheel += evt.pixelDeltaY > 0 ? +1.0f : -1.0f;
                return io.WantCaptureMouse;
            }

            return false;
        }

        // Maps Qt key constants to ImGui's predefined keys
        // Since this is the only place we use these, we didn't bother to introduce constants
        // for the Qt keycodes, just copy their values
        private static readonly Dictionary<int, ImGuiKey> KeyMap = new Dictionary<int, ImGuiKey>
        {
            {0x01000001, ImGuiKey.Tab},
            {0x01000012, ImGuiKey.LeftArrow},
            {0x01000014, ImGuiKey.RightArrow},
            {0x01000013, ImGuiKey.UpArrow},
            {0x01000015, ImGuiKey.DownArrow},
            {0x01000016, ImGuiKey.PageUp},
            {0x01000017, ImGuiKey.PageDown},
            {0x01000010, ImGuiKey.Home},
            {0x01000011, ImGuiKey.End},
            {0x01000006, ImGuiKey.Insert},
            {0x01000007, ImGuiKey.Delete},
            {0x01000003, ImGuiKey.Backspace},
            {0x01000005, ImGuiKey.Enter},
            {0x01000000, ImGuiKey.Escape},
            {0x41, ImGuiKey.A},
            {0x43, ImGuiKey.C},
            {0x56, ImGuiKey.V},
            {0x58, ImGuiKey.X},
            {0x59, ImGuiKey.Y},
            {0x5a, ImGuiKey.Z},
        };

        private bool FilterKeyEvent(in NativeKeyEvent evt)
        {
            var io = ImGui.GetIO();

            io.KeyCtrl = (evt.modifiers & NativeKeyboardModifiers.ControlModifier) != 0;
            io.KeyShift = (evt.modifiers & NativeKeyboardModifiers.ShiftModifier) != 0;
            io.KeyAlt = (evt.modifiers & NativeKeyboardModifiers.AltModifier) != 0;
            io.KeySuper = (evt.modifiers & NativeKeyboardModifiers.MetaModifier) != 0;

            ImGuiKey imGuiKey;
            switch (evt.type)
            {
                case NativeKeyEventType.KeyPress:
                    if (evt.text.Length == 1)
                    {
                        io.AddInputCharacter(evt.text[0]);
                    }

                    if (KeyMap.TryGetValue(evt.key, out imGuiKey))
                    {
                        io.KeysDown[(int) imGuiKey] = true;
                    }

                    break;
                case NativeKeyEventType.KeyRelease:
                    if (KeyMap.TryGetValue(evt.key, out imGuiKey))
                    {
                        io.KeysDown[(int) imGuiKey] = false;
                    }

                    break;
                default:
                    return false;
            }

            return io.WantCaptureKeyboard;
        }
    }
}