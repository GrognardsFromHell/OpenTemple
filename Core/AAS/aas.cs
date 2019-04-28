using System;
using System.Collections.Generic;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.AAS
{
    public struct AasAnimParams
    {
        public uint flags;
        public uint unknown;
        public ulong locX;
        public ulong locY;
        public float offsetX;
        public float offsetY;
        public float offsetZ;
        public float rotation;
        public float scale;
        public float rotationRoll;
        public float rotationPitch;
        public float rotationYaw;
        public AasHandle parentAnim;
        public string attachedBoneName;
    }

    public class AnimEvents
    {
        /**
         * Indicates the animation has ended.
         */
        public bool end;

        /**
         * Indicates that the frame on which an action should connect (weapon swings, etc.)
         * has occurred.
         */
        public bool action;
    }

    public struct AasMaterial
    {
        private object _material;

        public MaterialPlaceholderSlot? Slot { get; }

        public object Material
        {
            get => _material;
            set
            {
                if (_material is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                _material = value;
            }
        }

        public AasMaterial(MaterialPlaceholderSlot? slot, object material)
        {
            Slot = slot;
            _material = material;
        }

        public bool Equals(AasMaterial other)
        {
            return Equals(_material, other._material) && Slot == other.Slot;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is AasMaterial other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_material != null ? _material.GetHashCode() : 0) * 397) ^ Slot.GetHashCode();
            }
        }

        public static bool operator ==(AasMaterial left, AasMaterial right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AasMaterial left, AasMaterial right)
        {
            return !left.Equals(right);
        }
    }

    public interface IMaterialResolver
    {
        AasMaterial Acquire(ReadOnlySpan<char> materialName, ReadOnlySpan<char> context);
        void Release(AasMaterial material, ReadOnlySpan<char> context);

        bool IsMaterialPlaceholder(AasMaterial material);
        MaterialPlaceholderSlot GetMaterialPlaceholderSlot(AasMaterial material);
    }

    internal class ActiveModel : IDisposable
    {
        public readonly AasHandle handle;
        public EncodedAnimId animId = new EncodedAnimId(WeaponAnim.None);
        public float floatconst = 6.3940001f;
        public TimePoint timeLoaded = TimePoint.Now;
        public AnimatedModel model;
        public Mesh mesh;
        public Skeleton skeleton;
        public List<Mesh> additionalMeshes;

        public ActiveModel(AasHandle handle)
        {
            this.handle = handle;
        }

        public void Dispose()
        {
        }
    }

    public delegate void ScriptInterpreter(string script);


    internal class EventHandler : IAnimEventHandler
    {
        private readonly ScriptInterpreter _scriptInterpreter;

        private AnimEvents flagsOut_ = null;

        public EventHandler(ScriptInterpreter scriptInterpreter)
        {
            _scriptInterpreter = scriptInterpreter;
        }

        public void SetFlagsOut(AnimEvents flagsOut)
        {
            flagsOut_ = flagsOut;
        }

        public void ClearFlagsOut()
        {
            flagsOut_ = null;
        }

        public void HandleEvent(int frame, float frameTime, AnimEventType type, string args)
        {
            switch (type)
            {
                case AnimEventType.Action:
                    if (flagsOut_ != null)
                    {
                        flagsOut_.action = true;
                    }

                    break;
                case AnimEventType.End:
                    if (flagsOut_ != null)
                    {
                        flagsOut_.end = true;
                    }

                    break;
                case AnimEventType.Script:
                    _scriptInterpreter?.Invoke(args);
                    break;
            }
        }
    }
}