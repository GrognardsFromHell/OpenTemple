using System.Drawing;
using System.Numerics;
using OpenTemple.Core.Location;

namespace OpenTemple.Core.GFX
{
    public class WorldCamera
    {
        public bool IsBoxOnScreen(Vector2 screenCenter,
            float left, float top, float right, float bottom)
        {
            var dx = mCurScreenOffset.X;
            var dy = mCurScreenOffset.Y;

            var screenX = dx - screenCenter.X;
            var screenY = dy - screenCenter.Y;

            // calculate viewport
            float viewportRight = mScreenWidth * 0.5f / mScale;
            float viewportLeft = -viewportRight;
            float viewportBottom = mScreenHeight * 0.5f / mScale;
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
            if (mDirty)
            {
                Update();
                mDirty = false;
            }

            return mViewProjection;
        }

        public Matrix4x4 GetProj()
        {
            if (mDirty)
            {
                Update();
            }

            return mProjection;
        }

        public void SetTranslation(float x, float y)
        {
            mXTranslation = x;
            mYTranslation = y;
            mDirty = true;
        }

        public Vector2 GetTranslation()
        {
            return new Vector2(mXTranslation, mYTranslation);
        }

        public Vector2 Get2dTranslation()
        {
            if (mDirty)
            {
                Update();
            }

            return mCurScreenOffset;
        }

        public void SetScreenWidth(float width, float height)
        {
            mScreenWidth = width;
            mScreenHeight = height;
            mDirty = true;
        }

        public float GetScreenWidth()
        {
            return mScreenWidth;
        }

        public float GetScreenHeight()
        {
            return mScreenHeight;
        }

        public void SetScale(float scale)
        {
            mScale = scale;
            mDirty = true;
        }

        public float GetScale()
        {
            return mScale;
        }

        public void SetIdentityTransform(bool enable)
        {
            mIdentityTransform = enable;
            mDirty = true;
        }

        /**
        * Transforms a world coordinate into the local coordinate
        * space of the screen (in pixels).
        */
        public Vector2 WorldToScreen(Vector3 worldPos)
        {
            if (mDirty)
            {
                Update();
            }

            var pos = Vector3.Transform(worldPos, mView);

            var screenPos = new Vector2(pos.X, pos.Y);
            screenPos.X *= -1;

            return screenPos;
        }

        /**
        * Transforms a world coordinate into the local coordinate
        * space of the screen (in pixels).
        */
        [TempleDllLocation(0x10029040)]
        public Vector2 WorldToScreenUi(Vector3 worldPos)
        {
            var result = WorldToScreen(worldPos);
            result.X *= -1;
            result.Y *= -1;

            // Move the origin to the top left instead of the center of the screen
            result.X += mScreenWidth / 2;
            result.Y += mScreenHeight / 2;

            return result;
        }

        /**
         * Transforms a screen coordinate relative to the upper left
         * corner of the screen into a world position with y = 0.
         */
        public Vector3 ScreenToWorld(float x, float y)
        {
            if (mDirty)
            {
                Update();
            }

            var screenVecFar = new Vector3(x, y, 0);

            var worldFar = screenVecFar.Unproject(
                0,
                0,
                mScreenWidth,
                mScreenHeight,
                zNear,
                zFar,
                mProjection,
                mView,
                Matrix4x4.Identity
            );

            var screenVecClose = new Vector3(x, y, 1);
            var worldClose = screenVecClose.Unproject(
                0,
                0,
                mScreenWidth,
                mScreenHeight,
                zNear,
                zFar,
                mProjection,
                mView,
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
            var screenVecFar = new Vector3(x, y, 0);
            var worldFar = screenVecFar.Unproject(
                0,
                0,
                mScreenWidth,
                mScreenHeight,
                zNear,
                zFar,
                mProjection,
                mView,
                Matrix4x4.Identity
            );

            var screenVecClose = new Vector3(x, y, 1);
            var worldClose = screenVecClose.Unproject(
                0,
                0,
                mScreenWidth,
                mScreenHeight,
                zNear,
                zFar,
                mProjection,
                mView,
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
        public Vector2 ScreenToTileLegacy(int x, int y)
        {
            var tmpX = (x - mXTranslation) * 20 / locXY.INCH_PER_TILE; // * 0.70710677
            var tmpY = (y - mYTranslation) / 0.7f * 20 / locXY.INCH_PER_TILE; // * 1.0101526 originally

            return new Vector2(
                tmpY - tmpX - locXY.INCH_PER_HALFTILE,
                tmpY + tmpX - locXY.INCH_PER_HALFTILE
            );
        }

        public LocAndOffsets ScreenToTile(int screenX, int screenY)
        {
            var tmpX = (int) ((screenX - mXTranslation) / 2);
            var tmpY = (int) (((screenY - mYTranslation) / 2) / 0.7f);

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

        // replaces 10028EC0
        // Apparently used for townmap projection !
        [TempleDllLocation(0x10028ec0)]
        public Vector2 TileToWorld(locXY tilePos)
        {
            return new Vector2(
                (tilePos.locy - tilePos.locx - 1) * 20,
                (tilePos.locy + tilePos.locx) * 14
            );
        }

        public void CenterOn(float x, float y, float z)
        {
            mDirty = true;
            mXTranslation = 0;
            mYTranslation = 0;
            Update();

            var targetScreenPos = WorldToScreen(new Vector3(x, y, z));

            mXTranslation = targetScreenPos.X;
            mYTranslation = targetScreenPos.Y;
            mDirty = true;
        }

        public Matrix4x4 GetUiProjection()
        {
            if (mDirty)
            {
                Update();
                mDirty = false;
            }

            return mUiProjection;
        }

        private float mXTranslation = 0.0f;
        private float mYTranslation = 0.0f;
        private float mScale = 1.0f;
        private float mScreenWidth;
        private float mScreenHeight;
        private bool mDirty = true;
        private bool mIdentityTransform = false;

        // This is roughly 64 * PIXEL_PER_TILE (inches per tile to be precise)
        private const float zNear = -1814.2098860142813876543089825124f;
        private const float zFar = 1814.2098860142813876543089825124f;

        // Current x,y offset in screen space relative to a
        // cam position of 0,0
        private Vector2 mCurScreenOffset;

        private Matrix4x4 mProjection;
        private Matrix4x4 mView;
        private Matrix4x4 mViewProjection;
        private Matrix4x4 mViewUntranslated;
        private Matrix4x4 mInvViewProjection;
        private Matrix4x4 mUiProjection;

        private void Update()
        {
            if (mIdentityTransform)
            {
                mViewProjection = Matrix4x4.Identity;
                return;
            }

            var projection = CreateOrthographicLH(mScreenWidth / mScale,
                mScreenHeight / mScale,
                zNear,
                zFar);

            mProjection = projection;

            /*
                This is x for sin(x) = 0.7, so x is roughly 44.42°.
                The reason here is, that Troika used a 20 by 14 grid
                and 14 / 20 = 0,7. So this ensures that the rotation
                to the isometric perspective makes the height of tiles
                70% of the width.
            */
            var view = Matrix4x4.CreateRotationX(-0.77539754f); // Roughly 45° (but not exact)

            var dxOrigin = -mScreenWidth * 0.5f;
            var dyOrigin = -mScreenHeight * 0.5f;
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
            var dx = mXTranslation - mScreenWidth * 0.5f;
            var dy = mYTranslation - mScreenHeight * 0.5f;
            dy = dy * 20.0f / 14.0f;
            view = Matrix4x4.CreateTranslation(dx, 0, -dy) * view;

            // Calculate how much the screen has moved in x,y coordinates
            // using the current camera position
            mCurScreenOffset = Vector3.Transform(Vector3.Zero, view).ToVector2();
            mCurScreenOffset.X -= transformOrigin.X;
            // mCurScreenOffset.X *= -1;
            mCurScreenOffset.Y -= transformOrigin.Y;
            mCurScreenOffset.Y *= -1;

            view = Matrix4x4.CreateRotationY(2.3561945f) * view; // 135°

            mView = view;

            mViewProjection = view * projection;

            Matrix4x4.Invert(projection, out mInvViewProjection);

            // Build a projection matrix that maps [0,w] and [0,h] to the screen.
            // To be used for UI drawing
            // TODO: We used a "LH" version here... what's equivalent?
            mUiProjection = CreateOrthographicOffCenterLH(0, mScreenWidth, mScreenHeight, 0, 1, 0);

            mDirty = false;
        }

        // Ported from XMMatrixOrthographicLH
        private static Matrix4x4 CreateOrthographicLH(float ViewWidth,
            float ViewHeight,
            float NearZ,
            float FarZ)
        {
            float fRange = 1.0f / (FarZ - NearZ);

            var m = new Matrix4x4();
            m.M11 = 2.0f / ViewWidth;
            m.M12 = 0.0f;
            m.M13 = 0.0f;
            m.M14 = 0.0f;

            m.M21 = 0.0f;
            m.M22 = 2.0f / ViewHeight;
            m.M23 = 0.0f;
            m.M24 = 0.0f;

            m.M31 = 0.0f;
            m.M32 = 0.0f;
            m.M33 = fRange;
            m.M34 = 0.0f;

            m.M41 = 0.0f;
            m.M42 = 0.0f;
            m.M43 = -fRange * NearZ;
            m.M44 = 1.0f;
            return m;
        }

        // Ported from XMMatrixOrthographicOffCenterLH
        private static Matrix4x4 CreateOrthographicOffCenterLH(float ViewLeft,
            float ViewRight,
            float ViewBottom,
            float ViewTop,
            float NearZ,
            float FarZ)
        {
            float ReciprocalWidth = 1.0f / (ViewRight - ViewLeft);
            float ReciprocalHeight = 1.0f / (ViewTop - ViewBottom);
            float fRange = 1.0f / (FarZ - NearZ);

            var m = new Matrix4x4();
            m.M11 = ReciprocalWidth + ReciprocalWidth;
            m.M12 = 0.0f;
            m.M13 = 0.0f;
            m.M14 = 0.0f;

            m.M21 = 0.0f;
            m.M22 = ReciprocalHeight + ReciprocalHeight;
            m.M23 = 0.0f;
            m.M24 = 0.0f;

            m.M31 = 0.0f;
            m.M32 = 0.0f;
            m.M33 = fRange;
            m.M34 = 0.0f;

            m.M41 = -(ViewLeft + ViewRight) * ReciprocalWidth;
            m.M42 = -(ViewTop + ViewBottom) * ReciprocalHeight;
            m.M43 = -fRange * NearZ;
            m.M44 = 1.0f;
            return m;
        }

        public Size ScreenSize => new Size((int) GetScreenWidth(), (int) GetScreenHeight());
    }
}