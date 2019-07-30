using System;
using System.Collections.Generic;
using System.Drawing;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems
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

    public class TextFloaterSystem : IGameSystem, IBufferResettingSystem, IResetAwareSystem, ITimeAwareSystem,
        IMapCloseAwareGameSystem
    {
        [TempleDllLocation(0x10B3D8A4)]
        private const bool IsEditor = false; // Is Editor

        [TempleDllLocation(0x102CDF50)]
        private static readonly Dictionary<TextFloaterColor, PackedLinearColorA> Colors =
            new Dictionary<TextFloaterColor, PackedLinearColorA>
            {
                {TextFloaterColor.White, PackedLinearColorA.White},
                {TextFloaterColor.Red, new PackedLinearColorA(255, 0, 0, 255)},
                {TextFloaterColor.Green, new PackedLinearColorA(0, 255, 0, 255)},
                {TextFloaterColor.Blue, new PackedLinearColorA(64, 64, 255, 255)},
                {TextFloaterColor.Yellow, new PackedLinearColorA(255, 255, 0, 255)},
                {TextFloaterColor.LightBlue, new PackedLinearColorA(128, 226, 255, 255)},
            };

        [TempleDllLocation(0x10b3d8ac)]
        private readonly List<TextFloater> _activeFloaters = new List<TextFloater>();

        private PredefinedFont _font = PredefinedFont.PRIORY_12;

        [TempleDllLocation(0x10b3d850)]
        private readonly TigTextStyle _textStyle = new TigTextStyle
        {
            flags = 0,
            textColor = new ColorRect(PackedLinearColorA.White),
            shadowColor = new ColorRect(PackedLinearColorA.Black),
            kerning = 2,
            tracking = 5
        };

        [TempleDllLocation(0x10b3d80c)]
        private readonly int _lineHeight;

        [TempleDllLocation(0x10b3d808)]
        private TimePoint _timeReference;

        /// <summary>
        /// Defines the intended size of the text floater rectangle that will be
        /// centered above an object's "head".
        /// </summary>
        [TempleDllLocation(0x10B3D82C)] [TempleDllLocation(0x10B3D830)]
        private readonly Size _boundingBox;

        [TempleDllLocation(0x100a2040)]
        public TextFloaterSystem()
        {
            // Pre-measure the font-size with our chosen font
            Tig.Fonts.PushFont(_font);
            TigFontMetrics metrics = default;
            Tig.Fonts.Measure(_textStyle, "", ref metrics);
            Tig.Fonts.PopFont();

            _lineHeight = metrics.lineheight + 2;
            _boundingBox = new Size(
                200,
                5 * _lineHeight
            );
        }

        [TempleDllLocation(0x100a2980)]
        public void Dispose()
        {
        }

        [TempleDllLocation(0x100a1df0)]
        public void ResetBuffers()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100a2970)]
        public void Reset()
        {
            Stub.TODO();
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
            Stub.TODO();
        }

        [TempleDllLocation(0x100a2870)]
        public void Remove(GameObjectBody obj)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100a2200)]
        public void FloatLine(GameObjectBody obj, TextFloaterCategory category, TextFloaterColor color, string text)
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
            line.Y = 4 * _lineHeight;
            if (floater.Lines.Count > 0)
            {
                // If there's another existing line that's already overlapping the lowest line, move our new line down.
                var lastLineBottom = floater.Lines[^1].Y + _lineHeight;

                if (lastLineBottom > line.Y)
                {
                    line.Y = lastLineBottom;
                }
            }

            floater.Lines.Add(line);

            line.Text = text;
            line.Color = color;
            line.Category = category;

            // Measure the text height here once since it shouldn't change once the floater has been added
            Tig.Fonts.PushFont(_font);
            line.TextRectangle = Tig.Fonts.MeasureTextSize(text, _textStyle);
            Tig.Fonts.PopFont();

            GameSystems.MapObject.SetFlags(obj, ObjectFlag.TEXT_FLOATER);
            UpdateAlpha(line);
        }

        /// <summary>
        /// The transparency of a floating line is based on the current Y-position in relation to the
        /// text floater's bounding box.
        /// </summary>
        /// <param name="floater"></param>
        [TempleDllLocation(0x100a1fa0)]
        private void UpdateAlpha(TextFloaterLine floater)
        {
            var middleOfLine = _lineHeight / 2;
            var currentPos = floater.Y + middleOfLine;

            // This will fade-out items far below the fold as if they were far up
            if (currentPos > 3 * _lineHeight)
            {
                currentPos = _boundingBox.Height - currentPos;
            }

            if (currentPos > 2 * _lineHeight)
            {
                floater.Alpha = 0xFF;
            }
            else if (currentPos <= middleOfLine)
            {
                floater.Alpha = 0;
            }
            else
            {
                // Fade-in range is up until line 2 (of 5)
                var factor = (currentPos - middleOfLine) /
                             (float) (2 * _lineHeight - middleOfLine);
                floater.Alpha = (byte) (factor * 255.0f);
            }
        }

        [TempleDllLocation(0x100a2600)]
        public void Render()
        {
            if (IsEditor)
            {
                return;
            }

            Tig.Fonts.PushFont(PredefinedFont.PRIORY_12);

            var visibleRect = new Rectangle(Point.Empty, Tig.RenderingDevice.GetCamera().ScreenSize);

            foreach (var floater in _activeFloaters)
            {
                GetFloaterScreenRect(floater, out var floaterRect);
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

                    var extends = new Rectangle
                    {
                        // Horizontally center the text inside the floater's bounding rectangle
                        X = floaterRect.X + (floaterRect.Width - line.TextRectangle.Width) / 2,
                        Y = floaterRect.Y + line.Y,
                        Width = line.TextRectangle.Width,
                        Height = line.TextRectangle.Height
                    };

                    var color = Colors[line.Color];
                    color.A = line.Alpha;
                    _textStyle.textColor = new ColorRect(color);

                    Tig.Fonts.RenderText(line.Text, extends, _textStyle);
                }
            }

            Tig.Fonts.PopFont();
        }

        [TempleDllLocation(0x100a2410)]
        private void GetFloaterScreenRect(TextFloater floater, out Rectangle rectangle)
        {
            var floaterBottomCenter = floater.Object.GetLocationFull().ToInches3D(floater.ObjectHeight);
            var screenPos = Tig.RenderingDevice.GetCamera().WorldToScreenUi(floaterBottomCenter);

            // This will center the intended rectangle horizontally around the object head's screen pos,
            // and place it on top.
            rectangle = new Rectangle(
                (int) (screenPos.X - _boundingBox.Width / 2.0f),
                (int) (screenPos.Y - _boundingBox.Height - 1),
                _boundingBox.Width,
                _boundingBox.Height
            );
        }

        [TempleDllLocation(0x100a27d0)]
        public void CritterDied(GameObjectBody critter)
        {
            throw new NotImplementedException();
        }

        private class TextFloaterLine
        {
            public int Y;
            public string Text;
            public Rectangle TextRectangle;
            public byte Alpha;
            public TextFloaterColor Color;
            public TextFloaterCategory Category;
        }

        private class TextFloater
        {
            public GameObjectBody Object { get; }
            public float ObjectHeight { get; private set; }
            public List<TextFloaterLine> Lines { get; } = new List<TextFloaterLine>();

            public TextFloater(GameObjectBody obj)
            {
                Object = obj;
                ObjectHeight = obj.GetRenderHeight();
            }

            public void UpdateObjectHeight()
            {
                ObjectHeight = Object.GetRenderHeight();
            }
        }

    }
}