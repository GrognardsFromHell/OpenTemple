using OpenTemple.Core.Startup.Discovery;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class TotalDefense
    {
        // The fighting defensively query needs to cover both fighting defensively and total defense
        public static void FightingDefensivelyQuery(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            if ((evt.GetConditionArg1() != 0))
            {
                dispIo.return_val = 1;
            }
        }

        [AutoRegister]
        public static readonly ConditionSpec TotalDefenseExtension = ConditionSpec.Extend(StatusEffects.TotalDefense)
            .AddQueryHandler(D20DispatcherKey.QUE_FightingDefensively, FightingDefensivelyQuery)
            .Build();
    }
}