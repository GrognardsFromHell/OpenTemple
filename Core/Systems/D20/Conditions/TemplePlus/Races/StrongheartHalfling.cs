using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Startup.Discovery;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
{
    [AutoRegister]
    public class StrongheartHalfling
    {
        private const RaceId Id = RaceId.halfling + (3 << 5);

        public static readonly RaceSpec RaceSpec = new RaceSpec
        {
            effectiveLevel = 0,
            helpTopic = "TAG_STRONGHEART_HALFLING",
            flags = RaceDefinitionFlags.RDF_ForgottenRealms,
            conditionName = "Strongheart",
            heightMale = (32, 40),
            heightFemale = (30, 38),
            weightMale = (32, 34),
            weightFemale = (27, 29),
            statModifiers = {(Stat.strength, -2), (Stat.dexterity, 2)},
            protoId = 13038,
            materialOffset = 12, // offset into rules/material_ext.mes file
            bonusFirstLevelFeat = true,
            useBaseRaceForDeity = true
        };

        public static readonly ConditionSpec Condition = ConditionSpec.Create(RaceSpec.conditionName)
            .AddAbilityModifierHooks(RaceSpec)
            .AddSkillBonuses(
                (SkillId.listen, 2),
                (SkillId.move_silently, 2),
                (SkillId.climb, 2),
                (SkillId.jump, 2),
                (SkillId.hide, 4)
            )
            .AddBaseMoveSpeed(20)
            .AddFavoredClassHook(Stat.level_rogue)
            .AddHandler(DispatcherType.SaveThrowLevel, HalflingFearSaveBonus)
            .AddHandler(DispatcherType.ToHitBonus2, OnGetToHitBonusSlingsThrownWeapons)
            .Build();

        public static void HalflingFearSaveBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            var flags = dispIo.flags;
            if ((flags & D20SavingThrowFlag.SPELL_DESCRIPTOR_FEAR) != 0)
            {
                dispIo.bonlist.AddBonus(2, 13, 139);
            }
        }
        // +1 with thrown weapons and slings

        public static void OnGetToHitBonusSlingsThrownWeapons(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var thrownWeapon = (dispIo.attackPacket.flags & D20CAF.THROWN) != 0;
            var isSling = false;
            var wpn = dispIo.attackPacket.GetWeaponUsed();
            if (wpn != null)
            {
                var weaponType = wpn.GetWeaponType();
                if (weaponType == WeaponType.sling)
                {
                    isSling = true;
                }
            }

            // Check for sling or thrown weapon
            if (thrownWeapon || isSling)
            {
                dispIo.bonlist.AddBonus(1, 0, 139);
            }

        }

        // Note:  Adding the size +4 bonus to hide as a racial bonus since setting size to small does not grant the bonus
    }
}