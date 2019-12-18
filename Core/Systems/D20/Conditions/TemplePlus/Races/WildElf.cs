
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
    public class WildElf
    {

        private static readonly RaceId Id = RaceId.elf + (4 << 5);

        public static readonly RaceSpec RaceSpec = new RaceSpec
        {
            helpTopic = "TAG_WILD_ELF",
            conditionName = "Wild Elf",
            heightMale = (53, 65),
            heightFemale = (53, 65),
            weightMale = (87, 121),
            weightFemale = (82, 116),
            statModifiers = { (Stat.dexterity, 2), (Stat.intelligence, -2) },
            protoId = 13028,
            materialOffset = 2, // offset into rules/material_ext.mes file,
            feats = {FeatId.SIMPLE_WEAPON_PROFICIENCY_ELF},
            useBaseRaceForDeity = true
        };

        public static readonly ConditionSpec Condition = ConditionSpec.Create(RaceSpec.conditionName)
            .AddAbilityModifierHooks(RaceSpec)
            .AddSkillBonuses((SkillId.listen, 2), (SkillId.search, 2), (SkillId.spot, 2))
            .AddBaseMoveSpeed(30)
            .AddHandler(DispatcherType.SaveThrowLevel, ElvenSaveBonusEnchantment)
            .AddFavoredClassHook(Stat.level_sorcerer)
            .AddHandler(DispatcherType.ConditionAddPre, ConditionImmunityOnPreAdd)
            .Build();
        
        public static void ElvenSaveBonusEnchantment(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            var flags = dispIo.flags;
            if ((flags & D20SavingThrowFlag.SPELL_SCHOOL_ENCHANTMENT) != 0)
            {
                dispIo.bonlist.AddBonus(2, 31, 139); // Racial Bonus
            }
        }

        public static void ConditionImmunityOnPreAdd(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoCondStruct();
            if (dispIo.condStruct == SpellEffects.SpellSleep)
            {
                dispIo.outputFlag = false;
                evt.objHndCaller.FloatMesFileLine("mes/combat.mes", 5059, TextFloaterColor.Red); // "Sleep Immunity"
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(31, evt.objHndCaller, null);
            }
        }
    }
}
