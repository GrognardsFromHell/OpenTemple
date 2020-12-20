using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Ui.InGameSelect;

namespace OpenTemple.Core.Systems.Spells
{
    public struct SpellObj
    {
        public GameObjectBody obj;
        public object partSys;
    }

    [Flags]
    public enum SpellAnimationFlag
    {
        SAF_UNK8 = 0x8,
        SAF_ID_ATTEMPTED = 0x10,
        SAF_UNK20 = 0x20
    }

    public class SpellTarget
    {
        public GameObjectBody Object { get; set; }
        private object _particleSystem;

        public object ParticleSystem
        {
            get => _particleSystem;
            set
            {
                if (_particleSystem != null)
                {
                    GameSystems.ParticleSys.End(_particleSystem);
                }

                _particleSystem = value;
            }
        }

        public SpellTarget(GameObjectBody obj, object particleSystem)
        {
            Object = obj;
            _particleSystem = particleSystem;
        }
    }

    public class SpellPacketBody
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public const int INV_IDX_INVALID = -1;

        public int spellEnum;
        public int spellEnumOriginal; // used for spontaneous casting in order to debit the "original" spell
        public SpellAnimationFlag animFlags; // See SpellAnimationFlag
        public object pSthg;
        public GameObjectBody caster;
        public object casterPartSys;
        public int spellClass; // aka spellClass
        public int spellKnownSlotLevel; // aka spellLevel
        public int casterLevel;
        public int dc;
        public int numSpellObjs => spellObjs.Length;

        public bool IsCastFromItem => invIdx != INV_IDX_INVALID;

        public GameObjectBody aoeObj;
        public SpellObj[] spellObjs = Array.Empty<SpellObj>();

        /// <summary>
        /// The originally selected targets when the spell was cast.
        /// </summary>
        public ISet<GameObjectBody> InitialTargets = ImmutableHashSet<GameObjectBody>.Empty;

        public SpellTarget[] Targets = Array.Empty<SpellTarget>();
        public uint field_9C4;
        public GameObjectBody[] projectiles = Array.Empty<GameObjectBody>();
        public LocAndOffsets aoeCenter;
        public float aoeCenterZ; // TODO consolidate with aoeCenter

        public uint field_A04;

        public PickerResult pickerResult;
        public int duration;
        public int durationRemaining;
        public int spellRange;
        public bool savingThrowResult;

        // inventory index, used for casting spells from items e.g. scrolls; it is 0xFF for non-item spells
        public int invIdx;

        public MetaMagicData metaMagicData;
        public int spellId;
        public uint field_AE4;

        public SpellPacketBody()
        {
            Reset();
        }

        [TempleDllLocation(0x1008A350)]
        private void Reset()
        {
            spellId = 0;
            spellEnum = 0;
            spellEnumOriginal = 0;
            caster = null;
            if (casterPartSys != null)
            {
                GameSystems.ParticleSys.End(casterPartSys);
                casterPartSys = null;
            }

            casterLevel = 0;
            dc = 0;
            animFlags = 0;
            aoeCenter = LocAndOffsets.Zero;
            aoeCenterZ = 0;
            foreach (var spellTarget in Targets)
            {
                if (spellTarget.ParticleSystem != null)
                {
                    GameSystems.ParticleSys.End(spellTarget.ParticleSystem);
                    spellTarget.ParticleSystem = null;
                }
            }

            Targets = Array.Empty<SpellTarget>();
            duration = 0;
            durationRemaining = 0;
            metaMagicData = new MetaMagicData();
            spellClass = 0;
            spellKnownSlotLevel = 0;

            projectiles = Array.Empty<GameObjectBody>();

            // TODO this.orgTargetListNumItems = 0;

            aoeObj = null;

            spellObjs = Array.Empty<SpellObj>();

            spellRange = 0;
            savingThrowResult = false;
            invIdx = INV_IDX_INVALID;

            pickerResult = new PickerResult();
        }

        public bool IsVancian()
        {
            if (GameSystems.Spell.IsDomainSpell(spellClass))
                return true;

            if (D20ClassSystem.IsVancianCastingClass(GameSystems.Spell.GetCastingClass(spellClass)))
                return true;

            return false;
        }

        public bool IsDivine()
        {
            if (GameSystems.Spell.IsDomainSpell(spellClass))
                return true;
            var castingClass = GameSystems.Spell.GetCastingClass(spellClass);

            if (D20ClassSystem.IsDivineCastingClass(castingClass))
                return true;

            return false;
        }

        [TempleDllLocation(0x10079550)]
        public void Debit()
        {
            // preamble
            if (caster == null)
            {
                Logger.Warn("SpellPacketBody.Debit() Null caster!");
                return;
            }

            if (IsItemSpell()) // this is handled separately
                return;

            var spellEnumDebited = this.spellEnumOriginal;

            // Spontaneous vs. Normal logging
            bool isSpont = (spellEnum != spellEnumOriginal) && spellEnumOriginal != 0;
            var spellName = GameSystems.Spell.GetSpellName(spellEnumOriginal);
            if (isSpont)
            {
                Logger.Debug("Debiting Spontaneous casted spell. Original spell: {0}", spellName);
            }
            else
            {
                Logger.Debug("Debiting casted spell {0}", spellName);
            }

            // Vancian spell handling - debit from the spells_memorized list
            if (IsVancian())
            {
                var numMem = caster.GetSpellArray(obj_f.critter_spells_memorized_idx).Count;
                var spellFound = false;
                for (var i = 0; i < numMem; i++)
                {
                    var spellMem = caster.GetSpell(obj_f.critter_spells_memorized_idx, i);
                    spellMem.pad0 = (char) (spellMem.pad0 & 0x7F); // clear out metamagic indictor

                    if (!GameSystems.Spell.IsDomainSpell(spellMem.classCode))
                    {
                        if (spellMem.spellEnum != spellEnumDebited)
                            continue;
                    }
                    else if (spellMem.spellEnum != spellEnum)
                    {
                        continue;
                    }

                    if (spellMem.spellLevel ==
                        spellKnownSlotLevel // todo: check if the spell level should be adjusted for MetaMagic
                        && spellMem.classCode == spellClass
                        && spellMem.spellStoreState.spellStoreType == SpellStoreType.spellStoreMemorized
                        && !spellMem.spellStoreState.usedUp
                        && spellMem.metaMagicData == metaMagicData)
                    {
                        spellMem.spellStoreState.usedUp = true;
                        caster.SetSpell(obj_f.critter_spells_memorized_idx, i, spellMem);
                        spellFound = true;
                        break;
                    }
                }

                if (!spellFound)
                {
                    Logger.Warn("Spell debit: Spell not found!");
                }
            }

            // add to casted list (so it shows up as used in the Spellbook / gets counted up for spells per day)
            var sd = new SpellStoreData(spellEnum, spellKnownSlotLevel, spellClass, metaMagicData);
            sd.spellStoreState.spellStoreType = SpellStoreType.spellStoreCast;
            caster.AppendSpell(obj_f.critter_spells_cast_idx, sd);
        }

        private bool IsItemSpell()
        {
            return invIdx != INV_IDX_INVALID;
        }

        public string GetName()
        {
            return GameSystems.Spell.GetSpellName(spellEnum);
        }

        private int IndexOfTarget(GameObjectBody target)
        {
            for (var i = 0; i < Targets.Length; i++)
            {
                if (Targets[i].Object == target)
                {
                    return i;
                }
            }

            return -1;
        }

        private int IndexOfSpellObject(GameObjectBody obj)
        {
            for (var i = 0; i < spellObjs.Length; i++)
            {
                if (spellObjs[i].obj == obj)
                {
                    return i;
                }
            }

            return -1;
        }

        private static void EndParticles(ref SpellTarget target)
        {
            if (target.ParticleSystem != null)
            {
                GameSystems.ParticleSys.End(target.ParticleSystem);
                target = new SpellTarget(target.Object, null);
            }
        }

        private static void EndParticles(ref SpellObj spellObj)
        {
            if (spellObj.partSys != null)
            {
                GameSystems.ParticleSys.End(spellObj.partSys);
                spellObj.partSys = null;
            }
        }

        public bool AddSpellObject(GameObjectBody spellObj, object partSys = null, bool replaceExisting = false)
        {
            // Check if it's already there
            var idx = IndexOfTarget(spellObj);

            if (idx != -1)
            {
                if (replaceExisting)
                {
                    if (partSys != spellObjs[idx].partSys)
                    {
                        EndParticles(ref spellObjs[idx]);
                    }
                    spellObjs[idx].obj = spellObj;
                    spellObjs[idx].partSys = partSys;
                }
                else
                {
                    Logger.Info("{0} is already a spell object of spell {1}, not adding again.", spellObj, spellId);
                }

                return false;
            }

            Array.Resize(ref spellObjs, spellObjs.Length + 1);
            spellObjs[^1] = new SpellObj
            {
                obj = spellObj,
                partSys = partSys
            };

            GameSystems.Spell.UpdateSpellPacket(this);
            GameSystems.Script.Spells.UpdateSpell(spellId);
            return true;
        }

        public void AddProjectile(GameObjectBody projectile)
        {
            Array.Resize(ref projectiles, projectiles.Length + 1);
            projectiles[^1] = projectile;
        }

        [TempleDllLocation(0x100c3cc0)]
        public bool AddTarget(GameObjectBody target, object partSys = null, bool replaceExisting = false)
        {
            // Check if it's already there
            var idx = IndexOfTarget(target);

            if (idx != -1)
            {
                if (replaceExisting)
                {
                    if (Targets[idx].ParticleSystem != partSys)
                    {
                        EndParticles(ref Targets[idx]);
                    }
                    Targets[idx] = new SpellTarget(target, partSys);
                }
                else
                {
                    Logger.Info("{0} is already a target of spell {1}, not adding again.", target, spellId);
                }

                return false;
            }

            Array.Resize(ref Targets, Targets.Length + 1);
            Targets[^1] = new SpellTarget(target, partSys);

            GameSystems.Spell.UpdateSpellPacket(this);
            GameSystems.Script.Spells.UpdateSpell(spellId);
            return true;
        }

        public bool RemoveProjectile(GameObjectBody projectileToRemove,
            bool endParticles = true,
            bool destroyObject = true)
        {
            var newCount = 0;
            foreach (var projectile in projectiles)
            {
                if (projectile != projectileToRemove)
                {
                    newCount++;
                }
            }

            if (newCount != projectiles.Length)
            {
                return false; // Projectile is not part of this spell anymore
            }

            if (newCount == 0)
            {
                projectiles = Array.Empty<GameObjectBody>();
            }
            else
            {
                var idx = 0;
                var newProjectiles = new GameObjectBody[newCount];
                foreach (var projectile in projectiles)
                {
                    if (projectile != projectileToRemove)
                    {
                        newProjectiles[idx++] = projectile;
                    }
                }

                Trace.Assert(idx == newProjectiles.Length);
                projectiles = newProjectiles;
            }

            if (endParticles)
            {
                // TODO: Get part sys attached to projectile and kill it
                throw new NotImplementedException();
            }

            if (destroyObject)
            {
                GameSystems.MapObject.RemoveMapObj(projectileToRemove);
            }

            return true;
        }

        public void ClearTargets()
        {
            for (var i = 0; i < Targets.Length; i++)
            {
                EndParticles(ref Targets[i]);
            }

            Targets = Array.Empty<SpellTarget>();
        }

        public void RemoveTargets(IEnumerable<GameObjectBody> targets)
        {
            foreach (var target in targets)
            {
                RemoveTarget(target);
            }
        }

        [TempleDllLocation(0x100c3be0)]
        public bool RemoveTarget(GameObjectBody target, bool keepParticles = false)
        {
            var idx = IndexOfTarget(target);
            if (idx == -1)
            {
                Logger.Info("Could not remove {0} from target list of spell {1} because it was already removed.",
                    target, spellId);
                return false;
            }

            if (Targets[idx].ParticleSystem != null)
            {
                if (!keepParticles)
                {
                    EndParticles(ref Targets[idx]);
                }
                else
                {
                    // TODO: we have to check every single instance of this to see if it's intended... :(
                    throw new NotSupportedException();
                }
            }

            // Move all items one slot forward
            for (int i = idx; i < Targets.Length - 1; i++)
            {
                Targets[i] = Targets[i + 1];
            }

            Array.Resize(ref Targets, Targets.Length - 1);

            GameSystems.Spell.UpdateSpellPacket(this);
            GameSystems.Script.Spells.UpdateSpell(spellId);
            return true;
        }

        public bool HasTarget(GameObjectBody target)
        {
            return IndexOfTarget(target) != -1;
        }

        public void EndPartSysForTarget(GameObjectBody target)
        {
            var idx = IndexOfTarget(target);
            if (idx != -1)
            {
                EndParticles(ref Targets[idx]);
            }
        }

        [TempleDllLocation(0x100768f0)]
        public object GetPartSysForTarget(GameObjectBody target)
        {
            var idx = IndexOfTarget(target);

            if (idx != -1)
            {
                return Targets[idx].ParticleSystem;
            }

            Logger.Info("Spell.c: spell_packet_partsysid_for_obj: no partsys for obj {0} ({1} in target list)", target,
                Targets.Length);
            return null;
        }

        private void EndParticleSystems()
        {
            for (var i = 0; i < Targets.Length; i++)
            {
                EndParticles(ref Targets[i]);
            }
        }

        public void SetTargets(IList<GameObjectBody> toArray)
        {
            EndParticleSystems();

            Array.Resize(ref Targets, toArray.Count);
            for (var i = 0; i < Targets.Length; i++)
            {
                Targets[i] = new SpellTarget(toArray[i], null);
            }
        }

        public void EndPartSysForCaster()
        {
            if (casterPartSys != null)
            {
                GameSystems.ParticleSys.End(casterPartSys);
                casterPartSys = null;
            }
        }

        public void EndPartSysForSpellObjects()
        {
            for (var index = 0; index < spellObjs.Length; index++)
            {
                ref var spellObj = ref spellObjs[index];
                if (spellObj.partSys != null)
                {
                    GameSystems.ParticleSys.End(spellObj.partSys);
                    spellObj.partSys = null;
                }
            }
        }

        public bool SavingThrow(GameObjectBody target)
        {
            if (!GameSystems.Spell.TryGetSpellEntry(spellEnum, out var entry))
            {
                return false;
            }

            return GameSystems.D20.Combat.SavingThrowSpell(target, caster, dc, entry.savingThrowType, default, spellId);
        }

    }
}