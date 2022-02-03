using System;
using System.Numerics;

namespace OpenTemple.Core.AAS;

internal static class EdgeBuilder
{
    static bool FindEdge(ReadOnlySpan<EdgeDistance> edges, short from, short to)
    {
        if (to < from)
        {
            var tmp = from;
            from = to;
            to = tmp;
        }

        for (int i = 0; i < edges.Length; i++)
        {
            if (edges[i].from == from && edges[i].to == to)
            {
                return true;
            }
        }

        return false;
    }

    static void AppendEdge(Span<EdgeDistance> edges, ref int edgesCount, short from, short to)
    {
        if (to < from)
        {
            var tmp = from;
            from = to;
            to = tmp;
        }

        if (FindEdge(edges, from, to))
        {
            return;
        }

        ref var newEdge = ref edges[edgesCount++];
        newEdge.to = to;
        newEdge.from = from;
        newEdge.distSquared = 1.0f;
    }

    public static void Build(ReadOnlySpan<EdgeDistance> edges, Span<EdgeDistance> edges2, ref int edges2Count)
    {
        for (int i = 0; i < edges.Length - 1; i++)
        {
            var firstEdge1 = edges[i].to;
            var firstEdge2 = edges[i].from;

            for (var nextEdgeIdx = i + 1; nextEdgeIdx < edges.Length; nextEdgeIdx++)
            {
                var secondEdge1 = edges[nextEdgeIdx].to;
                var secondEdge2 = edges[nextEdgeIdx].from;

                if (firstEdge1 == secondEdge1)
                {
                    if (FindEdge(edges, secondEdge2, firstEdge2))
                    {
                        continue;
                    }

                    AppendEdge(edges2, ref edges2Count, secondEdge2, firstEdge2);
                }
                else if (firstEdge1 == secondEdge2)
                {
                    if (FindEdge(edges, secondEdge1, firstEdge2))
                    {
                        continue;
                    }

                    AppendEdge(edges2, ref edges2Count, secondEdge1, firstEdge2);
                }
                else if (firstEdge2 == secondEdge1)
                {
                    if (FindEdge(edges, secondEdge2, firstEdge1))
                    {
                        continue;
                    }

                    AppendEdge(edges2, ref edges2Count, secondEdge2, firstEdge1);
                }
                else if (firstEdge2 == secondEdge2)
                {
                    if (FindEdge(edges, secondEdge1, firstEdge1))
                    {
                        continue;
                    }

                    AppendEdge(edges2, ref edges2Count, secondEdge1, firstEdge1);
                }
            }
        }
    }
}

internal class AasClothStuff1 : IDisposable
{
    public Mesh mesh { get; }
    public int clothVertexCount; // Count of vertices found in skm_vertex_idx
    public short[] vertexIdxForClothVertexIdx; // List of the original vertex indices from SKM
    public byte[] bytePerClothVertex;
    public byte[] bytePerClothVertex2;
    public AasClothStuff clothStuff;
    public sbyte field_18;

    private enum ClothSimType : byte
    {
        NONE = 0,
        DIRECT,
        INDIRECT
    };

    private readonly static ClothSimType[] vertex_sim_type = new ClothSimType[0x8000];
    private readonly static int[] mesh_vertex_idx_map = new int[0x8000]; // Map from mesh vertex -> cloth vertex
    private readonly static short[] cloth_vertex_idx_map = new short[0x8000]; // Map from cloth vertex -> skm vertex
    private readonly static Vector4[] cloth_vertex_pos = new Vector4[0x8000];
    private readonly static byte[] byte_per_cloth_vertex = new byte[0x8000];
    private readonly static byte[] byte_per_cloth_vertex2 = new byte[0x8000];
    private readonly static EdgeDistance[] edges = new EdgeDistance[0x8000];
    private readonly static EdgeDistance[] edges2 = new EdgeDistance[0x8000];


    public AasClothStuff1(Mesh mesh,
        int clothBoneId,
        CollisionSphere collilsionSpheresHead,
        CollisionCylinder collisionCylindersHead)
    {
        this.mesh = mesh;


        var cloth_vertex_count = 0;

        var edges_count = 0;

        // Determine all vertices that are in some way influenced by the cloth simulation
        Span<MeshVertex> vertices = mesh.Vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            ref var vertex = ref vertices[i];

            // TODO: Also, the bone mapping should be used here!
            if (vertex.IsAttachedTo(clothBoneId))
            {
                // Establish the bidirectional mapping between the cloth state and the vertex
                mesh_vertex_idx_map[i] = cloth_vertex_count;
                cloth_vertex_idx_map[cloth_vertex_count] = (short) i;
                cloth_vertex_pos[cloth_vertex_count] = vertex.Pos;
                byte_per_cloth_vertex[cloth_vertex_count] = 0;
                byte_per_cloth_vertex2[cloth_vertex_count] = 1;
                cloth_vertex_count++;

                vertex_sim_type[i] = ClothSimType.DIRECT;
            }
            else
            {
                mesh_vertex_idx_map[i] = -1;
                vertex_sim_type[i] = ClothSimType.NONE;
            }
        }

        // Now process faces that incorporate vertices that are part of cloth sim
        foreach (var face in mesh.Faces)
        {
            bool has_cloth_sim = false;
            for (int j = 0; j < MeshFace.VertexCount; j++)
            {
                if (vertex_sim_type[face[j]] == ClothSimType.DIRECT)
                {
                    has_cloth_sim = true;
                    break;
                }
            }

            if (!has_cloth_sim)
            {
                continue; // Skip faces unaffected by cloth simulation
            }

            // For all vertices that are part of the face, but not directly part of cloth-sim
            // we have to record that they are indirectly part of cloth simulation
            for (int j = 0; j < MeshFace.VertexCount; j++)
            {
                var vertex_idx = face[j];
                if (vertex_sim_type[vertex_idx] == ClothSimType.NONE)
                {
                    vertex_sim_type[vertex_idx] = ClothSimType.INDIRECT;
                    mesh_vertex_idx_map[vertex_idx] = cloth_vertex_count;
                    cloth_vertex_idx_map[cloth_vertex_count] = vertex_idx;
                    cloth_vertex_pos[cloth_vertex_count] = vertices[vertex_idx].Pos;
                    byte_per_cloth_vertex[cloth_vertex_count] = 1;
                    byte_per_cloth_vertex2[cloth_vertex_count] = 1;
                    cloth_vertex_count++;
                }
            }

            // Handle the edges of the face (0->1, 1->2, 2->0)
            for (int fromIdx = 0; fromIdx < MeshFace.VertexCount; fromIdx++)
            {
                int toIdx = (fromIdx + 1) % MeshFace.VertexCount;

                var from = face[fromIdx];
                var to = face[toIdx];

                // Only consider edges where either end is directly participating in cloth simulation
                if (vertex_sim_type[from] != ClothSimType.DIRECT
                    && vertex_sim_type[to] != ClothSimType.DIRECT)
                {
                    continue;
                }

                var cloth_state_idx_from = mesh_vertex_idx_map[from];
                var cloth_state_idx_to = mesh_vertex_idx_map[to];

                // Since the edge-state is commutative, we only record it for from<to,
                // and not the other way around
                if (cloth_state_idx_to < cloth_state_idx_from)
                {
                    var tmp = cloth_state_idx_from;
                    cloth_state_idx_from = cloth_state_idx_to;
                    cloth_state_idx_to = tmp;
                }

                // Search if there's a record for the edge already
                bool found = false;
                for (int j = 0; j < edges_count; j++)
                {
                    if (edges[j].from == cloth_state_idx_from && edges[j].to == cloth_state_idx_to)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    var distSq = (vertices[to].Pos - vertices[from].Pos).LengthSquared();

                    ref var newEdge = ref edges[edges_count++];
                    newEdge.from = (short) cloth_state_idx_from;
                    newEdge.to = (short) cloth_state_idx_to;
                    newEdge.distSquared = distSq;
                }
            }
        }

        var edges2_count = 0;
        EdgeBuilder.Build(
            edges.AsSpan(0, edges_count),
            edges2,
            ref edges2_count
        );

        clothStuff = new AasClothStuff(
            cloth_vertex_pos.AsSpan(0, cloth_vertex_count),
            byte_per_cloth_vertex.AsSpan(0, cloth_vertex_count),
            edges.AsSpan(0, edges_count),
            edges2.AsSpan(0, edges2_count),
            collilsionSpheresHead,
            collisionCylindersHead
        );
        clothVertexCount = cloth_vertex_count;
        bytePerClothVertex = byte_per_cloth_vertex.AsSpan(0, cloth_vertex_count).ToArray();
        bytePerClothVertex2 = byte_per_cloth_vertex2.AsSpan(0, cloth_vertex_count).ToArray();
        vertexIdxForClothVertexIdx = cloth_vertex_idx_map.AsSpan(0, cloth_vertex_count).ToArray();
    }

    public void Dispose()
    {
        clothStuff.Dispose();
    }
}