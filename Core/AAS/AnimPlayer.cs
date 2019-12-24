using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OpenTemple.Core.AAS
{
    internal struct AnimEvent
    {
        public int frame; // On which frame does the event occur
        public AnimEventType type;
        public string args;
    };

    internal class AnimPlayer : IDisposable
    {
        public AnimatedModel ownerAnim;
        public sbyte field_D;
        public sbyte field_E;
        public sbyte field_F;
        public float weight; // 1.0 = Fully weighted, otherwise weighted with primary anims
        public float fadingSpeed; // Velocity with which weight is being changed
        public int eventHandlingDepth;
        public AnimPlayer nextRunningAnim;
        public AnimPlayer prevRunningAnim;
        public SkeletonAnimation animation;
        public int streamCount;
        public AnimPlayerStream[] streams = new AnimPlayerStream[4];
        public float[] streamFps = new float[4]; // "frames per drive unit"
        public sbyte[] streamVariationIndices = new sbyte[4]; // indices into streamVariationIds
        public sbyte[] streamVariationIds = new sbyte[4];
        public sbyte[] variationId = new sbyte[4];
        public float currentTimeEvents;
        public float currentTime;
        public float duration;
        public float frameRate;
        public float distancePerSecond;
        public List<AnimEvent> events;

        public AnimPlayer()
        {
            // Default is fading in over .5 seconds
            fadingSpeed = 2.0f;
        }

        public void Dispose()
        {
            ownerAnim?.RemoveRunningAnim(this);
        }

        public void GetDistPerSec(ref float distPerSec)
        {
            if (animation.DriveType == SkelAnimDriver.Distance && fadingSpeed > 0.0) {
                distPerSec = ownerAnim.scale * distancePerSecond;
            }
        }

        public void GetRotationPerSec(ref float rotationPerSec)
        {
            if (animation.DriveType == SkelAnimDriver.Rotation && fadingSpeed > 0.0) {
                rotationPerSec = distancePerSecond;
            }
        }

        public void AdvanceEvents(float timeChanged, float distanceChanged, float rotationChanged)
        {

            if (duration <= 0) {
                return;
            }

            // Decide what the effective advancement in the animation stream will be
            float effectiveAdvancement;
            switch (animation.DriveType) {
                default:
                case SkelAnimDriver.Time:
                    effectiveAdvancement = timeChanged;
                    break;
                case SkelAnimDriver.Distance:
                    effectiveAdvancement = ownerAnim.scaleInv * distanceChanged;
                    break;
                case SkelAnimDriver.Rotation:
                    // TODO: Weirdly enough, in the other function it's using the absolute value of it
                    // Does this mean that rotation-based animations will not properly trigger events???
                    effectiveAdvancement = rotationChanged;
                    break;
            }

            foreach (var evt in events) {

                // Convert from frame id to "time"
                var eventTime = MathF.Min(duration, evt.frame / frameRate);

                // Check if the increase in time will cause the event to trigger
                // and consider looping animations, but only if the event would occur during the next loop
                if (currentTimeEvents <= eventTime &&  currentTimeEvents + effectiveAdvancement > eventTime
                    || animation.Loopable
                    && currentTimeEvents - duration <= eventTime && eventTime < (currentTimeEvents + effectiveAdvancement - duration))
                {
                    var timeAfterEvent = currentTimeEvents + effectiveAdvancement - eventTime;
                    ownerAnim.EventHandler.HandleEvent(
	                    evt.frame,
	                    timeAfterEvent,
	                    evt.type,
	                    evt.args
                    );
                }
            }

            currentTimeEvents += effectiveAdvancement;

            if (currentTimeEvents > duration) {

                if (animation.Loopable) {
                    var extraTime = currentTimeEvents - duration;
                    currentTimeEvents = extraTime;
                    if (currentTimeEvents > duration) {
                        if (extraTime - effectiveAdvancement == 0.0f) {
                            currentTimeEvents = 0.0f;
                        } else {
                            currentTimeEvents = duration;
                        }
                    }
                } else {
                    currentTimeEvents = duration;
                        ownerAnim.EventHandler.HandleEvent(
                            (int)(frameRate * duration),
                            0.0f,
                            AnimEventType.End,
                            ""
                        );
                }

            }
        }

        public void FadeInOrOut(float timeChanged)
        {
            // Modify weight according to fadein/fadeout speed
            weight += timeChanged * fadingSpeed;

            // Clamp weight to [0,1]
            if (weight <= 0.0f) {
                weight = 0.0f;
            } else if (weight > 1.0) {
                weight = 1.0f;
            }
        }

        public void method6(Span<SkelBoneState> boneStateOut, float timeChanged, float distanceChanged, float rotationChanged)
        {

		// In ToEE, boneIdx is never set to anything other than -1

		if (duration > 0) {
			// Decide what the effective advancement in the animation stream will be
			float effectiveAdvancement;
			switch (animation.DriveType) {
			case SkelAnimDriver.Time:
				effectiveAdvancement = timeChanged;
				break;
			case SkelAnimDriver.Distance:
				effectiveAdvancement = ownerAnim.scaleInv * distanceChanged;
				break;
			case SkelAnimDriver.Rotation:
				// TODO: Weirdly enough, in the other function it's using the absolute value of it
				// Does this mean that rotation-based animations will not properly trigger events???
				effectiveAdvancement = MathF.Abs(rotationChanged);
				break;
			default:
				throw new AasException($"Unknown animation drive type: {animation.DriveType}");
			}

			// Same logic as in AddTime for events, just different data fields and no event handling
			currentTime += effectiveAdvancement;
			if (currentTime > duration) {
				if (animation.Loopable) {
					var extraTime = currentTime - duration;
					currentTime = extraTime;
					if (currentTime > duration) {
						if (extraTime - effectiveAdvancement == 0.0f) {
							currentTime = 0.0f;
						} else {
							currentTime = duration;
						}
					}
				} else {
					currentTime = duration;
				}
			}

			// Propagate the frame index derived from the current time to all streams
			for (int i = 0; i < streamCount; i++) {
				var frame = streamFps[i] * currentTime;
				streams[i].SetFrame(frame);
			}

		}

		if (streamCount != 1 || ownerAnim.variationCount != 1 || weight != 1.0f) {

			// Get the bone data for each stream
			Span<SkelBoneState> boneData = stackalloc SkelBoneState[4 * 1024]; // 4 streams with at most 1024 bones each
			for (int i = 0; i < streamCount; i++) {
				streams[i].GetBoneState(boneData.Slice(i * 1024, 1024));
			}

			var boneCount = ownerAnim.skeleton.Bones.Count;
			Trace.Assert(boneCount <= 1024);

			Span<SkelBoneState> boneDataTemp = stackalloc SkelBoneState[1024];
			var boneDataBuf = boneDataTemp;
			if (weight == 1.0f) {
				boneDataBuf = boneStateOut;
			}

			// Copy over the first stream's bone data
			boneData.Slice(1024 * streamVariationIndices[0], boneDataBuf.Length).CopyTo(boneDataBuf);

			// LERP the rest
			for (var i = 1; i < ownerAnim.variationCount; i++) {
				if (streamVariationIndices[i] < 4) {
					var factor = ownerAnim.variations[i].factor;
					SkelBoneState.Lerp(boneDataBuf, boneDataBuf, boneData.Slice(1024 * streamVariationIndices[i], 1024), boneCount, factor);
				}
			}

			SkelBoneState.Lerp(boneStateOut, boneStateOut, boneDataBuf, boneCount, weight);
		} else {
			streams[0].GetBoneState(boneStateOut);
		}
        }

        public void SetTime(float time)
        {
	        currentTime = time;
	        while (currentTime > duration) {
		        currentTime -= duration;
	        }

	        var frameIndex = frameRate * currentTime;
	        for (int i = 0; i < streamCount; i++) {
		        var stream = streams[i];
		        stream.SetFrame(frameIndex);
	        }
        }

        public float GetCurrentFrame()
        {

	        if (streamCount < 1) {
		        return 0.0f;
	        }
	        return streams[0].GetCurrentFrame();
        }

        public float method9()
        {
	        return 0.5f;
        }

        public float method10()
        {
	        return 0.5f;
        }

        public void EnterEventHandling()
        {
	        eventHandlingDepth++;
        }

        public void LeaveEventHandling()
        {
	        eventHandlingDepth--;

        }

        public int GetEventHandlingDepth()
        {
	        return eventHandlingDepth;

        }

        public void Attach(AnimatedModel owner, int animIdx)
        {

		Trace.Assert(streamCount == 0 && owner != null && ownerAnim == null);
		var skeleton = owner.skeleton;
		Trace.Assert(skeleton != null);

		var anims = skeleton.Animations;
		Trace.Assert(animIdx >= 0 && animIdx < anims.Count);

		this.animation = anims[animIdx];
		this.distancePerSecond = animation.Streams[0].DPS;
		this.frameRate = animation.Streams[0].FrameRate;
		SetEvents(owner, animation);
		this.streamCount = 0;

		// TODO: The entire variation stuff is unused I think
		Span<int> skaStreamIdxMap = stackalloc int[4]; // Maps this player's streams to their respective idx in the SKA anim
		for (int i = 0; i < owner.variationCount; i++) {
			var variation = owner.variations[i];

			// Find a stream in the animation suitable for the variation that is requested
			int skaStreamIdx = -1;
			for (int j = 0; j < animation.Streams.Length; j++) {
				int streamVariation = animation.Streams[j].VariationId;
				if (streamVariation == variation.variationId || skaStreamIdx == -1 && streamVariation == -1) {
					skaStreamIdx = j;
				}
			}

			streamVariationIds[i] = 4; // This effectively means no stream for this variation
			if (skaStreamIdx != -1) {
				// Do we already use that stream???
				int j;
				for (j = 0; j < streamCount; j++) {
					if (skaStreamIdxMap[j] == skaStreamIdx) {
						break; // Found it
					}
				}

				// Remember which stream in this player is used
				// for the variation found in the parent model
				streamVariationIndices[i] = (sbyte) j;

				// Do we have to create the new stream?
				if (j == streamCount && j < 4) {
					skaStreamIdxMap[j] = skaStreamIdx;

					streams[j] = new AnimPlayerStream(skeleton, animation.StreamData[skaStreamIdx]);
					streamFps[j] = 0;
					// NOTE: The following line used the incorrect index into animation.streams
					variationId[j] = (sbyte) animation.Streams[skaStreamIdx].VariationId;
					streamCount++;
				}

			}
		}

		// Calculate the duration based on the individual streams (weighted together)
		// NOTE: I don't think this makes a terrible amount of sense since as far as I understand
		// the variation factor it only affects bone blending, not length
		duration = 0;
		for (int i = 0; i < owner.variationCount; i++) {
			var factor = owner.variations[i].factor;

			var streamIdx = streamVariationIndices[i];
			if (streamIdx < 4) {
				ref var skaStream = ref animation.Streams[skaStreamIdxMap[i]];
				if (skaStream.Frames > 1 && skaStream.FrameRate > 0) {
					duration = (skaStream.Frames - 1) / skaStream.FrameRate * factor
						+ (1.0f - factor) * duration;
				}
			}
		}

		// Recalc the stream FPS taking into account the variation weighting applied above
		if (duration > 0) {
			for (int i = 0; i < streamCount; i++) {
				ref var skaStream = ref animation.Streams[skaStreamIdxMap[i]];
				if (skaStream.Frames > 1) {
					streamFps[i] = (skaStream.Frames - 1) / duration;
				}
			}
		}

		owner.AddRunningAnim(this);
		field_D = 1;
        }

        public void Setup2(float fadeInTimeSecs)
        {

	        // Always 0.5s
	        if (fadeInTimeSecs <= 0.0f) {
		        fadingSpeed = 1.0f;
		        weight = 1.0f;
	        } else {
		        var fadeInSpeed = 1.0f / fadeInTimeSecs;
		        if (fadeInSpeed <= fadingSpeed) {
			        weight = 0.0001f;
		        }
		        else {
			        fadingSpeed = fadeInSpeed;
			        weight = 0.0001f;
		        }
	        }
        }

        private static AnimEventType? GetEventType(ReadOnlySpan<char> type) {
	        if (type.Equals("script", StringComparison.OrdinalIgnoreCase)) {
		        return AnimEventType.Script;
	        } else if (type.Equals("end", StringComparison.OrdinalIgnoreCase)) {
		        return AnimEventType.End;
	        } else if (type.Equals("action", StringComparison.OrdinalIgnoreCase)) {
		        return AnimEventType.Action;
	        } else {
		        return null;
	        }
        }

        private void SetEvents(AnimatedModel owner, SkeletonAnimation anim)
        {
	        events = new List<AnimEvent>(anim.Events.Length);
	        foreach (var skelEvent in anim.Events) {
		        var type = GetEventType(skelEvent.Type);
		        if (!type.HasValue)
		        {
			        Debug.Print("Unknown animation type '{}' in {}", anim.Name, owner.skeleton.Path);
			        continue;
		        }

		        AnimEvent evt = new AnimEvent();
		        evt.frame = skelEvent.Frame;
		        evt.type = type.Value;
		        evt.args = skelEvent.Action;
		        events.Add(evt);
	        }
        }
    }
}