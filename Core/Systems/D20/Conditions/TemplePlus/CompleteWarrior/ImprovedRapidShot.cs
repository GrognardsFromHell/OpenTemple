using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.Script.Extensions;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{

    public class ImprovedRapidShot
    {
        public static void impRapidShotHitBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var RapidShotEnabled = false;
            var RapidShotRangerEnabled = false;
            if (evt.objHndCaller.HasFeat(FeatId.RAPID_SHOT))
            {
                RapidShotEnabled = evt.objHndCaller.D20Query("Rapid Shot Enabled");
            }

            if (evt.objHndCaller.HasFeat(FeatId.RANGER_RAPID_SHOT))
            {
                RapidShotRangerEnabled = evt.objHndCaller.D20Query("Rapid Shot Ranger Enabled");
            }

            // First, rapid shot mode must be enabled either regular or ranger
            if (RapidShotEnabled || RapidShotRangerEnabled)
            {
                // Must be a ranged full attack to qualify for the bonus
                if (((dispIo.attackPacket.flags & D20CAF.RANGED)) != D20CAF.NONE && ((dispIo.attackPacket.flags & D20CAF.FULL_ATTACK)) != D20CAF.NONE)
                {
                    dispIo.bonlist.AddBonus(2, 0, "Improved Rapid Shot Feat"); // +2 Bonus makes up for the -2 Rapid shot penalty
                }

            }
        }

        // args are just-in-case placeholders

        [FeatCondition("Improved Rapid Shot")]
        [AutoRegister] public static readonly ConditionSpec impRapidShot = ConditionSpec.Create("Improved Rapid Shot Feat", 2)
            .SetUnique()
            .AddHandler(DispatcherType.ToHitBonus2, impRapidShotHitBonus)
            .Build();
    }
}
