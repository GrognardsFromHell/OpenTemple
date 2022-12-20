using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using OpenTemple.Core.GFX;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui;

public class UiManagerDebug
{
    private readonly UiManager _uiManager;

    public bool DebugMenuVisible { get; set; }

    public UiManagerDebug(UiManager uiManager)
    {
        _uiManager = uiManager;
    }

    public bool RenderHoveredWidgetOutline { get; set; } = false;

    public void AfterRenderWidgets()
    {
        if (RenderHoveredWidgetOutline)
        {
            var widget = _uiManager.CurrentMouseOverWidget;
            if (widget != null)
            {
                RenderWidgetOutline(widget);
            }
        }

        RenderDebugUi();
    }

    private static void RenderWidgetOutline(WidgetBase widget)
    {
        var contentArea = widget.GetContentArea();
        Tig.ShapeRenderer2d.DrawRectangleOutline(
            new Vector2(contentArea.X, contentArea.Y),
            new Vector2(contentArea.X + contentArea.Width, contentArea.Y + contentArea.Height),
            PackedLinearColorA.White
        );
    }

    public void RenderDebugUi()
    {
        if (!DebugMenuVisible)
        {
            return;
        }

        var open = DebugMenuVisible;
        if (ImGui.Begin("UI Debug Menu", ref open, ImGuiWindowFlags.NoCollapse))
        {
            DebugMenuVisible = open;

            var enabled = RenderHoveredWidgetOutline;
            if (ImGui.Checkbox("Render Outline on Hover", ref enabled))
            {
                RenderHoveredWidgetOutline = enabled;
            }

            var currentMouseOver = _uiManager.CurrentMouseOverWidget;
            ImGui.Text("Mouse Over");
            if (currentMouseOver != null)
            {
                ImGui.Text("Source URI: " + currentMouseOver.SourceURI);
                ImGui.Text("ID: " + currentMouseOver.Id);
            }

            var mouseCapture = _uiManager.MouseCaptureWidget;
            if (mouseCapture != null)
            {
                ImGui.Text("Mouse Capture");
                ImGui.Text("Source URI: " + mouseCapture.SourceURI);
                ImGui.Text("ID: " + mouseCapture.Id);
            }

            var focus = _uiManager.KeyboardFocus;
            if (focus != null)
            {
                ImGui.Text("Keyboard Focus:");
                ImGui.Text("Source URI: " + focus.SourceURI);
                ImGui.Text("ID: " + focus.Id);
            }

            if (ImGui.CollapsingHeader("Widgets"))
            {
                RenderWidgetTreeNode(_uiManager.Root);
            }

            if (ImGui.CollapsingHeader("Active Hotkeys"))
            {
                RenderActiveHotkeyList(_uiManager.ActiveHotkeys);
            }

            if (ImGui.CollapsingHeader("Focus Chain"))
            {
                RenderFocusChain(_uiManager.Root);
            }

            ImGui.End();
        }
    }

    private void RenderFocusChain(WidgetBase widget)
    {
        if (!widget.Visible)
        {
            return;
        }

        if (widget.FocusMode is FocusMode.User or FocusMode.Code)
        {
            ImGui.Text($"{widget.GetType().Name} #{widget.GetHashCode()} - {widget.Id} ({widget.SourceURI})");
        }

        for (var w = widget.FirstChild; w != null; w = w.NextSibling)
        {
            RenderFocusChain(w);
        }
    }

    private static void RenderWidgetTreeNode(WidgetBase widget)
    {
        ImGui.PushID($"widget${widget.GetHashCode()}");
        var zIndex = $" Z:{widget.ZIndex}";

        if (ImGui.TreeNode($"{widget.GetType().Name} #{widget.GetHashCode()} - {widget.Id} ({widget.SourceURI}){zIndex}"))
        {
            if (ImGui.IsItemHovered())
            {
                RenderWidgetOutline(widget);
            }

            if (widget is WidgetContainer container)
            {
                foreach (var child in container.Children)
                {
                    RenderWidgetTreeNode(child);
                }
            }

            ImGui.TreePop();
        }
        else
        {
            // Render the hover outline even when collapsed
            if (ImGui.IsItemHovered())
            {
                RenderWidgetOutline(widget);
            }
        }
    }

    private void RenderActiveHotkeyList(IReadOnlyList<ActiveUiHotkey> activeHotkeys)
    {
        for (var i = 0; i < activeHotkeys.Count; i++)
        {
            ImGui.PushID($"hotkey${i}");
            var activeHotkey = activeHotkeys[i];

            ImGui.Text(activeHotkey.Hotkey.ToString());
            ImGui.SameLine();
            if (ImGui.SmallButton("Trigger"))
            {
                activeHotkey.Trigger();
            }

            if (!activeHotkey.ActiveCondition())
            {
                ImGui.SameLine();
                ImGui.TextColored(new Vector4(1, 0, 0, 1), "[disabled]");
            }
        }
    }
}