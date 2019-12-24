using OpenTemple.Core.Systems.Script.Extensions;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public static class RaceConditionExtensions
    {
        //////////////////////////////////////
        // Ability scores
        //////////////////////////////////////
        private static void OnGetAbilityScore(in DispatcherCallbackArgs evt, Stat statType, int statMod)
        {
            var dispIo = evt.GetDispIoBonusList();
            if (evt.objHndCaller.D20Query(D20DispatcherKey.QUE_Polymorphed))
            {
                // Do not confer racial ability bonuses while being polymorphed
                if ((statType == Stat.strength) || (statType == Stat.constitution) || (statType == Stat.dexterity))
                {
                    return;
                }
            }

            var newValue = statMod + dispIo.bonlist.OverallBonus;
            // ensure minimum stat of 3
            if (newValue < 3)
            {
                statMod = 3 - newValue;
            }

            dispIo.bonlist.AddBonus(statMod, 0, 139);
        }

        public static ConditionSpec.Builder AddAbilityModifierHooks(this ConditionSpec.Builder raceSpecObj,
            RaceSpec raceSpec)
        {
            foreach (var (stat, modifier) in raceSpec.statModifiers)
            {
                raceSpecObj.AddHandler(
                    DispatcherType.AbilityScoreLevel,
                    D20DispatcherKey.STAT_STRENGTH + (stat - Stat.strength),
                    OnGetAbilityScore,
                    stat,
                    modifier
                );
                raceSpecObj.AddHandler(
                    DispatcherType.StatBaseGet,
                    D20DispatcherKey.STAT_STRENGTH + (stat - Stat.strength),
                    OnGetAbilityScore,
                    stat,
                    modifier
                );
            }

            return raceSpecObj;
        }

        ////////////////////////////////////////
        // Saving Throws
        ////////////////////////////////////////
        public static void OnGetSaveThrow(in DispatcherCallbackArgs evt, int value)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            dispIo.bonlist.AddBonus(value, 0, 139);
        }

        public static ConditionSpec.Builder AddSaveThrowBonusHook(this ConditionSpec.Builder raceSpecObj,
            SavingThrowType saveThrowType, int value)
        {
            var k = D20DispatcherKey.SAVE_FORTITUDE + (saveThrowType - SavingThrowType.Fortitude);
            return raceSpecObj.AddHandler(DispatcherType.SaveThrowLevel, k, OnGetSaveThrow, value);
        }

        public static void OnGetSaveThrow(in DispatcherCallbackArgs evt, D20SavingThrowFlag saveThrowDescriptor,
            int value)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            if ((dispIo.flags & saveThrowDescriptor) != default)
            {
                dispIo.bonlist.AddBonus(value, 0, 139);
            }
        }

        public static ConditionSpec.Builder AddSaveBonusVsEffectType(this ConditionSpec.Builder raceSpecObj,
            D20SavingThrowFlag saveThrowDescriptor, int value)
        {
            return raceSpecObj.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.NONE, OnGetSaveThrow,
                saveThrowDescriptor, value);
        }
        ////////////////////////////////////////


        ////////////////////////////////////////#
        // Immunities and Resistances          //
        ////////////////////////////////////////#

        public static void ConditionImmunityOnPreAdd(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoCondStruct();
            var val = dispIo.condStruct.condName == StatusEffects.Poisoned.condName;
            if (val)
            {
                dispIo.outputFlag = false;
                evt.objHndCaller.FloatLine("Poison Immunity", TextFloaterColor.Red);
            }
        }

        public static ConditionSpec.Builder AddPoisonImmunity(this ConditionSpec.Builder raceSpecObj)
        {
            raceSpecObj.SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Poison, true);
            raceSpecObj.AddHandler(DispatcherType.ConditionAddPre, D20DispatcherKey.NONE, ConditionImmunityOnPreAdd);
            return raceSpecObj;
        }

        public static void OnGetDamageResistance(in DispatcherCallbackArgs evt, DamageType damType, int value)
        {
            var dispIo = evt.GetDispIoDamage();
            var DAMAGE_MES_RESISTANCE_TO_ENERGY = 124;
            dispIo.damage.AddDR(value, damType, DAMAGE_MES_RESISTANCE_TO_ENERGY);
        }

        public static ConditionSpec.Builder AddDamageResistances(
            this ConditionSpec.Builder raceSpecObj,
            params (DamageType, int)[] resistances)
        {
            foreach (var (damageType, amount) in resistances)
            {
                raceSpecObj.AddHandler(DispatcherType.TakingDamage, D20DispatcherKey.NONE, OnGetDamageResistance,
                    damageType, amount);
            }

            return raceSpecObj;
        }

        ////////////////////////////////////////#
        // Skill Bonuses
        ////////////////////////////////////////#
        public static void OnGetSkillLevel(in DispatcherCallbackArgs evt, int value)
        {
            var dispIo = evt.GetDispIoObjBonus();
            dispIo.bonlist.AddBonus(value, 0, 139);
        }

        public static ConditionSpec.Builder AddSkillBonuses(this ConditionSpec.Builder raceSpecObj,
            params (SkillId, int)[] bonuses)
        {
            foreach (var (skillId, bonus) in bonuses)
            {
                raceSpecObj.AddHandler(
                    DispatcherType.SkillLevel,
                    D20DispatcherKey.SKILL_APPRAISE + (skillId - SkillId.appraise),
                    OnGetSkillLevel,
                    bonus
                );
            }

            return raceSpecObj;
        }
        ////////////////////////////////////////#

        ////////////////////////////////////////#
        // favored class
        ////////////////////////////////////////#
        public static void OnGetFavoredClass(in DispatcherCallbackArgs evt, Stat favoredClass)
        {
            var dispIo = evt.GetDispIoD20Query();
            if ((Stat) dispIo.data1 == favoredClass)
            {
                dispIo.return_val = 1;
            }
        }

        public static ConditionSpec.Builder AddFavoredClassHook(this ConditionSpec.Builder raceSpecObj, Stat classEnum)
        {
            return raceSpecObj.AddQueryHandler(D20DispatcherKey.QUE_FavoredClass, OnGetFavoredClass, classEnum);
        }
        ////////////////////////////////////////#


        ////////////////////////////////////////#
        // Base move speed
        ////////////////////////////////////////#
        public static void OnGetBaseMoveSpeed(in DispatcherCallbackArgs evt, int val)
        {
            var dispIo = evt.GetDispIoMoveSpeed();
            dispIo.bonlist.AddBonus(val, 1, 139);
        }

        public static ConditionSpec.Builder AddBaseMoveSpeed(this ConditionSpec.Builder raceSpecObj, int value)
        {
            return raceSpecObj.AddHandler(DispatcherType.GetMoveSpeedBase, OnGetBaseMoveSpeed, value);
        }
    }
}