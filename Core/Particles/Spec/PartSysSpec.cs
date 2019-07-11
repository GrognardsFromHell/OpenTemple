using System.Collections.Generic;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Particles.Spec
{
    public class PartSysSpec
    {
        private readonly string _name;
        private readonly int _nameHash; // Cached ELF-hash of the name
        private readonly List<PartSysEmitterSpec> _emitters = new List<PartSysEmitterSpec>();

        public PartSysSpec(string name)
        {
            _name = name;
            _nameHash = ElfHash.Hash(name.ToLowerInvariant());
        }

        /// The name of the particle system
        public string GetName()
        {
            return _name;
        }

        /// The ELF32 Hash of the name
        public int GetNameHash()
        {
            return _nameHash;
        }

        /// All declared emitters in this particle system spec
        public IReadOnlyList<PartSysEmitterSpec> GetEmitters()
        {
            return _emitters;
        }

        public PartSysEmitterSpec CreateEmitter(string name)
        {
            var emitter = new PartSysEmitterSpec(this, name);
            _emitters.Add(emitter);
            return emitter;
        }
    }
}