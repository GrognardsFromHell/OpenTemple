using System.Numerics;

namespace OpenTemple.Particles;

public struct Box2d
{
    public float left;
    public float top;
    public float right;
    public float bottom;

    public Vector2 GetTopLeft()
    {
        return new Vector2(left, top);
    }

    public Vector2 GetTopRight()
    {
        return new Vector2(right, top);
    }

    public Vector2 GetBottomLeft()
    {
        return new Vector2(left, bottom);
    }

    public Vector2 GetBottomRight()
    {
        return new Vector2(right, bottom);
    }

    public Vector2 GetCenter()
    {
        return new Vector2((left + right) / 2.0f,
            (top + bottom) / 2.0f);
    }
}