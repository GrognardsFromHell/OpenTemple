using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class VexingFlanker
    {
        public static readonly FeatId Id = (FeatId) ElfHash.Hash("Vexing Flanker");

        public static void VFing(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            if (evt.objHndCaller.HasFeat(Id))
            {
                // Vexing Flanker
                if ((dispIo.attackPacket.flags & D20CAF.FLANKED) != D20CAF.NONE)
                {
                    dispIo.bonlist.AddBonus(2, 0, "Target Vexing flanker bonus");
                }
            }
        }

        [FeatCondition("Vexing Flanker")]
        [AutoRegister] public static readonly ConditionSpec eVF = ConditionSpec.Create("Vexing Flanker Feat", 2)
            .SetUnique()
            .AddHandler(DispatcherType.ToHitBonus2, VFing)
            .Build();
    }
}