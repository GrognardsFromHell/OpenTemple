using System;
using System.Numerics;

namespace OpenTemple.Core.Utils;

public static class Vector3Extensions
{
    public static Vector4 ToVector4(this Vector3 vector, float w)
    {
        return new Vector4(vector, w);
    }

    public static Vector3 AtFixedDistanceTo(this Vector3 source, Vector3 target, float distance)
    {
        var direction = Vector3.Normalize(target - source);
        return source + distance * direction;
    }
}
public static class Vector4Extensions
{
    public static Vector3 ToVector3(this Vector4 vector)
    {
        return new Vector3(vector.X, vector.Y, vector.Z);
    }
}

public static class Matrix4x4Extensions
{
    public static Vector4 GetRow(this Matrix4x4 matrix, int row)
    {
        switch (row)
        {
            case 0:
                return new Vector4(matrix.M11, matrix.M12, matrix.M13, matrix.M14);
            case 1:
                return new Vector4(matrix.M21, matrix.M22, matrix.M23, matrix.M24);
            case 2:
                return new Vector4(matrix.M31, matrix.M32, matrix.M33, matrix.M34);
            case 3:
                return new Vector4(matrix.M41, matrix.M42, matrix.M43, matrix.M44);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}