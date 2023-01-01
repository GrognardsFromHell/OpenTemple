using System;
using System.Drawing;
using System.Numerics;
using OpenTemple.Core.Location;

namespace OpenTemple.Core.GFX;

public class WorldCamera
{
    private float _xTranslation;
    private float _yTranslation;
    private float _viewportWidth;
    private float _viewportHeight;
    private bool _dirty = true;

    // This is roughly 64 * PIXEL_PER_TILE (inches per tile to be precise)
    private const float ZNear = -1814.2098860142813876543089825124f;
    private const float ZFar = 1814.2098860142813876543089825124f;

    // Current x,y offset in screen space relative to a
    // cam position of 0,0
    private Vector2 _curScreenOffset;

    private Matrix4x4 _projection;
    private Matrix4x4 _view;
    private Matrix4x4 _viewProjection;
    private Matrix4x4 _invViewProjection;

    public bool IsBoxOnScreen(Vector2 screenCenter,
        float left, float top, float right, float bottom)
    {
        var dx = _curScreenOffset.X;
        var dy = _curScreenOffset.Y;

        var screenX = dx - screenCenter.X;
        var screenY = dy - screenCenter.Y;

        // calculate viewport
        float viewportRight = _viewportWidth * 0.5f;
        float viewportLeft = -viewportRight;
        float viewportBottom = _viewportHeight * 0.5f;
        float viewportTop = -viewportBottom;

        if (screenX + left >= viewportRight)
            return false;

        if (screenX + right <= viewportLeft)
            return false;

        if (screenY + top >= viewportBottom)
            return false;

        if (screenY + bottom <= viewportTop)
            return false;

        return true;
    }

    public Matrix4x4 GetViewProj()
    {
        if (_dirty)
        {
            Update();
            _dirty = false;
        }

        return _viewProjection;
    }

    public Matrix4x4 GetProj()
    {
        if (_dirty)
        {
            Update();
        }

        return _projection;
    }

    public void SetTranslation(float x, float y)
    {
        _xTranslation = x;
        _yTranslation = y;
        _dirty = true;
    }

    public Vector2 Get2dTranslation()
    {
        if (_dirty)
        {
            Update();
        }

        return _curScreenOffset;
    }

    public float GetViewportWidth()
    {
        return _viewportWidth;
    }

    public float GetViewportHeight()
    {
        return _viewportHeight;
    }

    /// <summary>
    /// Transforms a world coordinate into the local coordinate
    /// space of the screen (in pixels).
    /// </summary>
    public Vector2 WorldToScreen(Vector3 worldPos)
    {
        if (_dirty)
        {
            Update();
        }

        var pos = Vector3.Transform(worldPos, _view);

        var screenPos = new Vector2(pos.X, pos.Y);
        screenPos.X *= -1;

        return screenPos;
    }

    /// <summary>
    /// Transforms a world coordinate into the local coordinate
    /// space of the screen (in pixels).
    /// </summary>
    [TempleDllLocation(0x10029040)]
    public Vector2 WorldToScreenUi(Vector3 worldPos)
    {
        if (_dirty)
        {
            Update();
        }

        var result = worldPos.Project(
            0,
            0,
            _viewportWidth,
            _viewportHeight,
            ZNear,
            ZFar,
            _viewProjection
        );
        return new Vector2(
            result.X,
            result.Y
        );
    }

    /// <summary>
    /// Transforms a screen coordinate relative to the upper left
    /// corner of the screen into a world position with y = 0.
    /// </summary>
    public Vector3 ScreenToWorld(float x, float y)
    {
        if (_dirty)
        {
            Update();
        }

        var screenVecFar = new Vector3(x, y, 0);

        var worldFar = screenVecFar.Unproject(
            0,
            0,
            _viewportWidth,
            _viewportHeight,
            ZNear,
            ZFar,
            _projection,
            _view,
            Matrix4x4.Identity
        );

        var screenVecClose = new Vector3(x, y, 1);
        var worldClose = screenVecClose.Unproject(
            0,
            0,
            _viewportWidth,
            _viewportHeight,
            ZNear,
            ZFar,
            _projection,
            _view,
            Matrix4x4.Identity
        );

        var worldRay = worldFar - worldClose;
        var dist = worldFar.Y / worldRay.Y;

        var result = worldFar - worldRay * dist;

        return new Vector3(result.X, result.Y, result.Z);
    }

    /**
         * Returns a ray that pierces through the screen starting at the mouse position
         * and goes to zFar, effectively.
         */
    public Ray3d GetPickRay(float x, float y)
    {
        if (_dirty)
        {
            Update();
        }

        var screenVecFar = new Vector3(x, y, 0);
        var worldFar = screenVecFar.Unproject(
            0,
            0,
            _viewportWidth,
            _viewportHeight,
            ZNear,
            ZFar,
            _projection,
            _view,
            Matrix4x4.Identity
        );

        var screenVecClose = new Vector3(x, y, 1);
        var worldClose = screenVecClose.Unproject(
            0,
            0,
            _viewportWidth,
            _viewportHeight,
            ZNear,
            ZFar,
            _projection,
            _view,
            Matrix4x4.Identity
        );

        var worldRay = worldFar - worldClose;
        var dist = worldFar.Y / worldRay.Y;

        // calculate the position on the ground (y=0) and then move
        // along the screen vector 250 units below ground and 500 above ground
        // to build the ray. This is not really correct.
        var onGround = worldFar - worldRay * dist;
        worldRay = Vector3.Normalize(worldRay);
        var belowGround = onGround - worldRay * 250.0f;
        var aboveGround = onGround + worldRay * 500.0f;

        return new Ray3d(aboveGround, belowGround - aboveGround);
    }

    // TODO: See if this can just be replaced by the proper version used below
    // This is equivalent to 10029570
    [Obsolete]
    public Vector2 ScreenToTileLegacy(int x, int y)
    {
        var tmpX = (x - _xTranslation) * 20 / locXY.INCH_PER_TILE; // * 0.70710677
        var tmpY = (y - _yTranslation) / 0.7f * 20 / locXY.INCH_PER_TILE; // * 1.0101526 originally

        return new Vector2(
            tmpY - tmpX - locXY.INCH_PER_HALFTILE,
            tmpY + tmpX - locXY.INCH_PER_HALFTILE
        );
    }

    public LocAndOffsets ScreenToTile(float screenX, float screenY)
    {
        var tmpX = (int) ((screenX - _xTranslation) / 2);
        var tmpY = (int) (((screenY - _yTranslation) / 2) / 0.7f);

        var unrotatedX = tmpY - tmpX;
        var unrotatedY = tmpY + tmpX;

        // Convert to tiles
        LocAndOffsets result;
        result.location.locx = unrotatedX / 20;
        result.location.locy = unrotatedY / 20;

        // Convert to offset within tile
        result.off_x = ((unrotatedX % 20) / 20.0f - 0.5f) * locXY.INCH_PER_TILE;
        result.off_y = ((unrotatedY % 20) / 20.0f - 0.5f) * locXY.INCH_PER_TILE;

        return result;
    }

    public Vector3 CenteredOn => ScreenToWorld(_viewportWidth / 2, _viewportHeight / 2);
    
    public void CenterOn(Vector3 pos) => CenterOn(pos.X, pos.Y, pos.Z); 

    public void CenterOn(float x, float y, float z)
    {
        _dirty = true;
        _xTranslation = 0;
        _yTranslation = 0;
        Update();

        var targetScreenPos = WorldToScreen(new Vector3(x, y, z));

        _xTranslation = targetScreenPos.X;
        _yTranslation = targetScreenPos.Y;
        _dirty = true;
    }

    private void Update()
    {
        var projection = CreateOrthographicLH(
            _viewportWidth,
            _viewportHeight,
            ZNear,
            ZFar
        );

        _projection = projection;

        /*
            This is x for sin(x) = 0.7, so x is roughly 44.42°.
            The reason here is, that Troika used a 20 by 14 grid
            and 14 / 20 = 0,7. So this ensures that the rotation
            to the isometric perspective makes the height of tiles
            70% of the width.
        */
        var view = Matrix4x4.CreateRotationX(-0.77539754f); // Roughly 45° (but not exact)

        var dxOrigin = -_viewportWidth * 0.5f;
        var dyOrigin = -_viewportHeight * 0.5f;
        dyOrigin = dyOrigin * 20.0f / 14.0f;
        Vector2 transformOrigin =
            Vector3.Transform(
                Vector3.Zero,
                Matrix4x4.CreateTranslation(dxOrigin, 0, -dyOrigin) * view
            ).ToVector2();

        /*
            We apply the translation before rotating the camera down,
            because otherwise the Z value is broken.
        */
        var dx = _xTranslation - _viewportWidth * 0.5f;
        var dy = _yTranslation - _viewportHeight * 0.5f;
        dy = dy * 20.0f / 14.0f;
        view = Matrix4x4.CreateTranslation(dx, 0, -dy) * view;

        // Calculate how much the screen has moved in x,y coordinates
        // using the current camera position
        _curScreenOffset = Vector3.Transform(Vector3.Zero, view).ToVector2();
        _curScreenOffset.X -= transformOrigin.X;
        // mCurScreenOffset.X *= -1;
        _curScreenOffset.Y -= transformOrigin.Y;
        _curScreenOffset.Y *= -1;

        view = Matrix4x4.CreateRotationY(2.3561945f) * view; // 135°

        _view = view;

        _viewProjection = view * projection;

        Matrix4x4.Invert(_viewProjection, out _invViewProjection);

        _dirty = false;
    }

    // Ported from XMMatrixOrthographicLH
    private static Matrix4x4 CreateOrthographicLH(float viewWidth,
        float viewHeight,
        float nearZ,
        float farZ)
    {
        float fRange = 1.0f / (farZ - nearZ);

        var m = new Matrix4x4();
        m.M11 = 2.0f / viewWidth;
        m.M12 = 0.0f;
        m.M13 = 0.0f;
        m.M14 = 0.0f;

        m.M21 = 0.0f;
        m.M22 = 2.0f / viewHeight;
        m.M23 = 0.0f;
        m.M24 = 0.0f;

        m.M31 = 0.0f;
        m.M32 = 0.0f;
        m.M33 = fRange;
        m.M34 = 0.0f;

        m.M41 = 0.0f;
        m.M42 = 0.0f;
        m.M43 = -fRange * nearZ;
        m.M44 = 1.0f;
        return m;
    }

    public SizeF ViewportSize
    {
        get => new(GetViewportWidth(), GetViewportHeight());
        set
        {
            if (value.Width <= 0 || value.Height <= 0)
            {
                throw new ArgumentException("Invalid viewport size: " + value);
            }

            _viewportWidth = value.Width;
            _viewportHeight = value.Height;
            _dirty = true;
        }
    }

    /// <summary>
    /// Get a normal vector that faces in Positive Z direction outwards of the screen.
    /// </summary>
    public (Vector3, Vector3) GetBillboardNormals()
    {
        if (_dirty)
        {
            Update();
        }

        // In D3D, negative Z extends outwards from the screen (left-handed coordinates)
        // 2 / width, 2 / height translates to 1 pixel due to screen-space being [-1,1]
        var up = Vector3.TransformNormal(new Vector3(0, 2 / _viewportHeight, 0), _invViewProjection);
        var right = Vector3.TransformNormal(new Vector3(2 / _viewportWidth, 0, 0), _invViewProjection);
        return (right, up);
    }
}