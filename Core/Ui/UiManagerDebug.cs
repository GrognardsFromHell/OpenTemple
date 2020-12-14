using System;
using System.Numerics;
using ImGuiNET;
using OpenTemple.Core.GFX;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.DOM;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui
{
    public class UiManagerDebug
    {
        private readonly UiManager _uiManager;

        private EventTarget _currentMouseOver;

        public bool DebugMenuVisible { get; set; }

        public UiManagerDebug(UiManager uiManager)
        {
            _uiManager = uiManager;

            _uiManager.Document.DocumentElement.AddEventListener("mouseover", evt =>
            {
                _currentMouseOver = evt.RelatedTarget;
            });
        }

        public bool RenderHoveredWidgetOutline { get; set; } = false;

        public void AfterRenderWidgets()
        {
            if (RenderHoveredWidgetOutline)
            {
                if (_currentMouseOver is WidgetBase widget)
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

                if (_currentMouseOver is WidgetBase currentMouseOver)
                {
                    ImGui.Text("Type: " + currentMouseOver);
                    ImGui.Text("ID: " + currentMouseOver.GetId());
                    ImGui.Text("States: " + GetStatesString(currentMouseOver));
                    ImGui.Text("Source URI: " + currentMouseOver.GetSourceURI());
                }

                if (ImGui.CollapsingHeader("Widgets"))
                {
                    foreach (var node in _uiManager.RootElement.ChildrenToArray())
                    {
                        if (node is WidgetContainer window)
                        {
                            RenderWidgetTreeNode(window);
                        }
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

            var states = GetStatesString(widget);

            if (ImGui.TreeNode($"{widget.GetType().Name} #{widget.GetHashCode()} - {widget.GetId()} {states} ({widget.GetSourceURI()}){zIndex}"))
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

        private static string GetStatesString(Element widget)
        {
            var states = "";
            foreach (var state in (EventState[]) Enum.GetValues(typeof(EventState)))
            {
                if ((widget.State & state) != 0)
                {
                    if (states != "")
                    {
                        states += ",  ";
                    }

                    states += Enum.GetName(typeof(EventState), state);
                }
            }

            return states;
        }
    }
}