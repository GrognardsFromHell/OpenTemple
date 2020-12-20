using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    [AutoRegister]
    public class Troll
    {
        private static readonly RaceId Id = RaceId.troll;

        // standard callbacks - BAB and Save values

        public static readonly RaceSpec RaceSpec = new RaceSpec(Id, RaceBase.troll)
        {
            hitDice = new Dice(6, 8),
            effectiveLevel = 5,
            helpTopic = "TAG_TROLL",
            flags = RaceDefinitionFlags.RDF_Monstrous,
            conditionName = "Troll",
            heightMale = (100, 120),
            heightFemale = (100, 120),
            weightMale = (870, 1210),
            weightFemale = (800, 1200),
            naturalArmor = 5,
            statModifiers =
            {
                (Stat.strength, 12), (Stat.dexterity, 4), (Stat.constitution, 12),
                (Stat.intelligence, -4), (Stat.wisdom, -2), (Stat.charisma, -4)
            },
            ProtoId = 13016,
            materialOffset = 0, // offset into rules/material_ext.mes file,
            feats = {FeatId.SIMPLE_WEAPON_PROFICIENCY, FeatId.MARTIAL_WEAPON_PROFICIENCY}
        };

        public static readonly ConditionSpec Condition = ConditionSpec.Create(RaceSpec.conditionName)
            .AddAbilityModifierHooks(RaceSpec)
            .AddFavoredClassHook(Stat.level_fighter)
            .AddSaveThrowBonusHook(SavingThrowType.Fortitude, 5)
            .AddSaveThrowBonusHook(SavingThrowType.Will, 2)
            .AddSaveThrowBonusHook(SavingThrowType.Reflex, 2)
            .AddBaseMoveSpeed(30)
            .Build();

    }
}