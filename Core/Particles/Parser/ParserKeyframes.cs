using System;
using System.Buffers.Text;
using System.Text;
using OpenTemple.Core.Logging;
using OpenTemple.Particles.Params;

namespace OpenTemple.Core.Particles.Parser
{
    /// Extracted logic for parsing animated particle system values into
    /// this class, since the initial parsing is the most complicated
    /// of all the parameters.
    ///
    /// Keyframe animations in particle values can be spotted by looking for
    /// commas. I.e. 0,255 would be a keyframed animation that goes from 0 -> 255
    /// with two keyframes at the start and end.
    public static class ParserKeyframes
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public static bool ParseKeyframe(ReadOnlySpan<byte> text, float lifespan, ref PartSysParamKeyframe frame)
        {
            if (!Utf8Parser.TryParse(text, out frame.value, out var bytesConsumed))
            {
                return false;
            }

            if (bytesConsumed >= text.Length)
            {
                // Only a value with a specific frame count
                return true;
            }

            if (text[bytesConsumed] == '?')
            {
                // This fixes a specific issue with borked particle systems in vanilla that specify values
                // such as "200?300,100" which was never actually supported
                return true;
            }

            if (text[bytesConsumed++] != '(')
            {
                // More data after the number, but not a bracket
                return false;
            }

            if (!Utf8Parser.TryParse(text.Slice(bytesConsumed), out frame.start, out var timeBytesConsumed))
            {
                return false;
            }

            bytesConsumed += timeBytesConsumed;

            // Handle percentage based keyframe, where the frame number is actually a percentage of lifetime
            if (bytesConsumed + 2 == text.Length && text[bytesConsumed] == '%' && text[bytesConsumed + 1] == ')')
            {
                frame.start *= lifespan / 100.0f;
                if (frame.start < 0)
                {
                    frame.start += lifespan;
                }

                return true;
            }
            else if (bytesConsumed + 1 == text.Length && text[bytesConsumed] == ')')
            {
                frame.start /= 30.0f; // convert from particle system frames to seconds

                // Relative to the end of the lifespan if its negative
                if (frame.start < 0)
                {
                    frame.start += lifespan;
                }

                return true;
            }

            Logger.Warn("Invalid keyframe: {0}", Encoding.Default.GetString(text));
            return false;
        }

        /// <summary>
        /// Validates that the keyframes are defined in ascending order.
        /// </summary>
        public static bool IsStartTimeAscending(PartSysParamKeyframe[] frames)
        {
            for (var i = 1; i < frames.Length; ++i)
            {
                if (frames[i].start <= frames[i - 1].start)
                {
                    return false;
                }
            }

            return true;
        }

        public static void PostprocessFrames(PartSysParamKeyframe[] frames)
        {
            // Calculate delta to next
            for (var j = 1; j < frames.Length; ++j)
            {
                float valueDelta = (frames[j].value - frames[j - 1].value);
                float timeDelta = (frames[j].start - frames[j - 1].start);
                frames[j - 1].deltaPerSec = valueDelta / timeDelta;
            }

            frames[^1].deltaPerSec = 0; // No delta for last
        }

        public static PartSysParamKeyframes Parse(ReadOnlySpan<byte> value, float parentLifespan)
        {
            using var frameDefsOwner = SpanUtils.SplitList(value, (byte) ',', out var frameCount);
            var frameDefs = frameDefsOwner.Memory.Span.Slice(0, frameCount);

            PartSysParamKeyframe preFrame = default;
            PartSysParamKeyframe postFrame = default;
            bool havePreFrame = false, havePostFrame = false;

            // Check if we need to insert a pre-frame to get a frame at time 0
            preFrame.start = 0;
            if (ParseKeyframe(value[frameDefs[0]], parentLifespan, ref preFrame))
            {
                if (preFrame.start > 0)
                {
                    preFrame.start = 0;
                    frameCount++;
                    havePreFrame = true;
                }
            }

            // Check if we need to insert a post-frame to get a frame at the end of the lifespan
            postFrame.start = parentLifespan;
            if (ParseKeyframe(value[frameDefs[^1]], parentLifespan, ref postFrame))
            {
                if (postFrame.start < parentLifespan)
                {
                    postFrame.start = parentLifespan;
                    frameCount++;
                    havePostFrame = true;
                }
            }

            var frames = new PartSysParamKeyframe[frameCount];

            // Pre-Frames are bugged in ToEE. The whole animation is shifted forward
            // if the first keyframe has a non-zero start-time and the last frame is
            // extended.
            if (havePreFrame)
            {
                // frames.push_back(preFrame);
            }

            // Increase in the frame time between frames
            float timeStep = parentLifespan / (frameCount - 1);

            var i = 0;
            float curTime = 0.0f;
            foreach (var frameDef in frameDefs)
            {
                ref var frame = ref frames[i];
                frame.start = curTime;
                curTime += timeStep;

                // If the frame's text is empty, copy the previous frame's value
                if (value[frameDef].IsEmpty)
                {
                    if (i == 0)
                    {
                        Logger.Warn("The first frame cannot be empty: {0}", Encoding.ASCII.GetString(value));
                        return null;
                    }
                    else
                    {
                        frames[i].value = frames[i - 1].value;
                    }
                }
                else
                {
                    if (!ParseKeyframe(value[frameDef], parentLifespan, ref frame))
                    {
                        Logger.Warn("Unable to parse particle system keyframes: {0}", Encoding.ASCII.GetString(value));
                        return null;
                    }
                }

                if (i++ == 0 && havePreFrame)
                {
                    frame.start = 0;
                }
            }

            // See above for the ToEE bug we're trying to replicate here...
            if (havePreFrame)
            {
                if (curTime <= parentLifespan)
                {
                    var lastFrame = frames[i - 1];

                    lastFrame.start = curTime;
                    curTime += timeStep;

                    frames[i++] = lastFrame;
                }
                else
                {
                    // We didn't need the extra frame after all
                    Array.Resize(ref frames, frames.Length - 1);
                }
            }

            if (havePostFrame)
            {
                frames[i++] = postFrame;
            }

            PostprocessFrames(frames);

            // Validate monotonically increasing start times
            if (!IsStartTimeAscending(frames))
            {
                Logger.Warn("Animated keyframes '{0}' are not in ascending time.", Encoding.ASCII.GetString(value));
                return null;
            }

            return new PartSysParamKeyframes(frames);
        }
    }
}