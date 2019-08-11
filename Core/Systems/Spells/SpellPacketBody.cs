using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Ui.InGameSelect;

namespace SpicyTemple.Core.Systems.Spells
{
    public struct SpellObj
    {
        public GameObjectBody obj;
        public int partSysId;
        public int field_C;
    }

    [Flags]
    public enum SpellAnimationFlag
    {
        SAF_UNK8 = 0x8,
        SAF_ID_ATTEMPTED = 0x10,
        SAF_UNK20 = 0x20
    }

    public class SpellPacketBody
    {
	    private static readonly ILogger Logger = new ConsoleLogger();

	    private const int INV_IDX_INVALID = 255;

        public int spellEnum;
        public int spellEnumOriginal; // used for spontaneous casting in order to debit the "original" spell
        public SpellAnimationFlag animFlags; // See SpellAnimationFlag
        public object pSthg;
        public GameObjectBody caster;
        public uint casterPartsysId;
        public int spellClass; // aka spellClass
        public int spellKnownSlotLevel; // aka spellLevel
        public int casterLevel;
        public int dc;
        public int numSpellObjs => spellObjs.Length;
        public GameObjectBody aoeObj;
        public SpellObj[] spellObjs = Array.Empty<SpellObj>();
        public int orgTargetCount; // Does this mean "unique" targets???
        public int targetCount => targetListHandles.Length;
        public GameObjectBody[] targetListHandles = Array.Empty<GameObjectBody>();
        public object[] targetListPartsysIds = Array.Empty<object>();
        public int projectileCount => projectiles.Length;
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
        public void Reset()
        {
	        spellId = 0;
	        spellEnum = 0;
	        spellEnumOriginal = 0;
	        caster = null;
	        casterPartsysId = 0;
	        casterLevel = 0;
	        dc = 0;
	        animFlags = 0;
	        aoeCenter = LocAndOffsets.Zero;
	        aoeCenterZ = 0;
	        targetListHandles = Array.Empty<GameObjectBody>();
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

        public bool IsVancian(){
	        if (GameSystems.Spell.IsDomainSpell(spellClass))
		        return true;

	        if (D20ClassSystem.IsVancianCastingClass(GameSystems.Spell.GetCastingClass(spellClass)))
		        return true;

	        return false;
        }

        public bool IsDivine(){
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
			if (caster == null) {
				Logger.Warn("SpellPacketBody.Debit() Null caster!");
				return;
			}

			if (IsItemSpell()) // this is handled separately
				return;

			var spellEnumDebited = this.spellEnumOriginal;

			// Spontaneous vs. Normal logging
			bool isSpont = (spellEnum != spellEnumOriginal) && spellEnumOriginal != 0;
			var spellName = GameSystems.Spell.GetSpellName(spellEnumOriginal);
			if (isSpont){
				Logger.Debug("Debiting Spontaneous casted spell. Original spell: {0}", spellName);
			} else	{
				Logger.Debug("Debiting casted spell {0}", spellName);
			}

			// Vancian spell handling - debit from the spells_memorized list
			if (IsVancian()){

				var numMem = caster.GetSpellArray(obj_f.critter_spells_memorized_idx).Count;
				var spellFound = false;
				for (var i = 0; i < numMem; i++){
					var spellMem = caster.GetSpell(obj_f.critter_spells_memorized_idx, i);
					spellMem.pad0 = (char) (spellMem.pad0 & 0x7F); // clear out metamagic indictor

					if (!GameSystems.Spell.IsDomainSpell(spellMem.classCode)){
						if (spellMem.spellEnum != spellEnumDebited)
							continue;
					}
					else if (spellMem.spellEnum != spellEnum){
						continue;
					}

					if (spellMem.spellLevel == spellKnownSlotLevel // todo: check if the spell level should be adjusted for MetaMagic
						&& spellMem.classCode == spellClass
						&& spellMem.spellStoreState.spellStoreType == SpellStoreType.spellStoreMemorized
						&& spellMem.spellStoreState.usedUp == 0
						&& spellMem.metaMagicData == metaMagicData)	{
						spellMem.spellStoreState.usedUp = 1;
						caster.SetSpell(obj_f.critter_spells_memorized_idx, i, spellMem);
						spellFound = true;
						break;
					}
				}

				if (!spellFound){
					Logger.Warn("Spell debit: Spell not found!");
				}

			}

			// add to casted list (so it shows up as used in the Spellbook / gets counted up for spells per day)
			var sd = new SpellStoreData(spellEnum, spellKnownSlotLevel, spellClass, metaMagicData);
			sd.spellStoreState.spellStoreType = SpellStoreType.spellStoreCast;
			var spellArraySize = caster.GetSpellArray(obj_f.critter_spells_cast_idx).Count;
			caster.SetSpell(obj_f.critter_spells_cast_idx, spellArraySize, sd);

        }

        private bool IsItemSpell()
        {
	        return invIdx != INV_IDX_INVALID;
        }

        public string GetName()
        {
	        return GameSystems.Spell.GetSpellName(spellEnum);
        }

        [TempleDllLocation(0x100c3cc0)]
        public bool AddTarget(GameObjectBody target, object partSys, bool replaceExisting)
        {

            // Check if it's already there
            var idx =Array.IndexOf(targetListHandles, target);

            if (idx != -1)
            {
                if (replaceExisting)
                {
                    // TODO: Shouldn't we end/free the existing particle system here...
                    targetListPartsysIds[idx] = partSys;
                }
                else
                {
                    Logger.Info("{0} is already a target of spell {1}, not adding again.", target, spellId);
                }
                return false;
            }

            Array.Resize(ref targetListHandles, targetListHandles.Length + 1);
            Array.Resize(ref targetListPartsysIds, targetListPartsysIds.Length + 1);            
            targetListHandles[^1] = target;
            targetListPartsysIds[^1] = partSys;

            GameSystems.Spell.UpdateSpellPacket(this);
            GameSystems.Script.Spells.UpdateSpell(spellId);
            return true;
        }

        [TempleDllLocation(0x100c3be0)]
        public bool RemoveTarget(GameObjectBody target)
        {
            var idx = Array.IndexOf(targetListHandles, target);
            if (idx == -1)
            {
                Logger.Info("Could not remove {0} from target list of spell {1} because it was already removed.", target, spellId);
                return false;
            }

            // Move all items one slot forward
            for (int i = idx; idx < targetListHandles.Length - 1; i++)
            {
                targetListHandles[i] = targetListHandles[i+1];
                targetListPartsysIds[i] = targetListPartsysIds[i+1];
            }
            Array.Resize(ref targetListHandles, targetListHandles.Length - 1);
            Array.Resize(ref targetListPartsysIds, targetListPartsysIds.Length - 1);

            GameSystems.Spell.UpdateSpellPacket(this);
            GameSystems.Script.Spells.UpdateSpell(spellId);
            return true;
        }

        [TempleDllLocation(0x100768f0)]
        public object GetPartSysForTarget(GameObjectBody target)
        {
            for (int i = 0; i < targetListHandles.Length; i++)
            {
                if (targetListHandles[i] == target) { 
                    return targetListPartsysIds[i];
                }
            }

            Logger.Info("Spell.c: spell_packet_partsysid_for_obj: no partsys for obj {0} ({1} in target list)", target, targetListHandles.Length);
            return null;
        }

    }
}