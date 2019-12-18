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
    public class GoldDwarf
    {
        public const RaceId Id = RaceId.dwarf + (5 << 5);

        public static readonly RaceSpec RaceSpec = new RaceSpec
        {
            effectiveLevel = 0,
            helpTopic = "TAG_GOLD_DWARF",
            flags = RaceDefinitionFlags.RDF_ForgottenRealms,
            conditionName = "Gold Dwarf",
            heightMale = (45, 53),
            heightFemale = (43, 51),
            weightMale = (148, 178),
            weightFemale = (104, 134),
            statModifiers = {(Stat.dexterity, -2), (Stat.constitution, 2)},
            protoId = 13036,
            materialOffset = 6, // offset into rules/material_ext.mes file,
            useBaseRaceForDeity = true
        };

        public static readonly ConditionSpec Condition = ConditionSpec.Create(RaceSpec.conditionName)
            .AddAbilityModifierHooks(RaceSpec)
            // note: dwarven move speed with heavy armor or when medium/heavy encumbered is already handled in Encumbered Medium, Encumbered Heavy condition callbacks
            .AddBaseMoveSpeed(20)
            .AddFavoredClassHook(Stat.level_fighter)
            .AddHandler(DispatcherType.GetMoveSpeed, OnGetMoveSpeedSetLowerLimit)
            .AddHandler(DispatcherType.ToHitBonus2, OnGetToHitBonusVsAberration)
            .AddHandler(DispatcherType.GetAC, OnGetArmorClassBonusVsGiants)
            .AddHandler(DispatcherType.AbilityCheckModifier, OnAbilityModCheckStabilityBonus)
            .AddHandler(DispatcherType.SkillLevel, D20DispatcherKey.SKILL_APPRAISE, OnGetAppraiseSkill)
            .AddHandler(DispatcherType.SaveThrowLevel, DwarfSaveBonus)
            .Build();

        private static readonly int BONUS_MES_RACIAL_BONUS = 139;
        private static readonly int BONUS_MES_STABILITY = 317;

        public static void DwarfSaveBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            var flags = dispIo.flags;
            if ((flags & D20SavingThrowFlag.SPELL_LIKE_EFFECT) != 0)
            {
                dispIo.bonlist.AddBonus(2, 31, BONUS_MES_RACIAL_BONUS); // Racial Bonus
            }
            else if ((flags & D20SavingThrowFlag.POISON) != 0)
            {
                dispIo.bonlist.AddBonus(2, 31, BONUS_MES_RACIAL_BONUS); // Racial Bonus
            }
        }

        public static void OnGetAppraiseSkill(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoObjBonus();
            // adds appraise bonus to metal or rock items
            var item = dispIo.obj;
            if (item == null)
            {
                return;
            }

            var material = item.GetMaterial();
            if ((material == Material.stone || material == Material.metal))
            {
                dispIo.bonlist.AddBonus(2, 0, BONUS_MES_RACIAL_BONUS);
            }
        }

        public static void OnGetMoveSpeedSetLowerLimit(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoMoveSpeed();
            // this sets the lower limit for dwarf move speed at 20, unless someone has already set it (e.g. by web/entangle)
            if ((dispIo.bonlist.bonFlags & 2) != 0)
            {
                return;
            }

            var moveSpeedCapValue = 20;
            var capFlags = 2; // set lower limit
            var capType = 0; // operate on all bonus types
            var bonusMesline = BONUS_MES_RACIAL_BONUS; // racial ability
            dispIo.bonlist.SetOverallCap(capFlags, moveSpeedCapValue, capType, bonusMesline);
        }

        public static void OnGetToHitBonusVsAberration(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var target = dispIo.attackPacket.victim;
            if (target == null)
            {
                return;
            }

            if (target.IsMonsterCategory(MonsterCategory.aberration))
            {
                dispIo.bonlist.AddBonus(1, 0, BONUS_MES_RACIAL_BONUS);
            }
        }

        public static void OnGetArmorClassBonusVsGiants(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var attacker = dispIo.attackPacket.attacker /*AttackPacket*/;
            if (attacker == null)
            {
                return;
            }

            if (attacker.IsMonsterCategory(MonsterCategory.giant))
            {
                dispIo.bonlist.AddBonus(4, 8, BONUS_MES_RACIAL_BONUS);
            }
        }

        public static void OnAbilityModCheckStabilityBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoObjBonus();
            var flags = dispIo.flags;
            if ((flags & SkillCheckFlags.UnderDuress) != 0 && (flags & SkillCheckFlags.Unk2) != 0) // defender bonus
            {
                dispIo.bonlist.AddBonus(4, 22, BONUS_MES_STABILITY);
            }
        }
    }
}