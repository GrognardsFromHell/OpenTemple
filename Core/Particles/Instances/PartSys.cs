using System;
using System.Collections.Generic;
using System.Numerics;
using SpicyTemple.Core.Particles.Spec;
using SpicyTemple.Particles;

namespace SpicyTemple.Core.Particles.Instances
{
    public class PartSys
    {
        private static int _idSequence;
        private readonly IPartSysExternal _external;
        private float _aliveInSecs;
        private object _attachedTo;
        private readonly List<PartSysEmitter> _emitters;
        private int _id = _idSequence++;
        private float _lastSimulated; // Also in Secs since creation
        private readonly Box2d _screenBounds;
        private Vector2 _screenPosAbs;
        private readonly PartSysSpec _spec;

        public PartSys(IPartSysExternal external, PartSysSpec spec)
        {
            _external = external;
            _spec = spec;
            _emitters = new List<PartSysEmitter>(spec.GetEmitters().Count);
            // Instantiate the emitters
            for (var i = 0; i < _spec.GetEmitters().Count; ++i)
            {
                var emitterSpec = spec.GetEmitters()[i];
                _emitters.Add(new PartSysEmitter(external, emitterSpec));

                // Update the screen bounds
                _screenBounds.left = MathF.Min(_screenBounds.left, emitterSpec.GetBoxLeft());
                _screenBounds.top = MathF.Min(_screenBounds.top, emitterSpec.GetBoxTop());
                _screenBounds.right = MathF.Max(_screenBounds.right, emitterSpec.GetBoxRight());
                _screenBounds.bottom = MathF.Max(_screenBounds.bottom, emitterSpec.GetBoxBottom());
            }
        }

        public float GetAliveInSecs()
        {
            return _aliveInSecs;
        }

        public void SetAliveInSecs(float aliveInSecs)
        {
            _aliveInSecs = aliveInSecs;
        }

        public float GetLastSimulated()
        {
            return _lastSimulated;
        }

        public void SetLastSimulated(float lastSimulated)
        {
            _lastSimulated = lastSimulated;
        }

        public bool IsDead()
        {
            foreach (var emitter in _emitters)
            {
                if (!emitter.IsDead(_aliveInSecs))
                {
                    return false;
                }
            }

            return true;
        }

        public void Simulate(float elapsedSecs)
        {
            // This ensures that the time for the particle system advances even if we don't
            // simulate because it is off screen
            _aliveInSecs += elapsedSecs;

            // If for whatever reason this method is called multiple times in succession,
            // don't act on it
            if (_aliveInSecs == _lastSimulated)
            {
                return;
            }

            UpdateObjBoundingBox();

            if (!IsOnScreen() || _emitters.Count == 0)
            {
                return; // Don't simulate while off-screen or having no emitters
            }

            // This is a pretty poor method if heuristically determining
            // whether this particle system is permanent
            var permanent = _emitters[0].GetSpec().IsPermanent();

            // Previously, permanent systems were not being simulated while
            // being offscreen, for which i dont really see a reason since
            // there i an off-screen check above.

            // Even if the particle system is offscreen, we advance "mAliveInSecs"
            // so this ensures that if the particle system has been off screen for 2
            // seconds, those two seconds will be simulated once it is on screen again
            var secsToSimulate = _aliveInSecs - _lastSimulated;
            _lastSimulated = _aliveInSecs;

            foreach (var emitter in _emitters)
            {
                var simForEmitter = secsToSimulate;
                if (emitter.GetSpec().IsPermanent())
                {
                    // If the emitter is permanent, simulate at most 0.5s
                    simForEmitter = MathF.Min(0.5f, simForEmitter);
                }

                // Emitters with a delay are not simulated until the particle system
                // has been alive longer than the delay
                if (emitter.GetSpec().GetDelay() > _aliveInSecs)
                {
                    continue;
                }

                emitter.Simulate(simForEmitter);
            }
        }

        public object GetAttachedTo()
        {
            return _attachedTo;
        }

        public void SetAttachedTo(object attachedTo)
        {
            _attachedTo = attachedTo;

            foreach (var emitter in _emitters)
            {
                emitter.SetAttachedTo(attachedTo);
            }
        }

        public void SetWorldPos(float x, float y, float z)
        {
            var worldPos = new Vector3(x, y, z);
            foreach (var emitter in _emitters)
            {
                if (emitter.GetSpec().GetSpace() == PartSysEmitterSpace.World)
                {
                    emitter.SetWorldPos(worldPos);
                }
            }

            _attachedTo = null;
            UpdateScreenBoundingBox(worldPos);
        }

        public PartSysSpec GetSpec()
        {
            return _spec;
        }

        public IReadOnlyList<PartSysEmitter> GetEmitters()
        {
            return _emitters;
        }

        public PartSysEmitter GetEmitter(int emitterIdx)
        {
            return _emitters[emitterIdx];
        }

        public int GetEmitterCount()
        {
            return _emitters.Count;
        }

        public void EndPrematurely()
        {
            foreach (var emitter in _emitters)
            {
                emitter.EndPrematurely();
            }
        }

        public Box2d GetScreenBounds()
        {
            return _screenBounds;
        }

        public Vector2 GetScreenPosAbs()
        {
            return _screenPosAbs;
        }

        public void Reset()
        {
            _aliveInSecs = 0.0f;
            _lastSimulated = 0.0f;

            foreach (var emitter in _emitters)
            {
                emitter.Reset();
            }
        }

        /*
          Does this particle system's bounding box intersect
          the screen?
  */
        private bool IsOnScreen()
        {
            return _external.IsBoxVisible(_screenPosAbs, _screenBounds);
        }

        /*
          Move this particle system's bounding box according
          to the current position of the object it's attached
          to. Does nothing if the system is unattached.
  */
        private void UpdateObjBoundingBox()
        {
            if (_attachedTo != null && _external.GetObjLocation(_attachedTo, out var pos))
            {
                UpdateScreenBoundingBox(pos);
            }
        }

        /*
          Updates the screen bounding box of this particle system
          to have the correct absolute coordinates accounting
          for the given world position.
        */
        private void UpdateScreenBoundingBox(Vector3 worldPos)
        {
            // Where is the obj center in screen coords?
            _external.WorldToScreen(worldPos, out _screenPosAbs);
        }
    }
}