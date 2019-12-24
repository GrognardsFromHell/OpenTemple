using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Classes;

namespace OpenTemple.Core.Systems.D20.Conditions
{
    [AutoRegister]
    public static class TemplePlusClassConditions
    {
        public static ConditionSpec.Builder Create(D20ClassSpec classSpec)
        {
            return ConditionSpec.Create(classSpec.conditionName)
                .SetUnique()
                .AddHandler(DispatcherType.ToHitBonusBase, AddClassBaseAttackBonus, classSpec.classEnum)
                .AddHandler(DispatcherType.SaveThrowLevel, AddClassSavingThrowBonus, classSpec.classEnum)
                .AddHandler(DispatcherType.GetBaseCasterLevel, AddBaseCasterLevel, classSpec.classEnum);
        }

        [TempleDllLocation(0x100fe020)]
        [TempleDllLocation(0x100fdfd0)]
        [TempleDllLocation(0x100fdf90)]
        private static void AddClassBaseAttackBonus(in DispatcherCallbackArgs evt, Stat classStat)
        {
            var classLevels = evt.objHndCaller.GetStat(classStat);
            var babValue = D20ClassSystem.GetBaseAttackBonus(classStat, classLevels);
            var dispIo = evt.GetDispIoAttackBonus();
            // TODO: We might want to add the name of the class to the description
            dispIo.bonlist.AddBonus(babValue, 0, 137); // untyped, description: "Class"
        }

        [TempleDllLocation(0x100fe070)]
        [TempleDllLocation(0x100fe0c0)]
        private static void AddClassSavingThrowBonus(in DispatcherCallbackArgs evt, Stat classStat)
        {
            var savingThrowType = evt.GetSavingThrowTypeFromDispatcherKey();

            var classSpec = D20ClassSystem.Classes[classStat];
            var saveProgression = classSpec.GetSavingThrowProgression(savingThrowType);
            var classLvl = evt.objHndCaller.GetStat(classStat);
            int bonus;
            if (saveProgression == SavingThrowProgressionType.HIGH)
            {
                bonus = 2 + classLvl / 2;
            }
            else
            {
                bonus = classLvl / 3;
            }

            var dispIo = evt.GetDispIoSavingThrow();
            // TODO: We might want to add the name of the class to the description
            dispIo.bonlist.AddBonus(bonus, 0, 137);
        }

        private static void AddBaseCasterLevel(in DispatcherCallbackArgs evt, Stat classStat)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            if (dispIo.arg0 == classStat)
            {
                var classLvl = evt.objHndCaller.GetStat(classStat);
                dispIo.bonlist.AddBonus(classLvl, 0, 137);
            }
        }
    }
}