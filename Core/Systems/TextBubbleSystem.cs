using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui;

namespace OpenTemple.Core.Systems
{
    public class TextBubbleSystem : IGameSystem, ITimeAwareSystem, IMapCloseAwareGameSystem, IResetAwareSystem
    {
        private const PredefinedFont Font = PredefinedFont.PRIORY_12;

        [TempleDllLocation(0x10b3d8d8)]
        private static readonly TigTextStyle TextStyle = new TigTextStyle(new ColorRect(PackedLinearColorA.White))
        {
            flags = TigTextStyleFlag.TTSF_DROP_SHADOW,
            shadowColor = new ColorRect(PackedLinearColorA.Black),
            kerning = 2,
            tracking = 5
        };

        private const int MaxTextBubbles = 8;

        [TempleDllLocation(0x10b3d928)]
        private TimeSpan TextDuration => TimeSpan.FromSeconds(Globals.Config.TextDuration);

        [TempleDllLocation(0x10b3d938)]
        private readonly List<TextBubble> _bubbles = new List<TextBubble>();

        private ResourceRef<ITexture> _portraitFrame;

        [TempleDllLocation(0x100a2cb0)]
        public TextBubbleSystem()
        {
            _portraitFrame =
                Tig.Textures.Resolve("art/interface/combat_ui/combat_initiative_ui/PortraitFrame.tga", false);
        }

        public void Dispose()
        {
            RemoveAll();
            _portraitFrame.Dispose();
        }

        [TempleDllLocation(0x100a3030)]
        public void Remove(GameObjectBody obj)
        {
            for (var i = _bubbles.Count - 1; i >= 0; i--)
            {
                var bubble = _bubbles[i];
                if (bubble.Object == obj)
                {
                    _bubbles.RemoveAt(i);
                }
            }

            obj.SetFlag(ObjectFlag.TEXT, false);
        }

        [TempleDllLocation(0x100a3420)]
        public void FloatText_100A3420(GameObjectBody obj, int a2, string text)
        {
            FloatText_100A2E60(obj, text, true);
        }

        private TextBubble FindOldestForObject(GameObjectBody obj)
        {
            TextBubble result = null;
            foreach (var textBubble in _bubbles)
            {
                if (textBubble.Object == obj)
                {
                    // Find the oldest one
                    if (result == null || !textBubble.Flag2 && textBubble.Shown < result.Shown)
                    {
                        result = textBubble;
                    }
                }
            }

            return result;
        }

        [TempleDllLocation(0x100a2e60)]
        public void FloatText_100A2E60(GameObjectBody obj, string text, bool withPortrait = false)
        {
            // Clean up an old bubble to show this new one
            if (_bubbles.Count >= MaxTextBubbles)
            {
                var oldest = FindOldestForObject(obj) ?? _bubbles[0];
                _bubbles.Remove(oldest);

                // Only clear the object's flags if it was the last bubble for the object
                if (FindOldestForObject(oldest.Object) == null)
                {
                    oldest.Object.SetFlag(ObjectFlag.TEXT, false);
                }
            }

            Tig.Fonts.PushFont(Font);
            var textSize = Tig.Fonts.MeasureTextSize(text, TextStyle, 200, 200);
            Tig.Fonts.PopFont();

            var bubble = new TextBubble();
            bubble.Size = textSize.Size;
            bubble.Text = text;
            bubble.Object = obj;
            bubble.Shown = TimePoint.Now;
            bubble.Duration = TextDuration;

            if (!withPortrait)
            {
                bubble.HidePortrait = true;
            }

            _bubbles.Add(bubble);

            GameSystems.MapObject.SetFlags(obj, ObjectFlag.TEXT);
        }

        /// <summary>
        /// durationSeconds = -1 resets to default
        /// durationSeconds = -2 makes permanent
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="durationSeconds"></param>
        [TempleDllLocation(0x100a2a60)]
        public void SetDuration(GameObjectBody obj, int durationSeconds)
        {
            for (var i = _bubbles.Count - 1; i >= 0; i--)
            {
                var bubble = _bubbles[i];
                if (bubble.Object != obj)
                {
                    continue;
                }

                if (durationSeconds == -2)
                {
                    bubble.Flag2 = true;
                }
                else
                {
                    bubble.Flag2 = false;
                    var duration = TextDuration;
                    // -1 resets to default
                    if (durationSeconds > -1)
                    {
                        duration = TimeSpan.FromSeconds(durationSeconds);
                    }

                    bubble.Shown = TimePoint.Now;
                    bubble.Duration = duration;
                }

                return;
            }
        }

        [TempleDllLocation(0x100a3210)]
        public void Render(IGameViewport viewport)
        {
            var visibleRect = new Rectangle(Point.Empty, viewport.Camera.ScreenSize);

            Tig.Fonts.PushFont(Font);
            foreach (var bubble in _bubbles)
            {
                GetScreenRect(viewport, bubble, out var screenRect);
                if (!screenRect.IntersectsWith(visibleRect))
                {
                    continue;
                }

                if (bubble.Text.Length == 0)
                {
                    continue;
                }

                Tig.Fonts.RenderText(bubble.Text, screenRect, TextStyle);

                if (!bubble.HidePortrait)
                {
                    var args = new Render2dArgs();
                    args.destRect = new Rectangle(
                        screenRect.X - 64,
                        screenRect.Y - 3,
                        62,
                        56
                    );
                    args.srcRect = new Rectangle(0, 0, 62, 56);
                    args.flags = Render2dFlag.BUFFERTEXTURE;
                    args.customTexture = _portraitFrame.Resource;
                    Tig.ShapeRenderer2d.DrawRectangle(ref args);

                    var portraitId = bubble.Object.GetInt32(obj_f.critter_portrait);
                    var portraitPath = GameSystems.UiArtManager.GetPortraitPath(portraitId, PortraitVariant.Small);
                    using var texture = Tig.Textures.Resolve(portraitPath, false);
                    args.customTexture = texture.Resource;
                    args.destRect = new Rectangle(
                        screenRect.X - 61,
                        screenRect.Y,
                        53,
                        47
                    );
                    args.srcRect = new Rectangle(0, 0, 53, 47); // TODO Assumption about portrait size -> bad
                    Tig.ShapeRenderer2d.DrawRectangle(ref args);
                }
            }

            Tig.Fonts.PopFont();
        }

        [TempleDllLocation(0x100a3150)]
        private void GetScreenRect(IGameViewport viewport, TextBubble bubble, out Rectangle rect)
        {
            var objLocation = bubble.Object.GetLocationFull();
            var objHeight = bubble.Object.GetRenderHeight();

            var worldPos = objLocation.ToInches3D(objHeight);
            var screenPos = viewport.Camera.WorldToScreenUi(worldPos);

            var pos = new Point(
                (int) (screenPos.X - bubble.Size.Width / 2.0f),
                (int) (screenPos.Y - 20)
            );
            rect = new Rectangle(pos, bubble.Size);
        }

        [TempleDllLocation(0x100a30f0)]
        public void RemoveAll()
        {
            foreach (var bubble in _bubbles)
            {
                bubble.Object.SetFlag(ObjectFlag.TEXT, false);
            }

            _bubbles.Clear();
        }

        [TempleDllLocation(0x100a2de0)]
        public void AdvanceTime(TimePoint time)
        {
            for (var i = _bubbles.Count - 1; i >= 0; i--)
            {
                var bubble = _bubbles[i];
                if (!bubble.Flag2 && (time - bubble.Shown) > bubble.Duration)
                {
                    _bubbles.RemoveAt(i);
                    // If there aren't any other bubbles for the same object, we need to clear the object's flags
                    bool foundOther = false;
                    for (var j = 0; j < i; j++)
                    {
                        if (_bubbles[j].Object == bubble.Object)
                        {
                            foundOther = true;
                            break;
                        }
                    }

                    if (!foundOther)
                    {
                        bubble.Object.SetFlag(ObjectFlag.TEXT, false);
                    }
                }
            }
        }

        [TempleDllLocation(0x100a31b0)]
        public void CloseMap()
        {
            RemoveAll();
        }

        [TempleDllLocation(0x100a31b0)]
        public void Reset()
        {
            RemoveAll();
        }
    }

    internal class TextBubble
    {
        // Probably means "permanent"
        public bool Flag2 { get; set; }

        // Formerly "flag 4"
        public bool HidePortrait { get; set; }

        public GameObjectBody Object { get; set; }

        public string Text { get; set; }

        public TimePoint Shown { get; set; }

        public TimeSpan Duration { get; set; }

        public Size Size { get; set; }
    }
}