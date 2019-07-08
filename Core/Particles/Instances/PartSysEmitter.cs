using System;
using System.Diagnostics;
using System.Numerics;
using SpicyTemple.Core.Particles.Spec;
using SpicyTemple.Core.Utils;
using SpicyTemple.Particles;
using SpicyTemple.Particles.Params;

namespace SpicyTemple.Core.Particles.Instances
{
    public class PartSysEmitter : IDisposable
    {

        private readonly IPartSysExternal _external;

        public IPartSysExternal External => _external;

        public PartSysEmitter(IPartSysExternal external, PartSysEmitterSpec spec)
        {
            _external = external;
            mSpec = spec;
            mParticleAges = new float[spec.GetMaxParticles()];
            mParamState = new PartSysParamState[(int) PartSysParamId.PARTICLE_PARAM_COUNT];
            mParticleState = new ParticleState(spec.GetMaxParticles());

            for (int i = 0; i < mParamState.Length; ++i) {
                var param = mSpec.GetParam((PartSysParamId)i);
                if (param != null) {
                    mParamState[i] = param.CreateState(mParticleAges.Length);
                }
            }
        }

        public void Dispose()
        {
            mParamState.DisposeAndNull();
        }

        public PartSysEmitterSpec GetSpec() { return mSpec; }

        public bool IsDead()
        {
            if (mSpec.IsPermanent()) {
                return false;
            }

            if (mEnded && GetActiveCount() == 0) {
                return true;
            }

            var result = false;

            // TODO: Here's a check for that ominous "permanent particle" flag
            // It only went into this, if it didn't have that flag, which is very odd
            if (true) {
                var lifespanSum = mSpec.GetLifespan() + mSpec.GetParticleLifespan();
                if (mAliveInSecs >= lifespanSum)
                    result = true;
            }

            return result;
        }

        public int GetActiveCount() {
            if (mNextFreeParticle < mFirstUsedParticle) {
                return (mParticleAges.Length - mFirstUsedParticle) + mNextFreeParticle;
            } else {
                return mNextFreeParticle - mFirstUsedParticle;
            }
        }

        public ParticleRange GetActiveRange() {
            return new ParticleRange(mFirstUsedParticle, mNextFreeParticle);
        }

        public Span<float> GetParticles() { return mParticleAges; }

        public PartSysParamState[] GetParamState() {
            return mParamState;
        }

        public PartSysParamState GetParamState(PartSysParamId paramId) {
            return mParamState[(int) paramId];
        }

        public ParticleIterator NewIterator() {
            return new ParticleIterator(mFirstUsedParticle, mNextFreeParticle, mParticleAges.Length);
        }

        public float GetParamValue(PartSysParamId paramId, int particleIdx, float lifetime,
            float defaultValue = 0.0f) {
            var state = GetParamState(paramId);
            if (state != null) {
                return state.GetValue(this, particleIdx, lifetime);
            } else {
                return defaultValue;
            }
        }

        public float GetAliveInSecs() { return mAliveInSecs; }

        public float GetOutstandingSimulation() { return mOutstandingSimulation; }

        public object GetAttachedTo() { return mAttachedTo; }

        public void SetAttachedTo(object attachedTo)
        {
            mAttachedTo = attachedTo;

            // Reinitialize the bone state we're tracking for the object
            if (mSpec.GetSpace() == PartSysEmitterSpace.Bones)
            {
                mBoneState = null;
                if (attachedTo != null) {
                    mBoneState = new BonesState(_external, attachedTo);
                }
            }

            UpdatePos();
        }

        public Vector3 GetWorldPos() { return mWorldPos; }

        public Vector3 GetWorldPosVar() { return mWorldPosVar; }

        public Vector3 GetObjPos() { return mObjPos; }

        public Vector3 GetPrevObjPos() { return mPrevObjPos; }

        public float GetObjRotation() { return mObjRotation; }

        public float GetPrevObjRotation() { return mPrevObjRotation; }

        public float GetParticleAge(int particleIdx) {
            return mParticleAges[particleIdx];
        }

        public float GetParticleSpawnTime(int particleIdx) {
            return mAliveInSecs - GetParticleAge(particleIdx);
        }

        public ParticleState GetParticleState() { return mParticleState; }

        public void SetWorldPos(Vector3 worldPos)
        {
            mWorldPos = worldPos;
        }

        public void PruneExpiredParticles()
        {
            // Cull expired particles
            if (mSpec.IsPermanentParticles()) {
                return;
            }

            var it = NewIterator();
            var maxAge = mSpec.GetParticleLifespan();
            while (it.HasNext()) {
                var particleIdx = it.Next();
                if (mParticleAges[particleIdx] > maxAge) {
                    mFirstUsedParticle = particleIdx + 1;
                    if (mFirstUsedParticle >= (int) mParticleAges.Length) {
                        mFirstUsedParticle = 0;
                    }
                }
            }
        }

        public void Reset()
        {
            mAliveInSecs = 0;
            mVelocity = Vector3.Zero;
            mPrevObjPos = mObjPos;
            mPrevObjRotation = mObjRotation;
            mEnded = false;
            mOutstandingSimulation = 0;
            mFirstUsedParticle = 0;
            mNextFreeParticle = 0;
        }

        public void SimulateEmitterMovement(float timeToSimulateSecs)
        {
            // This really doesn't seem necessary
            if (timeToSimulateSecs == 0) {
                return;
            }

            // Move the emitter according to its velocity accumulated from previous acceleration
            mWorldPos.X += timeToSimulateSecs * mVelocity.X;
            mWorldPos.Y += timeToSimulateSecs * mVelocity.Y;
            mWorldPos.Z += timeToSimulateSecs * mVelocity.Z;

            // Apply the acceleration to both the position and velocity
            ApplyAcceleration(PartSysParamId.emit_accel_X, timeToSimulateSecs, ref mWorldPos.X, ref mVelocity.X);
            ApplyAcceleration(PartSysParamId.emit_accel_Y, timeToSimulateSecs, ref mWorldPos.Y, ref mVelocity.Y);
            ApplyAcceleration(PartSysParamId.emit_accel_Z, timeToSimulateSecs, ref mWorldPos.Z, ref mVelocity.Z);

            // This seems to be for constant or keyframe based velocity
            var param = mParamState[(int)PartSysParamId.emit_velVariation_X];
            if (param != null) {
                mWorldPos.X += GetParamValue(param) * timeToSimulateSecs;
            }
            param = mParamState[(int)PartSysParamId.emit_velVariation_Y];
            if (param != null) {
                mWorldPos.Y += GetParamValue(param) * timeToSimulateSecs;
            }
            param = mParamState[(int)PartSysParamId.emit_velVariation_Z];
            if (param != null) {
                mWorldPos.Z += GetParamValue(param) * timeToSimulateSecs;
            }

            // Not sure yet, what pos variation is used for yet
            mWorldPosVar = mWorldPos;

            param = mParamState[(int)PartSysParamId.emit_posVariation_X];
            if (param != null) {
                mWorldPosVar.X += GetParamValue(param);
            }
            param = mParamState[(int)PartSysParamId.emit_posVariation_Y];
            if (param != null) {
                mWorldPosVar.Y += GetParamValue(param);
            }
            param = mParamState[(int)PartSysParamId.emit_posVariation_Z];
            if (param != null) {
                mWorldPosVar.Z += GetParamValue(param);
            }
        }

        public int ReserveParticle(float particleAge)
        {
            var result = mNextFreeParticle++;

            mParticleAges[result] = particleAge;

            // TODO UNKNOWN
            // if (a1.particleParams.flags & 8)
            //	a1.particles[v2] = (long double)(unsigned int)v2 * 0.12327 + a1.particles[v2];

            if (mNextFreeParticle == mSpec.GetMaxParticles())
                mNextFreeParticle = 0;

            if (mFirstUsedParticle == mNextFreeParticle) {
                // The following effectively frees up an existing particle
                mFirstUsedParticle++;
                if (mFirstUsedParticle == mSpec.GetMaxParticles())
                    mFirstUsedParticle = 0;
            }

            return result;
        }

        // Regenerate a random number for the new particle for each random parameter
        // ONLY if this emitter is permanent. Probable reason: For permanent emitter's
        // particle slots may be reused, while for non-permanent emitters they are
        // not reused.
        public void RefreshRandomness(int particleIdx)
        {
            if (mSpec.IsPermanent()) {
                foreach (var state  in  mParamState) {
                    if (state != null) {
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
		    if (mEnded || (!mSpec.IsPermanent() && mAliveInSecs > mSpec.GetLifespan())) {
			    mAliveInSecs += timeToSimulateSecs;
			    return;
		    }

		    // Particle spawning logic
		    if (mSpec.IsInstant()) {
			    // The secondary rate seem to be the "minimum" particles that circumvent the fidelity setting?
			    // Also note how the "max particles" count is used here instead of the rate as it is below
			    int scaledMaxParts = (int)(mSpec.GetParticleRateMin() +
				    (mSpec.GetMaxParticles() - mSpec.GetParticleRateMin()) * _external.GetParticleFidelity());

			    if (scaledMaxParts > 0) {
				    // The time here is probably only the smallest greater than 0 since there's a
				    // check in there that skips simulation if the time is zero
				    SimulateEmitterMovement(0.0001f);

				    // We fake spreading out the spawning over 1second equally for all particles
				    // If there's just one particle, this is "NaN", but it doesnt matter
				    mAliveInSecs = 0;
				    var timeStep = 1.0f / (float)(scaledMaxParts - 1);
				    int remaining = scaledMaxParts - 1;
				    if (remaining > 0) {
					    do {
						    mAliveInSecs += timeStep;

						    var particleIdx = ReserveParticle(0.0f);

						    RefreshRandomness(particleIdx);

                            PartSysSimulation.SimulateParticleSpawn(_external, this, particleIdx, timeToSimulateSecs);
						    --remaining;
					    } while (remaining != 0);
				    }
				    mAliveInSecs = 0;
				    mAliveInSecs = timeToSimulateSecs;
				    return;
			    }

			    // Set the emitter past it's lifespan to prevent this logic from being active again
			    mAliveInSecs = mSpec.GetLifespan() + 1.0f;
			    return;
		    }

		    // Scale the particle rate according to the fidelity setting
		    var partsPerSec = mSpec.GetEffectiveParticleRate(_external.GetParticleFidelity());

		    // If this emitter will not emit anything in 1000 seconds,
		    // because of the fidelity setting, simply set it to end
		    if (partsPerSec <= 0.001f) {
			    mAliveInSecs = mSpec.GetLifespan() + 1.0f;
			    return;
		    }

		    mOutstandingSimulation += timeToSimulateSecs;
		    mAliveInSecs += timeToSimulateSecs;

		    // Calculate how many seconds go by until the emitter spawns
		    // another particle
		    var secsPerPart = 1.0f / partsPerSec;

		    while (mOutstandingSimulation >= secsPerPart) {
			    mOutstandingSimulation -= secsPerPart;

			    // Simulate emitter movement just for the interval between two particle spawns
			    SimulateEmitterMovement(secsPerPart);

			    var particleIdx = ReserveParticle(mOutstandingSimulation);

			    RefreshRandomness(particleIdx);

                PartSysSimulation.SimulateParticleSpawn(_external, this, particleIdx, timeToSimulateSecs);
		    }

        }

        public void EndPrematurely()
        {
            mEnded = true;
        }

        static object PartSysCurObj; // This is stupid, this is only used for radius determination as far as i can tell

        public void SetRenderState(IPartSysEmitterRenderState renderState) {
            mRenderState = renderState;
        }
        public bool HasRenderState() { return mRenderState != null; }
        public IPartSysEmitterRenderState GetRenderState() {
            Trace.Assert(HasRenderState());
            return mRenderState;
        }

        public BonesState GetBoneState() {
            return mBoneState;
        }

        private PartSysEmitterSpec mSpec;
        private float[] mParticleAges;
        private PartSysParamState[] mParamState;
        private ParticleState mParticleState;
        private float mAliveInSecs = 0;
        private float mOutstandingSimulation = 0;
        private object mAttachedTo = null;
        private Vector3 mWorldPos;
        private bool mEnded = false; // Indicates that emission has ended but particles may
        // still be around

        private Vector3 mObjPos;               // Current known pos of mAttachedTo
        private Vector3 mPrevObjPos;           // Prev. known pos of mAttachedTo
        private float mObjRotation = 0;     // Current known rotation of mAttachedTo
        private float mPrevObjRotation = 0; // Prev. known rotation of mAttachedTo

        private Vector3 mVelocity; // Current velocity of this emitter based on previous
        // acceleration

        private Vector3 mWorldPosVar;

        private int mFirstUsedParticle = 0; // not sure what it is *exactly* yet
        private int mNextFreeParticle = 0;  // not sure what it is *exactly* yet

        private BonesState mBoneState; // Only used if space == bones

        private IPartSysEmitterRenderState mRenderState;

        private float GetParamValue(PartSysParamState state, int particleIdx = 0)
        {
            return state.GetValue(this, particleIdx, mAliveInSecs);
        }

        private void UpdatePos()
        {

            PartSysCurObj = mAttachedTo;

            // The position only needs to be updated if we're attached to an object
            if (mAttachedTo == null) {
                return;
            }

            switch (mSpec.GetSpace()) {
                // We are attached to one of the bones of the object
                case PartSysEmitterSpace.NodePos:
                case PartSysEmitterSpace.NodeYpr:
                    mPrevObjPos = mObjPos;
                    mPrevObjRotation = mObjRotation;
                    _external.GetObjRotation(mAttachedTo, out mObjRotation);

                    if (_external.GetBoneWorldMatrix(mAttachedTo, mSpec.GetNodeName(),out var boneMatrix)) {
                        mObjPos = boneMatrix.Translation;
                    } else {
                        // As a fallback we use the object's location
                        _external.GetObjLocation(mAttachedTo, out mObjPos);
                    }
                    break;

                // We're attached to the world position of the object
                case PartSysEmitterSpace.ObjectPos:
                case PartSysEmitterSpace.ObjectYpr:
                    mPrevObjPos = mObjPos;
                    _external.GetObjLocation(mAttachedTo, out mObjPos);
                    // The rotation is only relevant when we're in OBJECT_YPR space
                    if (mSpec.GetSpace() == PartSysEmitterSpace.ObjectYpr) {
                        mPrevObjRotation = mObjRotation;
                        _external.GetObjRotation(mAttachedTo, out mObjRotation);
                    }
                    break;

                case PartSysEmitterSpace.Bones:
                    if (mBoneState == null && mAttachedTo != null) {
                        mBoneState = new BonesState(_external, mAttachedTo);
                    }

                    mBoneState?.UpdatePos();
                    break;
                default:
                    break;
            }
        }

        private void ApplyAcceleration(PartSysParamId paramId, float timeToSimulateSecs,
            ref float position, ref float velocity)
        {
            var paramState = mParamState[(int) paramId];
            if (paramState != null) {
                float accel = GetParamValue(paramState); // Get value from param state
                position += timeToSimulateSecs * timeToSimulateSecs * accel * 0.5f;
                velocity += timeToSimulateSecs * accel;
            }
        }
    }
}