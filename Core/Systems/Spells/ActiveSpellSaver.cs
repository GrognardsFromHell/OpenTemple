using System.Linq;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO.SaveGames.GameState;

namespace SpicyTemple.Core.Systems.Spells
{
    public static class ActiveSpellSaver
    {
        public static SavedActiveSpell SaveActiveSpell(SpellPacketBody spellPacket, bool isActive)
        {
            var result = new SavedActiveSpell
            {
                Id = spellPacket.spellId,
                IsActive = isActive,
                SpellEnum = spellPacket.spellEnum,
                SpellEnumOriginal = spellPacket.spellEnumOriginal,
                AnimFlags = spellPacket.animFlags,
                CasterId = spellPacket.caster?.id ?? ObjectId.CreateNull(),
                CasterPartSysHash = GetPartSysNameHash(spellPacket.casterPartSys),
                ClassCode = spellPacket.spellClass,
                SpellLevel = spellPacket.spellKnownSlotLevel,
                CasterLevel = spellPacket.casterLevel,
                DC = spellPacket.dc,
                AoEObjectId = spellPacket.aoeObj?.id ?? ObjectId.CreateNull(),
                SpellObjects = spellPacket.spellObjs
                    .Select(spellObj => (spellObj.obj.id, GetPartSysNameHash(spellObj.partSys)))
                    .ToList(),
                Targets = spellPacket.Targets
                    .Select(target => (target.Object.id, GetPartSysNameHash(target.ParticleSystem)))
                    .ToList(),
                InitialTargets = spellPacket.InitialTargets.Select(obj => obj.id).ToHashSet(),
                Projectiles = spellPacket.projectiles.Select(p => p.id).ToList(),
                AoECenter = spellPacket.aoeCenter,
                AoECenterZ = spellPacket.aoeCenterZ,
                Duration = spellPacket.duration,
                DurationRemaining = spellPacket.durationRemaining,
                SpellRange = spellPacket.spellRange,
                SavingThrowResult = spellPacket.savingThrowResult,
                MetaMagic = spellPacket.metaMagicData
            };

            return result;
        }

        private static int GetPartSysNameHash(object partSys)
        {
            return partSys == null ? 0 : GameSystems.ParticleSys.GetNameHash(partSys);
        }
    }
}