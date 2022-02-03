using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Utils;
using CollectionExtensions = OpenTemple.Core.Utils.CollectionExtensions;

namespace OpenTemple.Core.Systems
{
    public enum TextFloaterColor
    {
        White = 0,
        Red = 1,
        Green = 2,
        Blue = 3,
        Yellow = 4,
        LightBlue = 5
    }

    [Flags]
    public enum TextFloaterCategory
    {
        None = 0,
        Generic = 1,
        Damage = 2
    }

    public class TextFloaterSystem : IGameSystem, IResetAwareSystem, ITimeAwareSystem,
        IMapCloseAwareGameSystem
    {
        [TempleDllLocation(0x10B3D8A4)]
        private const bool IsEditor = false; // Is Editor

        // TODO this is incorrect and the floating system needs a rework of how it fades in/out
        private const float LineHeight = 20;

        // The width/height allocated to floating text.
        private const float FloaterWidth = 200;
        private const float FloaterHeight = 5 * LineHeight;

        [TempleDllLocation(0x10b3d8ac)]
        private readonly List<TextFloater> _activeFloaters = new();

        [TempleDllLocation(0x10b3d808)]
        private TimePoint _timeReference;

        [TempleDllLocation(0x100a2040)]
        public TextFloaterSystem()
        {
        }

        [TempleDllLocation(0x100a2980)]
        public void Dispose()
        {
            RemoveAll();
        }

        [TempleDllLocation(0x100a2970)]
        public void Reset()
        {
            RemoveAll();
        }

        [TempleDllLocation(0x100a2480)]
        public void AdvanceTime(TimePoint time)
        {
            if ((time - _timeReference).Milliseconds < 50.0)
            {
                return;
            }

            _timeReference = time;
            foreach (var floater in _activeFloaters)
            {
                foreach (var line in floater.Lines)
                {
                    line.Y -= Globals.Config.TextFloatSpeed;

                    if (line.Y >= 0)
                    {
                        UpdateAlpha(line);
                    }
                }

                // Filter all lines that have left the bounding box
                floater.Lines.RemoveAll(line => line.Y < 0);
            }

            // Filter out all floaters that have no lines remaining
            for (var i = _activeFloaters.Count - 1; i >= 0; i--)
            {
                var floater = _activeFloaters[i];
                if (floater.Lines.Count == 0)
                {
                    floater.Object.SetFlag(ObjectFlag.TEXT_FLOATER, false);
                    _activeFloaters.RemoveAt(i);
                }
            }
        }

        [TempleDllLocation(0x100a2970)]
        public void CloseMap()
        {
            RemoveAll();
        }

        [TempleDllLocation(0x100a2870)]
        public void Remove(GameObject obj)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100a2200)]
        public void FloatLine(GameObject obj, TextFloaterCategory category, TextFloaterColor color, string text)
        {
            // Try to append the line to an existing floater
            TextFloater floater = null;
            foreach (var existingFloater in _activeFloaters)
            {
                if (existingFloater.Object == obj)
                {
                    floater = existingFloater;
                    break;
                }
            }

            if (floater == null)
            {
                floater = new TextFloater(obj);
                _activeFloaters.Add(floater);
            }

            floater.UpdateObjectHeight();

            var line = new TextFloaterLine();

            // Initially place the line as line 5
            line.Y = 4 * LineHeight;
            if (floater.Lines.Count > 0)
            {
                // If there's another existing line that's already overlapping the lowest line, move our new line down.
                var lastLineBottom = floater.Lines[^1].Y + floater.Lines[^1].TextLayout.OverallHeight;

                if (lastLineBottom > line.Y)
                {
                    line.Y = lastLineBottom;
                }
            }

            floater.Lines.Add(line);

            var paragraph = new Paragraph();
            paragraph.StyleIds = ImmutableList.Create("ingame-floating-text", GetColorStyle(color));
            paragraph.AppendContent(text);

            line.Text = text;
            line.TextLayout = Tig.RenderingDevice.TextEngine.CreateTextLayout(paragraph, FloaterWidth, FloaterHeight);
            line.Category = category;

            GameSystems.MapObject.SetFlags(obj, ObjectFlag.TEXT_FLOATER);
            UpdateAlpha(line);
        }

        private static string GetColorStyle(TextFloaterColor color)
        {
            return color switch
            {
                TextFloaterColor.White => "ingame-floating-text-white",
                TextFloaterColor.Red => "ingame-floating-text-red",
                TextFloaterColor.Green => "ingame-floating-text-green",
                TextFloaterColor.Blue => "ingame-floating-text-blue",
                TextFloaterColor.Yellow => "ingame-floating-text-yellow",
                TextFloaterColor.LightBlue => "ingame-floating-text-lightblue",
                _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
            };
        }

        /// <summary>
        /// The transparency of a floating line is based on the current Y-position in relation to the
        /// text floater's bounding box.
        /// </summary>
        /// <param name="floater"></param>
        [TempleDllLocation(0x100a1fa0)]
        private static void UpdateAlpha(TextFloaterLine floater)
        {
            var middleOfLine = LineHeight / 2;
            var currentPos = floater.Y + middleOfLine;

            // This will fade-out items far below the fold as if they were far up
            if (currentPos > 3 * LineHeight)
            {
                currentPos = FloaterHeight - currentPos;
            }

            if (currentPos > 2 * LineHeight)
            {
                floater.Opacity = 1;
            }
            else if (currentPos <= middleOfLine)
            {
                floater.Opacity = 0;
            }
            else
            {
                // Fade-in range is up until line 2 (of 5)
                floater.Opacity = (currentPos - middleOfLine) /
                                  (float)(2 * LineHeight - middleOfLine);
            }
        }

        [TempleDllLocation(0x100a30f0)]
        public void RemoveAll()
        {
            foreach (var floater in _activeFloaters)
            {
                floater.Object.SetFlag(ObjectFlag.TEXT_FLOATER, false);
            }

            _activeFloaters.DisposeAndClear();
        }

        [TempleDllLocation(0x100a2600)]
        public void Render(IGameViewport viewport)
        {
            if (IsEditor)
            {
                return;
            }

            var visibleRect = new Rectangle(Point.Empty, viewport.Camera.ViewportSize);

            foreach (var floater in _activeFloaters)
            {
                GetFloaterScreenRect(viewport, floater, out var floaterRect);
                if (!floaterRect.IntersectsWith(visibleRect))
                {
                    continue;
                }

                foreach (var line in floater.Lines)
                {
                    if ((Globals.Config.ActiveTextFloaters & line.Category) != line.Category)
                    {
                        // Line is from an inactive category
                        continue;
                    }

                    Tig.RenderingDevice.TextEngine.RenderTextLayout(
                        floaterRect.X,
                        floaterRect.Y + line.Y,
                        line.TextLayout,
                        new TextRenderOptions().WithOpacity(line.Opacity)
                    );
                }
            }
        }

        [TempleDllLocation(0x100a2410)]
        private static void GetFloaterScreenRect(IGameViewport viewport, TextFloater floater, out Rectangle rectangle)
        {
            var floaterBottomCenter = floater.Object.GetLocationFull().ToInches3D(floater.ObjectHeight);
            var screenPos = viewport.WorldToScreen(floaterBottomCenter);

            // This will center the intended rectangle horizontally around the object head's screen pos,
            // and place it on top.
            rectangle = new Rectangle(
                (int)(screenPos.X - FloaterWidth / 2f),
                (int)(screenPos.Y - FloaterHeight),
                (int)FloaterWidth,
                (int)FloaterHeight
            );
        }

        [TempleDllLocation(0x100a27d0)]
        public void CritterDied(GameObject critter)
        {
            if (!critter.HasFlag(ObjectFlag.TEXT_FLOATER))
            {
                return;
            }

            Stub.TODO(); // TODO: Vanilla did some stuff here to remove some of the lines, but not sure why
        }

        private class TextFloaterLine : IDisposable
        {
            public float Y;
            public string Text;
            public float Opacity;
            public TextFloaterCategory Category;
            public TextLayout TextLayout;

            public void Dispose()
            {
                TextLayout.Dispose();
            }
        }

        private class TextFloater : IDisposable
        {
            public GameObject Object { get; }
            public float ObjectHeight { get; private set; }
            public List<TextFloaterLine> Lines { get; } = new();

            public float OverallHeight
            {
                get
                {
                    float result = 0;
                    foreach (var line in Lines)
                    {
                        result += line.TextLayout.OverallHeight;
                    }

                    return result;
                }
            }

            public TextFloater(GameObject obj)
            {
                Object = obj;
                ObjectHeight = obj.GetRenderHeight();
            }

            public void UpdateObjectHeight()
            {
                ObjectHeight = Object.GetRenderHeight();
            }

            public void Dispose()
            {
                Lines.DisposeAndClear();
            }
        }
    }
}