using OpenTemple.Core.Startup.Discovery;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class RapidShotQueryExtension
    {
        public static void RapidShotEnabled(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = evt.GetConditionArg1();
        }

        [AutoRegister] public static readonly ConditionSpec RapidShotExtension = ConditionSpec
            .Extend(FeatConditions.RapidShot)
            .AddQueryHandler("Rapid Shot Enabled", RapidShotEnabled)
            .Build();

        [AutoRegister]
        public static readonly ConditionSpec RapidShotRangerExtension = ConditionSpec
            .Extend(FeatConditions.RapidShotRanger)
            .AddQueryHandler("Rapid Shot Ranger Enabled", RapidShotEnabled)
            .Build();
    }
}