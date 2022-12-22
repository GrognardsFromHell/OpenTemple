using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;

namespace OpenTemple.Core.AAS;

internal struct VariationSelector
{
    public int VariationId;
    public float Factor;
}

internal class AnimatedModel : IDisposable
{
    public readonly Skeleton? Skeleton;
    public readonly int VariationCount;
    public readonly VariationSelector[] Variations = new VariationSelector[8];
    public readonly bool HasClothBones;
    public readonly int ClothBoneId;
    public readonly List<AasClothStuff1> ClothStuff1 = new();
    public readonly CollisionSphere? CollisionSpheresHead;
    public readonly CollisionCylinder? CollisionCylindersHead;
    public readonly List<AasSubmeshWithMaterial> Submeshes = new();
    public readonly Matrix3x4[] BoneMatrices;
    public EventHandler EventHandler { get; set; } = new();   
    public AnimPlayer? RunningAnimsHead;
    public AnimPlayer? NewestRunningAnim;
    public Matrix3x4 CurrentWorldMatrix; // Current world matrix
    public Matrix3x4 WorldMatrix = Matrix3x4.Identity;    
    public bool SubmeshesValid;
    public float Scale;
    public float ScaleInv;
    public float DrivenTime;
    public float TimeForClothSim;
    public float DrivenDistance;
    public float DrivenRotation;     
    public uint FieldF8;
    public uint FieldFc;
    private IRenderState? _renderState;
    
    public AnimatedModel(Skeleton skeleton)
    {
        Skeleton = skeleton;
        Scale = 1.0f;
        ScaleInv = 1.0f;
        VariationCount = 1;
        Variations[0].VariationId = -1;
        Variations[0].Factor = 1.0f;

        var bones = skeleton.Bones;
        BoneMatrices = new Matrix3x4[bones.Count + 1];

        // count normal bones
        ClothBoneId = skeleton.FindBoneIdxByName("#ClothBone");
        if (ClothBoneId == -1)
        {
            ClothBoneId = bones.Count;
            HasClothBones = false;
        }
        else
        {
            HasClothBones = true;
            
            // Discover the collision geometry used for cloth simulation
            var collisionGeom = CollisionGeometry.Find(bones);
            CollisionSpheresHead = collisionGeom.FirstSphere;
            CollisionCylindersHead = collisionGeom.FirstCylinder;
        }
    }

    public void SetScale(float scale)
    {
        this.Scale = scale;
        if (scale <= 0.0f)
        {
            ScaleInv = 0.0f;
        }
        else
        {
            ScaleInv = 1.0f / scale;
        }
    }

    public int GetBoneCount()
    {
        return Skeleton?.Bones.Count ?? 0;
    }

    public string GetBoneName(int boneIdx)
    {
        if (Skeleton == null)
        {
            throw new InvalidOperationException("This animated model has no skeleton.");
        }
        
        return Skeleton.Bones[boneIdx].Name;
    }

    public int GetBoneParentId(int boneIdx)
    {
        if (Skeleton == null)
        {
            throw new InvalidOperationException("This animated model has no skeleton.");
        }
        
        return Skeleton.Bones[boneIdx].ParentId;
    }

    public void AddMesh(Mesh mesh, IMaterialResolver matResolver)
    {
        ResetSubmeshes();

        // If the skeleton has cloth bones, normalize the vertex bone weights
        if (HasClothBones)
        {
            mesh.RenormalizeClothVertices(ClothBoneId);
        }

        // Create a material group for every material in the SKM file and attach the SKM file to the group (in case a
        // material is used by multiple attached SKM files)
        var materials = mesh.Materials;
        for (var i = 0; i < materials.Count; i++)
        {
            var matHandle = matResolver.Acquire(materials[i], mesh.Path);

            var submesh = GetOrAddSubmesh(matHandle, matResolver);
            if (submesh.AttachMesh(mesh, i) < 0)
            {
                throw new AasException(
                    $"Failed to attach mesh '{mesh.Path}' with material index '{i}' to material group.");
            }
        }

        if (!HasClothBones)
        {
            return;
        }

        var cs1 = new AasClothStuff1(mesh, ClothBoneId, CollisionSpheresHead, CollisionCylindersHead);
        ClothStuff1.Add(cs1);
    }

    public void RemoveMesh(Mesh mesh)
    {
        ResetSubmeshes();

        for (var i = Submeshes.Count - 1; i >= 0; i--)
        {
            var submesh = Submeshes[i];
            submesh.DetachMesh(mesh);

            // Remove the submesh if it has no attachments left
            if (!submesh.HasAttachedMeshes())
            {
                Submeshes.RemoveAt(i);
            }
        }

        if (HasClothBones)
        {
            for (int i = 0; i < ClothStuff1.Count; i++)
            {
                var clothStuff = ClothStuff1[i];
                if (clothStuff.mesh == mesh)
                {
                    ClothStuff1.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public void ResetSubmeshes()
    {
        if (SubmeshesValid)
        {
            foreach (var submesh in Submeshes)
            {
                submesh.ResetState();
            }

            SubmeshesValid = false;
        }
    }

    private static readonly AasVertexState[] VertexState = new AasVertexState[0x7FFF];
    private static readonly AasWorkSet[] WorkSet = new AasWorkSet[0x47FF7];

    private static readonly SubmeshVertexClothStateWithFlag[] ClothVerticesWithFlag =
        new SubmeshVertexClothStateWithFlag[0x7FFF];

    private static readonly SubmeshVertexClothStateWithoutFlag[] ClothVerticesWithoutFlag =
        new SubmeshVertexClothStateWithoutFlag[0x7FFF];

    private readonly short[] _vertexIdxMapping = new short[0x7FFF];
    private readonly short[] _primVertIdx = new short[0xFFFF * 3];

    [TempleDllLocation(0x10267640)]
    private unsafe void UpdateSubmeshes()
    {
        // Cached bone mappings for SKM file
        Mesh boneMappingMesh = null;
        Span<BoneMapping> boneMapping = stackalloc BoneMapping[1024];
        int usedSkaBoneCount = 0; // Built by bone_mapping, may include gaps
        Span<PosPair> posPairs = stackalloc PosPair[1024];
        Span<SkaBoneAffectedCount> skaBoneAffectedCount = stackalloc SkaBoneAffectedCount[1024];

        // Mapping between index of vertex in SKM file, and index of vertex in submesh vertex array
        Span<short> vertexIdxMapping = _vertexIdxMapping;
        Span<short> primVertIdx = _primVertIdx; // 3 vertex indices per face

        int clothVerticesWithFlagCount = 0;
        int clothVerticesWithoutFlagCount = 0;
        
        // Scratch-Variables for inverting vertex attachments
        Span<float> vertexBoneAttachWeights = stackalloc float[6];
        Span<short> vertexBoneAttachSkaIds = stackalloc short[6]; // Attached bones mapped to SKA bone ids
        Span<Vector4> attachmentPositions = stackalloc Vector4[6];
        Span<Vector4> attachmentNormals = stackalloc Vector4[6];
        
        foreach (var submesh in Submeshes)
        {
            int worksetCount = 0;

            int primitiveCount = 0; // Primarily used for indexing into prim_vert_idx
            int vertexCount = 0;
            int totalVertexAttachmentCount = 0;

            foreach (var attachment in submesh.GetAttachedMeshes())
            {
                var mesh = attachment.mesh;

                // Rebuild the bone mapping if we're working on a new SKM file
                // TODO: This is expensive and stupid since pairs of SKM/SKA files are reused all the time
                // and the mapping between their bones is static
                if (mesh != boneMappingMesh)
                {
                    usedSkaBoneCount = BoneMapping.build_bone_mapping(boneMapping, Skeleton, mesh) + 1;
                    boneMappingMesh = mesh;
                }

                // Find the cloth state (?) for the mesh
                var matchingclothstuff1Idx = ClothStuff1.Count;
                if (HasClothBones)
                {
                    for (int i = 0; i < ClothStuff1.Count; i++)
                    {
                        if (ClothStuff1[i].mesh == mesh)
                        {
                            matchingclothstuff1Idx = i;
                            break;
                        }
                    }
                }

                // Clear bone state
                PosPair posPair = new PosPair(-1, -1);
                posPairs.Slice(0, usedSkaBoneCount).Fill(posPair);
                skaBoneAffectedCount.Fill(new SkaBoneAffectedCount());

                // Clear vertex idx mapping state
                vertexIdxMapping.Slice(0, mesh.Vertices.Length).Fill(-1);

                Span<MeshVertex> vertices = mesh.Vertices;

                foreach (ref var face in mesh.Faces)
                {
                    // Skip faces with different materials than this submesh
                    if (face.MaterialIndex != attachment.materialIdx)
                    {
                        continue;
                    }

                    var primVertIdxOut = primitiveCount++ * 3;

                    for (var faceVertexIdx = 0; faceVertexIdx < 3; faceVertexIdx++)
                    {
                        var skmVertexIdx = face[faceVertexIdx];
                        ref var vertex = ref vertices[skmVertexIdx];

                        // Is it mapped for the submesh already, then reuse the existing vertex
                        var mappedIdx = vertexIdxMapping[skmVertexIdx];
                        if (mappedIdx != -1)
                        {
                            primVertIdx[primVertIdxOut++] = mappedIdx;
                            continue;
                        }

                        // If it's not mapped already, we'll need to create the vertex
                        mappedIdx = (short) vertexCount++;
                        vertexIdxMapping[skmVertexIdx] = mappedIdx;
                        primVertIdx[primVertIdxOut++] = mappedIdx;
                        ref var curVertexState = ref VertexState[mappedIdx];

                        // Handle the cloth state
                        if (matchingclothstuff1Idx < ClothStuff1.Count)
                        {
                            var clothState = ClothStuff1[matchingclothstuff1Idx];

                            // Find the current vertex in the cloth vertex list (slow...)
                            for (short i = 0; i < clothState.clothVertexCount; i++)
                            {
                                if (clothState.vertexIdxForClothVertexIdx[i] == skmVertexIdx)
                                {
                                    if (clothState.bytePerClothVertex[i] == 1)
                                    {
                                        ClothVerticesWithFlag[clothVerticesWithFlagCount].cloth_stuff1 =
                                            clothState;
                                        ClothVerticesWithFlag[clothVerticesWithFlagCount]
                                            .submesh_vertex_idx = mappedIdx;
                                        ClothVerticesWithFlag[clothVerticesWithFlagCount]
                                            .cloth_stuff_vertex_idx = i;
                                        clothVerticesWithFlagCount++;
                                    }
                                    else
                                    {
                                        ClothVerticesWithoutFlag[clothVerticesWithoutFlagCount]
                                            .cloth_stuff1 = clothState;
                                        ClothVerticesWithoutFlag[clothVerticesWithoutFlagCount]
                                            .submesh_vertex_idx = mappedIdx;
                                        ClothVerticesWithoutFlag[clothVerticesWithoutFlagCount]
                                            .cloth_stuff_vertex_idx = i;
                                        clothVerticesWithoutFlagCount++;
                                    }
                                }
                            }
                        }

                        // Deduplicate bone attachment ids and weights
                        int vertexBoneAttachCount = 0;

                        for (int attachmentIdx = 0; attachmentIdx < vertex.AttachmentCount; attachmentIdx++)
                        {
                            var attachmentBoneMapping = boneMapping[vertex.GetAttachmentBone(attachmentIdx)];
                            var attachmentSkmBoneIdx =
                                attachmentBoneMapping.skm_bone_idx; // even the SKM bone can be remapped...
                            var attachmentSkaBoneIdx = attachmentBoneMapping.ska_bone_idx;
                            var attachmentWeight = vertex.GetAttachmentWeight(attachmentIdx);

                            // Check if for this particular vertex, the same SKA bone has already been used for an attachment
                            bool found = false;
                            for (int i = 0; i < vertexBoneAttachCount; i++)
                            {
                                if (vertexBoneAttachSkaIds[i] == attachmentSkaBoneIdx)
                                {
                                    vertexBoneAttachWeights[i] += attachmentWeight;
                                    found = true;
                                    break;
                                }
                            }

                            if (found)
                            {
                                // Continue with the next attachment if we were able to merge the current one with an existing attachment
                                continue;
                            }

                            var attachmentIdxOut = vertexBoneAttachCount++;
                            ref var attachmentPosition = ref attachmentPositions[attachmentIdxOut];
                            ref var attachmentNormal = ref attachmentNormals[attachmentIdxOut];

                            vertexBoneAttachSkaIds[attachmentIdxOut] = attachmentSkaBoneIdx;
                            vertexBoneAttachWeights[attachmentIdxOut] = attachmentWeight;

                            // Start out with the actual vertex position in model world space
                            attachmentPosition = vertex.Pos;
                            attachmentPosition.W = 1; // W component for mesh vertices is wrong
                            attachmentNormal = vertex.Normal;

                            // It's possible that the vertex has an assignment, but the assignment's bone is not present in the SKA file,
                            // so the SKM bone also get's mapped to -1 in case no parent bone can be used
                            if (attachmentSkmBoneIdx >= 0)
                            {
                                var skmBone = mesh.Bones[attachmentSkmBoneIdx];

                                // Transform the vertex position into the bone's local vector space
                                attachmentPosition =
                                    Vector4.Transform(attachmentPosition, skmBone.FullWorldInverse);
                                attachmentNormal = Vector4.Transform(attachmentNormal, skmBone.FullWorldInverse);
                            }

                            // NOTE: ToEE computed bounding boxes for each bone here, but it didn't seem to be used
                        }

                        // If the vertex has no bone attachments, simply use the vertex position with a weight of 1.0
                        if (vertexBoneAttachCount == 0)
                        {
                            vertexBoneAttachWeights[0] = 1.0f;
                            vertexBoneAttachSkaIds[0] = -1;
                            attachmentPositions[0] = vertex.Pos;
                            attachmentNormals[0] = vertex.Normal;
                            vertexBoneAttachCount = 1;
                        }

                        // Now convert from the temporary state to the linearized list of vertex positions
                        curVertexState.count = (short) vertexBoneAttachCount;
                        curVertexState.uv = vertex.UV;
                        totalVertexAttachmentCount += vertexBoneAttachCount;
                        for (int i = 0; i < vertexBoneAttachCount; i++)
                        {
                            var weight = vertexBoneAttachWeights[i];
                            var skaBoneIdx = vertexBoneAttachSkaIds[i];
                            var boneMatrixIdx = skaBoneIdx + 1;
                            curVertexState.array1[i] = (short) boneMatrixIdx;

                            Vector4 weightedPos = new Vector4(
                                weight * attachmentPositions[i].X,
                                weight * attachmentPositions[i].Y,
                                weight * attachmentPositions[i].Z,
                                1
                            );
                            curVertexState.array2[i] = (short) AasWorkSet.workset_add(WorkSet,
                                ref worksetCount,
                                weightedPos,
                                weight,
                                ref posPairs[boneMatrixIdx].first_position_idx,
                                ref skaBoneAffectedCount[boneMatrixIdx].pos_count
                            );

                            Vector4 weightedNormal = new Vector4(
                                weight * attachmentNormals[i].X,
                                weight * attachmentNormals[i].Y,
                                weight * attachmentNormals[i].Z,
                                0
                            );
                            curVertexState.array3[i] = (short) AasWorkSet.workset_add(WorkSet,
                                ref worksetCount,
                                weightedNormal,
                                0.0f,
                                ref posPairs[boneMatrixIdx].first_normal_idx,
                                ref skaBoneAffectedCount[boneMatrixIdx].normals_count
                            );
                        }
                    }
                }
            }

            if (HasClothBones)
            {
                submesh.cloth_vertices_without_flag = ClothVerticesWithoutFlag
                    .AsSpan(0, clothVerticesWithoutFlagCount)
                    .ToArray();
                clothVerticesWithoutFlagCount = 0;

                submesh.cloth_vertices_with_flag = ClothVerticesWithFlag
                    .AsSpan(0, clothVerticesWithFlagCount)
                    .ToArray();
                clothVerticesWithFlagCount = 0;
            }

            submesh.vertexCount = (ushort) vertexCount;
            submesh.primCount = (ushort) primitiveCount;
            submesh.bone_elem_counts = new BoneElementCount[usedSkaBoneCount];

            int curFloatOffset = 0;
            int curVecOffset = 0;
            for (int i = 0; i < usedSkaBoneCount; i++)
            {
                ref var affectedCount = ref skaBoneAffectedCount[i];
                var posCount = affectedCount.pos_count;
                var normalsCount = affectedCount.normals_count;
                submesh.bone_elem_counts[i].position_count = (short) posCount;
                submesh.bone_elem_counts[i].normal_count = (short) normalsCount;

                affectedCount.pos_float_start = curFloatOffset;
                affectedCount.pos_vec_start = curVecOffset;
                curFloatOffset += 4 * posCount;
                curVecOffset += posCount;

                affectedCount.normals_float_start = curFloatOffset;
                affectedCount.normals_vec_start = curVecOffset;
                curFloatOffset += 3 * normalsCount;
                curVecOffset += normalsCount;
            }

            submesh.bone_floats_count = (ushort) curFloatOffset;
            submesh.bone_floats = new float[curFloatOffset];

            for (int i = 0; i < usedSkaBoneCount; i++)
            {
                ref var affectedCount = ref skaBoneAffectedCount[i];

                // Copy over all positions to the float buffer
                var scratchIdx = posPairs[i].first_position_idx;
                var floatOut = submesh.bone_floats.AsSpan(affectedCount.pos_float_start);
                var offset = 0;
                while (scratchIdx != -1)
                {
                    floatOut[offset++] = WorkSet[scratchIdx].vector.X;
                    floatOut[offset++] = WorkSet[scratchIdx].vector.Y;
                    floatOut[offset++] = WorkSet[scratchIdx].vector.Z;
                    floatOut[offset++] = WorkSet[scratchIdx].weight;

                    scratchIdx = WorkSet[scratchIdx].next;
                }

                // Copy over all normals to the float buffer
                scratchIdx = posPairs[i].first_normal_idx;
                floatOut = submesh.bone_floats.AsSpan(affectedCount.normals_float_start);

                offset = 0;
                while (scratchIdx != -1)
                {
                    floatOut[offset++] = WorkSet[scratchIdx].vector.X;
                    floatOut[offset++] = WorkSet[scratchIdx].vector.Y;
                    floatOut[offset++] = WorkSet[scratchIdx].vector.Z;

                    scratchIdx = WorkSet[scratchIdx].next;
                }
            }

            submesh.positions = new Vector4[vertexCount];
            submesh.normals = new Vector4[vertexCount];
            submesh.vertex_copy_positions = new short[2 * totalVertexAttachmentCount + 1];
            submesh.uv = new Vector2[vertexCount];

            Span<short> curVertexCopyPosOut = submesh.vertex_copy_positions;
            var idxOffset = 0;
            for (int i = 0; i < vertexCount; i++)
            {
                ref var curVertexState = ref VertexState[i];
                submesh.uv[i].X = curVertexState.uv.X;
                submesh.uv[i].Y = 1.0f - curVertexState.uv.Y;

                for (int j = 0; j < curVertexState.count; j++)
                {
                    // TODO: Is this the "target" position in the positions array?
                    var x = skaBoneAffectedCount[curVertexState.array1[j]].pos_vec_start +
                            curVertexState.array2[j];
                    if (j == 0)
                    {
                        x = -1 - x;
                    }

                    curVertexCopyPosOut[idxOffset++] = (short) x;
                }

                for (int j = 0; j < curVertexState.count; j++)
                {
                    // TODO: Is this the "target" position in the positions array?
                    var x = skaBoneAffectedCount[curVertexState.array1[j]].normals_vec_start +
                            curVertexState.array3[j];
                    if (j == 0)
                    {
                        x = -1 - x;
                    }

                    curVertexCopyPosOut[idxOffset++] = (short) x;
                }
            }

            curVertexCopyPosOut[idxOffset] = short.MinValue;

            // This will actually flip the indices. Weird.
            submesh.indices = new ushort[3 * primitiveCount];
            int indicesIdx = 0;
            for (int i = 0; i < primitiveCount; i++)
            {
                submesh.indices[indicesIdx] = (ushort) primVertIdx[indicesIdx + 2];
                submesh.indices[indicesIdx + 1] = (ushort) primVertIdx[indicesIdx + 1];
                submesh.indices[indicesIdx + 2] = (ushort) primVertIdx[indicesIdx];
                indicesIdx += 3;
            }

            submesh.fullyInitialized = true;
        }

        SubmeshesValid = true;
    }

    public void SetClothFlagSth()
    {
        if (HasClothBones)
        {
            if (!SubmeshesValid)
            {
                UpdateSubmeshes();
            }

            foreach (var submesh in Submeshes)
            {
                for (int i = 0; i < submesh.cloth_vertices_without_flag.Length; i++)
                {
                    ref var vertexClothState = ref submesh.cloth_vertices_without_flag[i];
                    var vertexIdx = vertexClothState.cloth_stuff_vertex_idx;
                    vertexClothState.cloth_stuff1.bytePerClothVertex2[vertexIdx] = 1;
                }
            }
        }
    }

    public List<AasMaterial> GetSubmeshes()
    {
        List<AasMaterial> result = new List<AasMaterial>(Submeshes.Count);

        foreach (var submesh in Submeshes)
        {
            result.Add(submesh.materialId);
        }

        return result;
    }

    public int Method14()
    {
        return -1;
    }

    public bool HasAnimation(ReadOnlySpan<char> animName)
    {
        return Skeleton?.FindAnimByName(new string(animName)) != null;
    }

    public void PlayAnim(int animIdx)
    {
        var player = new AnimPlayer();

        player.Attach(this, animIdx);
        player.Setup2(0.5f);
    }

    public void Advance(in Matrix3x4 worldMatrix, float deltaTime, float deltaDistance, float deltaRotation)
    {
        DrivenTime += deltaTime;
        TimeForClothSim += deltaTime;
        DrivenDistance += deltaDistance;
        DrivenRotation += deltaRotation;
        this.WorldMatrix = worldMatrix;

        var animPlayer = RunningAnimsHead;
        while (animPlayer != null)
        {
            var next = animPlayer.nextRunningAnim;
            animPlayer.FadeInOrOut(deltaTime);
            animPlayer = next;
        }

        var anim = NewestRunningAnim;
        if (anim != null)
        {
            anim.EnterEventHandling();
            anim.AdvanceEvents(deltaTime, deltaDistance, deltaRotation);
            anim.LeaveEventHandling();

            CleanupAnimations(anim);
        }
    }

    public void SetWorldMatrix(in Matrix3x4 worldMatrix)
    {
        this.WorldMatrix = worldMatrix;
    }

    // Originally @ 0x102682a0 (maybe "SetBoneMatrices")
    public void Method19()
    {
        if (DrivenTime <= 0.0f && DrivenDistance <= 0.0f && DrivenRotation <= 0.0f &&
            CurrentWorldMatrix == WorldMatrix)
        {
            return;
        }

        // Build the effective translation/scale/rotation for each bone of the animated skeleton
        Span<SkelBoneState> boneState = stackalloc SkelBoneState[1024];
        var bones = Skeleton?.Bones ?? Array.Empty<SkeletonBone>();

        var runningAnim = RunningAnimsHead;
        if (runningAnim == null || runningAnim.weight != 1.0f || runningAnim.fadingSpeed < 0.0)
        {
            for (int i = 0; i < bones.Count; i++)
            {
                boneState[i] = bones[i].InitialState;
            }
        }

        while (runningAnim != null)
        {
            var nextRunningAnim = runningAnim.nextRunningAnim;

            runningAnim.method6(boneState, DrivenTime, DrivenDistance, DrivenRotation);

            CleanupAnimations(runningAnim);

            runningAnim = nextRunningAnim;
        }

        var scaleMat = Matrix3x4.scaleMatrix(Scale, Scale, Scale);
        BoneMatrices[0] = Matrix3x4.multiplyMatrix3x3_3x4(scaleMat, WorldMatrix);

        for (int i = 0; i < bones.Count; i++)
        {
            var rotation = Matrix3x4.rotationMatrix(boneState[i].Rotation);
            var scale = Matrix3x4.scaleMatrix(boneState[i].Scale.X, boneState[i].Scale.Y, boneState[i].Scale.Z);

            ref var boneMatrix = ref BoneMatrices[1 + i];
            boneMatrix = Matrix3x4.multiplyMatrix3x3(scale, rotation);
            boneMatrix.m03 = 0;
            boneMatrix.m13 = 0;
            boneMatrix.m23 = 0;

            var parentId = bones[i].ParentId;
            if (parentId >= 0)
            {
                var parentScale = boneState[parentId].Scale;
                var parentScaleMat = Matrix3x4.scaleMatrix(1.0f / parentScale.X, 1.0f / parentScale.Y,
                    1.0f / parentScale.Z);
                boneMatrix = Matrix3x4.multiplyMatrix3x4_3x3(boneMatrix, parentScaleMat);
            }

            var translation = Matrix3x4.translationMatrix(boneState[i].Translation.X,
                boneState[i].Translation.Y,
                boneState[i].Translation.Z);
            boneMatrix = Matrix3x4.multiplyMatrix3x4(boneMatrix,
                Matrix3x4.multiplyMatrix3x4(translation, BoneMatrices[1 + parentId]));
        }

        DrivenTime = 0;
        DrivenDistance = 0;
        DrivenRotation = 0;
        CurrentWorldMatrix = WorldMatrix;

        foreach (var submesh in Submeshes)
        {
            submesh.fullyInitialized = true;
        }
    }

    public void SetTime(float time, in Matrix3x4 worldMatrix)
    {
        var player = RunningAnimsHead;
        while (player != null)
        {
            var next = player.nextRunningAnim;
            player.weight = 0;

            if (next == null)
            {
                player.weight = 1.0f;
                player.SetTime(time);
                // epsilon
                Advance(
                    worldMatrix,
                    0.00001f,
                    0,
                    0);
            }

            player = next;
        }
    }

    public float GetCurrentFrame()
    {
        var player = RunningAnimsHead;
        if (player != null)
        {
            while (player.nextRunningAnim != null)
                player = player.nextRunningAnim;
            return player.GetCurrentFrame();
        }
        else
        {
            return 0.0f;
        }
    }

    private static readonly Vector4[] Vectors = new Vector4[0x5FFF6];

    public ISubmesh GetSubmesh(int submeshIdx)
    {
        if (submeshIdx < 0 || submeshIdx >= (int) Submeshes.Count)
        {
            return new SubmeshAdapter(
                0,
                0,
                ReadOnlyMemory<Vector4>.Empty,
                Memory<Vector4>.Empty,
                ReadOnlyMemory<Vector2>.Empty,
                ReadOnlyMemory<ushort>.Empty
            );
        }

        if (!SubmeshesValid)
        {
            UpdateSubmeshes();
        }

        var submesh = Submeshes[submeshIdx];

        Method19();

        if (submesh.fullyInitialized)
        {
            var vecOut = 0;

            // The cur_pos "w" component is (in reality) the attachment weight
            ReadOnlySpan<float> boneFloats = submesh.bone_floats;
            var curFloatIn = 0;
            var bones = Skeleton.Bones;

            for (short boneIdx = 0; boneIdx <= bones.Count; boneIdx++)
            {
                Matrix3x4 boneMatrix = BoneMatrices[boneIdx];

                var elemCounts = submesh.bone_elem_counts[boneIdx];

                for (int i = 0; i < elemCounts.position_count; i++)
                {
                    var x = boneFloats[curFloatIn++];
                    var y = boneFloats[curFloatIn++];
                    var z = boneFloats[curFloatIn++];
                    var weight = boneFloats[curFloatIn++];
                    Vectors[vecOut].X = x * boneMatrix.m00 + y * boneMatrix.m01 + z * boneMatrix.m02 +
                                         weight * boneMatrix.m03;
                    Vectors[vecOut].Y = x * boneMatrix.m10 + y * boneMatrix.m11 + z * boneMatrix.m12 +
                                         weight * boneMatrix.m13;
                    Vectors[vecOut].Z = x * boneMatrix.m20 + y * boneMatrix.m21 + z * boneMatrix.m22 +
                                         weight * boneMatrix.m23;
                    vecOut++;
                }

                for (int i = 0; i < elemCounts.normal_count; i++)
                {
                    var x = boneFloats[curFloatIn++];
                    var y = boneFloats[curFloatIn++];
                    var z = boneFloats[curFloatIn++];
                    Vectors[vecOut].X = x * boneMatrix.m00 + y * boneMatrix.m01 + z * boneMatrix.m02;
                    Vectors[vecOut].Y = x * boneMatrix.m10 + y * boneMatrix.m11 + z * boneMatrix.m12;
                    Vectors[vecOut].Z = x * boneMatrix.m20 + y * boneMatrix.m21 + z * boneMatrix.m22;
                    vecOut++;
                }
            }

            if (submesh.vertexCount > 0)
            {
                Span<Vector4> positionsOut = submesh.positions;
                Span<Vector4> normalsOut = submesh.normals;
                var positionOut = 0;
                var normalOut = 0;
                ReadOnlySpan<short> vertexCopyPositions = submesh.vertex_copy_positions;
                var j = 0;
                var v49 = Vectors[-(vertexCopyPositions[j] + 1)];
                j++;
                while (true)
                {
                    positionsOut[positionOut] = v49;
                    var v51 = vertexCopyPositions[j];
                    for (j = j + 1; v51 >= 0; v51 = vertexCopyPositions[j - 1])
                    {
                        ref var v53 = ref Vectors[v51];
                        ++j;
                        ref var position = ref positionsOut[positionOut];
                        position.X += v53.X;
                        position.Y += v53.Y;
                        position.Z += v53.Z;
                    }

                    normalsOut[normalOut] = Vectors[-(v51 + 1)];
                    var v55 = vertexCopyPositions[j];
                    for (j = j + 1; v55 >= 0; v55 = vertexCopyPositions[j - 1])
                    {
                        ref var v56 = ref Vectors[v55];
                        ++j;
                        ref var normal = ref normalsOut[normalOut];
                        normal.X += v56.X;
                        normal.Y += v56.Y;
                        normal.Z += v56.Z;
                    }

                    if (v55 == -32768)
                        break;
                    ++positionOut;
                    ++normalOut;
                    v49 = Vectors[-(v55 + 1)];
                }
            }

            // Renormalize the normals if necessary
            if (Scale != 1.0f)
            {
                Span<Vector4> normals = submesh.normals;
                for (int i = 0; i < submesh.vertexCount; i++)
                {
                    normals[i] = Vector4.Normalize(normals[i]);
                }
            }

            if (HasClothBones)
            {
                var inverseSomeMatrix = Matrix3x4.invertOrthogonalAffineTransform(CurrentWorldMatrix);

                for (var sphere = CollisionSpheresHead; sphere != null; sphere = sphere.Next)
                {
                    Method19();
                    var boneMatrix = BoneMatrices[sphere.BoneId + 1];
                    Matrix3x4.makeMatrixOrthogonal(ref boneMatrix);

                    var boneMult = Matrix3x4.multiplyMatrix3x4(boneMatrix, inverseSomeMatrix);
                    sphere.WorldMatrix = boneMult;
                    sphere.WorldMatrixInverse = Matrix3x4.invertOrthogonalAffineTransform(boneMult);
                }

                for (var cylinder = CollisionCylindersHead; cylinder != null; cylinder = cylinder.Next)
                {
                    Method19();
                    var boneMatrix = BoneMatrices[cylinder.BoneId + 1];
                    Matrix3x4.makeMatrixOrthogonal(ref boneMatrix);

                    var boneMult = Matrix3x4.multiplyMatrix3x4(boneMatrix, inverseSomeMatrix);
                    cylinder.WorldMatrix = boneMult;
                    cylinder.WorldMatrixInverse = Matrix3x4.invertOrthogonalAffineTransform(boneMult);
                }

                Span<SubmeshVertexClothStateWithFlag> clothStateWithFlag = submesh.cloth_vertices_with_flag;
                foreach (ref var state in clothStateWithFlag)
                {
                    var pos = submesh.positions[state.submesh_vertex_idx];
                    var positions = state.cloth_stuff1.clothStuff.clothVertexPos2.Span;
                    positions[state.cloth_stuff_vertex_idx] = Matrix3x4.transformPosition(inverseSomeMatrix, pos);
                }

                foreach (var stuff1 in ClothStuff1)
                {
                    if (stuff1.field_18 != 0)
                    {
                        stuff1.clothStuff.UpdateBoneDistances();
                        stuff1.field_18 = 0;
                    }
                }

                if (TimeForClothSim > 0.0)
                {
                    foreach (var stuff1 in ClothStuff1)
                    {
                        stuff1.clothStuff.Simulate(TimeForClothSim);
                        //static var aas_cloth_stuff_sim_maybe = temple::GetPointer<void __fastcall(AasClothStuff*, void*, float)>(0x10269d50);
                        //aas_cloth_stuff_sim_maybe(cloth_stuff1[i].clothStuff, 0, timeForClothSim);
                    }

                    TimeForClothSim = 0.0f;
                }

                Matrix4x4.Invert(CurrentWorldMatrix, out var inverseWorldMatrix);

                Span<SubmeshVertexClothStateWithoutFlag> withoutFlag = submesh.cloth_vertices_without_flag;
                foreach (ref var clothVertex in withoutFlag)
                {
                    var clothStuffVertexIdx = clothVertex.cloth_stuff_vertex_idx;
                    var submeshVertexIdx = clothVertex.submesh_vertex_idx;
                    var clothStuff1 = clothVertex.cloth_stuff1;

                    ref var meshPos = ref submesh.positions[submeshVertexIdx];
                    var positions = clothStuff1.clothStuff.clothVertexPos2.Span;
                    ref var clothPos = ref positions[clothStuffVertexIdx];

                    if (clothStuff1.bytePerClothVertex2[clothStuffVertexIdx] == 1)
                    {
                        var pos = new Vector3(meshPos.X, meshPos.Y, meshPos.Z);
                        clothPos = new Vector4(Vector3.Transform(pos, inverseWorldMatrix), 1.0f);

                        clothStuff1.bytePerClothVertex2[clothStuffVertexIdx] = 0;
                        clothStuff1.field_18 = 1;
                    }
                    else
                    {
                        meshPos = Matrix3x4.transformPosition(CurrentWorldMatrix, clothPos);
                    }
                }
            }

            submesh.fullyInitialized = false;
        }

        return new SubmeshAdapter(
            submesh.vertexCount,
            submesh.primCount,
            submesh.positions.AsMemory(0, submesh.vertexCount),
            submesh.normals.AsMemory(0, submesh.vertexCount),
            submesh.uv.AsMemory(0, submesh.vertexCount),
            submesh.indices.AsMemory(0, submesh.primCount * 3)
        );
    }

    public void AddRunningAnim(AnimPlayer player)
    {
        Trace.Assert(Skeleton != null);
        Trace.Assert(player.ownerAnim == null);

        player.ownerAnim = this;

        if (RunningAnimsHead == null)
        {
            player.nextRunningAnim = RunningAnimsHead;
            player.prevRunningAnim = null;
            if (RunningAnimsHead != null)
                RunningAnimsHead.prevRunningAnim = player;
            RunningAnimsHead = player;
            NewestRunningAnim = player;
            return;
        }

        var curAnim = RunningAnimsHead;
        for (var i = curAnim.nextRunningAnim; i != null; i = i.nextRunningAnim)
        {
            curAnim = i;
        }

        var v9 = curAnim.nextRunningAnim;
        player.nextRunningAnim = v9;
        player.prevRunningAnim = curAnim;
        if (v9  != null)
            v9.prevRunningAnim = player;

        curAnim.nextRunningAnim = player;

        var v11 = RunningAnimsHead.nextRunningAnim;
        var runningAnimCount = 1;
        if (v11 == null)
        {
            NewestRunningAnim = player;
            return;
        }

        do
        {
            v11 = v11.nextRunningAnim;
            ++runningAnimCount;
        } while (v11 != null);

        if (runningAnimCount <= 10)
        {
            NewestRunningAnim = player;
            return;
        }

        curAnim = RunningAnimsHead;
        while (runningAnimCount > 1)
        {
            var next = RunningAnimsHead.nextRunningAnim;
            RemoveRunningAnim(curAnim);
            curAnim.Dispose();
            curAnim = next;
            runningAnimCount--;
        }

        NewestRunningAnim = player;
    }

    public void RemoveRunningAnim(AnimPlayer player)
    {
        if (player.ownerAnim == this)
        {
            var prev = player.prevRunningAnim;
            if (prev != null)
            {
                prev.nextRunningAnim = player.nextRunningAnim;
            }
            else
            {
                RunningAnimsHead = player.nextRunningAnim;
            }

            var next = player.nextRunningAnim;
            if (next != null)
            {
                next.prevRunningAnim = player.prevRunningAnim;
            }

            player.prevRunningAnim = null;
            player.nextRunningAnim = null;
            player.ownerAnim = null;
        }
    }

    public int GetAnimCount()
    {
        return Skeleton?.Animations.Count ?? 0;
    }

    public ReadOnlySpan<char> GetAnimName(int animIdx)
    {
        if (Skeleton == null)
        {
            return ReadOnlySpan<char>.Empty;
        }

        var anims = Skeleton.Animations;
        if (animIdx < 0 || animIdx >= anims.Count)
        {
            return ReadOnlySpan<char>.Empty;
        }

        return anims[animIdx].Name;
    }

    public bool HasBone(ReadOnlySpan<char> boneName)
    {
        return Skeleton.FindBoneIdxByName(boneName) != -1;
    }

    public void ReplaceMaterial(AasMaterial oldMaterial, AasMaterial newMaterial)
    {
        foreach (var submesh in Submeshes)
        {
            if (submesh.materialId == oldMaterial)
            {
                // TODO: This is actually dangerous if new_id is already another submesh in this model
                submesh.materialId = newMaterial;
                return;
            }
        }
    }

    public float GetDistPerSec()
    {
        var result = 0.0f;

        var cur = RunningAnimsHead;
        while (cur != null)
        {
            var next = cur.nextRunningAnim;
            cur.GetDistPerSec(ref result);
            cur = next;
        }

        return result;
    }

    public float GetRotationPerSec()
    {
        var result = 0.0f;

        var cur = RunningAnimsHead;
        while (cur != null)
        {
            var next = cur.nextRunningAnim;
            cur.GetRotationPerSec(ref result);
            cur = next;
        }

        return result;
    }

    public void GetBoneMatrix(ReadOnlySpan<char> boneName, out Matrix3x4 matrixOut)
    {
        var boneIdx = Skeleton.FindBoneIdxByName(boneName);

        if (boneIdx != -1)
        {
            Method19();
            matrixOut = BoneMatrices[boneIdx + 1];
        }
        else
        {
            matrixOut = Matrix3x4.Identity;
        }
    }

    // No idea how they arrived at this value
    private const float DefaultHeight = 28.8f;

    public float GetHeight()
    {
        SetClothFlagSth();
        Advance(CurrentWorldMatrix, 0.0f, 0.0f, 0.0f);

        var maxHeight = -10000.0f;
        var minHeight = 10000.0f;

        for (var i = 0; i < Submeshes.Count; i++)
        {
            var submesh = GetSubmesh(i);
            var positions = submesh.Positions;

            for (var j = 0; j < positions.Length; j++)
            {
                var y = positions[j].Y;
                if (y < minHeight)
                {
                    minHeight = y;
                }

                if (y > maxHeight)
                {
                    maxHeight = y;
                }
            }
        }

        if (maxHeight == -10000.0f)
        {
            maxHeight = DefaultHeight;
        }
        else if (maxHeight <= 0)
        {
            maxHeight = maxHeight - minHeight;
            if (maxHeight <= 0.01f)
            {
                maxHeight = DefaultHeight;
            }
        }

        return maxHeight;
    }

    public float GetRadius()
    {
        SetClothFlagSth();
        Advance(CurrentWorldMatrix, 0.0f, 0.0f, 0.0f);

        var maxRadiusSquared = -10000.0f;

        for (var i = 0; i < Submeshes.Count; i++)
        {
            var submesh = GetSubmesh(i);
            var positions = submesh.Positions;

            for (var j = 0; j < submesh.VertexCount; j++)
            {
                var pos = positions[j];

                // Distance from model origin (squared)
                var distSq = pos.X * pos.X + pos.Z * pos.Z;

                if (distSq > maxRadiusSquared)
                {
                    maxRadiusSquared = distSq;
                }
            }
        }

        // No idea how they arrived at this value
        if (maxRadiusSquared <= 0)
        {
            return 0;
        }
        else
        {
            return MathF.Sqrt(maxRadiusSquared);
        }
    }

    public void DeleteSubmesh(int submeshIdx)
    {
        Submeshes[submeshIdx].Dispose();
        Submeshes.RemoveAt(submeshIdx);
    }

    public bool SetAnimByName(ReadOnlySpan<char> name)
    {
        if (Skeleton == null)
        {
            return false;
        }

        var animIdx = Skeleton.FindAnimIdxByName(name);
        if (animIdx != -1)
        {
            PlayAnim(animIdx);
            return true;
        }

        return false;
    }

    public void SetSpecialMaterial(MaterialPlaceholderSlot slot, IMdfRenderMaterial material)
    {
        foreach (var submesh in Submeshes)
        {
            if (submesh.materialId.Slot == slot)
            {
                submesh.materialId.Material = material.Ref();
            }
        }
    }

    public IRenderState? RenderState
    {
        get => _renderState;
        set
        {
            _renderState?.Dispose();
            _renderState = value;
        }
    }

    // Originally @ 10266450
    private AasSubmeshWithMaterial GetOrAddSubmesh(AasMaterial material, IMaterialResolver materialResolver)
    {
        // Check if there's an existing submesh for the material+resolver it came from
        foreach (var submesh in Submeshes)
        {
            if (submesh.materialId == material && submesh.materialResolver == materialResolver)
            {
                return submesh;
            }
        }

        var newSubmesh = new AasSubmeshWithMaterial(material, materialResolver);
        Submeshes.Add(newSubmesh);
        return newSubmesh;
    }

    private void CleanupAnimations(AnimPlayer player)
    {
        // Delete any animation that has ended once it has faded out
        if (player.GetEventHandlingDepth() == 0)
        {
            if (player.weight <= 0.0 && player.fadingSpeed < 0.0)
            {
                player.Dispose();
                return;
            }
        }

        // When any animation that is not currently fading out reaches weight 1,
        // delete any animation that may have ended regardless of whether it has
        // finished fading out
        if (player.weight >= 1.0f && player.fadingSpeed >= 0.0)
        {
            var curAnim = player.prevRunningAnim;
            while (curAnim != null)
            {
                var prev = curAnim.prevRunningAnim;
                if (curAnim.GetEventHandlingDepth() == 0)
                {
                    curAnim.Dispose();
                }

                curAnim = prev;
            }
        }
    }

    public void Dispose()
    {
        _renderState?.Dispose();

        while (RunningAnimsHead != null)
        {
            var runningAnim = RunningAnimsHead;
            if (runningAnim.ownerAnim == this)
            {
                var prevAnim = runningAnim.prevRunningAnim;
                if (prevAnim != null)
                {
                    prevAnim.nextRunningAnim = runningAnim.nextRunningAnim;
                }
                else
                {
                    RunningAnimsHead = runningAnim.nextRunningAnim;
                }

                var nextAnim = runningAnim.nextRunningAnim;
                if (nextAnim != null)
                {
                    nextAnim.prevRunningAnim = runningAnim.prevRunningAnim;
                }

                runningAnim.prevRunningAnim = null;
                runningAnim.nextRunningAnim = null;
                runningAnim.ownerAnim = null;
            }

            runningAnim.Dispose();
        }
    }
}

internal class SubmeshAdapter : ISubmesh
{
    internal SubmeshAdapter(
        int vertexCount,
        int primitiveCount,
        ReadOnlyMemory<Vector4> positions,
        Memory<Vector4> normals,
        ReadOnlyMemory<Vector2> uv,
        ReadOnlyMemory<ushort> indices
    )
    {
        VertexCount = vertexCount;
        PrimitiveCount = primitiveCount;
        _positions = positions;
        _normals = normals;
        _uv = uv;
        _indices = indices;
    }

    private ReadOnlyMemory<Vector4> _positions;
    private Memory<Vector4> _normals;
    private ReadOnlyMemory<Vector2> _uv;
    private ReadOnlyMemory<ushort> _indices;

    public int VertexCount { get; }
    public int PrimitiveCount { get; }
    public ReadOnlySpan<Vector4> Positions => _positions.Span;
    public Span<Vector4> Normals => _normals.Span;
    public ReadOnlySpan<Vector2> UV => _uv.Span;
    public ReadOnlySpan<ushort> Indices => _indices.Span;
}

internal struct SkaBoneAffectedCount
{
    public int pos_count;
    public int normals_count;
    public int pos_float_start;
    public int pos_vec_start;
    public int normals_float_start;
    public int normals_vec_start;
}

internal struct PosPair
{
    public int first_position_idx;
    public int first_normal_idx;

    public PosPair(int firstPositionIdx, int firstNormalIdx)
    {
        first_position_idx = firstPositionIdx;
        first_normal_idx = firstNormalIdx;
    }
};

internal struct BoneElementCount
{
    public short position_count;
    public short normal_count;
};

internal struct SubmeshVertexClothStateWithFlag
{
    public AasClothStuff1 cloth_stuff1;
    public short submesh_vertex_idx;
    public short cloth_stuff_vertex_idx;
};

internal struct SubmeshVertexClothStateWithoutFlag
{
    public AasClothStuff1 cloth_stuff1;
    public short cloth_stuff_vertex_idx;
    public short submesh_vertex_idx;
};

internal struct MeshMaterialPair
{
    public Mesh mesh;
    public int materialIdx;

    public MeshMaterialPair(Mesh mesh, int materialIdx)
    {
        this.mesh = mesh;
        this.materialIdx = materialIdx;
    }
};

class AasSubmeshWithMaterial : IDisposable
{
    public AasMaterial materialId;
    public IMaterialResolver materialResolver;
    public ushort bone_floats_count;
    public ushort vertexCount;
    public ushort primCount;
    public ushort field_E;
    public BoneElementCount[]? bone_elem_counts;
    public float[]? bone_floats;
    public short[]? vertex_copy_positions;
    public Vector2[]? uv;
    public Vector4[]? positions;
    public Vector4[]? normals;
    public bool fullyInitialized; // This could actually be: NEEDS UPDATE! (TODO)
    public SubmeshVertexClothStateWithoutFlag[] cloth_vertices_without_flag = Array.Empty<SubmeshVertexClothStateWithoutFlag>();
    public SubmeshVertexClothStateWithFlag[] cloth_vertices_with_flag = Array.Empty<SubmeshVertexClothStateWithFlag>();
    public ushort[]? indices;

    public AasSubmeshWithMaterial(AasMaterial materialId, IMaterialResolver materialResolver)
    {
        this.materialId = materialId;
        this.materialResolver = materialResolver;
    }

    public int AttachMesh(Mesh mesh, int materialIdx)
    {
        // Check if mesh is already attached
        for (var i = 0; i < meshes.Count; i++)
        {
            if (meshes[i].mesh == mesh && meshes[i].materialIdx == materialIdx)
            {
                return i;
            }
        }

        meshes.Add(new MeshMaterialPair(mesh, materialIdx));
        return meshes.Count - 1;
    }

    public void DetachMesh(Mesh mesh)
    {
        for (int i = meshes.Count - 1; i >= 0; i--)
        {
            if (meshes[i].mesh == mesh)
            {
                meshes.RemoveAt(i);
            }
        }
    }

    public bool HasAttachedMeshes() => meshes.Count > 0;
    public IList<MeshMaterialPair> GetAttachedMeshes() => meshes;

    public void ResetState()
    {
        cloth_vertices_without_flag = Array.Empty<SubmeshVertexClothStateWithoutFlag>();
        cloth_vertices_with_flag = Array.Empty<SubmeshVertexClothStateWithFlag>();
        bone_elem_counts = null;
        bone_floats = null;
        vertex_copy_positions = null;
        positions = null;
        normals = null;
        uv = null;
        indices = null;
    }

    private List<MeshMaterialPair> meshes = new();

    public void Dispose()
    {
        var context = ReadOnlySpan<char>.Empty;
        // Take the context of the first attachment
        if (meshes.Count > 0)
        {
            context = meshes[0].mesh.Path;
        }

        materialResolver.Release(materialId, context);
    }
}

struct BoneMapping
{
    public short skm_bone_idx;
    public short ska_bone_idx;

    /**
        * Originally @ 10265BB0
        * Builds a mapping from the bones of the SKM file to the bones in the SKA file, by comparing their names (ignoring case).
        * If a bone from the SKM cannot be found in the SKA, the mapping of the parent bone is used.
        * Returns the highest index of the SKA bone that was used in the mapping + 1. Presumably to allocate enough space for bone
        * matrices.
        */
    public static int build_bone_mapping(Span<BoneMapping> boneMapping, Skeleton skeleton, Mesh mesh)
    {
        var bones = mesh.Bones;

        var skaBoneCount = 0;

        for (int i = 0; i < bones.Count; i++)
        {
            var bone = bones[i];

            var effectiveSkmBone = -1;
            int skaBoneId = -1;

            var skelBoneIdx = skeleton.FindBoneIdxByName(bone.Name);
            if (skelBoneIdx != -1)
            {
                skaBoneId = skelBoneIdx;
                effectiveSkmBone = i;
            }

            // In case no bone mapping could be found, use the one for the parent
            if (effectiveSkmBone == -1 && bone.ParentId >= 0)
            {
                effectiveSkmBone = boneMapping[bone.ParentId].skm_bone_idx;
                skaBoneId = boneMapping[bone.ParentId].ska_bone_idx;
            }

            if (skaBoneId >= skaBoneCount)
            {
                skaBoneCount = skaBoneId + 1;
            }

            boneMapping[i].skm_bone_idx = (short) effectiveSkmBone;
            boneMapping[i].ska_bone_idx = (short) skaBoneId;
        }

        return skaBoneCount;
    }
}

unsafe struct AasVertexState
{
    public short count;
    public fixed short array1[6];
    public fixed short array2[6];
    public fixed short array3[6];
    public short field26; // padding??
    public Vector2 uv;
};

struct AasWorkSet
{
    public Vector4 vector;
    public float weight;
    public int next;

    public static int workset_add(Span<AasWorkSet> scratch_space, ref int scratch_space_size,
        in Vector4 vector, float bone_weight, ref int heap_pos_out, ref int some_count)
    {
        int insert_pos = 0;

        // Where to actually write the heap_pos
        ref var pos_out_ = ref heap_pos_out;
        if (heap_pos_out >= 0)
        {
            var cur_heap_pos = heap_pos_out;

            while (true)
            {
                ref var cur_entry = ref scratch_space[cur_heap_pos];

                if (distanceSquared3(cur_entry.vector, vector) == 0.0f && bone_weight == cur_entry.weight)
                {
                    return insert_pos; // The position within the linked list of the bone apparently
                }

                cur_heap_pos = cur_entry.next;
                pos_out_ = ref cur_entry.next;
                ++insert_pos;
                if (cur_heap_pos == -1)
                {
                    break; // Didn't find a matching entry in the linked list
                }
            }
        }

        var heap_pos = scratch_space_size;
        pos_out_ = scratch_space_size++;
        ++some_count;
        scratch_space[heap_pos].vector = vector;
        scratch_space[heap_pos].weight = bone_weight;
        scratch_space[heap_pos].next = -1;
        return insert_pos;
    }


    // Distance squared while disregarding w
    private static float distanceSquared3(in Vector4 a, in Vector4 b)
    {
        float deltaX = (a.X - b.X);
        float deltaY = (a.Y - b.Y);
        float deltaZ = (a.Z - b.Z);
        return deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ;
    }
}