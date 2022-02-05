using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;

namespace OpenTemple.Core.AAS;

internal struct VariationSelector
{
    public int variationId;
    public float factor;
}

internal class AnimatedModel : IDisposable
{
    public AnimPlayer runningAnimsHead = null;
    public AnimPlayer newestRunningAnim = null;
    public bool submeshesValid = false;
    public float scale;
    public float scaleInv;
    public Skeleton skeleton;
    public int variationCount = 0;
    public readonly VariationSelector[] variations = new VariationSelector[8];
    public bool hasClothBones = false;
    public int clothBoneId = 0;
    public List<AasClothStuff1> cloth_stuff1 = new();
    public CollisionSphere collisionSpheresHead;
    public CollisionCylinder collisionCylindersHead;
    public EventHandler EventHandler { get; set; } = new(null);
    public List<AasSubmeshWithMaterial> submeshes = new();
    public float drivenTime = 0.0f;
    public float timeForClothSim = 0.0f;
    public float drivenDistance = 0.0f;
    public float drivenRotation = 0.0f;
    public Matrix3x4 currentWorldMatrix; // Current world matrix
    public Matrix3x4 worldMatrix = Matrix3x4.Identity;
    public Matrix3x4[] boneMatrices;
    public uint field_F8;
    public uint field_FC;

    public void SetSkeleton(Skeleton skeleton)
    {
        Debug.Assert(this.skeleton == null);

        this.skeleton = skeleton;
        scale = 1.0f;
        scaleInv = 1.0f;
        variationCount = 1;
        variations[0].variationId = -1;
        variations[0].factor = 1.0f;

        var bones = skeleton.Bones;
        boneMatrices = new Matrix3x4[bones.Count + 1];

        // count normal bones
        clothBoneId = skeleton.FindBoneIdxByName("#ClothBone");
        if (clothBoneId == -1)
        {
            clothBoneId = bones.Count;
            hasClothBones = false;
        }
        else
        {
            hasClothBones = true;
        }

        if (!hasClothBones)
            return;

        // Discover the collision geometry used for cloth simulation
        var collisionGeom = CollisionGeometry.Find(bones);
        collisionSpheresHead = collisionGeom.firstSphere;
        collisionCylindersHead = collisionGeom.firstCylinder;
    }

    public void SetScale(float scale)
    {
        this.scale = scale;
        if (scale <= 0.0f)
        {
            scaleInv = 0.0f;
        }
        else
        {
            scaleInv = 1.0f / scale;
        }
    }

    public int GetBoneCount()
    {
        return skeleton.Bones.Count;
    }

    public string GetBoneName(int boneIdx)
    {
        return skeleton.Bones[boneIdx].Name;
    }

    public int GetBoneParentId(int boneIdx)
    {
        return skeleton.Bones[boneIdx].ParentId;
    }

    public void AddMesh(Mesh mesh, IMaterialResolver matResolver)
    {
        ResetSubmeshes();

        // If the skeleton has cloth bones, normalize the vertex bone weights
        if (hasClothBones)
        {
            mesh.RenormalizeClothVertices(clothBoneId);
        }

        // Create a material group for every material in the SKM file and attach the SKM file to the group (in case a
        // material is used by multiple attached SKM files)
        var materials = mesh.Materials;
        for (var i = 0; i < materials.Count; i++)
        {
            var matHandle = matResolver.Acquire(materials[i], mesh.Path);

            var submesh = GetOrAddSubmesh(matHandle, matResolver);
            Debug.Assert(submesh != null);

            if (submesh.AttachMesh(mesh, i) < 0)
            {
                throw new AasException(
                    $"Failed to attach mesh '{mesh.Path}' with material index '{i}' to material group.");
            }
        }

        if (!hasClothBones)
        {
            return;
        }

        var cs1 = new AasClothStuff1(mesh, clothBoneId, collisionSpheresHead, collisionCylindersHead);
        cloth_stuff1.Add(cs1);
    }

    public void RemoveMesh(Mesh mesh)
    {
        ResetSubmeshes();

        for (var i = submeshes.Count - 1; i >= 0; i--)
        {
            var submesh = submeshes[i];
            submesh.DetachMesh(mesh);

            // Remove the submesh if it has no attachments left
            if (!submesh.HasAttachedMeshes())
            {
                submeshes.RemoveAt(i);
            }
        }

        if (hasClothBones)
        {
            for (int i = 0; i < cloth_stuff1.Count; i++)
            {
                var clothStuff = cloth_stuff1[i];
                if (clothStuff.mesh == mesh)
                {
                    clothStuff.Dispose();
                    cloth_stuff1.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public void ResetSubmeshes()
    {
        if (submeshesValid)
        {
            foreach (var submesh in submeshes)
            {
                submesh.ResetState();
            }

            submeshesValid = false;
        }
    }

    private static AasVertexState[] vertex_state = new AasVertexState[0x7FFF];
    private static AasWorkSet[] workset = new AasWorkSet[0x47FF7];

    private static SubmeshVertexClothStateWithFlag[] cloth_vertices_with_flag =
        new SubmeshVertexClothStateWithFlag[0x7FFF];

    private static SubmeshVertexClothStateWithoutFlag[] cloth_vertices_without_flag =
        new SubmeshVertexClothStateWithoutFlag[0x7FFF];

    private short[] vertex_idx_mapping = new short[0x7FFF];
    private short[] prim_vert_idx = new short[0xFFFF * 3];

    public unsafe void Method11()
    {
        // Cached bone mappings for SKM file
        Mesh bone_mapping_mesh = null;
        Span<BoneMapping> bone_mapping = stackalloc BoneMapping[1024];
        int used_ska_bone_count = 0; // Built by bone_mapping, may include gaps
        Span<PosPair> pos_pairs = stackalloc PosPair[1024];
        Span<SkaBoneAffectedCount> ska_bone_affected_count = stackalloc SkaBoneAffectedCount[1024];

        // Mapping between index of vertex in SKM file, and index of vertex in submesh vertex array
        Span<short> vertex_idx_mapping = this.vertex_idx_mapping;
        Span<short> prim_vert_idx = this.prim_vert_idx; // 3 vertex indices per face

        int cloth_vertices_with_flag_count = 0;
        int cloth_vertices_without_flag_count = 0;

        foreach (var submesh in submeshes)
        {
            int workset_count = 0;

            int primitive_count = 0; // Primarily used for indexing into prim_vert_idx
            int vertex_count = 0;
            int total_vertex_attachment_count = 0;

            foreach (var attachment in submesh.GetAttachedMeshes())
            {
                var mesh = attachment.mesh;

                // Rebuild the bone maping if we're working on a new SKM file
                // TODO: This is expensive and stupid since pairs of SKM/SKA files are reused all the time
                // and the mapping between their bones is static
                if (mesh != bone_mapping_mesh)
                {
                    used_ska_bone_count = BoneMapping.build_bone_mapping(bone_mapping, skeleton, mesh) + 1;
                    bone_mapping_mesh = mesh;
                }

                // Find the cloth state (?) for the mesh
                var matchingclothstuff1idx = cloth_stuff1.Count;
                if (hasClothBones)
                {
                    for (int i = 0; i < cloth_stuff1.Count; i++)
                    {
                        if (cloth_stuff1[i].mesh == mesh)
                        {
                            matchingclothstuff1idx = i;
                            break;
                        }
                    }
                }

                // Clear bone state
                PosPair pos_pair = new PosPair(-1, -1);
                pos_pairs.Slice(0, used_ska_bone_count).Fill(pos_pair);
                ska_bone_affected_count.Fill(new SkaBoneAffectedCount());

                // Clear vertex idx mapping state
                vertex_idx_mapping.Slice(0, mesh.Vertices.Length).Fill(-1);

                Span<MeshVertex> vertices = mesh.Vertices;

                foreach (ref var face in mesh.Faces)
                {
                    // Skip faces with different materials than this submesh
                    if (face.MaterialIndex != attachment.materialIdx)
                    {
                        continue;
                    }

                    var prim_vert_idx_out = primitive_count++ * 3;

                    Span<short> faceVertices = stackalloc short[3];
                    faceVertices[0] = face.Vertex1;
                    faceVertices[1] = face.Vertex2;
                    faceVertices[2] = face.Vertex3;

                    foreach (var skm_vertex_idx in faceVertices)
                    {
                        ref var vertex = ref vertices[skm_vertex_idx];

                        // Is it mapped for the submesh already, then reuse the existing vertex
                        var mapped_idx = vertex_idx_mapping[skm_vertex_idx];
                        if (mapped_idx != -1)
                        {
                            prim_vert_idx[prim_vert_idx_out++] = mapped_idx;
                            continue;
                        }

                        // If it's not mapped already, we'll need to create the vertex
                        mapped_idx = (short) vertex_count++;
                        vertex_idx_mapping[skm_vertex_idx] = mapped_idx;
                        prim_vert_idx[prim_vert_idx_out++] = mapped_idx;
                        ref var cur_vertex_state = ref vertex_state[mapped_idx];

                        // Handle the cloth state
                        if (matchingclothstuff1idx < cloth_stuff1.Count)
                        {
                            var cloth_state = cloth_stuff1[matchingclothstuff1idx];

                            // Find the current vertex in the cloth vertex list (slow...)
                            for (short i = 0; i < cloth_state.clothVertexCount; i++)
                            {
                                if (cloth_state.vertexIdxForClothVertexIdx[i] == skm_vertex_idx)
                                {
                                    if (cloth_state.bytePerClothVertex[i] == 1)
                                    {
                                        cloth_vertices_with_flag[cloth_vertices_with_flag_count].cloth_stuff1 =
                                            cloth_state;
                                        cloth_vertices_with_flag[cloth_vertices_with_flag_count]
                                            .submesh_vertex_idx = mapped_idx;
                                        cloth_vertices_with_flag[cloth_vertices_with_flag_count]
                                            .cloth_stuff_vertex_idx = i;
                                        cloth_vertices_with_flag_count++;
                                    }
                                    else
                                    {
                                        cloth_vertices_without_flag[cloth_vertices_without_flag_count]
                                            .cloth_stuff1 = cloth_state;
                                        cloth_vertices_without_flag[cloth_vertices_without_flag_count]
                                            .submesh_vertex_idx = mapped_idx;
                                        cloth_vertices_without_flag[cloth_vertices_without_flag_count]
                                            .cloth_stuff_vertex_idx = i;
                                        cloth_vertices_without_flag_count++;
                                    }
                                }
                            }
                        }

                        // Deduplicate bone attachment ids and weights
                        int vertex_bone_attach_count = 0;
                        Span<float> vertex_bone_attach_weights = stackalloc float[6];
                        Span<short>
                            vertex_bone_attach_ska_ids =
                                stackalloc short[6]; // Attached bones mapped to SKA bone ids
                        Span<Vector4> attachment_positions = stackalloc Vector4[6];
                        Span<Vector4> attachment_normals = stackalloc Vector4[6];

                        for (int attachment_idx = 0; attachment_idx < vertex.AttachmentCount; attachment_idx++)
                        {
                            var attachment_bone_mapping = bone_mapping[vertex.GetAttachmentBone(attachment_idx)];
                            var attachment_skm_bone_idx =
                                attachment_bone_mapping.skm_bone_idx; // even the SKM bone can be remapped...
                            var attachment_ska_bone_idx = attachment_bone_mapping.ska_bone_idx;
                            var attachment_weight = vertex.GetAttachmentWeight(attachment_idx);

                            // Check if for this particular vertex, the same SKA bone has already been used for an attachment
                            bool found = false;
                            for (int i = 0; i < vertex_bone_attach_count; i++)
                            {
                                if (vertex_bone_attach_ska_ids[i] == attachment_ska_bone_idx)
                                {
                                    vertex_bone_attach_weights[i] += attachment_weight;
                                    found = true;
                                    break;
                                }
                            }

                            if (found)
                            {
                                // Continue with the next attachment if we were able to merge the current one with an existing attachment
                                continue;
                            }

                            var attachment_idx_out = vertex_bone_attach_count++;
                            ref var attachment_position = ref attachment_positions[attachment_idx_out];
                            ref var attachment_normal = ref attachment_normals[attachment_idx_out];

                            vertex_bone_attach_ska_ids[attachment_idx_out] = attachment_ska_bone_idx;
                            vertex_bone_attach_weights[attachment_idx_out] = attachment_weight;

                            // Start out with the actual vertex position in model world space
                            attachment_position = vertex.Pos;
                            attachment_position.W = 1; // W component for mesh vertices is wrong
                            attachment_normal = vertex.Normal;

                            // It's possible that the vertex has an assignment, but the assignment's bone is not present in the SKA file,
                            // so the SKM bone also get's mapped to -1 in case no parent bone can be used
                            if (attachment_skm_bone_idx >= 0)
                            {
                                var skm_bone = mesh.Bones[attachment_skm_bone_idx];

                                // Transform the vertex position into the bone's local vector space
                                attachment_position =
                                    Vector4.Transform(attachment_position, skm_bone.FullWorldInverse);
                                attachment_normal = Vector4.Transform(attachment_normal, skm_bone.FullWorldInverse);
                            }

                            // NOTE: ToEE computed bounding boxes for each bone here, but it didn't seem to be used
                        }

                        // If the vertex has no bone attachments, simply use the vertex position with a weight of 1.0
                        if (vertex_bone_attach_count == 0)
                        {
                            vertex_bone_attach_weights[0] = 1.0f;
                            vertex_bone_attach_ska_ids[0] = -1;
                            attachment_positions[0] = vertex.Pos;
                            attachment_normals[0] = vertex.Normal;
                            vertex_bone_attach_count = 1;
                        }

                        // Now convert from the temporary state to the linearized list of vertex positions
                        cur_vertex_state.count = (short) vertex_bone_attach_count;
                        cur_vertex_state.uv = vertex.UV;
                        total_vertex_attachment_count += vertex_bone_attach_count;
                        for (int i = 0; i < vertex_bone_attach_count; i++)
                        {
                            var weight = vertex_bone_attach_weights[i];
                            var ska_bone_idx = vertex_bone_attach_ska_ids[i];
                            var bone_matrix_idx = ska_bone_idx + 1;
                            cur_vertex_state.array1[i] = (short) bone_matrix_idx;

                            Vector4 weighted_pos = new Vector4(
                                weight * attachment_positions[i].X,
                                weight * attachment_positions[i].Y,
                                weight * attachment_positions[i].Z,
                                1
                            );
                            cur_vertex_state.array2[i] = (short) AasWorkSet.workset_add(workset,
                                ref workset_count,
                                weighted_pos,
                                weight,
                                ref pos_pairs[bone_matrix_idx].first_position_idx,
                                ref ska_bone_affected_count[bone_matrix_idx].pos_count
                            );

                            Vector4 weighted_normal = new Vector4(
                                weight * attachment_normals[i].X,
                                weight * attachment_normals[i].Y,
                                weight * attachment_normals[i].Z,
                                0
                            );
                            cur_vertex_state.array3[i] = (short) AasWorkSet.workset_add(workset,
                                ref workset_count,
                                weighted_normal,
                                0.0f,
                                ref pos_pairs[bone_matrix_idx].first_normal_idx,
                                ref ska_bone_affected_count[bone_matrix_idx].normals_count
                            );
                        }
                    }
                }
            }

            if (hasClothBones)
            {
                submesh.cloth_vertices_without_flag = cloth_vertices_without_flag
                    .AsSpan(0, cloth_vertices_without_flag_count)
                    .ToArray();
                cloth_vertices_without_flag_count = 0;

                submesh.cloth_vertices_with_flag = cloth_vertices_with_flag
                    .AsSpan(0, cloth_vertices_with_flag_count)
                    .ToArray();
                cloth_vertices_with_flag_count = 0;
            }

            submesh.vertexCount = (ushort) vertex_count;
            submesh.primCount = (ushort) primitive_count;
            submesh.bone_elem_counts = new BoneElementCount[used_ska_bone_count];

            int cur_float_offset = 0;
            int cur_vec_offset = 0;
            for (int i = 0; i < used_ska_bone_count; i++)
            {
                ref var affected_count = ref ska_bone_affected_count[i];
                var pos_count = affected_count.pos_count;
                var normals_count = affected_count.normals_count;
                submesh.bone_elem_counts[i].position_count = (short) pos_count;
                submesh.bone_elem_counts[i].normal_count = (short) normals_count;

                affected_count.pos_float_start = cur_float_offset;
                affected_count.pos_vec_start = cur_vec_offset;
                cur_float_offset += 4 * pos_count;
                cur_vec_offset += pos_count;

                affected_count.normals_float_start = cur_float_offset;
                affected_count.normals_vec_start = cur_vec_offset;
                cur_float_offset += 3 * normals_count;
                cur_vec_offset += normals_count;
            }

            submesh.bone_floats_count = (ushort) cur_float_offset;
            submesh.bone_floats = new float[cur_float_offset];

            for (int i = 0; i < used_ska_bone_count; i++)
            {
                ref var affected_count = ref ska_bone_affected_count[i];

                // Copy over all positions to the float buffer
                var scratch_idx = pos_pairs[i].first_position_idx;
                var float_out = submesh.bone_floats.AsSpan(affected_count.pos_float_start);
                var offset = 0;
                while (scratch_idx != -1)
                {
                    float_out[offset++] = workset[scratch_idx].vector.X;
                    float_out[offset++] = workset[scratch_idx].vector.Y;
                    float_out[offset++] = workset[scratch_idx].vector.Z;
                    float_out[offset++] = workset[scratch_idx].weight;

                    scratch_idx = workset[scratch_idx].next;
                }

                // Copy over all normals to the float buffer
                scratch_idx = pos_pairs[i].first_normal_idx;
                float_out = submesh.bone_floats.AsSpan(affected_count.normals_float_start);

                offset = 0;
                while (scratch_idx != -1)
                {
                    float_out[offset++] = workset[scratch_idx].vector.X;
                    float_out[offset++] = workset[scratch_idx].vector.Y;
                    float_out[offset++] = workset[scratch_idx].vector.Z;

                    scratch_idx = workset[scratch_idx].next;
                }
            }

            submesh.positions = new Vector4[vertex_count];
            submesh.normals = new Vector4[vertex_count];
            submesh.vertex_copy_positions = new short[2 * total_vertex_attachment_count + 1];
            submesh.uv = new Vector2[vertex_count];

            Span<short> cur_vertex_copy_pos_out = submesh.vertex_copy_positions;
            var idxOffset = 0;
            for (int i = 0; i < vertex_count; i++)
            {
                ref var cur_vertex_state = ref vertex_state[i];
                submesh.uv[i].X = cur_vertex_state.uv.X;
                submesh.uv[i].Y = 1.0f - cur_vertex_state.uv.Y;

                for (int j = 0; j < cur_vertex_state.count; j++)
                {
                    // TODO: Is this the "target" position in the positions array?
                    var x = ska_bone_affected_count[cur_vertex_state.array1[j]].pos_vec_start +
                            cur_vertex_state.array2[j];
                    if (j == 0)
                    {
                        x = -1 - x;
                    }

                    cur_vertex_copy_pos_out[idxOffset++] = (short) x;
                }

                for (int j = 0; j < cur_vertex_state.count; j++)
                {
                    // TODO: Is this the "target" position in the positions array?
                    var x = ska_bone_affected_count[cur_vertex_state.array1[j]].normals_vec_start +
                            cur_vertex_state.array3[j];
                    if (j == 0)
                    {
                        x = -1 - x;
                    }

                    cur_vertex_copy_pos_out[idxOffset++] = (short) x;
                }
            }

            cur_vertex_copy_pos_out[idxOffset] = short.MinValue;

            // This will actually flip the indices. Weird.
            submesh.indices = new ushort[3 * primitive_count];
            int indices_idx = 0;
            for (int i = 0; i < primitive_count; i++)
            {
                submesh.indices[indices_idx] = (ushort) prim_vert_idx[indices_idx + 2];
                submesh.indices[indices_idx + 1] = (ushort) prim_vert_idx[indices_idx + 1];
                submesh.indices[indices_idx + 2] = (ushort) prim_vert_idx[indices_idx];
                indices_idx += 3;
            }

            submesh.fullyInitialized = true;
        }

        submeshesValid = true;
    }

    public void SetClothFlagSth()
    {
        if (hasClothBones)
        {
            if (!submeshesValid)
            {
                Method11();
            }

            foreach (var submesh in submeshes)
            {
                for (int i = 0; i < submesh.cloth_vertices_without_flag.Length; i++)
                {
                    ref var vertex_cloth_state = ref submesh.cloth_vertices_without_flag[i];
                    var vertex_idx = vertex_cloth_state.cloth_stuff_vertex_idx;
                    vertex_cloth_state.cloth_stuff1.bytePerClothVertex2[vertex_idx] = 1;
                }
            }
        }
    }

    public List<AasMaterial> GetSubmeshes()
    {
        List<AasMaterial> result = new List<AasMaterial>(submeshes.Count);

        foreach (var submesh in submeshes)
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
        return skeleton?.FindAnimByName(new string(animName)) != null;
    }

    public void PlayAnim(int animIdx)
    {
        var player = new AnimPlayer();

        player.Attach(this, animIdx);
        player.Setup2(0.5f);
    }

    public void Advance(in Matrix3x4 worldMatrix, float deltaTime, float deltaDistance, float deltaRotation)
    {
        drivenTime += deltaTime;
        timeForClothSim += deltaTime;
        drivenDistance += deltaDistance;
        drivenRotation += deltaRotation;
        this.worldMatrix = worldMatrix;

        var anim_player = runningAnimsHead;
        while (anim_player != null)
        {
            var next = anim_player.nextRunningAnim;
            anim_player.FadeInOrOut(deltaTime);
            anim_player = next;
        }

        var anim = newestRunningAnim;
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
        this.worldMatrix = worldMatrix;
    }

    // Originally @ 0x102682a0 (maybe "SetBoneMatrices")
    public void Method19()
    {
        if (drivenTime <= 0.0f && drivenDistance <= 0.0f && drivenRotation <= 0.0f &&
            currentWorldMatrix == worldMatrix)
        {
            return;
        }

        // Build the effective translation/scale/rotation for each bone of the animated skeleton
        Span<SkelBoneState> boneState = stackalloc SkelBoneState[1024];
        var bones = skeleton.Bones;

        var running_anim = runningAnimsHead;
        if (running_anim == null || running_anim.weight != 1.0f || running_anim.fadingSpeed < 0.0)
        {
            for (int i = 0; i < bones.Count; i++)
            {
                boneState[i] = bones[i].InitialState;
            }
        }

        while (running_anim != null)
        {
            var next_running_anim = running_anim.nextRunningAnim;

            running_anim.method6(boneState, drivenTime, drivenDistance, drivenRotation);

            CleanupAnimations(running_anim);

            running_anim = next_running_anim;
        }

        var scaleMat = Matrix3x4.scaleMatrix(scale, scale, scale);
        boneMatrices[0] = Matrix3x4.multiplyMatrix3x3_3x4(scaleMat, worldMatrix);

        for (int i = 0; i < bones.Count; i++)
        {
            var rotation = Matrix3x4.rotationMatrix(boneState[i].Rotation);
            var scale = Matrix3x4.scaleMatrix(boneState[i].Scale.X, boneState[i].Scale.Y, boneState[i].Scale.Z);

            ref var boneMatrix = ref boneMatrices[1 + i];
            boneMatrix = Matrix3x4.multiplyMatrix3x3(scale, rotation);
            boneMatrix.m03 = 0;
            boneMatrix.m13 = 0;
            boneMatrix.m23 = 0;

            var parentId = bones[i].ParentId;
            if (parentId >= 0)
            {
                var parent_scale = boneState[parentId].Scale;
                var parent_scale_mat = Matrix3x4.scaleMatrix(1.0f / parent_scale.X, 1.0f / parent_scale.Y,
                    1.0f / parent_scale.Z);
                boneMatrix = Matrix3x4.multiplyMatrix3x4_3x3(boneMatrix, parent_scale_mat);
            }

            var translation = Matrix3x4.translationMatrix(boneState[i].Translation.X,
                boneState[i].Translation.Y,
                boneState[i].Translation.Z);
            boneMatrix = Matrix3x4.multiplyMatrix3x4(boneMatrix,
                Matrix3x4.multiplyMatrix3x4(translation, boneMatrices[1 + parentId]));
        }

        drivenTime = 0;
        drivenDistance = 0;
        drivenRotation = 0;
        currentWorldMatrix = worldMatrix;

        foreach (var submesh in submeshes)
        {
            submesh.fullyInitialized = true;
        }
    }

    public void SetTime(float time, in Matrix3x4 worldMatrix)
    {
        var player = runningAnimsHead;
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
        var player = runningAnimsHead;
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

    private static readonly Vector4[] vectors = new Vector4[0x5FFF6];

    public ISubmesh GetSubmesh(int submeshIdx)
    {
        if (submeshIdx < 0 || submeshIdx >= (int) submeshes.Count)
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

        if (!submeshesValid)
        {
            Method11();
        }

        var submesh = submeshes[submeshIdx];

        Method19();

        if (submesh.fullyInitialized)
        {
            var vec_out = 0;

            // The cur_pos "w" component is (in reality) the attachment weight
            ReadOnlySpan<float> bone_floats = submesh.bone_floats;
            var cur_float_in = 0;
            var bones = skeleton.Bones;

            for (short bone_idx = 0; bone_idx <= bones.Count; bone_idx++)
            {
                Matrix3x4 bone_matrix = boneMatrices[bone_idx];

                var elem_counts = submesh.bone_elem_counts[bone_idx];

                for (int i = 0; i < elem_counts.position_count; i++)
                {
                    var x = bone_floats[cur_float_in++];
                    var y = bone_floats[cur_float_in++];
                    var z = bone_floats[cur_float_in++];
                    var weight = bone_floats[cur_float_in++];
                    vectors[vec_out].X = x * bone_matrix.m00 + y * bone_matrix.m01 + z * bone_matrix.m02 +
                                         weight * bone_matrix.m03;
                    vectors[vec_out].Y = x * bone_matrix.m10 + y * bone_matrix.m11 + z * bone_matrix.m12 +
                                         weight * bone_matrix.m13;
                    vectors[vec_out].Z = x * bone_matrix.m20 + y * bone_matrix.m21 + z * bone_matrix.m22 +
                                         weight * bone_matrix.m23;
                    vec_out++;
                }

                for (int i = 0; i < elem_counts.normal_count; i++)
                {
                    var x = bone_floats[cur_float_in++];
                    var y = bone_floats[cur_float_in++];
                    var z = bone_floats[cur_float_in++];
                    vectors[vec_out].X = x * bone_matrix.m00 + y * bone_matrix.m01 + z * bone_matrix.m02;
                    vectors[vec_out].Y = x * bone_matrix.m10 + y * bone_matrix.m11 + z * bone_matrix.m12;
                    vectors[vec_out].Z = x * bone_matrix.m20 + y * bone_matrix.m21 + z * bone_matrix.m22;
                    vec_out++;
                }
            }

            if (submesh.vertexCount > 0)
            {
                Span<Vector4> positions_out = submesh.positions;
                Span<Vector4> normals_out = submesh.normals;
                var position_out = 0;
                var normal_out = 0;
                ReadOnlySpan<short> vertex_copy_positions = submesh.vertex_copy_positions;
                var j = 0;
                var v49 = vectors[-(vertex_copy_positions[j] + 1)];
                j++;
                while (true)
                {
                    positions_out[position_out] = v49;
                    var v51 = vertex_copy_positions[j];
                    for (j = j + 1; v51 >= 0; v51 = vertex_copy_positions[j - 1])
                    {
                        ref var v53 = ref vectors[v51];
                        ++j;
                        ref var position = ref positions_out[position_out];
                        position.X += v53.X;
                        position.Y += v53.Y;
                        position.Z += v53.Z;
                    }

                    normals_out[normal_out] = vectors[-(v51 + 1)];
                    var v55 = vertex_copy_positions[j];
                    for (j = j + 1; v55 >= 0; v55 = vertex_copy_positions[j - 1])
                    {
                        ref var v56 = ref vectors[v55];
                        ++j;
                        ref var normal = ref normals_out[normal_out];
                        normal.X += v56.X;
                        normal.Y += v56.Y;
                        normal.Z += v56.Z;
                    }

                    if (v55 == -32768)
                        break;
                    ++position_out;
                    ++normal_out;
                    v49 = vectors[-(v55 + 1)];
                }
            }

            // Renormalize the normals if necessary
            if (scale != 1.0f)
            {
                Span<Vector4> normals = submesh.normals;
                for (int i = 0; i < submesh.vertexCount; i++)
                {
                    normals[i] = Vector4.Normalize(normals[i]);
                }
            }

            if (hasClothBones)
            {
                var inverseSomeMatrix = Matrix3x4.invertOrthogonalAffineTransform(currentWorldMatrix);

                for (var sphere = collisionSpheresHead; sphere != null; sphere = sphere.next)
                {
                    Method19();
                    var boneMatrix = boneMatrices[sphere.boneId + 1];
                    Matrix3x4.makeMatrixOrthogonal(ref boneMatrix);

                    var boneMult = Matrix3x4.multiplyMatrix3x4(boneMatrix, inverseSomeMatrix);
                    sphere.worldMatrix = boneMult;
                    sphere.worldMatrixInverse = Matrix3x4.invertOrthogonalAffineTransform(boneMult);
                }

                for (var cylinder = collisionCylindersHead; cylinder != null; cylinder = cylinder.next)
                {
                    Method19();
                    var boneMatrix = boneMatrices[cylinder.boneId + 1];
                    Matrix3x4.makeMatrixOrthogonal(ref boneMatrix);

                    var boneMult = Matrix3x4.multiplyMatrix3x4(boneMatrix, inverseSomeMatrix);
                    cylinder.worldMatrix = boneMult;
                    cylinder.worldMatrixInverse = Matrix3x4.invertOrthogonalAffineTransform(boneMult);
                }

                Span<SubmeshVertexClothStateWithFlag> clothStateWithFlag = submesh.cloth_vertices_with_flag;
                foreach (ref var state in clothStateWithFlag)
                {
                    var pos = submesh.positions[state.submesh_vertex_idx];
                    var positions = state.cloth_stuff1.clothStuff.clothVertexPos2.Span;
                    positions[state.cloth_stuff_vertex_idx] = Matrix3x4.transformPosition(inverseSomeMatrix, pos);
                }

                foreach (var stuff1 in cloth_stuff1)
                {
                    if (stuff1.field_18 != 0)
                    {
                        stuff1.clothStuff.UpdateBoneDistances();
                        stuff1.field_18 = 0;
                    }
                }

                if (timeForClothSim > 0.0)
                {
                    foreach (var stuff1 in cloth_stuff1)
                    {
                        stuff1.clothStuff.Simulate(timeForClothSim);
                        //static var aas_cloth_stuff_sim_maybe = temple::GetPointer<void __fastcall(AasClothStuff*, void*, float)>(0x10269d50);
                        //aas_cloth_stuff_sim_maybe(cloth_stuff1[i].clothStuff, 0, timeForClothSim);
                    }

                    timeForClothSim = 0.0f;
                }

                Matrix4x4.Invert(currentWorldMatrix, out var inverseWorldMatrix);

                Span<SubmeshVertexClothStateWithoutFlag> withoutFlag = submesh.cloth_vertices_without_flag;
                foreach (ref var cloth_vertex in withoutFlag)
                {
                    var cloth_stuff_vertex_idx = cloth_vertex.cloth_stuff_vertex_idx;
                    var submesh_vertex_idx = cloth_vertex.submesh_vertex_idx;
                    var cloth_stuff1 = cloth_vertex.cloth_stuff1;

                    ref var mesh_pos = ref submesh.positions[submesh_vertex_idx];
                    var positions = cloth_stuff1.clothStuff.clothVertexPos2.Span;
                    ref var cloth_pos = ref positions[cloth_stuff_vertex_idx];

                    if (cloth_stuff1.bytePerClothVertex2[cloth_stuff_vertex_idx] == 1)
                    {
                        var pos = new Vector3(mesh_pos.X, mesh_pos.Y, mesh_pos.Z);
                        cloth_pos = new Vector4(Vector3.Transform(pos, inverseWorldMatrix), 1.0f);

                        cloth_stuff1.bytePerClothVertex2[cloth_stuff_vertex_idx] = 0;
                        cloth_stuff1.field_18 = 1;
                    }
                    else
                    {
                        mesh_pos = Matrix3x4.transformPosition(currentWorldMatrix, cloth_pos);
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
        Trace.Assert(skeleton != null);
        Trace.Assert(player.ownerAnim == null);

        player.ownerAnim = this;

        if (runningAnimsHead == null)
        {
            player.nextRunningAnim = runningAnimsHead;
            player.prevRunningAnim = null;
            if (runningAnimsHead != null)
                runningAnimsHead.prevRunningAnim = player;
            runningAnimsHead = player;
            newestRunningAnim = player;
            return;
        }

        var cur_anim = runningAnimsHead;
        for (var i = cur_anim.nextRunningAnim; i != null; i = i.nextRunningAnim)
        {
            cur_anim = i;
        }

        var v9 = cur_anim.nextRunningAnim;
        player.nextRunningAnim = v9;
        player.prevRunningAnim = cur_anim;
        if (v9  != null)
            v9.prevRunningAnim = player;

        cur_anim.nextRunningAnim = player;

        var v11 = runningAnimsHead.nextRunningAnim;
        var runningAnimCount = 1;
        if (v11 == null)
        {
            newestRunningAnim = player;
            return;
        }

        do
        {
            v11 = v11.nextRunningAnim;
            ++runningAnimCount;
        } while (v11 != null);

        if (runningAnimCount <= 10)
        {
            newestRunningAnim = player;
            return;
        }

        cur_anim = runningAnimsHead;
        while (runningAnimCount > 1)
        {
            var next = runningAnimsHead.nextRunningAnim;
            RemoveRunningAnim(cur_anim);
            cur_anim.Dispose();
            cur_anim = next;
            runningAnimCount--;
        }

        newestRunningAnim = player;
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
                runningAnimsHead = player.nextRunningAnim;
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
        return skeleton?.Animations.Count ?? 0;
    }

    public ReadOnlySpan<char> GetAnimName(int animIdx)
    {
        if (skeleton == null)
        {
            return ReadOnlySpan<char>.Empty;
        }

        var anims = skeleton.Animations;
        if (animIdx < 0 || animIdx >= anims.Count)
        {
            return ReadOnlySpan<char>.Empty;
        }

        return anims[animIdx].Name;
    }

    public bool HasBone(ReadOnlySpan<char> boneName)
    {
        return skeleton.FindBoneIdxByName(boneName) != -1;
    }

    public void ReplaceMaterial(AasMaterial oldMaterial, AasMaterial newMaterial)
    {
        foreach (var submesh in submeshes)
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

        var cur = runningAnimsHead;
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

        var cur = runningAnimsHead;
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
        var boneIdx = skeleton.FindBoneIdxByName(boneName);

        if (boneIdx != -1)
        {
            Method19();
            matrixOut = boneMatrices[boneIdx + 1];
        }
        else
        {
            matrixOut = Matrix3x4.Identity;
        }
    }

    // No idea how they arrived at this value
    private const float defaultHeight = 28.8f;

    public float GetHeight()
    {
        SetClothFlagSth();
        Advance(currentWorldMatrix, 0.0f, 0.0f, 0.0f);

        var maxHeight = -10000.0f;
        var minHeight = 10000.0f;

        for (var i = 0; i < submeshes.Count; i++)
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
            maxHeight = defaultHeight;
        }
        else if (maxHeight <= 0)
        {
            maxHeight = maxHeight - minHeight;
            if (maxHeight <= 0.01f)
            {
                maxHeight = defaultHeight;
            }
        }

        return maxHeight;
    }

    public float GetRadius()
    {
        SetClothFlagSth();
        Advance(currentWorldMatrix, 0.0f, 0.0f, 0.0f);

        var maxRadiusSquared = -10000.0f;

        for (var i = 0; i < submeshes.Count; i++)
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
        submeshes[submeshIdx].Dispose();
        submeshes.RemoveAt(submeshIdx);
    }

    public bool SetAnimByName(ReadOnlySpan<char> name)
    {
        if (skeleton == null)
        {
            return false;
        }

        var animIdx = skeleton.FindAnimIdxByName(name);
        if (animIdx != -1)
        {
            PlayAnim(animIdx);
            return true;
        }

        return false;
    }

    public void SetSpecialMaterial(MaterialPlaceholderSlot slot, IMdfRenderMaterial material)
    {
        foreach (var submesh in submeshes)
        {
            if (submesh.materialId.Slot == slot)
            {
                submesh.materialId.Material = material.Ref();
            }
        }
    }

    public IRenderState RenderState
    {
        get => renderState_;
        set
        {
            renderState_?.Dispose();
            renderState_ = value;
        }
    }

    // Originally @ 10266450
    private AasSubmeshWithMaterial GetOrAddSubmesh(AasMaterial material, IMaterialResolver materialResolver)
    {
        // Check if there's an existing submesh for the material+resolver it came from
        foreach (var submesh in submeshes)
        {
            if (submesh.materialId == material && submesh.materialResolver == materialResolver)
            {
                return submesh;
            }
        }

        var newSubmesh = new AasSubmeshWithMaterial(material, materialResolver);
        submeshes.Add(newSubmesh);
        return newSubmesh;
    }

    private IRenderState renderState_;

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
        renderState_?.Dispose();

        while (runningAnimsHead != null)
        {
            var running_anim = runningAnimsHead;
            if (running_anim.ownerAnim == this)
            {
                var prev_anim = running_anim.prevRunningAnim;
                if (prev_anim != null)
                {
                    prev_anim.nextRunningAnim = running_anim.nextRunningAnim;
                }
                else
                {
                    runningAnimsHead = running_anim.nextRunningAnim;
                }

                var next_anim = running_anim.nextRunningAnim;
                if (next_anim != null)
                {
                    next_anim.prevRunningAnim = running_anim.prevRunningAnim;
                }

                running_anim.prevRunningAnim = null;
                running_anim.nextRunningAnim = null;
                running_anim.ownerAnim = null;
            }

            running_anim.Dispose();
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
    public BoneElementCount[] bone_elem_counts;
    public float[] bone_floats;
    public short[] vertex_copy_positions;
    public Vector2[] uv;
    public Vector4[] positions;
    public Vector4[] normals;
    public bool fullyInitialized; // This could actually be: NEEDS UPDATE! (TODO)
    public SubmeshVertexClothStateWithoutFlag[] cloth_vertices_without_flag;
    public SubmeshVertexClothStateWithFlag[] cloth_vertices_with_flag;
    public ushort[] indices;

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
        cloth_vertices_without_flag = null;
        cloth_vertices_with_flag = null;
        bone_elem_counts = null;
        bone_floats = null;
        vertex_copy_positions = null;
        positions = null;
        normals = null;
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