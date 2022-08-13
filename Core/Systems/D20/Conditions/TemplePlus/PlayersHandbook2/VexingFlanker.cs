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
    [AutoRegister]
    public static readonly ConditionSpec eVF = ConditionSpec.Create("Vexing Flanker Feat", 2, UniquenessType.Unique)
        .Configure(builder => builder
            .AddHandler(DispatcherType.ToHitBonus2, VFing)
        );
}