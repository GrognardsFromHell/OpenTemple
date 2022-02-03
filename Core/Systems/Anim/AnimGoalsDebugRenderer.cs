using System;
using System.Collections.Generic;
using System.Numerics;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.Systems.MapSector;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.Styles;

namespace OpenTemple.Core.Systems.Anim
{
    public static class AnimGoalsDebugRenderer
    {
        public static bool Enabled { get; set; }

        public static bool ShowObjectNames { get; set; }

        public static void RenderAllAnimGoals(IGameViewport viewport, int tileX1, int tileX2, int tileY1, int tileY2)
        {
            if (!Enabled)
            {
                return;
            }

            for (var secY = tileY1 / 64; secY <= tileY2 / 64; ++secY)
            {
                for (var secX = tileX1 / 64; secX <= tileX2 / 64; ++secX)
                {
                    using var sector = new LockedMapSector(secX, secY);

                    foreach (var obj in sector.EnumerateObjects())
                    {
                        RenderAnimGoals(viewport, obj);
                    }
                }
            }
        }

        public static void RenderAnimGoals(IGameViewport viewport, GameObject obj)
        {
            RenderCurrentGoalPath(viewport, obj);

            var worldLocAboveHead = obj.GetLocationFull().ToInches3D(obj.GetRenderHeight());

            var topOfObjectInUi = viewport.WorldToScreen(worldLocAboveHead);

            var renderer2d = Tig.ShapeRenderer2d;

            var textEngine = Tig.RenderingDevice.TextEngine;

            var slotIdsPerLine = new List<int>();
            var lines = new List<TextLayout>();

            var style = Globals.UiStyles.StyleResolver.Resolve(
                new[] {Globals.UiStyles.Get("default-button-text")}
            );

            var overallHeight = 0f;
            foreach (var slot in GameSystems.Anim.EnumerateSlots(obj))
            {
                for (int i = 0; i <= slot.currentGoal; i++)
                {
                    var stackEntry = slot.goals[i];
                    var goalName = stackEntry.goalType.ToString();
                    var text = i == slot.currentGoal ? $"{goalName} ({slot.currentState})" : goalName;

                    slotIdsPerLine.Add(slotIdsPerLine.Count);
                    var layout = textEngine.CreateTextLayout(style, text, 120, 500);
                    overallHeight += layout.OverallHeight + 1;
                    lines.Add(layout);
                }
            }

            var x = (int) (topOfObjectInUi.X - 60);
            var y = topOfObjectInUi.Y - overallHeight;

            // Render the object name above the goal list
            if (ShowObjectNames)
            {
                var protoNum = obj.ProtoId;
                var displayName = GameSystems.MapObject.GetDisplayName(obj);

                var paragraph = new Paragraph();
                paragraph.LocalStyles.DropShadowColor = PackedLinearColorA.Black;
                var nameStyle = new StyleDefinition {FontWeight = FontWeight.Bold};
                paragraph.AppendContent(displayName, nameStyle);
                paragraph.AppendContent($" #{protoNum}");

                using var layout = textEngine.CreateTextLayout(paragraph, 0, 0);

                textEngine.RenderTextLayout(
                    x + 60 - layout.OverallWidth / 2,
                    y - layout.OverallHeight - 2,
                    layout
                );
            }

            // Draw in reverse because the stack is actually ordered the other way around
            var prevSlotIdx = -1;
            for (var i = lines.Count - 1; i >= 0; i--)
            {
                using var layout = lines[i];
                var slotIdx = slotIdsPerLine[i];
                renderer2d.DrawRectangle(
                    x,
                    y,
                    120.0f,
                    layout.OverallHeight,
                    null,
                    new PackedLinearColorA(127, 127, 127, 127)
                );

                // When starting rendering info about a new slot, draw a border at the top
                // and draw the slot idx on the left, outside of the text box
                if (slotIdx != prevSlotIdx)
                {
                    prevSlotIdx = slotIdx;
                    RenderSlotHeader(x, y, slotIdx, style, layout.OverallHeight);
                }

                textEngine.RenderTextLayout(x, y, layout);

                y += layout.OverallHeight + 1;
            }
        }

        private static void RenderSlotHeader(float x, float y, int slotIdx, ComputedStyles styles, float lineHeight)
        {
            var from = new Vector2(x, y - 1.0f);
            var to = new Vector2(x + 120.0f, y - 1.0f);
            Span<Line2d> borders = stackalloc Line2d[] {new Line2d(from, to, PackedLinearColorA.White)};
            Tig.ShapeRenderer2d.DrawLines(borders);

            var textEngine = Tig.RenderingDevice.TextEngine;
            using var layout = textEngine.CreateTextLayout(styles, $"#{slotIdx}", 240, 0);

            var originX = x - layout.OverallWidth - 2;
            var originY = y + (lineHeight - layout.OverallHeight) / 2;
            textEngine.RenderTextLayout(originX, originY, layout);
        }

        private static void RenderCurrentGoalPath(IGameViewport viewport, GameObject obj)
        {
            var slot = GameSystems.Anim.GetSlot(obj);
            if (slot == null || !slot.path.IsComplete)
            {
                return;
            }

            var renderer3d = Tig.ShapeRenderer3d;

            var color = PackedLinearColorA.OfFloats(1.0f, 1.0f, 1.0f, 0.5f);
            var circleBorderColor = PackedLinearColorA.OfFloats(1.0f, 1.0f, 1.0f, 1.0f);
            var circleFillColor = PackedLinearColorA.OfFloats(0.0f, 0.0f, 0.0f, 0.0f);

            var currentPos = obj.GetLocationFull().ToInches3D();

            for (int i = slot.path.currentNode; i + 1 < slot.path.nodeCount; i++)
            {
                var nextPos = slot.path.nodes[i].ToInches3D();
                renderer3d.DrawLineWithoutDepth(viewport, currentPos, nextPos, color);
                renderer3d.DrawFilledCircle(viewport, nextPos, 4, circleBorderColor, circleFillColor, false);
                renderer3d.DrawFilledCircle(viewport, nextPos, 4, circleBorderColor, circleFillColor, true);
                currentPos = nextPos;
            }

            // Draw the last path node
            var pathTo = slot.path.to.ToInches3D();
            renderer3d.DrawLineWithoutDepth(viewport, currentPos, pathTo, color);
            renderer3d.DrawFilledCircle(viewport, pathTo, 4, circleBorderColor, circleFillColor, false);
            renderer3d.DrawFilledCircle(viewport, pathTo, 4, circleBorderColor, circleFillColor, true);
        }
    }
}