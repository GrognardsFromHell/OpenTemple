using System;
using System.Collections.Generic;
using OpenTemple.Core.Particles.Instances;

namespace OpenTemple.Particles.Params;

public enum PartSysParamType : uint
{
    PSPT_CONSTANT = 0,
    PSPT_RANDOM = 1,
    PSPT_KEYFRAMES = 2,
    PSPT_SPECIAL = 5
};

/*
    Abstract base class that encapsulates the runtime state
    required to model a parameter.
*/
public abstract class PartSysParamState : IDisposable
{
    public abstract float GetValue(PartSysEmitter emitter, int particleIdx, float lifetimeSec);

    public virtual void InitParticle(int particleIdx)
    {
    }

    public virtual void Dispose()
    {
    }
}

public static class PartSysParamDefaultValues
{
    private static readonly float[] DefaultValues =
    {
        0, // 0
        0, // 1
        0, // 2
        0, // 3
        0, // 4
        0, // 5
        0, // 6
        0, // 7
        0, // 8
        0, // 9
        0, // 10
        0, // 11
        1.0f, // 12
        1.0f, // 13
        1.0f, // 14
        0, // 15
        0, // 16
        0, // 17
        0, // 18
        0, // 19
        0, // 20
        255.0f, // 21
        255.0f, // 22
        255.0f, // 23
        255.0f, // 24
        0, // 25
        0, // 26
        0, // 27
        0, // 28
        0, // 29
        0, // 30
        0, // 31
        0, // 32
        0, // 33
        0, // 34
        1.0f, // 35
        1.0f, // 36
        1.0f, // 37
        0, // 38
        0, // 39
        0, // 40
        0, // 41
        0, // 42
        0, // 43
        0, // 44
    };

    /// <summary>
    /// Returns the value a parameter shall have if it is undefined.
    /// </summary>
    public static float GetDefaultValue(PartSysParamId id)
    {
        return DefaultValues[(int) id];
    }
}

public interface IPartSysParam
{
    PartSysParamType Type { get; }

    PartSysParamState CreateState(int particleCount);
}

public struct PartSysParamKeyframe
{
    public float start;
    public float value;
    public float deltaPerSec;
}

public class PartSysParamKeyframes : IPartSysParam
{
    private readonly ImmutableState _state;

    private readonly PartSysParamKeyframe[] _frames;

    public PartSysParamKeyframes(PartSysParamKeyframe[] frames)
    {
        _frames = frames;
        _state = new ImmutableState(frames);
    }

    public PartSysParamType Type => PartSysParamType.PSPT_KEYFRAMES;

    public PartSysParamKeyframe[] GetFrames()
    {
        return _frames;
    }

    public PartSysParamState CreateState(int particleCount)
    {
        return _state;
    }

    private class ImmutableState : PartSysParamState
    {
        private readonly PartSysParamKeyframe[] _frames;

        public ImmutableState(PartSysParamKeyframe[] frames)
        {
            _frames = frames;
        }

        public override float GetValue(PartSysEmitter emitter, int particleIdx, float lifetimeSec)
        {
            for (var i = 0; i < _frames.Length - 1; ++i)
            {
                var frame = _frames[i];
                var nextFrame = _frames[i + 1];

                // Don't LERP if we're right on the frame border
                // (or in case of the first frame, possibly earlier)
                if (lifetimeSec <= frame.start)
                {
                    return frame.value;
                }
                else if (lifetimeSec >= nextFrame.start)
                {
                    continue; // The lifetime is beyond the current keyframe gap
                }

                // The lifetime is between start of frame and start of nextFrame
                // So let's lerp the value.
                var timeSinceFrame = lifetimeSec - frame.start;
                return frame.value + frame.deltaPerSec * timeSinceFrame;
            }

            // Return the value of the last frame since we seem to be beyond it
            return _frames[^1].value;
        }
    }
}

public class PartSysParamConstant : IPartSysParam
{
    private readonly State _state;

    private readonly float _value;

    public PartSysParamConstant(float value)
    {
        _value = value;
        _state = new State(value);
    }

    public float GetValue()
    {
        return _value;
    }

    public PartSysParamType Type => PartSysParamType.PSPT_CONSTANT;

    public PartSysParamState CreateState(int particleCount)
    {
        return _state;
    }

    private class State : PartSysParamState
    {
        private readonly float _value;

        public State(float value)
        {
            _value = value;
        }

        public override float GetValue(PartSysEmitter emitter, int particleIdx, float lifetimeSec)
        {
            return _value;
        }
    }
}

public static class PartSysRandomGen
{
    public const ushort MAX_VALUE = 0x7FFF;

    public const float MAX_VALUE_FACTOR = 1.0f / MAX_VALUE;

    private static uint _state = 0x1127E5;

    public static ushort NextValue()
    {
        _state = 0x19660D * _state + 0x3C6EF35F;
        return unchecked((ushort) ((_state >> 8) & 0x7FFF));
    }

    public static float NextValue(float rangeInclusive)
    {
        return NextValue() * rangeInclusive * MAX_VALUE_FACTOR;
    }
}

public sealed class PartSysParamStateRandom : PartSysParamState
{
    private readonly float[] _particles;
    private readonly float _base;
    private readonly float _variance;

    public PartSysParamStateRandom(int particleCount, float baseValue, float variance)
    {
        _particles = new float[particleCount];
        _base = baseValue;
        _variance = variance;

        // Create a value for each parameter
        for (var i = 0; i < particleCount; ++i)
        {
            InitParticle(i);
        }
    }

    public override float GetValue(PartSysEmitter emitter, int particleIdx, float lifetimeSec)
    {
        return _particles[particleIdx];
    }

    public override void InitParticle(int particleIdx)
    {
        _particles[particleIdx] = _base + PartSysRandomGen.NextValue(_variance);
    }
}

public class PartSysParamRandom : IPartSysParam
{
    private readonly float _base;
    private readonly float _variance;

    public PartSysParamRandom(float baseValue, float variance)
    {
        _base = baseValue;
        _variance = variance;
    }

    public PartSysParamType Type => PartSysParamType.PSPT_RANDOM;

    public float GetBase()
    {
        return _base;
    }

    public float GetVariance()
    {
        return _variance;
    }

    public PartSysParamState CreateState(int particleCount)
    {
        return new PartSysParamStateRandom(particleCount, _base, _variance);
    }
}

public enum PartSysParamSpecialType
{
    PSPST_RADIUS
}

public sealed class PartSysParamSpecial : IPartSysParam
{
    private readonly State _state;

    private readonly PartSysParamSpecialType _specialType;

    public PartSysParamSpecial(PartSysParamSpecialType specialType)
    {
        _state = new State(specialType);
        _specialType = specialType;
    }

    public PartSysParamType Type => PartSysParamType.PSPT_SPECIAL;

    public PartSysParamSpecialType GetSpecialType()
    {
        return _specialType;
    }

    public PartSysParamState CreateState(int particleCount)
    {
        return _state;
    }

    private class State : PartSysParamState
    {
        private readonly PartSysParamSpecialType _type;

        public State(PartSysParamSpecialType type)
        {
            _type = type;
        }

        public override float GetValue(PartSysEmitter emitter, int particleIdx, float lifetimeSec)
        {
            // Returns the radius of the object associated with the emitter
            var obj = emitter.GetAttachedTo();

            if (obj == null)
            {
                return 0; // Fallback value
            }

            if (_type == PartSysParamSpecialType.PSPST_RADIUS)
            {
                return emitter.External.GetObjRadius(obj);
            }
            else
            {
                return 0;
            }
        }
    }
}