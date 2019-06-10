using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.TextRendering;
using SpicyTemple.Core.Systems.MapSector;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems.Anim
{
    public static class AnimGoalsDebugRenderer
    {
        public static bool Enabled { get; set; }

        public static bool ShowObjectNames { get; set; }

        public static void RenderAllAnimGoals(int tileX1, int tileX2, int tileY1, int tileY2)
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

                    for (var tx = 0; tx < 64; ++tx)
                    {
                        for (var ty = 0; ty < 64; ++ty)
                        {
                            foreach (var obj in sector.GetObjectsAt(tx, ty))
                            {
                                RenderAnimGoals(obj);
                            }
                        }
                    }
                }
            }
        }

        public static void RenderAnimGoals(GameObjectBody obj)
        {
            RenderCurrentGoalPath(obj);

            var worldLocAboveHead = obj.GetLocationFull().ToInches3D(obj.GetRenderHeight());

            var topOfObjectInUi = Tig.RenderingDevice.GetCamera().WorldToScreenUi(worldLocAboveHead);

            var renderer2d = Tig.ShapeRenderer2d;

            var textEngine = Tig.RenderingDevice.GetTextEngine();

            var slotIdsPerLine = new List<int>();
            var lines = new List<string>();
            var lineHeights = new List<int>();

            var textStyle = Globals.WidgetTextStyles.GetTextStyle("default-button-text");

            var overallHeight = 0;
            foreach (var slot in GameSystems.Anim.EnumerateSlots(obj))
            {
                for (int i = 0; i <= slot.currentGoal; i++)
                {
                    var stackEntry = slot.goals[i];
                    var goalName = stackEntry.goalType.ToString();
                    if (i == slot.currentGoal)
                    {
                        lines.Add($"{goalName} ({slot.currentState})");
                    }
                    else
                    {
                        lines.Add(goalName);
                    }

                    slotIdsPerLine.Add(slotIdsPerLine.Count);
                    var metrics = textEngine.MeasureText(textStyle, lines[lines.Count - 1], 120, 500);
                    overallHeight += metrics.height + 1;
                    lineHeights.Add(metrics.height);
                }
            }

            var x = (int) (topOfObjectInUi.X - 60);
            var y = (int) (topOfObjectInUi.Y - overallHeight);

            // Render the object name above the goal list
            if (ShowObjectNames)
            {
                var t = new FormattedText();
                t.defaultStyle = textStyle.Copy();
                t.defaultStyle.align = TextAlign.Center;
                t.defaultStyle.dropShadow = true;
                t.defaultStyle.dropShadowBrush = new Brush(PackedLinearColorA.Black);
                var protoNum = obj.ProtoId;
                var displayName = GameSystems.MapObject.GetDisplayName(obj);
                t.text = $"{displayName} #{protoNum}";

                var boldStyle = t.defaultStyle.Copy();
                boldStyle.bold = true;

                t.AddFormat(boldStyle, 0, displayName.Length);

                var nameMetrics = textEngine.MeasureText(t);
                var nameRect = new Rectangle(
                    x - 60,
                    y - nameMetrics.lineHeight - 2,
                    240,
                    nameMetrics.lineHeight
                );
                textEngine.RenderText(nameRect, t);
            }

            // Draw in reverse because the stack is actually ordered the other way around
            var prevSlotIdx = -1;
            for (var i = lines.Count - 1; i >= 0; i--)
            {
                var lineHeight = lineHeights[i];
                var line = lines[i];
                var slotIdx = slotIdsPerLine[i];
                renderer2d.DrawRectangle(
                    x,
                    y,
                    120.0f,
                    lineHeight,
                    null,
                    new PackedLinearColorA(127, 127, 127, 127)
                );

                // When starting rendering info about a new slot, draw a border at the top
                // and draw the slot idx on the left, outside of the text box
                if (slotIdx != prevSlotIdx)
                {
                    prevSlotIdx = slotIdx;
                    RenderSlotHeader(x, y, slotIdx, textStyle, lineHeight);
                }

                var t = new FormattedText();
                t.defaultStyle = textStyle;
                t.text = line;

                var rect = new Rectangle(x, y, 120, lineHeight);
                textEngine.RenderText(rect, t);

                y += lineHeight + 1;
            }
        }

        private static void RenderSlotHeader(float x, float y, int slotIdx, TextStyle textStyle, int lineHeight)
        {
            var from = new Vector2(x, y - 1.0f);
            var to = new Vector2(x + 120.0f, y - 1.0f);
            Span<Line2d> borders = stackalloc Line2d[] {new Line2d(from, to, PackedLinearColorA.White)};
            Tig.ShapeRenderer2d.DrawLines(borders);

            var t = new FormattedText();
            t.defaultStyle = textStyle;
            t.defaultStyle.align = TextAlign.Left;
            t.text = $"#{slotIdx}";

            var metrics = Tig.RenderingDevice.GetTextEngine().MeasureText(t);

            var rect = new Rectangle(
                (int) (x - metrics.width - 2),
                (int) y + (lineHeight - metrics.lineHeight) / 2,
                metrics.width,
                metrics.height
            );
            Tig.RenderingDevice.GetTextEngine().RenderText(rect, t);
        }

        private static void RenderCurrentGoalPath(GameObjectBody obj)
        {
            var slot = GameSystems.Anim.GetSlot(obj);
            if (slot == null || !slot.path.IsComplete)
            {
                return;
            }

            var renderer3d = Tig.ShapeRenderer3d;

            var color = new PackedLinearColorA(1.0f, 1.0f, 1.0f, 0.5f);
            var circleBorderColor = new PackedLinearColorA(1.0f, 1.0f, 1.0f, 1.0f);
            var circleFillColor = new PackedLinearColorA(0.0f, 0.0f, 0.0f, 0.0f);

            var currentPos = obj.GetLocationFull().ToInches3D();

            for (int i = slot.path.currentNode; i + 1 < slot.path.nodeCount; i++)
            {
                var nextPos = slot.path.nodes[i].ToInches3D();
                renderer3d.DrawLineWithoutDepth(currentPos, nextPos, color);
                renderer3d.DrawFilledCircle(nextPos, 4, circleBorderColor, circleFillColor, false);
                renderer3d.DrawFilledCircle(nextPos, 4, circleBorderColor, circleFillColor, true);
                currentPos = nextPos;
            }

            // Draw the last path node
            var pathTo = slot.path.to.ToInches3D();
            renderer3d.DrawLineWithoutDepth(currentPos, pathTo, color);
            renderer3d.DrawFilledCircle(pathTo, 4, circleBorderColor, circleFillColor, false);
            renderer3d.DrawFilledCircle(pathTo, 4, circleBorderColor, circleFillColor, true);
        }
    }
}