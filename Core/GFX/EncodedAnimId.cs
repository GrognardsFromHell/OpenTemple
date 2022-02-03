namespace OpenTemple.Core.GFX;

public enum WeaponAnim
{
    None = 0,
    RightAttack = 1,
    RightAttack2 = 2,
    RightAttack3 = 3,
    LeftAttack = 4,
    LeftAttack2 = 5,
    LeftAttack3 = 6,
    Walk = 7,
    Run = 8,
    Idle = 9,
    FrontHit = 10,
    FrontHit2 = 11,
    FrontHit3 = 12,
    LeftHit = 13,
    LeftHit2 = 14,
    LeftHit3 = 15,
    RightHit = 16,
    RightHit2 = 17,
    RightHit3 = 18,
    BackHit = 19,
    BackHit2 = 20,
    BackHit3 = 21,
    RightCriticalSwing = 22,
    LeftCriticalSwing = 23,
    Fidget = 24,
    Fidget2 = 25,
    Fidget3 = 26,
    Sneak = 27,
    Panic = 28,
    RightCombatStart = 29,
    LeftCombatStart = 30,
    CombatIdle = 31,
    CombatFidget = 32,
    Special1 = 33,
    Special2 = 34,
    Special3 = 35,
    FrontDodge = 36,
    RightDodge = 37,
    LeftDodge = 38,
    BackDodge = 39,
    RightThrow = 40,
    LeftThrow = 41,
    LeftSnatch = 42,
    RightSnatch = 43,
    LeftTurn = 44,
    RightTurn = 45
}

public enum WeaponAnimType
{
    Unarmed = 0,
    Dagger = 1,
    Sword = 2,
    Mace = 3,
    Hammer = 4,
    Axe = 5,
    Club = 6,
    Battleaxe = 7,
    Greatsword = 8,
    Greataxe = 9,
    Greathammer = 10,
    Spear = 11,
    Staff = 12,
    Polearm = 13,
    Bow = 14,
    Crossbow = 15,
    Sling = 16,
    Shield = 17,
    Flail = 18,
    Chain = 19,
    TwoHandedFlail = 20,
    Shuriken = 21,
    Monk = 22
}

public enum BardInstrumentType
{
    Flute = 0,
    Drum,
    Mandolin,
    Trumpet,
    Harp,
    Lute,
    Pipers,
    Recorder
}

public enum NormalAnimType
{
    Falldown = 0,
    ProneIdle = 1,
    ProneFidget = 2,
    Getup = 3,
    Magichands = 4,
    Picklock = 5,
    PicklockConcentrated = 6,
    Examine = 7,
    Throw = 8,
    Death = 9,
    Death2 = 10,
    Death3 = 11,
    DeadIdle = 12,
    DeadFidget = 13,
    DeathProneIdle = 14,
    DeathProneFidget = 15,
    AbjurationCasting = 16,
    AbjurationConjuring = 17,
    ConjurationCasting = 18,
    ConjurationConjuring = 19,
    DivinationCasting = 20,
    DivinationConjuring = 21,
    EnchantmentCasting = 22,
    EnchantmentConjuring = 23,
    EvocationCasting = 24,
    EvocationConjuring = 25,
    IllusionCasting = 26,
    IllusionConjuring = 27,
    NecromancyCasting = 28,
    NecromancyConjuring = 29,
    TransmutationCasting = 30,
    TransmutationConjuring = 31,
    Conceal = 32,
    ConcealIdle = 33,
    Unconceal = 34,
    ItemIdle = 35,
    ItemFidget = 36,
    Open = 37,
    Close = 38,
    SkillAnimalEmpathy = 39,
    SkillDisableDevice = 40,
    SkillHeal = 41,
    SkillHealConcentrated = 42,
    SkillHide = 43,
    SkillHideIdle = 44,
    SkillHideFidget = 45,
    SkillUnhide = 46,
    SkillPickpocket = 47,
    SkillSearch = 48,
    SkillSpot = 49,
    FeatTrack = 50,
    Trip = 51,
    Bullrush = 52,
    Flurry = 53,
    Kistrike = 54,
    Tumble = 55,
    Special1 = 56,
    Special2 = 57,
    Special3 = 58,
    Special4 = 59,
    Throw2 = 60,
    WandAbjurationCasting = 61,
    WandAbjurationConjuring = 62,
    WandConjurationCasting = 63,
    WandConjurationConjuring = 64,
    WandDivinationCasting = 65,
    WandDivinationConjuring = 66,
    WandEnchantmentCasting = 67,
    WandEnchantmentConjuring = 68,
    WandEvocationCasting = 69,
    WandEvocationConjuring = 70,
    WandIllusionCasting = 71,
    WandIllusionConjuring = 72,
    WandNecromancyCasting = 73,
    WandNecromancyConjuring = 74,
    WandTransmutationCasting = 75,
    WandTransmutationConjuring = 76,
    SkillBarbarianRage = 77,
    OpenIdle = 78
}

/*
Represents an encoded animation id.
*/
public struct EncodedAnimId
{
    public EncodedAnimId(int id)
    {
        mId = id;
    }

    public EncodedAnimId(WeaponAnim anim,
        WeaponAnimType mainHand = WeaponAnimType.Unarmed,
        WeaponAnimType offHand = WeaponAnimType.Unarmed) : this(sWeaponAnimFlag)
    {
        var animId = (int) anim;
        var mainHandId = (int) mainHand;
        var offHandId = (int) offHand;

        mId |= animId & 0xFFFFF;
        mId |= mainHandId << 20;
        mId |= offHandId << 25;
    }

    public EncodedAnimId(BardInstrumentType instrumentType) : this(sBardInstrumentAnimFlag)
    {
        mId |= (int) instrumentType;
    }

    public EncodedAnimId(NormalAnimType animType) : this((int) animType)
    {
    }

    public static implicit operator int(EncodedAnimId animId)
    {
        return animId.mId;
    }

    /**
         * If this animation is a conjuration animation, returns the corresponding casting animation.
         */
    public bool ToCastingAnim(out EncodedAnimId animId)
    {
        if (IsSpecialAnim())
        {
            animId = default;
            return false;
        }

        var normalAnim = GetNormalAnimType();

        switch (normalAnim)
        {
            case NormalAnimType.AbjurationConjuring:
                normalAnim = NormalAnimType.AbjurationCasting;
                break;
            case NormalAnimType.ConjurationConjuring:
                normalAnim = NormalAnimType.ConjurationCasting;
                break;
            case NormalAnimType.DivinationConjuring:
                normalAnim = NormalAnimType.DivinationCasting;
                break;
            case NormalAnimType.EnchantmentConjuring:
                normalAnim = NormalAnimType.EnchantmentCasting;
                break;
            case NormalAnimType.EvocationConjuring:
                normalAnim = NormalAnimType.EvocationCasting;
                break;
            case NormalAnimType.IllusionConjuring:
                normalAnim = NormalAnimType.IllusionCasting;
                break;
            case NormalAnimType.NecromancyConjuring:
                normalAnim = NormalAnimType.NecromancyCasting;
                break;
            case NormalAnimType.TransmutationConjuring:
                normalAnim = NormalAnimType.TransmutationCasting;
                break;
            case NormalAnimType.WandAbjurationConjuring:
                normalAnim = NormalAnimType.WandAbjurationCasting;
                break;
            case NormalAnimType.WandConjurationConjuring:
                normalAnim = NormalAnimType.WandConjurationCasting;
                break;
            case NormalAnimType.WandDivinationConjuring:
                normalAnim = NormalAnimType.WandDivinationCasting;
                break;
            case NormalAnimType.WandEnchantmentConjuring:
                normalAnim = NormalAnimType.WandEnchantmentCasting;
                break;
            case NormalAnimType.WandEvocationConjuring:
                normalAnim = NormalAnimType.WandEvocationCasting;
                break;
            case NormalAnimType.WandIllusionConjuring:
                normalAnim = NormalAnimType.WandIllusionCasting;
                break;
            case NormalAnimType.WandNecromancyConjuring:
                normalAnim = NormalAnimType.WandNecromancyCasting;
                break;
            case NormalAnimType.WandTransmutationConjuring:
                normalAnim = NormalAnimType.WandTransmutationCasting;
                break;
            default:
                animId = default;
                return false;
        }

        animId = new EncodedAnimId(normalAnim);
        return true;
    }

    public bool IsConjureAnimation()
    {
        if (IsSpecialAnim())
        {
            return false;
        }

        var normalAnim = GetNormalAnimType();

        switch (normalAnim)
        {
            case NormalAnimType.AbjurationConjuring:
            case NormalAnimType.ConjurationConjuring:
            case NormalAnimType.DivinationConjuring:
            case NormalAnimType.EnchantmentConjuring:
            case NormalAnimType.EvocationConjuring:
            case NormalAnimType.IllusionConjuring:
            case NormalAnimType.NecromancyConjuring:
            case NormalAnimType.TransmutationConjuring:
            case NormalAnimType.WandAbjurationConjuring:
            case NormalAnimType.WandConjurationConjuring:
            case NormalAnimType.WandDivinationConjuring:
            case NormalAnimType.WandEnchantmentConjuring:
            case NormalAnimType.WandEvocationConjuring:
            case NormalAnimType.WandIllusionConjuring:
            case NormalAnimType.WandNecromancyConjuring:
            case NormalAnimType.WandTransmutationConjuring:
                return true;
            default:
                return false;
        }
    }

    public bool IsSpecialAnim()
    {
        return (mId & (sWeaponAnimFlag | sBardInstrumentAnimFlag)) != 0;
    }

    // Not for weapon/bard anims
    public NormalAnimType GetNormalAnimType()
    {
        return (NormalAnimType) mId;
    }

    public bool IsWeaponAnim()
    {
        return (mId & sWeaponAnimFlag) != 0;
    }

    // Only valid for weapon animations
    public WeaponAnimType GetWeaponLeftHand()
    {
        return (WeaponAnimType) ((mId >> 20) & 0x1F);
    }

    // Only valid for weapon animations
    public WeaponAnimType GetWeaponRightHand()
    {
        return (WeaponAnimType) ((mId >> 25) & 0x1F);
    }

    // Only valid for weapon animations
    public WeaponAnim GetWeaponAnim()
    {
        return (WeaponAnim) (mId & 0xFFFFF);
    }

    public bool IsBardInstrumentAnim()
    {
        return (mId & sBardInstrumentAnimFlag) != 0;
    }

    // Only valid for bard instrument anim
    public BardInstrumentType GetBardInstrumentType()
    {
        return (BardInstrumentType) (mId & 7);
    }

    private const string sUnknown = "<unknown>";

    private static readonly string[] sBardInstrumentTypeNames =
    {
        "bard_flute",
        "bard_drum",
        "bard_mandolin",
        "bard_trumpet",
        "bard_harp",
        "bard_lute",
        "bard_pipes",
        "bard_recorder"
    };

    private static readonly string[] sWeaponTypeNames =
    {
        "unarmed",
        "dagger",
        "sword",
        "mace",
        "hammer",
        "axe",
        "club",
        "battleaxe",
        "greatsword",
        "greataxe",
        "greathammer",
        "spear",
        "staff",
        "polearm",
        "bow",
        "crossbow",
        "sling",
        "shield",
        "flail",
        "chain",
        "2hflail",
        "shuriken",
        "monk"
    };

    private static readonly string[] sWeaponAnimNames =
    {
        "none",
        "rattack",
        "rattack2",
        "rattack3",
        "lattack",
        "lattack2",
        "lattack3",
        "walk",
        "run",
        "idle",
        "fhit",
        "fhit2",
        "fhit3",
        "lhit",
        "lhit2",
        "lhit3",
        "rhit",
        "rhit2",
        "rhit3",
        "bhit",
        "bhit2",
        "bhit3",
        "rcriticalswing",
        "lcriticalswing",
        "fidget",
        "fidget2",
        "fidget3",
        "sneak",
        "panic",
        "rcombatstart",
        "lcombatstart",
        "combatidle",
        "combatfidget",
        "special1",
        "special2",
        "special3",
        "fdodge",
        "rdodge",
        "ldodge",
        "bdodge",
        "rthrow",
        "lthrow",
        "lsnatch",
        "rsnatch",
        "lturn",
        "rturn"
    };

    private static readonly string[] sNormalAnimNames =
    {
        "falldown",
        "prone_idle",
        "prone_fidget",
        "getup",
        "magichands",
        "picklock",
        "picklock_concentrated",
        "examine",
        "throw",
        "death",
        "death2",
        "death3",
        "dead_idle",
        "dead_fidget",
        "death_prone_idle",
        "death_prone_fidget",
        "abjuration_casting",
        "abjuration_conjuring",
        "conjuration_casting",
        "conjuration_conjuring",
        "divination_casting",
        "divination_conjuring",
        "enchantment_casting",
        "enchantment_conjuring",
        "evocation_casting",
        "evocation_conjuring",
        "illusion_casting",
        "illusion_conjuring",
        "necromancy_casting",
        "necromancy_conjuring",
        "transmutation_casting",
        "transmutation_conjuring",
        "conceal",
        "conceal_idle",
        "unconceal",
        "item_idle",
        "item_fidget",
        "open",
        "close",
        "skill_animal_empathy",
        "skill_disable_device",
        "skill_heal",
        "skill_heal_concentrated",
        "skill_hide",
        "skill_hide_idle",
        "skill_hide_fidget",
        "skill_unhide",
        "skill_pickpocket",
        "skill_search",
        "skill_spot",
        "feat_track",
        "trip",
        "bullrush",
        "flurry",
        "kistrike",
        "tumble",
        "special1",
        "special2",
        "special3",
        "special4",
        "throw",
        "wand_abjuration_casting",
        "wand_abjuration_conjuring",
        "wand_conjuration_casting",
        "wand_conjuration_conjuring",
        "wand_divination_casting",
        "wand_divination_conjuring",
        "wand_enchantment_casting",
        "wand_enchantment_conjuring",
        "wand_evocation_casting",
        "wand_evocation_conjuring",
        "wand_illusion_casting",
        "wand_illusion_conjuring",
        "wand_necromancy_casting",
        "wand_necromancy_conjuring",
        "wand_transmutation_casting",
        "wand_transmutation_conjuring",
        "skill_barbarian_rage",
        "open_idle"
    };

    private static string GetBardInstrumentTypeName(BardInstrumentType instrumentType)
    {
        return sBardInstrumentTypeNames[(int) instrumentType];
    }

    private static string GetNormalAnimTypeName(NormalAnimType animType)
    {
        var idx = (int) animType;
        if (idx >= 79)
        {
            idx = 0;
        }

        return sNormalAnimNames[idx];
    }

    private static string GetWeaponAnimName(WeaponAnim weaponAnim)
    {
        return sWeaponAnimNames[(int) weaponAnim];
    }

    private static string GetWeaponTypeName(WeaponAnimType weaponAnimType)
    {
        return sWeaponTypeNames[(int) weaponAnimType];
    }

    public string GetName()
    {
        if (IsWeaponAnim())
        {
            return
                $"{GetWeaponTypeName(GetWeaponLeftHand())}_{GetWeaponTypeName(GetWeaponRightHand())}_{GetWeaponAnimName(GetWeaponAnim())}";
        }

        if (IsBardInstrumentAnim())
        {
            return GetBardInstrumentTypeName(GetBardInstrumentType());
        }

        return GetNormalAnimTypeName(GetNormalAnimType());
    }

// True if the weapon has to fallback on both hands
    private static bool IsWeapon2hFallback(WeaponAnimType weaponType)
    {
        switch (weaponType)
        {
            case WeaponAnimType.Greatsword:
            case WeaponAnimType.Greataxe:
            case WeaponAnimType.Greathammer:
            case WeaponAnimType.Spear:
            case WeaponAnimType.Staff:
            case WeaponAnimType.Polearm:
            case WeaponAnimType.Bow:
            case WeaponAnimType.Crossbow:
            case WeaponAnimType.Chain:
            case WeaponAnimType.TwoHandedFlail:
            case WeaponAnimType.Shuriken:
            case WeaponAnimType.Monk:
                return true;
            default:
                return false;
        }
    }

    private static WeaponAnimType GetWeaponFallback(WeaponAnimType weaponType)
    {
        switch (weaponType)
        {
            // Oddly enough these are not marked as having substitutes, so i wonder
            // if they are actually used...
            case WeaponAnimType.Mace:
            case WeaponAnimType.Hammer:
            case WeaponAnimType.Axe:
            case WeaponAnimType.Club:
            case WeaponAnimType.Battleaxe:
                return WeaponAnimType.Sword;

            case WeaponAnimType.Greataxe:
            case WeaponAnimType.Greathammer:
                return WeaponAnimType.Greatsword;

            case WeaponAnimType.Shield:
                return WeaponAnimType.Unarmed;

            case WeaponAnimType.Flail:
                return WeaponAnimType.Sword;

            case WeaponAnimType.TwoHandedFlail:
                return WeaponAnimType.Polearm;

            case WeaponAnimType.Shuriken:
            case WeaponAnimType.Monk:
                return WeaponAnimType.Unarmed;

            default:
                return weaponType;
        }
    }

    private static WeaponAnim GetWeaponAnimFallback(WeaponAnim weaponAnim)
    {
        switch (weaponAnim)
        {
            case WeaponAnim.RightAttack2:
                return WeaponAnim.RightAttack;
            case WeaponAnim.RightAttack3:
                return WeaponAnim.RightAttack2;
            case WeaponAnim.LeftAttack2:
                return WeaponAnim.LeftAttack;
            case WeaponAnim.LeftAttack3:
                return WeaponAnim.LeftAttack2;
            case WeaponAnim.Run:
                return WeaponAnim.Walk;
            case WeaponAnim.FrontHit:
                return WeaponAnim.RightAttack;
            case WeaponAnim.FrontHit2:
                return WeaponAnim.FrontHit;
            case WeaponAnim.FrontHit3:
                return WeaponAnim.FrontHit2;
            case WeaponAnim.LeftHit:
                return WeaponAnim.FrontHit;
            case WeaponAnim.LeftHit2:
                return WeaponAnim.LeftHit;
            case WeaponAnim.LeftHit3:
                return WeaponAnim.LeftHit2;
            case WeaponAnim.RightHit:
                return WeaponAnim.FrontHit;
            case WeaponAnim.RightHit2:
                return WeaponAnim.RightHit;
            case WeaponAnim.RightHit3:
                return WeaponAnim.RightHit2;
            case WeaponAnim.BackHit:
                return WeaponAnim.FrontHit;
            case WeaponAnim.BackHit2:
                return WeaponAnim.BackHit;
            case WeaponAnim.BackHit3:
                return WeaponAnim.BackHit2;
            case WeaponAnim.RightCriticalSwing:
                return WeaponAnim.RightAttack;
            case WeaponAnim.LeftCriticalSwing:
                return WeaponAnim.LeftAttack;
            case WeaponAnim.Fidget2:
                return WeaponAnim.Fidget;
            case WeaponAnim.Fidget3:
                return WeaponAnim.Fidget2;
            case WeaponAnim.Sneak:
                return WeaponAnim.Walk;
            case WeaponAnim.Panic:
                return WeaponAnim.Fidget;
            case WeaponAnim.RightCombatStart:
            case WeaponAnim.LeftCombatStart:
                return WeaponAnim.CombatFidget;
            case WeaponAnim.CombatIdle:
                return WeaponAnim.Idle;
            case WeaponAnim.CombatFidget:
                return WeaponAnim.Fidget;
            case WeaponAnim.Special1:
            case WeaponAnim.Special2:
            case WeaponAnim.Special3:
            case WeaponAnim.FrontDodge:
                return WeaponAnim.RightAttack;
            case WeaponAnim.RightDodge:
            case WeaponAnim.LeftDodge:
            case WeaponAnim.BackDodge:
                return WeaponAnim.FrontDodge;
            case WeaponAnim.RightThrow:
                return WeaponAnim.RightAttack;
            case WeaponAnim.LeftThrow:
                return WeaponAnim.LeftAttack;
            case WeaponAnim.LeftSnatch:
                return WeaponAnim.RightAttack;
            case WeaponAnim.RightSnatch:
                return WeaponAnim.LeftAttack;
            default:
                return WeaponAnim.None;
        }
    }

    private static bool GetNormalAnimFallback(NormalAnimType anim, ref EncodedAnimId animId)
    {
        switch (anim)
        {
            case NormalAnimType.ProneIdle:
                animId = new EncodedAnimId(NormalAnimType.DeadIdle);
                return true;
            case NormalAnimType.ProneFidget:
                animId = new EncodedAnimId(NormalAnimType.ProneIdle);
                return true;
            case NormalAnimType.Getup:
                animId = new EncodedAnimId(WeaponAnim.CombatFidget);
                return true;
            case NormalAnimType.Magichands:
                animId = new EncodedAnimId(WeaponAnim.RightAttack);
                return true;
            case NormalAnimType.Picklock:
                animId = new EncodedAnimId(NormalAnimType.Magichands);
                return true;
            case NormalAnimType.PicklockConcentrated:
                animId = new EncodedAnimId(NormalAnimType.Picklock);
                return true;
            case NormalAnimType.Examine:
                animId = new EncodedAnimId(NormalAnimType.Magichands);
                return true;
            case NormalAnimType.Throw:
                animId = new EncodedAnimId(NormalAnimType.ItemIdle);
                return true;
            case NormalAnimType.Death:
                animId = new EncodedAnimId(NormalAnimType.ItemIdle);
                return true;
            case NormalAnimType.Death2:
                animId = new EncodedAnimId(NormalAnimType.Death);
                return true;
            case NormalAnimType.Death3:
                animId = new EncodedAnimId(NormalAnimType.Death2);
                return true;
            case NormalAnimType.DeadIdle:
                animId = new EncodedAnimId(NormalAnimType.ItemIdle);
                return true;
            case NormalAnimType.DeadFidget:
                animId = new EncodedAnimId(NormalAnimType.DeadIdle);
                return true;
            case NormalAnimType.DeathProneIdle:
                animId = new EncodedAnimId(NormalAnimType.DeadIdle);
                return true;
            case NormalAnimType.DeathProneFidget:
                animId = new EncodedAnimId(NormalAnimType.DeathProneIdle);
                return true;
            case NormalAnimType.AbjurationCasting:
            case NormalAnimType.AbjurationConjuring:
                animId = new EncodedAnimId(WeaponAnim.RightAttack);
                return true;
            case NormalAnimType.ConjurationCasting:
                animId = new EncodedAnimId(NormalAnimType.AbjurationCasting);
                return true;
            case NormalAnimType.ConjurationConjuring:
                animId = new EncodedAnimId(NormalAnimType.AbjurationConjuring);
                return true;
            case NormalAnimType.DivinationCasting:
                animId = new EncodedAnimId(NormalAnimType.AbjurationCasting);
                return true;
            case NormalAnimType.DivinationConjuring:
                animId = new EncodedAnimId(NormalAnimType.AbjurationConjuring);
                return true;
            case NormalAnimType.EnchantmentCasting:
                animId = new EncodedAnimId(NormalAnimType.AbjurationCasting);
                return true;
            case NormalAnimType.EnchantmentConjuring:
                animId = new EncodedAnimId(NormalAnimType.AbjurationConjuring);
                return true;
            case NormalAnimType.EvocationCasting:
                animId = new EncodedAnimId(NormalAnimType.AbjurationCasting);
                return true;
            case NormalAnimType.EvocationConjuring:
                animId = new EncodedAnimId(NormalAnimType.AbjurationConjuring);
                return true;
            case NormalAnimType.IllusionCasting:
                animId = new EncodedAnimId(NormalAnimType.AbjurationCasting);
                return true;
            case NormalAnimType.IllusionConjuring:
                animId = new EncodedAnimId(NormalAnimType.AbjurationConjuring);
                return true;
            case NormalAnimType.NecromancyCasting:
                animId = new EncodedAnimId(NormalAnimType.AbjurationCasting);
                return true;
            case NormalAnimType.NecromancyConjuring:
                animId = new EncodedAnimId(NormalAnimType.AbjurationConjuring);
                return true;
            case NormalAnimType.TransmutationCasting:
                animId = new EncodedAnimId(NormalAnimType.AbjurationCasting);
                return true;
            case NormalAnimType.TransmutationConjuring:
                animId = new EncodedAnimId(NormalAnimType.AbjurationConjuring);
                return true;
            case NormalAnimType.Conceal:
                animId = new EncodedAnimId(WeaponAnim.RightAttack);
                return true;
            case NormalAnimType.ConcealIdle:
                animId = new EncodedAnimId(WeaponAnim.Idle);
                return true;
            case NormalAnimType.Unconceal:
                animId = new EncodedAnimId(NormalAnimType.Getup);
                return true;
            case NormalAnimType.ItemFidget:
            case NormalAnimType.Open:
            case NormalAnimType.Close:
                animId = new EncodedAnimId(NormalAnimType.ItemIdle);
                return true;
            case NormalAnimType.SkillAnimalEmpathy:
            case NormalAnimType.SkillDisableDevice:
            case NormalAnimType.SkillHeal:
                animId = new EncodedAnimId(NormalAnimType.Magichands);
                return true;
            case NormalAnimType.SkillHealConcentrated:
                animId = new EncodedAnimId(NormalAnimType.SkillHeal);
                return true;
            case NormalAnimType.SkillHide:
                animId = new EncodedAnimId(NormalAnimType.ItemIdle);
                return true;
            case NormalAnimType.SkillHideIdle:
                animId = new EncodedAnimId(WeaponAnim.Idle);
                return true;
            case NormalAnimType.SkillHideFidget:
                animId = new EncodedAnimId(WeaponAnim.CombatFidget);
                return true;
            case NormalAnimType.SkillUnhide:
                animId = new EncodedAnimId(NormalAnimType.ItemIdle);
                return true;
            case NormalAnimType.SkillPickpocket:
            case NormalAnimType.SkillSearch:
            case NormalAnimType.SkillSpot:
            case NormalAnimType.FeatTrack:
            case NormalAnimType.Trip:
            case NormalAnimType.Bullrush:
            case NormalAnimType.Flurry:
            case NormalAnimType.Kistrike:
            case NormalAnimType.Tumble:
                animId = new EncodedAnimId(NormalAnimType.Magichands);
                return true;
            case NormalAnimType.Special1:
            case NormalAnimType.Special2:
            case NormalAnimType.Special3:
            case NormalAnimType.Special4:
            case NormalAnimType.Throw2:
                animId = new EncodedAnimId(NormalAnimType.ItemIdle);
                return true;
            case NormalAnimType.WandAbjurationCasting:
                animId = new EncodedAnimId(NormalAnimType.AbjurationCasting);
                return true;
            case NormalAnimType.WandAbjurationConjuring:
                animId = new EncodedAnimId(NormalAnimType.AbjurationConjuring);
                return true;
            case NormalAnimType.WandConjurationCasting:
                animId = new EncodedAnimId(NormalAnimType.WandAbjurationCasting);
                return true;
            case NormalAnimType.WandConjurationConjuring:
                animId = new EncodedAnimId(NormalAnimType.WandAbjurationConjuring);
                return true;
            case NormalAnimType.WandDivinationCasting:
                animId = new EncodedAnimId(NormalAnimType.WandAbjurationCasting);
                return true;
            case NormalAnimType.WandDivinationConjuring:
                animId = new EncodedAnimId(NormalAnimType.WandAbjurationConjuring);
                return true;
            case NormalAnimType.WandEnchantmentCasting:
                animId = new EncodedAnimId(NormalAnimType.WandAbjurationCasting);
                return true;
            case NormalAnimType.WandEnchantmentConjuring:
                animId = new EncodedAnimId(NormalAnimType.WandAbjurationConjuring);
                return true;
            case NormalAnimType.WandEvocationCasting:
                animId = new EncodedAnimId(NormalAnimType.WandAbjurationCasting);
                return true;
            case NormalAnimType.WandEvocationConjuring:
                animId = new EncodedAnimId(NormalAnimType.WandAbjurationConjuring);
                return true;
            case NormalAnimType.WandIllusionCasting:
                animId = new EncodedAnimId(NormalAnimType.WandAbjurationCasting);
                return true;
            case NormalAnimType.WandIllusionConjuring:
                animId = new EncodedAnimId(NormalAnimType.WandAbjurationConjuring);
                return true;
            case NormalAnimType.WandNecromancyCasting:
                animId = new EncodedAnimId(NormalAnimType.WandAbjurationCasting);
                return true;
            case NormalAnimType.WandNecromancyConjuring:
                animId = new EncodedAnimId(NormalAnimType.WandAbjurationConjuring);
                return true;
            case NormalAnimType.WandTransmutationCasting:
                animId = new EncodedAnimId(NormalAnimType.WandAbjurationCasting);
                return true;
            case NormalAnimType.WandTransmutationConjuring:
                animId = new EncodedAnimId(NormalAnimType.WandAbjurationConjuring);
                return true;
            case NormalAnimType.SkillBarbarianRage:
                animId = new EncodedAnimId(WeaponAnim.RightAttack);
                return true;
            case NormalAnimType.OpenIdle:
                animId = new EncodedAnimId(NormalAnimType.ItemIdle);
                return true;
            default:
                return false;
        }
    }


    public static bool GetFallbackAnim(ref EncodedAnimId animId)
    {
        if (animId.IsBardInstrumentAnim())
        {
            // There are no fallback animations for bard instruments.
            // Apparently there was a table, but no instrument had a fallback in there
            return false;
        }

        if (animId.IsWeaponAnim())
        {
            var leftHand = animId.GetWeaponLeftHand();
            var rightHand = animId.GetWeaponRightHand();

            // Possibly the weapons in either hand need to fallback together
            if (IsWeapon2hFallback(leftHand) || IsWeapon2hFallback(rightHand))
            {
                var leftHandFallback = GetWeaponFallback(leftHand);
                var rightHandFallback = GetWeaponFallback(rightHand);

                if (leftHandFallback != leftHand && rightHandFallback != rightHand)
                {
                    animId = new EncodedAnimId(animId.GetWeaponAnim(), leftHandFallback, rightHandFallback);
                    return true;
                }
            }
            else
            {
                var rightHandFallback = GetWeaponFallback(rightHand);
                if (rightHandFallback != rightHand)
                {
                    animId = new EncodedAnimId(animId.GetWeaponAnim(), leftHand, rightHandFallback);
                    return true;
                }

                var leftHandFallback = GetWeaponFallback(leftHand);
                if (leftHandFallback != leftHand)
                {
                    animId = new EncodedAnimId(animId.GetWeaponAnim(), leftHandFallback, rightHand);
                    return true;
                }
            }

            var weaponAnimFallback = GetWeaponAnimFallback(animId.GetWeaponAnim());
            if (weaponAnimFallback == WeaponAnim.None)
            {
                return false;
            }

            animId = new EncodedAnimId(weaponAnimFallback, leftHand, rightHand);
            return true;
        }

        // Normal animations can fall back to weapon animations, so it's not just a lookup table here
        return GetNormalAnimFallback(animId.GetNormalAnimType(), ref animId);
    }

    public bool ToFallback()
    {
        return GetFallbackAnim(ref this);
    }

    // Indicates that an animation id uses the encoded format
    private const int sWeaponAnimFlag = 1 << 30;
    private const int sBardInstrumentAnimFlag = 1 << 31;

    private readonly int mId;
}