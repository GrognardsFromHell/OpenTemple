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

        if (ImGui.Begin("UI Debug Menu"))
        {
            var enabled = RenderHoveredWidgetOutline;
            if (ImGui.Checkbox("Render Outline on Hover", ref enabled))
            {
                RenderHoveredWidgetOutline = enabled;
            }

            var currentMouseOver = _uiManager.CurrentMouseOverWidget;
            if (currentMouseOver != null)
            {
                ImGui.Text("Source URI: " + currentMouseOver.SourceURI);
                ImGui.Text("ID: " + currentMouseOver.Id);
            }

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
        if (widget is WidgetContainer window && window.Parent == null)
        {
            zIndex = $" Z:{window.ZIndex}";
        }
        if (ImGui.TreeNode($"{widget.GetType().Name} #{widget.GetHashCode()} - {widget.Id} ({widget.SourceURI}){zIndex}"))
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