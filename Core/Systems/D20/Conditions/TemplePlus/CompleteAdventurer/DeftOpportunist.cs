using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus;

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
    public static readonly ConditionSpec eDO = ConditionSpec.Create("Deft Opportunist Feat", 2, UniquenessType.Unique)
        .Configure(builder => builder
            .AddHandler(DispatcherType.ToHitBonus2, DOAOO)
        );
}