using System.Drawing;
using OpenTemple.Core.GFX;
using static SDL2.SDL;

namespace OpenTemple.Core.TigSubsystems;

public delegate void CursorDrawCallback(int x, int y, object userArg);

public class TigMouse
{
    [TempleDllLocation(0x10D2558C)]
    private ResourceRef<ITexture> _iconUnderCursor;

    private PointF _iconUnderCursorCenter;

    private SizeF _iconUnderCursorSize;

    [TempleDllLocation(0x101dd500)]
    public void SetDraggedIcon(string texturePath, PointF center, SizeF size = default)
    {
        using var texture = Tig.Textures.Resolve(texturePath, false);
        SetDraggedIcon(texture.Resource, center, size);
    }

    public void SetDraggedIcon(ITexture? texture, PointF center, SizeF size = default)
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
        _iconUnderCursorCenter = PointF.Empty;
        _iconUnderCursorSize = SizeF.Empty;
    }

    // This is sometimes queried by ToEE to check which callback is active
    // It contains the callback function's address
    [TempleDllLocation(0x101dd5e0)]
    public CursorDrawCallback? CursorDrawCallback { get; private set; }

    // Extra argument passed to cursor draw callback.
    public object? CursorDrawCallbackArg { get; private set; }

    [TempleDllLocation(0x101DD5C0)]
    public void SetCursorDrawCallback(CursorDrawCallback? callback, object? arg = null)
    {
        CursorDrawCallback = callback;
        CursorDrawCallbackArg = arg;
    }

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