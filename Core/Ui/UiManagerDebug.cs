using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Logging;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui;

public class UiManagerDebug
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private static readonly DashPattern WidgetOutlinePattern = new(1f, 4, 3, PackedLinearColorA.White, 500);
    private static readonly DashPattern ContentOutlinePattern = new(0.5f, 2, 1, new PackedLinearColorA(0x7FFFFFFF), 500);

    private readonly UiManager _uiManager;

    // Set when a picked widget should be selected in tree
    private WidgetBase? _pickedWidget;

    private bool _pickingWidget;

    private bool _showInvisible;

    // For delaying a breakpoint entrance into a widget after a short delay
    private TimePoint _debugAt;
    private WidgetBase? _debugAtTarget;

    public bool DebugMenuVisible { get; set; }

    public UiManagerDebug(UiManager uiManager)
    {
        _uiManager = uiManager;
    }

    public bool RenderHoveredWidgetOutline { get; set; }

    public void AfterRenderWidgets()
    {
        if (RenderHoveredWidgetOutline || _pickingWidget)
        {
            var mousePos = GetMouseUiPos();
            var widget = _uiManager.PickWidget(mousePos.X, mousePos.Y);
            if (widget != null)
            {
                RenderWidgetOutline(widget);

                // Render hovered content outline too
                var content = widget.PickContent(mousePos.X, mousePos.Y);
                if (content != null)
                {
                    // Transform content bounds to parents viewport rect
                    var parentBounds = widget.GetViewportBorderArea(true);
                    var contentBounds = content.GetBounds();
                    contentBounds.Offset(parentBounds.Location);
                    contentBounds = RectangleF.Intersect(contentBounds, parentBounds);

                    Tig.ShapeRenderer2d.DrawDashedRectangle(
                        contentBounds,
                        ContentOutlinePattern.Scale(Tig.MainWindow.UiScale)
                    );
                }
            }
        }

        RenderDebugUi();
    }

    private static void RenderWidgetOutline(WidgetBase widget)
    {
        var borderArea = widget.GetViewportBorderArea(true);
        var paddingArea = widget.GetViewportPaddingArea(true);

        Tig.ShapeRenderer2d.DrawDashedRectangle(
            paddingArea,
            WidgetOutlinePattern.Scale(Tig.MainWindow.UiScale)
        );
        if (borderArea != paddingArea)
        {
            Tig.ShapeRenderer2d.DrawDashedRectangle(
                borderArea,
                WidgetOutlinePattern.Scale(Tig.MainWindow.UiScale)
            );
        }
    }

    public void RenderDebugUi()
    {
        if (_debugAtTarget != null && _debugAt <= TimePoint.Now)
        {
            _debugAt = default;
            Debug(_debugAtTarget);
            _debugAtTarget = null;
        }

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

            ImGui.SameLine();
            if (ImGui.Button("Pick"))
            {
                _pickingWidget = true;
            }

            ImGui.SameLine();
            var uiMousePos = GetMouseUiPos();
            ImGui.Text($"({uiMousePos.X}, {uiMousePos.Y})");

            ImGui.Checkbox("Show Invisible", ref _showInvisible);

            var currentMouseOver = _uiManager.CurrentMouseOverWidget;
            ImGui.Text("Mouse Over: " + (currentMouseOver != null ? currentMouseOver.ToString() : "-"));

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

            if (_pickedWidget != null)
            {
                ImGui.SetNextItemOpen(true);
            }

            if (ImGui.CollapsingHeader("Widgets"))
            {
                foreach (var child in _uiManager.Root.Children)
                {
                    if (_showInvisible || child.Visible)
                    {
                        RenderWidgetTreeNode(child);
                    }
                }
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

        // Clear one-frame transient state
        if (_pickingWidget)
        {
            _pickedWidget = null;
            ImGui.CaptureMouseFromApp(true);

            var mousePos = GetMouseUiPos();
            var widget = _uiManager.PickWidget(mousePos.X, mousePos.Y);
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                _pickingWidget = false;
                _pickedWidget = widget;
            }
        }
        else
        {
            _pickedWidget = null;
        }
    }

    private static Vector2 GetMouseUiPos()
    {
        var mousePos = ImGui.GetMousePos();
        mousePos.X /= Tig.MainWindow.UiScale;
        mousePos.Y /= Tig.MainWindow.UiScale;
        return mousePos;
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

    private void RenderWidgetTreeNode(WidgetBase widget)
    {
        ImGui.PushID($"widget${widget.GetHashCode()}");

        // Show the Z-Index if it's not the default
        var zIndex = "";
        if (widget.ZIndex != 0)
        {
            zIndex = $" Z:{widget.ZIndex}";
        }

        if (_pickedWidget != null && widget.IsInclusiveAncestorOf(_pickedWidget))
        {
            ImGui.SetNextItemOpen(true);
        }

        ImGuiTreeNodeFlags flags = default;

        if (widget.FirstChild == null)
        {
            flags |= ImGuiTreeNodeFlags.Leaf;
        }

        bool colorPushed = false;
        if (_uiManager.CurrentMouseOverWidget == widget)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, 0xFFFFE880);
            colorPushed = true;
        }
        else if (!widget.Visible)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, 0xFF999999);
            colorPushed = true;
        }

        var open = ImGui.TreeNodeEx($"{widget}{zIndex}", flags);
        if (colorPushed)
        {
            ImGui.PopStyleColor();
        }

        if (_pickedWidget == widget)
        {
            ImGui.SetScrollHereY();
        }

        if (ImGui.IsItemHovered())
        {
            RenderWidgetOutline(widget);
        }

        if (open)
        {
            if (ImGui.Button("Debug"))
            {
                Debug(widget);
            }

            ImGui.SameLine();
            if (ImGui.Button("Debug in 3s"))
            {
                _debugAt = TimePoint.Now + TimeSpan.FromSeconds(3);
                _debugAtTarget = widget;
            }

            ImGui.SameLine();
            var layoutBox = widget.LayoutBox;
            ImGui.Text("Layout: " + layoutBox);

            RenderCollapsibleContentList(widget);

            if (widget is WidgetContainer container)
            {
                foreach (var child in container.Children)
                {
                    if (_showInvisible || child.Visible)
                    {
                        RenderWidgetTreeNode(child);
                    }
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

        ImGui.PopID();
    }

    private void RenderCollapsibleContentList(WidgetBase widget)
    {
        if (widget.Content.Count == 0)
        {
            return;
        }

        if (ImGui.TreeNodeEx($"Content"))
        {
            foreach (var content in widget.Content)
            {
                var bounds = content.GetBounds();
                ImGui.Text($"{content} @ {bounds}");
            }

            ImGui.TreePop();
        }
    }

    private void RenderActiveHotkeyList(IReadOnlyList<ActiveActionHotkey> activeHotkeys)
    {
        foreach (var group in activeHotkeys.GroupBy(k => k.Hotkey.Id))
        {
            var id = group.Key;
            var groupCount = group.Count();
            
            ImGui.PushID($"hotkey:{id}");

            ImGui.Text(id);
            // If there's just a single associated widget, same-line it
            if (groupCount == 1)
            {
                ImGui.SameLine();
            }

            foreach (var activeHotkey in group)
            {
                if (groupCount != 1)
                {
                    ImGui.Indent();
                    ImGui.Text(activeHotkey.Owner.ToString());
                    ImGui.SameLine();
                }
                
                if (ImGui.SmallButton("Trigger"))
                {
                    // Build a fake keyboard event
                    var e = new KeyboardEvent
                    {
                        Type = UiEventType.KeyUp,
                        PhysicalKey = activeHotkey.Hotkey.PrimaryKey.PhysicalKey,
                        VirtualKey = activeHotkey.Hotkey.PrimaryKey.VirtualKey,
                        IsAltHeld = ImGui.GetIO().KeyAlt,
                        IsShiftHeld = ImGui.GetIO().KeyShift,
                        IsCtrlHeld = ImGui.GetIO().KeyCtrl,
                        IsMetaHeld = ImGui.GetIO().KeySuper,
                    };
                    activeHotkey.Trigger(e);
                }

                if (!activeHotkey.ActiveCondition())
                {
                    ImGui.SameLine();
                    ImGui.TextColored(new Vector4(1, 0, 0, 1), "[disabled]");
                }
                
                if (groupCount != 1)
                {
                    ImGui.Unindent();
                }
            }

            ImGui.PopID();
        }
    }

    private static void Debug(WidgetBase widget)
    {
        Logger.Info("Debugging {0}", widget);
        Debugger.Break();
    }
}