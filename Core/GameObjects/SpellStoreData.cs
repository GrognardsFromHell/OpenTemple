using System;
using System.Runtime.InteropServices;
using OpenTemple.Core.Systems;

namespace OpenTemple.Core.GameObjects
{
    [Flags]
    public enum SpellStoreType : byte
    {
        spellStoreNone = 0,
        spellStoreKnown = 1,
        spellStoreMemorized = 2,
        spellStoreCast = 3,
        spellStoreAtWill // New! Todo implementation
    }

    [Flags]
    public enum MetaMagicFlags : byte
    {
        MetaMagic_Maximize = 1,
        MetaMagic_Quicken = 2,
        MetaMagic_Silent = 4,
        MetaMagic_Still = 8
    }

    public struct MetaMagicData
    {
        public MetaMagicFlags
            metaMagicFlags; // 1 - Maximize Spell ; 2 - Quicken Spell ; 4 - Silent Spell;  8 - Still Spell

        public byte metaMagicEmpowerSpellCount;
        public byte metaMagicEnlargeSpellCount;
        public byte metaMagicExtendSpellCount;
        public byte metaMagicHeightenSpellCount;
        public byte metaMagicWidenSpellCount;

        /// <summary>
        /// Pack the data into a 32-bit unsigned integer.
        /// </summary>
        public uint Pack()
        {
            uint result = 0;
            result |= (uint) ((uint) metaMagicFlags & 0xF);
            result |= (uint) (metaMagicEmpowerSpellCount & 0xF) << 4;
            result |= (uint) (metaMagicEnlargeSpellCount & 0xF) << 8;
            result |= (uint) (metaMagicExtendSpellCount & 0xF) << 12;
            result |= (uint) (metaMagicHeightenSpellCount & 0xF) << 16;
            result |= (uint) (metaMagicWidenSpellCount & 0xF) << 20;
            return result;
        }

        public bool IsMaximize
        {
            get => (metaMagicFlags & MetaMagicFlags.MetaMagic_Maximize) != 0;
            set
            {
                if (value)
                {
                    metaMagicFlags |= MetaMagicFlags.MetaMagic_Maximize;
                }
                else
                {
                    metaMagicFlags &= ~MetaMagicFlags.MetaMagic_Maximize;
                }
            }
        }

        public bool IsQuicken
        {
            get => (metaMagicFlags & MetaMagicFlags.MetaMagic_Quicken) != 0;
            set
            {
                if (value)
                {
                    metaMagicFlags |= MetaMagicFlags.MetaMagic_Quicken;
                }
                else
                {
                    metaMagicFlags &= ~MetaMagicFlags.MetaMagic_Quicken;
                }
            }
        }

        public bool IsSilent
        {
            get => (metaMagicFlags & MetaMagicFlags.MetaMagic_Silent) != 0;
            set
            {
                if (value)
                {
                    metaMagicFlags |= MetaMagicFlags.MetaMagic_Silent;
                }
                else
                {
                    metaMagicFlags &= ~MetaMagicFlags.MetaMagic_Silent;
                }
            }
        }

        public bool IsStill
        {
            get => (metaMagicFlags & MetaMagicFlags.MetaMagic_Still) != 0;
            set
            {
                if (value)
                {
                    metaMagicFlags |= MetaMagicFlags.MetaMagic_Still;
                }
                else
                {
                    metaMagicFlags &= ~MetaMagicFlags.MetaMagic_Still;
                }
            }
        }

        public bool IsEmpowered => metaMagicEmpowerSpellCount > 0;

        public bool HasModifiers => metaMagicFlags != 0;

        /// <summary>
        /// Unpack the metamagic data from a 32-bit unsigned integer previously packed using the Pack method.
        /// </summary>
        public static MetaMagicData Unpack(uint raw)
        {
            var result = new MetaMagicData();
            result.metaMagicFlags = (MetaMagicFlags) (raw & 0xF);
            result.metaMagicEmpowerSpellCount = (byte) ((raw & 0xF0) >> 4);
            result.metaMagicEnlargeSpellCount = (byte) ((raw & 0xF00) >> 8);
            result.metaMagicExtendSpellCount = (byte) ((raw & 0xF000) >> 12);
            result.metaMagicHeightenSpellCount = (byte) ((raw & 0xF0000) >> 16);
            result.metaMagicWidenSpellCount = (byte) ((raw & 0xF00000) >> 20);
            return result;
        }

        public bool Equals(MetaMagicData other)
        {
            return metaMagicFlags == other.metaMagicFlags &&
                   metaMagicEmpowerSpellCount == other.metaMagicEmpowerSpellCount &&
                   metaMagicEnlargeSpellCount == other.metaMagicEnlargeSpellCount &&
                   metaMagicExtendSpellCount == other.metaMagicExtendSpellCount &&
                   metaMagicHeightenSpellCount == other.metaMagicHeightenSpellCount &&
                   metaMagicWidenSpellCount == other.metaMagicWidenSpellCount;
        }

        public override bool Equals(object obj)
        {
            return obj is MetaMagicData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) metaMagicFlags;
                hashCode = (hashCode * 397) ^ metaMagicEmpowerSpellCount.GetHashCode();
                hashCode = (hashCode * 397) ^ metaMagicEnlargeSpellCount.GetHashCode();
                hashCode = (hashCode * 397) ^ metaMagicExtendSpellCount.GetHashCode();
                hashCode = (hashCode * 397) ^ metaMagicHeightenSpellCount.GetHashCode();
                hashCode = (hashCode * 397) ^ metaMagicWidenSpellCount.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(MetaMagicData left, MetaMagicData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MetaMagicData left, MetaMagicData right)
        {
            return !left.Equals(right);
        }
    }

    public struct SpellStoreState
    {
        public SpellStoreType spellStoreType;
        public bool usedUp; // relevant only for spellStoreMemorized

        public static SpellStoreState Unpack(int packed)
        {
            var result = new SpellStoreState();
            result.spellStoreType = (SpellStoreType) (packed & 0xFF);
            result.usedUp = (byte) ((packed >> 8) & 0xFF) != 0;
            return result;
        }

        public int Pack()
        {
            return ((byte) spellStoreType) | ((usedUp ? 1 : 0) << 8);
        }

        public bool Equals(SpellStoreState other)
        {
            return spellStoreType == other.spellStoreType && usedUp == other.usedUp;
        }

        public override bool Equals(object obj)
        {
            return obj is SpellStoreState other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) spellStoreType * 397) ^ usedUp.GetHashCode();
            }
        }

        public static bool operator ==(SpellStoreState left, SpellStoreState right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SpellStoreState left, SpellStoreState right)
        {
            return !left.Equals(right);
        }
    };

    public enum SpontCastType : byte
    {
        None = 0,
        GoodCleric = 2,
        EvilCleric = 4,
        Druid = 8
    };

    [Flags]
    public enum AiSpellType : uint
    {
        ai_action_summon = 0x1,
        ai_action_offensive = 0x2,
        ai_action_defensive = 0x4,
        ai_action_flee = 0x8,
        ai_action_heal_heavy = 0x10,
        ai_action_heal_medium = 0x20,
        ai_action_heal_light = 0x40,
        ai_action_cure_poison = 0x80,
        ai_action_resurrect = 0x100
    };

    public enum SpellSourceType : int
    {
        Ability = 0,
        Arcane = 1,
        Divine = 2,
        Psionic = 3,
        Any = 4
    };

    public enum SpellReadyingType : int
    {
        Vancian = 0, // memorization slots
        Innate, // bards / sorcerers etc.
        Any
    };

    public enum SpellListType : int
    {
        None = 0,
        Any, // for prestige classes that stack spell progression with anything
        Arcane,
        Bardic, // subset of Arcane
        Clerical, // subset of Divine
        Divine,
        Druidic, // subset of divine
        Paladin, // subset of divine
        Psionic,
        Ranger, // subset of divine
        Special, // "independent" list
        Extender // extends an existing spell list
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SpellStoreData
    {
        public int spellEnum;
        public int classCode;
        public int spellLevel;
        public SpellStoreState spellStoreState;
        public ushort padSpellStore; // Used to store race apparently
        public MetaMagicData metaMagicData; // should be stored as 32bit value!
        public char pad0;
        public uint pad1; // these are actually related to MM indicator icons
        public uint pad2;
        public uint pad3;

        public SpellStoreData(int SpellEnum, int SpellLevel, int ClassCode, uint mmData = 0,
            int SpellStoreData = 0) : this()
        {
            spellEnum = SpellEnum;
            classCode = ClassCode;
            spellLevel = SpellLevel;
            metaMagicData = MetaMagicData.Unpack(mmData);
            spellStoreState = SpellStoreState.Unpack(SpellStoreData);
        }

        public SpellStoreData(int SpellEnum, int SpellLevel, int ClassCode, MetaMagicData mmData,
            SpellStoreState spellStoreData) : this()
        {
            spellEnum = SpellEnum;
            classCode = ClassCode;
            spellLevel = SpellLevel;
            metaMagicData = mmData;
            spellStoreState = spellStoreData;
        }

        [TempleDllLocation(0x10075280)]
        public SpellStoreData(int SpellEnum, int SpellLevel, int ClassCode, MetaMagicData mmData) : this()
        {
            spellEnum = SpellEnum;
            classCode = ClassCode;
            spellLevel = SpellLevel;
            metaMagicData = mmData;
        }

        public static bool operator <(SpellStoreData sp1, SpellStoreData sp2)
        {
            int levelDelta = (int) sp1.spellLevel - (int) sp2.spellLevel;
            if (levelDelta < 0)
                return true;
            else if (levelDelta > 0)
                return false;

            // if levels are equal
            var name1 = GameSystems.Spell.GetSpellName(sp1.spellEnum);
            var name2 = GameSystems.Spell.GetSpellName(sp2.spellEnum);
            var nameCmp = string.CompareOrdinal(name1, name2);
            return nameCmp < 0;
        }

        public static bool operator >(SpellStoreData a, SpellStoreData b)
        {
            throw new NotImplementedException();
        }
    }
}