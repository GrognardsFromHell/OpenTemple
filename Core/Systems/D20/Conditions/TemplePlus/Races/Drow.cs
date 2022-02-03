using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Systems.Spells;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus.Races
{
    [AutoRegister]
    public class Drow
    {
        public const RaceId Id = RaceId.aquatic_elf + (2 << 5);

        public static readonly RaceSpec RaceSpec = new RaceSpec(Id, RaceBase.elf, Subrace.drow)
        {
            effectiveLevel = 2,
            helpTopic = "TAG_DROW",
            flags = 0,
            conditionName = "Drow",
            heightMale = (53, 65),
            heightFemale = (53, 65),
            weightMale = (87, 121),
            weightFemale = (82, 116),
            statModifiers = {(Stat.dexterity, 2), (Stat.constitution, -2), (Stat.intelligence, 2), (Stat.charisma, -2)},
            ProtoId = 13014,
            materialOffset = 14, // offset into rules/material_ext.mes file,
            feats =
            {
                FeatId.EXOTIC_WEAPON_PROFICIENCY_HAND_CROSSBOW, FeatId.MARTIAL_WEAPON_PROFICIENCY_RAPIER,
                FeatId.MARTIAL_WEAPON_PROFICIENCY_SHORT_SWORD
            },
            useBaseRaceForDeity = false,
            spellLikeAbilities =
            {
                {new SpellStoreData(WellKnownSpells.FaerieFire, 1, SpellSystem.GetSpellClass(DomainId.Special)), 1}
            }
        };

        public static readonly ConditionSpec Condition = ConditionSpec.Create(RaceSpec.conditionName)
            .AddAbilityModifierHooks(RaceSpec)
            .AddSaveThrowBonusHook(SavingThrowType.Will, 2)
            .AddSkillBonuses(
                (SkillId.listen, 2),
                (SkillId.search, 2),
                (SkillId.spot, 2)
            )
            .AddBaseMoveSpeed(30)
            .AddHandler(DispatcherType.SpellResistanceMod, OnGetSpellResistance)
            .AddHandler(DispatcherType.Tooltip, OnGetSpellResistanceTooltip)
            .AddHandler(DispatcherType.SaveThrowLevel, ElvenSaveBonusEnchantment)
            .AddHandler(DispatcherType.D20Query, D20DispatcherKey.QUE_FavoredClass, OnGetFavoredClass)
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

        public static void OnGetSpellResistance(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIOBonusListAndSpellEntry();
            var classLvl = evt.objHndCaller.GetStat(Stat.level);
            dispIo.bonList.AddBonus(11 + classLvl, 36, "Racial Bonus (Drow)");
        }

        public static void OnGetSpellResistanceTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoTooltip();
            var classLvl = evt.objHndCaller.GetStat(Stat.level);
            dispIo.Append("Spell Resistance [" + (11 + classLvl) + "]");
        }

        public static void OnGetFavoredClass(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            var gender = evt.objHndCaller.GetGender();
            if (dispIo.data1 == (int) Stat.level_cleric && gender == Gender.Female ||
                dispIo.data1 == (int) Stat.level_wizard && gender == Gender.Male)
            {
                dispIo.return_val = 1;
            }
        }

        public static void ConditionImmunityOnPreAdd(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoCondStruct();
            var val = dispIo.condStruct == SpellEffects.SpellSleep;
            if (val)
            {
                dispIo.outputFlag = false;
                evt.objHndCaller.FloatMesFileLine("mes/combat.mes", 5059, TextFloaterColor.Red); // "Sleep Immunity"
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(31, evt.objHndCaller, null);
            }
        }
    }
}