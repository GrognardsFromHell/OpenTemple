using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.FlowModel;

namespace OpenTemple.Core.Systems;

public class TextBubbleSystem : IGameSystem, ITimeAwareSystem, IMapCloseAwareGameSystem, IResetAwareSystem
{
    private const int MaxTextBubbles = 8;

    [TempleDllLocation(0x10b3d928)]
    private static TimeSpan TextDuration => TimeSpan.FromSeconds(Globals.Config.TextDuration);

    [TempleDllLocation(0x10b3d938)]
    private readonly List<TextBubble> _bubbles = new();

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
    public void Remove(GameObject obj)
    {
        for (var i = _bubbles.Count - 1; i >= 0; i--)
        {
            var bubble = _bubbles[i];
            if (bubble.Object == obj)
            {
                _bubbles.RemoveAt(i);
                bubble.Dispose();
            }
        }

        obj.SetFlag(ObjectFlag.TEXT, false);
    }

    [TempleDllLocation(0x100a3420)]
    public void FloatText_100A3420(GameObject obj, int a2, string text)
    {
        FloatText_100A2E60(obj, text, true);
    }

    private TextBubble FindOldestForObject(GameObject obj)
    {
        TextBubble result = null;
        foreach (var textBubble in _bubbles)
        {
            if (textBubble.Object == obj)
            {
                // Find the oldest one
                if (result == null || !textBubble.IsPermanent && textBubble.Shown < result.Shown)
                {
                    result = textBubble;
                }
            }
        }

        return result;
    }

    [TempleDllLocation(0x100a2e60)]
    public void FloatText_100A2E60(GameObject obj, string text, bool withPortrait = false)
    {
        if (text.Length == 0)
        {
            return;
        }

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

            oldest.Dispose();
        }

        var paragraph = new Paragraph();
        paragraph.AppendContent(text, "ingame-text-bubble");

        var textLayout = Tig.RenderingDevice.TextEngine.CreateTextLayout(paragraph, 200, 200);

        var bubble = new TextBubble(text, textLayout, obj, TextDuration);

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
    public void SetDuration(GameObject obj, int durationSeconds)
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
                bubble.IsPermanent = true;
            }
            else
            {
                bubble.IsPermanent = false;
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
        var visibleRect = new RectangleF(Point.Empty, viewport.Camera.ViewportSize);

        foreach (var bubble in _bubbles)
        {
            GetScreenRect(viewport, bubble, out var screenRect);
            if (!screenRect.IntersectsWith(visibleRect))
            {
                continue;
            }

            Tig.RenderingDevice.TextEngine.RenderTextLayout(
                screenRect.X,
                screenRect.Y,
                bubble.TextLayout
            );

            if (!bubble.HidePortrait)
            {
                var args = new Render2dArgs();
                args.destRect = new RectangleF(
                    screenRect.X - 64,
                    screenRect.Y - 3,
                    62,
                    56
                );
                args.srcRect = new RectangleF(0, 0, 62, 56);
                args.flags = Render2dFlag.BUFFERTEXTURE;
                args.customTexture = _portraitFrame.Resource;
                Tig.ShapeRenderer2d.DrawRectangle(ref args);

                var portraitId = bubble.Object.GetInt32(obj_f.critter_portrait);
                var portraitPath = GameSystems.UiArtManager.GetPortraitPath(portraitId, PortraitVariant.Small);
                using var texture = Tig.Textures.Resolve(portraitPath, false);
                args.customTexture = texture.Resource;
                args.destRect = new RectangleF(
                    screenRect.X - 61,
                    screenRect.Y,
                    53,
                    47
                );
                args.srcRect = new RectangleF(0, 0, 53, 47); // TODO Assumption about portrait size -> bad
                Tig.ShapeRenderer2d.DrawRectangle(ref args);
            }
        }
    }

    [TempleDllLocation(0x100a3150)]
    private void GetScreenRect(IGameViewport viewport, TextBubble bubble, out RectangleF rect)
    {
        var objLocation = bubble.Object.GetLocationFull();
        var objHeight = bubble.Object.GetRenderHeight();

        var worldPos = objLocation.ToInches3D(objHeight);
        var screenPos = viewport.WorldToScreen(worldPos);

        var pos = new PointF(
            (screenPos.X - bubble.TextLayout.OverallWidth / 2.0f),
            (screenPos.Y - 20)
        );
        rect = new RectangleF(pos, new SizeF(bubble.TextLayout.OverallWidth, bubble.TextLayout.OverallHeight));
    }

    [TempleDllLocation(0x100a30f0)]
    public void RemoveAll()
    {
        foreach (var bubble in _bubbles)
        {
            bubble.Object.SetFlag(ObjectFlag.TEXT, false);
            bubble.Dispose();
        }

        _bubbles.Clear();
    }

    [TempleDllLocation(0x100a2de0)]
    public void AdvanceTime(TimePoint time)
    {
        for (var i = _bubbles.Count - 1; i >= 0; i--)
        {
            var bubble = _bubbles[i];
            if (!bubble.IsPermanent && (time - bubble.Shown) > bubble.Duration)
            {
                _bubbles.RemoveAt(i);
                bubble.Dispose();

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

internal class TextBubble : IDisposable
{
    public TextBubble(string text, TextLayout textLayout, GameObject owner, TimeSpan duration)
    {
        Text = text;
        TextLayout = textLayout;
        Object = owner;
        Shown = TimePoint.Now;
        Duration = duration;
    }

    public GameObject Object { get; }

    public string Text { get; }

    public TextLayout TextLayout { get; }

    // Formerly "flag 2"
    public bool IsPermanent { get; set; }

    // Formerly "flag 4"
    public bool HidePortrait { get; set; }

    public TimePoint Shown { get; set; }

    public TimeSpan Duration { get; set; }

    public void Dispose()
    {
        TextLayout.Dispose();
    }
}