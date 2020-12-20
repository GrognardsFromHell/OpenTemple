using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTemple.Core.IO;

namespace OpenTemple.Core.AAS
{
    public class Mesh
    {
        private static readonly int VertexSize = Marshal.SizeOf<MeshVertex>();

        private static readonly int FaceSize = Marshal.SizeOf<MeshFace>();

        private readonly Memory<byte> _faceData;

        private readonly Memory<byte> _vertexData;

        public Mesh(Memory<byte> data)
        {
            var header = MemoryMarshal.Read<MeshHeader>(data.Span);

            _vertexData = data.Slice(header.VertexDataStart, header.VertexCount * VertexSize);
            _faceData = data.Slice(header.FaceDataStart, header.FaceCount * FaceSize);

            unsafe
            {
                fixed (byte* spanData = data.Span)
                {
                    using (var stream = new UnmanagedMemoryStream(spanData, data.Length))
                    {
                        var reader = new BinaryReader(stream);

                        stream.Seek(header.BoneDataStart, SeekOrigin.Begin);
                        Bones = ReadBones(header.BoneCount, reader);

                        stream.Seek(header.MaterialDataStart, SeekOrigin.Begin);
                        Materials = ReadMaterials(header.MaterialCount, reader);
                    }
                }
            }
        }

        public Mesh(Memory<byte> faceData, Memory<byte> vertexData, List<MeshBone> bones, List<string> materials)
        {
            _faceData = faceData;
            _vertexData = vertexData;
            Bones = bones;
            Materials = materials;
        }

        public List<MeshBone> Bones { get; }

        public List<string> Materials { get; }

        public string Path { get; set; }

        public Span<MeshVertex> Vertices => MemoryMarshal.Cast<byte, MeshVertex>(_vertexData.Span);

        public Span<MeshFace> Faces => MemoryMarshal.Cast<byte, MeshFace>(_faceData.Span);
        
        private static List<string> ReadMaterials(int count, BinaryReader reader)
        {
            var materials = new List<string>(count);

            for (var i = 0; i < count; i++)
            {
                materials.Add(reader.ReadFixedString(128));
            }

            return materials;
        }

        private static List<MeshBone> ReadBones(int boneCount, BinaryReader reader)
        {
            var bones = new List<MeshBone>(boneCount);
            for (var i = 0; i < boneCount; i++)
            {
                var bone = new MeshBone
                {
                    Flags = reader.ReadInt16(),
                    ParentId = reader.ReadInt16(),
                    Name = reader.ReadFixedString(48),
                    FullWorldInverse = ReadMatrix4X3(reader)
                };

                if (bone.ParentId != -1)
                {
                    bone.Parent = bones[bone.ParentId];
                }

                bones.Add(bone);
            }

            return bones;
        }

        /// <summary>
        ///     Reads a matrix consisting of three columns.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static Matrix4x4 ReadMatrix4X3(BinaryReader reader)
        {
            Span<float> m = stackalloc float[12];
            reader.Read(MemoryMarshal.Cast<float, byte>(m));

            return new Matrix4x4(
                m[0], m[4], m[8], 0,
                m[1], m[5], m[9], 0,
                m[2], m[6], m[10], 0,
                m[3], m[7], m[11], 1
            );
        }

        public void RenormalizeClothVertices(int clothBoneId)
        {

            foreach (ref var vertex in Vertices) {

               	var weight_sum = 0.0f;
               	for (int j = 0; j < vertex.AttachmentCount; j++) {
               		// This is actually the "first cloth bone id" i guess...
               		// TODO: This should actually use the bone mapping! The normalBoneCount comes from the
               		// SKA file, while the attachment references SKM bones
               		if (vertex.GetAttachmentBone(j) == clothBoneId)
                    {
                        vertex.GetAttachmentWeight(j) = 0.0f;
                    }
               		else {
               			weight_sum += vertex.GetAttachmentWeight(j);
               		}
               	}

               	// Normalize remaining attachment weights
               	if (weight_sum > 0.0f) {
               		for (int j = 0; j < vertex.AttachmentCount; j++) {
               			vertex.GetAttachmentWeight(j) /= weight_sum;
               		}
               	}
            }

        }
    }

    public unsafe struct MeshVertex
    {
        public Vector4 Pos;
        public Vector4 Normal;
        public Vector2 UV;
        public short Padding;
        public short AttachmentCount;
        public fixed short AttachmentBones[6];
        public fixed float AttachmentWeights[6];

        public ref short GetAttachmentBone(int attachmentIdx)
        {
            return ref AttachmentBones[attachmentIdx];
        }

        public ref float GetAttachmentWeight(int attachmentIdx)
        {
            return ref AttachmentWeights[attachmentIdx];
        }

        public bool IsAttachedTo(int boneIndex)
        {
            for (var i = 0; i < AttachmentCount; i++)
            {
                if (AttachmentBones[i] == boneIndex)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public struct MeshFace : IEnumerable<short>
    {
        public short MaterialIndex;
        public short Vertex1;
        public short Vertex2;
        public short Vertex3;

        public const int VertexCount = 3;

        public IEnumerator<short> GetEnumerator()
        {
            yield return Vertex1;
            yield return Vertex2;
            yield return Vertex3;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public short this[int idx]
        {
            get
            {
                switch (idx)
                {
                    case 0:
                        return Vertex1;
                    case 1:
                        return Vertex2;
                    case 2:
                        return Vertex3;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(idx));
                }
            }
        }
    }

    public class MeshBone
    {
        public short Flags { get; set; }
        public short ParentId { get; set; }
        public MeshBone Parent { get; set; }
        public string Name { get; set; }
        public Matrix4x4 FullWorldInverse { get; set; }
    }

    internal readonly struct MeshHeader
    {
        // Fields will be initialized via MemoryMarshal, so
        // the compiler thinks they're never used
#pragma warning disable 649
        public readonly int BoneCount;
        public readonly int BoneDataStart;
        public readonly int MaterialCount;
        public readonly int MaterialDataStart;
        public readonly int VertexCount;
        public readonly int VertexDataStart;
        public readonly int FaceCount;
        public readonly int FaceDataStart;
#pragma warning restore 649
    }
}