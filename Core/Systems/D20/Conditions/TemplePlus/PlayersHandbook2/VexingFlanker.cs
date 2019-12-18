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
    public class VexingFlanker
    {
        public static void VFing(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            if (evt.objHndCaller.HasFeat((FeatId) ElfHash.Hash("Vexing Flanker")))
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