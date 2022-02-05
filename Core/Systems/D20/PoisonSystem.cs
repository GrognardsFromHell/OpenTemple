using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20;

public class PoisonSpec
{
    public int Id { get; }

    public int DC { get; }

    /// <summary>
    /// ID of the line in combat.mes that has the name of this poison.
    /// </summary>
    public int NameId { get; }

    public SavingThrowType SavingThrowType { get; } = SavingThrowType.Fortitude;

    public IImmutableList<PoisonEffect> ImmediateEffects { get; }

    public IImmutableList<PoisonEffect> DelayedEffects { get; }

    public PoisonSpec(int id,
        int nameId,
        int dc,
        IEnumerable<PoisonEffect> immediateEffects = null,
        IEnumerable<PoisonEffect> delayedEffects = null)
    {
        Id = id;
        NameId = nameId;
        DC = dc;
        ImmediateEffects = immediateEffects?.ToImmutableList() ?? ImmutableList<PoisonEffect>.Empty;
        DelayedEffects = delayedEffects?.ToImmutableList() ?? ImmutableList<PoisonEffect>.Empty;
    }
}

public struct PoisonEffect
{
    public PoisonEffectType Type { get; }

    public Dice Dice { get; }

    public PoisonEffect(PoisonEffectType type) : this(type, Dice.Zero)
    {
    }

    public PoisonEffect(PoisonEffectType type, Dice dice)
    {
        Type = type;
        Dice = dice;
    }
}

public enum PoisonEffectType
{
    None = -10,
    Unconsciousness = -9,
    Paralyze = -8,
    HPDamage = -7,
    StrPermanent = -6,
    DexPermanent = -5,
    ConPermanent = -4,
    IntPermanent = -3,
    WisPermanent = -2,
    ChaPermanent = -1,
    StrTemporary = 0,
    DexTemporary = 1,
    ConTemporary = 2,
    IntTemporary = 3,
    WisTemporary = 4,
    ChaTemporary = 5
}

public class PoisonSystem
{
    private readonly Dictionary<int, PoisonSpec> _poisons = new();

    public PoisonSystem()
    {
        InitVanillaPoisons();
    }

    public PoisonSpec GetPoison(int id)
    {
        return _poisons[id];
    }

    public string GetPoisonName(PoisonSpec spec)
    {
        return GameSystems.D20.Combat.GetCombatMesLine(spec.NameId);
    }

    public void ApplyPoisonEffect(GameObject critter, PoisonEffect effect)
    {
        void DealAbilityDamage(Stat stat, bool temporary)
        {
            GameSystems.D20.Combat.FloatCombatLine(critter, 56);
            GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(21, critter, null); // "X takes poison damage!"
            GameSystems.D20.Combat.FloatCombatLine(critter, 96);

            var rollRes = effect.Dice.Roll();
            critter.AddCondition(StatusEffects.TempAbilityLoss, (int) stat, rollRes);
        }

        switch (effect.Type)
        {
            case PoisonEffectType.Paralyze:
                // x10 due to minutes, not rounds
                var rollResParalyzedRounds = new Dice(2, 6, 0).Roll() * 10;
                critter.AddCondition(StatusEffects.Paralyzed, rollResParalyzedRounds, 0, 0);
                break;

            case PoisonEffectType.HPDamage:
                var dice = effect.Dice;
                GameSystems.D20.Combat.DoDamage(critter, null, dice, DamageType.Poison, D20AttackPower.UNSPECIFIED, 100,
                    0, D20ActionType.NONE);
                break;

            case PoisonEffectType.Unconsciousness:
                critter.AddCondition(StatusEffects.Unconscious);
                GameSystems.Anim.PushAnimate(critter, NormalAnimType.Falldown);
                GameSystems.D20.Combat.FloatCombatLine(critter, 17); // Unconscious!
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(16, critter,
                    null); // [ACTOR] falls ~unconscious~[TAG_UNCONSCIOUS]!
                break;

            case PoisonEffectType.StrPermanent:
                DealAbilityDamage(Stat.strength, true);
                break;
            case PoisonEffectType.StrTemporary:
                DealAbilityDamage(Stat.strength, false);
                break;
            case PoisonEffectType.DexPermanent:
                DealAbilityDamage(Stat.dexterity, true);
                break;
            case PoisonEffectType.DexTemporary:
                DealAbilityDamage(Stat.dexterity, false);
                break;
            case PoisonEffectType.ConPermanent:
                DealAbilityDamage(Stat.constitution, true);
                break;
            case PoisonEffectType.ConTemporary:
                DealAbilityDamage(Stat.constitution, false);
                break;
            case PoisonEffectType.IntPermanent:
                DealAbilityDamage(Stat.intelligence, true);
                break;
            case PoisonEffectType.IntTemporary:
                DealAbilityDamage(Stat.intelligence, false);
                break;
            case PoisonEffectType.WisPermanent:
                DealAbilityDamage(Stat.wisdom, true);
                break;
            case PoisonEffectType.WisTemporary:
                DealAbilityDamage(Stat.wisdom, false);
                break;
            case PoisonEffectType.ChaPermanent:
                DealAbilityDamage(Stat.charisma, true);
                break;
            case PoisonEffectType.ChaTemporary:
                DealAbilityDamage(Stat.charisma, false);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void InitVanillaPoisons()
    {
        // Small Centipede
        _poisons[0] = new PoisonSpec(0, 300,11,
            new[]
            {
                new PoisonEffect(PoisonEffectType.DexTemporary, Dice.D2),
            }
        );
        // Greenblood Oil
        _poisons[1] = new PoisonSpec(1, 301,13,
            new[]
            {
                new PoisonEffect(PoisonEffectType.ConTemporary, Dice.D2),
            }
        );
        // Spider Venom
        _poisons[2] = new PoisonSpec(2, 302,14,
            new[]
            {
                new PoisonEffect(PoisonEffectType.StrTemporary, Dice.D6),
            }
        );
        // Blood Root
        _poisons[3] = new PoisonSpec(3, 300+3,12,
            new[]
            {
                new PoisonEffect(PoisonEffectType.ConTemporary, Dice.D4),
                new PoisonEffect(PoisonEffectType.WisTemporary, Dice.D3),
            }
        );
        // Purple Worm
        _poisons[4] = new PoisonSpec(4, 304,24,
            new[]
            {
                new PoisonEffect(PoisonEffectType.StrTemporary, Dice.D6),
            }
        );
        // Large Scorpion
        _poisons[5] = new PoisonSpec(5, 305,18,
            new[]
            {
                new PoisonEffect(PoisonEffectType.StrTemporary, Dice.D6),
            }
        );
        // Wyvern
        _poisons[6] = new PoisonSpec(6, 306,17,
            new[]
            {
                new PoisonEffect(PoisonEffectType.ConTemporary, new Dice(2, 6)),
            }
        );
        // Giant Wasp
        _poisons[7] = new PoisonSpec(7, 307,18,
            new[]
            {
                new PoisonEffect(PoisonEffectType.DexTemporary, Dice.D6),
            }
        );
        // Black Adder
        _poisons[8] = new PoisonSpec(8, 308,12,
            new[]
            {
                new PoisonEffect(PoisonEffectType.StrTemporary, Dice.D6),
            }
        );
        // Malyss Root Paste
        _poisons[9] = new PoisonSpec(9, 309,16,
            new[]
            {
                new PoisonEffect(PoisonEffectType.DexTemporary, new Dice(2, 4)),
            }
        );
        // Dragon Bile
        _poisons[10] = new PoisonSpec(10, 310,26,
            new[]
            {
                new PoisonEffect(PoisonEffectType.StrTemporary, new Dice(3, 6)),
            }
        );
        // Sassone Leaf Residue
        _poisons[11] = new PoisonSpec(11, 311,16,
            new[]
            {
                new PoisonEffect(PoisonEffectType.ConTemporary, Dice.D6),
            }
        );
        // Terinav Root
        _poisons[12] = new PoisonSpec(12, 312,16,
            new[]
            {
                new PoisonEffect(PoisonEffectType.DexTemporary, new Dice(2, 6)),
            }
        );
        // Carrion Crawler Brain Juice
        _poisons[13] = new PoisonSpec(13, 313,13,
            new[]
            {
                new PoisonEffect(PoisonEffectType.Paralyze),
            }
        );
        // Black Lotus Extract
        _poisons[14] = new PoisonSpec(14, 314,20,
            new[]
            {
                new PoisonEffect(PoisonEffectType.ConTemporary, new Dice(3, 6)),
            }
        );
        // Id Moss
        _poisons[15] = new PoisonSpec(15, 315,14,
            new[]
            {
                new PoisonEffect(PoisonEffectType.IntTemporary, new Dice(2, 6)),
            }
        );
        // Striped Toadstool
        _poisons[16] = new PoisonSpec(16, 316,11,
            new[]
            {
                new PoisonEffect(PoisonEffectType.WisTemporary, new Dice(2, 6)),
                new PoisonEffect(PoisonEffectType.IntTemporary, Dice.D4),
            }
        );
        // Lich Dust
        _poisons[17] = new PoisonSpec(17, 317,17,
            new[]
            {
                new PoisonEffect(PoisonEffectType.StrTemporary, Dice.D6),
            }
        );
        // Dark Reaver Powder
        _poisons[18] = new PoisonSpec(18, 318,18,
            new[]
            {
                new PoisonEffect(PoisonEffectType.ConTemporary, Dice.D6),
                new PoisonEffect(PoisonEffectType.StrTemporary, Dice.D6),
            }
        );
        // Burnt Othur Fumes
        _poisons[19] = new PoisonSpec(19, 319,18,
            new[]
            {
                new PoisonEffect(PoisonEffectType.ConTemporary, new Dice(3, 6)),
            }
        );
        // Quasit
        _poisons[20] = new PoisonSpec(20, 320,13,
            new[]
            {
                new PoisonEffect(PoisonEffectType.DexTemporary, new Dice(2, 4)),
            }
        );
        // Violet Fungi
        _poisons[21] = new PoisonSpec(21, 321,14,
            new[]
            {
                new PoisonEffect(PoisonEffectType.StrTemporary, Dice.D4),
                new PoisonEffect(PoisonEffectType.ConTemporary, Dice.D4),
            }
        );
        // Yellow Mold
        _poisons[22] = new PoisonSpec(22, 322,15,
            new[]
            {
                new PoisonEffect(PoisonEffectType.ConTemporary, new Dice(2, 6)),
            }
        );
        // Mystical
        _poisons[23] = new PoisonSpec(23, 323,0,
            new[]
            {
                new PoisonEffect(PoisonEffectType.ConTemporary, Dice.D10),
            }
        );
        // Other
        _poisons[24] = new PoisonSpec(24, 324,0);
        // Blue Whinnis
        _poisons[25] = new PoisonSpec(25, 325,14,
            new[]
            {
                new PoisonEffect(PoisonEffectType.Unconsciousness),
            }
        );
        // Shadow Essence
        _poisons[26] = new PoisonSpec(26, 326,17,
            new[]
            {
                new PoisonEffect(PoisonEffectType.StrTemporary, new Dice(2, 6)),
            }
        );
        // Deathblade
        _poisons[27] = new PoisonSpec(27, 327,20,
            new[]
            {
                new PoisonEffect(PoisonEffectType.ConTemporary, new Dice(2, 6)),
            }
        );
        // Nitharit
        _poisons[28] = new PoisonSpec(28, 328,13,
            new[]
            {
                new PoisonEffect(PoisonEffectType.ConTemporary, new Dice(3, 6)),
            }
        );
        // Oil of Taggit
        _poisons[29] = new PoisonSpec(29, 329,15,
            new[]
            {
                new PoisonEffect(PoisonEffectType.Unconsciousness),
            }
        );
        // Arsenic
        _poisons[30] = new PoisonSpec(30, 330,13,
            new[]
            {
                new PoisonEffect(PoisonEffectType.ConTemporary, Dice.D8),
            }
        );
        // Ungol Dust
        _poisons[31] = new PoisonSpec(31, 331,15,
            new[]
            {
                new PoisonEffect(PoisonEffectType.ChaTemporary, Dice.D6),
                new PoisonEffect(PoisonEffectType.ChaPermanent, Dice.Constant(1)),
            }
        );
        // Insanity Mist
        _poisons[32] = new PoisonSpec(32, 332,15,
            new[]
            {
                new PoisonEffect(PoisonEffectType.WisTemporary, new Dice(2, 6)),
            }
        );
    }
}