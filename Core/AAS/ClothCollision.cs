using System;
using System.Collections.Generic;
using System.Globalization;

namespace OpenTemple.Core.AAS
{
    /**
    * Represents a sphere for cloth collision purposes.
    */
    internal class CollisionSphere
    {
        public int boneId;
        public float radius;
        public Matrix3x4 worldMatrix;
        public Matrix3x4 worldMatrixInverse;
        public CollisionSphere next;
    };

    /**
    * Represents a cylinder for cloth collision purposes.
    */
    internal class CollisionCylinder
    {
        public int boneId;
        public float radius;
        public float height;
        public Matrix3x4 worldMatrix;
        public Matrix3x4 worldMatrixInverse;
        public CollisionCylinder next;
    };

    internal class CollisionGeometry
    {
        public CollisionSphere firstSphere;
        public CollisionCylinder firstCylinder;

        /**
         * Extract the cloth collision information from a list of bones.
         * The information is parsed from the bone names.
         */
        public static CollisionGeometry Find(List<SkeletonBone> bones)
        {
            CollisionGeometry result = new CollisionGeometry();

            // Tails of the linked lists		
            CollisionSphere spheresTail = null;
            CollisionCylinder cylindersTail = null;

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
                    newClothSphere.radius = radius;
                    newClothSphere.next = null;
                    newClothSphere.boneId = i;

                    // Append to the linked list structure
                    if (result.firstSphere == null)
                    {
                        result.firstSphere = newClothSphere;
                        spheresTail = result.firstSphere;
                    }
                    else
                    {
                        spheresTail.next = newClothSphere;
                        spheresTail = spheresTail.next;
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
                    newClothCyl.radius = radius;
                    newClothCyl.height = height;
                    newClothCyl.next = null;
                    newClothCyl.boneId = i;

                    if (result.firstCylinder == null)
                    {
                        result.firstCylinder = newClothCyl;
                        cylindersTail = result.firstCylinder;
                    }
                    else
                    {
                        cylindersTail.next = newClothCyl;
                        cylindersTail = cylindersTail.next;
                    }

                    continue;
                }
            }

            return result;
        }
    }
}