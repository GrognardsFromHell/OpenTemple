using OpenTemple.Core.Startup.Discovery;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class Buckler
    {
        public static void BucklerACBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = evt.GetConditionArg2();
        }

        [AutoRegister]
        public static readonly ConditionSpec BucklerExtension = ConditionSpec.Extend(ItemEffects.Buckler)
            .AddQueryHandler("Buckler Bonus Disabled", BucklerACBonus)
            .Build();
    }
}