using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace OpenTemple.Core.AAS;

public struct KeyFrameBoneState
{
    public Vector3 Scale;

    public Quaternion Rotation;

    public Vector3 Translation;

    public bool Equals(KeyFrameBoneState other)
    {
        return Scale.Equals(other.Scale) && Rotation.Equals(other.Rotation) && Translation.Equals(other.Translation);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        return obj is KeyFrameBoneState other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Scale.GetHashCode();
            hashCode = (hashCode * 397) ^ Rotation.GetHashCode();
            hashCode = (hashCode * 397) ^ Translation.GetHashCode();
            return hashCode;
        }
    }
}

public class KeyFrameStreamReader : IDisposable
{
    private readonly BinaryReader _reader;

    private readonly float _scaleFactor;

    private readonly float _translationFactor;

    private const float RotationFactor = 1 / 32767.0f;

    public short[] BoneIds { get; }

    public KeyFrameBoneState[] BoneStates { get; }

    public KeyFrameStreamReader(Stream stream)
    {
        _reader = new BinaryReader(stream);

        _scaleFactor = _reader.ReadSingle();
        _translationFactor = _reader.ReadSingle();

        // Read keyframe 0
        var boneIds = new List<short>();
        var boneStates = new List<KeyFrameBoneState>();
        short boneId = _reader.ReadInt16();
        while (boneId >= 0)
        {
            boneIds.Add(boneId);
                
            var boneState = new KeyFrameBoneState();
            boneState.Scale.X = _scaleFactor * _reader.ReadInt16();
            boneState.Scale.Y = _scaleFactor * _reader.ReadInt16();
            boneState.Scale.Z = _scaleFactor * _reader.ReadInt16();

            boneState.Rotation.X = RotationFactor * _reader.ReadInt16();
            boneState.Rotation.Y = RotationFactor * _reader.ReadInt16();
            boneState.Rotation.Z = RotationFactor * _reader.ReadInt16();
            boneState.Rotation.W = RotationFactor * _reader.ReadInt16();

            boneState.Translation.X = _translationFactor * _reader.ReadInt16();
            boneState.Translation.Y = _translationFactor * _reader.ReadInt16();
            boneState.Translation.Z = _translationFactor * _reader.ReadInt16();

            boneStates.Add(boneState);

            boneId = _reader.ReadInt16();
        }

        BoneIds = boneIds.ToArray();
        BoneStates = boneStates.ToArray();

    }

    public bool NextFrame()
    {
        var marker = _reader.ReadInt16();
        var frame = marker >> 1;
        if (frame == -1)
        {
            return false;
        }

        var boneMarker = _reader.ReadInt16();
        while ((boneMarker & 1) != 0)
        {
            var boneId = boneMarker >> 4;
            var channelMask = boneMarker >> 1;
                
            var boneIdx = Array.IndexOf(BoneIds, (short) boneId);
            Debug.Assert(boneIdx != -1);
            ref var boneState = ref BoneStates[boneIdx];

            if ((channelMask & 4) == 4)
            {
                var f = _reader.ReadInt16(); // What is this frame idx for???
                boneState.Scale.X = _scaleFactor * _reader.ReadInt16();
                boneState.Scale.Y = _scaleFactor * _reader.ReadInt16();
                boneState.Scale.Z = _scaleFactor * _reader.ReadInt16();
            }

            if ((channelMask & 2) == 2)
            {
                var f = _reader.ReadInt16(); // What is this frame idx for???
                boneState.Rotation.X = RotationFactor * _reader.ReadInt16();
                boneState.Rotation.Y = RotationFactor * _reader.ReadInt16();
                boneState.Rotation.Z = RotationFactor * _reader.ReadInt16();
                boneState.Rotation.W = RotationFactor * _reader.ReadInt16();
            }

            if ((channelMask & 1) == 1)
            {
                var f = _reader.ReadInt16(); // What is this frame idx for???
                boneState.Translation.X = _translationFactor * _reader.ReadInt16();
                boneState.Translation.Y = _translationFactor * _reader.ReadInt16();
                boneState.Translation.Z = _translationFactor * _reader.ReadInt16();
            }

            boneMarker = _reader.ReadInt16();
            if ((boneMarker & 1) == 0)
            {
                _reader.BaseStream.Seek(-2, SeekOrigin.Current);
                break;
            }
        }

        return true;
    }

    public void Dispose()
    {
        _reader.Close();
    }
}