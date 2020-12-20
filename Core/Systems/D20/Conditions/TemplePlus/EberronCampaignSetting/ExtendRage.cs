using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class ExtendRage
    {
        public static void AddCondition(in DispatcherCallbackArgs evt)
        {
            // Add 5 rounds for the extend rage feat
            if (evt.objHndCaller.HasFeat((FeatId) ElfHash.Hash("Extend Rage")))
            {
                evt.SetConditionArg1(evt.GetConditionArg1() + 5);
            }
        }

        [AutoRegister] public static readonly ConditionSpec ExtendRageExtension = ConditionSpec.Extend(StatusEffects.BarbarianRaged)
            .AddHandler(DispatcherType.ConditionAdd, AddCondition)
            .Build();
    }
}