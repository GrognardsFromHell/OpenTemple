using System;
using System.Resources;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems.D20
{
    // DispIoType = 1
    public class DispIoCondStruct
    {
        public ConditionSpec condStruct;
        public uint outputFlag;
        public int arg1;
        public int arg2;

        public static DispIoCondStruct Default => new DispIoCondStruct();
    }

    // DispIoType = 2  used for fetching ability scores (dispType 10, 66), and Cur/Max HP
    public class DispIoBonusList
    {
        public BonusList bonlist;

        // checked in 0x100C5C30 vs 2 for eagle's splendor spell (used to ignore the bonus from the spell)
        public uint flags;

        public static DispIoBonusList Default => new DispIoBonusList
        {
            bonlist = BonusList.Default
        };
    }

    // DispIoType = 3
    public class DispIoSavingThrow
    {
        public GameObjectBody obj;

        // see D20SavingThrowFlag looks like: 2 - trap, 0x10 - Spell, 0x20 thru 0x1000 - spell schools (abjuration thru transmutation, e.g. 0x100 - enchantment), 0x100000 - fear/morale effect?
        public D20SavingThrowFlag flags;

        public int field_14;
        public BonusList bonlist;
        public int rollResult;

        [TempleDllLocation(0x1004DA80)]
        public static DispIoSavingThrow Default => new DispIoSavingThrow
        {
            bonlist = BonusList.Default
        };
    }

    // DispIoType 5
    public class DispIoAttackBonus
    {
        public AttackPacket attackPacket;
        public BonusList bonlist;

        public static DispIoAttackBonus Default
        {
            get
            {
                var result = new DispIoAttackBonus
                {
                    attackPacket = AttackPacket.Default,
                    bonlist = BonusList.Default
                };
                result.attackPacket.dispKey = 1;
                return result;
            }
        }

    }

    public class DispIoD20Signal // DispIoType 6
    {
        public uint return_val;
        public int data1;
        public int data2;

        public TimePoint TimePoint;

        public object obj; // Replaces data1+data2 in case a handle or disp io is sent

        public static DispIoD20Signal Default => new DispIoD20Signal();
    }

    public class DispIoD20Query // DispIoType 7
    {
        public int return_val; // changed to int type to avoid python casting madness
        public int data1;
        public int data2;

        public ulong resultData; // TODO Previously data1+data2. Have to adjust all conditions to match

        public object obj;

        public static DispIoD20Query Default => new DispIoD20Query();
    }

    [Flags]
    public enum TurnBasedStatusFlags
    {
        NONE = 0,
        UNK_1 = 1,
        Moved = 2,
        Moved5FootStep = 4,
        TouchAttack = 8, // denotes that you're doing a touch attack
        CritterSpell = 0x10, // denotes that the spell being cast is actually a critter's natural ability, so don't provoke AoO
        HasActedThisRound = 0x20, // prevents you from dragging the portrait in the initiative row
        FullAttack = 0x40,
        UNK_80 = 0x80,
        UNK_100 = 0x100,
        FreeActionSpellPerformed = 0x200, // already performed free-action spell this round (e.g. from Quickened metamagic feat), cannot do another
        UNK_400 = 0x400,
        ChangedWornItem = 0x800 // denotes that you've changed items in the inventory during combat (to prevent double-charging you); unflags this when hiding the inventory
    }

    public enum HourglassState
    {
        // 4 - full action remaining; 3 - partial?? used in interrupts, checked by partial charge; 2 - single action remaining; 1 - move action remaining
        INVALID = -1,
        EMPTY = 0,
        MOVE = 1, // move action
        STD = 2, // standard action
        PARTIAL = 3,
        FULL = 4, // full round action
    }

    public class TurnBasedStatus
    {
        public HourglassState hourglassState;

        public TurnBasedStatusFlags tbsFlags; // see TurnBasedStatusFlags
        public int idxSthg;

        public float
            surplusMoveDistance; // is nonzero when you have started a move action already and haven't used it all up

        public int
            baseAttackNumCode; // is composed of the base number of attacks (dispatch 51 or 53) + a code number: 99 for dual wielding (+1 for extra offhand attack), 999 for natural attacks

        public int attackModeCode; // 0 for normal main hand, 99 for dual wielding, 999 for natural attacks
        public int numBonusAttacks; // number of bonus attacks (dispatch 52)
        public int numAttacks;
        public ActionErrorCode errCode; // Might be action error code

        public TurnBasedStatus()
        {
            hourglassState = HourglassState.FULL;
            tbsFlags = 0;
            idxSthg = -1;
            surplusMoveDistance = 0.0f;
            baseAttackNumCode = 0;
            attackModeCode = 0;
            numBonusAttacks = 0;
            numAttacks = 0;
            errCode = 0;
        }

        public void Clear()
        {
            hourglassState = 0;
            tbsFlags = 0;
            idxSthg = 0;
            surplusMoveDistance = 0.0f;
            baseAttackNumCode = 0;
            attackModeCode = 0;
            numBonusAttacks = 0;
            numAttacks = 0;
            errCode = 0;
        }

        public void Reset()
        {
            Clear();
            hourglassState = HourglassState.FULL;
            idxSthg = -1;
        }

        public void CopyTo(TurnBasedStatus other)
        {
            other.hourglassState = hourglassState;
            other.tbsFlags = tbsFlags;
            other.idxSthg = idxSthg;
            other.surplusMoveDistance = surplusMoveDistance;
            other.baseAttackNumCode = baseAttackNumCode;
            other.attackModeCode = attackModeCode;
            other.numBonusAttacks = numBonusAttacks;
            other.numAttacks = numAttacks;
            other.errCode = errCode;
        }

        public TurnBasedStatus Copy()
        {
            return (TurnBasedStatus) MemberwiseClone();
        }
    };

    public class DispIOTurnBasedStatus // type 8
    {
        public TurnBasedStatus tbStatus;
    }

    public class DispIoTooltip // DispIoType 9 ; tooltip additional text when hovering over an object in the game
    {
        private string[] _strings;

        public string[] Lines => _strings ?? Array.Empty<string>();

        public void Append(string cs)
        {
            if (_strings == null)
            {
                _strings = new[] {cs};
            }
            else
            {
                Array.Resize(ref _strings, _strings.Length + 1);
                _strings[^1] = cs;
            }
        }

        public static DispIoTooltip Default => new DispIoTooltip();
    }

    public class DispIoObjBonus // type 10
    {
        public int flags;

        public uint pad;
        public GameObjectBody obj; //optional
        public BonusList bonlist;

        public ref BonusList bonOut => ref bonlist;

        public static DispIoObjBonus Default => new DispIoObjBonus()
        {
            bonlist = BonusList.Default
        };
    }

    public class DispIoDispelCheck // type 11
    {
        public uint spellId; // of the Dispel Spell (Break Enchantment, Dispel Magic etc.)

        public uint
            flags; // 0x80 - Dispel Magic   0x40 - Break Enchantment  0x20 - slippery mind 0x10 - 0x2 DispelAlignment stuff

        public uint returnVal;

        public static DispIoDispelCheck Default => new DispIoDispelCheck();
    };

    public class DispIoD20ActionTurnBased
    {
        // dispIoType = 12; matches dispTypes 36-38 , 52
        public ActionErrorCode returnVal;

        public D20Action action;
        public TurnBasedStatus tbStatus;
        public BonusList bonlist; // NEW (extended vanilla)

        private DispIoD20ActionTurnBased()
        {
        }

        public DispIoD20ActionTurnBased(D20Action action)
        {
            this.action = action;
        }

        public static DispIoD20ActionTurnBased Default => new DispIoD20ActionTurnBased();

        public void DispatchPerform(D20DispatcherKey key)
        {
            if (action == null || action.d20APerformer== null) {
                returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                return;
            }

            action.d20APerformer.GetDispatcher()?.Process(DispatcherType.D20ActionPerform, key, this);
        }

        public void DispatchPythonAdf(D20DispatcherKey key)
        {
            if (action == null || action.d20APerformer == null) {
                returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                return;
            }

            action.d20APerformer.GetDispatcher()?.Process(DispatcherType.PythonAdf, key, this);
        }

        public void DispatchPythonActionCheck(D20DispatcherKey key)
        {
            if (action == null || action.d20APerformer == null) {
                this.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                return;
            }

            action.d20APerformer.GetDispatcher()?.Process(DispatcherType.PythonActionCheck, key, this);
        }

        public void DispatchPythonActionAddToSeq(D20DispatcherKey key)
        {
            if (action == null || action.d20APerformer == null) {
                this.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                return;
            }

            action.d20APerformer.GetDispatcher()?.Process(DispatcherType.PythonActionAdd, key, this);
        }

        public void DispatchPythonActionPerform(D20DispatcherKey key)
        {
            if (action == null || action.d20APerformer == null) {
                this.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                return;
            }

            action.d20APerformer.GetDispatcher()?.Process(DispatcherType.PythonActionPerform, key, this);
        }

        public void DispatchPythonActionFrame(D20DispatcherKey key)
        {
            if (action == null || action.d20APerformer == null) {
                this.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                return;
            }

            action.d20APerformer.GetDispatcher()?.Process(DispatcherType.PythonActionFrame, key, this);
        }
    }

    public class DispIoMoveSpeed // dispIoType = 13, matches dispTypes 40,41
    {
        public BonusList bonlist;
        public float factor;

        public static DispIoMoveSpeed Default => new DispIoMoveSpeed
        {
            factor = 1.0f,
            bonlist = BonusList.Default
        };
    }

    public class DispIOBonusListAndSpellEntry
    {
        // Type 14
        public BonusList bonList;

        public SpellEntry spellEntry;
        public uint field_C; // unused?

        public static DispIOBonusListAndSpellEntry Default => new DispIOBonusListAndSpellEntry();
    };

    public class DispIoReflexThrow
    {
        // DispIoType = 15
        public int effectiveReduction;
        public D20SavingThrowReduction reduction;
        public int damageMesLine;
        public D20AttackPower attackPower;
        public DamageType attackType;
        public bool throwResult;
        public D20SavingThrowFlag flags;

        public static DispIoReflexThrow Default => new DispIoReflexThrow();
    };

    public class DispIoObjEvent // type 17
    {
        public int pad;
        public GameObjectBody aoeObj;
        public GameObjectBody tgt;
        public int evtId;

        public static DispIoObjEvent Default => new DispIoObjEvent();
    }

    public class DispIoAbilityLoss //  type 19
    {
        public int result;
        public Stat statDamaged;
        public int fieldC;

        public int spellId;

        // 8 - marked at the beginning of dispatch; 0x10 - checks against this in the Temp/Perm ability damage
        public int flags;

        public static DispIoAbilityLoss Default => new DispIoAbilityLoss
        {
            statDamaged = Stat.strength
        };
    };

    public class DispIoAttackDice // type 20
    {
        // TODO public BonusList* bonlist;
        public D20CAF flags;
        public int fieldC;
        public GameObjectBody weapon;
        public GameObjectBody wielder;
        public int dicePacked;
        public DamageType attackDamageType;

        public static DispIoAttackDice Default => new DispIoAttackDice()
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
        public static DispIoTypeImmunityTrigger Default => new DispIoTypeImmunityTrigger();
    }

    public class DispIoImmunity // type 23
    {
        public int returnVal;
        public int field8;

        public int flag;
        public SpellPacketBody spellPkt;
        public SpellEntry spellEntry;

        public static DispIoImmunity Default => new DispIoImmunity();
    }

    public class DispIoEffectTooltip // type 24
    {
        // TODO BuffDebuffPacket* bdb;

        /*
         spellEnum = -1 for no spell
        */
        // TODO void Append(int effectTypeId, int spellEnum, string text);

        public static DispIoEffectTooltip Default => new DispIoEffectTooltip();
    };


    public class EvtObjSpellCaster // type 34 (NEW!)
    {
        public BonusList bonlist;
        public GameObjectBody handle;
        public Stat arg0;

        public int arg1;
        // TODO SpellPacketBody* spellPkt;

        public static EvtObjSpellCaster Default => new EvtObjSpellCaster();
    }

    public class EvtObjMetaMagic // type 35 (NEW!)
    {
        public MetaMagicData mmData;

        public static EvtObjMetaMagic Default => new EvtObjMetaMagic();
    };

    public class EvtObjSpecialAttack // type 36 (NEW!)
    {
        enum AttackType
        {
            STUNNING_FIST = 1,
            NUM_EFFECTS
        };

        public int attack; //Uses the attack enum but unfortunately the enum can't be passed through to python
        public GameObjectBody target;

        public static EvtObjSpecialAttack Default => new EvtObjSpecialAttack();
    };

    public class EvtObjRangeIncrementBonus // type 37 (NEW!)
    {
        public GameObjectBody weaponUsed;
        public double rangeBonus;
    };

    public class EvtObjDealingSpellDamage // type 38 (NEW!)
    {
        public DamagePacket damage;
        public SpellPacketBody spellPkt;
        public GameObjectBody target;
    };

    public class EvtObjSpellTargetBonus // type 38 (NEW!)
    {
        public BonusList bonusList;
        public SpellPacketBody spellPkt;
        public GameObjectBody target;

        public static EvtObjSpellTargetBonus Default = new EvtObjSpellTargetBonus();
    }

    public class EvtObjActionCost
    {
        public ActionCostPacket acpOrig; // original
        public ActionCostPacket acpCur; // current
        public D20Action d20a;
        public TurnBasedStatus tbStat;

        public EvtObjActionCost(ActionCostPacket acp, TurnBasedStatus tbStatIn, D20Action d20aIn) {
            acpOrig = acp.Copy();
            acpCur = acp.Copy();
            d20a = d20aIn;
            tbStat = tbStatIn;
        }
    }

}