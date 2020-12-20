using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class ExtraSmiting
    {
        public static readonly FeatId ExtraSmitingId = (FeatId) ElfHash.Hash("Extra Smiting");

        public static void ExtraSmitingNewDay(in DispatcherCallbackArgs evt)
        {
            var extraSmitingCount = GameSystems.Feat.HasFeatCount(evt.objHndCaller, ExtraSmitingId);

            // Extra Smiting grants 2 additional uses of smite each time the feat is taken
            evt.SetConditionArg1(evt.GetConditionArg1() + 2 * extraSmitingCount);
        }

        [AutoRegister]
        public static readonly ConditionSpec DestructionDomainExtension = ConditionSpec
            .Extend(DomainConditions.DestructionDomain)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, ExtraSmitingNewDay)
            .Build();

        [AutoRegister]
        public static readonly ConditionSpec SmiteEvilExtension = ConditionSpec.Extend(FeatConditions.SmiteEvil)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, ExtraSmitingNewDay)
            .Build();
    }
}