using OpenTemple.Core.Startup.Discovery;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class FightingDefensively
    {
        // Returns true if the bonus should be active (check box selected and an attack made)
        public static void IsFightingDefensively(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            // The first arg checks the checkbox and the second checks that an attack has been made
            if ((evt.GetConditionArg1() != 0) && (evt.GetConditionArg2() != 0))
            {
                dispIo.return_val = 1;
            }
        }

        // Only checks that the option is selected but does not require an attack to be made (necessary for attack related feats like deadly defense)
        public static void IsFightingDefensivelyChecked(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            if ((evt.GetConditionArg1() != 0))
            {
                dispIo.return_val = 1;
            }
        }

        [AutoRegister]
        public static readonly ConditionSpec FightingDefensivelyExtension = ConditionSpec
            .Extend(FeatConditions.FightingDefensively)
            .AddQueryHandler(D20DispatcherKey.QUE_FightingDefensively, IsFightingDefensively)
            .AddQueryHandler("Fighting Defensively Checked", IsFightingDefensivelyChecked)
            .Build();
    }
}