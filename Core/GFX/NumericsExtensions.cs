using System.Numerics;

namespace OpenTemple.Core.GFX;

/// <summary>
/// Some functions of DirectXMath ported.
/// License: MIT
/// See https://github.com/microsoft/DirectXMath
/// </summary>
public static class Vector3Extensions
{
    public static Vector3 Project
    (
        this Vector3 v,
        float    viewportX,
        float    viewportY,
        float    viewportWidth,
        float    viewportHeight,
        float    viewportMinZ,
        float    viewportMaxZ,
        Matrix4x4 viewProj
    )
    {
        var halfViewportWidth = viewportWidth * 0.5f;
        var halfViewportHeight = viewportHeight * 0.5f;

        var scale = new Vector3(halfViewportWidth, -halfViewportHeight, viewportMaxZ - viewportMinZ);
        var offset = new Vector3(viewportX + halfViewportWidth, viewportY + halfViewportHeight, viewportMinZ);

        var result = Vector3.Transform(v, viewProj);

        return result * scale + offset;
    }

    /// <summary>
    /// Projects a 3D vector from screen space into object space.
    /// </summary>
    /// <param name="V">3D vector in screen space that will be projected into object space. X and Y are in pixels, while Z is 0.0 (at ViewportMinZ) to 1.0 (at ViewportMaxZ).</param>
    /// <returns></returns>
    public static Vector3 Unproject(
        this Vector3 Pos,
        float ViewportX,
        float ViewportY,
        float ViewportWidth,
        float ViewportHeight,
        float ViewportMinZ,
        float ViewportMaxZ,
        Matrix4x4 Projection,
        Matrix4x4 View,
        Matrix4x4 World
    )
    {
        Vector4 V = new Vector4(Pos, 1.0f);

        Vector4 D = new Vector4(-1.0f, 1.0f, 0.0f, 0.0f);

        var Scale = new Vector4(ViewportWidth * 0.5f, -ViewportHeight * 0.5f, ViewportMaxZ - ViewportMinZ, 1.0f);
        Scale = Vector4.One / Scale; // reciprocal

        var Offset = new Vector4(-ViewportX, -ViewportY, -ViewportMinZ, 0.0f);
        Offset = Scale * Offset + D;

        var Transform = Matrix4x4.Multiply(World, View);
        Transform = Matrix4x4.Multiply(Transform, Projection);
        Matrix4x4.Invert(Transform, out Transform);

        var Result = V * Scale + Offset;
        Result = Vector4.Transform(Result, Transform);
        Result /= Result.W;
        return new Vector3(Result.X, Result.Y, Result.Z);
    }

    public static Vector2 ToVector2(this Vector3 vector) => new(vector.X, vector.Y);
}