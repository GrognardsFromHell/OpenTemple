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
    public class DeftOpportunist
    {
        public const string Name = "Deft Opportunist";

        public static readonly FeatId Id = (FeatId) ElfHash.Hash(Name);

        public static void DOAOO(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            if (evt.objHndCaller.HasFeat(Id))
            {
                // Check if it's an AOO, if so add 4 to the Attack Roll
                if ((dispIo.attackPacket.flags & D20CAF.ATTACK_OF_OPPORTUNITY) != D20CAF.NONE)
                {
                    dispIo.bonlist.AddBonus(4, 0, "Target Deft Opportunist bonus");
                }
            }
        }

        [FeatCondition(Name)]
        [AutoRegister]
        public static readonly ConditionSpec eDO = ConditionSpec.Create("Deft Opportunist Feat", 2)
            .SetUnique()
            .AddHandler(DispatcherType.ToHitBonus2, DOAOO)
            .Build();
    }
}