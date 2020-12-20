using OpenTemple.Core.Startup.Discovery;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class Opportunist
    {
        public static void OpportunistReset(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            evt.SetConditionArg1(1);
        }

        [AutoRegister]
        public static readonly ConditionSpec OpportunistExtension = ConditionSpec.Extend(FeatConditions.Opportunist)
            .AddHandler(DispatcherType.BeginRound, OpportunistReset)
            .Build();
    }
}