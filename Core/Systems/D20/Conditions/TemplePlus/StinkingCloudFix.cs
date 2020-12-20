using OpenTemple.Core.Startup.Discovery;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class StinkingCloudFix
    {
        public static void StinkingCloudEffectAOOPossible(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            // No making AOOs when Nauseated
            dispIo.return_val = 0;
        }

        [AutoRegister]
        public static readonly ConditionSpec StinkingCloudEffectExtension = ConditionSpec
            .Extend(SpellEffects.SpellStinkingCloudHit)
            .AddQueryHandler(D20DispatcherKey.QUE_AOOPossible, StinkingCloudEffectAOOPossible)
            .Build();
    }
}