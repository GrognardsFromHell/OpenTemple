using OpenTemple.Core.Startup.Discovery;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class PowerAttack
    {
        public static void PowerAttackValue(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.resultData = (ulong) evt.GetConditionArg1();
        }

        [AutoRegister]
        public static readonly ConditionSpec PowerAttackExtension = ConditionSpec
            .Extend(FeatConditions.PowerAttack)
            .AddQueryHandler("Power Attack Value", PowerAttackValue)
            .Build();
    }
}