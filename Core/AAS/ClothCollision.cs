using System;
using System.Collections.Generic;
using System.Globalization;

namespace OpenTemple.Core.AAS;

/**
    * Represents a sphere for cloth collision purposes.
    */
internal class CollisionSphere
{
    public int BoneId;
    public float Radius;
    public Matrix3x4 WorldMatrix;
    public Matrix3x4 WorldMatrixInverse;
    public CollisionSphere? Next;
}

/**
    * Represents a cylinder for cloth collision purposes.
    */
internal class CollisionCylinder
{
    public int BoneId;
    public float Radius;
    public float Height;
    public Matrix3x4 WorldMatrix;
    public Matrix3x4 WorldMatrixInverse;
    public CollisionCylinder? Next;
}

internal class CollisionGeometry
{
    public CollisionSphere? FirstSphere;
    public CollisionCylinder? FirstCylinder;

    /// <summary>
    /// Extract the cloth collision information from a list of bones.
    /// The information is parsed from the bone names.
    /// </summary>
    public static CollisionGeometry Find(IReadOnlyList<SkeletonBone> bones)
    {
        CollisionGeometry result = new CollisionGeometry();

        // Tails of the linked lists		
        CollisionSphere? spheresTail = null;
        CollisionCylinder? cylindersTail = null;

        // Parse cloth bone state from Skeleton
        for (var i = 0; i < bones.Count; i++)
        {
            var name = bones[i].Name;
            if (name.StartsWith("#Sphere", StringComparison.OrdinalIgnoreCase))
            {
                var valueStart = name.IndexOf('{');
                var valueEnd = name.IndexOf('}', valueStart);
                var radius = 0.0f;
                if (valueStart != -1 && valueEnd != -1)
                {
                    var radiusText = name.AsSpan(valueStart + 1, valueEnd - valueStart - 1);
                    float.TryParse(
                        radiusText,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out radius
                    );
                }

                var newClothSphere = new CollisionSphere();
                newClothSphere.Radius = radius;
                newClothSphere.Next = null;
                newClothSphere.BoneId = i;

                // Append to the linked list structure
                if (result.FirstSphere == null)
                {
                    result.FirstSphere = newClothSphere;
                    spheresTail = result.FirstSphere;
                }
                else
                {
                    spheresTail.Next = newClothSphere;
                    spheresTail = spheresTail.Next;
                }

                continue;
            }

            if (name.StartsWith("#Cylinder", StringComparison.InvariantCultureIgnoreCase))
            {
                var valueStart = name.IndexOf('{');
                var sep = name.IndexOf(',', valueStart);
                var valueEnd = name.IndexOf('}', sep);

                var radius = 0.0f;
                var height = 0.0f;
                if (valueStart != -1 && sep != -1 && valueEnd != -1)
                {
                    var radiusText = name.AsSpan(valueStart + 1, sep - valueStart - 1);
                    float.TryParse(
                        radiusText,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out radius
                    );

                    var heightText = name.AsSpan(sep + 1, valueEnd - sep - 1);
                    float.TryParse(
                        heightText,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out radius
                    );
                }

                var newClothCyl = new CollisionCylinder();
                newClothCyl.Radius = radius;
                newClothCyl.Height = height;
                newClothCyl.Next = null;
                newClothCyl.BoneId = i;

                if (result.FirstCylinder == null)
                {
                    result.FirstCylinder = newClothCyl;
                    cylindersTail = result.FirstCylinder;
                }
                else
                {
                    cylindersTail.Next = newClothCyl;
                    cylindersTail = cylindersTail.Next;
                }
            }
        }

        return result;
    }
}
