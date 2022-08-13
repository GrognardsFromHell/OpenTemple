using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.Script.Extensions;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus.Races;

[AutoRegister]
public class AquaticElf
{
    public const RaceId Id = RaceId.aquatic_elf + (1 << 5);

    public static readonly RaceSpec RaceSpec = new(Id, RaceBase.elf, Subrace.aquatic_elf)
    {
        effectiveLevel = 0,
        helpTopic = "TAG_AQUATIC_ELF",
        flags = 0,
        conditionName = "Aquatic Elf",
        heightMale = (53, 65),
        heightFemale = (53, 65),
        weightMale = (87, 121),
        weightFemale = (82, 116),
        statModifiers = {(Stat.dexterity, 2), (Stat.intelligence, -2)},
        ProtoId = 13024,
        materialOffset = 2, // offset into rules/material_ext.mes file,
        feats = {FeatId.SIMPLE_WEAPON_PROFICIENCY_ELF},
        useBaseRaceForDeity = true,
    };

    public static readonly ConditionSpec Condition = ConditionSpec.Create(RaceSpec.conditionName, 0, UniquenessType.NotUnique)
        .Configure(builder => builder
            .AddAbilityModifierHooks(RaceSpec)
            .AddSkillBonuses((SkillId.listen, 2), (SkillId.search, 2), (SkillId.spot, 2))
            .AddBaseMoveSpeed(30)
            .AddHandler(DispatcherType.SaveThrowLevel, ElvenSaveBonusEnchantment)
            .AddQueryHandler(D20DispatcherKey.QUE_FavoredClass, OnGetFavoredClass)
            .AddHandler(DispatcherType.ConditionAddPre, ConditionImmunityOnPreAdd)
        );

    public static void ElvenSaveBonusEnchantment(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoSavingThrow();
        var flags = dispIo.flags;
        if ((flags & D20SavingThrowFlag.SPELL_SCHOOL_ENCHANTMENT) != 0)
        {
            dispIo.bonlist.AddBonus(2, 31, 139); // Racial Bonus
        }
    }

    public static void OnGetFavoredClass(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20Query();
        if (dispIo.data1 == (int) Stat.level_fighter)
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