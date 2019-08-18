using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.D20
{
    public struct DamageDice
    {
        // see 100E03F0 AddDamageDice
        public Dice dice;
        public DamageType type;
        public int rolledDamage;
        public string typeDescription;
        public string causedBy; // e.g. item name

        public DamageDice Default => new DamageDice
        {
            type = DamageType.Unspecified,
            rolledDamage = -1
        };

        public DamageDice(Dice dice, DamageType DamType, string TypeDescr) : this()
        {
            this.dice = dice;
            type = DamType;
            typeDescription = TypeDescr;
            rolledDamage = -1;
        }
    }

    public struct DamageReduction
    {
        public int damageReductionAmount;
        public float dmgFactor;
        public DamageType type;

        public D20AttackPower
            attackPowerType; // see D20AttackPower; if an attack has an overlapping attackPowerType (result of an  operation), the damage reduction will NOT apply; DamageReductions with attackPowerType = 1 will ALWAYS apply

        public string typeDescription;
        public string causedBy; // e.g. an item name
        public int damageReduced; // e.g. from CalcDamageModFromFactor 0x100E0E00
    }

    public class DamagePacket
    {
        public string description;

        public int
            critHitMultiplier; // 1 by default; gets set on an actual crit hit (and may be modified by various things)

        public List<DamageDice> dice = new List<DamageDice>();
        public List<DamageReduction> damageResistances = new List<DamageReduction>();

        public List<DamageReduction>
            damageFactorModifiers =
                new List<DamageReduction>(); // may also be used for vulnerabilities (e.g. Condition Monster Subtype Fire does this for Cold Damage)

        public BonusList bonuses = BonusList.Create();
        public D20AttackPower attackPowerType; // see D20DAP
        public int finalDamage;
        public int flags; // 1 - Maximized (takes max value of damage dice) ; 2 - Empowered (1.5x on rolls)
        public int field51c;

        public bool Maximized
        {
            get => (flags & 1) != 0;
            [TempleDllLocation(0x100e0a50)]
            set => flags |= 1;
        }

        public void AddEtherealImmunity()
        {
            var typeText = GameSystems.D20.Damage.GetTranslation(134);

            damageFactorModifiers.Add(new DamageReduction
            {
                dmgFactor = 0,
                type = DamageType.Unspecified,
                attackPowerType = 0,
                typeDescription = typeText,
                causedBy = null
            });
        }

        [TempleDllLocation(0x100e0780)]
        public void AddIncorporealImmunity()
        {
            var typeText = GameSystems.D20.Damage.GetTranslation(131);

            damageFactorModifiers.Add(new DamageReduction
            {
                dmgFactor = 0,
                type = DamageType.SlashingAndBludgeoningAndPiercing,
                attackPowerType = D20AttackPower.SILVER,
                typeDescription = typeText,
                causedBy = null
            });
        }

        [TempleDllLocation(0x100e03f0)]
        [TempleDllLocation(0x100e04e0)]
        public void AddDamageDice(Dice dicePacked, DamageType damType, int damageMesLine, string description = null)
        {
            var line = GameSystems.D20.Damage.GetTranslation(damageMesLine);

            if (damType == DamageType.Unspecified)
            {
                if (dice.Count > 0)
                {
                    damType = dice[0].type;
                }
            }

            dice.Add(new DamageDice(dicePacked, damType, line)
            {
                causedBy = description
            });
        }

        [TempleDllLocation(0x100e05b0)]
        [TempleDllLocation(0x100e05e0)]
        public bool AddDamageBonus(int damBonus, int bonType, int bonMesline, string causeDesc = null)
        {
            return bonuses.AddBonus(damBonus, bonType, bonMesline, causeDesc);
        }

        [TempleDllLocation(0x100e0610)]
        public void AddPhysicalDR(int amount, D20AttackPower bypasserBitmask, int damageMesLine)
        {
            var translation = GameSystems.D20.Damage.GetTranslation(damageMesLine);

            damageResistances.Add(new DamageReduction
            {
                damageReductionAmount = amount,
                dmgFactor = 0,
                type = DamageType.SlashingAndBludgeoningAndPiercing,
                attackPowerType = bypasserBitmask,
                typeDescription = translation,
                causedBy = null
            });
        }

        [TempleDllLocation(0x100e08f0)]
        [TempleDllLocation(0x100e0830)]
        public void AddDR(int amount, DamageType damType, int damageMesLine, string causedBy = null)
        {
            var translation = GameSystems.D20.Damage.GetTranslation(damageMesLine);
            damageResistances.Add(new DamageReduction
            {
                damageReductionAmount = amount,
                dmgFactor = 0,
                type = damType,
                attackPowerType = D20AttackPower.NORMAL,
                typeDescription = translation,
                causedBy = null
            });
        }

        [TempleDllLocation(0x100e0520)]
        public void AddAttackPower(D20AttackPower attackPower)
        {
            attackPowerType |= attackPower;
        }

        // calcualtes the finalDamage field
        [TempleDllLocation(0x100e16f0)]
        public void CalcFinalDamage()
        {
            for (var index = 0; index < this.dice.Count; index++)
            {
                var damageDice = this.dice[index];
                if (damageDice.rolledDamage < 0)
                {
                    var dice = damageDice.dice;
                    if ((flags & 1) != 0) // maximiuzed
                    {
                        damageDice.rolledDamage = dice.MaximumValue;
                    }
                    else // normal
                    {
                        damageDice.rolledDamage = dice.Roll();
                    }

                    if ((flags & 2) != 0) //empowered
                    {
                        damageDice.rolledDamage = (int) (damageDice.rolledDamage * 1.5f);
                    }

                    this.dice[index] = damageDice;
                }
            }

            finalDamage = GetOverallDamageByType();
        }

        [TempleDllLocation(0x100e1360)]
        public int GetOverallLethalDamage()
        {
            var damTot = 0.0f;
            var anyDiceMatched = false;

            foreach (var damageDice in dice)
            {
                if (damageDice.type != DamageType.Subdual)
                {
                    damTot += damageDice.rolledDamage;
                    anyDiceMatched = true;
                }
            }

            // Only apply the overall bonus if any of the damage dice matched the requested damage type
            if (anyDiceMatched)
            {
                damTot += bonuses.OverallBonus;
            }

            RefreshDamageFactorsAndResistances();

            // Sum up the applicable reductions
            foreach (var damageResistance in damageResistances)
            {
                if (damageResistance.type != DamageType.Subdual)
                {
                    damTot += damageResistance.damageReduced;
                }
            }

            foreach (var damageFactor in damageFactorModifiers)
            {
                if (damageFactor.type != DamageType.Subdual)
                {
                    damTot += damageFactor.damageReduced;
                }
            }

            if (damTot < 0.0f)
            {
                damTot = 0.0f;
            }

            return (int) damTot;
        }

        [TempleDllLocation(0x100E1210)]
        [TempleDllLocation(0x100e1630)]
        public int GetOverallDamageByType(DamageType damageType = DamageType.Unspecified)
        {
            var damTot = 0.0f;
            var anyDiceMatched = false;

            foreach (var damageDice in dice)
            {
                if (damageDice.type == damageType || damageType == DamageType.Unspecified)
                {
                    damTot += damageDice.rolledDamage;
                    anyDiceMatched = true;
                }
            }

            // Only apply the overall bonus if any of the damage dice matched the requested damage type
            if (anyDiceMatched)
            {
                damTot += bonuses.OverallBonus;
            }

            RefreshDamageFactorsAndResistances();

            // Sum up the applicable reductions
            foreach (var damageResistance in damageResistances)
            {
                if (DamageTypeMatch(damageType, damageResistance.type))
                {
                    damTot += damageResistance.damageReduced;
                }
            }

            foreach (var damageFactor in damageFactorModifiers)
            {
                if (DamageTypeMatch(damageType, damageFactor.type))
                {
                    damTot += damageFactor.damageReduced;
                }
            }

            if (damTot < 0.0f)
            {
                damTot = 0.0f;
            }

            return (int) damTot;
        }

        private void RefreshDamageFactorsAndResistances()
        {
            // Recalculate the actual damage factor modifiers
            for (var index = 0; index < damageFactorModifiers.Count; index++)
            {
                var damageModifier = damageFactorModifiers[index];
                CalcDamageModFromFactor(ref damageModifier, DamageType.Unspecified,
                    DamageType.Unspecified);
                damageFactorModifiers[index] = damageModifier;
            }

            // Recalculate the actual damage resistance modifiers
            for (var index = 0; index < damageResistances.Count; index++)
            {
                var damageResistance = damageResistances[index];
                CalcDamageModFromDR(ref damageResistance, DamageType.Unspecified,
                    DamageType.Unspecified);
                damageResistances[index] = damageResistance;
            }
        }

        [TempleDllLocation(0x100e0e00)]
        private void CalcDamageModFromFactor(ref DamageReduction damReduc,
            DamageType attackDamType, DamageType attackDamType2)
        {
            var applicableDamage = GetDamageThatReductionAppliesTo(damReduc, attackDamType, attackDamType2);

            if (applicableDamage > 0)
            {
                damReduc.damageReduced = -(int) ((1.0f - damReduc.dmgFactor) * applicableDamage);
            }
            else
            {
                damReduc.damageReduced = 0;
            }
        }

        [TempleDllLocation(0x100E0C90)]
        private void CalcDamageModFromDR(ref DamageReduction damReduc,
            DamageType attackDamType, DamageType attackDamType2)
        {
            var applicableDamage = GetDamageThatReductionAppliesTo(damReduc, attackDamType, attackDamType2);

            if (damReduc.damageReductionAmount >= applicableDamage)
            {
                damReduc.damageReduced = applicableDamage;
            }
            else
            {
                damReduc.damageReduced = damReduc.damageReductionAmount;
            }

            damReduc.damageReduced = -(int) ((1.0f - damReduc.dmgFactor) * damReduc.damageReduced);
        }

        private int GetDamageThatReductionAppliesTo(DamageReduction damReduc, DamageType attackDamType,
            DamageType attackDamType2)
        {
            // Sum up the damage again, but only for the matching damage types
            var applicableDamage = 0;
            bool anyDiceApplied = false;
            foreach (var damageDice in dice)
            {
                var attackPowerType = this.attackPowerType;
                var diceDmgType = damageDice.type;
                // TODO: The attack power values below don't match the enum. so either the attack power enum is wrong, or the attack type one is....
                throw new NotImplementedException();
                switch (diceDmgType)
                {
                    case DamageType.Bludgeoning:
                        attackPowerType |= (D20AttackPower) 0x100;
                        break;
                    case DamageType.Piercing:
                        attackPowerType |= (D20AttackPower) 0x200;
                        break;
                    case DamageType.Slashing:
                        attackPowerType |= (D20AttackPower) 0x400;
                        break;
                    case DamageType.BludgeoningAndPiercing:
                        attackPowerType |= (D20AttackPower) 0x300;
                        break;
                    case DamageType.PiercingAndSlashing:
                        attackPowerType |= (D20AttackPower) 0x600;
                        break;
                    case DamageType.SlashingAndBludgeoning:
                        attackPowerType |= (D20AttackPower) 0x500;
                        break;
                    case DamageType.SlashingAndBludgeoningAndPiercing:
                        attackPowerType |= (D20AttackPower) 0x700;
                        break;
                }

                if (DamageTypeMatch(diceDmgType, attackDamType)
                    && (attackDamType2 == DamageType.Unspecified || !DamageTypeMatch(diceDmgType, attackDamType2)))
                {
                    if (DamageTypeMatch(diceDmgType, damReduc.type))
                    {
                        if (damReduc.attackPowerType == D20AttackPower.NORMAL
                            || (damReduc.attackPowerType & attackPowerType) == 0)
                        {
                            applicableDamage += damageDice.rolledDamage;
                            anyDiceApplied = true;
                        }
                    }
                }
            }

            if (anyDiceApplied)
            {
                applicableDamage += bonuses.OverallBonus;
            }

            return applicableDamage;
        }

        private static bool DamageTypeMatch(DamageType reduction, DamageType attackType)
        {
            if (attackType == DamageType.Subdual)
            {
                if (reduction != DamageType.Subdual)
                    return false;
            }
            else if (attackType == DamageType.Unspecified)
                return true;

            if (reduction == DamageType.Unspecified || attackType == reduction)
                return true;

            switch (attackType)
            {
                case DamageType.BludgeoningAndPiercing:
                    if (reduction == DamageType.Bludgeoning || reduction == DamageType.Piercing)
                        return true;
                    break;
                case DamageType.PiercingAndSlashing:
                    if (reduction == DamageType.Piercing || reduction == DamageType.Slashing)
                        return true;
                    break;
                case DamageType.SlashingAndBludgeoning:
                    if (reduction == DamageType.Slashing || reduction == DamageType.Bludgeoning)
                        return true;
                    break;
                case DamageType.SlashingAndBludgeoningAndPiercing:
                    if (reduction == DamageType.Bludgeoning || reduction == DamageType.Slashing
                                                            || reduction == DamageType.Piercing ||
                                                            reduction == DamageType.BludgeoningAndPiercing
                                                            || reduction == DamageType.PiercingAndSlashing ||
                                                            reduction == DamageType.SlashingAndBludgeoning
                                                            || reduction == DamageType.Subdual)
                        return true;
                    break;
                default:
                    return false;
            }

            return false;
        }

        [TempleDllLocation(0x100E1640)]
        public void AddCritMultiplier(int multiplier, int mesLine)
        {
            var translation = GameSystems.D20.Damage.GetTranslation(mesLine);

            // Modify the first damage dice
            var firstDamageDice = dice[0];
            var firstDice = firstDamageDice.dice;
            firstDamageDice.dice = new Dice(
                firstDice.Count * multiplier,
                firstDice.Sides,
                firstDice.Modifier * multiplier
            );
            dice[0] = firstDamageDice;

            for (var i = 0; i < bonuses.bonCount; i++)
            {
                bonuses.bonusEntries[i].bonValue *= multiplier;
            }

            description = translation;
            critHitMultiplier = multiplier;
            finalDamage = GetOverallDamageByType(DamageType.Unspecified);
        }

        [TempleDllLocation(0x100E06D0)]
        public void AddModFactor(float factor, DamageType damType, int damageMesLine)
        {
            var translation = GameSystems.D20.Damage.GetTranslation(damageMesLine);

            damageFactorModifiers.Add(new DamageReduction
            {
                dmgFactor = factor,
                type = damType,
                attackPowerType = D20AttackPower.NORMAL,
                typeDescription = translation
            });
        }

        public DamagePacket()
        {
            critHitMultiplier = 1;
        }

        [TempleDllLocation(0x100e0540)]
        public void SetDamageType(DamageType damageType)
        {
            var firstDice = dice[0];
            firstDice.type = damageType;
            dice[0] = firstDice;
        }
    }

    public class DispIoDamage
    {
        // Io type 4
        public AttackPacket attackPacket = new AttackPacket();
        public DamagePacket damage = new DamagePacket();

        [TempleDllLocation(0x1004da00)]
        public DispIoDamage()
        {
            attackPacket.d20ActnType = D20ActionType.NONE;
        }

        public static DispIoDamage CreateWithWeapon(GameObjectBody attacker, GameObjectBody victim)
        {
            var result = new DispIoDamage();
            result.attackPacket.d20ActnType = D20ActionType.NONE;
            result.attackPacket.SetAttacker(attacker);
            result.attackPacket.victim = victim;
            return result;
        }

        public static DispIoDamage Create(GameObjectBody attacker, GameObjectBody victim)
        {
            var result = new DispIoDamage();
            result.attackPacket.d20ActnType = D20ActionType.NONE;
            result.attackPacket.attacker = attacker;
            result.attackPacket.victim = victim;
            return result;
        }

        public void Debug()
        {
            throw new NotImplementedException();
        }
    }
}