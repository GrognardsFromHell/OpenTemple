using System;
using System.Diagnostics;
using System.Numerics;

namespace OpenTemple.Core.AAS
{

    /**
     * Represents an edge (between two vertices) and the distance (squared).
     */
    struct EdgeDistance {
        public short to;
        public short from;
        public float distSquared;
    };

    internal class AasClothStuff : IDisposable
    {

        private readonly Vector4[] _vertexPosBuffer;

        public int clothVertexCount = 0;
        public Memory<Vector4> clothVertexPos1 = null;
        public Memory<Vector4> clothVertexPos2 = null;
        public Memory<Vector4> clothVertexPos3 = null;
        public byte[] bytePerVertex = null;
        public int boneDistancesCount = 0;
        public int boneDistancesCountDelta = 0;
        public EdgeDistance[] boneDistances = null;
        public int boneDistances2Count = 0;
        public int boneDistances2CountDelta = 0;
        public EdgeDistance[] boneDistances2 = null;
        public CollisionSphere spheres = null;
        public CollisionCylinder cylinders = null;
        public float field_34_time = 0.0f;

        public void UpdateBoneDistances()
        {
            clothVertexPos2.CopyTo(clothVertexPos3);

            var positions = clothVertexPos2.Span;

            for (var i = 0; i < boneDistancesCountDelta + boneDistancesCount; i++) {
                ref var distance = ref boneDistances[i];
                var pos1 = positions[distance.to];
                var pos2 = positions[distance.from];

                var deltaX = pos1.X - pos2.X;
                var deltaY = pos1.Y - pos2.Y;
                var deltaZ = pos1.Z - pos2.Z;
                distance.distSquared = deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ;
            }

            for (var i = 0; i < boneDistances2CountDelta + boneDistances2Count; i++) {
                ref var distance = ref boneDistances2[i];
                var pos1 = positions[distance.to];
                var pos2 = positions[distance.from];

                var deltaX = pos1.X - pos2.X;
                var deltaY = pos1.Y - pos2.Y;
                var deltaZ = pos1.Z - pos2.Z;
                distance.distSquared = deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ;
            }
        }

        public void Simulate(float time)
        {

            int step_count = (int)MathF.Ceiling(time * 200.0f);
            float timePerStep;
            if (step_count <= 5) {
                timePerStep = time / step_count;
            }
            else {
                step_count = 5;
                timePerStep = 0.005f;
            }

            for (int i = 0; i < step_count; i++) {
                SimulateStep(timePerStep);
                ApplySprings();
                ApplySprings();
                HandleCollisions();
            }
        }

        internal AasClothStuff(
            ReadOnlySpan<Vector4> clothVertexPos,
            ReadOnlySpan<byte> bytePerClothVertex,
            ReadOnlySpan<EdgeDistance> edges,
            ReadOnlySpan<EdgeDistance> indirectEdges,
            CollisionSphere spheres,
            CollisionCylinder cylinders
        )
        {

            this.spheres = spheres;
            this.cylinders = cylinders;

            this.clothVertexCount = clothVertexPos.Length;
            _vertexPosBuffer = new Vector4[2 * clothVertexPos.Length];

            // separate the buffer into two parts (first half, second half)
            clothVertexPos1 = _vertexPosBuffer.AsMemory(0, clothVertexPos.Length);
            clothVertexPos2 = _vertexPosBuffer.AsMemory(0, clothVertexPos.Length);
            clothVertexPos3 = _vertexPosBuffer.AsMemory(clothVertexPos.Length, clothVertexPos.Length);

           clothVertexPos.CopyTo(clothVertexPos1.Span);
           clothVertexPos.CopyTo(clothVertexPos3.Span);

           Trace.Assert(bytePerClothVertex.Length == clothVertexPos.Length);
           bytePerVertex = bytePerClothVertex.ToArray();

            this.boneDistancesCount = 0;
            this.boneDistancesCountDelta = 0;
            this.boneDistances = null;

            if (!edges.IsEmpty)
            {
                this.boneDistances = edges.ToArray();

                SplitEdges(
                    this.boneDistances,
                    bytePerClothVertex,
                    out this.boneDistancesCount,
                    out this.boneDistancesCountDelta);
            }

            this.boneDistances2Count = 0;
            this.boneDistances2CountDelta = 0;
            this.boneDistances2 = null;
            if (!indirectEdges.IsEmpty)
            {
                this.boneDistances2 = indirectEdges.ToArray();

                SplitEdges(
                    this.boneDistances2,
                    bytePerClothVertex,
                    out this.boneDistances2Count,
                    out this.boneDistances2CountDelta);
            }
            field_34_time = 0.0f;
        }

        public void Dispose()
        {
            clothVertexPos1 = null;
            bytePerVertex = null;
            boneDistances = null;
            boneDistances2 = null;
        }

        private void SimulateStep(float step_time)
        {

            if (field_34_time <= 0.0 || step_time <= 0.0)
            {
                field_34_time = step_time;
            }
            else
            {
                var factor = (MathF.Pow(2.0f, -step_time) * step_time + field_34_time) / field_34_time;

                var oldState = clothVertexPos2.Span;
                var newState = clothVertexPos3.Span;

                for (var i = 0; i < clothVertexCount; i++) {
                    if (bytePerVertex[i] == 1) {
                        newState[i] = oldState[i];
                    } else {
                        ref var newPos = ref newState[i];
                        ref var oldPos = ref oldState[i];

                        newPos.X += (oldPos.X - newPos.X) * factor;
                        newPos.Y += (oldPos.Y - newPos.Y) * factor;
                        newPos.Z += (oldPos.Z - newPos.Z) * factor;
                        newState[i].Y -= step_time * step_time * 2400.0f;
                    }
                }

                Swap(ref clothVertexPos2, ref clothVertexPos3);
                field_34_time = step_time;
            }

        }

        private void ApplySprings()
        {

            var positions = clothVertexPos2.Span;

            for (var i = 0; i < boneDistancesCount; i++) {
                ref var distance = ref boneDistances[i];

                ref var pos1 = ref positions[distance.to];
                ref var pos2 = ref positions[distance.from];

                var deltaX = pos1.X - pos2.X;
                var deltaY = pos1.Y - pos2.Y;
                var deltaZ = pos1.Z - pos2.Z;

                var factor = 2 * (distance.distSquared / (deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ + distance.distSquared) - 0.5f);

                pos2.X -= deltaX * factor;
                pos2.Y -= deltaY * factor;
                pos2.Z -= deltaZ * factor;
            }

            for (var i = 0; i < boneDistancesCountDelta; i++) {

                ref var distance = ref boneDistances[boneDistancesCount + i];

                ref var pos1 = ref positions[distance.to];
                ref var pos2 = ref positions[distance.from];

                var deltaX = pos1.X - pos2.X;
                var deltaY = pos1.Y - pos2.Y;
                var deltaZ = pos1.Z - pos2.Z;

                var factor = (distance.distSquared / (deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ + distance.distSquared) - 0.5f);

                pos1.X += deltaX * factor;
                pos1.Y += deltaY * factor;
                pos1.Z += deltaZ * factor;
                pos2.X -= deltaX * factor;
                pos2.Y -= deltaY * factor;
                pos2.Z -= deltaZ * factor;
            }
        }

        private void HandleCollisions()
        {

            var positions = clothVertexPos2.Span;

            for (var i = 0; i < clothVertexCount; i++) {

                if (bytePerVertex[i] == 0) {
                    ref var pos = ref positions[i];
                    // Collision with (assumed) ground plane
                    if (pos.Y < 0.0f) {
                        pos.Y = 0.0f;
                    }

                    for (var sphere = spheres; sphere != null; sphere = sphere.next) {
                        var posInSphere = Matrix3x4.transformPosition(sphere.worldMatrixInverse, pos);
                        var distanceFromOriginSq = posInSphere.LengthSquared();
                        if (distanceFromOriginSq < sphere.radius * sphere.radius) {
                            var ratio = sphere.radius / MathF.Sqrt(distanceFromOriginSq);
                            posInSphere.X *= ratio;
                            posInSphere.Y *= ratio;
                            posInSphere.Z *= ratio;
                            pos = Matrix3x4.transformPosition(sphere.worldMatrix, posInSphere);
                        }
                    }

                    for (var cylinder = cylinders; cylinder != null; cylinder = cylinder.next) {
                        var posInCylinder = Matrix3x4.transformPosition(cylinder.worldMatrixInverse, pos);

                        if (posInCylinder.Z >= 0.0 && posInCylinder.Z <= cylinder.height) {
                            var distanceFromOriginSq = posInCylinder.X * posInCylinder.X + posInCylinder.Y * posInCylinder.Y;
                            if (distanceFromOriginSq < cylinder.radius * cylinder.radius) {
                                var ratio = cylinder.radius / MathF.Sqrt(distanceFromOriginSq);
                                posInCylinder.X *= ratio;
                                posInCylinder.Y *= ratio;

                                pos = Matrix3x4.transformPosition(cylinder.worldMatrix, posInCylinder);
                            }
                        }
                    }
                }

            }
        }

        private void SplitEdges(
            Span<EdgeDistance> edges,
            ReadOnlySpan<byte> bytePerClothVertex,
            out int boneCountOut,
            out int boneCountOutDelta
        )
        {

            var firstBucketCount = 0;
            var edgesProcessed = 0;
            if (!edges.IsEmpty)
            {
                var edgesItB = 0;
                var edgesBack = edges.Length - 1;
                var edgesItA = edgesItB;
                while (edgesProcessed < edges.Length)
                {
                    if (bytePerClothVertex[edges[edgesItB].to] == 0) {
                        if (bytePerClothVertex[edges[edgesItB].from] == 1)
                        {
                            // Swap vertices of edgesitb so that the first vertex is the one with flag=1
                            Swap(ref edges[edgesItB].to, ref edges[edgesItB].from);

                            // Then swap edgesItB with edgesItA
                            Swap(ref edges[edgesItA], ref edges[edgesItB]);
                            ++firstBucketCount;
                            ++edgesItA;
                        }
                        ++edgesProcessed;
                        ++edgesItB;
                        continue;
                    }
                    if (bytePerClothVertex[edges[edgesItB].from] == 0)
                    {
                        // Swap EdgesitA with EdgesitB
                        Swap(ref edges[edgesItA], ref edges[edgesItB]);

                        ++firstBucketCount;
                        ++edgesItA;
                        ++edgesProcessed;
                        ++edgesItB;
                        continue;
                    }
                    // Removes the edge at edge iterator position B
                    edges[edgesItB] = edges[edgesBack];
                    edges = edges.Slice(0, edges.Length - 1);
                    --edgesBack;
                }
            }

            boneCountOut = firstBucketCount;
            boneCountOutDelta = edges.Length - firstBucketCount;
        }

        private static void Swap<T>(ref T a, ref T b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }
    }
}