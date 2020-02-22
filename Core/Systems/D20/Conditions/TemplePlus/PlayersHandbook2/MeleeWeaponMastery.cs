using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class MeleeWeaponMastery
    {
        private static readonly int bon_type_mwm = 856;
        private static readonly int bon_val = 2;

        public static readonly FeatId BaseFeatId = (FeatId) ElfHash.Hash("Melee Weapon Mastery");

        public static void MWMToHit(in DispatcherCallbackArgs evt, DamageType featDamType)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            DamageType weapDamType;
            var wpn = dispIo.attackPacket.GetWeaponUsed();
            if (wpn == null)
            {
                if (!(evt.objHndCaller.HasFeat(FeatId.IMPROVED_UNARMED_STRIKE)))
                {
                    return;
                }

                weapDamType = DamageType.Bludgeoning;
            }
            else
            {
                weapDamType = (DamageType) wpn.GetInt32(obj_f.weapon_attacktype);
            }

            if ((dispIo.attackPacket.flags & D20CAF.RANGED) != D20CAF.NONE)
            {
                return;
            }

            if (weapDamType == featDamType || weapDamType == DamageType.SlashingAndBludgeoningAndPiercing)
            {
                dispIo.bonlist.AddBonusFromFeat(bon_val, bon_type_mwm, 114, BaseFeatId);
                return;
            }
            else if (featDamType == DamageType.Bludgeoning &&
                     (weapDamType == DamageType.BludgeoningAndPiercing ||
                      weapDamType == DamageType.SlashingAndBludgeoning))
            {
                dispIo.bonlist.AddBonusFromFeat(bon_val, bon_type_mwm, 114, BaseFeatId);
                return;
            }
            else if (featDamType == DamageType.Slashing &&
                     (weapDamType == DamageType.SlashingAndBludgeoning ||
                      weapDamType == DamageType.PiercingAndSlashing))
            {
                dispIo.bonlist.AddBonusFromFeat(bon_val, bon_type_mwm, 114, BaseFeatId);
                return;
            }
            else if (featDamType == DamageType.Piercing &&
                     (weapDamType == DamageType.BludgeoningAndPiercing ||
                      weapDamType == DamageType.PiercingAndSlashing))
            {
                dispIo.bonlist.AddBonusFromFeat(bon_val, bon_type_mwm, 114, BaseFeatId);
                return;
            }

            return;
        }

        public static void MWMToDam(in DispatcherCallbackArgs evt, DamageType featDamType)
        {
            var dispIo = evt.GetDispIoDamage();
            var wpn = dispIo.attackPacket.GetWeaponUsed();
            DamageType weapDamType;
            if (wpn == null)
            {
                if (!(evt.objHndCaller.HasFeat(FeatId.IMPROVED_UNARMED_STRIKE)))
                {
                    return;
                }

                weapDamType = DamageType.Bludgeoning;
            }
            else
            {
                weapDamType = (DamageType) wpn.GetInt32(obj_f.weapon_attacktype);
            }

            if ((dispIo.attackPacket.flags & D20CAF.RANGED) != D20CAF.NONE)
            {
                return;
            }

            if (weapDamType == featDamType || weapDamType == DamageType.SlashingAndBludgeoningAndPiercing)
            {
                dispIo.damage.bonuses.AddBonusFromFeat(bon_val, bon_type_mwm, 114, BaseFeatId);
                return;
            }
            else if (featDamType == DamageType.Bludgeoning &&
                     (weapDamType == DamageType.BludgeoningAndPiercing ||
                      weapDamType == DamageType.SlashingAndBludgeoning))
            {
                dispIo.damage.bonuses.AddBonusFromFeat(bon_val, bon_type_mwm, 114, BaseFeatId);
                return;
            }
            else if (featDamType == DamageType.Slashing &&
                     (weapDamType == DamageType.SlashingAndBludgeoning ||
                      weapDamType == DamageType.PiercingAndSlashing))
            {
                dispIo.damage.bonuses.AddBonusFromFeat(bon_val, bon_type_mwm, 114, BaseFeatId);
                return;
            }
            else if (featDamType == DamageType.Piercing &&
                     (weapDamType == DamageType.BludgeoningAndPiercing ||
                      weapDamType == DamageType.PiercingAndSlashing))
            {
                dispIo.damage.bonuses.AddBonusFromFeat(bon_val, bon_type_mwm, 114, BaseFeatId);
                return;
            }

            return;
        }

        [FeatCondition("Melee Weapon Mastery - Bludgeoning")]
        [AutoRegister] public static readonly ConditionSpec mwmBludg = ConditionSpec.Create("Melee Weapon Mastery - Bludgeoning", 3)
            .SetUnique()
            .AddHandler(DispatcherType.ToHitBonus2, MWMToHit, DamageType.Bludgeoning)
            .AddHandler(DispatcherType.DealingDamage, MWMToDam, DamageType.Bludgeoning)
            .Build();

        [FeatCondition("Melee Weapon Mastery - Slashing")]
        [AutoRegister] public static readonly ConditionSpec mwmSlash = ConditionSpec.Create("Melee Weapon Mastery - Slashing", 3)
            .SetUnique()
            .AddHandler(DispatcherType.ToHitBonus2, MWMToHit, DamageType.Slashing)
            .AddHandler(DispatcherType.DealingDamage, MWMToDam, DamageType.Slashing)
            .Build();

        [FeatCondition("Melee Weapon Mastery - Piercing")]
        [AutoRegister] public static readonly ConditionSpec mwmPierc = ConditionSpec.Create("Melee Weapon Mastery - Piercing", 3)
            .SetUnique()
            .AddHandler(DispatcherType.ToHitBonus2, MWMToHit, DamageType.Piercing)
            .AddHandler(DispatcherType.DealingDamage, MWMToDam, DamageType.Piercing)
            .Build();
    }
}