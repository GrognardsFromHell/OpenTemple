using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;

namespace OpenTemple.Core.AAS;

public class Skeleton
{
    private readonly Dictionary<string, SkeletonAnimation> _animationsByName;

    public Skeleton(ReadOnlyMemory<byte> memory)
    {
        using (var stream = CreateStream(memory))
        {
            var reader = new BinaryReader(stream);

            var boneCount = reader.ReadInt32();
            var boneDataStart = reader.ReadInt32();
            reader.ReadInt32(); // Formerly variationCount
            reader.ReadInt32(); // Formerly variationDataStart
            var animationCount = reader.ReadInt32();
            var animationDataStart = reader.ReadInt32();

            stream.Seek(boneDataStart, SeekOrigin.Begin);
            var bones = new List<SkeletonBone>(boneCount);
            for (var i = 0; i < boneCount; i++)
            {
                var flags = reader.ReadInt16();
                var parentId = reader.ReadInt16();
                var name = reader.ReadFixedString(48);
                var scale = reader.ReadVector4();
                var rotation = reader.ReadVector4();
                var translation = reader.ReadVector4();

                var bone = new SkeletonBone
                {
                    Flags = flags,
                    ParentId = parentId,
                    Name = name,
                    InitialState = new SkelBoneState
                    {
                        Scale = new Vector3(scale.X, scale.Y, scale.Z),
                        Rotation = new Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W),
                        Translation = new Vector3(translation.X, translation.Y, translation.Z)
                    }
                };
                // Flip axes for root bones
                //if (bone.ParentId == -1)
                //{
                //    bone.InitialState = bone.InitialState.WithSwappedAxes();
                //}

                bones.Add(bone);
            }

            Bones = bones;

            stream.Seek(animationDataStart, SeekOrigin.Begin);

            Span<int> animDataStarts = stackalloc int[animationCount];
            Animations = new List<SkeletonAnimation>(animationCount);
            var streamStartOffsets = new HashSet<int>();
            for (var i = 0; i < animationCount; i++)
            {
                animDataStarts[i] = (int) stream.Position;

                var animation = new SkeletonAnimation
                {
                    Name = reader.ReadFixedString(64),
                    DriveType = (SkelAnimDriver) reader.ReadByte(),
                    Loopable = reader.ReadBoolean()
                };

                var eventCount = reader.ReadUInt16();
                var eventOffset = reader.ReadUInt32();

                // Read the events (but we need to seek for this)
                if (eventCount > 0)
                {
                    var posAfterEvents = stream.Position;
                    stream.Position = animDataStarts[i] + eventOffset;

                    var events = new SkelAnimEvent[eventCount];
                    for (var j = 0; j < events.Length; j++)
                    {
                        events[j] = new SkelAnimEvent
                        {
                            Frame = reader.ReadUInt16(),
                            Type = reader.ReadFixedString(48),
                            Action = reader.ReadFixedString(128)
                        };
                    }

                    stream.Position = posAfterEvents;
                    animation.Events = events;
                }
                else
                {
                    animation.Events = Array.Empty<SkelAnimEvent>();
                }

                var streamCount = reader.ReadUInt16();
                reader.ReadUInt16(); // Unknown value (padding most likely)

                var streams = new SkelAnimStream[10];
                var mem = MemoryMarshal.CreateFromPinnedArray(streams, 0, streams.Length);

                var byteSpan = MemoryMarshal.Cast<SkelAnimStream, byte>(mem.Span);
                reader.Read(byteSpan);
                if (streamCount == 0)
                {
                    streams = new SkelAnimStream[0];
                }
                else
                {
                    Array.Resize(ref streams, streamCount);
                }

                foreach (var animStream in streams)
                {
                    streamStartOffsets.Add(animDataStarts[i] + animStream.DataOffset);
                }

                animation.Streams = streams;


                Animations.Add(animation);
            }

            // Key frame data is contiguous in the file until the end of the file
            var sortedKeyFrameDataStart = streamStartOffsets.ToImmutableSortedSet().ToArray();
            var sortedKeyFrameDataLength = sortedKeyFrameDataStart.Select((start, idx) =>
            {
                if (idx + 1 < sortedKeyFrameDataStart.Length)
                {
                    return sortedKeyFrameDataStart[idx + 1] - start;
                }

                return memory.Length - start;
            }).ToArray();

            _animationsByName = new Dictionary<string, SkeletonAnimation>(Animations.Count);
            for (var i = 0; i < animationCount; i++)
            {
                var anim = Animations[i];
                anim.StreamData = new ReadOnlyMemory<byte>[anim.Streams.Length];
                anim.StreamDataIds = new int[anim.Streams.Length];
                for (var j = 0; j < anim.Streams.Length; j++)
                {
                    var dataStart = animDataStarts[i] + anim.Streams[j].DataOffset;
                    var dataStartIdx = Array.BinarySearch(sortedKeyFrameDataStart, dataStart);
                    var dataLength = sortedKeyFrameDataLength[dataStartIdx];
                    anim.StreamDataIds[j] = dataStart;
                    anim.StreamData[j] = memory.Slice(dataStart, dataLength);
                }

                _animationsByName[anim.Name.ToLowerInvariant()] = anim;
            }
        }
    }

    public string Path { get; set; }

    /// <summary>
    /// Searches for the index of a bone in this skeleton by name (case-insensitive).
    /// Returns -1 if no bone is found.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public int FindBoneIdxByName(ReadOnlySpan<char> name)
    {
        for (int i = 0; i < Bones.Count; i++)
        {
            ReadOnlySpan<char> boneName = Bones[i].Name;
            if (boneName.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    public List<SkeletonBone> Bones { get; }

    public List<SkeletonAnimation> Animations { get; }

    public KeyFrameStreamReader OpenKeyFrameStream(ReadOnlyMemory<byte> streamData)
    {
        return new KeyFrameStreamReader(CreateStream(streamData));
    }

    public SkeletonAnimation FindAnimByName(string name)
    {
        return _animationsByName.GetValueOrDefault(name.ToLowerInvariant(), null);
    }

    public int FindAnimIdxByName(ReadOnlySpan<char> name)
    {
        for (int i = 0; i < Animations.Count; i++)
        {
            ReadOnlySpan<char> animName = Animations[i].Name;
            if (animName.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    private static Stream CreateStream(ReadOnlyMemory<byte> data)
    {
        if (MemoryMarshal.TryGetArray(data, out var segment))
        {
            return new MemoryStream(segment.Array, segment.Offset, segment.Count, false);
        }

        throw new ArgumentException();
    }
}

public class SkeletonBone
{
    public short Flags { get; set; }
    public short ParentId { get; set; }
    public string Name { get; set; }
    public SkelBoneState InitialState { get; set; }
}

public class SkeletonAnimation
{
    public SkelAnimDriver DriveType;
    public SkelAnimEvent[] Events;
    public bool Loopable;
    public string Name;
    public ReadOnlyMemory<byte>[] StreamData;
    public int[] StreamDataIds;
    public SkelAnimStream[] Streams;
}

public struct SkelBoneState
{
    public Vector3 Scale;
    public Vector3 Translation;
    public Quaternion Rotation;

    public SkelBoneState WithSwappedAxes()
    {
        var scale = Matrix4x4.CreateScale(Scale.X, Scale.Y, Scale.Z);
        var translation = Matrix4x4.CreateTranslation(Translation.X, Translation.Y, Translation.Z);
        var rotation =
            Matrix4x4.CreateFromQuaternion(new Quaternion(Rotation.X, Rotation.Y, Rotation.Z, Rotation.W));

        var transform = Matrix4x4.Multiply(scale, Matrix4x4.Multiply(rotation, translation));

        Swap(ref transform.M12, ref transform.M13);
        Swap(ref transform.M22, ref transform.M23);
        Swap(ref transform.M32, ref transform.M33);
        Swap(ref transform.M42, ref transform.M43);

        Matrix4x4.Decompose(transform, out var fixedScale, out var fixedRotation, out var fixedTranslation);

        return new SkelBoneState
        {
            Scale = fixedScale,
            Rotation = fixedRotation,
            Translation = fixedTranslation
        };
    }

    public static void Lerp(Span<SkelBoneState> result,
        ReadOnlySpan<SkelBoneState> from,
        ReadOnlySpan<SkelBoneState> to,
        int boneCount,
        float fraction)
    {
        Trace.Assert(boneCount<= from.Length);
        Trace.Assert(boneCount<= to.Length);
        Trace.Assert(boneCount<= result.Length);

        // Handle simple cases where no interpolation needs to be performed
        if (fraction < 0.001) {
            if (result != from) {
                from.Slice(0, boneCount).CopyTo(result);
            }
            return;
        }
        else if (fraction > 0.999) {
            if (result != to) {
                to.Slice(0, boneCount).CopyTo(result);
            }
            return;
        }

        for (int i = 0; i < boneCount; i++) {
            result[i].Scale = Vector3.Lerp(from[i].Scale, to[i].Scale, fraction);
            result[i].Translation = Vector3.Lerp(from[i].Translation, to[i].Translation, fraction);
            result[i].Rotation = Quaternion.Slerp(from[i].Rotation, to[i].Rotation, fraction);
        }
    }

    private static void Swap(ref float y, ref float z)
    {
        var tmp = y;
        y = -z;
        z = tmp;
    }
}

public class SkelAnimEvent
{
    public int Frame { get; set; }
    public string Type { get; set; }
    public string Action { get; set; }
}

public enum SkelAnimDriver : byte
{
    Time = 0,
    Distance,
    Rotation
}

public struct SkelAnimStream
{
    public ushort Frames;
    public short VariationId; // This might actually be a uint8_t
    public float FrameRate;

    public float DPS;

    // Offset to the start of the key-frame stream in relation to SkaAnimation start
    public int DataOffset;
}