using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;

namespace SpicyTemple.Core.Systems.Spells
{
    public struct SpellObj
    {
        public GameObjectBody obj;
        public int partySysId;
        public int field_C;
    }

    public class SpellPacketBody
    {
        public uint spellEnum;
        public uint spellEnumOriginal; // used for spontaneous casting in order to debit the "original" spell
        public uint animFlags; // See SpellAnimationFlag
        public object pSthg;
        public GameObjectBody caster;
        public uint casterPartsysId;
        public uint spellClass; // aka spellClass
        public uint spellKnownSlotLevel; // aka spellLevel
        public uint casterLevel;
        public uint dc;
        public int numSpellObjs;
        public GameObjectBody aoeObj;
        public SpellObj[] spellObjs = new SpellObj[128];
        public uint orgTargetCount;
        public uint targetCount;
        public GameObjectBody[] targetListHandles = new GameObjectBody[32];
        public uint[] targetListPartsysIds = new uint[32];
        public uint projectileCount;
        public uint field_9C4;
        public GameObjectBody[] projectiles = new GameObjectBody[5];
        public LocAndOffsets aoeCenter;

        public uint field_A04;

        // TODO public PickerResult pickerResult;
        public int duration;
        public int durationRemaining;
        public uint spellRange;
        public uint savingThrowResult;

        public uint
            invIdx; // inventory index, used for casting spells from items e.g. scrolls; it is 0xFF for non-item spells

        public uint metaMagicData;
        public uint spellId;
        public uint field_AE4;
    }
}