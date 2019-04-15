using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SpicyTemple.Core.AAS
{
    internal struct AnimPlayerStreamBone
    {
        // W = 1 / (scaleNextFrame - scaleFrame)
        public Vector4 prevScale;
        public Vector4 scale;
        public float scaleFrameFactor;
        public Quaternion prevRotation;
        public Quaternion rotation;
        // W = 1 / (translationNextFrame - translationFrame)
        public Vector4 prevTranslation;
        public Vector4 translation;
        public short scaleFrame;
        public short scaleNextFrame;
        public short rotationFrame;
        public short rotationNextFrame;
        public short translationFrame;
        public short translationNextFrame;
        public int field6C;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct AnimStreamHeader
    {
        public readonly float ScaleFactor;
        public readonly float TranslationFactor;
    }

    internal class AnimPlayerStream
    {
        public readonly Skeleton skeleton;
        public readonly float scaleFactor;
        public readonly float translationFactor;
        public float currentFrame;
        private readonly ReadOnlyMemory<byte> streamData;
        private int frameDataIdx;
        public readonly AnimPlayerStreamBone[] boneState;

        private ReadOnlySpan<short> FrameData => MemoryMarshal.Cast<byte, short>(
            streamData.Span.Slice(sizeof(float) * 2)
        );

        private const float rFactor = 1.0f / 32767.0f;

        public AnimPlayerStream(Skeleton skeleton, ReadOnlyMemory<byte> streamData)
        {
            this.skeleton = skeleton;
            this.boneState = new AnimPlayerStreamBone[skeleton.Bones.Count];
            this.streamData = streamData;
            this.frameDataIdx = 0;

            // Read the two floating point factors from the stream header
            var header = MemoryMarshal.Read<AnimStreamHeader>(streamData.Span);
            scaleFactor = header.ScaleFactor;
            translationFactor = header.TranslationFactor;

            InitBoneState();
        }

        private void InitBoneState()
        {
            var bones = skeleton.Bones;

            // Initialize this stream with the initial state from the skeleton
            for (var i = 0; i < bones.Count; i++)
            {
                ref var sBone = ref boneState[i];
                var fBone = bones[i];
                sBone.scale = new Vector4(fBone.InitialState.Scale, 0);
                sBone.prevScale = sBone.scale;

                sBone.rotation = fBone.InitialState.Rotation;
                sBone.prevRotation = sBone.rotation;

                sBone.translation = new Vector4(fBone.InitialState.Translation, 0);
                sBone.prevTranslation = sBone.translation;

                sBone.scaleFrame = sBone.scaleNextFrame = sBone.translationFrame = sBone.translationNextFrame = 0;
                sBone.rotationFrame = sBone.rotationNextFrame = 0;
                sBone.field6C = 0;
            }

            var frameData = FrameData;

            while (frameData[frameDataIdx] >= 0)
            {
                var boneId = frameData[frameDataIdx++];
                ref var sBone = ref boneState[boneId];
                sBone.scale.X = scaleFactor * frameData[frameDataIdx++];
                sBone.scale.Y = scaleFactor * frameData[frameDataIdx++];
                sBone.scale.Z = scaleFactor * frameData[frameDataIdx++];
                sBone.prevScale = sBone.scale;

                sBone.rotation.X = rFactor * frameData[frameDataIdx++];
                sBone.rotation.Y = rFactor * frameData[frameDataIdx++];
                sBone.rotation.Z = rFactor * frameData[frameDataIdx++];
                sBone.rotation.W = rFactor * frameData[frameDataIdx++];
                sBone.prevRotation = sBone.rotation;

                sBone.translation.X = translationFactor * frameData[frameDataIdx++];
                sBone.translation.Y = translationFactor * frameData[frameDataIdx++];
                sBone.translation.Z = translationFactor * frameData[frameDataIdx++];
                sBone.prevTranslation = sBone.translation;
            }

            currentFrame = -1.0f;
            frameDataIdx++; // Skip the -1 terminator
            SetFrame(0.0f);
        }

        public void SetFrame(float frame)
        {
            var frameRounded = MathF.Floor(frame);
            if (MathF.Floor(this.currentFrame) == frameRounded)
            {
                this.currentFrame = frame;
                return;
            }

            if (frame < this.currentFrame)
            {
                SetFrame(32766.0f); // run to end
                InitBoneState();
            }

            // set currentFrame
            this.currentFrame = frame;
            if (frameRounded >= 32767)
                frameRounded = 32766;

            var frameData = FrameData;
            var keyframeFrame = frameData[frameDataIdx] >> 1;

            // advance the frames
            while (keyframeFrame <= frameRounded)
            {
                frameDataIdx++;
                var flags = frameData[frameDataIdx];
                while ((flags & 1) == 1)
                {
                    var boneId = flags >> 4;
                    ref var tBone = ref boneState[boneId];
                    frameDataIdx++;

                    if ((flags & 0xE) == 0)
                    {
                        flags = frameData[frameDataIdx];
                        continue;
                    }

                    if ((flags & 8) == 8) // scale
                    {
                        tBone.scaleFrame = (short) keyframeFrame;
                        tBone.scaleNextFrame = frameData[frameDataIdx];
                        tBone.prevScale = tBone.scale;

                        frameDataIdx++;
                        tBone.scale.X = frameData[frameDataIdx] * scaleFactor;
                        frameDataIdx++;
                        tBone.scale.Y = frameData[frameDataIdx] * scaleFactor;
                        frameDataIdx++;
                        tBone.scale.Z = frameData[frameDataIdx] * scaleFactor;

                        var one_over_frame_delta = 0.0f;
                        if (keyframeFrame < tBone.scaleNextFrame)
                            one_over_frame_delta = 1.0f / (tBone.scaleNextFrame - keyframeFrame);
                        tBone.prevScale.W = one_over_frame_delta;

                        frameDataIdx++;
                    }

                    if ((flags & 4) == 4) // rotation
                    {
                        tBone.rotationFrame = (short) keyframeFrame;
                        tBone.rotationNextFrame = frameData[frameDataIdx];
                        tBone.prevRotation = tBone.rotation;

                        frameDataIdx++;
                        tBone.rotation.X = frameData[frameDataIdx] * rFactor;
                        frameDataIdx++;
                        tBone.rotation.Y = frameData[frameDataIdx] * rFactor;
                        frameDataIdx++;
                        tBone.rotation.Z = frameData[frameDataIdx] * rFactor;
                        frameDataIdx++;
                        tBone.rotation.W = frameData[frameDataIdx] * rFactor;

                        var one_over_frame_delta = 0.0f;
                        if (keyframeFrame < tBone.rotationNextFrame)
                            one_over_frame_delta = 1.0f / (tBone.rotationNextFrame - keyframeFrame);
                        tBone.scale.W = one_over_frame_delta;

                        frameDataIdx++;
                    }

                    if ((flags & 2) == 2) // translation
                    {
                        tBone.translationFrame = (short) keyframeFrame;
                        tBone.translationNextFrame = frameData[frameDataIdx];
                        tBone.prevTranslation.X = tBone.translation.X;
                        tBone.prevTranslation.Y = tBone.translation.Y;
                        tBone.prevTranslation.Z = tBone.translation.Z;

                        frameDataIdx++;
                        int transX = frameData[frameDataIdx];
                        tBone.translation.X = transX * translationFactor;
                        frameDataIdx++;
                        int transY = frameData[frameDataIdx];
                        tBone.translation.Y = transY * translationFactor;
                        frameDataIdx++;
                        int transZ = frameData[frameDataIdx];
                        tBone.translation.Z = transZ * translationFactor;

                        var one_over_frame_delta = 0.0f;
                        if (keyframeFrame < tBone.translationNextFrame)
                            one_over_frame_delta = 1.0f / (tBone.translationNextFrame - keyframeFrame);
                        tBone.translation.W = one_over_frame_delta;

                        frameDataIdx++;
                    }

                    flags = frameData[frameDataIdx];
                }

                keyframeFrame = frameData[frameDataIdx] >> 1;
            }
        }


        public void GetBoneState(Span<SkelBoneState> boneStateOut)
        {
            Trace.Assert(boneState.Length <= boneStateOut.Length);

            var currentFrameCeil = (int) Math.Ceiling(currentFrame);
            var currentFrameFloor = (int) Math.Floor(currentFrame);

            for (int i = 0; i < boneState.Length; i++)
            {
                ref var bone = ref boneState[i];
                ref var boneOut = ref boneStateOut[i];

                // Handle interpolation of the scale data
                if (currentFrameCeil <= bone.scaleFrame)
                {
                    boneOut.Scale.X = bone.prevScale.X;
                    boneOut.Scale.Y = bone.prevScale.Y;
                    boneOut.Scale.Z = bone.prevScale.Z;
                }
                else if (currentFrameFloor >= bone.scaleNextFrame)
                {
                    boneOut.Scale.X = bone.scale.X;
                    boneOut.Scale.Y = bone.scale.Y;
                    boneOut.Scale.Z = bone.scale.Z;
                }
                else
                {
                    // Position [0,1] between the two frames.
                    var f = (currentFrame - bone.scaleFrame) * bone.scale.W;

                    // Interpolate between the scale of the two keyframes
                    boneOut.Scale.X = (1.0f - f) * bone.prevScale.X + f * bone.scale.X;
                    boneOut.Scale.Y = (1.0f - f) * bone.prevScale.Y + f * bone.scale.Y;
                    boneOut.Scale.Z = (1.0f - f) * bone.prevScale.Z + f * bone.scale.Z;
                }

                // Handle interpolation of the rotation data
                if (currentFrameCeil <= bone.rotationFrame)
                {
                    boneOut.Rotation.X = bone.prevRotation.X;
                    boneOut.Rotation.Y = bone.prevRotation.Y;
                    boneOut.Rotation.Z = bone.prevRotation.Z;
                    boneOut.Rotation.W = bone.prevRotation.W;
                }
                else if (currentFrameFloor >= bone.rotationNextFrame)
                {
                    boneOut.Rotation.X = bone.rotation.X;
                    boneOut.Rotation.Y = bone.rotation.Y;
                    boneOut.Rotation.Z = bone.rotation.Z;
                    boneOut.Rotation.W = bone.rotation.W;
                }
                else
                {
                    // Position [0,1] between the two frames.
                    var f = (currentFrame - bone.rotationFrame) * bone.scale.W;
                    boneOut.Rotation = Quaternion.Slerp(bone.prevRotation, bone.rotation, f);
                }

                // Handle interpolation of the translation data
                if (currentFrameCeil <= bone.translationFrame)
                {
                    boneOut.Translation.X = bone.prevTranslation.X;
                    boneOut.Translation.Y = bone.prevTranslation.Y;
                    boneOut.Translation.Z = bone.prevTranslation.Z;
                }
                else if (currentFrameFloor >= bone.translationNextFrame)
                {
                    boneOut.Translation.X = bone.translation.X;
                    boneOut.Translation.Y = bone.translation.Y;
                    boneOut.Translation.Z = bone.translation.Z;
                }
                else
                {
                    // Position [0,1] between the two frames.
                    var f = (currentFrame - bone.translationFrame) * bone.translation.W;
                    boneOut.Translation.X = f * bone.translation.X + (1.0f - f) * bone.prevTranslation.X;
                    boneOut.Translation.Y = f * bone.translation.Y + (1.0f - f) * bone.prevTranslation.Y;
                    boneOut.Translation.Z = f * bone.translation.Z + (1.0f - f) * bone.prevTranslation.Z;
                }
            }
        }

        public float GetCurrentFrame() => currentFrame;
    }
}