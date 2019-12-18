
using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Startup.Discovery;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
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
