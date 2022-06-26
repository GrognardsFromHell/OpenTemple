using System.Numerics;
using ImGuiNET;
using OpenTemple.Core.DebugUi;
using OpenTemple.Core.GFX;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui;

// Debug UI for the user interface
public class UiManagerDebug
{
    private readonly UiManager _uiManager;

    public bool DebugMenuVisible { get; set; }

    public bool RenderHoveredWidgetOutline { get; set; }

    public UiManagerDebug(UiManager uiManager)
    {
        _uiManager = uiManager;
    }

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

        if (ImGui.Begin("UI Debug Menu"))
        {
            var enabled = RenderHoveredWidgetOutline;
            if (ImGui.Checkbox("Render Outline on Hover", ref enabled))
            {
                RenderHoveredWidgetOutline = enabled;
            }

            var currentMouseOver = _uiManager.CurrentMouseOverWidget;
            var uiMousePos = _uiManager.Mouse.GetPos();
            ImGui.Text($"Widget Under Cursor ({uiMousePos})");
            ImGui.Text("Source URI: " + (currentMouseOver?.GetSourceURI() ?? "-"));
            ImGui.Text("ID: " + (currentMouseOver?.GetId() ?? "-"));

            if (ImGui.CollapsingHeader("Widgets"))
            {
                foreach (var window in _uiManager.ActiveWindows)
                {
                    RenderWidgetTreeNode(window);
                }
            }

            ImGui.End();
        }
    }

    private static void RenderWidgetTreeNode(WidgetBase widget)
    {
        ImGui.PushID($"widget${widget.GetHashCode()}");
        var zIndex = "";
        if (widget is WidgetContainer window && window.GetParent() == null)
        {
            zIndex = $" Z:{window.ZIndex}";
        }

        if (ImGui.TreeNode(
                $"{widget.GetType().Name} #{widget.GetHashCode()} - {widget.GetId()} ({widget.GetSourceURI()}){zIndex}"))
        {
            if (ImGui.IsItemHovered())
            {
                RenderWidgetOutline(widget);
            }

            if (widget is WidgetContainer container)
            {
                foreach (var child in container.GetChildren())
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
}