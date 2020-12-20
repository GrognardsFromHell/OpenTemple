using System;
using OpenTemple.Core.GameObject;

namespace OpenTemple.Core.Systems.Spells
{
    public enum SpellsPerDayType
    {
        /// <summary>
        /// Spells need to be memorized in advance.
        /// </summary>
        Vancian,

        /// <summary>
        /// Any known spell can be cast using one of the unused slots of the appropriate level.
        /// </summary>
        Spontaneous
    }

    /// <summary>
    /// Spells per Day information for a single casting class.
    /// </summary>
    public class SpellsPerDay
    {
        public SpellsPerDayType Type { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }

        /// <summary>
        /// The class code used for spells that are cast using the associated casting class.
        /// </summary>
        public int ClassCode { get; set; }

        /// <summary>
        /// Spells per day for spell levels 0 to 9.
        /// </summary>
        public SpellsPerDayLevel[] Levels;

        public SpellsPerDay()
        {
            Levels = new SpellsPerDayLevel[10];
            for (var i = 0; i < Levels.Length; i++)
            {
                ref var level = ref Levels[i];
                level.Level = i;
                level.Slots = Array.Empty<SpellSlot>();
            }
        }

        public bool TryFindEmptyUnusedSlot(int level, out int index)
        {
            if (level < Levels.Length)
            {
                return Levels[level].TryFindEmptyUnusedSlot(out index);
            }

            index = -1;
            return false;
        }
    }

    public struct SpellsPerDayLevel
    {
        public int Level { get; set; }

        public SpellSlot[] Slots { get; set; }

        public bool TryFindEmptyUnusedSlot(out int index)
        {
            for (var i = 0; i < Slots.Length; i++)
            {
                ref var slot = ref Slots[i];
                if (!slot.HasSpell && !slot.HasBeenUsed)
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }
    }

    public enum SpellSlotSource
    {
        Unknown = 0,

        /// <summary>
        /// Spell slot was gained through levels of a casting class or levels in a prestige class increasing a class
        /// level to determine spells per day.
        /// </summary>
        ClassLevels = 1,

        /// <summary>
        /// From the primary attribute associated with the casting class.
        /// </summary>
        BonusSpells = 2,

        /// <summary>
        /// Bonus slot gained from wizard specialisation.
        /// </summary>
        WizardSpecialization = 3
    }

    public struct SpellSlot
    {
        public SpellSlotSource Source;

        /// <summary>
        /// Whether there is spell associated with this slot.
        /// This varies between vancian and spotaneous casting in that for vancian casting this means there is
        /// a memorized spell in this slot, while for spontaneous casting, it simply means the slot has been used.
        /// </summary>
        public bool HasSpell => SpellEnum > 0;

        public bool HasBeenUsed => MemorizedSpell.spellStoreState.usedUp;

        public int SpellEnum => MemorizedSpell.spellEnum;

        public MetaMagicData MetaMagic => MemorizedSpell.metaMagicData;

        /// <summary>
        /// This is only useful for domain spell lists which are shared between multiple domains.
        /// </summary>
        public int ClassCode => MemorizedSpell.classCode;

        public SpellStoreData MemorizedSpell { get; set; }

        public void ClearSpell()
        {
            MemorizedSpell = default;
        }
    }
}