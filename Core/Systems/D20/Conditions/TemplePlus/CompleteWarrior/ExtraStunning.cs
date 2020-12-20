using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class ExtraStunning
    {
        public static readonly FeatId ExtraStunningId = (FeatId) ElfHash.Hash("Extra Stunning");

        public static void ExtraStunningNewDay(in DispatcherCallbackArgs evt)
        {
            // Add 3 extra stunning attacks per feat taken
            var extraSmitingCount = GameSystems.Feat.HasFeatCount(evt.objHndCaller, ExtraStunningId);
            evt.SetConditionArg1(evt.GetConditionArg1() + 3 * extraSmitingCount);
        }

        [AutoRegister]
        public static readonly ConditionSpec ExtraStunningExtension = ConditionSpec
            .Extend(FeatConditions.featstunningfist)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, ExtraStunningNewDay)
            .Build();
    }
}