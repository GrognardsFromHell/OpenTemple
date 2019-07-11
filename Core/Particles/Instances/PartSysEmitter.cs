using System;
using System.Diagnostics;
using System.Numerics;
using SpicyTemple.Core.Particles.Spec;
using SpicyTemple.Core.Utils;
using SpicyTemple.Particles.Params;

namespace SpicyTemple.Core.Particles.Instances
{
    public class PartSysEmitter : IDisposable
    {
        private static object
            PartSysCurObj; // This is stupid, this is only used for radius determination as far as i can tell

        private object _attachedTo;
        private readonly ParticleState _particleState;
        private float _aliveInSecs;

        private BonesState _boneState; // Only used if space == bones

        private bool _ended; // Indicates that emission has ended but particles may

        private int _firstUsedParticle; // not sure what it is *exactly* yet

        private int _nextFreeParticle; // not sure what it is *exactly* yet
        // still be around

        private Vector3 _objPos; // Current known pos of mAttachedTo
        private float _objRotation; // Current known rotation of mAttachedTo
        private float _outstandingSimulation;
        private readonly PartSysParamState[] _paramState;
        private readonly float[] _particleAges;
        private Vector3 _prevObjPos; // Prev. known pos of mAttachedTo
        private float _prevObjRotation; // Prev. known rotation of mAttachedTo

        private IPartSysEmitterRenderState _renderState;

        private readonly PartSysEmitterSpec _spec;

        private Vector3 _velocity; // Current velocity of this emitter based on previous

        private Vector3 _worldPos;
        // acceleration

        private Vector3 _worldPosVar;

        public PartSysEmitter(IPartSysExternal external, PartSysEmitterSpec spec)
        {
            External = external;
            _spec = spec;
            _particleAges = new float[spec.GetMaxParticles()];
            _paramState = new PartSysParamState[(int) PartSysParamId.PARTICLE_PARAM_COUNT];
            _particleState = new ParticleState(spec.GetMaxParticles());

            for (var i = 0; i < _paramState.Length; ++i)
            {
                var param = _spec.GetParam((PartSysParamId) i);
                if (param != null)
                {
                    _paramState[i] = param.CreateState(_particleAges.Length);
                }
            }
        }

        public IPartSysExternal External { get; }

        public void Dispose()
        {
            _paramState.DisposeAndNull();
        }

        public PartSysEmitterSpec GetSpec()
        {
            return _spec;
        }

        public bool IsDead()
        {
            if (_spec.IsPermanent())
            {
                return false;
            }

            if (_ended && GetActiveCount() == 0)
            {
                return true;
            }

            var result = false;

            // TODO: Here's a check for that ominous "permanent particle" flag
            // It only went into this, if it didn't have that flag, which is very odd
            if (true)
            {
                var lifespanSum = _spec.GetLifespan() + _spec.GetParticleLifespan();
                if (_aliveInSecs >= lifespanSum)
                {
                    result = true;
                }
            }

            return result;
        }

        public int GetActiveCount()
        {
            if (_nextFreeParticle < _firstUsedParticle)
            {
                return _particleAges.Length - _firstUsedParticle + _nextFreeParticle;
            }

            return _nextFreeParticle - _firstUsedParticle;
        }

        public ParticleRange GetActiveRange()
        {
            return new ParticleRange(_firstUsedParticle, _nextFreeParticle);
        }

        public Span<float> GetParticles()
        {
            return _particleAges;
        }

        public PartSysParamState[] GetParamState()
        {
            return _paramState;
        }

        public PartSysParamState GetParamState(PartSysParamId paramId)
        {
            return _paramState[(int) paramId];
        }

        public ParticleIterator NewIterator()
        {
            return new ParticleIterator(_firstUsedParticle, _nextFreeParticle, _particleAges.Length);
        }

        public float GetParamValue(PartSysParamId paramId, int particleIdx, float lifetime,
            float defaultValue = 0.0f)
        {
            var state = GetParamState(paramId);
            if (state != null)
            {
                return state.GetValue(this, particleIdx, lifetime);
            }

            return defaultValue;
        }

        public float GetAliveInSecs()
        {
            return _aliveInSecs;
        }

        public float GetOutstandingSimulation()
        {
            return _outstandingSimulation;
        }

        public object GetAttachedTo()
        {
            return _attachedTo;
        }

        public void SetAttachedTo(object attachedTo)
        {
            _attachedTo = attachedTo;

            // Reinitialize the bone state we're tracking for the object
            if (_spec.GetSpace() == PartSysEmitterSpace.Bones)
            {
                _boneState = null;
                if (attachedTo != null)
                {
                    _boneState = new BonesState(External, attachedTo);
                }
            }

            UpdatePos();
        }

        public Vector3 GetWorldPos()
        {
            return _worldPos;
        }

        public Vector3 GetWorldPosVar()
        {
            return _worldPosVar;
        }

        public Vector3 GetObjPos()
        {
            return _objPos;
        }

        public Vector3 GetPrevObjPos()
        {
            return _prevObjPos;
        }

        public float GetObjRotation()
        {
            return _objRotation;
        }

        public float GetPrevObjRotation()
        {
            return _prevObjRotation;
        }

        public float GetParticleAge(int particleIdx)
        {
            return _particleAges[particleIdx];
        }

        public float GetParticleSpawnTime(int particleIdx)
        {
            return _aliveInSecs - GetParticleAge(particleIdx);
        }

        public ParticleState GetParticleState()
        {
            return _particleState;
        }

        public void SetWorldPos(Vector3 worldPos)
        {
            _worldPos = worldPos;
        }

        public void PruneExpiredParticles()
        {
            // Cull expired particles
            if (_spec.IsPermanentParticles())
            {
                return;
            }

            var it = NewIterator();
            var maxAge = _spec.GetParticleLifespan();
            while (it.HasNext())
            {
                var particleIdx = it.Next();
                if (_particleAges[particleIdx] > maxAge)
                {
                    _firstUsedParticle = particleIdx + 1;
                    if (_firstUsedParticle >= _particleAges.Length)
                    {
                        _firstUsedParticle = 0;
                    }
                }
            }
        }

        public void Reset()
        {
            _aliveInSecs = 0;
            _velocity = Vector3.Zero;
            _prevObjPos = _objPos;
            _prevObjRotation = _objRotation;
            _ended = false;
            _outstandingSimulation = 0;
            _firstUsedParticle = 0;
            _nextFreeParticle = 0;
        }

        public void SimulateEmitterMovement(float timeToSimulateSecs)
        {
            // This really doesn't seem necessary
            if (timeToSimulateSecs == 0)
            {
                return;
            }

            // Move the emitter according to its velocity accumulated from previous acceleration
            _worldPos.X += timeToSimulateSecs * _velocity.X;
            _worldPos.Y += timeToSimulateSecs * _velocity.Y;
            _worldPos.Z += timeToSimulateSecs * _velocity.Z;

            // Apply the acceleration to both the position and velocity
            ApplyAcceleration(PartSysParamId.emit_accel_X, timeToSimulateSecs, ref _worldPos.X, ref _velocity.X);
            ApplyAcceleration(PartSysParamId.emit_accel_Y, timeToSimulateSecs, ref _worldPos.Y, ref _velocity.Y);
            ApplyAcceleration(PartSysParamId.emit_accel_Z, timeToSimulateSecs, ref _worldPos.Z, ref _velocity.Z);

            // This seems to be for constant or keyframe based velocity
            var param = _paramState[(int) PartSysParamId.emit_velVariation_X];
            if (param != null)
            {
                _worldPos.X += GetParamValue(param) * timeToSimulateSecs;
            }

            param = _paramState[(int) PartSysParamId.emit_velVariation_Y];
            if (param != null)
            {
                _worldPos.Y += GetParamValue(param) * timeToSimulateSecs;
            }

            param = _paramState[(int) PartSysParamId.emit_velVariation_Z];
            if (param != null)
            {
                _worldPos.Z += GetParamValue(param) * timeToSimulateSecs;
            }

            // Not sure yet, what pos variation is used for yet
            _worldPosVar = _worldPos;

            param = _paramState[(int) PartSysParamId.emit_posVariation_X];
            if (param != null)
            {
                _worldPosVar.X += GetParamValue(param);
            }

            param = _paramState[(int) PartSysParamId.emit_posVariation_Y];
            if (param != null)
            {
                _worldPosVar.Y += GetParamValue(param);
            }

            param = _paramState[(int) PartSysParamId.emit_posVariation_Z];
            if (param != null)
            {
                _worldPosVar.Z += GetParamValue(param);
            }
        }

        public int ReserveParticle(float particleAge)
        {
            var result = _nextFreeParticle++;

            _particleAges[result] = particleAge;

            // TODO UNKNOWN
            // if (a1.particleParams.flags & 8)
            //	a1.particles[v2] = (long double)(unsigned int)v2 * 0.12327 + a1.particles[v2];

            if (_nextFreeParticle == _spec.GetMaxParticles())
            {
                _nextFreeParticle = 0;
            }

            if (_firstUsedParticle == _nextFreeParticle)
            {
                // The following effectively frees up an existing particle
                _firstUsedParticle++;
                if (_firstUsedParticle == _spec.GetMaxParticles())
                {
                    _firstUsedParticle = 0;
                }
            }

            return result;
        }

        // Regenerate a random number for the new particle for each random parameter
        // ONLY if this emitter is permanent. Probable reason: For permanent emitter's
        // particle slots may be reused, while for non-permanent emitters they are
        // not reused.
        public void RefreshRandomness(int particleIdx)
        {
            if (_spec.IsPermanent())
            {
                foreach (var state in _paramState)
                {
                    if (state != null)
                    {
                        state.InitParticle(particleIdx);
                    }
                }
            }
        }

        public void Simulate(float timeToSimulateSecs)
        {
            UpdatePos();

            PartSysSimulation.SimulateParticleAging(this, timeToSimulateSecs);
            PartSysSimulation.SimulateParticleMovement(this, timeToSimulateSecs);

            // Emitter already dead or lifetime expired?
            if (_ended || !_spec.IsPermanent() && _aliveInSecs > _spec.GetLifespan())
            {
                _aliveInSecs += timeToSimulateSecs;
                return;
            }

            // Particle spawning logic
            if (_spec.IsInstant())
            {
                // The secondary rate seem to be the "minimum" particles that circumvent the fidelity setting?
                // Also note how the "max particles" count is used here instead of the rate as it is below
                var scaledMaxParts = (int) (_spec.GetParticleRateMin() +
                                            (_spec.GetMaxParticles() - _spec.GetParticleRateMin()) *
                                            External.GetParticleFidelity());

                if (scaledMaxParts > 0)
                {
                    // The time here is probably only the smallest greater than 0 since there's a
                    // check in there that skips simulation if the time is zero
                    SimulateEmitterMovement(0.0001f);

                    // We fake spreading out the spawning over 1second equally for all particles
                    // If there's just one particle, this is "NaN", but it doesnt matter
                    _aliveInSecs = 0;
                    var timeStep = 1.0f / (scaledMaxParts - 1);
                    var remaining = scaledMaxParts - 1;
                    if (remaining > 0)
                    {
                        do
                        {
                            _aliveInSecs += timeStep;

                            var particleIdx = ReserveParticle(0.0f);

                            RefreshRandomness(particleIdx);

                            PartSysSimulation.SimulateParticleSpawn(External, this, particleIdx, timeToSimulateSecs);
                            --remaining;
                        } while (remaining != 0);
                    }

                    _aliveInSecs = 0;
                    _aliveInSecs = timeToSimulateSecs;
                    return;
                }

                // Set the emitter past it's lifespan to prevent this logic from being active again
                _aliveInSecs = _spec.GetLifespan() + 1.0f;
                return;
            }

            // Scale the particle rate according to the fidelity setting
            var partsPerSec = _spec.GetEffectiveParticleRate(External.GetParticleFidelity());

            // If this emitter will not emit anything in 1000 seconds,
            // because of the fidelity setting, simply set it to end
            if (partsPerSec <= 0.001f)
            {
                _aliveInSecs = _spec.GetLifespan() + 1.0f;
                return;
            }

            _outstandingSimulation += timeToSimulateSecs;
            _aliveInSecs += timeToSimulateSecs;

            // Calculate how many seconds go by until the emitter spawns
            // another particle
            var secsPerPart = 1.0f / partsPerSec;

            while (_outstandingSimulation >= secsPerPart)
            {
                _outstandingSimulation -= secsPerPart;

                // Simulate emitter movement just for the interval between two particle spawns
                SimulateEmitterMovement(secsPerPart);

                var particleIdx = ReserveParticle(_outstandingSimulation);

                RefreshRandomness(particleIdx);

                PartSysSimulation.SimulateParticleSpawn(External, this, particleIdx, timeToSimulateSecs);
            }
        }

        public void EndPrematurely()
        {
            _ended = true;
        }

        public void SetRenderState(IPartSysEmitterRenderState renderState)
        {
            _renderState = renderState;
        }

        public bool HasRenderState()
        {
            return _renderState != null;
        }

        public IPartSysEmitterRenderState GetRenderState()
        {
            Trace.Assert(HasRenderState());
            return _renderState;
        }

        public BonesState GetBoneState()
        {
            return _boneState;
        }

        private float GetParamValue(PartSysParamState state, int particleIdx = 0)
        {
            return state.GetValue(this, particleIdx, _aliveInSecs);
        }

        private void UpdatePos()
        {
            PartSysCurObj = _attachedTo;

            // The position only needs to be updated if we're attached to an object
            if (_attachedTo == null)
            {
                return;
            }

            switch (_spec.GetSpace())
            {
                // We are attached to one of the bones of the object
                case PartSysEmitterSpace.NodePos:
                case PartSysEmitterSpace.NodeYpr:
                    _prevObjPos = _objPos;
                    _prevObjRotation = _objRotation;
                    External.GetObjRotation(_attachedTo, out _objRotation);

                    if (External.GetBoneWorldMatrix(_attachedTo, _spec.GetNodeName(), out var boneMatrix))
                    {
                        _objPos = boneMatrix.Translation;
                    }
                    else
                    {
                        // As a fallback we use the object's location
                        External.GetObjLocation(_attachedTo, out _objPos);
                    }

                    break;

                // We're attached to the world position of the object
                case PartSysEmitterSpace.ObjectPos:
                case PartSysEmitterSpace.ObjectYpr:
                    _prevObjPos = _objPos;
                    External.GetObjLocation(_attachedTo, out _objPos);
                    // The rotation is only relevant when we're in OBJECT_YPR space
                    if (_spec.GetSpace() == PartSysEmitterSpace.ObjectYpr)
                    {
                        _prevObjRotation = _objRotation;
                        External.GetObjRotation(_attachedTo, out _objRotation);
                    }

                    break;

                case PartSysEmitterSpace.Bones:
                    if (_boneState == null && _attachedTo != null)
                    {
                        _boneState = new BonesState(External, _attachedTo);
                    }

                    _boneState?.UpdatePos();
                    break;
            }
        }

        private void ApplyAcceleration(PartSysParamId paramId, float timeToSimulateSecs,
            ref float position, ref float velocity)
        {
            var paramState = _paramState[(int) paramId];
            if (paramState != null)
            {
                var accel = GetParamValue(paramState); // Get value from param state
                position += timeToSimulateSecs * timeToSimulateSecs * accel * 0.5f;
                velocity += timeToSimulateSecs * accel;
            }
        }
    }
}