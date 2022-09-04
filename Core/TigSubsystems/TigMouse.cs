using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GFX;
using static SDL2.SDL;

namespace OpenTemple.Core.TigSubsystems;

public delegate void CursorDrawCallback(int x, int y, object userArg);

public class TigMouse
{
    private readonly Stack<string> _cursorStash = new();

    [TempleDllLocation(0x10D2558C)]
    private ResourceRef<ITexture> _iconUnderCursor;

    private Point _iconUnderCursorCenter;

    private Size _iconUnderCursorSize;

    public void ShowCursor()
    {
        Tig.MainWindow.IsCursorVisible = true;
    }

    public void HideCursor()
    {
        Tig.MainWindow.IsCursorVisible = false;
    }

    public void SetCursor(string cursorPath)
    {
        _cursorStash.Push(cursorPath);
        SetCursorInternal(cursorPath);
    }

    private void SetCursorInternal(string cursorPath)
    {
        int hotspotX = 0;
        int hotspotY = 0;

        // TODO: Introduce special "cursor" file type that includes hotspot info to remove these hardcoded values
        // Special handling for cursors that don't have their hotspot on 0,0
        if (cursorPath.Contains("Map_GrabHand_Closed.tga")
            || cursorPath.Contains("Map_GrabHand_Open.tga")
            || cursorPath.Contains("SlidePortraits.tga"))
        {
            var data = Tig.FS.ReadBinaryFile(cursorPath);
            var info = IO.Images.ImageIO.DetectImageFormat(data);

            hotspotX = info.width / 2;
            hotspotY = info.height / 2;
        }
        else if (cursorPath.Contains("ZoomCursor.tga"))
        {
            // This was previously set from the townmap UI via function @ 0x101dd4a0
            hotspotX = 10;
            hotspotY = 11;
        }

        Tig.MainWindow.SetCursor(hotspotX, hotspotY, cursorPath);
    }

    [TempleDllLocation(0x101DD770)]
    [TemplePlusLocation("tig_mouse.cpp:180")]
    public void ResetCursor()
    {
        // The back is the one on screen
        if (_cursorStash.Count > 0)
        {
            _cursorStash.Pop();
        }

        // The back is the one on screen
        if (_cursorStash.TryPeek(out var texturePath))
        {
            SetCursorInternal(texturePath);
        }
    }

    [TempleDllLocation(0x101dd500)]
    public void SetDraggedIcon(string texturePath, Point center, Size size = default)
    {
        using var texture = Tig.Textures.Resolve(texturePath, false);
        SetDraggedIcon(texture.Resource, center, size);
    }

    public void SetDraggedIcon(ITexture? texture, Point center, Size size = default)
    {
        if (texture != null)
        {
            if (size.IsEmpty)
            {
                _iconUnderCursorSize = texture.GetSize();
            }
            else
            {
                _iconUnderCursorSize = size;
            }

            _iconUnderCursorCenter = center;
            _iconUnderCursor.Dispose();
            _iconUnderCursor = texture.Ref();
        }
        else
        {
            _iconUnderCursor.Dispose();
        }
    }

    [TempleDllLocation(0x101dd500, true)]
    public void ClearDraggedIcon()
    {
        _iconUnderCursor.Dispose();
        _iconUnderCursorCenter = Point.Empty;
        _iconUnderCursorSize = Size.Empty;
    }

    // This is sometimes queried by ToEE to check which callback is active
    // It contains the callback function's address
    [TempleDllLocation(0x101dd5e0)]
    public CursorDrawCallback? CursorDrawCallback { get; private set; }

    // Extra argument passed to cursor draw callback.
    public object CursorDrawCallbackArg { get; private set; }

    [TempleDllLocation(0x101DD5C0)]
    public void SetCursorDrawCallback(CursorDrawCallback callback, object arg = null)
    {
        CursorDrawCallback = callback;
        CursorDrawCallbackArg = arg;
    }

    public bool IsMouseOutsideWindow { get; set; } = true;

    [TempleDllLocation(0x101dd330, true)]
    public void DrawTooltip(PointF pos)
    {
        CursorDrawCallback?.Invoke((int) pos.X, (int) pos.Y, CursorDrawCallbackArg);
    }

    [TempleDllLocation(0x101dd330, true)]
    public void DrawItemUnderCursor(PointF pos)
    {
        if (!_iconUnderCursor.IsValid)
        {
            return;
        }

        Tig.ShapeRenderer2d.DrawRectangle(
            pos.X + _iconUnderCursorCenter.X,
            pos.Y + _iconUnderCursorCenter.Y,
            _iconUnderCursorSize.Width,
            _iconUnderCursorSize.Height,
            _iconUnderCursor.Resource
        );
    }

    [TempleDllLocation(0x101ddee0)]
    public void PushCursorLock()
    {
        SDL_SetRelativeMouseMode(SDL_bool.SDL_TRUE);
    }

    [TempleDllLocation(0x101dd470)]
    public void PopCursorLock()
    {
        SDL_SetRelativeMouseMode(SDL_bool.SDL_FALSE);
    }
}