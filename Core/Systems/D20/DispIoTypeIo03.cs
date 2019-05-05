using System;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Systems.GameObjects;

namespace SpicyTemple.Core.Systems.D20
{
    // DispIoType = 1
    public class DispIoCondStruct
    {
        public ConditionSpec condStruct;
        public uint outputFlag;
        public int arg1;
        public int arg2;

        public static readonly DispIoCondStruct Default = new DispIoCondStruct();
    }

    // DispIoType = 2  used for fetching ability scores (dispType 10, 66), and Cur/Max HP
    public class DispIoBonusList
    {
        public BonusList bonlist;

        // checked in 0x100C5C30 vs 2 for eagle's splendor spell (used to ignore the bonus from the spell)
        public uint flags;

        public static readonly DispIoBonusList Default = new DispIoBonusList
        {
            bonlist = BonusList.Default
        };
    }

    // DispIoType = 3
    public class DispIoSavingThrow
    {
        public uint returVal;
        public ObjHndl obj;

        // see D20SavingThrowFlag looks like: 2 - trap, 0x10 - Spell, 0x20 thru 0x1000 - spell schools (abjuration thru transmutation, e.g. 0x100 - enchantment), 0x100000 - fear/morale effect?
        public uint flags;

        public int field_14;
        public BonusList bonlist;
        public int rollResult;

        [TempleDllLocation(0x1004DA80)]
        public static readonly DispIoSavingThrow Default = new DispIoSavingThrow
        {
            bonlist = BonusList.Default
        };
    }

    // DispIoType 5
    public class DispIoAttackBonus
    {
        public int field_4;
        public AttackPacket attackPacket;
        public BonusList bonlist;

        public static readonly DispIoAttackBonus Default;

        static DispIoAttackBonus()
        {
            Default = new DispIoAttackBonus
            {
                attackPacket = AttackPacket.Default,
                bonlist = BonusList.Default
            };
            Default.attackPacket.dispKey = D20DispatcherKey.STAT_STRENGTH;
        }
    }

    public class DispIoD20Signal // DispIoType 6
    {
        public uint return_val;
        public uint data1;
        public uint data2;

        public GameObjectBody obj; // Replaces data1+data2 in case a handle is sent

        public static readonly DispIoD20Signal Default = new DispIoD20Signal();
    }

    public class DispIoD20Query // DispIoType 7
    {
        public int return_val; // changed to int type to avoid python casting madness
        public uint data1;
        public uint data2;

        public object obj;

        public static readonly DispIoD20Query Default = new DispIoD20Query();
    }

    public class TurnBasedStatus
    {
        public int
            hourglassState; // 4 - full action remaining; 3 - partial?? used in interrupts, checked by partial charge; 2 - single action remaining; 1 - move action remaining

        public int tbsFlags; // see TurnBasedStatusFlags
        public int idxSthg;

        public float
            surplusMoveDistance; // is nonzero when you have started a move action already and haven't used it all up

        public int
            baseAttackNumCode; // is composed of the base number of attacks (dispatch 51 or 53) + a code number: 99 for dual wielding (+1 for extra offhand attack), 999 for natural attacks

        public int attackModeCode; // 0 for normal main hand, 99 for dual wielding, 999 for natural attacks
        public int numBonusAttacks; // number of bonus attacks (dispatch 52)
        public int numAttacks;
        public int errCode;

        public TurnBasedStatus()
        {
            hourglassState = 4;
            tbsFlags = 0;
            idxSthg = -1;
            surplusMoveDistance = 0.0f;
            baseAttackNumCode = 0;
            attackModeCode = 0;
            numBonusAttacks = 0;
            numAttacks = 0;
            errCode = 0;
        }
    };

    public class DispIOTurnBasedStatus // type 8
    {
        TurnBasedStatus tbStatus;
    }

    public class DispIoTooltip // DispIoType 9 ; tooltip additional text when hovering over an object in the game
    {
        public string[] strings;

        public void Append(string cs)
        {
            if (strings == null)
            {
                strings = new[] {cs};
            }
            else
            {
                Array.Resize(ref strings, strings.Length + 1);
                strings[^1] = cs;
            }
        }

        public static readonly DispIoTooltip Default = new DispIoTooltip();
    }

    public class DispIoObjBonus // type 10
    {
        public uint flags;

        // TODO public BonusList? bonOut;
        public uint pad;
        public ObjHndl obj; //optional
        public BonusList bonlist;

        public static readonly DispIoObjBonus Default = new DispIoObjBonus();
    }

    public class DispIoDispelCheck // type 11
    {
        public uint spellId; // of the Dispel Spell (Break Enchantment, Dispel Magic etc.)

        public uint
            flags; // 0x80 - Dispel Magic   0x40 - Break Enchantment  0x20 - slippery mind 0x10 - 0x2 DispelAlignment stuff

        public uint returnVal;

        public static readonly DispIoDispelCheck Default = new DispIoDispelCheck();
    };

    public class DispIoD20ActionTurnBased
    {
        // dispIoType = 12; matches dispTypes 36-38 , 52
        public int returnVal;

        // TODO public D20Actn d20a;
        public TurnBasedStatus tbStatus;
        public BonusList bonlist; // NEW (extended vanilla)

        public static readonly DispIoD20ActionTurnBased Default = new DispIoD20ActionTurnBased();
    };

    public class DispIoMoveSpeed // dispIoType = 13, matches dispTypes 40,41
    {
        public BonusList bonlist;
        public float factor;

        public static readonly DispIoMoveSpeed Default = new DispIoMoveSpeed
        {
            factor = 1.0f
        };
    }

    public class DispIOBonusListAndSpellEntry
    {
        // Type 14
        public BonusList bonList;

        // TODO public SpellEntry* spellEntry;
        public uint field_C; // unused?

        public static readonly DispIOBonusListAndSpellEntry Default = new DispIOBonusListAndSpellEntry();
    };

    public class DispIoReflexThrow
    {
        // DispIoType = 15
        public int effectiveReduction;
        public D20SavingThrowReduction reduction;
        public int damageMesLine;
        public D20AttackPower attackPower;
        public int attackType;
        public int throwResult;
        public D20SavingThrowFlag flags;

        public static readonly DispIoReflexThrow Default = new DispIoReflexThrow();
    };

    public class DispIoObjEvent // type 17
    {
        public int pad;
        public ObjHndl aoeObj;
        public ObjHndl tgt;
        public uint evtId;

        public static readonly DispIoObjEvent Default = new DispIoObjEvent();
    }

    public class DispIoAbilityLoss //  type 19
    {
        public int result;
        public Stat statDamaged;
        public int fieldC;

        public int spellId;

        // 8 - marked at the beginning of dispatch; 0x10 - checks against this in the Temp/Perm ability damage
        public int flags;

        public static readonly DispIoAbilityLoss Default = new DispIoAbilityLoss
        {
            statDamaged = Stat.strength
        };
    };

    public class DispIoAttackDice // type 20
    {
        // TODO public BonusList* bonlist;
        public D20CAF flags;
        public int fieldC;
        public ObjHndl weapon;
        public ObjHndl wielder;
        public int dicePacked;
        public DamageType attackDamageType;

        public static readonly DispIoAttackDice Default = new DispIoAttackDice()
        {
            flags = D20CAF.HIT
        };
    }

    public class DispIoTypeImmunityTrigger
    {
        // DispIoType 21
        public uint interrupt;
        public uint field_8;
        public uint field_C;
        public uint SDDKey1;
        public uint val2;

        public uint okToAdd; // or spellId???
        public ConditionAttachment condNode;

        [TempleDllLocation(0x1004DBA0)]
        public static readonly DispIoTypeImmunityTrigger Default = new DispIoTypeImmunityTrigger();
    }

    public class DispIoImmunity // type 23
    {
        public int returnVal;
        public int field8;

        public int flag;
        // TODO public SpellPacketBody* spellPkt;
        // TODO public SpellEntry spellEntry;

        public static readonly DispIoImmunity Default = new DispIoImmunity();
    }

    public class DispIoEffectTooltip // type 24
    {
        // TODO BuffDebuffPacket* bdb;

        /*
         spellEnum = -1 for no spell
        */
        // TODO void Append(int effectTypeId, int spellEnum, string text);

        public static readonly DispIoEffectTooltip Default = new DispIoEffectTooltip();
    };


    public class EvtObjSpellCaster // type 34 (NEW!)
    {
        public BonusList bonlist;
        public GameObjectBody handle;
        public Stat arg0;

        public int arg1;
        // TODO SpellPacketBody* spellPkt;

        public static readonly EvtObjSpellCaster Default = new EvtObjSpellCaster();
    }

    public class EvtObjMetaMagic // type 35 (NEW!)
    {
        public MetaMagicData mmData;

        public static readonly EvtObjMetaMagic Default = new EvtObjMetaMagic();
    };

    public class EvtObjSpecialAttack // type 36 (NEW!)
    {
        enum AttackType
        {
            STUNNING_FIST = 1,
            NUM_EFFECTS
        };

        public int attack; //Uses the attack enum but unfortunately the enum can't be passed through to python
        public ObjHndl target;

        public static readonly EvtObjSpecialAttack Default = new EvtObjSpecialAttack();
    };

    public class EvtObjRangeIncrementBonus // type 37 (NEW!)
    {
        public ObjHndl weaponUsed;
        public double rangeBonus;
    };
}