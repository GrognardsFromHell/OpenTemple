using OpenTemple.Core.Startup.Discovery;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class CombatExpertise
    {

        public static void CombatExpertiseValue(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = evt.GetConditionArg1();
        }

        [AutoRegister] public static readonly ConditionSpec CombatExpertiseExtension = ConditionSpec
            .Extend(FeatConditions.FeatExpertise)
            .AddQueryHandler("Combat Expertise Value", CombatExpertiseValue)
            .Build();

    }
}