using System;
using System.Collections.Generic;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems.D20
{
    public class D20DamageSystem
    {
        private readonly Dictionary<int, string> _translations;

        [TempleDllLocation(0x100e0360)]
        public D20DamageSystem()
        {
            _translations = Tig.FS.ReadMesFile("mes/damage.mes");
        }

        public string GetTranslation(int id)
        {
            return _translations[id];
        }

        [TempleDllLocation(0x100E1210)]
        public int GetOverallDamage(in DamagePacket damagePacket, DamageType damageType)
        {
            var damTot = 0.0f;
            var anyDiceMatched = false;

            foreach (var damageDice in damagePacket.dice)
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
                damTot += damagePacket.bonuses.OverallBonus;
            }

            // Recalculate the actual damage factor modifiers
            for (var index = 0; index < damagePacket.damageFactorModifiers.Count; index++)
            {
                var damageModifier = damagePacket.damageFactorModifiers[index];
                CalcDamageModFromFactor(damagePacket, ref damageModifier, DamageType.Unspecified,
                    DamageType.Unspecified);
                damagePacket.damageFactorModifiers[index] = damageModifier;
            }

            // Recalculate the actual damage resistance modifiers
            for (var index = 0; index < damagePacket.damageResistances.Count; index++)
            {
                var damageResistance = damagePacket.damageResistances[index];
                CalcDamageModFromDR(damagePacket, ref damageResistance, DamageType.Unspecified,
                    DamageType.Unspecified);
                damagePacket.damageResistances[index] = damageResistance;
            }

            // Sum up the applicable reductions
            foreach (var damageResistance in damagePacket.damageResistances)
            {
                if (DamageTypeMatch(damageType, damageResistance.type))
                {
                    damTot += damageResistance.damageReduced;
                }
            }

            foreach (var damageFactor in damagePacket.damageFactorModifiers)
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

        [TempleDllLocation(0x100e0e00)]
        private void CalcDamageModFromFactor(in DamagePacket damPkt, ref DamageReduction damReduc,
            DamageType attackDamType, DamageType attackDamType2)
        {
            var applicableDamage = GetDamageThatReductionAppliesTo(damPkt, damReduc, attackDamType, attackDamType2);

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
        private void CalcDamageModFromDR(in DamagePacket damPkt, ref DamageReduction damReduc,
            DamageType attackDamType, DamageType attackDamType2)
        {
            var applicableDamage = GetDamageThatReductionAppliesTo(damPkt, damReduc, attackDamType, attackDamType2);

            if ( damReduc.damageReductionAmount >= applicableDamage )
            {
                damReduc.damageReduced = applicableDamage;
            }
            else
            {
                damReduc.damageReduced = damReduc.damageReductionAmount;
            }

            damReduc.damageReduced = -(int) ((1.0f - damReduc.dmgFactor) * damReduc.damageReduced);
        }

        private static int GetDamageThatReductionAppliesTo(DamagePacket damPkt, DamageReduction damReduc, DamageType attackDamType,
            DamageType attackDamType2)
        {
            // Sum up the damage again, but only for the matching damage types
            var applicableDamage = 0;
            bool anyDiceApplied = false;
            foreach (var damageDice in damPkt.dice)
            {
                var attackPowerType = damPkt.attackPowerType;
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
                applicableDamage += damPkt.bonuses.OverallBonus;
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
    }
}